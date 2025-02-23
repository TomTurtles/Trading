namespace Marek.Trading.Core;

public class OnPositionClosedEventArgs(DateTime timestamp, Position position) : EventArgs
{
    public DateTime Timestamp { get; } = timestamp;
    public Position Position { get; } = position;
}