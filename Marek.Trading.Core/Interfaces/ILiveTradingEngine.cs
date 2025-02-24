namespace Marek.Trading.Core;

public interface ILiveTradingEngine
{
    LiveTradingState State { get; }
    void Start();
    void Stop();
}
