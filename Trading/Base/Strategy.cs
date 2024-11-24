namespace Trading;

public abstract class Strategy : IStrategy
{
    protected Strategy(IExchange exchange, string symbol, CandleInterval interval)
    {
        Exchange = exchange;
        Symbol = symbol;
        CandleInterval = interval;
    }

    // Services
    protected IExchange Exchange { get; }

    // Definition
    public abstract string Name { get; }
    public string Symbol { get; }
    public CandleInterval CandleInterval { get; }

    // Portfolio
    protected double Margin { get; private set; }
    protected Position? Position { get; private set; }
    protected IEnumerable<Order> Orders { get; private set; } = [];

    // Market Data
    protected Candle CurrentCandle { get; private set; }
    protected List<Candle> Candles { get; private set; } = new();
    protected double FeeRate { get; private set; }
    protected int Leverage { get; private set; }
    protected double MarketPrice { get; private set; }


    #region Abstract / Virtual

    // Before
    public virtual Task BeforeAsync(Candle candle) => Task.CompletedTask;

    // Orders
    public virtual Task<bool> ShouldCancelOrdersAsync() => Task.FromResult(false);
    public abstract Task<bool> ShouldLongAsync();
    public abstract Task<bool> ShouldShortAsync();
    public abstract Task GoLongAsync(Order order);
    public abstract Task GoShortAsync(Order order);

    // Order Events
    public virtual Task OnOrderPlacedAsync(Order order) => Task.CompletedTask;

    // Positions
    public virtual Task UpdatePositionAsync(Position currentPosition) => Task.CompletedTask;
    public virtual Task<bool> ShouldClosePositionAsync(Position currentPosition) => Task.FromResult(false);
    public virtual Task<bool> ShouldUpdatePositionAsync(Position currentPosition) => Task.FromResult(false);
    public virtual Task<Action<Position>> GoUpdatePositionAsync(Position currentPosition) => Task.FromResult((Position p) => { });

    // Position Events
    public virtual Task OnPositionChangedAsync(Position position) => Task.CompletedTask;
    public virtual Task OnPositionOpenedAsync(Position position) => Task.CompletedTask;
    public virtual Task OnPositionIncreasedAsync(Position position) => Task.CompletedTask;
    public virtual Task OnPositionDecreasedAsync(Position position) => Task.CompletedTask;
    public virtual Task OnPositionClosedAsync(Position position) => Task.CompletedTask;

    // After
    public virtual Task AfterAsync(Candle candle, StrategyDecision decision) => Task.CompletedTask;

    #endregion Abstract / Virtual

    #region Base Methods
    public async Task<StrategyDecision> ExecuteAsync(Candle candle)
    {
        await SynchronizeWithExchange(candle);

        await BeforeAsync(candle);

        var decision = await FindDecisionAsync(candle);

        await AfterAsync(candle, decision);

        return decision;
    }

    private async Task SynchronizeWithExchange(Candle candle)
    {
        await Task.WhenAll(
            Task.Run(() => CurrentCandle = candle),
            Task.Run(async () => { Position = await Exchange.GetPositionAsync(Symbol); }),
            Task.Run(async () => { Orders = await Exchange.GetOrdersAsync(Symbol); }),
            Task.Run(async () => { Candles = await Exchange.GetCandlesAsync(Symbol, CandleInterval); }),
            Task.Run(async () => { Margin = await Exchange.GetMarginAsync(); }),
            Task.Run(async () => { FeeRate = await Exchange.GetFeeRateAsync(Symbol); }),
            Task.Run(async () => { Leverage = await Exchange.GetLeverageAsync(Symbol); }),
            Task.Run(async () => { MarketPrice = await Exchange.GetMarketPriceAsync(Symbol); })
        );
    }

    private async Task<StrategyDecision> FindDecisionAsync(Candle candle)
    {
        try
        {
            // has open position?
            if (Position is not null && Position.IsOpen)
            {              
                // decide whether to stay with current strategy or not (stop-loss, take-profit)
                // decide whether to alter the current position in profit/loss or size
                if (await ShouldClosePositionAsync(Position))
                {
                    return StrategyDecision.ClosePosition(Position);
                }
                else if (await ShouldUpdatePositionAsync(Position))
                {
                    var configureNewPosition = await GoUpdatePositionAsync(Position);
                    return StrategyDecision.UpdatePosition(Position, configureNewPosition);
                }
                else
                {
                    return StrategyDecision.Wait("no need to update position", (nameof(Position), Position));
                }
            }
            // has active orders?
            else if (Orders.Any())
            {
                // should cancel that orders?
                // are they still fitting to strategy?
                if (await ShouldCancelOrdersAsync())
                {
                    return StrategyDecision.CancelOrders(Orders);
                }
                else
                {
                    return StrategyDecision.Wait("no need to cancel orders", (nameof(Orders), Orders));
                }
            }
            // should open new position?
            else
            {
                if (await ShouldLongAsync())
                {
                    var order = Order.CreateLong(Symbol);
                    await GoLongAsync(order);
                    order.CalculateType(MarketPrice);
                    return StrategyDecision.GoLong(order);
                }
                else if (await ShouldShortAsync())
                {
                    var order = Order.CreateShort(Symbol);
                    await GoShortAsync(order);
                    order.CalculateType(MarketPrice);
                    return StrategyDecision.GoShort(order);
                }
                else
                {
                    return StrategyDecision.Wait("no need to create orders");
                }
            }
        }
        catch (Exception ex)
        {
            return StrategyDecision.Error(ex);
        }
    }

    #endregion Base Methods
}

