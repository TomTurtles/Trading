namespace Marek.Trading.Core;

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
    public PositionStatus Status => EntryQuantity > 0 && Quantity <= 0 ? PositionStatus.Closed : PositionStatus.Open;
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
    public double EntryPrice => EntryOrders.Average(o => o.ExecutedPrice!.Value);
    public double EntryValue => EntryPrice * EntryQuantity;

    /// <summary>
    /// All Exit Orders Avg Execution Price
    /// </summary>
    public double? ExitPrice => ExitOrders.Any() ? ExitOrders.Average(o => o.ExecutedPrice!.Value) : null;
    public double ExitValue => (ExitPrice ?? 0) * ExitQuantity;

    /// <summary>
    /// All Orders Sum Fee
    /// </summary>
    public double Fee => ExecutedOrders.Sum(o => o.ExecutedFee!.Value);


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
        if (ExecutedOrders.Any() && order.Lever != Lever)
        {
            Debug.WriteLine($"order with lever '{Lever}' already added. New lever '{order.Lever}' not allowed while adding next order to position. Lever will be adjusted.");
            order.Lever = Lever;
        }
        if (IsClosed) throw new InvalidOperationException($"cannot add order on a closed position");

        ExecutedOrders.Add(order);
    }

    public double GetValue(double? price = null, double? quantity = null)
    {
        price ??= ExitPrice ?? EntryPrice;
        quantity ??= Quantity;

        // cancel
        if (price == null) return 0;
        if (quantity == null) return 0;
        if (price < 0) return 0;
        if (quantity < 0) return 0;

        return EntryValue + GetPNL(price, quantity);
    }

    public double GetPNL(double? price = null, double? quantity = null)
    {
        price ??= ExitPrice ?? EntryPrice;
        quantity ??= Quantity;
        return (Side == PositionSide.LONG ? price.Value - EntryPrice : EntryPrice - price.Value) * Lever * quantity.Value;
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
            sb.AppendLine($"PNL: {GetPNL()} (leverage: {Lever})");
            sb.AppendLine($"Fee: {Fee}");
        }

        return sb.ToString();
    }
}
