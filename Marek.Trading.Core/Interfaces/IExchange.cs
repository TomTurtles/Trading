namespace Marek.Trading.Core;

public interface IExchange
{
    string Name { get; }

    #region Connection
    Task ConnectAsync(Dictionary<string, object>? parameter = null, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    #endregion Connection

    #region Candles
    EventHandler<OnNewCandleEventArgs>? OnNewCandle { get; set; }
    //Task<List<Candle>> GetHistoricalCandlesAsync(CandleInterval interval, DateTime start, DateTime end, int? limit = null, CancellationToken cancellationToken = default);
    Task<List<Candle>> GetCandlesAsync(CandleInterval interval, int? limit = null, CancellationToken cancellationToken = default);
    Task<Candle> GetCandleAsync(CancellationToken cancellationToken = default);
    #endregion Candles

    #region Orders
    Task<IOrder?> GetOrderAsync(string id, CancellationToken cancellationToken = default);
    Task<List<IOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    Task<List<IOrder>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<string> PlaceOrderAsync(IOrder order, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(string id, CancellationToken cancellationToken = default);
    Task CancelOrdersAsync(List<IOrder> orders, CancellationToken cancellationToken = default);
    #endregion Orders

    #region Positions
    Task<IPosition?> GetOpenPositionAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<IPosition>> GetPositionsAsync(CancellationToken cancellationToken = default);
    Task UpdatePositionAsync(string id, Action<IPosition> configure, CancellationToken cancellationToken = default);
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