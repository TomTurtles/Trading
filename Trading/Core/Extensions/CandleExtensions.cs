namespace Trading;
public static class CandleExtensions
{
    public static bool PriceHit(this Candle candle, decimal price)
    {
        return candle.LowHit(price) && candle.HighHit(price);
    }
    public static bool LowHit(this Candle candle, decimal price)
    {
        return candle.Low <= price;
    }
    public static bool HighHit(this Candle candle, decimal price)
    {
        return candle.High >= price;
    }
}
