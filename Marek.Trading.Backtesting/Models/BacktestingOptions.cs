namespace Marek.Trading.Backtesting;

public class BacktestingOptions : IStrategyOptions, IBacktestingExchangeOptions, IDataFeedOptions
{
    public string Symbol { get; set; }
    public DateTime? StartAt { get; set; } = DateTime.Now.AddMonths(-1);
    public DateTime? EndAt { get; set; } = DateTime.Now;
    public CandleInterval Interval { get; set; } = CandleInterval.Day_1;
    public List<CandleInterval> LoadingIntervals { get; set; } = new();
    public double InitialCash { get; set; } = 10000;
    public double MarginCallLevel { get; set; } = 100;
    public int WarmUpCandles { get; set; } = 30;
    public double Lever { get; set; } = 1d;
    public IDictionary<string, object> Parameters { get; set; }
}
