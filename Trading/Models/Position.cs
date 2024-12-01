using System.Text;

namespace Trading;

public class Position
{
    public Position()
    {
        Id = Guid.NewGuid().ToString();    
    }

    public string Id { get; } 
    public string Symbol { get; init; }

    [ConvertStringEnum]
    public PositionSide Side { get; init; }

    public double? StopPrice { get; set; }
    public double? TakePrice { get; set; }

    /// <summary>
    /// Depending on executed Orders
    /// </summary>
    public PositionStatus Status => Quantity <= 0 ? PositionStatus.Closed : PositionStatus.Open;
    public bool IsOpen => Status == PositionStatus.Open;
    public bool IsClosed => Status == PositionStatus.Closed;

    /// <summary>
    /// Depending on executed Orders
    /// </summary>
    public double EntryQuantity => EntryOrders.Sum(o => o.Quantity);
    public double ExitQuantity => ExitOrders.Sum(o => o.Quantity);
    public double Quantity => EntryQuantity - ExitQuantity;

    /// <summary>
    /// Depending on Executed Orders, all orders must share the same lever
    /// </summary>
    public double Lever => ExecutedOrders.First().Lever;

    /// <summary>
    /// Last Entry Order Execution Time
    /// </summary>
    public DateTime EntryTime => OrderedEntryOrders.Last().ExecutedTime!.Value;

    /// <summary>
    /// Last Exit Order Execution Time
    /// </summary>
    public DateTime? ExitTime => OrderedExitOrders.LastOrDefault()?.ExecutedTime;

    /// <summary>
    /// All Entry Orders Avg Execution Price
    /// </summary>
    public double EntryPrice => EntryOrders.Sum(o => o.ExecutedPrice!.Value * o.Quantity) / EntryOrders.Sum(o => o.Quantity);

    /// <summary>
    /// All Exit Orders Avg Execution Price
    /// </summary>
    public double? ExitPrice => ExitOrders.Any() ? ExitOrders.Sum(o => o.ExecutedPrice!.Value * o.Quantity) / ExitOrders.Sum(o => o.Quantity) : null;

    /// <summary>
    /// All Orders Sum Fee
    /// </summary>
    public double Fee => ExecutedOrders.Sum(o => o.ExecutedFee!.Value);

    /// <summary>
    /// PNL
    /// </summary>
    public double? PNL => ExitOrders.Any() ? (Side == PositionSide.Long ? ExitPrice - EntryPrice : EntryPrice - ExitPrice) * ExitQuantity * Lever : null;


    #region Orders
    private List<Order> ExecutedOrders { get; } = [];
    public IReadOnlyList<Order> EntryOrders => ExecutedOrders.Where(o => o.IsSameSideAs(this)).ToList();
    public IReadOnlyList<Order> ExitOrders => ExecutedOrders.Where(o => !o.IsSameSideAs(this)).ToList();
    public IReadOnlyList<Order> OrderedEntryOrders => EntryOrders.OrderBy(o => o.ExecutedTime).ToList();
    public IReadOnlyList<Order> OrderedExitOrders => ExitOrders.OrderBy(o => o.ExecutedTime).ToList();
    #endregion Orders



    public static Position CreateFromOrder(Order order)
    {
        if (order.Quantity <= 0) throw new InvalidOperationException($"invalid quantity: '{order.Quantity}'");

        var position = new Position()
        {
            Symbol = order.Symbol,
            Side = order.ToPositionSide(),
            StopPrice = order.StopPrice,
            TakePrice = order.TakePrice,
        };

        if (order.Status != OrderStatus.Filled) order.Status = OrderStatus.Filled;

        position.AddExecutedOrder(order);

        return position;
    }

    public void AddExecutedOrder(Order order)
    {
        if (order.Status != OrderStatus.Filled) throw new InvalidOperationException($"status: '{order.Status}' invalid");
        if (order.Symbol != Symbol) throw new InvalidOperationException($"symbol: '{order.Symbol}' invalid");
        if (order.ExecutedTime is null) throw new InvalidOperationException($"{nameof(order.ExecutedTime)} is null");
        if (order.ExecutedPrice is null) throw new InvalidOperationException($"{nameof(order.ExecutedPrice)} is null");
        if (order.ExecutedFee is null) throw new InvalidOperationException($"{nameof(order.ExecutedFee)} is null");
        if (order.Quantity <= 0) throw new InvalidOperationException($"invalid quantity: '{order.Quantity}'");
        if (ExecutedOrders.Any() && order.Lever != ExecutedOrders.First().Lever) throw new InvalidOperationException($"order with lever '{ExecutedOrders.First().Lever}' already added. New lever '{order.Lever}' not allowed.");

        ExecutedOrders.Add(order);
    }

    public override string ToString()
    {
        var sb = new StringBuilder()
            .AppendLine($"")
            .AppendLine($"")
            .AppendLine($"{Id}")
            .AppendLine($"{Side}, Entry: {EntryPrice} x {EntryQuantity} at {EntryTime}");

        if (ExitPrice is not null)
        {
            sb.AppendLine($"Exit: ({ExitPrice}) at {ExitTime}");
            sb.AppendLine($"PNL: {PNL} (leverage: {Lever})");
            sb.AppendLine($"Fee: {Fee}");
        }

        return sb.ToString();
    }
}
