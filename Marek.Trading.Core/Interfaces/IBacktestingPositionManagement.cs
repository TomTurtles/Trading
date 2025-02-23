namespace Marek.Trading.Core;

public interface IBacktestingPositionManagement
{
    Task<Position?> GetOpenPositionAsync(CancellationToken cancellationToken = default);
    Task<Position?> GetPositionAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Position>> GetPositionsAsync(CancellationToken cancellationToken = default);
    Dictionary<DateTime,Position> GetHistory();
    Task UpdatePositionAsync(string id, Action<Position> configure, CancellationToken cancellationToken = default);
    Task UpdatePositionByExecutedOrderAsync(DateTime timestamp, Order order, CancellationToken cancellationToken = default);
}
