namespace Trading.Backtesting;
public class BacktestEnginePerformanceResult
{
    public IEnumerable<BacktestEngineCandleState> States { get; private set; }

    internal static BacktestEnginePerformanceResult FromStates(IEnumerable<BacktestEngineCandleState> states)
    {
        return new()
        {
            States = states,
        };
    }
}
