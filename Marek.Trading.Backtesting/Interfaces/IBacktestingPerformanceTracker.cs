namespace Marek.Trading.Backtesting;

public interface IBacktestingPerformanceTracker
{
    bool IsRunning { get; }
    void Start();
    void Finish();
}
