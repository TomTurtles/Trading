namespace Marek.Trading.Backtesting;

public class BacktestingOrderManagement(
    IBacktestingPositionManagement positionManagement,
    IBacktestingCashManagement cashManagement,
    ILogger<BacktestingOrderManagement> logger,
    IOptions<BacktestingOptions> options)
    : IBacktestingOrderManagement
{

    // Services
    public IBacktestingPositionManagement PositionManagement { get; } = positionManagement;
    public IBacktestingCashManagement CashManagement { get; } = cashManagement;
    public ILogger<BacktestingOrderManagement> Logger { get; } = logger;
    public IOptions<BacktestingOptions> Options { get; } = options;

    // Management
    private readonly ConcurrentDictionary<DateTime, List<IBacktestingOrder>> _orderHistory = new(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, List<IBacktestingOrder>> OrderedOrders => new(_orderHistory.OrderBy(o => o.Key));
    private IEnumerable<IBacktestingOrder> Orders => OrderedOrders.Values.SelectMany(o => o);

    #region Requests

    public Task<List<IBacktestingOrder>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Orders.ToList());
    }
    public async Task<List<IBacktestingOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await GetOrdersAsync(cancellationToken);
        return orders.Where(o => o.IsPending()).ToList();
    }
    public Task<IBacktestingOrder?> GetOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Orders.SingleOrDefault(o => o.Id == id) ?? null);
    }
    public Task<Dictionary<DateTime, List<IBacktestingOrder>>> GetOrderHistoryAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OrderedOrders);
    }

    #endregion Requests

    #region Commands

    public async Task PlaceOrderAsync(DateTime timestamp, IBacktestingOrder order, CancellationToken cancellationToken = default, double? marketPrice = null, double? feeRate = null)
    {
        if (order.Quantity <= 0) throw new InvalidOperationException($"invalid order quantity '{order.Quantity}'");

        // Existiert bereits schon eine passende Position oder wird eine Position neu eröffnet?
        var position = await PositionManagement.GetOpenPositionAsync(cancellationToken);
        var createNewPosition = position is null;

        // Limit oder Market?
        var isMarket = IsMarketOrder(order, marketPrice);

        // Anwenden
        if (createNewPosition)
        {
            // erzeuge neue Position, Cash wird reserviert
            PlaceOrder(timestamp, order, marketPrice);
            if (isMarket)
            {
                // Order wird direkt ausgeführt und an die Position angehängt
                ExecuteOrder(timestamp, order, marketPrice, feeRate);
            }
        }
        else
        {
            // aktualisiere bestehende Position
            Guard.IsNotNull(position);

            // geht die Order in die gleiche Richtung?
            var isSameSide = order.IsSameSideAs(position);

            // Wenn isSameSide, dann gleiche Logik wie bei Erzeugung neuer Position
            // Wenn nicht, dann cash nicht investieren bei PlaceOrder
            PlaceOrder(timestamp, order, marketPrice, isSameSide);
            if (isMarket)
            {
                // Order wird direkt ausgeführt und an die Position angehängt
                ExecuteOrder(timestamp, order, marketPrice, feeRate);
            }
        }
    }

    public Task ExecuteOrderAsync(DateTime timestamp, IBacktestingOrder order, double executionPrice, double feeRate, CancellationToken cancellationToken)
    {
        ExecuteOrder(timestamp, order, executionPrice, feeRate);
        return Task.CompletedTask;
    }

    public Task CheckOrdersToExecuteAsync(Candle candle, double marketPrice, double feeRate, CancellationToken cancellationToken = default)
    {
        var pendingOrders = Orders.Where(o => o.IsPending());

        var ordersToExecute = pendingOrders.Where(o => o.CandleHit(candle));

        foreach (var order in ordersToExecute)
        {
            var executionPrice = order.IsMarket() ? marketPrice : order.Price!.Value;
            ExecuteOrder(candle.Timestamp, order, executionPrice, feeRate);
        }

        return Task.CompletedTask;
    }

    private bool IsMarketOrder(IOrder order, double? marketPrice = null)
    {
        if (marketPrice is null) return false;
        if (order.IsMarket()) return true;

        if (order.Price.IsPriceNear(marketPrice)) return true;
        
        // Wenn eine Limit Order zu teuer drin ist, dann wird sie zur MarkOrder korrigiert
        return order.IsLong()
            ? order.Price >= marketPrice
            : order.Price <= marketPrice;
    }

    public Task CancelOrderAsync(DateTime timestamp, string id, CancellationToken cancellationToken = default)
    {
        var order = Orders.SingleOrDefault(o => o.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));

        if (order is not null)
        {
            CancelOrder(timestamp, order);
        }
        return Task.CompletedTask;
    }


    #endregion Commands


    /// <summary>
    /// Wenn ich eine Order platziere, dann muss das Geld reserviert werden.
    /// Wenn es eine MarketOrder ist, dann muss Geld analog zu MarketPrice reserviert werden.
    /// Wenn es eine LimitOrder ist, dann muss Geld analog zu Order.Price reserviert werden.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="order"></param>
    private void PlaceOrder(DateTime timestamp, IBacktestingOrder order, double? marketPrice, bool doAddCash = true)
    {
        if (Orders.Select(o => o.Id).Contains(order.Id))
        {
            throw new Exception($"Order Id already in use '{order.Id}'");
        }

        order.ValidateBeforePlacement(marketPrice);

        var price = (order.IsMarket() ? order.Price ?? marketPrice : order.Price)
            ?? throw new NullReferenceException("unable to estimate placing price");

        _orderHistory.AddOrUpdate(
            timestamp,
            ts => [order],
            (ts, list) => [order, ..list]
        );

        order.SetPlaced(timestamp, price);

        if (order.PlacedPrice is null) throw new NullReferenceException(nameof(order.PlacedPrice));

        if (doAddCash)
        {
            CashManagement.AddCash(new(timestamp, (-1) * order.GetValue(), "Place Order"));
        }
    }

    /// <summary>
    /// Wenn ich eine Order abbreche, dann wird reserviertes Geld frei
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="order"></param>
    private void CancelOrder(DateTime timestamp, IBacktestingOrder order)
    {
        // hier ist die Reihenfolge wichtig
        CashManagement.AddCash(new(timestamp, order.GetValue(), "Cancel Order"));
        order.SetCancelled(timestamp);
        if (order.PlacedPrice is null) throw new NullReferenceException(nameof(order.PlacedPrice));
    }

    /// <summary>
    /// Eine Order ausführen verändert den Positionsbestand
    /// Entweder wird eine neue Positioneröffnet oder eine bestehende aktualisiert.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="order"></param>
    /// <param name="executionPrice"></param>
    /// <param name="feeRate"></param>
    private void ExecuteOrder(DateTime timestamp, IBacktestingOrder order, double? executionPrice, double? feeRate)
    {
        if (executionPrice is null) throw new NullReferenceException($"execution price is not set for executing a market order");
        if (feeRate is null) throw new NullReferenceException($"fee rate is not set for executing a market order");
        order.SetExecuted(timestamp, executionPrice.Value, feeRate.Value);
        PositionManagement.UpdatePositionByExecutedOrderAsync(timestamp, order);
    }
}
