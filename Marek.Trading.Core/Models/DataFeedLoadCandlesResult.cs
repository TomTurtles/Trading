namespace Marek.Trading.Core;

public class DataFeedLoadCandlesResult
{
    public List<Candle> Candles { get; set; } = [];
    public Dictionary<CandleInterval, List<Candle>> AdditionalCandles { get; set; } = [];
}
