namespace Trading;

public class Candle
{

    public DateTime Timestamp { get; set; }

    [ConvertStringEnum]
    public CandleInterval Interval { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Volume { get; set; }

}
