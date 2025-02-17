namespace Marek.Trading.Core;
public static class DoubleExtensions
{
    public static bool IsPriceNear(this double? x, double? y, double threshold = 0.00015)
    {
        if (y is null) return false; 
        if (x is null) return false;
        if (y == 0) return false;
        
        return Math.Abs(1 - x.Value/y.Value) <= threshold;
    }
    public static bool IsPriceNear(this double x, double? y, double threshold = 0.00015)
    {
        if (y is null) return false;
        if (y == 0) return false;

        return Math.Abs(1 - x / y.Value) <= threshold;
    }
}
