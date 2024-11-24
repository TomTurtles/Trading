namespace Trading;


public interface IStrategy
{
    // Definition
    string Name { get; }

    // Entry
    Task ExecuteAsync(Candle candle);
}
