namespace Trading;
public class Backtesting
{
    private Portfolio _portfolio;

    public Backtesting( Portfolio portfolio)
    {
        _portfolio = portfolio;
    }

    //public async Task RunAync(IStrategy strategy, IEnumerable<Candle> candles, CancellationToken token = default)
    //{
    //    foreach (var candle in candles)
    //    {
    //        // Übermittele die neue Candle an die Strategie
    //        await strategy.InitializeAsync(candle, token);
    //        await strategy.ExecuteStrategyAsync(token);

    //        // Verarbeite offene Orders
    //        var pendingOrders = strategy.GetPendingOrders();
    //        foreach (var order in pendingOrders)
    //        {
    //            if (OrderShouldExecute(order, candle))
    //            {
    //                // Setze Ausführungsdetails
    //                order.ExecutedPrice = GetExecutionPrice(order, candle);
    //                order.ExecutedTime = candle.Date;
    //                order.Status = Order.OrderStatus.Filled;

    //                // Aktualisiere das Portfolio
    //                _portfolio.UpdatePortfolio(order);
    //            }
    //        }

    //        // Aktualisiere das Portfolio
    //        _portfolio.UpdateMarketValue(candle);

    //        // Optional: Sammle Performance-Daten
    //    }

    //    // Nach dem Backtest: Berechne Performance-Metriken
    //    var metrics = _portfolio.CalculatePerformanceMetrics();
    //    //var reportGenerator = new ReportGenerator();
    //    //reportGenerator.GenerateReport(metrics);
    //}

    //private void ProcessOrders(Candle candle)
    //{
    //    var pendingOrders = _portfolio.GetPendingOrders();
    //    foreach (var order in pendingOrders)
    //    {
    //        if (OrderShouldExecute(order, candle))
    //        {
    //            ExecuteOrder(order, candle);
    //        }
    //    }
    //}

    //private bool OrderShouldExecute(Order order, Candle candle)
    //{
    //    switch (order.Type)
    //    {
    //        case Order.OrderType.Market:
    //            return true;
    //        case Order.OrderType.Limit:
    //            if (order.Side == Order.OrderSide.Buy && candle.Low <= order.Price)
    //                return true;
    //            if (order.Side == Order.OrderSide.Sell && candle.High >= order.Price)
    //                return true;
    //            break;
    //        case Order.OrderType.Stop:
    //            if (order.Side == Order.OrderSide.Buy && candle.High >= order.Price)
    //                return true;
    //            if (order.Side == Order.OrderSide.Sell && candle.Low <= order.Price)
    //                return true;
    //            break;
    //    }
    //    return false;
    //}

    //private void ExecuteOrder(Order order, Candle candle)
    //{
    //    // Setze Ausführungsdetails
    //    order.ExecutedPrice = GetExecutionPrice(order, candle);
    //    order.ExecutedTime = candle.Date;
    //    order.Status = Order.OrderStatus.Filled;

    //    // Aktualisiere das Portfolio
    //    _portfolio.UpdatePortfolio(order);
    //}

    //private decimal GetExecutionPrice(Order order, Candle candle)
    //{
    //    // Für Market-Orders nehmen wir den Open-Preis der nächsten Candle
    //    return order.Type == Order.OrderType.Market ? candle.Open : order.Price;
    //}
}
