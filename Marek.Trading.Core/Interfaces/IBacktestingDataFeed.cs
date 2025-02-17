namespace Marek.Trading.Core;

public interface IBacktestingDataFeed
{
    Task<List<Candle>> GetCandlesAsync(CancellationToken cancellationToken = default);
}
