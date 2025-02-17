namespace Marek.Trading.Core;

public class BacktestingStrategyDecisionNotification(Candle candle, StrategyDecision decision) 
{
    public Candle Candle { get; } = candle;
    public StrategyDecision Decision { get; } = decision;
}
