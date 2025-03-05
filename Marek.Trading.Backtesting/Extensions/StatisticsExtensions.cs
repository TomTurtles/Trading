namespace Marek.Trading.Backtesting;

public static class StatisticsExtensions
{
    /// <summary>
    /// Berechnet die Standardabweichung einer Menge von Werten.
    /// </summary>
    public static double StandardDeviation<T>(this IEnumerable<T> values, Func<T, double> selector)
    {
        return values.Select(selector).ToList().StandardDeviation();
    }

    /// <summary>
    /// Berechnet die Standardabweichung einer Menge von Werten.
    /// </summary>
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        if (values == null || !values.Any()) return 0;

        var selectedValues = values.ToList();
        double average = selectedValues.Average();
        return Math.Sqrt(selectedValues.Average(v => Math.Pow(v - average, 2)));
    }


    /// <summary>
    /// Berechnet die Standardabweichung einer Menge von Werten.
    /// </summary>
    public static decimal StandardDeviation(this IEnumerable<decimal> values)
    {
        if (values == null || !values.Any()) return 0;

        var selectedValues = values.ToList();
        var average = selectedValues.Average();
        return Math.Sqrt(selectedValues.Average(v => Math.Pow((v - average).ToDouble(), 2))).ToDecimal();
    }
}