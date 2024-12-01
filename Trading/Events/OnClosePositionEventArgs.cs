namespace Trading;

public class OnClosePositionEventArgs(Candle candle, Position position, double executionPrice, double executionFee) : TradingBaseEventArgs(candle)
{
    public Position Position { get; } = position;
    public double ExecutionPrice { get; } = executionPrice;
    public double ExecutionFee { get; } = executionFee;
}
