
namespace Trading.Backtesting;

public class BacktestEngine : IBacktestEngine
{
    public BacktestExchange Exchange { get; } = new();
    public IDataFeed DataFeed { get; }
    public IStrategy Strategy { get; }

    public BacktestEngine(IDataFeed dataFeed, IStrategy strategy)
    {
        DataFeed = dataFeed;
        Strategy = strategy;
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
        Console.WriteLine($"Trading Simulation on {runningCandles.Count} Candles with initial {options.InitialCash:0.00 $}.");

        Exchange.Initialize(runningCandles, options.InitialCash);

        await Exchange.ConnectAsync();

        var statesTasks = runningCandles.Select(async candle => await RunOnCandleAsync(candle, options.Symbol));
        var states = await Task.WhenAll(statesTasks);

        return BacktestEnginePerformanceResult.FromStates(states);
    }

    private async Task<BacktestEngineCandleState> RunOnCandleAsync(Candle candle, string symbol)
    {
        try
        {
            // Exchange reacts on new candle first
            Exchange.HandleNewCandle(candle, symbol);

            // strategy
            var decision = await Strategy.ExecuteAsync(candle);

            // Exchange reacts on strategy decision
            var exchangeState = await Exchange.HandleNewDecisionAsync(candle, decision, symbol);

            return BacktestEngineCandleState.Create(candle, decision, exchangeState);
        }
        catch (Exception ex)
        {
            return BacktestEngineCandleState.FromException(candle, ex);
        }
    }
}
