using FarmRegistry.Application.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace FarmRegistry.Api.Controllers;

public abstract class BaseController : ControllerBase
{
    protected void LogUserInfo(string action, IUserContext userContext, object? additionalData = null)
    {
        Console.WriteLine($"[{GetType().Name}.{action}] User Info:");
        Console.WriteLine($"  OwnerId: {userContext.OwnerId}");
        Console.WriteLine($"  UserName: {userContext.UserName ?? "NULL"}");
        Console.WriteLine($"  IsAuthenticated: {userContext.IsAuthenticated}");
        
        if (additionalData != null)
        {
            Console.WriteLine($"  Additional Data: {additionalData}");
        }
        
        Console.WriteLine($"  Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine(new string('-', 50));
    }
    
    protected void LogHttpContextUser(string action, object? additionalData = null)
    {
        Console.WriteLine($"[{GetType().Name}.{action}] HttpContext User Info:");
        Console.WriteLine($"  User.Identity.Name: {HttpContext.User?.Identity?.Name ?? "NULL"}");
        Console.WriteLine($"  User.Identity.IsAuthenticated: {HttpContext.User?.Identity?.IsAuthenticated}");
        Console.WriteLine($"  User.Identity.AuthenticationType: {HttpContext.User?.Identity?.AuthenticationType ?? "NULL"}");
        
        if (additionalData != null)
        {
            Console.WriteLine($"  Additional Data: {additionalData}");
        }
        
        // Exibir claims
        if (HttpContext.User?.Claims?.Any() == true)
        {
            Console.WriteLine("  Claims:");
            foreach (var claim in HttpContext.User.Claims)
            {
                Console.WriteLine($"    {claim.Type}: {claim.Value}");
            }
        }
        
        Console.WriteLine($"  Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine(new string('-', 50));
    }
}