using System.ComponentModel.DataAnnotations;

namespace SampleApi.Auth;

/// <summary>
/// 登录请求实体
/// </summary>
public class LoginInfo
{
    public LoginInfo(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string Password { get; set; }
}
