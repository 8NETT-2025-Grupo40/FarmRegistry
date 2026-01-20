using FarmRegistry.Application.Contracts.Common;

namespace FarmRegistry.Infrastructure.Common;

public sealed class UserContext : IUserContext
{
    public Guid OwnerId { get; }
    public string? UserName { get; }
    public bool IsAuthenticated { get; }

    public UserContext(Guid ownerId, string? userName = null, bool isAuthenticated = true)
    {
        OwnerId = ownerId;
        UserName = userName;
        IsAuthenticated = isAuthenticated;
    }
}