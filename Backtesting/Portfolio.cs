namespace Trading;

public class Portfolio
{
    public decimal Cash { get; private set; }
    public List<Position> Positions { get; private set; }
    public List<Order> Orders { get; private set; }
    public decimal Equity { get; private set; } // Cash + Wert der offenen Positionen

    public Portfolio(decimal initialCash)
    {
        Cash = initialCash;
        Positions = new List<Position>();
        Orders = new List<Order>();
        Equity = initialCash;
    }

    public void UpdatePortfolio(Order order)
    {
        decimal orderCost = order.ExecutedPrice.Value * order.Quantity;
        if (order.Side == Order.OrderSide.Buy)
        {
            Cash -= orderCost;
            AddPosition(order);
        }
        else if (order.Side == Order.OrderSide.Sell)
        {
            Cash += orderCost;
            RemovePosition(order);
        }

        Orders.Add(order);
    }

    private void AddPosition(Order order)
    {
        var position = Positions.FirstOrDefault(p => p.Symbol == order.Symbol && p.Side == Position.PositionSide.Long);
        if (position == null)
        {
            position = new Position
            {
                Symbol = order.Symbol,
                Side = Position.PositionSide.Long,
                Quantity = order.Quantity,
                EntryPrice = order.ExecutedPrice.Value,
                EntryTime = order.ExecutedTime.Value
            };
            Positions.Add(position);
        }
        else
        {
            decimal totalQuantity = position.Quantity + order.Quantity;
            position.EntryPrice = ((position.EntryPrice * position.Quantity) + (order.ExecutedPrice.Value * order.Quantity)) / totalQuantity;
            position.Quantity = totalQuantity;
        }
    }

    private void RemovePosition(Order order)
    {
        var position = Positions.FirstOrDefault(p => p.Symbol == order.Symbol && p.Side == Position.PositionSide.Long);
        if (position != null)
        {
            position.Quantity -= order.Quantity;
            if (position.Quantity <= 0)
            {
                Positions.Remove(position);
            }
        }
    }

    public void UpdateMarketValue(Candle candle)
    {
        //decimal positionsValue = Positions.Sum(p => p.Quantity * candle.Close);
        //Equity = Cash + positionsValue;
    }

    public List<Order> GetPendingOrders()
    {
        return Orders.Where(o => o.Status == Order.OrderStatus.Pending).ToList();
    }

    public Metrics CalculatePerformanceMetrics()
    {
        var metrics = new Metrics();
        metrics.Calculate(Orders, Equity);
        return metrics;
    }
}
