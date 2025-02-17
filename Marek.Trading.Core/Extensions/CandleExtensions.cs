namespace Marek.Trading.Core;
public static class CandleExtensions
{
    public static bool IsTakeProfitHit(this Candle candle, Position? position)
    {
        if (position?.TakePrice is null) return false;

        if (position.Side == PositionSide.LONG)
        {
            return position.TakePrice.Value <= candle.High;
        }
        else
        {
            return position.TakePrice.Value >= candle.Low;
        }
    }
    public static bool IsStopLossHit(this Candle candle, Position? position)
    {
        if (position?.StopPrice is null) return false;

        if (position.Side == PositionSide.LONG)
        {
            return position.StopPrice.Value >= candle.Low;
        }
        else
        {
            return position.StopPrice.Value <= candle.High;
        }
    }
}
