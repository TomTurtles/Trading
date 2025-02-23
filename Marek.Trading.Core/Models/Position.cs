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
    public bool IsLong => Side == PositionSide.LONG;
    public bool IsShort => Side == PositionSide.SHORT;

    public double? StopLossPrice { get; set; }
    public double? TakeProfitPrice { get; set; }

    /// <summary>
    /// Depending on executed Orders
    /// </summary>
    public PositionStatus Status => EntryQuantity > 0 && ExitQuantity == EntryQuantity ? PositionStatus.Closed : PositionStatus.Open;
    public bool IsOpen => Status == PositionStatus.Open;
    public bool IsClosed => Status == PositionStatus.Closed;

    /// <summary>
    /// Depending on executed Orders
    /// </summary>
    public double EntryQuantity => EntryOrders.Sum(o => o.Quantity);
    public double ExitQuantity => ExitOrders.Sum(o => o.Quantity);
    public double Quantity => EntryQuantity;
    public double UnrealizedQuantity => EntryQuantity - ExitQuantity;
    public double RealizedQuantity => ExitQuantity;

    /// <summary>
    /// Depending on Executed Orders, all orders must share the same lever
    /// </summary>
    public double Lever => ExecutedOrders.FirstOrDefault()?.Lever ?? 1d;

    /// <summary>
    /// Last Entry Order Execution Time
    /// </summary>
    public DateTime EntryTime => OrderedEntryOrders.FirstOrDefault().ExecutedTime!.Value;

    /// <summary>
    /// Last Exit Order Execution Time
    /// </summary>
    public DateTime? ExitTime => OrderedExitOrders.LastOrDefault()?.ExecutedTime;

    /// <summary>
    /// Timespan of position to be "alive"
    /// </summary>
    public TimeSpan Lifetime => ExitTime is null ? TimeSpan.FromSeconds(0) : ExitTime.Value - EntryTime;

    /// <summary>
    /// Durchschnittlicher Einstiegspreis basierend auf ausgeführten Entry-Orders.
    /// Falls keine Orders vorhanden sind, wird 0 zurückgegeben.
    /// </summary>
    public double EntryPrice
    {
        get
        {
            var totalQuantity = EntryOrders.Sum(o => o.Quantity);
            if (totalQuantity == 0) return 0; // Absicherung gegen Division durch 0

            var weightedSum = EntryOrders.Sum(o => (o.ExecutedPrice ?? 0) * o.Quantity);
            return weightedSum / totalQuantity;
        }
    }

    public double EntryValue => EntryPrice * EntryQuantity;

    /// <summary>
    /// Durchschnittlicher Exit-Preis basierend auf ausgeführten Exit-Orders.
    /// Falls keine Exit-Orders vorhanden sind, wird null zurückgegeben.
    /// </summary>
    public double? ExitPrice
    {
        get
        {
            var totalQuantity = ExitOrders.Sum(o => o.Quantity);
            if (totalQuantity == 0) return null; // Kein Exit-Preis, falls keine Exit-Orders existieren

            var weightedSum = ExitOrders.Sum(o => (o.ExecutedPrice ?? 0) * o.Quantity);
            return weightedSum / totalQuantity;
        }
    }

    public double ExitValue => (ExitPrice ?? 0) * ExitQuantity;

    /// <summary>
    /// All Orders Sum Fee
    /// </summary>
    public double Fee => ExecutedOrders.Sum(o => o.ExecutedFee ?? 0);

    public double RealizedPNL
    {
        get
        {
            var diff = (ExitPrice ?? EntryPrice) - EntryPrice;
            var sign = Side == PositionSide.LONG ? 1 : -1;
            return sign * diff * Lever * RealizedQuantity;
        }
    }

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
            StopLossPrice = order.StopLossPrice,
            TakeProfitPrice = order.TakeProfitPrice,
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


    public override string ToString()
    {
        var sb = new StringBuilder()
            .AppendLine($"Position: {Id}")
            .AppendLine($"{Side}, Entry: {EntryPrice} x {EntryQuantity} at {EntryTime}");

        if (ExitPrice is not null)
        {
            sb.AppendLine($"Exit: ({ExitPrice}) at {ExitTime}");
        }

        sb.AppendLine($"PNL: {RealizedPNL} (leverage: {Lever})")
          .AppendLine($"Fee: {Fee}")
          .AppendLine($"Value: {this.GetValue()}");

        return sb.ToString();
    }
}
