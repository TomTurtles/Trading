namespace Marek.Trading.Backtesting;

public class OnBacktestingPositionClosedEventArgs(IPosition position) : EventArgs
{
    public IPosition Position { get; } = position;
}