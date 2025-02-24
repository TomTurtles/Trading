namespace Marek.Trading.Backtesting;

public interface IBacktestingPosition : IPosition
{
    IReadOnlyList<IOrder> EntryOrders { get; }
    IReadOnlyList<IOrder> ExitOrders { get; }
    IReadOnlyList<IOrder> OrderedEntryOrders { get; }
    IReadOnlyList<IOrder> OrderedExitOrders { get; }
    void AddExecutedOrder(IOrder order);
}
