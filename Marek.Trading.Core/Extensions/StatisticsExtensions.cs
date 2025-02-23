namespace Marek.Trading.Core;

public static class StatisticsExtensions
{
    /// <summary>
    /// Berechnet die Standardabweichung einer Menge von Werten.
    /// </summary>
    public static double StandardDeviation<T>(this IEnumerable<T> values, Func<T, double> selector)
    {
        if (values == null || !values.Any()) return 0;

        var selectedValues = values.Select(selector).ToList();
        double average = selectedValues.Average();
        return Math.Sqrt(selectedValues.Average(v => Math.Pow(v - average, 2)));
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

}