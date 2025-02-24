namespace Marek.Trading.Live;

public class LiveTradingEngine(
    ILogger<LiveTradingEngine> logger,
    IExchange exchange,
    IStrategy strategy,
    IOptions<LiveTradingOptions> options
    ) : ILiveTradingEngine
{
    #region Services
    public ILogger<LiveTradingEngine> Logger { get; } = logger;
    public IExchange Exchange { get; } = exchange;
    public IStrategy Strategy { get; } = strategy;
    public LiveTradingOptions Options { get; } = options.Value;
    #endregion Services

    #region State
    public LiveTradingState State { get; private set; } = LiveTradingState.Idle;
    public Exception? Exception { get; private set; } = null;
    public CancellationTokenSource CancellationTokenSource { get; private set; } = new CancellationTokenSource();
    #endregion State

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Connect
            await Exchange.ConnectAsync(Options.Parameter, cancellationToken);

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
            Logger.LogInformation($"Livetrading HandleNewCandle {e}");
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
