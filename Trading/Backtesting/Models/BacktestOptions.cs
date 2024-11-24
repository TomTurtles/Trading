namespace Trading.Backtesting;

public class BacktestOptions
{
    public decimal InitialCash { get; set; } = 10000;
    public string Symbol { get; set; } = "BTC_USDT_PERP";
    public CandleInterval Interval { get; set; } = CandleInterval.Day_1;
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int WarmUpCandles { get; set; } = 200;
}
