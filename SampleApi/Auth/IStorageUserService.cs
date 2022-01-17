using SampleApi.Controllers;
using SampleApi.Models;

namespace SampleApi.Auth;

public interface IStorageUserService
{
    Task<SysUser> CheckPasswordAsync(LoginInfo loginInfo);

    Task<CurrentUser> GetUserByRequestContext();
}
