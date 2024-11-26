namespace Trading.Backtesting;

public interface IDataFeed
{
    string Name { get; }

    Task<IEnumerable<Candle>> GetCandlesAsync(DateTime start, DateTime end);
}
