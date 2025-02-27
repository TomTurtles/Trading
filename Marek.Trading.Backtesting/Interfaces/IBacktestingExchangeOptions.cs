namespace Marek.Trading.Core;

public interface IBacktestingExchangeOptions : IExchangeOptions
{
    double InitialCash { get; set; }
    double MarginCallLevel { get; set; }
}
