namespace Marek.Trading.Core;

public class CanAffordRequest(double cost) : IRequest<bool>
{
    public double Cost { get; } = cost;
}
