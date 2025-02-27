namespace Marek.Trading.Live;
public class LiveTradingOptions : IExchangeOptions, IStrategyOptions
{
    public string Symbol { get;set; }
    public CandleInterval Interval { get;set; }
    public Dictionary<string, object> Parameter { get; set; }
    public double Lever { get; set; }
}
