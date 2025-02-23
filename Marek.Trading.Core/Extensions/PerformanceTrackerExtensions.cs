namespace Marek.Trading.Core;

public static class PerformanceTrackerExtensions
{
    /// <summary>
    /// Wandelt eine Liste von absoluten Equity-Werten in relative Renditen um.
    /// </summary>
    public static List<double> ToRelativeReturns(this IEnumerable<double> equityValues)
    {
        var values = equityValues.ToList();
        var returns = new List<double>();

        for (int i = 1; i < values.Count; i++)
        {
            if (values[i - 1] != 0)
            {
                double relativeReturn = (values[i] - values[i - 1]) / values[i - 1];
                returns.Add(relativeReturn);
            }
            else
            {
                returns.Add(0); // Schutz vor Division durch Null
            }
        }

        return returns;
    }

    public static double CalculateSharpeRatio(this IEnumerable<double> equityValues, double riskFreeRate = 0.02)
    {
        if (equityValues.Count() < 2) return 0;
        var returns = equityValues.ToRelativeReturns();
        var avgReturn = returns.Average();
        var stdDev = returns.StandardDeviation();

        return stdDev == 0 ? 0 : (avgReturn - riskFreeRate) / stdDev;
    }

    public static double CalculateSharpeRatio(this SortedDictionary<DateTime, double> equityDictionary, double annualRiskFreeRate = 0.02)
    {
        var interval = Math.Round((equityDictionary.Keys.ToArray()[^1] - equityDictionary.Keys.ToArray()[^2]).TotalSeconds, 3).ToEnum<CandleInterval>();
        return CalculateSharpeRatio(equityDictionary.Values, interval, annualRiskFreeRate); 
    }

    private const double SecondsPerYear = 365.25 * 24 * 60 * 60;

    public static double CalculateSharpeRatio(this IEnumerable<double> equityValues, CandleInterval candleInterval, double annualRiskFreeRate = 0.02)
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

        var annualizedReturn = avgReturn / interval * SecondsPerYear;
        var annualizedStdDev = stdDev * Math.Sqrt(SecondsPerYear / interval);
        return (annualizedReturn - annualRiskFreeRate) / annualizedStdDev;
    }
}