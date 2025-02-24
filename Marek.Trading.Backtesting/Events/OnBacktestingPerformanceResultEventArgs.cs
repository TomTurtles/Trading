namespace Marek.Trading.Backtesting;

public class OnBacktestingPerformanceResultEventArgs(
    StrategyPerformanceResult strategy,
    BacktestingPerformanceResult backtesting,
    CandlesPerformanceResult candles,
    EquityPerformanceResult equity,
    CashPerformanceResult cash) : EventArgs
{
    public double EquityPerformanceRatio => Equity.Performance / Candles.Performance;

    public EquityPerformanceResult Equity { get; } = equity;
    public StrategyPerformanceResult Strategy { get; } = strategy;
    public CashPerformanceResult Cash { get; } = cash;
    public CandlesPerformanceResult Candles { get; } = candles;
    public BacktestingPerformanceResult Backtesting { get; } = backtesting;

}
public class StrategyPerformanceResult(SortedDictionary<DateTime, IBacktestingPosition> positionHistory)
{
    private readonly IEnumerable<IPosition> _positions = positionHistory.Values;
    private IEnumerable<IPosition> WinPositions => _positions.Where(p => p.RealizedPNL > 0); 
    private IEnumerable<IPosition> LossPositions => _positions.Where(p => p.RealizedPNL < 0);

    public double PositionsCount => _positions.Count();
    public TimeSpan AveragePositionLifetime => TimeSpan.FromSeconds(_positions.Average(p => p.Lifetime.TotalSeconds));
    public double TotalFee => _positions.Sum(p => p.Fee);
    public double Profit => _positions.Sum(p => p.RealizedPNL - p.Fee);
    public double AverageProfitPerPosition => _positions.Average(p => p.RealizedPNL - p.Fee);
    public double WinLossRatio
    {
        get
        {
            if (!LossPositions.Any()) return double.PositiveInfinity; // Kein Verlust -> unbegrenztes Verhältnis
            if (!WinPositions.Any()) return 0; // Kein Gewinn -> Verhältnis ist 0
            return WinPositions.Average(p => p.RealizedPNL - p.Fee) / Math.Abs(LossPositions.Average(p => p.RealizedPNL - p.Fee));
        }
    }
    public double WinRate
    {
        get
        {
            if (_positions.Count() == 0) return 0;
            return WinPositions.Count() / (double)_positions.Count();
        }
    }

    public double KellyCriterion
    {
        get
        {
            if (WinLossRatio <= 0) return 0;
            return WinRate - (1 - WinRate) / WinLossRatio;
        }
    }
}

public class CandlesPerformanceResult(List<Candle> candles)
{
    private readonly List<Candle> _candles = [.. candles.OrderBy(c => c.Timestamp)];
    public int CandlesCount => _candles.Count;
    public double Max => _candles.Max(c => c.Close);
    public double Min => _candles.Min(c => c.Close);
    public double Start => _candles.First().Close;
    public double End => _candles.Last().Close;
    public double Performance => (End - Start) / Start;
    public int Days => (_candles.Last().Timestamp - _candles.First().Timestamp).Days;
}

public class BacktestingPerformanceResult(TimeSpan duration, List<Candle> candles)
{
    private readonly List<Candle> _candles = [.. candles.OrderBy(c => c.Timestamp)];
    public TimeSpan Duration { get; } = duration;
    public double CandlesPerSecond => (double)_candles.Count / Duration.TotalSeconds;
}

public class EquityPerformanceResult(SortedDictionary<DateTime, double> equityHistory)
{
    private readonly SortedDictionary<DateTime, double> _equityHistory = equityHistory;
    public double Count => _equityHistory.Count();
    public double Max => _equityHistory.Values.Max();
    public double Min => _equityHistory.Values.Min();
    public double Start => _equityHistory.Values.First();
    public double End => _equityHistory.Values.Last();
    public double Performance => (End - Start) / Start;
    public double SharpeRatio => _equityHistory.CalculateSharpeRatio();
        
    public double MaximumDrawdown
    {
        get
        {
            double maxDrawdown = 0;
            double peak = _equityHistory.Values.First();

            foreach (var equity in _equityHistory.Values)
            {
                if (equity > peak)
                    peak = equity;

                double drawdown = (peak - equity) / peak;
                maxDrawdown = Math.Max(maxDrawdown, drawdown);
            }

            return maxDrawdown;
        }
    }
    public double RecoveryFactor => (MaximumDrawdown == 0 || Start == 0) ? double.PositiveInfinity : (End - Start) / Math.Abs(MaximumDrawdown * Start);
}

public class CashPerformanceResult(SortedDictionary<DateTime, BacktestingCashState> cashHistory)
{
    private readonly SortedDictionary<DateTime, double> _cashHistory = new(cashHistory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Cash));
    public double Count => _cashHistory.Count();
    public double Max => _cashHistory.Values.Max();
    public double Min => _cashHistory.Values.Min();
    public double Start => _cashHistory.Values.First();
    public double End => _cashHistory.Values.Last();
    public double Performance => (End - Start) / Start;
}