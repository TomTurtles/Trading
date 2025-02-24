namespace Marek.Trading.Live;

public class LiveTradingEngine(ILogger<LiveTradingEngine> logger) : ILiveTradingEngine
{
    public LiveTradingState State { get; private set; }

    public ILogger<LiveTradingEngine> Logger { get; } = logger;

    public void Start()
    {
        State = LiveTradingState.Running;
        Logger.LogInformation($"Livetrading started");
    }

    public void Stop()
    {
        State = LiveTradingState.Idle;
        Logger.LogInformation($"Livetrading stopped");
    }
}
