namespace Marek.Trading.Core;

public interface IBacktestingCashManagement
{
    Task<double> GetMarginAsync(CancellationToken cancellationToken = default);
    Task<List<double>> GetCashList(CancellationToken cancellationToken = default);
    bool CanAfford(double cost);
    void AddCash(DateTime timestamp, double relative);
}
