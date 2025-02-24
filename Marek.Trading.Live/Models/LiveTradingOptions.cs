namespace Marek.Trading.Live;
public class LiveTradingOptions
{
    public string Symbol { get;set; }
    public string Interval { get;set; }
    public Dictionary<string, object> Parameter { get; set; } 
}
