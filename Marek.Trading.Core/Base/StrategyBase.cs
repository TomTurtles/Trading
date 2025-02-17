namespace Marek.Trading.Core;

public abstract class StrategyBase 
    : IBacktestingStrategy
    , IStrategy
    , IStrategyInitializable
{
    #region Initialize

    protected StrategyBase() { }

    public void Initialize(
        IMareatorEventDispatcher dispatcher,
        IExchange exchange, 
        ILogger<IStrategy> logger, 
        IStrategyOptions options)
    {
        _dispatcher = dispatcher;
        _exchange = exchange;
        _logger = logger;
        _options = options;
    }

    #endregion Initialize

    #region Services
    private IMareatorEventDispatcher? _dispatcher;
    protected IMareatorEventDispatcher Dispatcher => _dispatcher ?? throw new NullReferenceException(nameof(_dispatcher));

    private IExchange? _exchange;
    protected IExchange Exchange => _exchange ?? throw new NullReferenceException(nameof(_exchange));

    private ILogger<IStrategy>? _logger;
    protected ILogger<IStrategy> Logger => _logger ?? throw new NullReferenceException(nameof(_logger));

    #endregion Services

    #region Options
    
    private IStrategyOptions? _options;
    protected string Symbol 
        => _options?.Symbol ?? throw new NullReferenceException(nameof(_options.Symbol));
    protected CandleInterval CandleInterval 
        => _options?.Interval ?? throw new NullReferenceException(nameof(_options.Interval));
    protected double Lever 
        => _options?.Lever ?? throw new NullReferenceException(nameof(_options.Lever));
    
    #endregion Options

    #region Abstract / Virtual

    // Identify
    public abstract string Name { get; }

    // Before
    public virtual Task BeforeAsync(Candle candle) => Task.CompletedTask;

    // Orders
    public abstract Task<bool> ShouldLongAsync(Candle candle);
    public abstract Task<bool> ShouldShortAsync(Candle candle);
    public abstract Task GoLongAsync(Candle candle, Order order);
    public abstract Task GoShortAsync(Candle candle, Order order);
    public virtual Task<bool> ShouldCancelOrdersAsync(Candle candle) => Task.FromResult(false);

    // Positions
    public virtual Task<bool> ShouldClosePositionAsync(Candle candle, Position position) => Task.FromResult(false);
    public virtual Task<bool> ShouldUpdatePositionAsync(Candle candle, Position position) => Task.FromResult(false);
    public virtual Task<Action<Position>> GoUpdatePositionAsync(Candle candle, Position position) => Task.FromResult<Action<Position>>(position => { });

    // After
    public virtual Task AfterAsync(Candle candle) => Task.CompletedTask;

    #endregion Abstract / Virtual

    #region Requests

    protected async Task<double> GetMarginAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetMarginAsync(cancellationToken);
    }
    protected async Task<Position?> GetOpenPositionAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetOpenPositionAsync(cancellationToken);
    }
    protected async Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetPendingOrdersAsync(cancellationToken);
    }
    protected async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetOrdersAsync(cancellationToken);
    }
    protected async Task<Candle> GetCandleAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetCandleAsync(cancellationToken);
    }
    protected async Task<List<Candle>> GetCandlesAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetCandlesAsync(cancellationToken: cancellationToken);
    }
    protected async Task<double> GetFeeRateAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetFeeRateAsync(cancellationToken);
    }
    protected async Task<double> GetLeverageAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetLeverageAsync(cancellationToken);
    }
    protected async Task<double> GetMarketPriceAsync(CancellationToken cancellationToken = default)
    {
        return await Exchange.GetMarketPriceAsync(cancellationToken);
    }

    #endregion Requests

    #region Commands
    private async Task<StrategyDecision> ClosePositionAsync(Candle candle, Position position, CancellationToken cancellationToken = default)
    {
        await Exchange.ClosePositionAsync(position.Id, cancellationToken: cancellationToken);
        return StrategyDecision.ClosePosition(candle, position);
    }
    private async Task<StrategyDecision> WaitAsync(Candle candle, string reason, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => StrategyDecision.Wait(candle, reason), cancellationToken);
    }
    private async Task<StrategyDecision> CancelOrdersAsync(Candle candle, List<Order> orders, CancellationToken cancellationToken = default)
    {
        await Exchange.CancelOrdersAsync(orders, cancellationToken);
        return StrategyDecision.CancelOrders(candle, orders);
    }
    private async Task<StrategyDecision> PlaceOrderAsync(Candle candle, Order order, CancellationToken cancellationToken = default)
    {
        await Exchange.PlaceOrderAsync(order, cancellationToken);
        return order.Side == OrderSide.Buy
            ? StrategyDecision.GoLong(candle, order)
            : StrategyDecision.GoShort(candle, order);
    }

    #endregion Commands

    #region Events

    private void NotifyStrategyDecision(Candle candle, StrategyDecision decision)
    {
        Dispatcher.Publish(this, new OnStrategyDecisionEventArgs(candle, decision));
    }

    #endregion Events

    #region Handle new Candle

    public async Task RunAsync(Candle candle, CancellationToken cancellationToken = default)
    {
        await BeforeAsync(candle);

        var decision = await FindDecisionAsync(candle);

        await AfterAsync(candle);

        NotifyStrategyDecision(candle, decision);
    }

    private async Task<StrategyDecision> FindDecisionAsync(Candle candle)
    {
        try
        {
            // has open position?
            var position = await GetOpenPositionAsync();

            if (position is not null && position.IsOpen)
            {
                // decide whether to stay with current strategy or not (stop-loss, take-profit)
                // decide whether to alter the current position in profit/loss or size
                if (await ShouldClosePositionAsync(candle, position))
                {
                    return await ClosePositionAsync(candle, position);
                }
                else
                {
                    return await WaitAsync(candle, "open position");
                }
            }

            // has pending orders?
            var pendingOrders = await GetPendingOrdersAsync();

            if (pendingOrders.Any())
            {
                // should cancel that orders?
                // are they still fitting to strategy?
                if (await ShouldCancelOrdersAsync(candle))
                {
                    return await CancelOrdersAsync(candle, pendingOrders);
                }
                else
                {
                    return await WaitAsync(candle, "pending orders");
                }
            }

            // should open new position?
            if (await ShouldLongAsync(candle))
            {
                var order = Order.CreateLong(Symbol, Lever);
                await GoLongAsync(candle, order);
                return await PlaceOrderAsync(candle, order);
            }
            else if (await ShouldShortAsync(candle))
            {
                var order = Order.CreateShort(Symbol, Lever);
                await GoShortAsync(candle, order);
                return await PlaceOrderAsync(candle, order);
            }
            else
            {
                return await WaitAsync(candle, "no need to place any order");
            }
        }
        catch (Exception ex)
        {
            return StrategyDecision.Error(candle, ex);
        }
    }

    #endregion Handle new Candle


    //TODO: Handle updated Order

    //TODO: Handle updated Position
}

