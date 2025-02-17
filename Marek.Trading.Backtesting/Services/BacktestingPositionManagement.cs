namespace Marek.Trading.Backtesting;

public class BacktestingPositionManagement : IBacktestingPositionManagement
{
    // Services
    public IBacktestingCashManagement CashManagement { get; }
    public ILogger<BacktestingOrderManagement> Logger { get; }
    public IOptions<BacktestingOptions> Options { get; }

    // Management
    private ConcurrentDictionary<DateTime, Position> PositionHistory { get; } = new(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, Position> OrderedPositionHistory => new(PositionHistory.OrderBy(kvp => kvp.Key));
    private IEnumerable<Position> Positions => OrderedPositionHistory.Values;

    public BacktestingPositionManagement(
        IBacktestingCashManagement cashManagement,
        ILogger<BacktestingOrderManagement> logger,
        IOptions<BacktestingOptions> options)
    {
        CashManagement = cashManagement;
        Logger = logger;
        Options = options;
    }

    #region Requests

    public Task<Position?> GetOpenPositionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Positions.SingleOrDefault(p => p.IsOpen) ?? null);
    }
    public Task<Position?> GetPositionAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Positions.SingleOrDefault(p => p.Id == id) ?? null);
    }
    public Task<List<Position>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Positions.ToList());
    }

    #endregion Requests

    public Task UpdatePositionByOrderAsync(DateTime timestamp, Order order, CancellationToken cancellationToken = default)
    {
        if (order.IsPending()) throw new InvalidOperationException($"pending orders cannot update positions");
        if (order.IsCancelled()) throw new InvalidOperationException($"cancelled orders cannot update positions");
        if (order.ExecutedPrice is null) throw new InvalidOperationException($"must have {nameof(order.ExecutedPrice)} != null to update positions");
        if (order.ExecutedFee is null) throw new InvalidOperationException($"must have {nameof(order.ExecutedFee)} != null to update positions");

        var position = Positions.SingleOrDefault(p => p.IsOpen);

        if (position == null)
        {
            // add
            position = Position.CreateFromOrder(order);
            PositionHistory.TryAdd(timestamp, position);
        }
        else
        {
            // update
            position.AddExecutedOrder(order);

            if (order.IsOppositeSideAs(position))
            {
                CashManagement.AddCash(timestamp, position.GetValue(null, order.Quantity));
            }
        }

        // always: fee reducing cash
        CashManagement.AddCash(timestamp, (-1) * order.ExecutedFee.Value);

        return Task.CompletedTask;
    }

}
