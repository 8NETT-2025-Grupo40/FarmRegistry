namespace FarmRegistry.Domain.Common;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
