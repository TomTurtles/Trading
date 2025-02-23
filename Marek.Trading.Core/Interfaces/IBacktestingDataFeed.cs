namespace Marek.Trading.Core;

public interface IBacktestingDataFeed
{
    Task<DataFeedLoadCandlesResult> LoadCandlesAsync(CancellationToken cancellationToken = default);
}
