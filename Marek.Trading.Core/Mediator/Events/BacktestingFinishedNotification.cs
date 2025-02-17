namespace Marek.Trading.Core;

public class BacktestingFinishedNotification(TimeSpan elapsed) 
{
    public TimeSpan Elapsed { get; } = elapsed;
}
