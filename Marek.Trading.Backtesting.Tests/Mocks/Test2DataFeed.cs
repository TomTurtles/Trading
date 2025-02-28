
namespace Marek.Trading.Backtesting.Tests;

internal class Test2DataFeed : IDataFeedExchange
{
    public string Name => "test2";

    public Task CancelOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CancelOrdersAsync(List<Order> orders, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ClosePositionAsync(string id, double? executionPrice = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default, params string[] args)
    {
        throw new NotImplementedException();
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Candle> GetCandleAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Candle>> GetCandlesAsync(string symbol, CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null)
    {
        return Task.FromResult(new List<Candle>());
    }

    public Task<List<Candle>> GetCandlesAsync(int? limit = null, DateTime? start = null, DateTime? end = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<double> GetEquityAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<double> GetFeeRateAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<double> GetLeverageAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<double> GetMarginAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<double> GetMarketPriceAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Position?> GetPositionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Position>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task PlaceMarketOrderAsync(Order order, double executionPrice, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task PlaceOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
