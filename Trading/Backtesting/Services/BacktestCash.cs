namespace Trading;

public class BacktestCash : IExchangeCash
{
    private IEventSystem EventSystem { get; }
    private decimal InitialCash { get; set; }
    private ConcurrentDictionary<DateTime, decimal> Cash { get; } = new ConcurrentDictionary<DateTime, decimal>(DateTimeEqualityComparer.Use());
    private IOrderedEnumerable<KeyValuePair<DateTime, decimal>> OrderedCash => Cash.OrderBy(kvp => kvp.Key);
    private decimal Margin => OrderedCash.LastOrDefault().Value;

    // risk
    private decimal MarginCallPercentage => 30m / 100m;
    private decimal MarginCallLevel => InitialCash * MarginCallPercentage;

    public BacktestCash(IEventSystem eventSystem)
    {
        EventSystem = eventSystem;
        EventSystem.Subscribe<OnNewCandleEventArgs>(EventType.OnNewCandle, async evt => await HandleNewCandleAsync(evt.Candle, evt.Symbol));
        EventSystem.Subscribe<OnPositionOpenedEventArgs>(EventType.OnPositionOpened, async evt => await HandlePositionOpened(evt.Candle, evt.Position));
        EventSystem.Subscribe<OnPositionClosedEventArgs>(EventType.OnPositionClosed, async evt => await HandlePositionClosedAsync(evt.Candle, evt.Position));
        EventSystem.Subscribe<OnPositionUpdatedEventArgs>(EventType.OnPositionUpdated, async evt => await HandlePositionUpdatedAsync(evt.Candle, evt.Position, evt.OldPosition));
    }

    #region Initialize
    public void InitializeCash(Candle candle, int initCash)
    {
        InitialCash = initCash;
        AddCash(candle, initCash);
    }

    #endregion Initialize

    #region Get

    public decimal GetInitialCash() => InitialCash;
    public Task<decimal> GetMarginAsync() => Task.FromResult(Margin);
    public bool CanAfford(decimal cost) => Margin >= cost;
    public Dictionary<DateTime, decimal> GetPerformance() => OrderedCash.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    #endregion Get

    #region Add

    public void AddCash(Candle candle, decimal relative)
    {
        Cash.TryAdd(candle.Timestamp, relative);
        EventSystem.Publish<OnCashChangedEventArgs>(EventType.OnCashChanged, new OnCashChangedEventArgs(candle, Margin));
    }

    #endregion Add

    #region Handler

    private async Task HandleNewCandleAsync(Candle candle, string symbol)
    {
        // Margin Call prüfen
        if (Margin < MarginCallLevel)
        {
            // TODO: where market price?
            var marketPrice = 0m;
            EventSystem.Publish<OnMarginCallEventArgs>(EventType.OnMarginCall, new OnMarginCallEventArgs(candle, marketPrice));
        }
    }
    private Task HandlePositionOpened(Candle candle, Position position)
    {
        AddCash(candle, (-1) * position.Quantity * position.EntryPrice);
        return Task.CompletedTask;
    }

    private Task HandlePositionUpdatedAsync(Candle candle, Position position, Position oldPosition)
    {
        // Vorzeichen schon einberechnet in diff
        var diff = oldPosition.Quantity - position.Quantity;
        AddCash(candle, position.EntryPrice * diff); 
        return Task.CompletedTask;
    }

    private Task HandlePositionClosedAsync(Candle candle, Position position)
    {
        if (position.ExitPrice is null || position.IsOpen) return Task.CompletedTask;
        AddCash(candle, position.ExitPrice.Value * position.Quantity - position.Fee);
        return Task.CompletedTask;
    }

    #endregion Handler
}
