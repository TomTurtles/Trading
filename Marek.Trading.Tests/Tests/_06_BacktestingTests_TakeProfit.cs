namespace Marek.Trading.Backtesting.Tests;

[TestClass]
public sealed class _06_BacktestingTests_TakeProfit
{
    private readonly IServiceProvider _serviceProvider = TestSettings.ServiceProvider;

    [TestMethod]
    [DataRow(1, 1, 6)]
    [DataRow(2, 1, 6)]
    [DataRow(3, 1, 6)]
    [DataRow(4, 1, 6)]
    [DataRow(5, 1, 6)]
    [DataRow(1, 2, 6)]
    [DataRow(4, 3, 6)]
    [DataRow(3, 4, 6)]
    [DataRow(2, 5, 6)]
    [DataRow(1, 6, 6)]
    [DataRow(1, 1, 3)]
    [DataRow(1, 1, 4)]
    [DataRow(1, 1, 5)]
    [DataRow(1, 1, 6)]
    [DataRow(1, 1, 7)]
    [DataRow(1, 1, 8)]
    [DataRow(1, 1, 9)]
    [DataRow(3, 2, 3)]
    [DataRow(.4, 4.5, 4)]
    [DataRow(.2, 8, 5)]
    [DataRow(1, 1.5, 6)]
    [DataRow(5, 3, 7)]
    [DataRow(3.1, 1, 8)]
    [DataRow(.1, 2, 9)]
    public async Task _01_CreateMarketOrder_Long_TakeProfit_ShouldWorkAsync(double quantity, double lever, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 8 - 1, Close = 8, High = 8 + 1  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.TakeProfitPrice = takeProfitPrice;
        await exchange.PlaceOrderAsync(order);

        // Assert expected Margin after first Order Placement
        var margin = await cashManagement.GetMarginAsync();
        var expectedMarginAfterOrderPlacement = options.Value.InitialCash - (order.Quantity * order.ExecutedPrice!.Value) - order.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterOrderPlacement, 8), Math.Round(margin, 8));

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert
        var pendingOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, pendingOrders.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.IsNotNull(positions);
        Assert.AreEqual(1, positions.Count);

        var closedPosition = positions[0];
        Assert.IsNotNull(closedPosition);
        Assert.IsTrue(closedPosition.IsClosed());
        Assert.IsNotNull(closedPosition.ExitOrders);
        Assert.AreEqual(1, closedPosition.ExitOrders.Count);

        var closingOrder = closedPosition.ExitOrders[0];
        Assert.IsNotNull(closingOrder);
        Assert.IsTrue(closingOrder.IsFilled());
        Assert.AreEqual(order.TakeProfitPrice, closingOrder.ExecutedPrice);

        margin = await cashManagement.GetMarginAsync();
        var diff = (closingOrder.ExecutedPrice!.Value * closingOrder.Quantity) - (order.Quantity * order.ExecutedPrice!.Value);
        var expectedMarginAfterCandleRun = options.Value.InitialCash + diff * lever - order.ExecutedFee!.Value - closingOrder.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterCandleRun, 8), Math.Round(margin, 8));

        // Mit Gewinn
        Assert.IsTrue(closedPosition.RealizedPNL > 0);
        Assert.IsTrue(expectedMarginAfterCandleRun > options.Value.InitialCash);
    }

    [TestMethod]
    [DataRow(1, 1, 10)]
    [DataRow(.1, 2, 10)]
    [DataRow(10, 3, 10)]
    [DataRow(4, 4, 10)]
    [DataRow(1.3, 5, 10)]
    [DataRow(1, 1, 12.4)]
    [DataRow(.1, 2, 12.4)]
    [DataRow(10, 3, 12.4)]
    [DataRow(4, 4, 12.4)]
    [DataRow(1.3, 5, 12.4)]
    public async Task _02_CreateMarketOrder_Long_TakeProfit_ShouldNotWorkAsync(double quantity, double lever, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 8 - 1, Close = 8, High = 8 + 1  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.TakeProfitPrice = takeProfitPrice;
        await exchange.PlaceOrderAsync(order);

        // Assert expected Margin after first Order Placement
        var margin = await cashManagement.GetMarginAsync();
        var expectedMarginAfterOrderPlacement = options.Value.InitialCash - (order.Quantity * order.ExecutedPrice!.Value) - order.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterOrderPlacement, 8), Math.Round(margin, 8));

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert
        var pendingOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, pendingOrders.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);
        Assert.IsTrue(openPosition.IsOpen());
        Assert.IsNotNull(openPosition.ExitOrders);
        Assert.AreEqual(0, openPosition.ExitOrders.Count);

        margin = await cashManagement.GetMarginAsync();
        var expectedMarginAfterCandleRun = expectedMarginAfterOrderPlacement;
        Assert.AreEqual(Math.Round(expectedMarginAfterCandleRun, 8), Math.Round(margin, 8));
    }

    [TestMethod]
    [DataRow(1, 1, 1)]
    [DataRow(3, 2, 1)]
    [DataRow(1, 1.5, 1)]
    [DataRow(1.8, 1, 1)]
    [DataRow(.3, 4, 1)]
    [DataRow(1, 1, 1.5)]
    [DataRow(3, 2, 1.1)]
    [DataRow(1, 1.5, 1.2)]
    [DataRow(1.8, 1, 1.3)]
    [DataRow(.3, 4, 2)]
    public async Task _03_CreateMarketOrder_Long_TakeProfit_ShouldThrowAsync(double quantity, double lever, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 8 - 1, Close = 8, High = 8 + 1  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.TakeProfitPrice = takeProfitPrice;
        
        var ex = await Assert.ThrowsExceptionAsync<OrderInvalidException>(async() => await exchange.PlaceOrderAsync(order));
        Assert.IsNotNull(ex);   
        Assert.AreEqual(ex.Message, "take profit price must be higher than market price");
    }

    [TestMethod]
    [DataRow(1, 1, 6)]
    [DataRow(2, 1, 6)]
    [DataRow(.1, 3, 6)]
    [DataRow(3, 1.8, 6)]
    [DataRow(1, 1, 5)]
    [DataRow(2, 1, 5)]
    [DataRow(.1, 3, 5)]
    [DataRow(3, 1.8, 5)]
    [DataRow(1, 1, 7)]
    [DataRow(2, 1, 7)]
    [DataRow(.1, 3, 7)]
    [DataRow(3, 1.8, 7)]
    [DataRow(1, 1, 1)]
    [DataRow(2, 1, 1)]
    [DataRow(.1, 3, 1)]
    [DataRow(3, 1.8, 1)]
    public async Task _04_CreateMarketOrder_Short_TakeProfit_ShouldWorkAsync(double quantity, double lever, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 2 - 1, Close = 2, High = 2 + 1  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.TakeProfitPrice = takeProfitPrice;
        await exchange.PlaceOrderAsync(order);

        // Assert expected Margin after first Order Placement
        var margin = await cashManagement.GetMarginAsync();
        var expectedMarginAfterOrderPlacement = options.Value.InitialCash - (order.Quantity * order.ExecutedPrice!.Value) - order.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterOrderPlacement, 8), Math.Round(margin, 8));

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert
        var pendingOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, pendingOrders.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.IsNotNull(positions);
        Assert.AreEqual(1, positions.Count);

        var closedPosition = positions[0];
        Assert.IsNotNull(closedPosition);
        Assert.IsTrue(closedPosition.IsClosed());
        Assert.IsNotNull(closedPosition.ExitOrders);
        Assert.AreEqual(1, closedPosition.ExitOrders.Count);

        var closingOrder = closedPosition.ExitOrders[0];
        Assert.IsNotNull(closingOrder);
        Assert.IsTrue(closingOrder.IsFilled());
        Assert.AreEqual(order.TakeProfitPrice, closingOrder.ExecutedPrice);

        margin = await cashManagement.GetMarginAsync();
        var diff = (closingOrder.ExecutedPrice!.Value * closingOrder.Quantity) - (order.Quantity * order.ExecutedPrice!.Value);
        var expectedMarginAfterCandleRun = options.Value.InitialCash + (-1) * diff * lever - order.ExecutedFee!.Value - closingOrder.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterCandleRun, 8), Math.Round(margin, 8));

        // Mit Gewinn
        Assert.IsTrue(closedPosition.RealizedPNL > 0);
        Assert.IsTrue(expectedMarginAfterCandleRun > options.Value.InitialCash);
    }

    [TestMethod]
    [DataRow(1, 1, .9)]
    [DataRow(2, 1, .9)]
    [DataRow(3, 1, .9)]
    [DataRow(4, 1, .9)]
    [DataRow(.1, 4, .89)]
    [DataRow(2, 3, .3)]
    [DataRow(.3, 5, .2)]
    [DataRow(4, 1, .1)]
    public async Task _05_CreateMarketOrder_Short_TakeProfit_ShouldNotWorkAsync(double quantity, double lever, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 2 - 1, Close = 2, High = 2 + 1  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.TakeProfitPrice = takeProfitPrice;
        await exchange.PlaceOrderAsync(order);

        // Assert expected Margin after first Order Placement
        var margin = await cashManagement.GetMarginAsync();
        var expectedMarginAfterOrderPlacement = options.Value.InitialCash - (order.Quantity * order.ExecutedPrice!.Value) - order.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterOrderPlacement, 8), Math.Round(margin, 8));

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert
        var pendingOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, pendingOrders.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);
        Assert.IsTrue(openPosition.IsOpen());
        Assert.IsNotNull(openPosition.ExitOrders);
        Assert.AreEqual(0, openPosition.ExitOrders.Count);

        margin = await cashManagement.GetMarginAsync();
        var expectedMarginAfterCandleRun = expectedMarginAfterOrderPlacement;
        Assert.AreEqual(Math.Round(expectedMarginAfterCandleRun, 8), Math.Round(margin, 8));
    }


    [TestMethod]
    [DataRow(1, 1, 8)]
    [DataRow(.1, 1, 8)]
    [DataRow(1, 2, 8.1)]
    [DataRow(1, 3, 9)]
    [DataRow(1, 1, 9)]
    [DataRow(.1, 4, 9.5)]
    [DataRow(1, 1, 10)]
    [DataRow(.1, 5, 10)]
    [DataRow(1, 1, 10.9)]
    public async Task _06_CreateMarketOrder_Short_TakeProfit_ShouldThrowAsync(double quantity, double lever, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 2 - 1, Close = 2, High = 2 + 1  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.TakeProfitPrice = takeProfitPrice;

        var ex = await Assert.ThrowsExceptionAsync<OrderInvalidException>(async () => await exchange.PlaceOrderAsync(order));
        Assert.IsNotNull(ex);
        Assert.AreEqual(ex.Message, "take profit price must be lower than market price");
    }

    [TestMethod]
    [DataRow(1, 1, 4, 6)]
    [DataRow(2, 1, 4, 6)]
    [DataRow(3, 1, 4, 6)]
    [DataRow(4, 1, 5, 6)]
    [DataRow(5, 1, 3, 6)]
    [DataRow(1, 2, 2, 6)]
    [DataRow(4, 3, 3, 6)]
    [DataRow(3, 4, 4, 6)]
    [DataRow(2, 5, 5, 6)]
    [DataRow(1, 6, 5, 6)]
    [DataRow(1, 1, 2, 3)]
    [DataRow(1, 1, 3, 4)]
    [DataRow(1, 1, 4, 5)]
    [DataRow(1, 1, 2, 6)]
    [DataRow(1, 1, 5, 7)]
    [DataRow(1, 1, 7, 8)]
    [DataRow(1, 1, 6, 9)]
    [DataRow(3, 2, 2, 3)]
    [DataRow(.4, 4.5, 2, 4)]
    [DataRow(.2, 8, 3, 5)]
    [DataRow(1, 1.5, 5, 6)]
    [DataRow(5, 3, 4, 7)]
    [DataRow(3.1, 1, 4, 8)]
    [DataRow(.1, 2, 8, 9)]
    public async Task _07_CreateLimitOrder_Long_TakeProfit_ShouldWorkAsync(double quantity, double lever, double price, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-18), Low = 10 - 1, Close = 10, High = 10 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-17), Low = 9 - 1, Close = 9, High = 9 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-16), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-15), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-14), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-13), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-12), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-11), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 9 - 1, Close = 9, High = 9 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low = 10 - 1, Close = 10, High = 10 + 1  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.Price = price;
        order.TakeProfitPrice = takeProfitPrice;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert
        var pendingOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, pendingOrders.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.IsNotNull(positions);
        Assert.AreEqual(1, positions.Count);

        var closedPosition = positions[0];
        Assert.IsNotNull(closedPosition);
        Assert.IsTrue(closedPosition.IsClosed());
        Assert.IsNotNull(closedPosition.ExitOrders);
        Assert.AreEqual(1, closedPosition.ExitOrders.Count);

        var closingOrder = closedPosition.ExitOrders[0];
        Assert.IsNotNull(closingOrder);
        Assert.IsTrue(closingOrder.IsFilled());
        Assert.AreEqual(order.TakeProfitPrice, takeProfitPrice);
        Assert.AreEqual(order.TakeProfitPrice, closingOrder.ExecutedPrice);

        var margin = await cashManagement.GetMarginAsync();
        var diff = (closingOrder.ExecutedPrice!.Value * closingOrder.Quantity) - (order.Quantity * order.ExecutedPrice!.Value);
        var expectedMarginAfterCandleRun = options.Value.InitialCash + diff * lever - order.ExecutedFee!.Value - closingOrder.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterCandleRun, 8), Math.Round(margin, 8));

        // Mit Gewinn
        Assert.IsTrue(closedPosition.RealizedPNL > 0);
        Assert.IsTrue(expectedMarginAfterCandleRun > options.Value.InitialCash);
    }

    [TestMethod]
    [DataRow(1, 1, 3, 12.4)]
    [DataRow(1, 2, 2, 12.4)]
    [DataRow(5, 1, 4, 12.4)]
    [DataRow(.1, 2.5, 6, 12.4)]
    [DataRow(1, 1, 3.33, 12.4)]
    public async Task _08_CreateLimitOrder_Long_TakeProfit_ShouldNotWorkAsync(double quantity, double lever, double price, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-18), Low = 10 - 1, Close = 10, High = 10 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-17), Low = 9 - 1, Close = 9, High = 9 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-16), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-15), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-14), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-13), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-12), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-11), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 9 - 1, Close = 9, High = 9 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low = 10 - 1, Close = 10, High = 10 + 1  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.Price = price;
        order.TakeProfitPrice = takeProfitPrice;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert
        var pendingOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, pendingOrders.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);
        Assert.IsTrue(openPosition.IsOpen());
        Assert.IsNotNull(openPosition.ExitOrders);
        Assert.AreEqual(0, openPosition.ExitOrders.Count);

        var margin = await cashManagement.GetMarginAsync();
        var expectedMarginAfterCandleRun = options.Value.InitialCash - (order.Quantity * order.ExecutedPrice!.Value) - order.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterCandleRun, 8), Math.Round(margin, 8));
    }

    [TestMethod]
    [DataRow(.1, 1, 1, 1)]
    [DataRow(.15, 6, 3, 3)]
    [DataRow(1.3, 1, 2, 1)]
    [DataRow(5, 2, 5, 5)]
    [DataRow(17, 1, 6, 5)]
    [DataRow(1, 3, 7, 7)]
    [DataRow(8, 2, 8, 5)]
    public async Task _09_CreateLimitOrder_Long_TakeProfit_ShouldThrowAsync(double quantity, double lever, double price, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-18), Low = 10 - 1, Close = 10, High = 10 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-17), Low = 9 - 1, Close = 9, High = 9 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-16), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-15), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-14), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-13), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-12), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-11), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 9 - 1, Close = 9, High = 9 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low = 10 - 1, Close = 10, High = 10 + 1  },
        ];


        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.Price = price;
        order.TakeProfitPrice = takeProfitPrice;

        var ex = await Assert.ThrowsExceptionAsync<OrderInvalidException>(async () => await exchange.PlaceOrderAsync(order));
        Assert.IsNotNull(ex);
        Assert.AreEqual("take profit price must be higher than limit order price", ex.Message);
    }

    [TestMethod]
    [DataRow(1, 1, 7, 6)]
    [DataRow(2, 2, 7, 6)]
    [DataRow(3, 1.5, 7, 6)]
    [DataRow(.1, 1, 8, 3)]
    [DataRow(16, 2, 5, 2)]
    [DataRow(16, 2, 5, .5)]
    public async Task _10_CreateLimitOrder_Short_TakeProfit_ShouldWorkAsync(double quantity, double lever, double price, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-17), Low = 1 - .5, Close = 1, High = 1 + .5  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-16), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-15), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-14), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-13), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-12), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-11), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 1 - .5, Close = 1, High = 1 + .5  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.Price = price;
        order.TakeProfitPrice = takeProfitPrice;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert
        var pendingOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, pendingOrders.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.IsNotNull(positions);
        Assert.AreEqual(1, positions.Count);

        var closedPosition = positions[0];
        Assert.IsNotNull(closedPosition);
        Assert.IsTrue(closedPosition.IsClosed());
        Assert.IsNotNull(closedPosition.ExitOrders);
        Assert.AreEqual(1, closedPosition.ExitOrders.Count);

        var closingOrder = closedPosition.ExitOrders[0];
        Assert.IsNotNull(closingOrder);
        Assert.IsTrue(closingOrder.IsFilled());
        Assert.AreEqual(order.TakeProfitPrice, closingOrder.ExecutedPrice);

        var margin = await cashManagement.GetMarginAsync();
        var diff = (closingOrder.ExecutedPrice!.Value * closingOrder.Quantity) - (order.Quantity * order.ExecutedPrice!.Value);
        var expectedMarginAfterCandleRun = options.Value.InitialCash + (-1) * diff * lever - order.ExecutedFee!.Value - closingOrder.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterCandleRun, 8), Math.Round(margin, 8));

        // Mit Gewinn
        Assert.IsTrue(closedPosition.RealizedPNL > 0);
        Assert.IsTrue(expectedMarginAfterCandleRun > options.Value.InitialCash);
    }

    [TestMethod]
    [DataRow(1, 1, 1, .45)]
    [DataRow(2, 3, 1, .45)]
    [DataRow(.1, 1.4, 1, .45)]
    [DataRow(.1, 1, 6, .45)]
    [DataRow(1, 10, 7, .499)]
    public async Task _11_CreateLimitOrder_Short_TakeProfit_ShouldNotWorkAsync(double quantity, double lever, double price, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-17), Low = 1 - .5, Close = 1, High = 1 + .5  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-16), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-15), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-14), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-13), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-12), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-11), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 1 - .5, Close = 1, High = 1 + .5  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.Price = price;
        order.TakeProfitPrice = takeProfitPrice;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert
        var pendingOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, pendingOrders.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);
        Assert.IsTrue(openPosition.IsOpen());
        Assert.IsNotNull(openPosition.ExitOrders);
        Assert.AreEqual(0, openPosition.ExitOrders.Count);

        var margin = await cashManagement.GetMarginAsync();
        var expectedMarginAfterCandleRun = options.Value.InitialCash - (order.Quantity * order.ExecutedPrice!.Value) - order.ExecutedFee!.Value;
        Assert.AreEqual(Math.Round(expectedMarginAfterCandleRun, 8), Math.Round(margin, 8));
    }


    [TestMethod]
    [DataRow(1, 1, 7, 8)]
    [DataRow(.1, 2, 8, 8)]
    [DataRow(15, 1, 4, 4)]
    [DataRow(12, 7, 5, 6)]
    public async Task _12_CreateLimitOrder_Short_TakeProfit_ShouldThrowAsync(double quantity, double lever, double price, double takeProfitPrice)
    {
        // Load Services
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-17), Low = 1 - .5, Close = 1, High = 1 + .5  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-16), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-15), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-14), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-13), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-12), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-11), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-10), Low = 8 - 1, Close = 8, High = 8 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-9), Low = 7 - 1, Close = 7, High = 7 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-8), Low = 6 - 1, Close = 6, High = 6 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-7), Low = 5 - 1, Close = 5, High = 5 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-6), Low = 4 - 1, Close = 4, High = 4 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-5), Low = 3 - 1, Close = 3, High = 3 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 2 - 1, Close = 2, High = 2 + 1  },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 1 - .5, Close = 1, High = 1 + .5  },
        ];

        exchange.SetCandle(inputCandles[0]);

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Lever = lever;
        order.Price = price;
        order.TakeProfitPrice = takeProfitPrice;

        var ex = await Assert.ThrowsExceptionAsync<OrderInvalidException>(async () => await exchange.PlaceOrderAsync(order));
        Assert.IsNotNull(ex);
        Assert.AreEqual(ex.Message, "take profit price must be lower than limit order price");
    }

}

