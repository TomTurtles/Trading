namespace Marek.Trading;

public class BacktestingCashManagement : IBacktestingCashManagement
{
    // Services
    public IMareatorEventDispatcher EventDispatcher { get; }
    public ILogger<BacktestingCashManagement> Logger { get; }
    public IOptions<BacktestingOptions> Options { get; }

    // Options
    private double InitialCash => Options.Value.InitialCash;

    // Management
    private readonly ConcurrentDictionary<DateTime, double> _cashHistory = new(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, double> OrderedCash => new(_cashHistory.OrderBy(kvp => kvp.Key));
    private double Cash => OrderedCash.LastOrDefault().Value;

    public BacktestingCashManagement(
        IMareatorEventDispatcher eventDispatcher,
        ILogger<BacktestingCashManagement> logger,
        IOptions<BacktestingOptions> options)
    {
        EventDispatcher = eventDispatcher;
        Logger = logger;
        Options = options;

        AddCash(DateTime.MinValue, InitialCash);
    }

    public Task<double> GetMarginAsync(CancellationToken cancellationToken = default) => Task.FromResult(Cash);
    public Task<List<double>> GetCashList(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OrderedCash.Select(kvp => kvp.Value).ToList());  
    }

    public bool CanAfford(double cost) => cost <= Cash;

    public void AddCash(DateTime timestamp, double relative)
    {
        var newCash = Cash + relative;
        if (newCash < 0)
        {
            throw new InvalidOperationException($"transaction ({relative}) not allowed, insufficient cash ({Cash}).");
        }

        _cashHistory.AddOrUpdate(timestamp, newCash, (ts, prev) =>
        {
            Debug.WriteLine($"[{ts}] Cash update {prev} => {newCash}");
            return newCash;
        });

        EventDispatcher.Publish(this, new OnBacktestingCashUpdatedEventArgs(timestamp, newCash, relative));
    }
}
