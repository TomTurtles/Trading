namespace Marek.Trading.Core;

public class BacktestingAccountState
{
    public double Equity { get; set; }
    public double MarketPrice { get; set; }
    public double Cash { get; set; }
    public int PendingOrders { get; set; }
    public int OpenPositions { get; set; }
    public Position? ClosedPosition { get; set; }

    public static BacktestingAccountState Create(double cash, IEnumerable<Order> pendingOrders, Position? openPosition, Position? closedPosition, double marketPrice, double equity)
    {
        //var equity = cash + openPositions.Sum(p => p.GetValue(marketPrice));

        return new()
        {
            Cash = cash,
            PendingOrders = pendingOrders.Count(),
            OpenPositions = openPosition is null ? 0 : 1,
            ClosedPosition = closedPosition,
            MarketPrice = marketPrice,
            Equity = equity,
        };
    }
}
