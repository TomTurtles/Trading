namespace Trading;

public interface IExchange
{
    // identify
    string Name { get; }

    // connection
    Task ConnectAsync();
    Task DisconnectAsync();

    // candles
    Task<Candle> GetCurrentCandleAsync(string symbol, CandleInterval interval);
    Task<List<Candle>> GetCandlesAsync(string symbol, CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null);

    // orders
    Task<IEnumerable<Order>> GetOrdersAsync(string symbol);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task PlaceOrderAsync(Order order);
    Task PlaceMarketOrderAsync(Order order, double executionPrice);
    Task UpdateOrderAsync(string id, Action<Order?> configureOrder);
    Task CancelOrderAsync(string id);
    Task CancelOrdersAsync(IEnumerable<Order> orders);
    Task CancelOrdersAsync(string symbol);
    Task CancelAllOrdersAsync();

    // positions
    Task<IEnumerable<Position>> GetPositionsAsync();
    Task<Position?> GetPositionAsync(string symbol);
    Task UpdatePositionAsync(string id, Action<Position> configure);

    // others
    Task<double> GetMarginAsync();
    Task<double> GetEquityAsync();
    Task<double> GetFeeRateAsync(string symbol);
    Task<int> GetLeverageAsync(string symbol);
    Task<double> GetMarketPriceAsync(string symbol);
}