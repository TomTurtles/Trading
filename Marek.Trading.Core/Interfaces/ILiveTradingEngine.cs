namespace Marek.Trading.Core;

public interface ILiveTradingEngine
{
    LiveTradingState State { get; }
    Exception? Exception { get; }

    EventHandler<OnStrategyDecisionEventArgs>? OnStrategyDecision { get; set; }
    EventHandler<OnPositionOpenedEventArgs>? OnPositionOpened { get; set; }
    EventHandler<OnPositionClosedEventArgs>? OnPositionClosed { get; set; }

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
