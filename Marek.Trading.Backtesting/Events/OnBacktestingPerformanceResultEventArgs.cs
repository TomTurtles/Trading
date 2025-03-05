namespace Marek.Trading.Backtesting;

public class OnBacktestingPerformanceResultEventArgs(BacktestPerformanceResult result) : EventArgs
{
    public BacktestPerformanceResult Result { get; set; } = result;
}