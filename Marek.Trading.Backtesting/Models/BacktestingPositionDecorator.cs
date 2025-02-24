namespace Marek.Trading.Backtesting;

public class BacktestingPositionDecorator(IPosition position) : IBacktestingPosition
{
    private readonly IPosition _position = position;


    public string Id { get; } = Guid.NewGuid().ToString();
    public string Symbol { get; set; } = position.Symbol;
    public PositionSide Side { get; set; } = position.Side;
    public double? StopLossPrice { get; set; } = position.StopLossPrice;
    public double? TakeProfitPrice { get; set; } = position.TakeProfitPrice;
    public PositionStatus Status => EntryQuantity > 0 && ExitQuantity == EntryQuantity ? PositionStatus.Closed : PositionStatus.Open;
    public double EntryQuantity => EntryOrders.Sum(o => o.Quantity);
    public double ExitQuantity => ExitOrders.Sum(o => o.Quantity);
    public double Quantity => EntryQuantity;
    public double UnrealizedQuantity => EntryQuantity - ExitQuantity;
    public double RealizedQuantity => ExitQuantity;
    public double Lever => ExecutedOrders.FirstOrDefault()?.Lever ?? 1d;
    public DateTime EntryTime => OrderedEntryOrders.FirstOrDefault().ExecutedTime!.Value;
    public DateTime? ExitTime => OrderedExitOrders.LastOrDefault()?.ExecutedTime;
    public TimeSpan Lifetime => ExitTime is null ? TimeSpan.FromSeconds(0) : ExitTime.Value - EntryTime;
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
    private List<IOrder> ExecutedOrders { get; } = [];
    public IReadOnlyList<IOrder> EntryOrders => ExecutedOrders.Where(o => o.ToPositionSide() == Side).ToList();
    public IReadOnlyList<IOrder> ExitOrders => ExecutedOrders.Where(o => o.ToPositionSide() != Side).ToList();
    public IReadOnlyList<IOrder> OrderedEntryOrders => EntryOrders.OrderBy(o => o.ExecutedTime).ToList();
    public IReadOnlyList<IOrder> OrderedExitOrders => ExitOrders.OrderBy(o => o.ExecutedTime).ToList();



    #endregion Orders



    public void AddExecutedOrder(IOrder order)
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
        if (this.IsClosed()) throw new InvalidOperationException($"cannot add order on a closed position");

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
