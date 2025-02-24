namespace Marek.Trading.Core;

public class OnPositionClosedEventArgs(DateTime timestamp, IPosition position) : EventArgs
{
    public DateTime Timestamp { get; } = timestamp;
    public IPosition Position { get; } = position;
}