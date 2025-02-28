namespace Marek.Trading.Backtesting.Tests;

[TestClass]
public sealed class _00_BacktestingTests_Basic
{
    private readonly IServiceProvider _serviceProvider = TestSettings.ServiceProvider;

    [TestMethod]
    public void _01_Registration_ShouldWork()
    {
        var engine = _serviceProvider.GetService<IBacktestingEngine>();
        var exchange = _serviceProvider.GetService<IBacktestingExchange>();
        var strategy = _serviceProvider.GetService<IStrategy>();
        var dataFeed = _serviceProvider.GetService<IDataFeedExchange>();

        Assert.IsNotNull(engine);
        Assert.IsNotNull(exchange);
        Assert.IsNotNull(strategy);
        Assert.IsNotNull(dataFeed);
        Assert.AreSame("test1", strategy.Name);
        Assert.AreSame("test1", dataFeed.Name);
    }

    [TestMethod]
    public async Task _02_DataFeedCandles_ShouldWorkAsync()
    {
        var dataFeed = _serviceProvider.GetService<IBacktestingDataFeed>();

        Assert.IsNotNull(dataFeed);

        var result = await dataFeed.LoadCandlesAsync();

        var expectedCandles = await new Test1DataFeed().GetCandlesAsync("", CandleInterval.Minute_5);

        Assert.AreNotEqual(0, result.Candles.Count);
        Assert.AreEqual(expectedCandles.Count(), result.Candles.Count);
    }

    [TestMethod]
    public async Task _03_RunningOnCandles_ShouldWorkAsync()
    {
        var engine = _serviceProvider.GetService<IBacktestingEngine>();

        Assert.IsNotNull(engine);

        await engine.RunAsync();
    }

    [TestMethod]
    public async Task _04_StrategyNotificationsCount_ShouldWorkAsync()
    {
        var engine = _serviceProvider.GetService<IBacktestingEngine>();
        Assert.IsNotNull(engine);

        var eventDispatcher = _serviceProvider.GetService<IMareatorEventDispatcher>();
        Assert.IsNotNull(eventDispatcher);

        var eventCounter = 0;
        eventDispatcher.Subscribe<OnStrategyDecisionEventArgs>((s, e) =>
        {
            Console.WriteLine($"{e.Candle} --> {e.Decision}");
            eventCounter++;
        }); 

        await engine.RunAsync();

        while (engine.IsRunning)
        {
            await Task.Delay(100);
        }

        var expectedCandles = await new Test1DataFeed().GetCandlesAsync("", CandleInterval.Minute_5);

        Assert.AreEqual(expectedCandles.Count, eventCounter);
    }


    [TestMethod]
    public async Task _05_StrategyNotificationsType_ShouldWorkAsync()
    {
        var engine = _serviceProvider.GetService<IBacktestingEngine>();
        Assert.IsNotNull(engine);

        var eventDispatcher = _serviceProvider.GetService<IMareatorEventDispatcher>();
        Assert.IsNotNull(eventDispatcher);

        eventDispatcher.Subscribe<OnStrategyDecisionEventArgs>((s, e) =>
        {
            Console.WriteLine($"{e.Candle} --> {e.Decision}");
            
            Assert.IsNull(e.Decision.Exception);
            Assert.AreNotEqual(StrategyDecisionType.Error, e.Decision.Type);
        });

        await engine.RunAsync();

        while (engine.IsRunning)
        {
            await Task.Delay(100);
        }
    }
}

