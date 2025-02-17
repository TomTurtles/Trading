namespace Marek.Trading.Tests;

[TestClass]
public sealed class _01_BacktestingTests_Orders
{
    private readonly IServiceProvider _serviceProvider = TestSettings.ServiceProvider;

    [TestMethod]
    public async Task _01_PlaceMarketOrder_Long_OrderManagement_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(CandleHelper.Random(DateTime.Now));

        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 1;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);
        Assert.IsFalse(getOrder.IsPending());
        Assert.IsFalse(getOrder.IsCancelled());
        Assert.IsTrue(getOrder.IsFilled());
        Assert.IsNotNull(getOrder.PlacedPrice);
        Assert.IsNotNull(getOrder.PlacedTime);
        Assert.IsNotNull(getOrder.ExecutedPrice);
        Assert.IsNotNull(getOrder.ExecutedTime);
        Assert.IsNotNull(getOrder.ExecutedValue);
        Assert.AreNotEqual(getOrder.ExecutedValue, getOrder.ExecutedPrice);

        var orders = await orderManagement.GetOrdersAsync();
        Assert.AreEqual(1, orders.Count);

        var openOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, openOrders.Count);
    }

    [TestMethod]
    public async Task _02_PlaceMarketOrder_Short_OrderManagement_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        exchange.SetCandle(CandleHelper.Random(DateTime.Now));

        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 1;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);
        Assert.IsFalse(getOrder.IsPending());
        Assert.IsFalse(getOrder.IsCancelled());
        Assert.IsTrue(getOrder.IsFilled());
        Assert.IsNotNull(getOrder.PlacedPrice);
        Assert.IsNotNull(getOrder.PlacedTime);
        Assert.IsNotNull(getOrder.ExecutedPrice);
        Assert.IsNotNull(getOrder.ExecutedTime);
        Assert.IsNotNull(getOrder.ExecutedValue);
        Assert.AreNotEqual(getOrder.ExecutedValue, getOrder.ExecutedPrice);

        var orders = await orderManagement.GetOrdersAsync();
        Assert.AreEqual(1, orders.Count);

        var openOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, openOrders.Count);
    }

    [TestMethod]
    public async Task _03_PlaceLimitOrder_Long_OrderManagement_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 1;
        order.Price = candle.Close * 0.8;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);
        Assert.IsTrue(getOrder.IsPending());
        Assert.IsFalse(getOrder.IsCancelled());
        Assert.IsFalse(getOrder.IsFilled());
        Assert.IsNotNull(getOrder.PlacedPrice);
        Assert.IsNotNull(getOrder.PlacedTime);
        Assert.IsNull(getOrder.ExecutedPrice);
        Assert.IsNull(getOrder.ExecutedTime);
        Assert.IsNull(getOrder.ExecutedValue);

        var orders = await orderManagement.GetOrdersAsync();
        Assert.AreEqual(1, orders.Count);

        var openOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(1, openOrders.Count);
    }

    [TestMethod]
    public async Task _04_PlaceLimitOrder_Short_OrderManagement_ShouldWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 1;
        order.Price = candle.Close * 1.05;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);
        Assert.IsTrue(getOrder.IsPending());
        Assert.IsFalse(getOrder.IsCancelled());
        Assert.IsFalse(getOrder.IsFilled());
        Assert.IsNotNull(getOrder.PlacedPrice);
        Assert.IsNotNull(getOrder.PlacedTime);
        Assert.IsNull(getOrder.ExecutedPrice);
        Assert.IsNull(getOrder.ExecutedTime);
        Assert.IsNull(getOrder.ExecutedValue);

        var orders = await orderManagement.GetOrdersAsync();
        Assert.AreEqual(1, orders.Count);

        var openOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(1, openOrders.Count);
    }

    [TestMethod]
    public async Task _05_PlaceLimitOrder_Long_OrderManagement_ShouldNotWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 1;
        order.Price = candle.Close * 1.1; // reason why it should not work (wrong price, executing as market order instead)

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);
        Assert.IsFalse(getOrder.IsPending());
        Assert.IsFalse(getOrder.IsCancelled());
        Assert.IsTrue(getOrder.IsFilled());
        Assert.IsNotNull(getOrder.PlacedPrice);
        Assert.IsNotNull(getOrder.PlacedTime);
        Assert.IsNotNull(getOrder.ExecutedPrice);
        Assert.IsNotNull(getOrder.ExecutedTime);
        Assert.IsNotNull(getOrder.ExecutedValue);
        Assert.AreNotEqual(order.Price, getOrder.ExecutedPrice);
        Assert.AreNotEqual(getOrder.PlacedPrice, getOrder.ExecutedPrice); // executed price is already corrected?
        Assert.IsTrue(getOrder.PlacedPrice >  getOrder.ExecutedPrice);
        Assert.AreNotEqual(getOrder.ExecutedValue, getOrder.ExecutedPrice); // executed value is reduced by fee

        var orders = await orderManagement.GetOrdersAsync();
        Assert.AreEqual(1, orders.Count);

        var openOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, openOrders.Count);
    }

    [TestMethod]
    public async Task _06_PlaceLimitOrder_Short_OrderManagement_ShouldNotWorkAsync()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 1;
        order.Price = candle.Close * 0.3;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);
        Assert.IsFalse(getOrder.IsPending());
        Assert.IsFalse(getOrder.IsCancelled());
        Assert.IsTrue(getOrder.IsFilled()); 
        Assert.IsNotNull(getOrder.PlacedPrice);
        Assert.IsNotNull(getOrder.PlacedTime);
        Assert.IsNotNull(getOrder.ExecutedPrice);
        Assert.IsNotNull(getOrder.ExecutedTime);
        Assert.IsNotNull(getOrder.ExecutedValue);
        Assert.AreNotEqual(order.Price, getOrder.ExecutedPrice);
        Assert.AreNotEqual(getOrder.PlacedPrice, getOrder.ExecutedPrice); // executed price is already corrected?
        Assert.IsTrue(getOrder.PlacedPrice < getOrder.ExecutedPrice);
        Assert.AreNotEqual(getOrder.ExecutedValue, getOrder.ExecutedPrice); // executed value is reduced by fee

        var orders = await orderManagement.GetOrdersAsync();
        Assert.AreEqual(1, orders.Count);

        var openOrders = await orderManagement.GetPendingOrdersAsync();
        Assert.AreEqual(0, openOrders.Count);
    }

    [TestMethod]
    public async Task _07_EstimateLimitOrderValue_Long_ShouldWork()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = 17;
        order.Price = candle.Close * 0.3;
        order.Lever = (new Random().NextDouble() + 1) * 3;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);

        Assert.AreEqual(order.Price * order.Quantity, getOrder.GetValue());
        Assert.AreEqual(order.PlacedPrice * order.Quantity, getOrder.GetValue());
        Assert.AreNotEqual(order.ExecutedPrice * order.Quantity, getOrder.GetValue());
    }

    [TestMethod]
    public async Task _08_EstimateLimitOrderValue_Short_ShouldWork()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = 2;
        order.Price = candle.Close * 1.3;
        order.Lever = (new Random().NextDouble() + 1) * 3;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);

        Assert.AreEqual(order.Price * order.Quantity, getOrder.GetValue());
        Assert.AreEqual(order.PlacedPrice * order.Quantity, getOrder.GetValue());
        Assert.AreNotEqual(order.ExecutedPrice * order.Quantity, getOrder.GetValue());
    }

    [TestMethod]
    public async Task _09_EstimateMarketOrderValue_Long_ShouldWork()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateLong("BTC_USDT");
        order.Quantity = new Random().Next(1, 2) * 4;
        order.Lever = (new Random().NextDouble() + 1) * 3;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);

        Assert.AreNotEqual(order.Price * order.Quantity, getOrder.GetValue());
        Assert.AreEqual(order.PlacedPrice * order.Quantity, getOrder.GetValue());
        Assert.AreEqual(order.ExecutedPrice * order.Quantity, getOrder.GetValue());
    }

    [TestMethod]
    public async Task _10_EstimateMarketOrderValue_Short_ShouldWork()
    {
        // Arrange
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var candle = CandleHelper.Random(DateTime.Now);
        exchange.SetCandle(candle);

        var order = Order.CreateShort("BTC_USDT");
        order.Quantity = new Random().Next(1, 2) * 4;
        order.Lever = (new Random().NextDouble() + 1) * 3;

        // Act
        await exchange.PlaceOrderAsync(order);

        // Assert
        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(orderManagement);

        var getOrder = await orderManagement.GetOrderAsync(order.Id);
        Assert.IsNotNull(getOrder);
        Assert.AreSame(order, getOrder);

        Assert.AreNotEqual(order.Price * order.Quantity, getOrder.GetValue());
        Assert.AreEqual(order.PlacedPrice * order.Quantity, getOrder.GetValue());
        Assert.AreEqual(order.ExecutedPrice * order.Quantity, getOrder.GetValue());
    }
}

