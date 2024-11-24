namespace Trading;
public class Position
{
    public enum PositionSide { Long, Short }

    public Guid PositionId { get; set; }
    public PositionSide Side { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal Quantity { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public decimal? ExitPrice { get; set; }
    public string Symbol { get; set; }
}
