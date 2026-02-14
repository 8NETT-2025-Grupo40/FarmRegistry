using FarmRegistry.Application.Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FarmRegistry.Api.Controllers;

public abstract class BaseController : ControllerBase
{
    private ILogger Logger => HttpContext.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger(GetType());

    protected void LogUserInfo(string action, IUserContext userContext, object? additionalData = null)
    {
        Logger.LogInformation(
            "{Controller}.{Action} - Authenticated: {IsAuthenticated} - OwnerId: {OwnerId}",
            GetType().Name,
            action,
            userContext.IsAuthenticated,
            userContext.OwnerId);

        if (additionalData != null)
        {
            Logger.LogDebug(
                "{Controller}.{Action} - AdditionalData: {@AdditionalData}",
                GetType().Name,
                action,
                additionalData);
        }
    }

    protected void LogHttpContextUser(string action, object? additionalData = null)
    {
        Logger.LogDebug(
            "{Controller}.{Action} - HttpContext Identity: Name={Name}, IsAuthenticated={IsAuthenticated}, Type={AuthenticationType}",
            GetType().Name,
            action,
            HttpContext.User?.Identity?.Name,
            HttpContext.User?.Identity?.IsAuthenticated,
            HttpContext.User?.Identity?.AuthenticationType);

        if (additionalData != null)
        {
            Logger.LogDebug(
                "{Controller}.{Action} - AdditionalData: {@AdditionalData}",
                GetType().Name,
                action,
                additionalData);
        }
    }
}
