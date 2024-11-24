namespace Trading.Backtesting;

public class BacktestPositionManagement 
{
    private ConcurrentDictionary<DateTime, Position> PositionHistory { get; set; } = []; 
    private Dictionary<DateTime, Position> OrderedPositions => new(PositionHistory.OrderBy(kvp => kvp.Key));
    private IEnumerable<Position> Positions => OrderedPositions.Values;


    #region Get

    public Task<Position?> GetPositionAsync(string symbol) 
        => Task.FromResult(Positions.LastOrDefault(p => p.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase)) ?? null);
    public Task<Position?> GetOpenPositionAsync(string symbol)
        => Task.FromResult(Positions.LastOrDefault(p => p.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase) && p.IsOpen) ?? null);
    public Task<IEnumerable<Position>> GetPositionsAsync()
        => Task.FromResult(Positions);
    public Task<IEnumerable<Position>> GetOpenPositionsAsync()
        => Task.FromResult(Positions.Where(p => p.IsOpen));
    public Task<IEnumerable<Position>> GetClosedPositionsAsync() 
        => Task.FromResult(Positions.Where(p => !p.IsOpen));
    public async Task<bool> HasCurrentOpenPositionsAsync(string symbol)
        => await GetPositionAsync(symbol) != null;

    #endregion Get

    #region Open

    private async Task OpenPositionAsync(Candle candle, Order order)
    {
        if (await HasCurrentOpenPositionsAsync(order.Symbol))
        {
            throw new InvalidOperationException($"opening position not allowed, open position already exists");
        }

        var position = Position.CreateFromOrder(order);
        PositionHistory.TryAdd(candle.Timestamp, position);
    }

    #endregion Open

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
        PositionHistory.TryAdd(candle.Timestamp, position);
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
        return Task.CompletedTask;
    }

    #endregion Update / Close

    #region Liquidation

    public async Task<bool> TryLiquidatePositionAsync(Candle candle, string symbol, double executionPrice, double executionFee)
    {
        var position = await GetOpenPositionAsync(symbol);
        if (position == null) return false;
        await LiquidatePositionAsync(candle, position, executionPrice, executionFee);
        return true;
    }

    public async Task LiquidatePositionAsync(Candle candle, Position position, double executionPrice, double executionFee)
    {
        if (position.IsClosed) return;

        var orderSide = position.Side.ToOrderSide();

        var order = Order.Create(position.Symbol, orderSide);

        order.Quantity = position.Quantity;
        order.Status = OrderStatus.Filled;
        order.Price = executionPrice;
        order.ExecutedFee = executionFee;
        order.ExecutedTime = candle.Timestamp;

        order.CalculateType(executionPrice);

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
    //private async Task HandleNewCandleAsync(Candle candle, string symbol)
    //{
    //    var position = await GetOpenPositionAsync(symbol);
    //    if (position == null) return;

    //    var timestamp = candle.Timestamp;

    //    // Stop Loss erreicht?
    //    if (position.StopPriceHit(candle))
    //    {
    //        var executionPrice = position.StopPrice!.Value;
    //        var fee = executionPrice * FeeRate;
    //        await LiquidatePositionAsync(candle, position, executionPrice, fee);
    //    }
    //    // Take Profit erreicht? 
    //    else if (position.TakePriceHit(candle))
    //    {
    //        var executionPrice = position.TakePrice!.Value;
    //        var fee = executionPrice * FeeRate;
    //        await LiquidatePositionAsync(candle, position, executionPrice, fee);
    //    }
    //    // Njentes
    //    else
    //    {

    //    }
    //}

    //private async Task HandleMarginCallAsync(Candle candle, double marketPrice)
    //{
    //    var expensivePosition = Positions.OrderBy(p => p.GetValue(marketPrice)).Last();
    //    var timestamp = candle.Timestamp;
    //    await LiquidatePositionAsync(candle, expensivePosition, marketPrice, marketPrice * FeeRate);
    //}

    #endregion Handler

}
