namespace Marek.Trading.Backtesting;

public class BacktestingExchange : ExchangeBase, IBacktestingExchange
{
    // Services
    public IBacktestingOrderManagement OrderManagement { get; }
    public IBacktestingPositionManagement PositionManagement { get; }
    public IBacktestingCashManagement CashManagement { get; }

    // Identy
    public override string Name => "Backtesting";

    // Candles
    private Candle? _candle;
    private List<Candle>? _candles;
    private Dictionary<CandleInterval, List<Candle>>? _additionalCandles;
    private Candle Candle => _candle ?? throw new NullReferenceException(nameof(_candle));
    private List<Candle> Candles => _candles ?? throw new NullReferenceException(nameof(_candles));
    private Dictionary<CandleInterval, List<Candle>> AdditionalCandles => _additionalCandles ?? [];


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
    }

    #region Requests

    public override Task<List<Candle>> GetCandlesAsync(CandleInterval candleInterval, int? limit = null, DateTime? start = null, DateTime? end = null, CancellationToken cancellationToken = default)
    {
        // Candle Quelle ermitteln
        List<Candle> candles = [];
        if (candleInterval == Interval)
        {
            candles = Candles;
        }
        else if (AdditionalCandles.ContainsKey(candleInterval))
        {
            candles = AdditionalCandles[candleInterval];
        }
        else
        {
            throw new ArgumentException($"Candles im Intervall {candleInterval} nicht vorhanden.");
        }

        // Simuliere das Abrufen von Candles (kann nicht in die Zukunft gucken)
        var result = candles.Where(c => c.Timestamp <= Candle.Timestamp);
        if (limit != null) result = result.OrderBy(c => c.Timestamp).Take(limit.Value);

        return Task.FromResult(result.OrderBy(c => c.Timestamp).ToList());
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
        var result = 0d;

        var margin = await GetMarginAsync();
        result += margin;

        var orders = await GetPendingOrdersAsync(cancellationToken);
        result += orders.Sum(o => o.GetValue());

        var openPosition = await GetOpenPositionAsync();
        if (openPosition == null) return result;

        // Realised PNL schlagen sich bereits im Margin nieder
        var marketPrice = await GetMarketPriceAsync();
        result += openPosition.GetUnrealizedValue(marketPrice);

        return result;
    }
    public override Task<double> GetFeeRateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0.2d / 100d);
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

    public override async Task UpdatePositionAsync(string id, Action<Position> configure, CancellationToken cancellationToken = default)
    {
        await PositionManagement.UpdatePositionAsync(id, configure, cancellationToken);
    }

    public override async Task ClosePositionAsync(string id, double? executionPrice = null, CancellationToken cancellationToken = default)
    {
        executionPrice ??= await GetMarketPriceAsync();

        // aktuelle (offene) Position holen
        var positionToLiquidate = await GetOpenPositionAsync() ?? throw new NullReferenceException(nameof(GetOpenPositionAsync));

        // Liquiditäts-Order erstellen
        var liquidationOrder = new Order(positionToLiquidate.Side.ToOppositeOrderSide(), positionToLiquidate.Symbol)
        {
            Quantity = positionToLiquidate.Quantity,
            Lever = positionToLiquidate.Lever,
        };

        // Liquiditäts-Order ausführen
        await ExecuteOrderAsync(liquidationOrder, executionPrice!.Value, cancellationToken);

        // Restliche Folge-Effekte (Positionsausgleich, Cash-Flow) entstehen in den entsprechenden Klassen
    }
    public override async Task IncreasePositionAsync(string id, double size, double? executionPrice = null, CancellationToken cancellationToken = default)
    {
        executionPrice ??= await GetMarketPriceAsync();

        // aktuelle (offene) Position holen
        var positionToUpdate = await GetOpenPositionAsync() ?? throw new NullReferenceException(nameof(GetOpenPositionAsync));

        // Liquiditäts-Order erstellen
        var updateOrder = new Order(positionToUpdate.Side.ToOrderSide(), positionToUpdate.Symbol)
        {
            Quantity = size,
            Lever = positionToUpdate.Lever,
        };

        // Market Order platzieren
        await PlaceOrderAsync(updateOrder, cancellationToken);

        // Restliche Folge-Effekte (Positionsausgleich, Cash-Flow) entstehen in den entsprechenden Klassen
    }
    public override async Task DecreasePositionAsync(string id, double size, double? executionPrice = null, CancellationToken cancellationToken = default)
    {
        executionPrice ??= await GetMarketPriceAsync();

        // aktuelle (offene) Position holen
        var positionToUpdate = await GetOpenPositionAsync() ?? throw new NullReferenceException(nameof(GetOpenPositionAsync));

        // Order erstellen
        var updateOrder = new Order(positionToUpdate.Side.ToOppositeOrderSide(), positionToUpdate.Symbol)
        {
            Quantity = size,
            Lever = positionToUpdate.Lever,
        };

        // Order ausführen
        await ExecuteOrderAsync(updateOrder, executionPrice!.Value, cancellationToken);

        // Restliche Folge-Effekte (Positionsausgleich, Cash-Flow) entstehen in den entsprechenden Klassen
    }

    #endregion Commands

    public void SetCandles(DataFeedLoadCandlesResult result)
    {
        _candles = result.Candles;
        _additionalCandles = result.AdditionalCandles;
        EventDispatcher.Publish(this, new OnCandlesLoadedEventArgs(result.Candles));
    }

    public void SetCandle(Candle candle)
    {
        _candle = candle;
        EventDispatcher.Publish(this, new OnNewCandleEventArgs(candle));
    }

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


    #region Notifications

    public async Task NotifyAccountReportAsync(CancellationToken cancellationToken = default)
    {
        EventDispatcher.Publish(this, new OnAccountReportEventArgs(await GetCandleAsync(), await GetEquityAsync(), await GetMarginAsync()));
    }

    #endregion Notifications


    private async Task CheckExecuteOrderAsync(CancellationToken cancellationToken)
    {
        var candle = await GetCandleAsync(cancellationToken);
        var marketPrice = await GetMarketPriceAsync(cancellationToken);
        var feeRate = await GetFeeRateAsync(cancellationToken);
        await OrderManagement.CheckOrdersToExecuteAsync(candle, marketPrice, feeRate, cancellationToken);
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
        if (position.IsClosed) return;

        // takeprofit
        if (position.TakeProfitPrice is not null && candle.IsTakeProfitHit(position))
        {
            await ClosePositionAsync(position.Id, position.TakeProfitPrice.Value);
        }

        if (position.IsClosed) return;

        // stoploss
        if (position.StopLossPrice is not null && candle.IsStopLossHit(position))
        {
            await ClosePositionAsync(position.Id, position.StopLossPrice.Value);
        }
    }

    private async Task ExecuteOrderAsync(Order order, double executionPrice, CancellationToken cancellationToken = default)
    {
        var feeRate = await GetFeeRateAsync(cancellationToken);
        await OrderManagement.ExecuteOrderAsync(Candle.Timestamp, order, executionPrice, feeRate, cancellationToken);
    }
}
