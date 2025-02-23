namespace Marek.Trading.Core;

public class BacktestingCashTransaction(DateTime timestamp, double relative, string reason)
{
    public DateTime Timestamp { get; } = timestamp;
    public double Relative { get; } = relative;
    public string Reason { get; } = reason;

    public override string ToString()
    {
        return $"{Timestamp} | -->{Relative} | {Reason}";
    }
}
