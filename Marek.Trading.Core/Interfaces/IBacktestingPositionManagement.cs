

namespace Marek.Trading.Core;

public interface IBacktestingPositionManagement
{
    Task<Position?> GetOpenPositionAsync(CancellationToken cancellationToken = default);
    Task<Position?> GetPositionAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Position>> GetPositionsAsync(CancellationToken cancellationToken = default);
    Task UpdatePositionByOrderAsync(DateTime timestamp, Order order, CancellationToken cancellationToken = default);
}
