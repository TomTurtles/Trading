namespace Trading.Backtesting;

public interface IBacktestEngine
{
    Task<BacktestEnginePerformanceResult> RunAsync(Action<BacktestOptions> configureOptions);
}
