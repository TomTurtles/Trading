namespace Marek.Trading.Live;
public class LiveTradingOptions : IExchangeOptions, IStrategyOptions
{
    public string Symbol { get;set; }
    public CandleInterval Interval { get;set; }
    public double Lever { get; set; }
    public double FaceValue { get; set; }
    public IDictionary<string, object> Parameters { get; set; }
}
