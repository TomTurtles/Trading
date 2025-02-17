namespace Marek.Trading.Core;

public class BacktestingExchangeInitializeCommand(List<Candle> candles) : ICommand
{
    public List<Candle> Candles { get; } = candles;
}
