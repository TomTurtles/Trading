namespace Trading;

public interface IStrategy
{
    // Definition
    string Name { get; }

    // Entry
    Task<StrategyDecision> ExecuteAsync(Candle candle);
}
