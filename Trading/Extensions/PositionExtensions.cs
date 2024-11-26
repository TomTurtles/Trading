namespace Trading;
public static class PositionExtensions
{
    public static bool StopPriceHit(this Position position, Candle candle)
    {
        return position.StopPrice.HasValue && candle.PriceHit(position.StopPrice.Value);
    }

    public static bool TakePriceHit(this Position position, Candle candle)
    {
        return position.TakePrice.HasValue && candle.PriceHit(position.TakePrice.Value);
    }

    public static double GetValue(this Position position, double marketPrice)
    {
        var entryValue = position.Quantity * position.EntryPrice;

        var unrealizedPNL = position.Side == PositionSide.Long ? marketPrice - position.EntryPrice : position.EntryPrice - marketPrice;

        var currentUnrealizedPNL = position.Quantity * unrealizedPNL;

        return entryValue + currentUnrealizedPNL;
    }
}
