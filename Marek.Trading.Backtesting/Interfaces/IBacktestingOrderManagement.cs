namespace Marek.Trading.Backtesting;

public interface IBacktestingOrderManagement
{
    Task CancelOrderAsync(DateTime timestamp, string id, CancellationToken cancellationToken = default);
    Task<Dictionary<DateTime, List<IBacktestingOrder>>> GetOrderHistoryAsync(CancellationToken cancellationToken = default);
    Task<IBacktestingOrder?> GetOrderAsync(string id, CancellationToken cancellationToken = default);
    Task<List<IBacktestingOrder>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<List<IBacktestingOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    Task PlaceOrderAsync(DateTime timestamp, IBacktestingOrder order, CancellationToken cancellationToken = default, double? marketPrice = null, double? feeRate = null);
    Task CheckOrdersToExecuteAsync(Candle candle, double marketPrice, double feeRate, CancellationToken cancellationToken = default);
    Task ExecuteOrderAsync(DateTime timestamp, IBacktestingOrder order, double executionPrice, double feeRate, CancellationToken cancellationToken);
}
