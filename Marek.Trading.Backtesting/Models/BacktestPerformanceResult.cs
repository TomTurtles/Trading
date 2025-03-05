namespace Marek.Trading.Backtesting;

public class BacktestPerformanceResult
{
    // Performance

    /// <summary>
    /// Dauer des Backtests.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Anzahl der getesteten Tage.
    /// </summary>
    public int Days { get; set; }

    /// <summary>
    /// Perofrmance des Backtests in Candles pro Sekunde.
    /// </summary>
    public double CandlesPerSecond { get; set; }

    // Candles

    /// <summary>
    /// Gesamtanzahl der verwendeten Candles im Backtest.
    /// </summary>
    public int Candles { get; set; }

    /// <summary>
    /// Erster Candle-Wert des Backtest-Zeitraums.
    /// </summary>
    public decimal CandleStart { get; set; }

    /// <summary>
    /// Letzter Candle-Wert des Backtest-Zeitraums.
    /// </summary>
    public decimal CandleEnd { get; set; }

    /// <summary>
    /// Höchster Candle-Wert im Backtest-Zeitraum.
    /// </summary>
    public decimal CandleMax { get; set; }

    /// <summary>
    /// Niedrigster Candle-Wert im Backtest-Zeitraum.
    /// </summary>
    public decimal CandleMin { get; set; }

    /// <summary>
    /// Prozentuale Candle-Performance basierend auf Start- und Endwert.
    /// </summary>
    public decimal CandlePerformance { get; set; }

    // Equity

    /// <summary>
    /// Startkapital des Backtests.
    /// </summary>
    public decimal EquityStart { get; set; }

    /// <summary>
    /// Endkapital des Backtests.
    /// </summary>
    public decimal EquityEnd { get; set; }

    /// <summary>
    /// Höchster Kapitalwert während des Backtests.
    /// </summary>
    public decimal EquityMax { get; set; }

    /// <summary>
    /// Niedrigster Kapitalwert während des Backtests.
    /// </summary>
    public decimal EquityMin { get; set; }

    /// <summary>
    /// Prozentuale Equity-Performance basierend auf Start- und Endkapital.
    /// </summary>
    public decimal EquityPerformance { get; set; }

    // Indicators

    /// <summary>
    /// Sharpe Ratio des Backtests (Risiko-adjustierte Rendite).
    /// </summary>
    public decimal SharpeRatio { get; set; }

    /// <summary>
    /// Kelly-Kriterium für die optimale Kapitaleinsatzstrategie.
    /// </summary>
    public decimal KellyCriterion { get; set; }

    /// <summary>
    /// Maximaler Drawdown während des Backtests.
    /// </summary>
    public decimal MaximumDrawdown { get; set; }

    /// <summary>
    /// Recovery-Faktor: Verhältnis von Gesamtprofit zu maximalem Drawdown.
    /// </summary>
    public decimal RecoveryFactor { get; set; }

    /// <summary>
    /// Gesamter Profit aus allen Positionen.
    /// </summary>
    public decimal TotalProfit { get; set; }

    /// <summary>
    /// Gesamtgebühren während des Backtests.
    /// </summary>
    public decimal TotalFee { get; set; }

    /// <summary>
    /// Gewinnrate: Prozentualer Anteil der gewinnenden Positionen.
    /// </summary>
    public decimal WinRate { get; set; }

    /// <summary>
    /// Verhältnis zwischen durchschnittlichem Gewinn- und Verlust-Trade.
    /// </summary>
    public decimal WinLossRatio { get; set; }

    /// <summary>
    /// Vergleich der Performance mit einer Buy-and-Hold-Strategie.
    /// </summary>
    public decimal BuyAndHoldRatio { get; set; }

    // Positions

    /// <summary>
    /// Anzahl der gehandelten Positionen im Backtest.
    /// </summary>
    public int Positions { get; set; }

    /// <summary>
    /// Anzahl der Long-Positionen.
    /// </summary>
    public int LongPositions { get; set; }

    /// <summary>
    /// Anzahl der Short-Positionen.
    /// </summary>
    public int ShortPositions { get; set; }

    /// <summary>
    /// Anzahl der gewinnenden Positionen.
    /// </summary>
    public int WinningPositions { get; set; }

    /// <summary>
    /// Anzahl der verlierenden Positionen.
    /// </summary>
    public int LosingPositions { get; set; }

    /// <summary>
    /// Durchschnittliche Haltedauer einer Position.
    /// </summary>
    public TimeSpan AveragePositionLifetime { get; set; }

    /// <summary>
    /// Durchschnittliche Positionsgröße.
    /// </summary>
    public decimal AveragePositionSize { get; set; }

    /// <summary>
    /// Durchschnittlicher Gewinn pro Position.
    /// </summary>
    public decimal AverageProfitPerPosition { get; set; }

    /// <summary>
    /// Durchschnittliche Gebühr pro Position.
    /// </summary>
    public decimal AverageFeePerPosition { get; set; }
}