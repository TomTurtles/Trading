namespace Marek.Trading.Core;

public class OnPositionClosedEventArgs(Candle candle, Position position) : EventArgs
{
    public Candle Candle { get; } = candle;
    public Position Position { get; } = position;
}
