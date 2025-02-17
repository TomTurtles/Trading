namespace Marek.Trading.Core;

public class OnPositionUpdatedEventArgs()
{
    public string Symbol {  get; set; }
    public PositionSide Side { get; set; }
    public double AvgEntryPrice { get;set; }
    public int Lever { get;set; }
    public double Quantity { get;set; } 
    public double OldQuantity { get;set; }
    public double UnrealizedPNL { get; set; }
    public DateTime Timestamp { get; set; }
}
