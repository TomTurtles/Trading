namespace Trading;

public class OnPositionClosedEventArgs(Candle candle, Position position) : TradingBaseEventArgs(candle)
{
    public Position Position { get; } = position;
}
