namespace Marek.Trading;

public abstract class ExchangeBase : IExchange, IExchangeInitializable
{
    #region Initialize

    protected ExchangeBase() { }
    protected ExchangeBase(IMareatorEventDispatcher eventDispatcher, ILogger<IExchange> logger, IExchangeOptions options) => Initialize(eventDispatcher, logger, options);
    public void Initialize(IMareatorEventDispatcher eventDispatcher, ILogger<IExchange> logger, IExchangeOptions options)
    {
        _logger = logger;
        _options = options;
        _eventDispatcher = eventDispatcher;
    }

    #endregion Initialize

    #region Services

    private ILogger<IExchange>? _logger;
    protected ILogger<IExchange> Logger => _logger ?? throw new NullReferenceException(nameof(_logger));

    private IMareatorEventDispatcher? _eventDispatcher;
    protected IMareatorEventDispatcher EventDispatcher => _eventDispatcher ?? throw new NullReferenceException(nameof(_eventDispatcher));

    #endregion Services

    #region Options

    private IExchangeOptions? _options;

    protected string Symbol => _options?.Symbol ?? throw new NullReferenceException(nameof(_options));
    protected CandleInterval Interval => _options?.Interval ?? throw new NullReferenceException(nameof(_options));
    protected double InitialCash => _options?.InitialCash ?? throw new NullReferenceException(nameof(_options));
    protected double MarginCallLevel => _options?.MarginCallLevel ?? throw new NullReferenceException(nameof(_options)); 

    #endregion Options

    #region Abstract / Virtual

    // Identify
    public abstract string Name { get; }


    // Connection
    public virtual Task ConnectAsync(Dictionary<string, object>? parameter = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public virtual Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    // Candles
    public EventHandler<OnNewCandleEventArgs>? OnNewCandle { get; set; }
    public abstract Task<List<Candle>> GetCandlesAsync(CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null, CancellationToken cancellationToken = default);
    public abstract Task<Candle> GetCandleAsync(CancellationToken cancellationToken = default);

    // Orders
    public abstract Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken = default);
    public abstract Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    public abstract Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default);
    public abstract Task PlaceOrderAsync(Order order, CancellationToken cancellationToken = default);
    public abstract Task CancelOrderAsync(string id, CancellationToken cancellationToken = default);
    public abstract Task CancelOrdersAsync(List<Order> orders, CancellationToken cancellationToken = default);

    // Positions
    public abstract Task<Position?> GetOpenPositionAsync(CancellationToken cancellationToken = default);
    public abstract Task<IEnumerable<Position>> GetPositionsAsync(CancellationToken cancellationToken = default);
    public abstract Task UpdatePositionAsync(string id, Action<Position> configure, CancellationToken cancellationToken = default);
    public abstract Task ClosePositionAsync(string id, double? executionPrice = null, CancellationToken cancellationToken = default);
    public abstract Task IncreasePositionAsync(string id, double size, double? executionPrice = null, CancellationToken cancellationToken = default);
    public abstract Task DecreasePositionAsync(string id, double size, double? executionPrice = null, CancellationToken cancellationToken = default);

    // Others
    public abstract Task<double> GetEquityAsync(CancellationToken cancellationToken = default);
    public abstract Task<double> GetFeeRateAsync(CancellationToken cancellationToken = default);
    public virtual Task<double> GetLeverageAsync(CancellationToken cancellationToken = default) => Task.FromResult(1d);
    public abstract Task<double> GetMarginAsync(CancellationToken cancellationToken = default);
    public abstract Task<double> GetMarketPriceAsync(CancellationToken cancellationToken = default);

    #endregion Abstract / Virtual
}
