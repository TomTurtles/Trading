namespace Marek.Trading.Backtesting;

public interface IBacktestingPositionManagement
{
    Task<IBacktestingPosition?> GetOpenPositionAsync(CancellationToken cancellationToken = default);
    Task<IBacktestingPosition?> GetPositionAsync(string id, CancellationToken cancellationToken = default);
    Task<List<IBacktestingPosition>> GetPositionsAsync(CancellationToken cancellationToken = default);
    Dictionary<DateTime, IBacktestingPosition> GetHistory();
    Task UpdatePositionAsync(string id, Action<IBacktestingPosition> configure, CancellationToken cancellationToken = default);
    Task UpdatePositionByExecutedOrderAsync(DateTime timestamp, Order order, CancellationToken cancellationToken = default);
}
