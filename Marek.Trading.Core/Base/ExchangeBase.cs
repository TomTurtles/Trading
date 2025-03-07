namespace Marek.Trading.Core;

public abstract class ExchangeBase : IExchange, IExchangeInitializable
{
    #region Initialize

    /// <summary>
    ///  Parameterloser Konstruktor für die Implementierung von Exchanges
    /// </summary>
    protected ExchangeBase() { }

    public virtual void Initialize(IMareatorEventDispatcher eventDispatcher, ILogger<IExchange> logger, IExchangeOptions options)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        _options = options;
    }

    #endregion Initialize

    #region Services
    private IMareatorEventDispatcher? _eventDispatcher;
    protected IMareatorEventDispatcher EventDispatcher => _eventDispatcher ?? throw new NullReferenceException(nameof(_eventDispatcher));

    private ILogger<IExchange>? _logger;
    protected ILogger<IExchange> Logger => _logger ?? throw new NullReferenceException(nameof(_logger));

    #endregion Services

    #region Options

    private IExchangeOptions? _options;
    protected string Symbol 
        => _options?.Symbol ?? throw new NullReferenceException(nameof(_options.Symbol));
    protected CandleInterval Interval 
        => _options?.Interval ?? throw new NullReferenceException(nameof(_options.Interval));
    protected double FaceValue 
        => _options?.FaceValue ?? throw new NullReferenceException(nameof(_options));
    protected IDictionary<string, object> Parameters
        => _options?.Parameters ?? new Dictionary<string, object>();

    #endregion Options

    #region Abstract / Virtual

    // Identify
    public abstract string Name { get; }


    // Connection
    public virtual Task ConnectAsync(IDictionary<string, object>? parameter = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public virtual Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    // Candles
    public EventHandler<OnNewCandleEventArgs>? OnNewCandle { get; set; }
    //public abstract Task<List<Candle>> GetHistoricalCandlesAsync(CandleInterval interval, DateTime start, DateTime end, int? limit = null, CancellationToken cancellationToken = default);
    public abstract Task<List<Candle>> GetCandlesAsync(CandleInterval interval, int? limit = null, CancellationToken cancellationToken = default);
    public abstract Task<Candle> GetCandleAsync(CancellationToken cancellationToken = default);

    // Orders
    public abstract Task<IOrder?> GetOrderAsync(string id, CancellationToken cancellationToken = default);
    public abstract Task<List<IOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    public abstract Task<List<IOrder>> GetOrdersAsync(CancellationToken cancellationToken = default);
    public abstract Task<string> PlaceOrderAsync(IOrder order, CancellationToken cancellationToken = default);
    public abstract Task CancelOrderAsync(string id, CancellationToken cancellationToken = default);
    public abstract Task CancelOrdersAsync(List<IOrder> orders, CancellationToken cancellationToken = default);

    // Positions
    public abstract Task<IPosition?> GetOpenPositionAsync(CancellationToken cancellationToken = default);
    public abstract Task<IEnumerable<IPosition>> GetPositionsAsync(CancellationToken cancellationToken = default);
    public abstract Task UpdatePositionAsync(string id, Action<IPosition> configure, CancellationToken cancellationToken = default);
    public abstract Task ClosePositionAsync(string id, double? executionPrice = null, CancellationToken cancellationToken = default);
    public abstract Task IncreasePositionAsync(string id, double size, double? executionPrice = null, CancellationToken cancellationToken = default);
    public abstract Task DecreasePositionAsync(string id, double size, double? executionPrice = null, CancellationToken cancellationToken = default);

    // Others
    public abstract Task<double> GetEquityAsync(CancellationToken cancellationToken = default);
    public abstract Task<double> GetFeeRateAsync(CancellationToken cancellationToken = default);
    public virtual Task<double> GetLeverageAsync(CancellationToken cancellationToken = default) => Task.FromResult(1d);
    public abstract Task<double> GetMarginAsync(CancellationToken cancellationToken = default);
    public abstract Task<double> GetMarketPriceAsync(CancellationToken cancellationToken = default);
    public abstract Task<IMarketProductInfo> GetMarketProductInfoAsync(CancellationToken cancellationToken = default);

    #endregion Abstract / Virtual
}
