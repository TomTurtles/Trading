
namespace Trading;
public interface IExchangeCash
{
    void AddCash(Candle candle, decimal relative);
    bool CanAfford(decimal cost);
    decimal GetInitialCash();
    Task<decimal> GetMarginAsync();
    void InitializeCash(Candle candle, int initCash);
    Dictionary<DateTime, decimal> GetPerformance();
}
