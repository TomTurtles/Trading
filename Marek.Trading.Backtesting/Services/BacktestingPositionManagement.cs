namespace Marek.Trading.Backtesting;

public class BacktestingPositionManagement : IBacktestingPositionManagement
{

    // Services
    public IMareatorEventDispatcher EventDispatcher { get; }
    public IBacktestingCashManagement CashManagement { get; }
    public ILogger<BacktestingOrderManagement> Logger { get; }
    public IOptions<BacktestingOptions> Options { get; }

    // Management
    private ConcurrentDictionary<DateTime, IBacktestingPosition> PositionHistory { get; } = new(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, IBacktestingPosition> OrderedPositionHistory => new(PositionHistory.OrderBy(kvp => kvp.Key));
    private IEnumerable<IBacktestingPosition> Positions => OrderedPositionHistory.Values;

    public BacktestingPositionManagement(
        IMareatorEventDispatcher eventDispatcher,
        IBacktestingCashManagement cashManagement,
        ILogger<BacktestingOrderManagement> logger,
        IOptions<BacktestingOptions> options)
    {
        EventDispatcher = eventDispatcher;
        CashManagement = cashManagement;
        Logger = logger;
        Options = options;
    }

    #region Requests

    public Task<IBacktestingPosition?> GetOpenPositionAsync(CancellationToken cancellationToken = default)
    {
        var openPosition = Positions.SingleOrDefault(p => p.IsOpen()) ?? null;
        return Task.FromResult(openPosition);
    }
    public Task<IBacktestingPosition?> GetPositionAsync(string id, CancellationToken cancellationToken = default)
    {
        var openPosition = Positions.SingleOrDefault(p => p.Id == id) ?? null;
        return Task.FromResult(openPosition);
    }
    public Task<List<IBacktestingPosition>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        var positions = Positions.ToList();
        return Task.FromResult(positions);
    }

    public Dictionary<DateTime, IBacktestingPosition> GetHistory() => OrderedPositionHistory;

    #endregion Requests

    #region Commands

    public Task UpdatePositionAsync(string id, Action<IBacktestingPosition> configure, CancellationToken cancellationToken = default)
    {
        var position = Positions.SingleOrDefault(p => p.IsOpen()) ?? throw new NullReferenceException("only open positions can be updated.");
        configure?.Invoke(position);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Hier passieren Veränderung in Quantity und Price
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="order"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task UpdatePositionByExecutedOrderAsync(DateTime timestamp, Order order, CancellationToken cancellationToken = default)
    {
        if (order.IsPending()) throw new InvalidOperationException($"pending orders cannot update positions");
        if (order.IsCancelled()) throw new InvalidOperationException($"cancelled orders cannot update positions");
        if (order.ExecutedPrice is null) throw new InvalidOperationException($"must have {nameof(order.ExecutedPrice)} != null to update positions");
        if (order.ExecutedFee is null) throw new InvalidOperationException($"must have {nameof(order.ExecutedFee)} != null to update positions");

        var position = Positions.SingleOrDefault(p => p.IsOpen());

        if (position == null)
        {
            // add
            position = CreatePositionFromOrder(order);
            var success = PositionHistory.TryAdd(timestamp, position);
            if (!success) 
            {
                Logger.LogWarning($"{timestamp} Konnte Position nicht zur PositionHistory hinzufügen: {position}");
            }
            NotifyPositionOpened(timestamp, position);
        }
        else
        {
            // update
            position.AddExecutedOrder(order);

            if (order.IsOppositeSideAs(position))
            {
                var cashToAdd = position.EntryPrice * order.Quantity + position.GetUnrealizedPNL(order.ExecutedPrice.Value, order.Quantity);
                CashManagement.AddCash(new(timestamp, cashToAdd, "Sell Position"));
            }
            else
            {
                // In diesem Fall müsste alles Cash-Relevante über die PlaceOrder Logik gelaufen sein
            }

            if (position.IsClosed()) NotifyPositionClosed(timestamp, position);
            if (position.IsOpen()) NotifyPositionUpdated(timestamp, position);
        }

        // always: fee reducing cash
        CashManagement.AddCash(new (timestamp, (-1) * order.ExecutedFee!.Value, "Order Fee"));

        return Task.CompletedTask;
    }

    #endregion Commands


    #region Notifications

    public void NotifyPositionOpened(DateTime timestamp, IBacktestingPosition position)
    {
        EventDispatcher.Publish(this, new OnPositionOpenedEventArgs(timestamp, position));
    }
    public void NotifyPositionUpdated(DateTime timestamp, IBacktestingPosition position)
    {
        EventDispatcher.Publish(this, new OnPositionUpdatedEventArgs(timestamp, position));
    }
    public void NotifyPositionClosed(DateTime timestamp, IBacktestingPosition position)
    {
        EventDispatcher.Publish(this, new OnPositionClosedEventArgs(timestamp, position));
    }

    #endregion Notifications


    private static IBacktestingPosition CreatePositionFromOrder(Order order)
    {
        if (order.Quantity <= 0) throw new InvalidOperationException($"invalid quantity: '{order.Quantity}'");

        var position = new Position()
        {
            Symbol = order.Symbol,
            Side = order.ToPositionSide(),
            StopLossPrice = order.StopLossPrice,
            TakeProfitPrice = order.TakeProfitPrice,
        };

        var backtestingPosition = new BacktestingPositionDecorator(position);

        backtestingPosition.AddExecutedOrder(order);

        return backtestingPosition;
    }
}
