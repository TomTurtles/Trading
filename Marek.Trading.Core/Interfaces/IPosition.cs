namespace Marek.Trading.Core;

public interface IPosition
{
    string Id { get; }
    string Symbol { get; }
    PositionSide Side { get; }
    double? StopLossPrice { get; set; }
    double? TakeProfitPrice { get; set; }
    PositionStatus Status { get; }
    double EntryQuantity { get; }
    double ExitQuantity { get; }
    double Quantity { get; }
    double UnrealizedQuantity { get; }
    double RealizedQuantity { get; }
    double Lever { get; }
    DateTime EntryTime { get; }
    DateTime? ExitTime { get; }
    TimeSpan Lifetime { get; }
    double EntryPrice { get; }
    double EntryValue { get; }
    double? ExitPrice { get; }
    double ExitValue { get; }
    double Fee { get; }
    double RealizedPNL { get; }
}
