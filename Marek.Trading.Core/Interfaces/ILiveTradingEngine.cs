namespace Marek.Trading.Core;

public interface ILiveTradingEngine
{
    LiveTradingState State { get; }
    Exception? Exception { get; }

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
