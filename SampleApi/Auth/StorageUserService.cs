using SampleApi.Models;
using System.Security.Claims;

namespace SampleApi.Auth;

public class StorageUserService : IStorageUserService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public StorageUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public async Task<SysUser> CheckPasswordAsync(LoginInfo loginInfo)
    {
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
