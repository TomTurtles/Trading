namespace Trading.Backtesting;

public class BacktestExchange : IBacktestExchange
{
    public BacktestExchange(IEventSystem eventSystem)
    {
        EventSystem = eventSystem;
        Cash = new BacktestCash(EventSystem);
        Orders = new BacktestOrders(EventSystem);
        Positions  = new BacktestPositions(EventSystem);
    }

    // services
    private IEventSystem EventSystem { get; }
    public IExchangeCash Cash { get; }
    public IExchangeOrders Orders { get; }
    public IExchangePositions Positions { get; }

    // identify
    public string Name => "Backtest";

    // candle
    private Candle CurrentCandle { get; set; } = new();
    private IEnumerable<Candle> Candles { get; set; } = Enumerable.Empty<Candle>();

    // state
    private bool IsConnected { get;set; }


    #region Connection

    /// <summary>
    /// Simuliere die Verbindung, danach werden candles emittiert
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task ConnectAsync(params object[] args)
    {
        // Erstes Argument beinhaltet das Initial Cash
        if (args.Length == 0) throw new ArgumentException("args[0] must be filled with initial cash value");

        // Zweites Argument beinhaltet die zu emittierenden Candles
        if (args.Length <= 1) throw new ArgumentException("args[1] must be filled with candles from datafeed");

        try
        {
            Candles = (IEnumerable<Candle>)args[1];
        }
        catch (Exception ex)
        {
            throw new ArgumentException("args[1] must be filled with candles from datafeed", ex);
        }

        try
        {
            var initCash = Convert.ToInt32(args[0]);
            Cash.InitializeCash(Candles.First(), initCash);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("args[0] must be filled with initial cash value", ex);
        }


        // Connection successful
        EventSystem.Publish(EventType.OnConnectionOpened, new OnConnectionOpenedEventArgs());

        // Candles emittieren
        IsConnected = true;
        var frequency = (int?)args.ElementAtOrDefault(2) ?? 1000;
        foreach (var candle in Candles)
        {
            CurrentCandle = candle;
            EventSystem.Publish(EventType.OnNewCandle, new OnCandleCreatedEventArgs(candle));

            // Drittes Argument beinhaltet die Frequenz der Emission
            await Task.Delay(frequency);
        };
    }

    public Task DisconnectAsync(params object[] args)
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    #endregion Connection

    #region Candle

    public Task<Candle> GetCurrentCandleAsync(string symbol, CandleInterval interval)
    {
        throw new NotImplementedException();
    }

    public Task<List<Candle>> GetCandlesAsync(string symbol, CandleInterval interval, int? limit = null, DateTime? start = null, DateTime? end = null)
    {
        // Simuliere das Abrufen von Candles
        return Task.FromResult(Candles.Where(c => c.Timestamp < CurrentCandle.Timestamp).ToList());
    }

    #endregion Candle

    #region Market Data

    // Abrufen der Gebühr für ein bestimmtes Symbol
    public Task<decimal> GetFeeRateAsync(string symbol)
    {
        // Beispiel: Eine feste Gebühr 
        return Task.FromResult(0.00001m);
    }

    public async Task<decimal> CalculateFeeAsync(string symbol, decimal price, decimal quantity)
    {
        decimal feeRate = await GetFeeRateAsync(symbol);
        return Math.Abs(price * quantity) * feeRate; // Berechne die Gebühr
    }

    // Abrufen des Hebels für ein bestimmtes Symbol
    public Task<int> GetLeverageAsync(string symbol)
    {
        // Beispiel: Fester Hebel 
        return Task.FromResult(10);
    }

    // Abrufen des aktuellen Marktpreises
    public Task<decimal> GetMarketPriceAsync(string symbol)
    {
        return Task.FromResult<decimal>(CurrentCandle.Close);
    }

    public async Task<decimal> GetMarketPriceWithSlippage(string symbol)
    {
        var marketPrice = await GetMarketPriceAsync(symbol);
        return marketPrice.ApplySlippage();
    }

    #endregion Market Data

    #region Cash

    public Task<decimal> GetMarginAsync() => Cash.GetMarginAsync();
    private bool CanAfford(decimal cost) => Cash.CanAfford(cost);

    #endregion Cash

    #region Equity

    private async Task<decimal> GetEquityAsync()
    {
        var equity = 0m;
        equity += await GetMarginAsync();
        var positions = await GetPositionsAsync();
        foreach (var position in positions)
        {
            var marketPrice = await GetMarketPriceAsync(position.Symbol);
            equity += (marketPrice * position.Quantity);
        }
        return equity;
    }

    #endregion Equity

    #region Orders

    // Abrufen der Orders für ein bestimmtes Symbol
    public Task<IEnumerable<Order>> GetOrdersAsync(string symbol) => Orders.GetOrdersAsync(symbol);
    public Task PlaceOrderAsync(Candle candle, Order order) => Orders.PlaceOrderAsync(candle, order);
    public Task PlaceMarketOrderAsync(Candle candle, Order order, decimal executionPrice) => Orders.PlaceMarketOrderAsync(candle, order, executionPrice);
    public Task UpdateOrderAsync(Candle candle, string id, Action<Order?> configureOrder) => Orders.UpdateOrderAsync(candle, id, configureOrder);
    public Task CancelOrderAsync(Candle candle, string id) => Orders.CancelOrderAsync(candle, id);
    public Task CancelAllOrdersAsync(Candle candle) => Orders.CancelAllOrdersAsync(candle);
    public Task CancelOrdersAsync(Candle candle, IEnumerable<Order> orders) => Orders.CancelOrdersAsync(candle, orders);
    public Task CancelOrdersAsync(Candle candle, string symbol) => Orders.CancelOrdersAsync(candle, symbol);

    #endregion Orders

    #region Positions


    // Abrufen der aktuellen Position
    public Task<Position?> GetPositionAsync(string symbol) => Positions.GetPositionAsync(symbol);
    public Task<IEnumerable<Position>> GetPositionsAsync() => Positions.GetPositionsAsync();
    public Task UpdatePositionAsync(Candle candle, string id, Action<Position> configure) => Positions.UpdatePositionAsync(candle, id, configure);

    #endregion Positions
}
