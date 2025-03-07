namespace Marek.Trading.Core;

public interface IMarketProductInfo
{
    public string Symbol { get; set; }
    public string BaseCurrency { get; set; }
    public string QuoteCurrency { get; set; }
    public double TickSize { get; set; }
    public MarketProductState State { get; set; }
    double FaceValue { get; set; }  
}
