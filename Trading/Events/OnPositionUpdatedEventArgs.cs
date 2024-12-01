namespace Trading;

public class OnPositionUpdatedEventArgs(Candle candle, Position position, Position oldPosition) : TradingBaseEventArgs(candle)
{
    public Position Position { get; } = position;
    public Position OldPosition { get; } = oldPosition;
}
