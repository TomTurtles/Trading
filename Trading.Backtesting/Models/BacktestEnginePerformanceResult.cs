namespace Trading.Backtesting;
public class BacktestEnginePerformanceResult
{
    public CashPerformanceResult Cash { get; private set; }
    public EquityPerformanceResult Equity { get; private set; }
    public BacktestEngineCandleState Last { get; private set; }
    public IEnumerable<BacktestEngineCandleStateError> Errors { get; private set; }

    internal static BacktestEnginePerformanceResult FromStates(IEnumerable<BacktestEngineCandleState> states)
    {
        return new()
        {
            Cash = new CashPerformanceResult(states.Where(s => s.ExchangeState != null).Select(s => s.ExchangeState!.Cash)),
            Equity = new EquityPerformanceResult(states.Where(s => s.ExchangeState != null).Select(s => s.ExchangeState!.Equity)),
            Last = states.Last(),
            Errors = states.Where(e => e.Error != null).Select(s => s.Error!)
        };
    }
}

public class CashPerformanceResult(IEnumerable<double> cashList)
{
    public double Start => cashList.First();
    public double End => cashList.Last();
    public double Min => cashList.Min();
    public double Max => cashList.Max();
    public string Performance => ((End - Start) / Start).ToString("0.## %");
}

public class EquityPerformanceResult(IEnumerable<double> equityList)
{
    public double Start => equityList.First();
    public double End => equityList.Last();
    public double Min => equityList.Min();
    public double Max => equityList.Max();
    public string Performance => ((End - Start) / Start).ToString("0.## %");
}