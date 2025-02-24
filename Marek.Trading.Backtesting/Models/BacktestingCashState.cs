namespace Marek.Trading.Backtesting;

public class BacktestingCashState(BacktestingCashTransaction transaction, double? previousCash = null)
    : BacktestingCashTransaction(transaction.Timestamp, transaction.Relative, transaction.Reason), IBacktestingCashState
{
    public double Cash { get; } = (previousCash ?? 0) + transaction.Relative;

    public override string ToString()
    {
        return $"{Timestamp} | {Relative} --> {Cash} | {Reason}";
    }
}
