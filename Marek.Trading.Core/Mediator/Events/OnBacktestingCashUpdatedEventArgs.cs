namespace Marek.Trading.Core;
public class OnBacktestingCashUpdatedEventArgs(DateTime timestamp, double absolute, double relative) : EventArgs
{
    public DateTime Timestamp { get; } = timestamp;
    public double Absolute { get; } = absolute;
    public double Relative { get; } = relative;
}
