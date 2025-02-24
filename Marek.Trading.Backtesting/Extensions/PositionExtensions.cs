namespace Marek.Trading.Backtesting;

public static class PositionExtensions
{
    /// <summary>
    /// Berechnet die unrealisierten Gewinne (PNL) für eine Position.
    /// </summary>
    /// <param name="position">Die Position, für die die unrealisierten Gewinne berechnet werden.</param>
    /// <param name="exit">Der Exit-Preis (entweder aktueller Marktpreis oder ExitPrice).</param>
    /// <param name="quantity">Die Menge der Position, für die die Berechnung durchgeführt wird.</param>
    /// <returns>Die unrealisierten Gewinne in Geldeinheiten.</returns>
    private static double CalculateUnrealizedPNL(IPosition position, double exit, double quantity)
    {
        var diff = exit - position.EntryPrice;
        var sign = position.Side == PositionSide.LONG ? 1 : -1;
        return sign * diff * position.Lever * quantity;
    }

    /// <summary>
    /// Berechnet die unrealisierten Gewinne der Position für eine bestimmte Menge.
    /// Falls die Position noch offen ist, wird als Exit-Preis der Entry-Preis verwendet.
    /// </summary>
    /// <param name="position">Die Position, für die die Berechnung durchgeführt wird.</param>
    /// <param name="quantity">Die Menge, für die die Berechnung erfolgen soll.</param>
    /// <returns>Die unrealisierten Gewinne der Position für die gegebene Menge.</returns>
    public static double GetUnrealizedPNLForQuantity(this IPosition position, double quantity)
        => CalculateUnrealizedPNL(position, position.ExitPrice ?? position.EntryPrice, quantity);

    /// <summary>
    /// Berechnet die unrealisierten Gewinne der Position für eine bestimmte Menge
    /// und einen bestimmten Marktpreis.
    /// </summary>
    /// <param name="position">Die Position, für die die Berechnung durchgeführt wird.</param>
    /// <param name="marketPrice">Der Marktpreis, zu dem die Berechnung erfolgen soll.</param>
    /// <param name="quantity">Die Menge, für die die Berechnung erfolgen soll.</param>
    /// <returns>Die unrealisierten Gewinne der Position für die gegebene Menge und den Marktpreis.</returns>
    public static double GetUnrealizedPNL(this IPosition position, double marketPrice, double quantity)
        => CalculateUnrealizedPNL(position, marketPrice, quantity);

    /// <summary>
    /// Berechnet die unrealisierten Gewinne der Position mit einem optionalen Marktpreis.
    /// Falls kein Marktpreis angegeben ist, wird der Exit-Preis oder der Entry-Preis verwendet.
    /// </summary>
    /// <param name="position">Die Position, für die die Berechnung durchgeführt wird.</param>
    /// <param name="marketPrice">Optionaler Marktpreis für die Berechnung.</param>
    /// <returns>Die unrealisierten Gewinne der Position basierend auf dem aktuellen oder angegebenen Preis.</returns>
    public static double GetUnrealizedPNL(this IPosition position, double? marketPrice = null) 
        => CalculateUnrealizedPNL(position, marketPrice ?? position.ExitPrice ?? position.EntryPrice, position.UnrealizedQuantity);

    /// <summary>
    /// Berechnet den aktuellen Wert einer Position basierend auf ihren Einstiegs-Orders,
    /// realisierten Gewinnen und unrealisierten Gewinnen. Falls die Position noch offen ist,
    /// muss ein Marktpreis angegeben werden.
    /// </summary>
    /// <param name="position">Die Position, für die der Wert berechnet werden soll.</param>
    /// <param name="marketPrice">Der aktuelle Marktpreis (erforderlich, wenn die Position noch offen ist).</param>
    /// <returns>Der aktuelle Wert der Position.</returns>
    /// <exception cref="ArgumentException">Wird geworfen, wenn die Position offen ist, aber kein Marktpreis übergeben wurde.</exception>
    public static double GetValue(this IPosition position, double? marketPrice = null)
    {
        if (position.IsOpen() && !marketPrice.HasValue)
        {
            throw new ArgumentException("Die Position ist noch offen. Bitte gib einen Marktpreis an, um eine korrekte Berechnung durchzuführen.", nameof(marketPrice));
        }

        return (position.EntryPrice * position.Quantity)
             + position.RealizedPNL
             + (position.IsOpen() ? position.GetUnrealizedPNL(marketPrice!.Value) : 0);
    }

    /// <summary>
    /// Berechnet den realisierten Wert einer Position, also den bereits realisierten Gewinn/Verlust
    /// und die ursprüngliche Investition aus geschlossenen Teilen der Position.
    /// </summary>
    /// <param name="position">Die Position, deren realisierter Wert berechnet werden soll.</param>
    /// <returns>Der realisierte Wert der Position.</returns>
    public static double GetUnrealizedValue(this IPosition position, double marketPrice)
    {
        return (position.UnrealizedQuantity * position.EntryPrice) + position.GetUnrealizedPNL(marketPrice);
    }
}