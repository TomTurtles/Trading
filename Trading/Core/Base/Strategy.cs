namespace Trading;

public abstract class Strategy : IStrategy
{
    protected Strategy(IExchange exchange, IEventSystem eventSystem, string symbol, CandleInterval interval)
    {
        Exchange = exchange;
        EventSystem = eventSystem;
        Symbol = symbol;
        CandleInterval = interval;
        EventSystem.Subscribe<OnCandleCreatedEventArgs>(EventType.OnNewCandle, async (@event) => await ExecuteAsync(@event.Candle));
    }

    // Services
    protected IExchange Exchange { get; }
    protected IEventSystem EventSystem { get; }

    // Definition
    public abstract string Name { get; }
    public string Symbol { get; }
    public CandleInterval CandleInterval { get; }

    // Portfolio
    protected decimal Margin { get; private set; }
    protected Position? Position { get; private set; }
    protected IEnumerable<Order> Orders { get; private set; } = [];

    // Market Data
    protected Candle CurrentCandle { get; private set; }
    protected List<Candle> Candles { get; private set; } = new();
    protected decimal Fee { get; private set; }
    protected int Leverage { get; private set; }
    protected decimal MarketPrice { get; private set; }


    #region Abstract / Virtual
    public virtual Task BeforeAsync(Candle candle) => Task.CompletedTask;

    public virtual Task UpdatePositionAsync(Position currentPosition) => Task.CompletedTask;

    public virtual Task<bool> ShouldCancelOrdersAsync() => Task.FromResult(false);
    public virtual Task<bool> ShouldClosePositionAsync(Position currentPosition) => Task.FromResult(false);
    public virtual Task<bool> ShouldUpdatePositionAsync(Position currentPosition) => Task.FromResult(false);
    public virtual Task<Action<Position>> GoUpdatePositionAsync(Position currentPosition) => Task.FromResult((Position p) => { });

    public abstract Task<bool> ShouldLongAsync();

    public abstract Task<bool> ShouldShortAsync();

    public abstract Task GoLongAsync(Order order);

    public abstract Task GoShortAsync(Order order);

    public virtual Task AfterAsync(Candle candle) => Task.CompletedTask;

    public virtual Task OnOrderPlacedAsync(Order order) => Task.CompletedTask;

    public virtual Task OnPositionChangedAsync(Position position) => Task.CompletedTask;
    public virtual Task OnPositionOpenedAsync(Position position) => Task.CompletedTask;
    public virtual Task OnPositionIncreasedAsync(Position position) => Task.CompletedTask;
    public virtual Task OnPositionDecreasedAsync(Position position) => Task.CompletedTask;
    public virtual Task OnPositionClosedAsync(Position position) => Task.CompletedTask;

    #endregion Abstract / Virtual
    public async Task ExecuteAsync(Candle candle)
    {
        await InitializeAsync(candle);

        await BeforeAsync(candle);

        await FindDecisionAsync(candle);

        await AfterAsync(candle);

        EventSystem.Publish(EventType.OnStrategyExecuted, new OnStrategyExecutedEventArgs(candle));
    }

    private async Task InitializeAsync(Candle candle)
    {
        await Task.WhenAll(
            Task.Run(() => CurrentCandle = candle),
            Task.Run(async () => { Position = await Exchange.GetPositionAsync(Symbol); }),
            Task.Run(async () => { Orders = await Exchange.GetOrdersAsync(Symbol); }),
            Task.Run(async () => { Candles = await Exchange.GetCandlesAsync(Symbol, CandleInterval); }),
            Task.Run(async () => { Margin = await Exchange.GetMarginAsync(); }),
            Task.Run(async () => { Fee = await Exchange.GetFeeRateAsync(Symbol); }),
            Task.Run(async () => { Leverage = await Exchange.GetLeverageAsync(Symbol); }),
            Task.Run(async () => { MarketPrice = await Exchange.GetMarketPriceAsync(Symbol); })
        );
    }

    private async Task FindDecisionAsync(Candle candle)
    {
        try
        {
            var positionJustClosed = false;
            var ordersJustCancelled = false;

            // has open position?
            if (Position is not null && Position.IsOpen)
            {              
                // decide whether to stay with current strategy or not (stop-loss, take-profit)
                // decide whether to alter the current position in profit/loss or size
                if (await ShouldClosePositionAsync(Position))
                {
                    EventSystem.Publish(EventType.OnClosePosition, new OnClosePositionEventArgs(candle, Position, MarketPrice, Fee * MarketPrice));
                    positionJustClosed = true;
                }
                else if (await ShouldUpdatePositionAsync(Position))
                {
                    var updatedPosition = await GoUpdatePositionAsync(Position);
                    EventSystem.Publish(EventType.OnUpdatePosition, new OnUpdatePositionEventArgs(candle, Position, updatedPosition));
                }
                else
                {
                    // Keine Veränderung der bestehenden Position
                    EventSystem.Publish(EventType.OnDoNothing, new OnDoNothingEventArgs(candle, "positions are ok"));
                }
            }
            // has active orders?
            else if (Orders.Any())
            {
                // should cancel that orders?
                // are they still fitting to strategy?
                if (await ShouldCancelOrdersAsync())
                {
                    EventSystem.Publish(EventType.OnCancelOrders, new OnCancelOrdersEventArgs(candle, Orders));
                    ordersJustCancelled = true;
                }
                else
                {
                    // Keine Veränderung der bestehenden Order(s)
                    EventSystem.Publish(EventType.OnDoNothing, new OnDoNothingEventArgs(candle, "orders are ok"));
                }
            }

            // should open new position?
            if ((Position is null || !Position.IsOpen) && !Orders.Any() || positionJustClosed || ordersJustCancelled)
            {
                if (await ShouldLongAsync())
                {
                    var order = new Order(Symbol, OrderSide.Buy);
                    await GoLongAsync(order);
                    EventSystem.Publish(EventType.OnGoLong, new OnGoLongEventArgs(candle, order, MarketPrice));
                }
                else if (await ShouldShortAsync())
                {
                    var order = new Order(Symbol, OrderSide.Sell);
                    await GoShortAsync(order);
                    EventSystem.Publish(EventType.OnGoShort, new OnGoShortEventArgs(candle, order, MarketPrice));
                }
                else
                {
                    // Keine neue Order setzen
                    EventSystem.Publish(EventType.OnDoNothing, new OnDoNothingEventArgs(candle, "no new order"));
                }
            }
        }
        catch (Exception ex)
        {
            EventSystem.Publish(EventType.OnStrategyException, new OnStrategyExceptionEventArgs(candle, ex));
        }
    }
}
