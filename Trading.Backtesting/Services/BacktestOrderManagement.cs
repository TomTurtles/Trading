namespace Trading.Backtesting;

public class BacktestOrderManagement
{
    private ConcurrentDictionary<DateTime, Order> OrderHistory { get; } = new ConcurrentDictionary<DateTime, Order>(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, Order> OrderedOrders => new(OrderHistory.OrderBy(o => o.Key));
    private IEnumerable<Order> Orders => OrderedOrders.Values;


    #region Get
    public Task<Order?> GetOrderAsync(string id) 
        => Task.FromResult(Orders.SingleOrDefault(o => o.ID.Equals(id)) ?? null);
    public Task<IEnumerable<Order>> GetOrdersAsync(string symbol) 
        => Task.FromResult(Orders.Where(o => o.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase)));
    public Task<IEnumerable<Order>> GetAllOrdersAsync() => Task.FromResult(Orders);
    public Task<IEnumerable<Order>> GetOpenOrdersAsync() => GetOrdersAsync(OrderStatus.Pending);
    public Task<IEnumerable<Order>> GetOpenOrdersAsync(string symbol) => GetOrdersAsync(symbol, OrderStatus.Pending);
    public Task<bool> HasOpenOrdersAsync(string symbol) 
        => Task.FromResult(Orders.Any(o => o.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase) && o.Status.Equals(OrderStatus.Pending)));
    public Task<IEnumerable<Order>> GetOrdersAsync(OrderStatus status)
        => Task.FromResult(OrderHistory.Values.Where(o => o.Status.Equals(status)));
    public Task<IEnumerable<Order>> GetOrdersAsync(string symbol, OrderStatus status)
        => Task.FromResult(OrderHistory.Values.Where(o => o.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase)).Where(o => o.Status.Equals(status)));

    #endregion get

    #region Cancel
    public Task CancelAllOrdersAsync(Candle candle)
    {
        CancelOrders(candle, OrderHistory.Values);
        return Task.CompletedTask;
    }

    public Task CancelOrdersAsync(Candle candle, IEnumerable<Order> orders)
    {
        CancelOrders(candle, orders);
        return Task.CompletedTask;
    }

    public Task CancelOrdersAsync(Candle candle, string symbol)
    {
        var orders = OrderHistory.Values.Where(o => o.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase));
        CancelOrders(candle, orders);
        return Task.CompletedTask;
    }

    public Task CancelOrderAsync(Candle candle, string id)
    {
        var orders = OrderHistory.Values.Where(o => o.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase));
        CancelOrders(candle, orders);
        return Task.CompletedTask;
    }

    private void CancelOrders(Candle candle, IEnumerable<Order> orders)
    {
        foreach (var order in orders)
        {
            order.Status = OrderStatus.Cancelled;
        }
    }

    #endregion Cancel

    #region Place & Execute
    public Task PlaceOrderAsync(Candle candle, Order order)
    {
        // Simuliere das Hinzufügen der Order und Warten auf Ausführung
        if (order.Type != OrderType.Market)
        {
            if (order.Quantity <= 0) throw new InvalidOperationException($"invalid order quantity '{order.Quantity}'");
            if (order.Price is null || order.Price <= 0) throw new InvalidOperationException($"invalid order price '{order.Price}'");
            Place(candle, order);
        }

        return Task.CompletedTask;
    }

    public Task PlaceMarketOrderAsync(Candle candle, Order order, double executionPrice, double feeRate)
    {
        // Simuliere die Ausführung der Market-Order
        if (order.Type == OrderType.Market)
        {
            if (order.Quantity <= 0) throw new InvalidOperationException($"invalid order quantity '{order.Quantity}'");
            if (executionPrice <= 0) throw new InvalidOperationException($"invalid execution price '{executionPrice}'");
            Place(candle, order);
            Execute(candle, order, executionPrice, feeRate);
        }

        return Task.CompletedTask;
    }

    private void Place(Candle candle, Order order)
    {
        if (order.Quantity <= 0) throw new InvalidOperationException($"invalid order quantity '{order.Quantity}'");
        OrderHistory.TryAdd(candle.Timestamp, order);
    }

    private void Execute(Candle candle, Order order, double executionPrice, double feeRate)
    {
        if (executionPrice <= 0) throw new InvalidOperationException($"invalid execution price '{executionPrice}'");
        order.SetExecuted(candle.Timestamp, executionPrice, feeRate);
    }

    #endregion Place & Execute

    #region Update

    public Task UpdateOrderAsync(Candle candle, string id, Action<Order> configureOrder)
    {
        var order = OrderHistory.Values
            .SingleOrDefault(o => o.ID.Equals(id, StringComparison.InvariantCulture));

        if (order != null)
        {
            configureOrder(order);
        }

        return Task.CompletedTask;
    }

    #endregion Update
}
