namespace Marek.Trading.Backtesting;

public class OnBacktestingStateChangedEventArgs(BacktestingState state, Exception? exception = null) : EventArgs
{
    public BacktestingState State { get; } = state;
    public Exception? Exception { get; } = exception;
}
