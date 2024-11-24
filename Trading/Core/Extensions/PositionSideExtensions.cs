namespace Trading;

public static class PositionSideExtensions
{
    public static OrderSide ToOrderSide(this PositionSide side) => side switch
    {
        PositionSide.Long => OrderSide.Buy,
        PositionSide.Short => OrderSide.Sell,
        _ => throw new NotImplementedException(),
    };
}