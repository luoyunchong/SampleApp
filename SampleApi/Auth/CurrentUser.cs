namespace SampleApi.Auth;

public class CurrentUser
{
    /// <summary>
    /// 是否登录
    /// </summary>
    public bool IsAuthenticated { get; set; }
    /// <summary>
    /// 用户Id
    /// </summary>
    public int? UserId { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }
}