namespace Marek.Trading.Core;

public static class PositionSideExtensions
{
    public static OrderSide ToOrderSide(this PositionSide side) => side switch
    {
        PositionSide.LONG => OrderSide.Buy,
        PositionSide.SHORT => OrderSide.Sell,
        _ => throw new NotImplementedException(),
    };
    public static OrderSide ToOppositeOrderSide(this PositionSide side) => side switch
    {
        PositionSide.LONG => OrderSide.Sell,
        PositionSide.SHORT => OrderSide.Buy,
        _ => throw new NotImplementedException(),
    };
}