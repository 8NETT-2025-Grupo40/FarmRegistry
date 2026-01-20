namespace FarmRegistry.Application.Contracts.Common;

public interface IUserContext
{
    Guid OwnerId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}