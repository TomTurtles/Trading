
namespace Marek.Trading.Tests;

[TestClass]
public sealed class _03_BacktestingTests_Cash
{
    private readonly IServiceProvider _serviceProvider = TestSettings.ServiceProvider;

    [TestMethod]
    public async Task _01_CreateMarketOrder_Long_CashManagement_ShouldWorkAsync()
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
        Assert.AreEqual(candle.Close, order.ExecutedPrice);
        Assert.AreEqual(candle.Timestamp, order.ExecutedTime);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var cashList = cashManagement.GetCashStateList();
        Assert.IsTrue(cashList.Any());

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var margin = await cashManagement.GetMarginAsync();
        var expectedMargin = options.Value.InitialCash - order.GetValue() - order.ExecutedFee;
        Assert.AreEqual(Math.Round(expectedMargin!.Value, 6), Math.Round(margin, 6));
    }

    [TestMethod]
    public async Task _02_CreateMarketOrder_Short_CashManagement_ShouldWorkAsync()
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
        Assert.AreEqual(candle.Close, order.ExecutedPrice);
        Assert.AreEqual(candle.Timestamp, order.ExecutedTime);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var cashList = cashManagement.GetCashStateList();
        Assert.IsTrue(cashList.Any());

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var margin = await cashManagement.GetMarginAsync();
        var expectedMargin = options.Value.InitialCash - order.GetValue() - order.ExecutedFee;
        Assert.AreEqual(Math.Round(expectedMargin!.Value, 6), Math.Round(margin, 6));
    }

    [TestMethod]
    public async Task _03_CreateLimitOrder_Long_CashManagement_ShouldWorkAsync()
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
        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var cashList = cashManagement.GetCashStateList();
        Assert.IsTrue(cashList.Any());

        var margin = await cashManagement.GetMarginAsync();
        var expectedMargin = options.Value.InitialCash - order.GetValue();
        Assert.AreEqual(Math.Round(expectedMargin, 8), Math.Round(margin, 8));
    }

    [TestMethod]
    public async Task _04_CreateLimitOrder_Short_CashManagement_ShouldWorkAsync()
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
        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var cashList = cashManagement.GetCashStateList();
        Assert.IsTrue(cashList.Any());

        var margin = await cashManagement.GetMarginAsync();
        var expectedMargin = options.Value.InitialCash - order.GetValue();
        Assert.AreEqual(Math.Round(expectedMargin, 8), Math.Round(margin, 8));
    }


    [TestMethod]
    public async Task _05_ClosePositionMarketOrder_Long_CashManagement_ShouldWorkAsync()
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
        order.Quantity = 1;
        order.Price = 3.5;
        order.Lever = 1;
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

        // Assert
        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(cashManagement);

        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var cashList = cashManagement.GetCashStateList();
        Assert.IsTrue(cashList.Any());

        // Keine Pending Orders mehr vorhanden, keine offenen Positionen mehr vorhanden
        // In diesem Fall muss Equity = Cash sein
        var equity = await exchange.GetEquityAsync();
        var margin = await cashManagement.GetMarginAsync();
        var expectedMargin = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(equity, 8), Math.Round(margin, 8));
        Assert.AreEqual(Math.Round(expectedMargin, 8), Math.Round(margin, 8));
    }


    [TestMethod]
    public async Task _06_ClosePositionMarketOrder_Short_CashManagement_ShouldWorkAsync()
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
        order.Quantity = (new Random().NextDouble() + 1) * 4;
        order.Price = 5.5;
        order.Lever = (new Random().NextDouble() + 1) * 4;
        await exchange.PlaceOrderAsync(order);

        // Run Candles
        foreach (var candle in inputCandles)
        {
            exchange.SetCandle(candle);
            await exchange.RunAsync();
        }

        // Assert first Limit Order
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

        var margin = await cashManagement.GetMarginAsync();
        var expectedCashAfterFirstExecutedOrder = options.Value.InitialCash - order.GetValue() - order.ExecutedFee;
        Assert.AreEqual(expectedCashAfterFirstExecutedOrder, margin);

        // Closing Market Order
        var closingOrder = Order.CreateLong("BTC_USDT");
        closingOrder.Quantity = order.Quantity;
        closingOrder.Lever = order.Lever;
        await exchange.PlaceOrderAsync(closingOrder);

        // Assert Position after Market Order
        openPosition = await positionManagement.GetOpenPositionAsync();
        Assert.IsNull(openPosition);

        positions = await positionManagement.GetPositionsAsync();
        Assert.AreEqual(1, positions.Count);

        // Keine Pending Orders mehr vorhanden, keine offenen Positionen mehr vorhanden
        // In diesem Fall muss Equity = Cash sein
        var equity = await exchange.GetEquityAsync();
        margin = await cashManagement.GetMarginAsync();
        var expectedMargin = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(equity, 8), Math.Round(margin, 8));
        Assert.AreEqual(Math.Round(expectedMargin, 8), Math.Round(margin, 8));
    }

    [TestMethod]
    public async Task _07_ClosePositionMarketOrder_Long_NotProfitable_ShouldWorkAsync()
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
        order.Quantity = 1;
        order.Price = 6;
        order.Lever = 1;
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

        // Keine Pending Orders mehr vorhanden, keine offenen Positionen mehr vorhanden
        // In diesem Fall muss Equity = Cash sein
        var equity = await exchange.GetEquityAsync();
        var margin = await cashManagement.GetMarginAsync();
        var expectedMargin = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(equity, 8), Math.Round(margin, 8));
        Assert.AreEqual(Math.Round(expectedMargin, 8), Math.Round(margin, 8));
    }

    [TestMethod]
    public async Task _08_ClosePositionMarketOrder_Short_NotProfitable_ShouldWorkAsync()
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
        order.Quantity = 1;
        order.Price = 6;
        order.Lever = 1;
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

        // Keine Pending Orders mehr vorhanden, keine offenen Positionen mehr vorhanden
        // In diesem Fall muss Equity = Cash sein
        var equity = await exchange.GetEquityAsync();
        var margin = await cashManagement.GetMarginAsync();
        var expectedMargin = options.Value.InitialCash + positions.Sum(p => p.RealizedPNL) - positions.Sum(p => p.Fee);
        Assert.AreEqual(Math.Round(equity, 8), Math.Round(margin, 8));
        Assert.AreEqual(Math.Round(expectedMargin, 8), Math.Round(margin, 8));
    }
}

