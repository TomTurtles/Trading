namespace Marek.Trading.Core;

public interface IStrategyOptions
{
    public string Symbol { get; set; }
    public CandleInterval Interval { get; set; }
    public double Lever { get; set; }
}
