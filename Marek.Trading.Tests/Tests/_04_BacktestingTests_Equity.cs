namespace Marek.Trading.Backtesting.Tests;

[TestClass]
public sealed class _04_BacktestingTests_Equity
{
    private readonly IServiceProvider _serviceProvider = TestSettings.ServiceProvider;

    [TestMethod]
    public async Task _01_CreateMarketOrder_Long_Equity_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 3;

        // Market Order
        await exchange.PlaceOrderAsync(order);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.IsNotNull(positions);
        Assert.AreNotEqual(0, positions.Count);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);

        // Wenn ich eine Market Order platziere und bisher keine weitere Candle aufgetaucht ist,
        // dann müsste ich "nur" equity über das fee verloren haben
        var equity = await exchange.GetEquityAsync();
        var expectedEquity = options.Value.InitialCash - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(expectedEquity, 6), Math.Round(equity, 6));
    }

    [TestMethod]
    public async Task _02_CreateMarketOrder_Short_Equity_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 7;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.IsNotNull(positions);
        Assert.AreNotEqual(0, positions.Count);

        // Wenn ich eine Market Order platziere und bisher keine weitere Candle aufgetaucht ist,
        // dann müsste ich "nur" equity über das fee verloren haben
        var equity = await exchange.GetEquityAsync();
        var expectedEquity = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(expectedEquity, 6), Math.Round(equity, 6));
    }

    [TestMethod]
    public async Task _03_CreateLimitOrder_Long_Equity_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 4;
        order.Price = candle.Close * .9;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Wenn die Order noch nicht ausgeführt ist, und wir uns noch bei der gleichen candle befinden
        // dann sollte die equity sich nicht verändert haben (keine gebühren)
        var equity = await exchange.GetEquityAsync();
        var expectedEquity = options.Value.InitialCash;
        Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));
    }

    [TestMethod]
    public async Task _04_CreateLimitOrder_Short_Equity_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 3;
        order.Price = candle.Close * 1.02;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Wenn die Order noch nicht ausgeführt ist, und wir uns noch bei der gleichen candle befinden
        // dann sollte die equity sich nicht verändert haben (keine gebühren)
        var equity = await exchange.GetEquityAsync();
        var expectedEquity = options.Value.InitialCash;
        Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));
    }


    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 1)]
    [DataRow(1, 2)]
    [DataRow(2, 3)]
    [DataRow(4, 3.25)]
    [DataRow(0.5, 6.11)]
    [DataRow(1.2, 0.8)]
    public async Task _05_ClosePositionMarketOrder_Long_Equity_ShouldWorkAsync(double quantity, double lever)
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 4, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 4, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low = 3, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), Low = 4, Close = 6 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Price = 3.5;
        order.Lever = lever;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Closing Market Order
        var closingOrder = Order.CreateShort("BTC_USDT");
        closingOrder.Quantity = order.Quantity;
        closingOrder.Lever = order.Lever;
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert
        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        // Wenn die Position bereits geschlossen wurde, dann sollte die Equity so groß sein, wie das Anfangscash + die realisierten Gewinne - den Gebühren
        var equity = await exchange.GetEquityAsync();
        var expectedEquity = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));


    }


    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 1)]
    [DataRow(1, 2)]
    [DataRow(2, 3)]
    [DataRow(4, 3.25)]
    [DataRow(0.5, 6.11)]
    [DataRow(1.2, 0.8)]
    public async Task _06_ClosePositionMarketOrder_Short_Equity_ShouldWorkAsync(double quantity, double lever)
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), High = 5, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), High = 5, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), High = 7, Close = 6 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), High = 3, Close = 3 },
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Price = 5.5;
        order.Lever = lever;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Closing Market Order
        var closingOrder = Order.CreateLong("BTC_USDT");
        closingOrder.Quantity = order.Quantity;
        closingOrder.Lever = order.Lever;
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert
        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        // Wenn die Position bereits geschlossen wurde, dann sollte die Equity so groß sein, wie das Anfangscash + die realisierten Gewinne - den Gebühren
        var equity = await exchange.GetEquityAsync();
        var expectedEquity = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 1)]
    [DataRow(1, 2)]
    [DataRow(2, 3)]
    [DataRow(4, 3.25)]
    [DataRow(0.5, 6.11)]
    [DataRow(1.2, 0.8)]
    public async Task _07_ClosePositionMarketOrder_Long_NotProfitable_ShouldWorkAsync(double quantity, double lever)
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 6, Close = 7 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 5, Close = 6 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low = 4, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), Low = 4, Close = 5 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Price = 6;
        order.Lever = lever;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Closing Market Order
        var closingOrder = Order.CreateShort("BTC_USDT");
        closingOrder.Quantity = order.Quantity;
        closingOrder.Lever = order.Lever;
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert Position
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        // Assert Cash
        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Wenn die Position bereits geschlossen wurde, dann sollte die Equity so groß sein, wie das Anfangscash + die realisierten Gewinne - den Gebühren
        var equity = await exchange.GetEquityAsync();
        var expectedEquity = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 1)]
    [DataRow(1, 2)]
    [DataRow(2, 3)]
    [DataRow(4, 3.25)]
    [DataRow(0.5, 6.11)]
    [DataRow(1.2, 0.8)]
    public async Task _08_ClosePositionMarketOrder_Short_NotProfitable_ShouldWorkAsync(double quantity, double lever)
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), High = 5, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), High = 5, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), High = 7, Close = 6 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), High = 8, Close = 7 },
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Price = 6;
        order.Lever = lever;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Closing Market Order
        var closingOrder = Order.CreateLong("BTC_USDT");
        closingOrder.Quantity = order.Quantity;
        closingOrder.Lever = order.Lever;
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert Position
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        // Assert Cash
        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        // Wenn die Position bereits geschlossen wurde, dann sollte die Equity so groß sein, wie das Anfangscash + die realisierten Gewinne - den Gebühren
        var equity = await exchange.GetEquityAsync();
        var expectedEquity = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));
    }

    [TestMethod]
    [DataRow(1, 1)]
    //[DataRow(2, 1)]
    //[DataRow(1, 2)]
    //[DataRow(2, 3)]
    //[DataRow(4, 3.25)]
    //[DataRow(0.5, 6.11)]
    //[DataRow(1.2, 0.8)]
    public async Task _09_ClosePositionPartially_Long_ShouldWorkAsync(double quantity, double lever)
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 6, Close = 7 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 5, Close = 6 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low = 4, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), Low = 4, Close = 5 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = quantity;
        order.Price = 6;
        order.Lever = lever;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Closing Market Order
        var closingOrder = Order.CreateShort("BTC_USDT");
        closingOrder.Quantity = order.Quantity * .4;
        closingOrder.Lever = order.Lever;
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var marketPrice = await exchange.GetMarketPriceAsync();
        var equity = await exchange.GetEquityAsync();

        // Bei teilweise geschlossenen Positionen gibt es einen realisierten Gewinn und einen nicht realisierten Gewinn. Diese wirken auf die Equity ein.
        var realized = positions.Sum(p => p.RealizedPNL);
        var unrealized = positions.Sum(p => p.GetUnrealizedPNL(marketPrice));
        var fee = positions.Sum(p => p.Fee);
        var expectedEquity = options.Value.InitialCash + realized + unrealized - fee;
        Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));
    }

    [TestMethod]
    [DataRow(1, 1)]
    //[DataRow(2, 1)]
    //[DataRow(1, 2)]
    //[DataRow(2, 3)]
    //[DataRow(4, 3.25)]
    //[DataRow(0.5, 6.11)]
    //[DataRow(1.2, 0.8)]
    public async Task _10_ClosePositionPartially_Short_ShouldWorkAsync(double quantity, double lever)
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), High = 5, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), High = 5, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), High = 7, Close = 6 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), High = 8, Close = 7 },
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = quantity;
        order.Price = 6;
        order.Lever = lever;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Closing Market Order
        var closingOrder = Order.CreateLong("BTC_USDT");
        closingOrder.Quantity = order.Quantity * .6;
        closingOrder.Lever = order.Lever;
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var marketPrice = await exchange.GetMarketPriceAsync();
        var equity = await exchange.GetEquityAsync();

        // Bei teilweise geschlossenen Positionen gibt es einen realisierten Gewinn und einen nicht realisierten Gewinn. Diese wirken auf die Equity ein.
        var realized = positions.Sum(p => p.RealizedPNL);
        var unrealized = positions.Sum(p => p.GetUnrealizedPNL(marketPrice));
        var fee = positions.Sum(p => p.Fee);
        var expectedEquity = options.Value.InitialCash + realized + unrealized - fee;
        Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));
    }
}

