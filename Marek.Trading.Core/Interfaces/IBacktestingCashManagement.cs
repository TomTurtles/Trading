namespace Marek.Trading.Core;

public interface IBacktestingCashManagement
{
    Task<double> GetMarginAsync(CancellationToken cancellationToken = default);
    Dictionary<DateTime, BacktestingCashState> GetHistory();
    List<BacktestingCashState> GetCashStateList();
    bool CanAfford(double cost);
    void AddCash(BacktestingCashTransaction cashTransaction);
}
