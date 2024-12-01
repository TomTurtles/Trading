namespace Trading.Backtesting;

public class BacktestEngine : IBacktestEngine
{
    public BacktestExchange Exchange { get; } 
    public IDataFeed DataFeed { get; }
    public IStrategy Strategy { get; }
    public ILogger? Logger { get; set; }

    public BacktestEngine(IDataFeed dataFeed, IStrategy strategy, BacktestExchange exchange, ILogger? logger)
    {
        DataFeed = dataFeed;
        Strategy = strategy;
        Exchange = exchange;
        Logger = logger;
    }

    public async Task<BacktestEnginePerformanceResult> RunAsync(Action<BacktestOptions> configureOptions)
    {
        var options = new BacktestOptions();
        configureOptions(options);

        // Start Datum des Performance Tests geradebiegen
        var startAt = options.StartAt ?? new DateTime(DateTime.Now.AddYears(-2).Year, 1, 1);

        // End Datum des Performance Tests geradebiegen
        var endAt = options.EndAt is null || options.EndAt > DateTime.Now ? DateTime.Now : options.EndAt!.Value;

        // DataFeed holt den Candle_Datensatz gegen den getestet wird
        // Dabei wird noch etwas "warm-up" Zeit hinzugesteuert
        var warmupSeconds = options.WarmUpCandles * (int)options.Interval;
        var dataFeedCandles = (await DataFeed.GetCandlesAsync(startAt.AddSeconds(-1 * warmupSeconds), endAt)).ToList();

        // Über die Running Candles soll iteriert werden, sie beinhalten NICHT die WarmUp Candles
        var runningCandles = dataFeedCandles
            .Where(candle => candle.Timestamp >= startAt)
            .Where(candle => candle.Timestamp <= endAt)
            .ToList();

        // Exchange erhält das Initiale Cash und verwaltet es (Einzahlung)
        Logger?.LogInformation($"Trading Simulation on {runningCandles.Count} Candles with initial {options.InitialCash:0.00 $}.");

        Exchange.Logger = Logger;
        Exchange.Initialize(dataFeedCandles, options.InitialCash, options.Symbol);

        await Exchange.ConnectAsync();

        Stopwatch sw = Stopwatch.StartNew();

        // with first initial state
        List<BacktestEngineCandleState> states = new() 
        { 
            new BacktestEngineCandleState() 
            { 
                Candle = dataFeedCandles.Last(c => c.Timestamp < startAt ),  
                ExchangeState = ExchangeState.Create(options.InitialCash, [], null, null, 0, options.InitialCash),
                ClosedPositions = [],
                Decision = StrategyDecisionType.Start,
            } 
        };

        for (int i = 0; i < runningCandles.Count; i++)
        {
            Candle? candle = runningCandles[i];
            var state = await RunOnCandleAsync(candle, options.Symbol);
            states.Add(state);
            LogProcess(i, runningCandles.Count);
        }
        LogProcess(runningCandles.Count, runningCandles.Count);

        sw.Stop();

        return BacktestEnginePerformanceResult.FromStates(Strategy.Name, options, states, sw);
    }

    private void LogProcess(int i, int count)
    {
        if (i % (count / 25) == 0 || i == count)
        {
            Logger?.LogInformation($"Candles processed: {(double)i / count:0.00%}");
        }
    }

    private async Task<BacktestEngineCandleState> RunOnCandleAsync(Candle candle, string symbol)
    {
        try
        {
            // Exchange reacts on new candle first
            var closedPositions = await Exchange.HandleNewCandleAsync(candle, symbol);

            // strategy
            var decision = await Strategy.ExecuteAsync(candle);
            Logger?.LogDebug($"{candle}: {decision}");

            // Exchange reacts on strategy decision
            var exchangeState = await Exchange.HandleNewDecisionAsync(decision);

            return BacktestEngineCandleState.Create(candle, decision, exchangeState, closedPositions);
        }
        catch (Exception ex)
        {
            return BacktestEngineCandleState.FromException(candle, ex);
        }
    }
}
