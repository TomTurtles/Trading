namespace Trading;

public interface IExchangePositions
{
    Task<Position?> GetPositionAsync(string symbol);
    Task<Position?> GetOpenPositionAsync(string symbol);
    Task<IEnumerable<Position>> GetPositionsAsync();
    Task<IEnumerable<Position>> GetOpenPositionsAsync();
    Task<IEnumerable<Position>> GetClosedPositionsAsync();

    Task UpdatePositionAsync(Candle candle, string symbol, Action<Position> configure);
    Task UpdatePositionAsync(Candle candle, Position position, Order order);
}
