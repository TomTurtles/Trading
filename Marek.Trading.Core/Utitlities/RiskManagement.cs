namespace Marek.Trading.Core;

/// <summary>
/// Bietet Methoden für Risikomanagement-Berechnungen.
/// </summary>
public static class RiskManagement
{
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
    public static double RiskToQty(double capital, double riskPerCapital, double entryPrice, double stopLossPrice, double feeRate = 0)
    {
        // Das Risiko pro Einheit berechnen
        double riskPerQty = Math.Abs(entryPrice - stopLossPrice);

        // Die zu riskierende Positionsgröße berechnen
        double size = RiskToSize(capital, riskPerCapital, riskPerQty, entryPrice);

        // Die Positionsgröße in die Menge umrechnen unter Berücksichtigung der Gebühren
        return SizeToQty(size, entryPrice, feeRate);
    }

    /// <summary>
    /// Berechnet die Positionsgröße basierend auf dem Risikoprozentsatz, den Sie eingehen möchten.
    /// Beispiel: RiskToSize(10000, 1, 0.7, 8.6) ergibt ungefähr 1229.
    /// </summary>
    /// <param name="capital">Die Größe des Kapitals.</param>
    /// <param name="riskPercentage">Der Risikoprozentsatz.</param>
    /// <param name="riskPerUnit">Das Risiko pro Einheit.</param>
    /// <param name="price">Der Einstiegspreis.</param>
    /// <returns>Die berechnete Positionsgröße.</returns>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn das Risiko pro Einheit null ist.</exception>
    public static double RiskToSize(double capital, double riskPercentage, double riskPerUnit, double price)
    {
        if (capital < 0)
        {
            throw new ArgumentException("Das Kapital kann nicht kleiner als null sein.", nameof(riskPerUnit));
        }

        if (riskPercentage < 0)
        {
            throw new ArgumentException("Das Risiko kann nicht kleiner als null sein.", nameof(riskPerUnit));
        }

        if (riskPerUnit == 0)
        {
            throw new ArgumentException("Das Risiko pro Einheit kann nicht null sein.", nameof(riskPerUnit));
        }

        if (riskPerUnit < 0)
        {
            throw new ArgumentException("Das Risiko pro Einheit kann nicht kleiner als null sein.", nameof(riskPerUnit));
        }

        // Das Risiko in Prozent in einen Dezimalwert umwandeln
        var risk = riskPercentage / 100;

        // Das zu riskierende Kapital berechnen
        var riskOfCapital = capital * risk;

        // Der Risikofaktor berechnet sich aus dem Verhältnis des Risikos des Kapitals zum Risiko pro Einheit
        var riskFactor = riskOfCapital / riskPerUnit;

        // Die Positionsgröße berechnen
        var size = riskFactor * price;

        // Die Positionsgröße auf das Kapital begrenzen
        return Math.Min(size, capital);
    }

    /// <summary>
    /// Konvertiert die gewünschte Positionsgröße in die Menge.
    /// Beispiel: Anfordern von 100€ zum Einstiegspreis von 50€ ergibt 2 Einheiten.
    /// </summary>
    /// <param name="size">Die SOLL Positionsgröße.</param>
    /// <param name="price">Der Einstiegspreis.</param>
    /// <param name="feeRate">Der Gebührensatz (Standard ist 0).</param>
    /// <returns>Die berechnete Menge.</returns>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn der Einstiegspreis null ist oder ungültige Werte vorliegen.</exception>
    public static double SizeToQty(double size, double price, double feeRate = 0)
    {
        if (price == 0)
        {
            throw new ArgumentException("Der Einstiegspreis kann nicht '0' sein.", nameof(price));
        }

        return (size / price) * (1 - feeRate);
    }

    /// <summary>
    /// Konvertiert die Menge in die Positionsgröße.
    /// Beispiel: Anfordern von 2 Aktien zum Preis von 50€ ergibt 100€.
    /// </summary>
    /// <param name="qty">Die Menge.</param>
    /// <param name="price">Der Preis pro Einheit.</param>
    /// <param name="feeRate">Der Gebührensatz (Standard ist 0).</param>
    /// <returns>Die Positionsgröße.</returns>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn die Menge oder der Preis ungültig (NaN) ist.</exception>
    public static double QtyToSize(double qty, double price, double feeRate = 0)
    {
        if (double.IsNaN(qty) || double.IsNaN(price))
        {
            throw new ArgumentException("Menge oder Preis ist ungültig (NaN).");
        }
        return (qty * price) * (1 - feeRate); ;
    }
}
