namespace Trading;

public class OnClosePositionEventArgs(Candle candle, Position position, decimal executionPrice, decimal executionFee) : TradingBaseEventArgs(candle)
{
    public Position Position { get; } = position;
    public decimal ExecutionPrice { get; } = executionPrice;
    public decimal ExecutionFee { get; } = executionFee;
}
