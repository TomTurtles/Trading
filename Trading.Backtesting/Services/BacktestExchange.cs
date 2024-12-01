namespace Trading.Backtesting;

public class BacktestExchange : IExchange
{
    // services
    public ILogger? Logger { get; set; }
    private BacktestCashManagement CashManagement { get; } = new BacktestCashManagement();
    private BacktestOrderManagement OrderManagement { get; } = new BacktestOrderManagement();
    private BacktestPositionManagement PositionManagement { get; } = new BacktestPositionManagement();

    // identify
    public string Name => "Backtest";

    // candle
    private Candle CurrentCandle { get; set; } = new();
    private IEnumerable<Candle> Candles { get; set; } = Enumerable.Empty<Candle>();

    // state
    private bool IsConnected { get; set; } = false;
    private string? Symbol { get; set; }

    #region Initialize

    internal void Initialize(IEnumerable<Candle> candles, double initialCash, string symbol)
    {
        Candles = candles;
        CashManagement.InitializeCash(candles.ElementAt(0) ,initialCash);
        Symbol = symbol;
    }

    #endregion Initialize

    #region Connection

    public Task ConnectAsync()
    {
        IsConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    #endregion Connection

    #region Candles

    public Task<Candle> GetCurrentCandleAsync(string symbol, CandleInterval interval)
    {
        return Task.FromResult(CurrentCandle);
    }

    public Task<List<Candle>> GetCandlesAsync(string symbol, CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null)
    {
        // Simuliere das Abrufen von Candles
        return Task.FromResult(Candles.Where(c => c.Timestamp < CurrentCandle.Timestamp).ToList());
    }

    #endregion Candles

    #region Market Data

    // Abrufen der Gebühr für ein bestimmtes Symbol
    public Task<double> GetFeeRateAsync(string symbol)
    {
        // Beispiel: Eine feste Gebühr 
        var feeRate = 0.05d;
        return Task.FromResult(feeRate / 100);
    }

    public async Task<double> CalculateFeeAsync(string symbol, double price, double quantity)
    {
        double feeRate = await GetFeeRateAsync(symbol);
        return Math.Abs(price * quantity) * feeRate; // Berechne die Gebühr
    }

    // Abrufen des Hebels für ein bestimmtes Symbol
    public Task<int> GetLeverageAsync(string symbol)
    {
        // Beispiel: Fester Hebel 
        return Task.FromResult(10);
    }

    // Abrufen des aktuellen Marktpreises
    public Task<double> GetMarketPriceAsync(string symbol)
    {
        return Task.FromResult<double>(CurrentCandle.Close);
    }

    public async Task<double> GetMarketPriceWithSlippage(string symbol)
    {
        var marketPrice = await GetMarketPriceAsync(symbol);
        return marketPrice.ApplySlippage();
    }

    #endregion Market Data

    #region Cash

    public Task<double> GetMarginAsync() => CashManagement.GetMarginAsync();
    private bool CanAfford(double cost) => CashManagement.CanAfford(cost);

    #endregion Cash

    #region Equity

    public async Task<double> GetEquityAsync()
    {
        var equity = 0d;
        equity += await GetMarginAsync();

        var position = await GetOpenPositionAsync(Symbol!);
        if (position != null)
        {
            var marketPrice = await GetMarketPriceAsync(Symbol!);
            equity += position.GetValue(marketPrice);
        }

        return equity;
    }

    #endregion Equity

    #region Orders

    // Abrufen der Orders für ein bestimmtes Symbol
    public Task<IEnumerable<Order>> GetOrdersAsync(string symbol) => OrderManagement.GetOpenOrdersAsync(symbol);
    public Task<IEnumerable<Order>> GetAllOrdersAsync() => OrderManagement.GetAllOrdersAsync();
    private async Task<IEnumerable<Order>> GetPendingOrdersAsync() => await OrderManagement.GetOpenOrdersAsync();
    public async Task PlaceOrderAsync(Order order)
    {
        await OrderManagement.PlaceOrderAsync(CurrentCandle, order);
        Logger?.LogDebug("order placed ({order})", order);
    }

    public async Task PlaceMarketOrderAsync(Order order, double executionPrice)
    {
        var feeRate = await GetFeeRateAsync(order.Symbol);

        executionPrice = executionPrice.ApplySlippage();

        var cost = executionPrice * (feeRate + order.Quantity);
        if (!CashManagement.CanAfford(cost))
        {
            Logger?.LogWarning("insufficient cash ({cash}), cannot afford order: {order}", await CashManagement.GetMarginAsync(), order);
            return;
        }

        await OrderManagement.PlaceMarketOrderAsync(CurrentCandle, order, executionPrice, feeRate);
        Logger?.LogDebug("market order placed and executed: {order}", order);

        var position = await PositionManagement.OpenPositionAsync(CurrentCandle, order);
        Logger?.LogDebug("position opened: {position}", position);

        CashManagement.AddCash(CurrentCandle, (-1) * order.ExecutedPrice!.Value * order.Quantity - order.ExecutedFee!.Value);
        Logger?.LogDebug("cash now: {cash}", await CashManagement.GetMarginAsync());
    }

    public Task UpdateOrderAsync(string id, Action<Order?> configureOrder) => OrderManagement.UpdateOrderAsync(CurrentCandle, id, configureOrder);
    public Task CancelOrderAsync(string id) => OrderManagement.CancelOrderAsync(CurrentCandle, id);
    public Task CancelAllOrdersAsync() => OrderManagement.CancelAllOrdersAsync(CurrentCandle);
    public Task CancelOrdersAsync(IEnumerable<Order> orders) => OrderManagement.CancelOrdersAsync(CurrentCandle, orders);
    public Task CancelOrdersAsync(string symbol) => OrderManagement.CancelOrdersAsync(CurrentCandle, symbol);

    #endregion Orders

    #region Positions

    // Abrufen der aktuellen Position
    public async Task<Position?> GetPositionAsync(string symbol) => await PositionManagement.GetPositionAsync();
    private async Task<Position?> GetOpenPositionAsync(string symbol) => await PositionManagement.GetPositionAsync();
    public async Task<IEnumerable<Position>> GetPositionsAsync() => await PositionManagement.GetPositionHistoryAsync();
    public async Task UpdatePositionAsync(string id, Action<Position> configure)
    {
        var position = await PositionManagement.UpdatePositionAsync(CurrentCandle, id, configure);
        Logger?.LogDebug("position updated: {position}", position);
    }

    public async Task<Position?> ClosePositionAtPriceAsync(string symbol, double price)
    {
        var position = await GetPositionAsync(symbol);
        if (position == null) return null;
        return await ClosePositionAtPriceAsync(position, price);
    }

    public async Task<Position?> ClosePositionAtPriceAsync(Position position, double price)
    {
        var feeRate = await GetFeeRateAsync(position.Symbol);
        
        price = price.ApplySlippage();

        var liquidatedPosition = await PositionManagement.LiquidatePositionAsync(CurrentCandle, position, price, feeRate);

        if (liquidatedPosition is not null && liquidatedPosition.IsClosed)
        {
            Logger?.LogDebug("position closed: {position}", liquidatedPosition);

            var qty = position.EntryQuantity - position.ExitQuantity;
            var value = (price * qty + liquidatedPosition.PNL!.Value) * (1 - feeRate);
            CashManagement.AddCash(CurrentCandle, value);
            Logger?.LogDebug("cash now: {cash}", await CashManagement.GetMarginAsync());
        }

        return liquidatedPosition.Copy();
    }

    #endregion Positions

    #region Handlers

    /// <summary> 
    /// Exchange reacts on new candle first
    /// margin call?
    /// stoploss/takeprofit?
    /// orders executed?
    /// </summary>
    /// <param name="candle"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal async Task<IEnumerable<Position>> HandleNewCandleAsync(Candle candle, string symbol)
    {
        CurrentCandle = candle;

        var marketPrice = await GetMarketPriceAsync(symbol);
        var feeRate = await GetFeeRateAsync(symbol);

        List<Position> closedPositions = new();

        if (await PositionManagement.HasCurrentOpenPositionAsync())
        {
            // margin call?
            if (CashManagement.ShouldMarginCall(await PositionManagement.GetPositionAsync(), marketPrice))
            {
                Logger?.LogWarning("MARGIN CALL");
                while (await PositionManagement.HasCurrentOpenPositionAsync())
                {
                    var closedPosition = await ClosePositionAtPriceAsync(symbol, marketPrice);
                    if (closedPosition is not null && closedPosition.IsClosed) closedPositions.Add(closedPosition);
                }
            }
            // stoploss/takeprofit?
            else
            {
                var position = await PositionManagement.GetPositionAsync();

                // takeprofit
                if (position!.TakePrice is not null && CurrentCandle.IsTakeProfitHit(position))
                {
                    var closedPosition = await ClosePositionAtPriceAsync(symbol, position!.TakePrice!.Value);
                    if (closedPosition is not null && closedPosition.IsClosed) closedPositions.Add(closedPosition);
                }

                // stoploss
                if (position!.StopPrice is not null && CurrentCandle.IsStopLossHit(position))
                {
                    var closedPosition = await ClosePositionAtPriceAsync(symbol, position!.StopPrice!.Value);
                    if (closedPosition is not null && closedPosition.IsClosed) closedPositions.Add(closedPosition);
                }
            }
        }

        // orders executed?
        if (await OrderManagement.HasOpenOrdersAsync(symbol))
        {
            var openOrders = await OrderManagement.GetOpenOrdersAsync();

            var ordersToExecute = openOrders.Where(order => order.CandleHit(CurrentCandle));

            foreach (var order in ordersToExecute)
            {
                await PlaceMarketOrderAsync(order, marketPrice);
            }
        }


        return closedPositions;
    }

    /// <summary> 
    /// </summary>
    /// <param name="candle"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal async Task<ExchangeState> HandleNewDecisionAsync(StrategyDecision decision)
    {
        Position? closedPosition = null;

        switch (decision.Type)
        {
            case StrategyDecisionType.GoLong:
                await HandleGoLongDecisionAsync(decision);
                break;
            case StrategyDecisionType.GoShort:
                await HandleGoShortDecisionAsync(decision);
                break;
            case StrategyDecisionType.CancelOrders:
                await HandleCancelOrdersDecisionAsync(decision);
                break;
            case StrategyDecisionType.UpdatePosition:
                await HandleUpdatePositionDecisionAsync(decision);
                break;
            case StrategyDecisionType.ClosePosition:
                closedPosition = await HandleClosePositionDecisionAsync(decision);
                break;

            case StrategyDecisionType.Error:

                break;

            // no action when these
            case StrategyDecisionType.Wait:
            default:
                break;
        }

        return ExchangeState.Create(
                await GetMarginAsync(),
                await GetPendingOrdersAsync(),
                await GetOpenPositionAsync(Symbol!),
                closedPosition,
                await GetMarketPriceAsync(Symbol!),
                await GetEquityAsync()
            );
    }

    private async Task<Position?> HandleClosePositionDecisionAsync(StrategyDecision decision)
    {
        var position = decision.Get<Position>("position");
        var marketPrice = await GetMarketPriceAsync(Symbol!);
        return await ClosePositionAtPriceAsync(position, marketPrice);
    }

    private async Task HandleUpdatePositionDecisionAsync(StrategyDecision decision)
    {
        var configureNewPosition = decision.Get<Action<Position>>("configureNewPosition");
        await PositionManagement.UpdatePositionAsync(CurrentCandle, Symbol!, configureNewPosition);
    }

    private async Task HandleCancelOrdersDecisionAsync(StrategyDecision decision)
    {
        var orders = decision.Get<IEnumerable<Order>>("orders");

        foreach (var order in orders)
        {
            await OrderManagement.CancelOrderAsync(CurrentCandle, order.ID);
        }
    }

    private async Task HandleGoShortDecisionAsync(StrategyDecision decision)
    {
        var order = decision.Get<Order>("order");
        var marketPrice = await GetMarketPriceAsync(Symbol!);

        if (order.Type == OrderType.Market)
        {
            await PlaceMarketOrderAsync(order, marketPrice);
        }
        else
        {
            await PlaceOrderAsync(order);
        }
    }

    private async Task HandleGoLongDecisionAsync(StrategyDecision decision)
    {
        var order = decision.Get<Order>("order");
        var marketPrice = await GetMarketPriceAsync(Symbol!);

        if (order.Type == OrderType.Market)
        {
            await PlaceMarketOrderAsync(order, marketPrice);
        }
        else
        {
            await PlaceOrderAsync(order);
        }
    }


    #endregion Handlers
}
