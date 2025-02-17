namespace Marek.Trading.Core;

public class GetPositionRequest(string symbol) : IRequest<Position?>
{
    public string Symbol { get; } = symbol;
}
