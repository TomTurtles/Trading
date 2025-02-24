namespace Marek.Trading.Backtesting;
public static class CandleExtensions
{
    public static bool IsTakeProfitHit(this Candle candle, BacktestingPositionDecorator? position)
    {
        if (position?.TakeProfitPrice is null) return false;

        if (position.Side == PositionSide.LONG)
        {
            return position.TakeProfitPrice.Value <= candle.High;
        }
        else
        {
            return position.TakeProfitPrice.Value >= candle.Low;
        }
    }
    public static bool IsStopLossHit(this Candle candle, BacktestingPositionDecorator? position)
    {
        if (position?.StopLossPrice is null) return false;

        if (position.Side == PositionSide.LONG)
        {
            return position.StopLossPrice.Value >= candle.Low;
        }
        else
        {
            return position.StopLossPrice.Value <= candle.High;
        }
    }
}
