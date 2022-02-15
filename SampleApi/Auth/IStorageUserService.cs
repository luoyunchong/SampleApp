using SampleApi.Controllers;
using SampleApi.Models;

namespace SampleApi.Auth;

public interface IStorageUserService
{
    /// <summary>
    /// 根据登录验证用户
    /// </summary>
    /// <param name="loginInfo"></param>
    /// <returns></returns>
    Task<SysUser> CheckPasswordAsync(LoginInfo loginInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据Request Header携带Authorization:Bearer+空格+AccessToken获取当前登录人信息
    /// </summary>
    /// <returns></returns>
    Task<CurrentUser> GetUserByRequestContext();
}
