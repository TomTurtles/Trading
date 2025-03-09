namespace Marek.Trading.Live;

public class LiveTradingEngine : ILiveTradingEngine
{
    #region Services
    public ILogger<LiveTradingEngine> Logger { get; }
    public IExchange Exchange { get; }
    public IStrategy Strategy { get; }
    public IMareatorEventDispatcher EventDispatcher { get; }
    public LiveTradingOptions Options { get; }
    #endregion Services

    #region State
    public LiveTradingState State { get; private set; } = LiveTradingState.Idle;
    public Exception? Exception { get; private set; } = null;
    public CancellationTokenSource CancellationTokenSource { get; private set; } = new CancellationTokenSource();

    #endregion State

    #region Events

    public EventHandler<OnStrategyDecisionEventArgs>? OnStrategyDecision { get; set; }
    public EventHandler<OnPositionOpenedEventArgs>? OnPositionOpened { get; set; }
    public EventHandler<OnPositionClosedEventArgs>? OnPositionClosed { get; set; }

    #endregion Events

    public LiveTradingEngine(
        ILogger<LiveTradingEngine> logger,
        IExchange exchange,
        IStrategy strategy,
        IOptions<LiveTradingOptions> options,
        IMareatorEventDispatcher eventDispatcher
    )
    {
        Logger = logger;
        Exchange = exchange;
        Strategy = strategy;
        EventDispatcher = eventDispatcher;
        Options = options.Value;

        eventDispatcher.Subscribe<OnStrategyDecisionEventArgs>((s, e) => OnStrategyDecision?.Invoke(this, new(e.Candle, e.Decision)));
        eventDispatcher.Subscribe<OnPositionOpenedEventArgs>((s, e) => OnPositionOpened?.Invoke(this, new(e.Timestamp, e.Position)));
        eventDispatcher.Subscribe<OnPositionClosedEventArgs>((s, e) => OnPositionClosed?.Invoke(this, new(e.Timestamp, e.Position)));
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Connect
            await Exchange.ConnectAsync(Options.Parameters, cancellationToken);

            // Subscribe
            Exchange.OnNewCandle += HandleNewCandle;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Livetrading general exception");
            Exception = ex;
            State = LiveTradingState.Error;
        }

        Logger.LogInformation($"Livetrading started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Cancel
        CancellationTokenSource.Cancel();

        // Unsubscribe
        Exchange.OnNewCandle -= HandleNewCandle;

        // Disconnect
        await Exchange.DisconnectAsync(cancellationToken);

        State = LiveTradingState.Idle;
        Logger.LogInformation($"Livetrading stopped");
    }

    private async void HandleNewCandle(object sender, OnNewCandleEventArgs e)
    {
        try
        {
            Logger.LogInformation($"Livetrading HandleNewCandle {e.Candle}");
            await Strategy.RunAsync(e.Candle, CancellationTokenSource.Token);
            State = LiveTradingState.Running;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Livetrading exception while HandleNewCandle {e}");
            Exception = ex;
            State = LiveTradingState.Error;
        }
    }
}
