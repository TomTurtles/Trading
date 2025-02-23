namespace Marek.Trading.Tests;

[TestClass]
public sealed class _05_BacktestingTests_PerformanceResults
{
    private readonly IServiceProvider _serviceProvider = TestSettings.ServiceProvider;

    [TestMethod]
    public async Task _01_CreateMarketOrder_Long_BacktestingPerformanceResults_ShouldWorkAsync()
    {
        // Arrange
        var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
        Assert.IsNotNull(options);

        var engine = _serviceProvider.GetService<IBacktestingEngine>();
        Assert.IsNotNull(engine);

        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        Assert.IsNotNull(exchange);

        var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
        Assert.IsNotNull(positionManagement);

        var orderManagement = _serviceProvider.GetService<IBacktestingOrderManagement>();
        Assert.IsNotNull(positionManagement);

        var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
        Assert.IsNotNull(positionManagement);

        var assertionExecuted = false;
        string? exceptionMessage = null;
        engine.OnBacktestingFinished += (async (s, e) =>
        {
            try
            {
                // Assert im Event selbst
                Assert.IsNotNull(e);

                var positions = await positionManagement.GetPositionsAsync();
                Assert.IsNotNull(positions);
                Assert.AreNotEqual(0, positions.Count);

                var marketPrice = await exchange.GetMarketPriceAsync();

                var positionEquity = positions.Sum(p => (p.IsOpen ? p.GetUnrealizedPNL(marketPrice) : 0) + p.RealizedPNL);
                var totalFee = positions.Sum(p => p.Fee);

                var expectedEquity = options.Value.InitialCash + positionEquity - totalFee;
                var equity = await exchange.GetEquityAsync();
                Assert.AreEqual(Math.Round(expectedEquity, 8), Math.Round(equity, 8));

            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
                Debug.WriteLine(exceptionMessage);  
            }
            finally
            {
                assertionExecuted = true;
            }
        });

        // Market Order
        await engine.RunAsync();

        while (!assertionExecuted) await Task.Delay(1000);

        Assert.IsTrue(assertionExecuted);
        Assert.IsNull(exceptionMessage);
    }

    //[TestMethod]
    //public async Task _02_CreateMarketOrder_Short_CashManagement_ShouldWorkAsync()
    //{
    //    // Arrange
    //    var exchange = _serviceProvider.GetService<IBacktestingExchange>();
    //    Assert.IsNotNull(exchange);

    //    var candle = CandleHelper.Random(DateTime.Now);
    //    exchange.SetCandle(candle);

    //    var order = Order.CreateShort("BTC_USDT");
    //    order.Quantity = 7;

    //    // Act
    //    await exchange.PlaceOrderAsync(order);

    //    // Assert
    //    var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
    //    Assert.IsNotNull(cashManagement);

    //    var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
    //    Assert.IsNotNull(options);

    //    var margin = await cashManagement.GetMarginAsync();
    //    Assert.AreNotEqual(options.Value.InitialCash, margin);
    //    Assert.IsTrue(options.Value.InitialCash > margin);
    //    Assert.AreEqual(candle.Close, order.ExecutedPrice);
    //    Assert.AreEqual(candle.Timestamp, order.ExecutedTime);
    //    Assert.IsTrue(options.Value.InitialCash.IsPriceNear(margin + (candle.Close * order.Quantity) + order.ExecutedFee, 0.0001));

    //    var cashList = cashManagement.GetCashStateList();
    //    Assert.IsTrue(cashList.Any());
    //}

    //[TestMethod]
    //public async Task _03_CreateLimitOrder_Long_CashManagement_ShouldWorkAsync()
    //{
    //    // Arrange
    //    var exchange = _serviceProvider.GetService<IBacktestingExchange>();
    //    Assert.IsNotNull(exchange);

    //    var candle = CandleHelper.Random(DateTime.Now);
    //    exchange.SetCandle(candle);

    //    var order = Order.CreateLong("BTC_USDT");
    //    order.Quantity = 4;
    //    order.Price = candle.Close * .9;

    //    // Act
    //    await exchange.PlaceOrderAsync(order);

    //    // Assert
    //    var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
    //    Assert.IsNotNull(cashManagement);

    //    var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
    //    Assert.IsNotNull(options);

    //    var margin = await cashManagement.GetMarginAsync();
    //    Assert.AreNotEqual(options.Value.InitialCash, margin);
    //    Assert.IsTrue(options.Value.InitialCash > margin);
    //    Assert.AreNotEqual(options.Value.InitialCash, margin + (candle.Close * order.Quantity));
    //    Assert.AreEqual(options.Value.InitialCash, margin + (order.Price * order.Quantity));

    //    var cashList = cashManagement.GetCashStateList();
    //    Assert.IsTrue(cashList.Any());
    //}

    //[TestMethod]
    //public async Task _04_CreateLimitOrder_Short_CashManagement_ShouldWorkAsync()
    //{
    //    // Arrange
    //    var exchange = _serviceProvider.GetService<IBacktestingExchange>();
    //    Assert.IsNotNull(exchange);

    //    var candle = CandleHelper.Random(DateTime.Now);
    //    exchange.SetCandle(candle);

    //    var order = Order.CreateShort("BTC_USDT");
    //    order.Quantity = 3;
    //    order.Price = candle.Close * 1.02;

    //    // Act
    //    await exchange.PlaceOrderAsync(order);

    //    // Assert
    //    var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
    //    Assert.IsNotNull(cashManagement);

    //    var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
    //    Assert.IsNotNull(options);

    //    var margin = await cashManagement.GetMarginAsync();
    //    Assert.AreNotEqual(options.Value.InitialCash, margin);
    //    Assert.IsTrue(options.Value.InitialCash > margin);
    //    Assert.AreNotEqual(options.Value.InitialCash, margin + (candle.Close * order.Quantity));
    //    Assert.AreEqual(options.Value.InitialCash, margin + (order.Price * order.Quantity));

    //    var cashList = cashManagement.GetCashStateList();
    //    Assert.IsTrue(cashList.Any());
    //}


    //[TestMethod]
    //public async Task _05_ClosePositionMarketOrder_Long_CashManagement_ShouldWorkAsync()
    //{
    //    // Alle Candles
    //    Candle[] inputCandles = [
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 4, Close = 5 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 4, Close = 4 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low = 3, Close = 4 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), Low = 4, Close = 6 }
    //    ];

    //    // Exchange
    //    var exchange = _serviceProvider.GetService<IBacktestingExchange>();
    //    Assert.IsNotNull(exchange);

    //    exchange.SetCandle(inputCandles[0]);

    //    // Limit Order
    //    var order = Order.CreateLong("BTC_USDT");
    //    order.Quantity = 1;
    //    order.Price = 3.5;
    //    order.Lever = 1;
    //    await exchange.PlaceOrderAsync(order);

    //    // Run Candles
    //    foreach (var candle in inputCandles)
    //    {
    //        exchange.SetCandle(candle);
    //        await exchange.RunAsync();
    //    }

    //    // Closing Market Order
    //    var closingOrder = Order.CreateShort("BTC_USDT");
    //    closingOrder.Quantity = order.Quantity;
    //    closingOrder.Lever = order.Lever;
    //    await exchange.PlaceOrderAsync(closingOrder);

    //    // Assert Position
    //    var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
    //    Assert.IsNotNull(positionManagement);

    //    var openPosition = await positionManagement.GetOpenPositionAsync();
    //    Assert.IsNull(openPosition);

    //    var positions = await positionManagement.GetPositionsAsync();
    //    Assert.AreEqual(1, positions.Count);

    //    // Assert Cash
    //    var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
    //    Assert.IsNotNull(cashManagement);

    //    var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
    //    Assert.IsNotNull(options);

    //    var margin = await cashManagement.GetMarginAsync();
    //    Assert.AreNotEqual(options.Value.InitialCash, margin);
    //    Assert.IsTrue(margin > options.Value.InitialCash);

    //    var expectedMargin = options.Value.InitialCash - (order.ExecutedValue + order.ExecutedFee) + (closingOrder.ExecutedValue - closingOrder.ExecutedFee) + positions[0].PNL;
    //    Assert.AreEqual(expectedMargin, margin);

    //    var cashList = cashManagement.GetCashStateList();
    //    Assert.IsTrue(cashList.Any());
    //    Assert.AreEqual(options.Value.InitialCash, cashList[0].Cash);
    //    Assert.IsTrue(cashList[0].Cash > cashList[1].Cash); // create position
    //    Assert.IsTrue(cashList[2].Cash < cashList[1].Cash); // fee
    //    Assert.IsTrue(cashList[2].Cash < cashList[3].Cash); // sell position
    //    Assert.IsTrue(options.Value.InitialCash < cashList[3].Cash); // sell position with win
    //}

    //[TestMethod]
    //public async Task _06_ClosePositionMarketOrder_Short_CashManagement_ShouldWorkAsync()
    //{
    //    // Alle Candles
    //    Candle[] inputCandles = [
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), High = 5, Close = 5 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), High = 5, Close = 4 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), High = 7, Close = 6 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), High = 3, Close = 3 },
    //    ];

    //    // Exchange
    //    var exchange = _serviceProvider.GetService<IBacktestingExchange>();
    //    Assert.IsNotNull(exchange);

    //    exchange.SetCandle(inputCandles[0]);

    //    // Limit Order
    //    var order = Order.CreateShort("BTC_USDT");
    //    order.Quantity = (new Random().NextDouble() + 1) * 4;
    //    order.Price = 5.5;
    //    order.Lever = (new Random().NextDouble() + 1) * 4;
    //    await exchange.PlaceOrderAsync(order);

    //    // Run Candles
    //    foreach (var candle in inputCandles)
    //    {
    //        exchange.SetCandle(candle);
    //        await exchange.RunAsync();
    //    }

    //    // Assert first Limit Order
    //    var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
    //    Assert.IsNotNull(positionManagement);

    //    var openPosition = await positionManagement.GetOpenPositionAsync();
    //    Assert.IsNotNull(openPosition);

    //    var positions = await positionManagement.GetPositionsAsync();
    //    Assert.AreEqual(1, positions.Count);

    //    var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
    //    Assert.IsNotNull(cashManagement);

    //    var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
    //    Assert.IsNotNull(options);

    //    var margin = await cashManagement.GetMarginAsync();
    //    var expectedCashAfterFirstExecutedOrder = options.Value.InitialCash - order.GetValue() - order.ExecutedFee;
    //    Assert.AreEqual(expectedCashAfterFirstExecutedOrder, margin);

    //    // Closing Market Order
    //    var closingOrder = Order.CreateLong("BTC_USDT");
    //    closingOrder.Quantity = order.Quantity;
    //    closingOrder.Lever = order.Lever;
    //    await exchange.PlaceOrderAsync(closingOrder);

    //    // Assert Position after Market Order
    //    openPosition = await positionManagement.GetOpenPositionAsync();
    //    Assert.IsNull(openPosition);

    //    positions = await positionManagement.GetPositionsAsync();
    //    Assert.AreEqual(1, positions.Count);

    //    // Assert Cash
    //    margin = await cashManagement.GetMarginAsync();
    //    var expectedMargin = options.Value.InitialCash - (order.ExecutedValue + order.ExecutedFee) + (closingOrder.ExecutedValue - closingOrder.ExecutedFee) + positions[0].PNL;
    //    Assert.AreEqual(Math.Round(expectedMargin!.Value, 6), Math.Round(margin, 6));

    //    var cashList = cashManagement.GetCashStateList();
    //    Assert.IsTrue(cashList.Any());
    //    Assert.AreEqual(options.Value.InitialCash, cashList[0].Cash);
    //    Assert.IsTrue(cashList[0].Cash > cashList[1].Cash); // create position
    //    Assert.IsTrue(cashList[2].Cash < cashList[1].Cash); // fee
    //    Assert.IsTrue(cashList[2].Cash < cashList[3].Cash); // sell position
    //}

    //[TestMethod]
    //public async Task _07_ClosePositionMarketOrder_Long_NotProfitable_ShouldWorkAsync()
    //{
    //    // Alle Candles
    //    Candle[] inputCandles = [
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), Low = 6, Close = 7 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), Low = 5, Close = 6 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), Low = 4, Close = 5 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), Low = 4, Close = 5 }
    //    ];

    //    // Exchange
    //    var exchange = _serviceProvider.GetService<IBacktestingExchange>();
    //    Assert.IsNotNull(exchange);

    //    exchange.SetCandle(inputCandles[0]);

    //    // Limit Order
    //    var order = Order.CreateLong("BTC_USDT");
    //    order.Quantity = 1;
    //    order.Price = 6;
    //    order.Lever = 1;
    //    await exchange.PlaceOrderAsync(order);

    //    // Run Candles
    //    foreach (var candle in inputCandles)
    //    {
    //        exchange.SetCandle(candle);
    //        await exchange.RunAsync();
    //    }

    //    // Closing Market Order
    //    var closingOrder = Order.CreateShort("BTC_USDT");
    //    closingOrder.Quantity = order.Quantity;
    //    closingOrder.Lever = order.Lever;
    //    await exchange.PlaceOrderAsync(closingOrder);

    //    // Assert Position
    //    var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
    //    Assert.IsNotNull(positionManagement);

    //    var openPosition = await positionManagement.GetOpenPositionAsync();
    //    Assert.IsNull(openPosition);

    //    var positions = await positionManagement.GetPositionsAsync();
    //    Assert.AreEqual(1, positions.Count);

    //    // Assert Cash
    //    var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
    //    Assert.IsNotNull(cashManagement);

    //    var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
    //    Assert.IsNotNull(options);

    //    var margin = await cashManagement.GetMarginAsync();
    //    Assert.AreNotEqual(options.Value.InitialCash, margin);
    //    Assert.IsTrue(margin < options.Value.InitialCash);

    //    var expectedMargin = options.Value.InitialCash - (order.ExecutedValue + order.ExecutedFee) + (closingOrder.ExecutedValue - closingOrder.ExecutedFee) + positions[0].PNL;
    //    Assert.AreEqual(expectedMargin, margin);

    //    var cashList = cashManagement.GetCashStateList();
    //    Assert.IsTrue(cashList.Any());
    //}

    //[TestMethod]
    //public async Task _08_ClosePositionMarketOrder_Short_NotProfitable_ShouldWorkAsync()
    //{
    //    // Alle Candles
    //    Candle[] inputCandles = [
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-4), High = 5, Close = 5 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-3), High = 5, Close = 4 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-2), High = 7, Close = 6 },
    //        new Candle() { Timestamp = DateTime.Now.AddMinutes(-1), High = 8, Close = 7 },
    //    ];

    //    // Exchange
    //    var exchange = _serviceProvider.GetService<IBacktestingExchange>();
    //    Assert.IsNotNull(exchange);

    //    exchange.SetCandle(inputCandles[0]);

    //    // Limit Order
    //    var order = Order.CreateShort("BTC_USDT");
    //    order.Quantity = 1;
    //    order.Price = 6;
    //    order.Lever = 1;
    //    await exchange.PlaceOrderAsync(order);

    //    // Run Candles
    //    foreach (var candle in inputCandles)
    //    {
    //        exchange.SetCandle(candle);
    //        await exchange.RunAsync();
    //    }

    //    // Closing Market Order
    //    var closingOrder = Order.CreateLong("BTC_USDT");
    //    closingOrder.Quantity = order.Quantity;
    //    closingOrder.Lever = order.Lever;
    //    await exchange.PlaceOrderAsync(closingOrder);

    //    // Assert Position
    //    var positionManagement = _serviceProvider.GetService<IBacktestingPositionManagement>();
    //    Assert.IsNotNull(positionManagement);

    //    var openPosition = await positionManagement.GetOpenPositionAsync();
    //    Assert.IsNull(openPosition);

    //    var positions = await positionManagement.GetPositionsAsync();
    //    Assert.AreEqual(1, positions.Count);

    //    // Assert Cash
    //    var cashManagement = _serviceProvider.GetService<IBacktestingCashManagement>();
    //    Assert.IsNotNull(cashManagement);

    //    var options = _serviceProvider.GetService<IOptions<BacktestingOptions>>();
    //    Assert.IsNotNull(options);

    //    var margin = await cashManagement.GetMarginAsync();
    //    Assert.AreNotEqual(options.Value.InitialCash, margin);
    //    Assert.IsTrue(margin < options.Value.InitialCash);

    //    var expectedMargin = options.Value.InitialCash - (order.ExecutedValue + order.ExecutedFee) + (closingOrder.ExecutedValue - closingOrder.ExecutedFee) + positions[0].PNL;
    //    Assert.AreEqual(expectedMargin, margin);

    //    var cashList = cashManagement.GetCashStateList();
    //    Assert.IsTrue(cashList.Any());
    //}
}

