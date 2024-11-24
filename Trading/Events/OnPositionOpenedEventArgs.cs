namespace Trading;

public class OnPositionOpenedEventArgs(Candle candle, Position position) : TradingBaseEventArgs(candle)
{
    public Position Position { get; } = position;
}
