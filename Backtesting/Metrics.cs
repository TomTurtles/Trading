namespace Trading;
public class Metrics
{
    public decimal TotalProfit { get; private set; }
    public decimal TotalLoss { get; private set; }
    public decimal NetProfit { get; private set; }
    public decimal ProfitFactor { get; private set; }
    public decimal MaxDrawdown { get; private set; }
    // Weitere Kennzahlen

    public void Calculate(List<Order> orders, decimal finalEquity)
    {
        var closedTrades = orders.Where(o => o.Status == Order.OrderStatus.Filled && o.ExecutedPrice.HasValue).ToList();

        foreach (var order in closedTrades)
        {
            decimal profit = (order.Side == Order.OrderSide.Buy ? -1 : 1) * order.Quantity * order.ExecutedPrice.Value;
            if (profit > 0)
                TotalProfit += profit;
            else
                TotalLoss += profit;
        }

        NetProfit = TotalProfit + TotalLoss;
        ProfitFactor = TotalProfit / Math.Abs(TotalLoss);
        // MaxDrawdown und weitere Kennzahlen berechnen
    }
}
