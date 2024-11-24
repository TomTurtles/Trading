using Trading.Backtesting;

namespace Trading;

public interface IBacktestEngine
{
    Task<PerformanceResult> RunAsync(Action<BacktestOptions> configureOptions);
}
