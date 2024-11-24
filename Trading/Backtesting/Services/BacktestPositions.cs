namespace Trading.Backtesting;

public class BacktestPositions : IExchangePositions
{
    public IEventSystem EventSystem { get; }
    public decimal FeeRate => 0.001m;
    private ConcurrentBag<Position> Positions { get; set; } = [];
    public BacktestPositions(IEventSystem eventSystem)
    {
        EventSystem = eventSystem;
        EventSystem.Subscribe<OnClosePositionEventArgs>(EventType.OnClosePosition, async evt => await LiquidatePositionAsync(evt.Candle, evt.Position, evt.ExecutionPrice, evt.ExecutionFee));
        EventSystem.Subscribe<OnUpdatePositionEventArgs>(EventType.OnUpdatePosition, async evt => await UpdatePositionAsync(evt.Candle, evt.CurrentPosition, evt.Configuration));
        EventSystem.Subscribe<OnOrderExecutedEventArgs>(EventType.OnOrderExecuted, async evt => await HandleExecutedOrderAsync(evt.Candle, evt.Order));
        EventSystem.Subscribe<OnNewCandleEventArgs>(EventType.OnNewCandle, async evt => await HandleNewCandleAsync(evt.Candle, evt.Symbol));
        EventSystem.Subscribe<OnMarginCallEventArgs>(EventType.OnMarginCall, async evt => await HandleMarginCallAsync(evt.Candle, evt.MarketPrice));
    }

    #region Get

    public Task<Position?> GetPositionAsync(string symbol) 
        => Task.FromResult(Positions.SingleOrDefault(p => p.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase)) ?? null);

    public Task<Position?> GetOpenPositionAsync(string symbol)
        => Task.FromResult(Positions.SingleOrDefault(p => p.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase) && p.IsOpen) ?? null);

    public Task<IEnumerable<Position>> GetPositionsAsync()
        => Task.FromResult(Positions.AsEnumerable());
    public Task<IEnumerable<Position>> GetOpenPositionsAsync()
        => Task.FromResult(Positions.Where(p => p.IsOpen));

    public Task<IEnumerable<Position>> GetClosedPositionsAsync() 
        => Task.FromResult(Positions.Where(p => !p.IsOpen));


    #endregion Get

    #region Update / Close

    /// <summary>
    /// Es dürfen nur verändert werden: StopLoss, TakeProfit
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public async Task UpdatePositionAsync(Candle candle, string symbol, Action<Position> configure)
    {
        var position = await GetOpenPositionAsync(symbol);
        if (position != null) await UpdatePositionAsync(candle, position, configure);
    }

    private Task UpdatePositionAsync(Candle candle, Position position, Action<Position> configure)
    {
        var oldPosition = position.Copy();
        configure(position);
        EventSystem.Publish<OnPositionUpdatedEventArgs>(EventType.OnPositionUpdated, new OnPositionUpdatedEventArgs(candle, position, oldPosition));
        return Task.CompletedTask;
    }

    /// <summary>
    /// increase / reduce / close with an order
    /// </summary>
    /// <param name="position"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public Task UpdatePositionAsync(Candle candle, Position position, Order order)
    {
        var oldPosition = position.Copy();
        position.AddExecutedOrder(order);

        if (position.IsClosed)
        {
            EventSystem.Publish<OnPositionClosedEventArgs>(EventType.OnPositionClosed, new OnPositionClosedEventArgs(candle, position));
        }
        else
        {
            EventSystem.Publish<OnPositionUpdatedEventArgs>(EventType.OnPositionUpdated, new OnPositionUpdatedEventArgs(candle, position, oldPosition));
        }
        
        return Task.CompletedTask;
    }

    #endregion Update / Close

    #region Open

    private Task OpenPositionAsync(Candle candle, Order order)
    {
        var position = new Position(order);
        Positions.Add(position);
        EventSystem.Publish<OnPositionOpenedEventArgs>(EventType.OnPositionOpened, new OnPositionOpenedEventArgs(candle, position));
        return Task.CompletedTask;
    }

    #endregion Open

    #region Liquidation

    public async Task LiquidatePositionAsync(Candle candle, string symbol, decimal executionPrice, decimal executionFee)
    {
        var position = await GetPositionAsync(symbol);
        if (position == null) return;
        await LiquidatePositionAsync(candle, position, executionPrice, executionFee);
    }

    public async Task LiquidatePositionAsync(Candle candle, Position position, decimal executionPrice, decimal executionFee)
    {
        if (position.IsClosed) return;

        var orderSide = position.Side.ToOrderSide();

        var order = new Order(position.Symbol, orderSide)
        {
            Quantity = position.Quantity,
            Status = OrderStatus.Filled,
            Type = OrderType.Market,
            Price = executionPrice,
            ExecutedFee = executionFee,
            ExecutedPrice = executionPrice,
            ExecutedTime = candle.Timestamp,
        };

        await UpdatePositionAsync(candle, position, order);
    }

    #endregion Liquidation

    #region Handler

    private async Task HandleExecutedOrderAsync(Candle candle, Order order)
    {
        var position = await GetOpenPositionAsync(order.Symbol);

        if (position == null)
        {
            await OpenPositionAsync(candle, order);
        }
        else
        {
            await UpdatePositionAsync(candle, position, order);
        }
    }

    /// <summary>
    /// Prüfen, ob eine Position aus diversen Gründen geschlossen werden muss
    /// </summary>
    /// <param name="candle"></param>
    /// <returns></returns>
    private async Task HandleNewCandleAsync(Candle candle, string symbol)
    {
        var position = await GetOpenPositionAsync(symbol);
        if (position == null) return;

        var timestamp = candle.Timestamp;

        // Stop Loss erreicht?
        if (position.StopPriceHit(candle))
        {
            var executionPrice = position.StopPrice!.Value;
            var fee = executionPrice * FeeRate;
            await LiquidatePositionAsync(candle, position, executionPrice, fee);
        }
        // Take Profit erreicht? 
        else if (position.TakePriceHit(candle))
        {
            var executionPrice = position.TakePrice!.Value;
            var fee = executionPrice * FeeRate;
            await LiquidatePositionAsync(candle, position, executionPrice, fee);
        }
        // Njentes
        else
        {

        }
    }

    private async Task HandleMarginCallAsync(Candle candle, decimal marketPrice)
    {
        var expensivePosition = Positions.OrderBy(p => p.GetValue(marketPrice)).Last();
        var timestamp = candle.Timestamp;
        await LiquidatePositionAsync(candle, expensivePosition, marketPrice, marketPrice * FeeRate);
    }

    #endregion Handler

}
