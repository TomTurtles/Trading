namespace Marek.Trading.Core;

public interface IExchangeOptions
{
    string Symbol { get; set; }
    CandleInterval Interval { get; set; }
}
