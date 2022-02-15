using SampleApi.Models;
using System.Security.Claims;

namespace SampleApi.Auth;

public class StorageUserService : IStorageUserService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<StorageUserService> logger;
    public StorageUserService(IHttpContextAccessor contextAccessor, ILogger<StorageUserService> logger)
    {
        _contextAccessor = contextAccessor;
        this.logger = logger;
    }

    public async Task<SysUser> CheckPasswordAsync(LoginInfo loginInfo, CancellationToken cancellationToken = default)
    {

        logger.LogInformation($"Begin-----延迟执行{loginInfo.UserName}....");
        await Task.Delay(5000);
        if (_contextAccessor.HttpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation($"End-------IsCancellationRequested{loginInfo.UserName}....");
            return null;
        }
        logger.LogInformation($"End-------延迟执行{loginInfo.UserName}....");
        return await Task.FromResult(new SysUser { Id = new Random().Next(10000), UserName = loginInfo.UserName });
    }

    public async Task<CurrentUser> GetUserByRequestContext()
    {
        var user = _contextAccessor.HttpContext.User;

        string? userIdOrNull = user.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        string? UserName = user.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        CurrentUser currentUser = new CurrentUser
        {
            IsAuthenticated = user.Identity.IsAuthenticated,
            UserId = userIdOrNull == null ? null : Convert.ToInt32(userIdOrNull),
            UserName = UserName
        };
        return await Task.FromResult(currentUser);
    }
}
