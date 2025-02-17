namespace Marek.Trading.Core;

public class BacktestingStrategySetCandleCommand(Candle candle) : ICommand
{
    public Candle Candle { get; } = candle;
}
