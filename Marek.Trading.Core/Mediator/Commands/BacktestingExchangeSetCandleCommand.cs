namespace Marek.Trading.Core;

public class BacktestingExchangeSetCandleCommand(Candle candle) : ICommand
{
    public Candle Candle { get; } = candle;
}
