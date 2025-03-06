namespace Marek.Trading.Backtesting;

public static class PerformanceTrackerExtensions
{
    /// <summary>
    /// Wandelt eine Liste von absoluten Equity-Werten in relative Renditen um.
    /// </summary>
    public static List<decimal> ToRelativeReturns(this IEnumerable<decimal> equityValues)
    {
        var values = equityValues.ToList();
        var returns = new List<decimal>();

        for (int i = 1; i < values.Count; i++)
        {
            if (values[i - 1] != 0)
            {
                decimal relativeReturn = (values[i] - values[i - 1]) / values[i - 1];
                returns.Add(relativeReturn);
            }
            else
            {
                returns.Add(0); // Schutz vor Division durch Null
            }
        }

        return returns;
    }

    public static decimal SharpeRatio(this SortedDictionary<DateTime, decimal> equityHistory, decimal annualRiskFreeRate = 0.02m)
    {
        var interval = Math.Round((equityHistory.Keys.ToArray()[^1] - equityHistory.Keys.ToArray()[^2]).TotalSeconds, 3).ToEnum<CandleInterval>();
        return CalculateSharpeRatio(equityHistory.Values, interval, annualRiskFreeRate);
    }

    private const double SecondsPerYear = 365.25 * 24 * 60 * 60;

    
    public static decimal CalculateSharpeRatio(this IEnumerable<decimal> equityValues, CandleInterval candleInterval, decimal annualRiskFreeRate = 0.02m)
    {
        if (equityValues.Count() < 2)
            throw new ArgumentException("Mindestens zwei Werte werden benötigt, um Renditen zu berechnen.");

        var returns = equityValues.ToRelativeReturns();

        // Average Return und Standard-Abweichung beziehen sich genau auf das Candle ZeitInterval und wird aufs Jahr normiert
        var avgReturn = returns.Average();
        var stdDev = returns.StandardDeviation();
        if (stdDev == 0) return 0;

        var interval = (int)candleInterval;
        if (interval == 0) return 0;

        var annualizedReturn = avgReturn / interval * SecondsPerYear.ToDecimal();
        var annualizedStdDev = stdDev * Math.Sqrt(SecondsPerYear / interval).ToDecimal();
        return (annualizedReturn - annualRiskFreeRate) / annualizedStdDev;
    }

    public static decimal MaxDrawdown(this SortedDictionary<DateTime, decimal> equityHistory)
    {
        var maxDrawdown = 0m;
        var peak = equityHistory.Values.First();

        foreach (var equity in equityHistory.Values)
        {
            if (equity > peak)
                peak = equity;

            var drawdown = (peak - equity) / peak;
            maxDrawdown = Math.Max(maxDrawdown, drawdown);
        }

        return maxDrawdown;
    }

    public static decimal RecoveryFactor(this SortedDictionary<DateTime, decimal> equityHistory)
    {
        if (equityHistory.Count == 0) return 0;

        var start = equityHistory.Values.First();
        var end = equityHistory.Values.Last();
        var maxDrawdown = equityHistory.MaxDrawdown();

        return (maxDrawdown == 0 || start == 0) ? decimal.MaxValue : (end - start) / Math.Abs(maxDrawdown * start);
    }

    public static decimal WinRate(this SortedDictionary<DateTime, IBacktestingPosition> positionHistory)
    {
        if (positionHistory.Count == 0) return 0;
        var positions = positionHistory.Values.ToList();
        return positions.Count(p => p.RealizedPNL > 0) / (decimal)positions.Count;
    }

    public static decimal WinLossRatio(this SortedDictionary<DateTime, IBacktestingPosition> positionHistory)
    {
        if (positionHistory.Count == 0) return 0;
        var positions = positionHistory.Values.ToList();
        if (positions.Count(p => p.RealizedPNL < 0) == 0) return decimal.MaxValue;
        return positions.Where(p => p.RealizedPNL > 0).Average(p => p.RealizedPNL - p.Fee).ToDecimal() / Math.Abs(positions.Where(p => p.RealizedPNL < 0).Average(p => p.RealizedPNL - p.Fee).ToDecimal());
    }

    public static decimal KellyCriterion(this SortedDictionary<DateTime, IBacktestingPosition> positionHistory)
    {
        if (positionHistory.Count == 0) return 0;

        // Gewinnwahrscheinlichkeit
        var p = positionHistory.WinRate();

        // Gewinnfaktor
        var b = positionHistory.WinLossRatio();

        // Verlustwahrscheinlichkeit
        decimal q = 1 - p;

        // Kelly-Formel
        decimal f = (b * p - q) / b;

        // Interpretationslogik: 
        // - f <= 0: Trade vermeiden,
        // - 0 < f <= 1: Optimale Positionsgröße,
        // - f > 1: Hinweis auf überoptimistische Parameter, maximale Investition begrenzen.

        return f;
    }

    public static decimal BuyAndHoldRatio(this SortedDictionary<DateTime, decimal> equityHistory, SortedDictionary<DateTime, Candle> candles)
    {
        if (!equityHistory.Any() || !candles.Any()) return 0;

        if (!equityHistory.TryGetValue(equityHistory.First().Key, out var equityStart) ||
            !equityHistory.TryGetValue(equityHistory.Last().Key, out var equityEnd) ||
            !candles.TryGetValue(candles.First().Key, out var candleStartCandle) ||
            !candles.TryGetValue(candles.Last().Key, out var candleEndCandle))
        {
            return 0;
        }

        var candleStart = candleStartCandle.Close.ToDecimal();
        var candleEnd = candleEndCandle.Close.ToDecimal();

        if (equityStart == 0 || candleStart == 0) return 0; // Schutz gegen Division durch 0

        var equityPerformance = (equityEnd - equityStart) / equityStart;
        var candlePerformance = (candleEnd - candleStart) / candleStart;

        // Angepasste Formel für fairen Vergleich
        return (equityPerformance - candlePerformance) / (Math.Abs(candlePerformance) + 1);
    }
}