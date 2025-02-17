namespace Marek.Trading.Backtesting;

public class BacktestingExchange : ExchangeBase, IBacktestingExchange
{
    // Services
    public IBacktestingOrderManagement OrderManagement { get; }
    public IBacktestingPositionManagement PositionManagement { get; }
    public IBacktestingCashManagement CashManagement { get; }
    public IMareatorEventDispatcher EventDispatcher { get; }

    // Identy
    public override string Name => "Backtesting";

    // Candles
    private Candle? _candle;
    private List<Candle>? _candles;
    private Candle Candle => _candle ?? throw new NullReferenceException(nameof(_candle));
    private List<Candle> Candles => _candles ?? throw new NullReferenceException(nameof(_candles));


    public BacktestingExchange(
        IMareatorEventDispatcher eventDispatcher, 
        ILogger<IExchange> logger,
        IBacktestingOrderManagement orderManagement,
        IBacktestingPositionManagement positionManagement,
        IBacktestingCashManagement cashManagement,
        IOptions<BacktestingOptions> options) 
        : base(eventDispatcher, logger, options.Value)
    {
        OrderManagement = orderManagement;
        PositionManagement = positionManagement;
        CashManagement = cashManagement;
        EventDispatcher = eventDispatcher;

        EventDispatcher.Subscribe<OnStrategyDecisionEventArgs>(NotifyAccountReport);
    }

    #region Requests

    public override Task<List<Candle>> GetCandlesAsync(int? limit = null, DateTime? start = null, DateTime? end = null, CancellationToken cancellationToken = default)
    {
        // Simuliere das Abrufen von Candles (kann nicht in die Zukunft gucken)
        var result = Candles.Where(c => c.Timestamp < Candle.Timestamp);
        if (start != null) result = result.Where(c => c.Timestamp >= start.Value);
        if (end != null) result = result.Where(c => c.Timestamp <= end.Value);
        if (limit != null) result = result.OrderBy(c => c.Timestamp).Take(limit.Value);
       
        return Task.FromResult(result.ToList());
    }

    public override Task<Candle> GetCandleAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Candle);
    }
    public override async Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        return await OrderManagement.GetOrderAsync(id, cancellationToken);
    }
    public override async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await OrderManagement.GetOrdersAsync(cancellationToken);
    }
    public override async Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await OrderManagement.GetPendingOrdersAsync(cancellationToken);
    }
    public override async Task<Position?> GetOpenPositionAsync(CancellationToken cancellationToken = default)
    {
        return await PositionManagement.GetOpenPositionAsync(cancellationToken);
    }
    public override async Task<IEnumerable<Position>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        return await PositionManagement.GetPositionsAsync(cancellationToken);
    }
    public override async Task<double> GetEquityAsync(CancellationToken cancellationToken = default)
    {
        var margin = await GetMarginAsync();
        var position = await GetOpenPositionAsync();
        if (position == null) return margin;
        var marketPrice = await GetMarketPriceAsync();
        return margin + position.GetValue(marketPrice);
    }
    public override Task<double> GetFeeRateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0.01);
    }
    public override async Task<double> GetMarginAsync(CancellationToken cancellationToken = default)
    {
        return await CashManagement.GetMarginAsync(cancellationToken);
    }
    public override Task<double> GetMarketPriceAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Candle.Close);
    }

    #endregion Requests

    #region Commands
    public override async Task PlaceOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        // Hier wird (momentan) nicht zwischen Market/Limit Order unterschieden, erst in OrderManagement
        var marketPrice = await GetMarketPriceAsync(cancellationToken);
        var feeRate = await GetFeeRateAsync(cancellationToken);
        await OrderManagement.PlaceOrderAsync(Candle.Timestamp, order, cancellationToken, marketPrice, feeRate);
    }

    public override async Task CancelOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        await OrderManagement.CancelOrderAsync(Candle.Timestamp, id, cancellationToken);
    }

    public override async Task CancelOrdersAsync(List<Order> orders, CancellationToken cancellationToken = default)
    {
        var tasks = orders.Select(order => CancelOrderAsync(order.Id));
        await Task.WhenAll(tasks.ToArray());
    }

    public override async Task ClosePositionAsync(string id, double? executionPrice = null, CancellationToken cancellationToken = default)
    {
        executionPrice ??= await GetMarketPriceAsync();

        // aktuelle (offene) Position holen
        var positionToLiquidate = await GetOpenPositionAsync() ?? throw new NullReferenceException(nameof(GetOpenPositionAsync));

        // Liquiditäts-Order erstellen
        var liquidationOrder = new Order(positionToLiquidate.Side.ToOppositeOrderSide(), positionToLiquidate.Symbol)
        {
            Quantity = positionToLiquidate.Quantity
        };

        // Liquiditäts-Order platzieren
        await PlaceOrderAsync(liquidationOrder, cancellationToken);
        
        // Restliche Folge-Effekte (Positionsausgleich, Cash-Flow) entstehen in den entsprechenden Klassen
    }

    #endregion Commands

    public void SetCandles(List<Candle> candles) => _candles = candles;
    public void SetCandle(Candle candle) => _candle = candle;

    public async Task<bool> HasMarginCallAsync(CancellationToken cancellationToken = default)
    {
        var equity = await GetEquityAsync();
        return equity < MarginCallLevel;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await CheckClosePositionAsync(cancellationToken);
        await CheckExecuteOrderAsync(cancellationToken);
    }


    private async Task CheckExecuteOrderAsync(CancellationToken cancellationToken)
    {
        var candle = await GetCandleAsync(cancellationToken);
        var marketPrice = await GetMarketPriceAsync(cancellationToken);
        var feeRate = await GetFeeRateAsync(cancellationToken);
        await OrderManagement.CheckOrdersToExecuteAsync(candle, marketPrice, feeRate, cancellationToken);

        //var orders = await GetOrdersAsync(cancellationToken);

        //var pendingOrders = orders.Where(o => o.IsPending());
        //if (!pendingOrders.Any()) return;

        //var ordersToExecute = pendingOrders.Where(order => order.CandleHit(candle));
        //if (!ordersToExecute.Any()) return;

        //var marketPrice = await GetMarketPriceAsync(cancellationToken);

        //var tasks = ordersToExecute.Select(async order =>
        //{
        //    await PlaceOrderAsync(order, cancellationToken);
        //});
        //await Task.WhenAll(tasks);
    }

    /// <summary> 
    /// Exchange reacts on new candle first
    /// stoploss/takeprofit?
    /// </summary>
    /// <param name="candle"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async Task CheckClosePositionAsync(CancellationToken cancellationToken)
    {
        var candle = await GetCandleAsync();

        var position = await GetOpenPositionAsync();
        if (position is null) return;

        // takeprofit
        if (position.TakePrice is not null && candle.IsTakeProfitHit(position))
        {
            await ClosePositionAsync(position.Id, position.TakePrice.Value);
            EventDispatcher.Publish(this, new OnPositionClosedEventArgs(candle, position));
        }

        // stoploss
        if (position.StopPrice is not null && candle.IsStopLossHit(position))
        {
            await ClosePositionAsync(position.Id, position.StopPrice.Value);
            EventDispatcher.Publish(this, new OnPositionClosedEventArgs(candle, position));
        }
    }



    public async Task NotifyAccountReportAsync(CancellationToken cancellationToken = default)
    {
        EventDispatcher.Publish(this, new OnAccountReportEventArgs(Candle, await GetEquityAsync(), await GetMarginAsync()));
    }

    private async void NotifyAccountReport(object sender, OnStrategyDecisionEventArgs e)
    {
        EventDispatcher.Publish(this, new OnAccountReportEventArgs(e.Candle, await GetEquityAsync(), await GetMarginAsync()));
    }
}
