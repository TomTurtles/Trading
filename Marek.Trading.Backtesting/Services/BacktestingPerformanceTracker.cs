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

        _previousMarketPrice = 0;
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
        Logger.LogInformation($"{e.Timestamp} --> Position Closed {e.Position.Side} --> {e.Position.RealizedPNL}");
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
        Logger.LogInformation($"{e.Candle} --> Decision {e.Decision} ({string.Join(" | ", e.Decision.PositionUpdateCommands ?? [])})");
    }

    private double _previousMarketPrice;
    private double _previousEquity;
    private void HandleAccountReport(object sender, OnAccountReportEventArgs e)
    {
        if (e.Equity != _previousEquity)
        {
            Logger.LogInformation($"{e.Candle} ({(e.Candle.Close - _previousMarketPrice) / _previousMarketPrice:0.00 %}) --> Equity {e.Equity} ({(e.Equity - _previousEquity) / _previousEquity:0.00 %})");
        }

        _previousMarketPrice = e.Candle.Close;
        _previousEquity = e.Equity;
        _equityHistory.AddOrUpdate(e.Candle.Timestamp, e.Equity, (ts, eq) => e.Equity);
    }

    private void HandleCashUpdated(object sender, OnBacktestingCashUpdatedEventArgs e)
    {

    }

    #endregion EventHandlers

    private async void NotifyPerformanceResult()
    {
        try
        { 
            // Warten wegen Race-Conditions und async Event Verarbeitung
            await Task.Delay(10);

            var candles = _candles.OrderBy(c => c.Timestamp).ToList();
            var candlesHistory = new SortedDictionary<DateTime, Candle>(_candles.OrderBy(c => c.Timestamp).ToDictionary(c => c.Timestamp, c => c));
            var equityHistory = new SortedDictionary<DateTime, decimal>(_equityHistory.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDecimal()));
            var positionHistory = new SortedDictionary<DateTime, IBacktestingPosition>(PositionManagement.GetHistory());
            var positions = positionHistory.Values.ToList();


            var result = new BacktestPerformanceResult()
            {
                // Performance
                Duration = _stopwatch.Elapsed,
                Days = (candles.Last().Timestamp - candles.First().Timestamp).Days,
                CandlesPerSecond = candles.Count() / _stopwatch.Elapsed.TotalSeconds,

                // Candles
                Candles = candles.Count(),
                CandleStart = candles.First().Close.ToDecimal(),
                CandleEnd = candles.Last().Close.ToDecimal(),
                CandleMax = candles.Max(c => c.Close).ToDecimal(),
                CandleMin = candles.Min(c => c.Close).ToDecimal(),
                CandlePerformance = ((candles.Last().Close - candles.First().Close) / candles.First().Close).ToDecimal(),

                // Equity
                EquityStart = equityHistory.Values.First(),
                EquityEnd = equityHistory.Values.Last(),
                EquityMax = equityHistory.Values.Max(),
                EquityMin = equityHistory.Values.Min(),
                EquityPerformance = (equityHistory.Values.Last() - equityHistory.Values.First()) / equityHistory.Values.First(),

                // Positions
                Positions = positions.Count,
                LongPositions = positions.Count(p => p.Side == PositionSide.LONG),
                ShortPositions = positions.Count(p => p.Side == PositionSide.SHORT),
                WinningPositions = positions.Count(p => p.RealizedPNL > 0),
                LosingPositions = positions.Count(p => p.RealizedPNL < 0),
                AveragePositionLifetime = TimeSpan.FromSeconds(positions.Average(p => p.Lifetime.TotalSeconds)),
                TotalFee = positions.Sum(p => p.Fee).ToDecimal(),
                TotalProfit = positions.Sum(p => p.RealizedPNL - p.Fee).ToDecimal(),
                AverageProfitPerPosition = positions.Average(p => p.RealizedPNL - p.Fee).ToDecimal(),
                AverageFeePerPosition = positions.Average(p => p.Fee).ToDecimal(),
                AveragePositionSize = positions.Average(p => p.Quantity).ToDecimal(),

                // Indicators
                WinRate = positions.Count == 0
                    ? 0
                    : positions.Count(p => p.RealizedPNL > 0) / (decimal)positions.Count,

                WinLossRatio = positions.Count == 0
                    ? 0
                    : positions.Where(p => p.RealizedPNL < 0).Any()
                        ? positions.Where(p => p.RealizedPNL > 0).Average(p => p.RealizedPNL - p.Fee).ToDecimal() / Math.Abs(positions.Where(p => p.RealizedPNL < 0).Average(p => p.RealizedPNL - p.Fee).ToDecimal())
                        : decimal.MaxValue,

                MaximumDrawdown = equityHistory.MaxDrawdown(),
                RecoveryFactor = equityHistory.RecoveryFactor(),
                SharpeRatio = equityHistory.SharpeRatio(),
                KellyCriterion = equityHistory.KellyCriterion(),
                BuyAndHoldRatio = equityHistory.BuyAndHoldRatio(candlesHistory),
            };

            Mareator.Publish(this, new OnBacktestingPerformanceResultEventArgs(result));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Fehler bei Auswertung der Backtesting Performance: {ex.Message}");
            Debug.WriteLine($"Fehler bei Auswertung der Backtesting Performance: {ex.Message}");
            throw;
        }
    }

}
