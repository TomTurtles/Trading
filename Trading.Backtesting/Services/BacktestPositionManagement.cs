namespace Trading.Backtesting;

public class BacktestPositionManagement 
{
    private ConcurrentDictionary<DateTime, Position> ClosedPositionsHistory { get; } = new ConcurrentDictionary<DateTime, Position>(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, Position> OrderedClosedPositions => new(ClosedPositionsHistory.OrderBy(kvp => kvp.Key));
    private IEnumerable<Position> ClosedPositions => OrderedClosedPositions.Values;
    private Position? CurrentPosition { get; set; }

    #region Get

    public Task<Position?> GetPositionAsync() => Task.FromResult(CurrentPosition);
    public Task<IEnumerable<Position>> GetPositionHistoryAsync() => Task.FromResult(ClosedPositions);
    public Task<IEnumerable<Position>> GetClosedPositionsAsync() => Task.FromResult(ClosedPositions.Where(p => !p.IsOpen));
    public async Task<bool> HasCurrentOpenPositionAsync() => (await GetPositionAsync()) != null;

    #endregion Get

    #region Open

    public async Task<Position> OpenPositionAsync(Candle candle, Order order)
    {
        if (await HasCurrentOpenPositionAsync())
        {
            throw new InvalidOperationException($"opening position not allowed, open position already exists");
        }

        var position = Position.CreateFromOrder(order);
        CurrentPosition = position;
        ClosedPositionsHistory.TryAdd(candle.Timestamp, position);

        return position;
    }

    #endregion Open

    #region Update

    /// <summary>
    /// Es dürfen nur verändert werden: StopLoss, TakeProfit
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public async Task<Position?> UpdatePositionAsync(Candle candle, string symbol, Action<Position> configure)
    {
        var position = await GetPositionAsync();
        return position != null ? await UpdatePositionAsync(candle, position, configure) : position;
    }

    /// <summary>
    /// increase / reduce / close with an order
    /// </summary>
    /// <param name="position"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public async Task<Position> UpdatePositionAsync(Candle candle, Position position, Order order)
    {
        AddToHistory(candle, position.Copy());
        Action<Position> configure = (p) => p.AddExecutedOrder(order);
        return await UpdatePositionAsync(candle, position, configure);
    }

    private Task<Position> UpdatePositionAsync(Candle candle, Position position, Action<Position> configure)
    {
        AddToHistory(candle, position.Copy());
        configure(position);

        if (CurrentPosition?.IsClosed ?? false)
        {
            AddToHistory(candle, CurrentPosition);
            CurrentPosition = null;
        }

        return Task.FromResult(position);
    }


    #endregion Update

    #region Liquidation

    public async Task<Position?> TryLiquidatePositionAsync(Candle candle, double executionPrice, double executionFee)
    {
        var position = await GetPositionAsync();
        if (position == null) return position;
        return await LiquidatePositionAsync(candle, position, executionPrice, executionFee);
    }

    public async Task<Position?> LiquidatePositionAsync(Candle candle, Position position, double executionPrice, double executionFee)
    {
        if (position.IsClosed) throw new Exception("position is already closed");

        var order = Order.Create(position.Symbol, position.Side.ToOppositeOrderSide());

        order.Quantity = position.Quantity;
        order.Status = OrderStatus.Filled;
        order.ExecutedPrice = executionPrice;
        order.ExecutedFee = executionFee;
        order.ExecutedTime = candle.Timestamp;

        order.CalculateType(executionPrice);

        var updatedPosition = await UpdatePositionAsync(candle, position, order);

        if (updatedPosition.IsOpen) throw new Exception("updated position should be closed at this point");

        return updatedPosition;
    }

    #endregion Liquidation


    private void AddToHistory(Candle candle, Position position) => ClosedPositionsHistory.TryAdd(candle.Timestamp, position);
}
