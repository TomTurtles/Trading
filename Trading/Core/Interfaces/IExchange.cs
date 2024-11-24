namespace Trading;

public interface IExchange
{
    // identify
    string Name { get; }

    // connection
    Task ConnectAsync(params object[] args);
    Task DisconnectAsync(params object[] args);

    // candles
    Task<Candle> GetCurrentCandleAsync(string symbol, CandleInterval interval);
    Task<List<Candle>> GetCandlesAsync(string symbol, CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null);

    // orders
    Task<IEnumerable<Order>> GetOrdersAsync(string symbol);
    Task PlaceOrderAsync(Candle candle, Order order);
    Task PlaceMarketOrderAsync(Candle candle, Order order, decimal executionPrice);
    Task UpdateOrderAsync(Candle candle, string id, Action<Order?> configureOrder);
    Task CancelOrderAsync(Candle candle, string id);
    Task CancelOrdersAsync(Candle candle, IEnumerable<Order> orders);
    Task CancelOrdersAsync(Candle candle, string symbol);
    Task CancelAllOrdersAsync(Candle candle);

    // positions
    Task<IEnumerable<Position>> GetPositionsAsync();
    Task<Position?> GetPositionAsync(string symbol);
    Task UpdatePositionAsync(Candle candle, string id, Action<Position> configure);

    // others
    Task<decimal> GetMarginAsync();
    Task<decimal> GetFeeRateAsync(string symbol);
    Task<int> GetLeverageAsync(string symbol);
    Task<decimal> GetMarketPriceAsync(string symbol);
}