namespace Trading;

public interface IExchangeOrders
{
    Task<Order?> GetOrderAsync(string id);
    Task<IEnumerable<Order>> GetOrdersAsync(string symbol);
    Task<IEnumerable<Order>> GetOpenOrdersAsync();
    Task<IEnumerable<Order>> GetOrdersAsync(OrderStatus status);

    Task PlaceOrderAsync(Candle candle, Order order);
    Task PlaceMarketOrderAsync(Candle candle, Order order, decimal executionPrice);
    Task UpdateOrderAsync(Candle candle, string id, Action<Order> configureOrder);

    Task CancelAllOrdersAsync(Candle candle);
    Task CancelOrdersAsync(Candle candle, IEnumerable<Order> orders);
    Task CancelOrdersAsync(Candle candle, string symbol);
    Task CancelOrderAsync(Candle candle, string id);
}
