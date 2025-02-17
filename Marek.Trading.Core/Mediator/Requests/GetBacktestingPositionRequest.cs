namespace Marek.Trading.Core;

public class GetBacktestingPositionRequest(string id) : IRequest<Position?>
{
    public string Id { get; } = id;
}
