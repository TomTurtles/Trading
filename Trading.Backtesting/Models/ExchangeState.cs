namespace Trading.Backtesting;

public class ExchangeState
{
    public double Cash { get; set; }   
    public IEnumerable<Order> Orders { get; set; }
    public IEnumerable<Position> Positions { get; set; }
}
