
namespace Marek.Trading.Backtesting;

public class BacktestingEngine(
    IBacktestingDataFeed dataFeed, 
    IBacktestingStrategy strategy,
    IBacktestingExchange exchange,
    IBacktestingPerformanceTracker performanceTracker,
    ILogger<BacktestingEngine> logger) : IBacktestingEngine
{
    public IBacktestingDataFeed DataFeed { get; } = dataFeed;
    public IBacktestingExchange Exchange { get; } = exchange;
    public IBacktestingStrategy Strategy { get; } = strategy;
    public IBacktestingPerformanceTracker PerformanceTracker { get; } = performanceTracker;
    public ILogger<BacktestingEngine> Logger { get; set; } = logger;

    public bool IsRunning { get; private set; } = false;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        IsRunning = true;

        PerformanceTracker.Start();

        // DataFeed triggern
        var candles = await DataFeed.GetCandlesAsync(cancellationToken);

        // Exchange Candles übergeben
        Exchange.SetCandles(candles);

        foreach (var candle in candles)
        {
            // Exchange über die Aktuelle Candle informieren
            Exchange.SetCandle(candle);

            // Margin Call prüfen und ggf. Abbruch
            if (await Exchange.HasMarginCallAsync(cancellationToken))
            {
                Logger.LogWarning($"MARGIN CALL LEVEL REACHED AT CANDLE {candle}. Backtesting will be cancelled.");
                return;
            }

            // Exchange Informationen mit der neuen Candle aktualisieren
            await Exchange.RunAsync(cancellationToken);

            // Strategy die aktuelle Candle behandeln lassen
            await Strategy.RunAsync(candle, cancellationToken);
        }

        // Letzte offene Position schließen
        await CloseLastOpenPositionAsync(cancellationToken);

        PerformanceTracker.Finish();

        IsRunning = false;
    }

    private async Task CloseLastOpenPositionAsync(CancellationToken cancellationToken)
    {
        await Exchange.ClosePositionAsync("", null, cancellationToken);
        await Exchange.NotifyAccountReportAsync(cancellationToken);
    }
}
