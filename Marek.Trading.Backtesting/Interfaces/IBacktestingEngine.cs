namespace Marek.Trading.Backtesting;

public interface IBacktestingEngine
{
    bool IsRunning { get; }

    Task RunAsync(CancellationToken cancellationToken = default);

    event EventHandler<OnBacktestingStateChangedEventArgs> OnStateChanged;
    event EventHandler<OnBacktestingPerformanceResultEventArgs> OnFinished;
    event EventHandler<OnBacktestingPositionClosedEventArgs> OnPositionClosed;
}
