namespace Marek.Trading.Core;

public class Position : IPosition
{
    public string Id { get; set; }
    public string Symbol { get; set; }

    [ConvertStringEnum]
    public PositionSide Side { get; set; }
    public double? StopLossPrice { get; set; }
    public double? TakeProfitPrice { get; set; }
    public PositionStatus Status { get; set; }
    public double EntryQuantity { get; set; }
    public double ExitQuantity { get; set; }
    public double Quantity { get; set; }
    public double UnrealizedQuantity { get; set; }
    public double RealizedQuantity { get; set; }
    public double Lever { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public TimeSpan Lifetime { get; set; }
    public double EntryPrice { get; set; }
    public double EntryValue { get; set; }
    public double? ExitPrice { get; set; }
    public double ExitValue { get; set; }
    public double Fee { get; set; }
    public double RealizedPNL { get; set; }


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
          .AppendLine($"Fee: {Fee}");

        return sb.ToString();
    }
}
