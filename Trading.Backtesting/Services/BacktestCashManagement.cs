namespace Trading;

public class BacktestCashManagement 
{
    private IEventSystem EventSystem { get; }
    private double InitialCash { get; set; }
    private ConcurrentDictionary<DateTime, double> CashHistory { get; } = new ConcurrentDictionary<DateTime, double>(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, double> OrderedCash => new(CashHistory.OrderBy(kvp => kvp.Key));
    private double Margin => OrderedCash.LastOrDefault().Value;

    // risk
    private double MarginCallPercentage => 30d / 100d;
    private double MarginCallLevel => InitialCash * MarginCallPercentage;

    #region Initialize
    public void InitializeCash(double initCash)
    {
        InitialCash = initCash;
    }

    #endregion Initialize

    #region Get

    public double GetInitialCash() => InitialCash;
    public Task<double> GetMarginAsync() => Task.FromResult(Margin);
    public bool CanAfford(double cost) => Margin >= cost;
    public Dictionary<DateTime, double> GetPerformance() => OrderedCash.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    #endregion Get

    #region Add

    public void AddCash(Candle candle, double relative)
    {
        CashHistory.TryAdd(candle.Timestamp, relative);
        EventSystem.Publish<OnCashChangedEventArgs>(EventType.OnCashChanged, new OnCashChangedEventArgs(candle, Margin));
    }

    #endregion Add

    #region MarginCall
    public bool ShouldMarginCall() => Margin < MarginCallLevel;

    #endregion MarginCall
}
