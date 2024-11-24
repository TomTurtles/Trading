namespace Trading.Backtesting;

public class BacktestOrders : IExchangeOrders
{
    public IEventSystem EventSystem { get; }
    public decimal FeeRate => 0.001m;
    private ConcurrentDictionary<Candle, Order> OrdersDictionnary { get; set; } = [];
    private Dictionary<Candle, Order> OpenOrders => OrdersDictionnary.Where(o => o.Value.IsOpen()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public BacktestOrders(IEventSystem eventSystem)
    {
        EventSystem = eventSystem;
        EventSystem.Subscribe<OnCancelOrdersEventArgs>(EventType.OnCancelOrders, async evt => await HandleOnCandleOrders(evt.Candle, evt.Orders));
        EventSystem.Subscribe<OnGoLongEventArgs>(EventType.OnGoLong, async evt => await HandeOnGoLongAsync(evt.Candle, evt.Order, evt.MarketPrice));
        EventSystem.Subscribe<OnGoShortEventArgs>(EventType.OnGoShort, async evt => await HandleOnGoShortAsync(evt.Candle, evt.Order, evt.MarketPrice));
        EventSystem.Subscribe<OnNewCandleEventArgs>(EventType.OnNewCandle, async evt => await CheckExecuteOrdersAsync(evt.Candle));
    }

    #region Get
    public Task<Order?> GetOrderAsync(string id) 
        => Task.FromResult(OrdersDictionnary.Values.SingleOrDefault(o => o.ID.Equals(id)) ?? null);

    public Task<IEnumerable<Order>> GetOrdersAsync(string symbol) 
        => Task.FromResult(OrdersDictionnary.Values.Where(o => o.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase)));

    public Task<IEnumerable<Order>> GetOpenOrdersAsync() => GetOrdersAsync(OrderStatus.Pending);

    public Task<IEnumerable<Order>> GetOrdersAsync(OrderStatus status)
        => Task.FromResult(OrdersDictionnary.Values.Where(o => o.Status.Equals(status)));

    #endregion get

    #region Cancel
    public Task CancelAllOrdersAsync(Candle candle)
    {
        CancelOrders(candle, OrdersDictionnary.Values);
        return Task.CompletedTask;
    }

    public Task CancelOrdersAsync(Candle candle, IEnumerable<Order> orders)
    {
        CancelOrders(candle, orders);
        return Task.CompletedTask;
    }

    public Task CancelOrdersAsync(Candle candle, string symbol)
    {
        var orders = OrdersDictionnary.Values.Where(o => o.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase));
        CancelOrders(candle, orders);
        return Task.CompletedTask;
    }

    public Task CancelOrderAsync(Candle candle, string id)
    {
        var orders = OrdersDictionnary.Values.Where(o => o.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase));
        CancelOrders(candle, orders);
        return Task.CompletedTask;
    }

    private void CancelOrders(Candle candle, IEnumerable<Order> orders)
    {
        foreach (var order in orders)
        {
            order.Status = OrderStatus.Cancelled;
            EventSystem.Publish<OnOrderCancelledEventArgs>(EventType.OnOrderCancelled, new OnOrderCancelledEventArgs(candle, order));
        }
    }

    #endregion Cancel

    #region Place & Execute
    public Task PlaceOrderAsync(Candle candle, Order order)
    {
        // Simuliere das Hinzufügen der Order und Warten auf Ausführung
        if (order.Type != OrderType.Market)
        {
            Place(candle, order);
        }

        return Task.CompletedTask;
    }

    public Task PlaceMarketOrderAsync(Candle candle, Order order, decimal executionPrice)
    {
        // Simuliere die Ausführung der Market-Order
        if (order.Type == OrderType.Market)
        {
            Place(candle, order);
            Execute(candle, order, executionPrice);
        }

        return Task.CompletedTask;
    }

    private void Place(Candle candle, Order order)
    {
        OrdersDictionnary.TryAdd(candle, order);
        EventSystem.Publish<OnOrderPlacedEventArgs>(EventType.OnOrderPlaced, new OnOrderPlacedEventArgs(candle, order));
    }

    private async Task CheckExecuteOrdersAsync(Candle candle)
    {
        var openOrders = await GetOpenOrdersAsync();

        var ordersToExecute = openOrders.Where(order => order.CandleHit(candle));

        foreach (var order in ordersToExecute)
        {
            var executionPrice = order.Price ?? candle.Close;
            var executionTime = candle.Timestamp;
            Execute(candle, order, executionPrice);
        }
    }

    private void Execute(Candle candle, Order order, decimal executionPrice)
    {
        order.SetExecuted(candle.Timestamp, executionPrice, FeeRate);
        EventSystem.Publish<OnOrderExecutedEventArgs>(EventType.OnOrderExecuted, new OnOrderExecutedEventArgs(candle, order));
    }

    #endregion Place & Execute

    #region Update

    public Task UpdateOrderAsync(Candle candle, string id, Action<Order> configureOrder)
    {
        var order = OrdersDictionnary.Values
            .SingleOrDefault(o => o.ID.Equals(id, StringComparison.InvariantCulture));

        if (order != null)
        {
            configureOrder(order);
        }

        return Task.CompletedTask;
    }

    #endregion Update

    #region Handler

    private async Task HandleOnCandleOrders(Candle candle, IEnumerable<Order> orders)
    {
        await CancelOrdersAsync(candle, orders);
    }

    private async Task HandleOnGoShortAsync(Candle candle, Order order, decimal marketPrice)
    {
        if (order.Type == OrderType.Market)
        {
            await PlaceMarketOrderAsync(candle, order, marketPrice);
        }
        else
        {
            await PlaceOrderAsync(candle, order);
        }
    }

    private async Task HandeOnGoLongAsync(Candle candle, Order order, decimal marketPrice)
    {
        if (order.Type == OrderType.Market)
        {
            await PlaceMarketOrderAsync(candle, order, marketPrice);
        }
        else
        {
            await PlaceOrderAsync(candle, order);
        }
    }

    #endregion Handler
}
