namespace Marek.Trading.Backtesting;

public interface IBacktestingCashState
{
    public DateTime Timestamp { get; } 
    public double Relative { get; }
    public double Cash { get; }
    public string Reason { get; }
}
