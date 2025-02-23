namespace Marek.Trading.Core;

public interface IBacktestingExchange : IExchange
{
    void SetCandles(DataFeedLoadCandlesResult result);
    void SetCandle(Candle candle);
    Task<bool> HasMarginCallAsync(CancellationToken cancellationToken = default);
    Task RunAsync(CancellationToken cancellationToken = default);
    Task NotifyAccountReportAsync(CancellationToken cancellationToken = default);
}
