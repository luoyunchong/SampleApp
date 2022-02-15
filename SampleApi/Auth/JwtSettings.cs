using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SampleApi.Auth;

public class JwtSettings
{
    public JwtSettings(byte[] key, string issuer, string audience)
    {
        Key = key;
        Issuer = issuer;
        Audience = audience;
    }

    /// <summary>
    ///令牌的颁发者
    /// </summary>
    public string Issuer { get; }

    /// <summary>
    /// 颁发给谁
    /// </summary>
    public string Audience { get; }

    /// <summary>
    /// 签名验证的KEY
    /// </summary>
    public byte[] Key { get; }

    public TokenValidationParameters TokenValidationParameters => new TokenValidationParameters
    {
        //验证Issuer和Audience
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        //是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
        ValidateLifetime = true,
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Key)
    };

    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        var issuser = configuration["Authentication:JwtBearer:Issuer"] ?? "default_issuer";
        var auidence = configuration["Authentication:JwtBearer:Audience"] ?? "default_auidence";
        var securityKey = configuration["Authentication:JwtBearer:SecurityKey"] ?? "default_securitykey";

        byte[] key = Encoding.ASCII.GetBytes(securityKey);

        return new JwtSettings(key, issuser, auidence);
    }
}
