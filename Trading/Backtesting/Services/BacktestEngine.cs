namespace Trading.Backtesting;

public class BacktestEngine : IBacktestEngine
{
    public IBacktestExchange Exchange { get; }
    public IEventSystem EventSystem { get; }
    public IDataFeed DataFeed { get; }

    public BacktestEngine(IDataFeed dataFeed, IBacktestExchange exchange, IEventSystem eventSystem)
    {
        DataFeed = dataFeed;
        Exchange = exchange;
        EventSystem = eventSystem;
    }

    public async Task<PerformanceResult> RunAsync(Action<BacktestOptions> configureOptions)
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
        Console.WriteLine($"Trading Simulation on {runningCandles.Count} Candles with initial {options.InitialCash:0.00 $}.");

        var tracker = new PerformanceTracker(EventSystem, Exchange, options.InitialCash, runningCandles);

        // Beim Connecten wird die IBacktestExchange alle Candles nach und nach hinausfeuern
        await Exchange.ConnectAsync(options.InitialCash, runningCandles);


        // warte auf Event Verarbeitung
        await tracker.ProcessAsync();


        return new PerformanceResult(tracker);
    }
}
