namespace Trading;

public class PerformanceTracker
{
    public PerformanceTracker()
    {
     
    }

    //private int NewCandleCounter = 0;
    //private int StrategyExecutedCounter = 0;


    //public IEventSystem EventSystem { get; }
    //public BacktestExchange Exchange { get; }

    //public double InitialCash { get; }
    //private Stopwatch Stopwatch { get; set; }
    //public TimeSpan Duration => Stopwatch.Elapsed;
    //public DateTime Start => RunningCandles.First().Timestamp;
    //public DateTime End => RunningCandles.Last().Timestamp;
    //public TimeSpan Period => End - Start;
    //public bool MarginCall { get; private set; } = false;
    //public IEnumerable<Candle> RunningCandles { get; }
    //public int MaxCandleCount => RunningCandles.Count();



    //// Candles
    //public int GetCandlesCount() => RunningCandles.Count();
    //public CandleInterval GetCandleInterval() => RunningCandles.First().Interval;

    //// Cash

    //public IDictionary<DateTime, double> GetCashCurve() => Exchange.CashManagement.GetPerformance();
    //public double GetCashMaximum() => GetCashCurve().Values.Max();
    //public double GetCashMinimum() => GetCashCurve().Values.Min();
    //public double GetCashStart() => GetCashCurve().Select(kvp => kvp.Value).First();
    //public double GetCashEnd() => GetCashCurve().Select(kvp => kvp.Value).Last();
    //public double GetCashPerformance() => (GetCashEnd() - GetCashStart()) / GetCashStart();
    //public double GetMaxDrawdown()
    //{
    //    var maxDrawdown = 0m;
    //    var peak = GetCashCurve().First().Value;

    //    foreach (var (date, cash) in GetCashCurve())
    //    {
    //        if (cash > peak) peak = cash;
    //        var drawdown = (peak - cash) / peak;
    //        if (drawdown > maxDrawdown) maxDrawdown = drawdown;
    //    }

    //    return -maxDrawdown;
    //}

    //// Exceptions

    //private void HandleStrategyException(OnStrategyExceptionEventArgs args) => OrderedExceptions.Add(args.Candle.Timestamp, args.GetException());

    //public Dictionary<DateTime, Exception> OrderedExceptions = new Dictionary<DateTime, Exception>();
    //public IEnumerable<string> GetExceptions() => OrderedExceptions.Values.Select(ex => ex.Message);
    //public void AddException(Candle candle, Exception ex) => OrderedExceptions.TryAdd(candle.Timestamp, ex);
    //public IEnumerable<Order> GetOrders() => Enumerable.Empty<Order>();
    //internal void CallMargin() => MarginCall = true;

    //#region Positions

    //public IEnumerable<Position> GetClosedPositions() => Exchange.PositionManagement.GetClosedPositionsAsync().GetAwaiter().GetResult();
    //public int GetClosedPositionsCount() => GetClosedPositions().Count();
    //public double GetClosedPositionsRealization() => GetClosedPositions().Sum(p => p.RealisationAfterFee ?? 0m);
    //public double GetClosedPositionsWinRate()
    //{
    //    var closedPositions = GetClosedPositions();
    //    if (!closedPositions.Any()) return 1;
    //    return (double)closedPositions.Count(p => p.Win) / GetClosedPositionsCount() * 100;
    //}

    //public double GetClosedPositionsWinToLossAverage()
    //{
    //    var closedPositions = GetClosedPositions();
    //    if (!closedPositions.Any()) return 1;

    //    var positiveRealization = closedPositions.Where(p => p.Win).Average(p => p.RealisationAfterFee!.Value);
    //    var negativeRealization = Math.Abs(closedPositions.Where(p => !p.Win).Average(p => p.RealisationAfterFee!.Value));

    //    return positiveRealization / (negativeRealization == 0 ? 1 : negativeRealization);
    //}
    //#endregion Positions

    //#region Equity

    //public IDictionary<DateTime, double> GetEquityCurve() => GetCashCurve();
    //public double GetEquityStart() => GetEquityCurve().Values.First();
    //public double GetEquityEnd() => GetEquityCurve().Values.Last();
    //public double GetEquityMinimum() => GetEquityCurve().Values.Min();
    //public double GetEquityMaximum() => GetEquityCurve().Values.Max();
    //public object GetEquityPerformance() => (GetEquityEnd() - GetEquityStart()) / GetEquityStart();

    //#endregion Equity

    //#region Stopwatch
    //private void HandleNewCandle(OnNewCandleEventArgs args)
    //{
    //    if (NewCandleCounter == 0)
    //    {
    //        Stopwatch = Stopwatch.StartNew();
    //    }

    //    NewCandleCounter++;

    //    if (NewCandleCounter == MaxCandleCount)
    //    {
    //        Console.WriteLine($"All Candles published");
    //    }
    //}
    //private void HandleStrategyExecuted(OnStrategyExecutedEventArgs args)
    //{
    //    StrategyExecutedCounter++;

    //    if (StrategyExecutedCounter == MaxCandleCount)
    //    {
    //        Console.WriteLine($"All Strategies handled");
    //        Stopwatch.Stop();
    //    }
    //}

    //internal async Task ProcessAsync()
    //{
    //    while (StrategyExecutedCounter < MaxCandleCount) await Task.Delay(30);
    //}

    //#endregion Stopwatch
}
