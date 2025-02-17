namespace Marek.Trading.Core;

public class OnStrategyDecisionEventArgs(Candle candle, StrategyDecision decision) : EventArgs
{
    public Candle Candle { get; } = candle;
    public StrategyDecision Decision { get; } = decision;
}
