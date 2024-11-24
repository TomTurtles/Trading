namespace Trading;

public class OnUpdatePositionEventArgs(Candle candle, Position current, Action<Position> configuration) : TradingBaseEventArgs(candle)
{
    public Position CurrentPosition { get; } = current;
    public Action<Position> Configuration { get; } = configuration;
}
