namespace Marek.Trading.Core;

public class AddBacktestingCashCommand(DateTime timestamp, double relative) : ICommand
{
    public DateTime Timestamp { get; } = timestamp;
    public double Relative { get; } = relative;
}
