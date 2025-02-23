namespace Marek.Trading.Core;
public interface IDataFeedOptions
{
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    int WarmUpCandles { get; set; }
    List<CandleInterval> LoadingIntervals { get; set; }
}
