namespace Trading;

internal static class PriceSlippageCalculator
{
    internal static decimal ApplySlippage(this decimal price)
    {
        var random = new Random();
        var maxSlippagePercentage = 0.05m / 100m;
        var slippagePercentage = (decimal)random.NextDouble() * maxSlippagePercentage;
        var slippagePart = price * slippagePercentage;

        var random2 = new Random();
        return price + (slippagePart * (decimal)random.Next(-1, 2));
    }
}
