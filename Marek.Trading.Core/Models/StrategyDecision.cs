namespace Marek.Trading.Core;

public class StrategyDecision(Candle candle, StrategyDecisionType type)
{
    [ConvertStringEnum]
    public StrategyDecisionType Type { get; } = type;
    public Candle Candle { get; } = candle;
    public string? Reason { get; set; }
    public IPosition? Position { get; set; }
    public IEnumerable<UpdatePositionCommandType>? PositionUpdateCommands { get; set; }
    public IOrder? Order { get; set; }
    public List<IOrder>? Orders { get; set; }
    public Exception? Exception { get; set; }

    internal static StrategyDecision Wait(Candle candle, string reason) => new(candle, StrategyDecisionType.Wait)
    {
        Reason = reason,
    };

    internal static StrategyDecision ClosePosition(Candle candle, IPosition position) => new(candle, StrategyDecisionType.ClosePosition)
    {
        Position = position
    };

    internal static StrategyDecision UpdatePosition(Candle candle, IPosition position, IEnumerable<UpdatePositionCommandType> updateCommands) => new(candle, StrategyDecisionType.UpdatePosition)
    {
        Position = position,
        PositionUpdateCommands = updateCommands
    };

    internal static StrategyDecision CancelOrders(Candle candle, List<IOrder> orders) => new(candle, StrategyDecisionType.CancelOrders)
    {
        Orders = orders
    };

    internal static StrategyDecision GoLong(Candle candle, IOrder order) => new(candle, StrategyDecisionType.GoLong)
    {
        Order = order
    };

    internal static StrategyDecision GoShort(Candle candle, IOrder order) => new(candle, StrategyDecisionType.GoShort)
    {
        Order = order
    };

    internal static StrategyDecision Error(Candle candle, Exception exception) => new(candle, StrategyDecisionType.Error)
    {
        Reason = exception.Message,
        Exception = exception
    };

    public override string ToString()
    {
        return $"{Type}{(Reason is null ? "" : ": " + Reason)}";
    }
}
