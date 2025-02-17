namespace Marek.Trading.Core;
public class PlaceBacktestingOrderCommand(DateTime Timestamp, Order order, double? marketPrice = null, double? feeRate = null) : ICommand
{
    public DateTime Timestamp { get; } = Timestamp;
    public Order Order { get; } = order;
    public double? MarketPrice { get; } = marketPrice;
    public double FeeRate { get; } = feeRate ?? 0d;

    public bool IsMarketOrder => MarketPrice is not null;
}
