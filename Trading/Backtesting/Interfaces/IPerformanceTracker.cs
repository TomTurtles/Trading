namespace Trading;

public interface IPerformanceTracker
{
    // Backtest
    TimeSpan Duration { get; }

    // Period
    DateTime Start { get; }
    DateTime End { get; }
    TimeSpan Period { get; }

    // Risk
    bool MarginCall { get; }

    // Candles
    int GetCandlesCount();
    CandleInterval GetCandleInterval();


    IDictionary<DateTime, decimal> GetCashCurve();
    decimal GetCashMaximum();
    decimal GetCashMinimum();
    decimal GetCashStart();
    decimal GetCashEnd();
    decimal GetCashPerformance();
    decimal GetMaxDrawdown();

    // Exceptions
    IEnumerable<string> GetExceptions();

    // Orders
    IEnumerable<Order> GetOrders();


    // Closed Positions
    IEnumerable<Position> GetClosedPositions();
    int GetClosedPositionsCount();
    decimal GetClosedPositionsRealization();
    decimal GetClosedPositionsWinRate();
    decimal GetClosedPositionsWinToLossAverage();

    // Equity
    IDictionary<DateTime, decimal> GetEquityCurve();
    decimal GetEquityStart();
    decimal GetEquityEnd();
    decimal GetEquityMinimum();
    decimal GetEquityMaximum();
    object GetEquityPerformance();
}
