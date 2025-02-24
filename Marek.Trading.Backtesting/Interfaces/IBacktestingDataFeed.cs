namespace Marek.Trading.Backtesting;

public interface IBacktestingDataFeed
{
    Task<DataFeedLoadCandlesResult> LoadCandlesAsync(CancellationToken cancellationToken = default);
}
