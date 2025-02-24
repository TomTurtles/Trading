namespace Marek.Trading.Backtesting.Tests;

[TestClass]
public sealed class _02_BacktestingTests_Positions
{
    private readonly IServiceProvider _serviceProvider = TestSettings.ServiceProvider;

    [TestMethod]
    public async Task _01_CreatePositionByMarketOrder_Long_PositionManagement_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(CandleHelper.Random(DateTime.Now));

        // Market Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 1;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var position = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(position);
        Assert.AreEqual(PositionSide.LONG, position.Side);
        Assert.AreEqual(order.Symbol, position.Symbol);
        Assert.AreEqual(order.Quantity, position.Quantity);
        Assert.AreEqual(PositionStatus.Open, position.Status);
        Assert.IsTrue(position.IsOpen());
        Assert.IsFalse(position.IsClosed());
        Assert.IsNull(position.ExitPrice);
        Assert.AreEqual(order.Quantity, position.Quantity);
        Assert.IsTrue(position.EntryOrders.Any());
        Assert.IsFalse(position.ExitOrders.Any());
        Assert.AreNotEqual(position.EntryOrders.Count, position.ExitOrders.Count);
        Assert.AreEqual(position.EntryOrders[0].Id, order.Id);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);
    }

    [TestMethod]
    public async Task _02_CreatePositionByMarketOrder_Short_PositionManagement_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(CandleHelper.Random(DateTime.Now));

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 1;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var position = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(position);
        Assert.AreEqual(PositionSide.SHORT, position.Side);
        Assert.AreEqual(order.Symbol, position.Symbol);
        Assert.AreEqual(1, position.Quantity);
        Assert.AreEqual(PositionStatus.Open, position.Status);
        Assert.IsTrue(position.IsOpen());
        Assert.IsFalse(position.IsClosed());
        Assert.IsNull(position.ExitPrice);
        Assert.AreEqual(order.Quantity, position.Quantity);
        Assert.IsTrue(position.EntryOrders.Any());
        Assert.IsFalse(position.ExitOrders.Any());
        Assert.AreNotEqual(position.EntryOrders.Count, position.ExitOrders.Count);
        Assert.AreEqual(position.EntryOrders[0].Id, order.Id);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);
    }

    [TestMethod]
    public async Task _03_CreatePositionByLimitOrder_Long_PositionManagement_ShouldWorkAsync()
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low=5, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low=4, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low=3, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), Low=4, Close = 4 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 3;
        order.Price = 3.5;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert Order
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreEqual(OrderStatus.Filled, getOrder.Status);
        Assert.AreEqual(inputCandles[2].Timestamp, getOrder.ExecutedTime);
        Assert.AreEqual(order.Price, getOrder.ExecutedPrice);

        // Assert Position
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var position = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(position);
        Assert.AreEqual(PositionSide.LONG, position.Side);
        Assert.AreEqual(order.Symbol, position.Symbol);
        Assert.AreEqual(order.Quantity, position.Quantity);
        Assert.AreEqual(PositionStatus.Open, position.Status);
        Assert.AreEqual(order.Price, position.EntryPrice);
        Assert.AreEqual(order.Quantity * order.Price, position.GetValue(position.EntryPrice));
        Assert.IsTrue(position.IsOpen());
        Assert.IsFalse(position.IsClosed());
        Assert.IsNull(position.ExitPrice);
        Assert.AreEqual(order.Quantity, position.Quantity);
        Assert.IsTrue(position.EntryOrders.Any());
        Assert.IsFalse(position.ExitOrders.Any());
        Assert.AreNotEqual(position.EntryOrders.Count, position.ExitOrders.Count);
        Assert.AreEqual(position.EntryOrders[0].Id, order.Id);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);
    }

    [TestMethod]
    public async Task _04_CreatePositionByLimitOrder_Short_PositionManagement_ShouldWorkAsync()
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), High=3, Close = 3 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), High=4, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), High=5, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), High=6, Close = 6 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 2;
        order.Price = 4.5;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert Order
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreEqual(OrderStatus.Filled, getOrder.Status);
        Assert.AreEqual(inputCandles[2].Timestamp, getOrder.ExecutedTime);
        Assert.AreEqual(order.Price, getOrder.ExecutedPrice);

        // Assert Position
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var position = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(position);
        Assert.AreEqual(PositionSide.SHORT, position.Side);
        Assert.AreEqual(order.Symbol, position.Symbol);
        Assert.AreEqual(order.Quantity, position.Quantity);
        Assert.AreEqual(PositionStatus.Open, position.Status);
        Assert.AreEqual(order.Price, position.EntryPrice);
        Assert.IsTrue(position.IsOpen());
        Assert.IsFalse(position.IsClosed());
        Assert.IsNull(position.ExitPrice);
        Assert.AreEqual(order.Quantity, position.Quantity);
        Assert.IsTrue(position.EntryOrders.Any());
        Assert.IsFalse(position.ExitOrders.Any());
        Assert.AreNotEqual(position.EntryOrders.Count, position.ExitOrders.Count);
        Assert.AreEqual(position.EntryOrders[0].Id, order.Id);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);
    }


    [TestMethod]
    public async Task _05_ClosePositionMarketOrder_Long_PositionManagement_ShouldWorkAsync()
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low=5, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low=4, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low=3, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), Low=4, Close = 4 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 3;
        order.Price = 3.5;
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
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert Order
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreEqual(OrderStatus.Filled, getOrder.Status);
        Assert.AreEqual(inputCandles[2].Timestamp, getOrder.ExecutedTime);
        Assert.AreEqual(order.Price, getOrder.ExecutedPrice);

        // Assert Position
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        var position = positions[0];
        Assert.IsTrue(position.IsClosed());
        Assert.IsFalse(position.IsOpen());
        Assert.IsNotNull(position.ExitPrice);
        Assert.AreEqual(position.ExitQuantity, position.EntryQuantity);
        Assert.IsTrue(position.EntryOrders.Any());
        Assert.IsTrue(position.ExitOrders.Any());
        Assert.AreEqual(position.EntryOrders.Count, position.ExitOrders.Count);
        Assert.AreEqual(position.EntryOrders[0].Id, order.Id);
        Assert.AreEqual(position.ExitOrders[0].Id, closingOrder.Id);
    }

    [TestMethod]
    public async Task _06_ClosePositionMarketOrder_Short_PositionManagement_ShouldWorkAsync()
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), High=3, Close = 3 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), High=4, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), High=5, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), High=6, Close = 6 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 3;
        order.Price = 4.5;
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
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert Order
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreEqual(OrderStatus.Filled, getOrder.Status);
        Assert.AreEqual(inputCandles[2].Timestamp, getOrder.ExecutedTime);
        Assert.AreEqual(order.Price, getOrder.ExecutedPrice);

        // Assert Position
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        var position = positions[0];
        Assert.IsTrue(position.IsClosed());
        Assert.IsFalse(position.IsOpen());
        Assert.IsNotNull(position.ExitPrice);
        Assert.AreEqual(position.ExitQuantity, position.EntryQuantity);
        Assert.IsTrue(position.EntryOrders.Any());
        Assert.IsTrue(position.ExitOrders.Any());
        Assert.AreEqual(position.EntryOrders.Count, position.ExitOrders.Count);
        Assert.AreEqual(position.EntryOrders[0].Id, order.Id);
        Assert.AreEqual(position.ExitOrders[0].Id, closingOrder.Id);
    }

    [TestMethod]
    public async Task _07_ClosePositionMarketOrder_Long_PositionManagement_ShouldNotWorkAsync()
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low=5, Close = 5 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low=4, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low=3, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), Low=4, Close = 4 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 3;
        order.Price = 3.5;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Closing Market Order
        var closingOrder = Order.CreateShort("BTC_USDT");
        closingOrder.Quantity = order.Quantity - 1; // The reason why it should not work
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert Order
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreEqual(OrderStatus.Filled, getOrder.Status);
        Assert.AreEqual(inputCandles[2].Timestamp, getOrder.ExecutedTime);
        Assert.AreEqual(order.Price, getOrder.ExecutedPrice);

        // Assert Position
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        var position = positions[0];
        Assert.IsFalse(position.IsClosed());
        Assert.IsTrue(position.IsOpen());
        Assert.IsNotNull(position.ExitPrice);
        Assert.AreNotEqual(position.ExitQuantity, position.EntryQuantity);
        Assert.IsTrue(position.EntryOrders.Any());
        Assert.IsTrue(position.ExitOrders.Any());
        Assert.AreEqual(position.EntryOrders.Count, position.ExitOrders.Count);
        Assert.AreNotEqual(position.EntryQuantity, position.ExitQuantity);
        Assert.AreEqual(position.EntryOrders[0].Id, order.Id);
        Assert.AreEqual(position.ExitOrders[0].Id, closingOrder.Id);
    }


    [TestMethod]
    public async Task _08_ClosePositionMarketOrder_Short_PositionManagement_ShouldNotWorkAsync()
    {
        // Alle Candles
        Candle[] inputCandles = [
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), High=3, Close = 3 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), High=4, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), High=5, Close = 4 },
            new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), High=6, Close = 6 }
        ];

        // Exchange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(inputCandles[0]);

        // Limit Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 3;
        order.Price = 4.5;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Closing Market Order
        var closingOrder = Order.CreateLong("BTC_USDT");
        closingOrder.Quantity = order.Quantity - 1; // The reason why it should not work
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert Order
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreEqual(OrderStatus.Filled, getOrder.Status);
        Assert.AreEqual(inputCandles[2].Timestamp, getOrder.ExecutedTime);
        Assert.AreEqual(order.Price, getOrder.ExecutedPrice);

        // Assert Position
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(openPosition);

        var positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        var position = positions[0];
        Assert.IsFalse(position.IsClosed());
        Assert.IsTrue(position.IsOpen());
        Assert.IsNotNull(position.ExitPrice);
        Assert.AreNotEqual(position.ExitQuantity, position.EntryQuantity);
        Assert.IsTrue(position.EntryOrders.Any());
        Assert.IsTrue(position.ExitOrders.Any());
        Assert.AreEqual(position.EntryOrders.Count, position.ExitOrders.Count);
        Assert.AreNotEqual(position.EntryQuantity, position.ExitQuantity);
        Assert.AreEqual(position.EntryOrders[0].Id, order.Id);
        Assert.AreEqual(position.ExitOrders[0].Id, closingOrder.Id);
    }

    [TestMethod]
    public async Task _09_EstimatePNL_MarketOrder_Long_ShouldWork()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        // Market Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = (new Random().NextDouble() + 1) * 5;
        order.Lever = (new Random().NextDouble() + 1) * 3;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var position = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(position);

        var newHigherMarketPrice = candle.Close * 1.4;
        var expectedPositivePNL = (newHigherMarketPrice - candle.Close) * order.Lever * order.Quantity;
        Assert.AreEqual(Math.Round(expectedPositivePNL, 8), Math.Round(position.GetUnrealizedPNL(newHigherMarketPrice, order.Quantity), 8));
        Assert.IsTrue(position.GetUnrealizedPNL(newHigherMarketPrice, order.Quantity) > 0);

        var newLowerMarketPrice = candle.Close * 0.95;
        var expectedNegativePNL = (newLowerMarketPrice - candle.Close) * order.Lever * order.Quantity;
        Assert.AreEqual(Math.Round(expectedNegativePNL, 8), Math.Round(position.GetUnrealizedPNL(newLowerMarketPrice, order.Quantity), 8));
        Assert.IsTrue(position.GetUnrealizedPNL(newLowerMarketPrice, order.Quantity) < 0);

        var newEqualMarketPrice = candle.Close * 1;
        Assert.AreEqual(Math.Round(position.GetUnrealizedPNL(newEqualMarketPrice), 8), 0);
    }

    [TestMethod]
    public async Task _10_EstimatePNL_MarketOrder_Short_ShouldWork()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = (new Random().NextDouble() + 1) * 5;
        order.Lever = (new Random().NextDouble() + 1) * 3;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var position = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(position);

        var newHigherMarketPrice = candle.Close * 1.4;
        var expectedPositivePNL = (candle.Close - newHigherMarketPrice) * order.Lever * order.Quantity;
        Assert.AreEqual(Math.Round(expectedPositivePNL, 8), Math.Round(position.GetUnrealizedPNL(newHigherMarketPrice, order.Quantity), 8));
        Assert.IsTrue(position.GetUnrealizedPNL(newHigherMarketPrice, order.Quantity) < 0);

        var newLowerMarketPrice = candle.Close * 0.95;
        var expectedNegativePNL = (candle.Close - newLowerMarketPrice) * order.Lever * order.Quantity;
        Assert.AreEqual(Math.Round(expectedNegativePNL, 8), Math.Round(position.GetUnrealizedPNL(newLowerMarketPrice, order.Quantity), 8));
        Assert.IsTrue(position.GetUnrealizedPNL(newLowerMarketPrice, order.Quantity) > 0);

        var newEqualMarketPrice = candle.Close * 1;
        Assert.AreEqual(Math.Round(position.GetUnrealizedPNL(newEqualMarketPrice), 8), 0);
    }

    [TestMethod]
    public async Task _11_EstimatePNL_PartiallyclosedPosition_Long_ShouldWork()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        // Candle 1
        var candle1 = CandleHelper.Random(DateTime.Now.AddMinutes(-1));
        exchange.SetCandle(candle1);

        // Market Order
        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 4;
        order.Lever = 2;

        // Act 1
        await exchange.PlaceOrderAsync(order);

        // Candle 2
        var candle2 = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle2);

        // Partially Closing Market Order
        var closingOrder = Order.CreateShort("BTC_USDT");
        closingOrder.Quantity = 1.5;

        // Act 2
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var position = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(position);

        // Assert Quantity
        Assert.AreEqual(order.Quantity, position.EntryQuantity);
        Assert.AreEqual(closingOrder.Quantity, position.ExitQuantity);

        // Assert Value
        var expectedPNL = (candle2.Close - candle1.Close) * closingOrder.Lever * closingOrder.Quantity;
        Assert.AreEqual(Math.Round(expectedPNL, 8), Math.Round(position.RealizedPNL, 8));
    }

    [TestMethod]
    public async Task _12_EstimatePNL_PartiallyclosedPosition_Short_ShouldWork()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        // Candle 1
        var candle1 = CandleHelper.Random(DateTime.Now.AddMinutes(-1));
        exchange.SetCandle(candle1);

        // Market Order
        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = (new Random().NextDouble() + 1) * 2;
        order.Lever = (new Random().NextDouble() + 1) * 3;

        // Act 1
        await exchange.PlaceOrderAsync(order);

        // Candle 2
        var candle2 = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle2);

        // Partially Closing Market Order
        var closingFactor = .6;
        var closingOrder = Order.CreateLong("BTC_USDT");
        closingOrder.Quantity = order.Quantity * closingFactor;

        // Act 2
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert
        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var position = await positionManagement.GetOpenPositionAsync();
        Assert.IsNotNull(position);

        // Assert Quantity
        Assert.AreEqual(order.Quantity, position.EntryQuantity);
        Assert.AreEqual(closingOrder.Quantity, position.ExitQuantity);

        // Assert Value
        var expectedPNL = (candle1.Close - candle2.Close) * closingOrder.Lever * closingOrder.Quantity;
        Assert.AreEqual(Math.Round(expectedPNL, 8), Math.Round(position.RealizedPNL, 8));
    }
}

