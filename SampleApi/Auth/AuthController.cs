using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SampleApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

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

    //需要将此数据改成数据库存储
    public static ISet<RefreshToken> RefreshTokens = new Lazy<ISet<RefreshToken>>(() =>
      {
          return new HashSet<RefreshToken>();
      }).Value;

    public AuthController(JwtSettings jwtSettings, IStorageUserService userService)
    {
        _jwtSettings = jwtSettings;
        _userService = userService;
    }


    #region 登录
    /// <summary>
    /// 登录，生成访问Token
    /// </summary>
    /// <param name="loginInfo"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginInfo loginInfo)
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

        return Ok(CreateAccessToken(user));

    }

    private CommonReponse CreateAccessToken(SysUser user)
    {
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

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = GenerateToken(),
            CreateTime = DateTime.Now,
            ExpiredTime = DateTime.Now.AddDays(30),
        };

        //模拟保存Token
        RefreshTokens.Add(refreshToken);
        return new CommonReponse
        {
            Status = true,
            Data = new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken.Token
            }
        };
    }
    private string GenerateToken(int size = 32)
    {
        var randomNumber = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    #endregion

    /// <summary>
    /// RefreshToken 刷新 Token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {
        var refreshTokenEntity = RefreshTokens.SingleOrDefault(t => t.Token == refreshToken);
        if (refreshTokenEntity != null)
            RefreshTokens.Remove(refreshTokenEntity);
        if (refreshTokenEntity == null)
        {
            return Ok(new { Status = false, Message = "Invalid refresh token." });
        }
        if (refreshTokenEntity.IsExpired)
        {
            return Ok(new { Status = false, Message = "Expired refresh token." });
        }

        SysUser user = await _userService.GetUserById(refreshTokenEntity.UserId);

        return Ok(CreateAccessToken(user));
    }


    /// <summary>
    /// 编码Token
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public CurrentUser DecodeToken(string accessToken)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        if (jwtTokenHandler.CanReadToken(accessToken))
        {
            JwtPayload jwtPayload = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Payload;
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
