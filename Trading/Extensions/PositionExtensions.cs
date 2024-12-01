namespace Trading;
public static class PositionExtensions
{
    public static double GetValue(this Position position, double marketPrice)
    {
        var entryValue = position.Quantity * position.EntryPrice;

        var unrealizedPNL = position.Side == PositionSide.Long ? marketPrice - position.EntryPrice : position.EntryPrice - marketPrice;

        var currentUnrealizedPNL = position.Quantity * unrealizedPNL * position.Lever;

        return entryValue + currentUnrealizedPNL;
    }
}
