namespace Marek.Trading.Core;

public static class PositionValueExtensions
{
    /// <summary>
    /// Berechnet den aktuellen Wert einer Position basierend auf ihren Einstiegs-Orders,
    /// realisierten Gewinnen und unrealisierten Gewinnen. Falls die Position noch offen ist,
    /// muss ein Marktpreis angegeben werden.
    /// </summary>
    /// <param name="position">Die Position, für die der Wert berechnet werden soll.</param>
    /// <param name="marketPrice">Der aktuelle Marktpreis (erforderlich, wenn die Position noch offen ist).</param>
    /// <returns>Der aktuelle Wert der Position.</returns>
    /// <exception cref="ArgumentException">Wird geworfen, wenn die Position offen ist, aber kein Marktpreis übergeben wurde.</exception>
    public static double GetValue(this Position position, double? marketPrice = null)
    {
        if (position.IsOpen && !marketPrice.HasValue)
        {
            throw new ArgumentException("Die Position ist noch offen. Bitte gib einen Marktpreis an, um eine korrekte Berechnung durchzuführen.", nameof(marketPrice));
        }

        return position.EntryValue
             + position.RealizedPNL
             + (position.IsOpen ? position.GetUnrealizedPNL(marketPrice!.Value) : 0);
    }

    /// <summary>
    /// Berechnet den realisierten Wert einer Position, also den bereits realisierten Gewinn/Verlust
    /// und die ursprüngliche Investition aus geschlossenen Teilen der Position.
    /// </summary>
    /// <param name="position">Die Position, deren realisierter Wert berechnet werden soll.</param>
    /// <returns>Der realisierte Wert der Position.</returns>
    public static double GetUnrealizedValue(this Position position, double marketPrice)
    {
        return (position.UnrealizedQuantity * position.EntryPrice) + position.GetUnrealizedPNL(marketPrice);
    }

}
