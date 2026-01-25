using FarmRegistry.Application.Contracts.Common;

namespace FarmRegistry.Infrastructure.Services;

public class MockUserContext : IUserContext
{
    public Guid OwnerId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public string? UserName => "Mock User";
    public bool IsAuthenticated => true;
}