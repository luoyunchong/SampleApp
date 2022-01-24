using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SampleApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SampleApi.Auth;

/// <summary>
/// 登录认证个人信息
/// </summary>
[ApiController]
[Route("/api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IStorageUserService _userService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(JwtSettings jwtSettings, IStorageUserService userService)
    {
        _jwtSettings = jwtSettings;
        _userService = userService;
    }

    /// <summary>
    /// 登录，生成访问Toekn
    /// </summary>
    /// <param name="loginInfo"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateToken(LoginInfo loginInfo)
    {
        SysUser user = await _userService.CheckPasswordAsync(loginInfo);
        if (user == null)
        {
            return Ok(new
            {
                Status = false,
                Message = "账号或密码错误"
            });
        }

        var claims = new List<Claim>();

        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.Name, user.UserName));

        var key = new SymmetricSecurityKey(_jwtSettings.Key);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: creds
            );
        return Ok(new
        {
            Status = true,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }

    /// <summary>
    /// 编码Token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public CurrentUser DecodeToken(string token)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        if (jwtTokenHandler.CanReadToken(token))
        {
            JwtPayload jwtPayload = new JwtSecurityTokenHandler().ReadJwtToken(token).Payload;
            string? userIdOrNull = jwtPayload.Claims.FirstOrDefault(r => r.Type == ClaimTypes.NameIdentifier)?.Value;
            string? UserName = jwtPayload.Claims.FirstOrDefault(r => r.Type == ClaimTypes.Name)?.Value;
            CurrentUser currentUser = new CurrentUser
            {
                IsAuthenticated = userIdOrNull != null ? true : false,
                UserId = userIdOrNull == null ? null : Convert.ToInt32(userIdOrNull),
                UserName = UserName
            };
            return currentUser;
        }
        return null;
    }

    /// <summary>
    /// 根据Request Header携带Authorization:Bearer+空格+AccessToken获取当前登录人信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<CurrentUser> GetUserByRequestContext()
    {
        return await _userService.GetUserByRequestContext();
    }
}
