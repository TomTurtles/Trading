namespace Trading;

public class BacktestCashManagement 
{
    private double InitialCash { get; set; }
    private ConcurrentDictionary<DateTime, double> CashHistory { get; } = new ConcurrentDictionary<DateTime, double>(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, double> OrderedCash => new(CashHistory.OrderBy(kvp => kvp.Key));
    private double Margin => OrderedCash.LastOrDefault().Value;

    // risk
    private double MarginCallPercentage => 30d / 100d;
    private double MarginCallLevel => InitialCash * MarginCallPercentage;

    #region Initialize
    public void InitializeCash(Candle candle, double initCash)
    {
        InitialCash = initCash;
        AddCash(candle, initCash);
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
        if (Margin + relative < 0) throw new InvalidOperationException($"transaction ({relative}) not allowed, insufficient margin ({Margin}).");
        var newMargin = Margin + relative;
        if (!CashHistory.TryAdd(candle.Timestamp, newMargin))
        {
            //throw new InvalidOperationException($"unable to add new cash for candle {candle}.");
            CashHistory.AddOrUpdate(candle.Timestamp, relative, (ts, prev) => prev + relative);
        };
    }

    #endregion Add

    #region MarginCall
    public bool ShouldMarginCall(Position? position, double marketPrice)
    {
        if (position == null) return false;
        var positionValue = position.GetValue(marketPrice);
        return positionValue + Margin < MarginCallLevel;
    }

    #endregion MarginCall
}
