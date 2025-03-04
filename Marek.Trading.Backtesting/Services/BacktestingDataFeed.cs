namespace Marek.Trading.Backtesting;

public class BacktestingDataFeed(
    IDataFeedExchange exchange,
    IOptions<BacktestingOptions> options,
    ILogger<BacktestingDataFeed> logger)
    : IBacktestingDataFeed
{
    public DateTime EndAt => new DateTime((options.Value.EndAt is null || options.Value.EndAt > DateTime.Now ? DateTime.Now : options.Value.EndAt!.Value).Ticks);
    public DateTime StartAt => new DateTime((options.Value.StartAt ?? EndAt.AddSeconds((-1) * (int)options.Value.Interval * 100)).Ticks);

    public IDataFeedExchange Exchange { get; } = exchange;
    public ILogger<BacktestingDataFeed> Logger { get; } = logger;

    public async Task<DataFeedLoadCandlesResult> LoadCandlesAsync(CancellationToken cancellationToken = default)
    {
        var candles = await GetCandlesAsync(options.Value.Interval);

        var dict = new Dictionary<CandleInterval, List<Candle>>();

        foreach (var interval in options.Value.LoadingIntervals)
        {
            dict.Add(interval, await GetCandlesAsync(interval));
        }

        return new DataFeedLoadCandlesResult()
        {
            Candles = candles,
            AdditionalCandles = dict
        };
    }


    private async Task<List<Candle>> GetCandlesAsync(CandleInterval interval, CancellationToken cancellationToken = default)
    {
        var symbol = options.Value.Symbol;
        var limit = int.MaxValue;

        // Es wird noch etwas "warm-up" Zeit hinzugesteuert
        var startAt = StartAt.AddSeconds((-1) * (int)interval * options.Value.WarmUpCandles);
        var endAt = EndAt;

        var result = new Dictionary<DateTime, Candle>();
        var exceptionCount = 0;
        do
        {
            try
            {
                var candles = await Exchange.GetCandlesAsync(symbol, interval, limit, startAt, endAt);

                if (!candles.Any())
                {
                    Logger.LogWarning($"No candles received");
                    break;
                }

                var addedNewCandles = candles.Select(c => result.TryAdd(c.Timestamp, c)).ToList();

                var hasAddedNewCandles = addedNewCandles.Any(c => c);

                endAt = result.Values.OrderBy(c => c.Timestamp).First().Timestamp.AddSeconds((-1) * (int)interval);

                Logger.LogInformation($"[{interval}] fetched backtesting candle data '{GetLinearDateScale(startAt, EndAt, endAt):0.0%}' ({result.Count})");
                Logger.LogDebug($"[{interval}] fetched backtesting candle data '{startAt:yyyy-MM-dd HH:mm:ss}' - '{endAt:yyyy-MM-dd HH:mm:ss}': {hasAddedNewCandles}");

                // Abbruchbedingung: falls sich nix verändert hat
                if (!hasAddedNewCandles) throw new InvalidOperationException("no new data received");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                exceptionCount++;
            }

        } while (endAt > startAt && exceptionCount < 5);

        return result.Values.OrderBy(c => c.Timestamp).ToList();
    }

    private double GetLinearDateScale(DateTime startAt, DateTime endAt, DateTime variable)
    {
        double totalDuration = (endAt - startAt).TotalSeconds;
        double elapsedDuration = (variable - startAt).TotalSeconds;
        return 1 - (elapsedDuration / totalDuration);
    }
}
