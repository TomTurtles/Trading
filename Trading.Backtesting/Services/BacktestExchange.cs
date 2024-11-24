namespace Trading.Backtesting;

public class BacktestExchange : IExchange
{
    // services
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

    #region Initialize

    internal void Initialize(IEnumerable<Candle> candles, double initialCash)
    {
        Candles = candles;
        CashManagement.InitializeCash(initialCash);
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
    internal void SetCurrentCandle(Candle candle) { CurrentCandle = candle; }

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
        return Task.FromResult(0.00001d);
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
        var positions = await GetPositionsAsync();
        foreach (var position in positions)
        {
            var marketPrice = await GetMarketPriceAsync(position.Symbol);
            equity += (marketPrice * position.Quantity);
        }
        return equity;
    }

    #endregion Equity

    #region Orders

    // Abrufen der Orders für ein bestimmtes Symbol
    public Task<IEnumerable<Order>> GetOrdersAsync(string symbol) => OrderManagement.GetOrdersAsync(symbol);
    public Task<IEnumerable<Order>> GetAllOrdersAsync() => OrderManagement.GetAllOrdersAsync();
    public Task PlaceOrderAsync(Order order) => OrderManagement.PlaceOrderAsync(CurrentCandle, order);
    public async Task PlaceMarketOrderAsync(Order order, double executionPrice) => await OrderManagement.PlaceMarketOrderAsync(CurrentCandle, order, executionPrice, await GetFeeRateAsync(order.Symbol));
    public Task UpdateOrderAsync(string id, Action<Order?> configureOrder) => OrderManagement.UpdateOrderAsync(CurrentCandle, id, configureOrder);
    public Task CancelOrderAsync(string id) => OrderManagement.CancelOrderAsync(CurrentCandle, id);
    public Task CancelAllOrdersAsync() => OrderManagement.CancelAllOrdersAsync(CurrentCandle);
    public Task CancelOrdersAsync(IEnumerable<Order> orders) => OrderManagement.CancelOrdersAsync(CurrentCandle, orders);
    public Task CancelOrdersAsync(string symbol) => OrderManagement.CancelOrdersAsync(CurrentCandle, symbol);

    #endregion Orders

    #region Positions

    // Abrufen der aktuellen Position
    public Task<Position?> GetPositionAsync(string symbol) => PositionManagement.GetPositionAsync(symbol);
    public Task<IEnumerable<Position>> GetPositionsAsync() => PositionManagement.GetPositionsAsync();
    public Task UpdatePositionAsync(string id, Action<Position> configure) => PositionManagement.UpdatePositionAsync(CurrentCandle, id, configure);

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
    internal async void HandleNewCandle(Candle candle, string symbol)
    {
        var marketPrice = await GetMarketPriceAsync(symbol);
        var feeRate = await GetFeeRateAsync(symbol);

        // margin call?
        if (CashManagement.ShouldMarginCall())
        {
            while (await PositionManagement.HasCurrentOpenPositionsAsync(symbol))
            {
                await PositionManagement.TryLiquidatePositionAsync(candle, symbol, marketPrice, feeRate);
            }
        }
        else
        {
            // stoploss/takeprofit?
            if (await PositionManagement.HasCurrentOpenPositionsAsync(symbol))
            {
                var position = await PositionManagement.GetOpenPositionAsync(symbol);

                // takeprofig
                if (position!.TakePrice is not null && candle.PriceHit(position!.TakePrice!.Value))
                {
                    await PositionManagement.LiquidatePositionAsync(candle, position, position!.TakePrice!.Value, feeRate);
                    CashManagement.AddCash(candle, position!.Quantity * position!.TakePrice!.Value - feeRate);
                }

                // stoploss
                if (position!.StopPrice is not null && candle.PriceHit(position!.StopPrice!.Value))
                {
                    await PositionManagement.LiquidatePositionAsync(candle, position, position!.StopPrice!.Value, feeRate);
                    CashManagement.AddCash(candle, position!.Quantity * position!.StopPrice!.Value - feeRate);
                }
            }

            // orders executed?
            if (await OrderManagement.HasOpenOrdersAsync(symbol))
            {
                var openOrders = await OrderManagement.GetOpenOrdersAsync();

                var ordersToExecute = openOrders.Where(order => order.CandleHit(candle));

                foreach (var order in ordersToExecute)
                {
                    var executionPrice = order.Price ?? candle.Close;
                    var executionTime = candle.Timestamp;
                    order.SetExecuted(executionTime, executionPrice, feeRate);
                }
            }
        }
    }

    /// <summary> 
    /// </summary>
    /// <param name="candle"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal async Task<ExchangeState> HandleNewDecisionAsync(Candle candle, StrategyDecision decision, string symbol)
    {
        switch (decision.Type)
        {
            case Trading.StrategyDecisionType.GoLong:
                await HandleGoLongDecisionAsync(candle, decision, symbol);
                break;
            case Trading.StrategyDecisionType.GoShort:
                await HandleGoShortDecisionAsync(candle, decision, symbol);
                break;
            case Trading.StrategyDecisionType.CancelOrders:
                await HandleCancelOrdersDecisionAsync(candle, decision, symbol);
                break;
            case Trading.StrategyDecisionType.UpdatePosition:
                await HandleUpdatePositionDecisionAsync(candle, decision, symbol);
                break;
            case Trading.StrategyDecisionType.ClosePosition:
                await HandleClosePositionDecisionAsync(candle, decision, symbol);
                break;

            case Trading.StrategyDecisionType.Error:

                break;

            // no action when these
            case Trading.StrategyDecisionType.Wait:
            default:
                break;
        }

        return new ExchangeState()
        {
            Cash = await GetMarginAsync(),
            Orders = await GetAllOrdersAsync(),
            Positions = await GetPositionsAsync(),
        };
    }

    private async Task HandleClosePositionDecisionAsync(Candle candle, StrategyDecision decision, string symbol)
    {
        var position = decision.Get<Position>("position");
        var marketPrice = await GetMarketPriceAsync(symbol);
        var feeRate = await GetFeeRateAsync(symbol);
        await PositionManagement.LiquidatePositionAsync(candle, position, marketPrice, feeRate);
    }

    private async Task HandleUpdatePositionDecisionAsync(Candle candle, StrategyDecision decision, string symbol)
    {
        var configureNewPosition = decision.Get<Action<Position>>("configureNewPosition");
        await PositionManagement.UpdatePositionAsync(candle, symbol, configureNewPosition);
    }

    private async Task HandleCancelOrdersDecisionAsync(Candle candle, StrategyDecision decision, string symbol)
    {
        var orders = decision.Get<IEnumerable<Order>>("orders");

        foreach (var order in orders) 
        {
            await OrderManagement.CancelOrderAsync(candle, order.ID);
        }
    }

    private async Task HandleGoShortDecisionAsync(Candle candle, StrategyDecision decision, string symbol)
    {
        var order = decision.Get<Order>("order");
        var marketPrice = await GetMarketPriceAsync(symbol);
        var feeRate = await GetFeeRateAsync(symbol);

        if (order.Type == OrderType.Market)
        {
            await OrderManagement.PlaceMarketOrderAsync(candle, order, marketPrice, feeRate);
        }
        else
        {
            await OrderManagement.PlaceOrderAsync(candle, order);
        }
    }

    private async Task HandleGoLongDecisionAsync(Candle candle, StrategyDecision decision, string symbol)
    {
        var order = decision.Get<Order>("order");
        var marketPrice = await GetMarketPriceAsync(symbol);
        var feeRate = await GetFeeRateAsync(symbol);

        if (order.Type == OrderType.Market)
        {
            await OrderManagement.PlaceMarketOrderAsync(candle, order, marketPrice, feeRate);
        }
        else
        {
            await OrderManagement.PlaceOrderAsync(candle, order);
        }
    }


    #endregion Handlers
}
