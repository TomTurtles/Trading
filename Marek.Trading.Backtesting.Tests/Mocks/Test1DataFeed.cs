namespace Marek.Trading.Backtesting.Tests;

internal class Test1DataFeed : IDataFeedExchange
{
    public string Name => "test1";

    private readonly List<Candle> _candleList =
    [
        CandleHelper.Random(DateTime.Now.AddMinutes(-10)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-9)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-8)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-7)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-6)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-5)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-4)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-3)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-2)),
        CandleHelper.Random(DateTime.Now.AddMinutes(-1)),
    ];

    public Task<List<Candle>> GetCandlesAsync(string symbol, CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null)
    {
        return Task.FromResult(_candleList);
    }

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
