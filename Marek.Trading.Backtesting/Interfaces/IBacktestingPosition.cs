namespace Marek.Trading.Backtesting;

public interface IBacktestingPosition : IPosition
{
    IReadOnlyList<Order> EntryOrders { get; }
    IReadOnlyList<Order> ExitOrders { get; }
    IReadOnlyList<Order> OrderedEntryOrders { get; }
    IReadOnlyList<Order> OrderedExitOrders { get; }
    void AddExecutedOrder(Order order);
}
