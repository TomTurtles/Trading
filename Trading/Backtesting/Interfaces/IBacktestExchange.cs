namespace Trading.Backtesting;
public interface IBacktestExchange : IExchange
{
    IExchangeCash Cash { get; }
    IExchangeOrders Orders { get; }
    IExchangePositions Positions { get; }
}
