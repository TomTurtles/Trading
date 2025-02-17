namespace Marek.Trading.Core;

public interface IDataFeedExchange 
{
    string Name { get; }
    Task<List<Candle>> GetCandlesAsync(string symbol, CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null);
}
