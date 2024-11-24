namespace Trading;

public class PerformanceResult(IPerformanceTracker tracker)
{
    public BacktestPerformanceResult Backtest { get; } = new(tracker);
    public PeriodPerformanceResult Period { get; } = new(tracker);
    public EquityPerformanceResult Equity { get; } = new(tracker);
    public CashPerformanceResult Cash { get; } = new(tracker);
}

public class HistoryPerformanceResult(IPerformanceTracker tracker)
{
}


public class EquityPerformanceResult(IPerformanceTracker tracker)
{
    public decimal Start => tracker.GetEquityStart();
    public decimal End => tracker.GetEquityEnd();
    public decimal Min => tracker.GetEquityMinimum();
    public decimal Max => tracker.GetEquityMaximum();
    public string Performance => $"{tracker.GetEquityPerformance():0.00%}";
}


public class BacktestPerformanceResult(IPerformanceTracker tracker)
{
    public TimeSpan Duration => tracker.Duration;
    public int Candles => tracker.GetCandlesCount();

    [ConvertStringEnum]
    public CandleInterval Interval => tracker.GetCandleInterval();
}

public class PeriodPerformanceResult(IPerformanceTracker tracker)
{
    public DateTime Start => tracker.Start;
    public DateTime End => tracker.End;
    public TimeSpan Duration => tracker.Period;
}

public class CashPerformanceResult(IPerformanceTracker tracker)
{
    public decimal Start => tracker.GetCashStart();
    public decimal End => tracker.GetCashEnd();
    public decimal Min => tracker.GetCashMinimum();
    public decimal Max => tracker.GetCashMaximum();
    public string Performance => $"{tracker.GetCashPerformance():0.00%}";
    public string MaxDrawdown => $"{tracker.GetMaxDrawdown():0.00%}";
}

public class PositionPerformanceResult(IPerformanceTracker tracker)
{
    public int Closed => tracker.GetClosedPositionsCount();
    public decimal Realisation => tracker.GetClosedPositionsRealization();
    public decimal WinRate => tracker.GetClosedPositionsWinRate();
    public decimal AverageWinToLoss => tracker.GetClosedPositionsWinToLossAverage();
    public IEnumerable<Position> ClosedPositions => tracker.GetClosedPositions();
}
