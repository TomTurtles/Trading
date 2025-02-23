namespace Marek.Trading.Core;

public class OnBacktestingCashUpdatedEventArgs(BacktestingCashState state) : EventArgs, IBacktestingCashState
{
    public DateTime Timestamp => state.Timestamp;
    public double Relative => state.Relative;
    public double Cash => state.Cash;
    public string Reason => state.Reason;
}
