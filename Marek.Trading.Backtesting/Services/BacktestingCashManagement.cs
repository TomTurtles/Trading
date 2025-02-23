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
    private readonly ConcurrentDictionary<DateTime, BacktestingCashState> _cashHistory = new(DateTimeEqualityComparer.Use());
    private Dictionary<DateTime, BacktestingCashState> OrderedCash => new(_cashHistory.OrderBy(kvp => kvp.Key));
    private double Cash => OrderedCash.Any() ? OrderedCash.LastOrDefault().Value.Cash : 0;
    public BacktestingCashManagement(
        IMareatorEventDispatcher eventDispatcher,
        ILogger<BacktestingCashManagement> logger,
        IOptions<BacktestingOptions> options)
    {
        EventDispatcher = eventDispatcher;
        Logger = logger;
        Options = options;

        AddCash(new(DateTime.MinValue, InitialCash, "Initial"));
    }

    public Task<double> GetMarginAsync(CancellationToken cancellationToken = default) => Task.FromResult(Cash);
    public Dictionary<DateTime, BacktestingCashState> GetHistory() => OrderedCash;
    public List<BacktestingCashState> GetCashStateList() => OrderedCash.Select(kvp => kvp.Value).ToList();  

    public bool CanAfford(double cost) => cost <= Cash;

    public void AddCash(BacktestingCashTransaction cashTransaction)
    {
        var newCash = Cash + cashTransaction.Relative;
        if (newCash < 0)
        {
            throw new InvalidOperationException($"transaction ({cashTransaction.Relative}) not allowed, insufficient cash ({Cash}).");
        }

        _cashHistory.AddOrUpdate(
            cashTransaction.Timestamp, 
            new BacktestingCashState(cashTransaction, Cash), 
            (ts, prevState) =>
            {
                var newTransaction = new BacktestingCashTransaction(
                    ts,
                    prevState.Relative + cashTransaction.Relative,
                    string.Join(" | ", [$"{prevState.Reason} ({prevState.Relative})", $"{cashTransaction.Reason} ({cashTransaction.Relative})"])
                );

                var previousPreviousCash = prevState.Cash - prevState.Relative;
                var newState = new BacktestingCashState(newTransaction, previousPreviousCash); 
                Debug.WriteLine($"[{ts}] Cash update {prevState.Cash} => {newState.Cash}");
                return newState;
            }
        );

        EventDispatcher.Publish(this, new OnBacktestingCashUpdatedEventArgs(OrderedCash.LastOrDefault().Value));
    }
}
