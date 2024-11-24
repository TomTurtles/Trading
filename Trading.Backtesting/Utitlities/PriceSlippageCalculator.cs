namespace Trading;

internal static class PriceSlippageCalculator
{
    internal static double ApplySlippage(this double price)
    {
        var random = new Random();
        var maxSlippagePercentage = 0.05d / 100d;
        var slippagePercentage = (double)random.NextDouble() * maxSlippagePercentage;
        var slippagePart = price * slippagePercentage;

        var random2 = new Random();
        return price + (slippagePart * (double)random.Next(-1, 2));
    }
}
