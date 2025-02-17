namespace Marek.Trading.Core;

public interface IBacktestingOrderManagement
{
    Task CancelOrderAsync(DateTime timestamp, string id, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    Task PlaceOrderAsync(DateTime timestamp, Order order, CancellationToken cancellationToken = default, double? marketPrice = null, double? feeRate = null);
    Task CheckOrdersToExecuteAsync(Candle candle, double marketPrice, double feeRate, CancellationToken cancellationToken = default);
}
