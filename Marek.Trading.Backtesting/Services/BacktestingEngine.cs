namespace Marek.Trading.Backtesting;

public class BacktestingEngine : IBacktestingEngine
{
    public IMareatorEventDispatcher EventDispatcher { get; }
    public IBacktestingDataFeed DataFeed { get; }
    public IBacktestingExchange Exchange { get; }
    public IStrategy Strategy { get; }
    public IBacktestingPerformanceTracker PerformanceTracker { get; }
    public IOptions<BacktestingOptions> Options { get; }
    public ILogger<BacktestingEngine> Logger { get; set; }

    public bool IsRunning => PerformanceTracker.IsRunning;

    public event EventHandler<OnBacktestingPerformanceResultEventArgs> OnBacktestingFinished;
    public BacktestingEngine(
        IMareatorEventDispatcher eventDispatcher,
        IBacktestingDataFeed dataFeed,
        IStrategy strategy,
        IBacktestingExchange exchange,
        IBacktestingPerformanceTracker performanceTracker,
        IOptions<BacktestingOptions> options,
        ILogger<BacktestingEngine> logger)
    {
        EventDispatcher = eventDispatcher;
        DataFeed = dataFeed;
        Exchange = exchange;
        Strategy = strategy;
        PerformanceTracker = performanceTracker;
        Options = options;
        Logger = logger;

        EventDispatcher.Subscribe<OnBacktestingPerformanceResultEventArgs>(HandleBacktestingPerformanceResult);
    }


    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        PerformanceTracker.Start();

        // DataFeed triggern
        var result = await DataFeed.LoadCandlesAsync(cancellationToken);

        // Exchange Candles übergeben
        Exchange.SetCandles(result);

        foreach (var candle in result.Candles.Where(c => c.Timestamp >= Options.Value.StartAt))
        {
            // Exchange über die Aktuelle Candle informieren
            Exchange.SetCandle(candle);

            // Margin Call prüfen und ggf. Abbruch
            if (await Exchange.HasMarginCallAsync(cancellationToken))
            {
                Logger.LogWarning($"MARGIN CALL LEVEL REACHED AT CANDLE {candle}. Backtesting will be cancelled.");
                PerformanceTracker.Finish();
                return;
            }

            // Exchange Informationen mit der neuen Candle aktualisieren
            await Exchange.RunAsync(cancellationToken);

            // Strategy die aktuelle Candle behandeln lassen
            await Strategy.RunAsync(candle, cancellationToken);

            // Exchange Account Report
            await Exchange.NotifyAccountReportAsync(cancellationToken);
        }

        // Letzte offene Position schließen
        await CloseLastOpenPositionAsync(cancellationToken);

        PerformanceTracker.Finish();
    }

    private async Task CloseLastOpenPositionAsync(CancellationToken cancellationToken)
    {
        var openPosition = await Exchange.GetOpenPositionAsync();
        if (openPosition is not null && openPosition.IsOpen())
        {
            await Exchange.ClosePositionAsync(openPosition.Id, null, cancellationToken);
        }
        await Exchange.NotifyAccountReportAsync(cancellationToken);
    }

    private void HandleBacktestingPerformanceResult(object sender, OnBacktestingPerformanceResultEventArgs e)
    {
        var prefixGroups = e
            .ToKeyValuePairs()
            .GroupBy(kvp =>
            {
                var arr = kvp.Key.Split('.');
                return (arr.Length <= 1) ? "Global" : arr[0];
            });

        var sb = new StringBuilder()
            .AppendLine()
            .AppendLine()
            .AppendLine($"----------------------------------------")
            .AppendLine($"PERFORMANCE RESULT")
            .AppendLine($"----------------------------------------");

        foreach (var group in prefixGroups)
        {
            var prefix = group.Key;

            var values = group.ToDictionary(g => g.Key.Split('.').Last(), g => g.Value);

            sb
            .AppendLine()
            .AppendLine()
            .AppendLine($"-------------------")
            .AppendLine($"{prefix.ToUpperInvariant()}")
            .AppendLine()
            .AppendJoin('\n', values.Select(kvp => $"{kvp.Key}: {Format(kvp.Value)}"));

        };

        var resultString = sb.ToString();

        Logger.LogInformation(resultString);
        Debug.WriteLine(resultString);

        OnBacktestingFinished?.Invoke(this, e);
    }

    private object Format(object value)
    {
        if (value is double doubleValue)
        {
            return Math.Round(doubleValue, 2);
        }
        else
        {
            return value;   
        }
    }
}
