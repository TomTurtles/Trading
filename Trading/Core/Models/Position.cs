namespace Trading;

public class Position
{
    [JsonConstructor]
    public Position() { }

    public Position(Order order)
    {
        Symbol = order.Symbol;
        Side = order.ToPositionSide();
        AddExecutedOrder(order);
    }

    public string ID { get; } = Guid.NewGuid().ToString();
    public string Symbol { get; }

    [ConvertStringEnum]
    public PositionSide Side { get; }
    public decimal? StopPrice { get; set; }
    public decimal? TakePrice { get; set; }

    /// <summary>
    /// Depending on executed Orders
    /// </summary>
    public PositionStatus Status => Quantity <= 0 ? PositionStatus.Closed : PositionStatus.Open;
    public bool IsOpen => Status == PositionStatus.Open;
    public bool IsClosed => Status == PositionStatus.Closed;

    /// <summary>
    /// Depending on executed Orders
    /// </summary>
    public decimal Quantity => EntryOrders.Sum(o => o.Quantity) - ExitOrders.Sum(o => o.Quantity);

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
    public decimal EntryPrice => EntryOrders.Sum(o => o.ExecutedPrice!.Value * o.Quantity) / EntryOrders.Sum(o => o.Quantity);

    /// <summary>
    /// All Exit Orders Avg Execution Price
    /// </summary>
    public decimal? ExitPrice => ExitOrders.Any() ? ExitOrders.Sum(o => o.ExecutedPrice!.Value * o.Quantity) / ExitOrders.Sum(o => o.Quantity) : null;

    /// <summary>
    /// All Orders Sum Fee
    /// </summary>
    public decimal Fee => ExecutedOrders.Sum(o => o.ExecutedFee!.Value);


    #region Orders
    public void AddExecutedOrder(Order order)
    {
        if (!order.IsExecuted) throw new InvalidOperationException("order must be set as filled");
        if (order.ExecutedPrice is null) throw new InvalidOperationException("executed order has no executed price");
        if (order.ExecutedTime is null) throw new InvalidOperationException("executed order has no executed time");
        if (order.ExecutedFee is null) throw new InvalidOperationException("executed order has no executed fee");
        ExecutedOrders.Add(order);
    }

    private List<Order> ExecutedOrders { get; } = [];
    public IEnumerable<Order> EntryOrders => ExecutedOrders.Where(o => o.IsSameSideAs(this));
    public IEnumerable<Order> ExitOrders => ExecutedOrders.Where(o => !o.IsSameSideAs(this));
    public IOrderedEnumerable<Order> OrderedExecutedOrders => ExecutedOrders.OrderBy(o => o.ExecutedTime);
    public IOrderedEnumerable<Order> OrderedEntryOrders => EntryOrders.OrderBy(o => o.ExecutedTime);
    public IOrderedEnumerable<Order> OrderedExitOrders => ExitOrders.OrderBy(o => o.ExecutedTime);
    #endregion Orders


    #region Metrics
    public decimal? Realisation => ExitOrders.Sum(o => o.ExecutedPrice!.Value * o.Quantity) - EntryOrders.Sum(o => o.ExecutedPrice!.Value * o.Quantity);
    public decimal? RealisationAfterFee => Realisation is null ? null : Realisation - Fee;
    public bool Win => Realisation is not null && Realisation > 0;
    #endregion Metrics
}
