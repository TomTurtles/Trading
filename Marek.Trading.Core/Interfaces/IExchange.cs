namespace Marek.Trading.Core;

public interface IExchange
{
    string Name { get; }

    #region Connection
    Task ConnectAsync(CancellationToken cancellationToken = default, params string[] args);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    #endregion Connection

    #region Candles
    Task<List<Candle>> GetCandlesAsync(CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null, CancellationToken cancellationToken = default);
    Task<Candle> GetCandleAsync(CancellationToken cancellationToken = default);
    #endregion Candles

    #region Orders
    Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task PlaceOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(string id, CancellationToken cancellationToken = default);
    Task CancelOrdersAsync(List<Order> orders, CancellationToken cancellationToken = default);
    #endregion Orders

    #region Positions
    Task<Position?> GetOpenPositionAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Position>> GetPositionsAsync(CancellationToken cancellationToken = default);
    Task UpdatePositionAsync(string id, Action<Position> configure, CancellationToken cancellationToken = default);
    Task ClosePositionAsync(string id, double? executionPrice = null, CancellationToken cancellationToken = default);
    Task IncreasePositionAsync(string id, double size, double? executionPrice = null, CancellationToken cancellationToken = default);
    Task DecreasePositionAsync(string id, double size, double? executionPrice = null, CancellationToken cancellationToken = default);
    #endregion Positions

    #region Account
    Task<double> GetEquityAsync(CancellationToken cancellationToken = default);
    Task<double> GetFeeRateAsync(CancellationToken cancellationToken = default);
    Task<double> GetLeverageAsync(CancellationToken cancellationToken = default);
    Task<double> GetMarginAsync(CancellationToken cancellationToken = default);
    #endregion Account

    #region Market
    Task<double> GetMarketPriceAsync(CancellationToken cancellationToken = default);
    #endregion Market
}