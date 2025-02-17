namespace Marek.Trading.Core;

public class BacktestingCandleResultNotification(BacktestingCandleResult result) 
{
    public BacktestingCandleResult Result { get; } = result;
}
