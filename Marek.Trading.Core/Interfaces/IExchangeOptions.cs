namespace Marek.Trading.Core;

public interface IExchangeOptions
{
    string Symbol { get; set; }
    double InitialCash { get; set; }
    double MarginCallLevel { get; set; }
    CandleInterval Interval { get; set; }
}
