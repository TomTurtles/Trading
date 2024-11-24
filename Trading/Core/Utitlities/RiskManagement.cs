namespace Traing;

/// <summary>
/// Bietet Methoden für Risikomanagement-Berechnungen.
/// </summary>
public static class RiskManagement
{
    /// <summary>
    /// Konvertiert die Menge in die Positionsgröße.
    /// Beispiel: Anfordern von 2 Aktien zum Preis von 50€ ergibt 100€.
    /// </summary>
    /// <param name="qty">Die Menge.</param>
    /// <param name="price">Der Preis pro Einheit.</param>
    /// <returns>Die Positionsgröße.</returns>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn die Menge oder der Preis ungültig (NaN) ist.</exception>
    public static double QtyToSize(double qty, double price)
    {
        if (double.IsNaN(qty) || double.IsNaN(price))
        {
            throw new ArgumentException("Menge oder Preis ist ungültig (NaN).");
        }
        return qty * price;
    }

    /// <summary>
    /// Ein Risikomanagement-Tool, um schnell die Menge basierend auf dem Risikoprozentsatz zu ermitteln.
    /// </summary>
    /// <param name="capital">Das Gesamtkapital.</param>
    /// <param name="riskPerCapital">Der Risikoprozentsatz des Kapitals.</param>
    /// <param name="entryPrice">Der Einstiegspreis.</param>
    /// <param name="stopLossPrice">Der Stop-Loss-Preis.</param>
    /// <param name="precision">Die Anzahl der Dezimalstellen für die Rundung (Standard ist 8).</param>
    /// <param name="feeRate">Der Gebührensatz (Standard ist 0).</param>
    /// <returns>Die berechnete Menge.</returns>
    public static decimal RiskToQty(decimal capital, decimal riskPerCapital, decimal entryPrice, decimal stopLossPrice, int precision = 8, decimal feeRate = 0)
    {
        decimal riskPerQty = Math.Abs(entryPrice - stopLossPrice);
        decimal size = RiskToSize(capital, riskPerCapital, riskPerQty, entryPrice);

        if (feeRate != 0)
        {
            size *= (1 - feeRate * 3);
        }

        return SizeToQty(size, entryPrice, precision, feeRate);
    }

    /// <summary>
    /// Berechnet die Positionsgröße basierend auf dem Risikoprozentsatz, den Sie eingehen möchten.
    /// Beispiel: RiskToSize(10000, 1, 0.7, 8.6) ergibt ungefähr 1229.
    /// </summary>
    /// <param name="capitalSize">Die Größe des Kapitals.</param>
    /// <param name="riskPercentage">Der Risikoprozentsatz.</param>
    /// <param name="riskPerQty">Das Risiko pro Einheit.</param>
    /// <param name="entryPrice">Der Einstiegspreis.</param>
    /// <returns>Die berechnete Positionsgröße.</returns>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn das Risiko pro Einheit null ist.</exception>
    public static decimal RiskToSize(decimal capitalSize, decimal riskPercentage, decimal riskPerQty, decimal entryPrice)
    {
        if (riskPerQty == 0)
        {
            throw new ArgumentException("Das Risiko pro Einheit kann nicht null sein.", nameof(riskPerQty));
        }

        riskPercentage /= 100;
        decimal tempSize = ((riskPercentage * capitalSize) / riskPerQty) * entryPrice;
        return Math.Min(tempSize, capitalSize);
    }

    /// <summary>
    /// Konvertiert die Positionsgröße in die Menge.
    /// Beispiel: Anfordern von 100€ zum Einstiegspreis von 50€ ergibt 2 Einheiten.
    /// </summary>
    /// <param name="positionSize">Die Positionsgröße.</param>
    /// <param name="entryPrice">Der Einstiegspreis.</param>
    /// <param name="precision">Die Anzahl der Dezimalstellen für die Rundung (Standard ist 3).</param>
    /// <param name="feeRate">Der Gebührensatz (Standard ist 0).</param>
    /// <returns>Die berechnete Menge.</returns>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn der Einstiegspreis null ist oder ungültige Werte vorliegen.</exception>
    public static decimal SizeToQty(decimal positionSize, decimal entryPrice, int precision = 3, decimal feeRate = 0)
    {
        if (entryPrice == 0)
        {
            throw new ArgumentException("Der Einstiegspreis kann nicht null sein.", nameof(entryPrice));
        }

        if (feeRate != 0)
        {
            positionSize *= (1 - feeRate * 3);
        }

        return FloorWithPrecision(positionSize / entryPrice, precision);
    }

    /// <summary>
    /// Rundet einen Wert auf eine bestimmte Anzahl von Dezimalstellen ab.
    /// </summary>
    /// <param name="value">Der zu rundende Wert.</param>
    /// <param name="precision">Die Anzahl der Dezimalstellen.</param>
    /// <returns>Der abgerundete Wert.</returns>
    private static decimal FloorWithPrecision(decimal value, int precision)
    {
        var factor = Math.Pow(10, precision);
        return Math.Floor(value * (decimal)factor) / (decimal)factor;
    }
}
