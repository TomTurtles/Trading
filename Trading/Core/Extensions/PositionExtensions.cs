﻿namespace Trading;
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
    public static decimal GetValue(this Position position, decimal marketPrice)
    {
        return position.Quantity * marketPrice;
    }
}
