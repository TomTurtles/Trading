namespace Trading;

public interface IStrategy
{
    // Definition
    string Name { get; }
    string Symbol { get; }
    //CandleStickInterval Interval { get; }

    // Portfolio
    Position? Position { get; }
    List<Order> Orders { get; }

    // Execution
    Task InitializeAsync(Candle candle, CancellationToken cancellationToken = default);
    Task ExecuteStrategyAsync(CancellationToken cancellationToken = default);

    // Eventhandler
    void OnPositionOpened(Position position);
    void OnPositionClosed(Position position);
    void OnOrderPlaced(Order order);
    void OnOrderExecuted(Order order);
}
