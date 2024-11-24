using System.Text.Json.Serialization;

namespace Trading;

public class OnStrategyExceptionEventArgs : TradingBaseEventArgs
{
    public string Message { get; }
    public string Details { get; }


    private Exception Exception { get; }

    public OnStrategyExceptionEventArgs(Candle candle, Exception exception) : base(candle)
    {
        Message = exception.Message;
        Details = exception.StackTrace;
        Exception = exception;
    }

    [JsonConstructor]
    public OnStrategyExceptionEventArgs(Candle candle) : base(candle)
    {
        
    }

    public Exception GetException() => Exception;
}
