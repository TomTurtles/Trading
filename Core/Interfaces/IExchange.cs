namespace Trading;
public interface IExchange
{
    Task ConnectAsync(CancellationToken token);
    Task DisconnectAsync(CancellationToken token);
    Task PlaceOrderAsync(Order order, CancellationToken token);
    Task CancelOrderAsync(Guid orderId, CancellationToken token);
    Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken token);

    event Action<Order> OrderExecuted;
    event Action<Position> PositionClosed;
}
