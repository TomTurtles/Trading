namespace Marek.Trading;

public class BacktestingPerformanceTracker : IBacktestingPerformanceTracker
{
    #region Services

    public IMareator Mareator { get; }
    public IBacktestingOrderManagement OrderManagement { get; }
    public IBacktestingPositionManagement PositionManagement { get; }
    public IBacktestingCashManagement CashManagement { get; }
    public IOptions<BacktestingOptions> Options { get; }
    public ILogger<BacktestingPerformanceTracker> Logger { get; }
    
    #endregion Services

    public bool IsRunning => _stopwatch.IsRunning;

    private readonly Stopwatch _stopwatch = new();
    private int _candlesProcessedCount = 0;
    private List<Candle> _candles = [];
    private readonly ConcurrentDictionary<DateTime, double> _equityHistory = [];

    #region Initialize

    public BacktestingPerformanceTracker(
        IMareator mareator, 
        IBacktestingOrderManagement orderManagement,
        IBacktestingPositionManagement positionManagement,
        IBacktestingCashManagement cashManagement,
        IOptions<BacktestingOptions> options,
        ILogger<BacktestingPerformanceTracker> logger)
    {
        Mareator = mareator;
        OrderManagement = orderManagement;
        PositionManagement = positionManagement;
        CashManagement = cashManagement;
        Options = options;
        Logger = logger;

        _previousEquity = Options.Value.InitialCash;
        _equityHistory.TryAdd(DateTime.MinValue, Options.Value.InitialCash);

        Mareator.Subscribe<OnCandlesLoadedEventArgs>(HandleCandlesLoaded);
        Mareator.Subscribe<OnNewCandleEventArgs>(HandleNewCandle);
        Mareator.Subscribe<OnStrategyDecisionEventArgs>(HandleStrategyDecision);
        Mareator.Subscribe<OnAccountReportEventArgs>(HandleAccountReport);
        Mareator.Subscribe<OnBacktestingCashUpdatedEventArgs>(HandleCashUpdated);
        Mareator.Subscribe<OnPositionOpenedEventArgs>(HandlePositionOpened);
        Mareator.Subscribe<OnPositionUpdatedEventArgs>(HandlePositionUpdated);
        Mareator.Subscribe<OnPositionClosedEventArgs>(HandlePositionClosed);
    }

    #endregion Initialize

    #region Start/Finish

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Finish()
    {
        _stopwatch.Stop();
        Logger.LogInformation($"Duration: {_stopwatch.Elapsed}");
        NotifyPerformanceResult();
    }

    #endregion Start/Finish

    #region EventHandlers
    private void HandleCandlesLoaded(object sender, OnCandlesLoadedEventArgs e)
    {
        _candles = e.Candles;
    }

    private void HandleNewCandle(object sender, OnNewCandleEventArgs e)
    {
        _candlesProcessedCount++;
    }

    private void HandlePositionClosed(object sender, OnPositionClosedEventArgs e)
    {

    }

    private void HandlePositionUpdated(object sender, OnPositionUpdatedEventArgs e)
    {

    }

    private void HandlePositionOpened(object sender, OnPositionOpenedEventArgs e)
    {

    }
    private void HandleStrategyDecision(object sender, OnStrategyDecisionEventArgs e)
    {
        if (e.Decision.Type == StrategyDecisionType.Wait) return;
        Logger.LogInformation($"{e.Candle} --> Decision {e.Decision}");
    }

    private double _previousEquity;
    private void HandleAccountReport(object sender, OnAccountReportEventArgs e)
    {
        if (e.Equity != _previousEquity)
        {
            Logger.LogInformation($"{e.Candle} --> Equity {e.Equity} ({(e.Equity - _previousEquity)/_previousEquity:0.00 %})");
            _previousEquity = e.Equity;
        }
        
        _equityHistory.AddOrUpdate(e.Candle.Timestamp, e.Equity, (ts, eq) => e.Equity);
    }

    private void HandleCashUpdated(object sender, OnBacktestingCashUpdatedEventArgs e)
    {

    }

    #endregion EventHandlers

    private async void NotifyPerformanceResult()
    {
        // darauf warten, dass alle Daten ankommen (equityHistory hat einen Wert mehr - InitialCash)
        await Task.Delay(200);
        Debug.WriteLine($"Vergleich CandleCount: {_candlesProcessedCount} | {_candles.Count} | {_equityHistory.Count} | {PositionManagement.GetHistory().Count} | {CashManagement.GetHistory().Count}");
        await Task.Delay(200);
        Debug.WriteLine($"Vergleich CandleCount: {_candlesProcessedCount} | {_candles.Count} | {_equityHistory.Count} | {PositionManagement.GetHistory().Count} | {CashManagement.GetHistory().Count}");
        await Task.Delay(200);
        Debug.WriteLine($"Vergleich CandleCount: {_candlesProcessedCount} | {_candles.Count} | {_equityHistory.Count} | {PositionManagement.GetHistory().Count} | {CashManagement.GetHistory().Count}");

        var strategy = new StrategyPerformanceResult(new(PositionManagement.GetHistory()));
        var backtesting = new BacktestingPerformanceResult(_stopwatch.Elapsed, _candles);
        var candles = new CandlesPerformanceResult(_candles);
        var equity = new EquityPerformanceResult(new(_equityHistory.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
        var cash = new CashPerformanceResult(new(CashManagement.GetHistory()));

        Mareator.Publish(this, new OnBacktestingPerformanceResultEventArgs(strategy, backtesting, candles, equity, cash));

    }
}
