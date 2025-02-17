namespace Marek.Trading.Backtesting;

public class BacktestingDataFeed(
    IDataFeedExchange exchange, 
    IOptions<BacktestingOptions> options, 
    ILogger<BacktestingDataFeed> logger) 
    : IBacktestingDataFeed
{
    public DateTime EndAt => new DateTime((options.Value.EndAt is null || options.Value.EndAt > DateTime.Now ? DateTime.Now : options.Value.EndAt!.Value).Ticks);
    public DateTime StartAt => new DateTime((options.Value.StartAt ?? EndAt.AddSeconds((-1) * (int) options.Value.Interval * 100)).Ticks);

    public IDataFeedExchange Exchange { get; } = exchange;
    public ILogger<BacktestingDataFeed> Logger { get; } = logger;

    public async Task<List<Candle>> GetCandlesAsync(CancellationToken cancellationToken = default)
    {
        var symbol = options.Value.Symbol;
        var limit = int.MaxValue;
        var interval = options.Value.Interval.ToEnum<CandleInterval>();

        // Es wird noch etwas "warm-up" Zeit hinzugesteuert
        var startAt = StartAt.AddSeconds((-1) * (int)options.Value.Interval * 20);
        var endAt = EndAt;

        var result = new List<Candle>();
        do
        {
            var preCount = result.Count;

            var candles = await Exchange.GetCandlesAsync(symbol, interval, limit, startAt, endAt);
            
            if (!candles.Any())
            {
                Logger.LogWarning($"No candles received");
                break;
            }

            result.AddRange(candles);

            result = result
                .OrderBy(candle => candle.Timestamp)
                .Distinct()
                .ToList();

            endAt = result.First().Timestamp.AddSeconds((-1) * (int)options.Value.Interval);

            Logger.LogInformation($"fetched backtesting candle data '{GetLinearDateScale(startAt, EndAt, endAt):0.0%}' ({result.Count})");

            // Abbruchbedingung: falls sich nix verändert hat
            if (preCount >= result.Count) break;

        } while (endAt > startAt);

        return result;
    }

    private double GetLinearDateScale(DateTime startAt, DateTime endAt, DateTime variable)
    {
        double totalDuration = (endAt - startAt).TotalSeconds;
        double elapsedDuration = (variable - startAt).TotalSeconds;
        return 1 - (elapsedDuration / totalDuration);
    }
}
