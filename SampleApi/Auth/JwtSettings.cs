using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace SampleApi.Auth;

public class JwtSettings
{
    public JwtSettings(byte[] key, string issuer, string audience)
    {
        Key = key;
        Issuer = issuer;
        Audience = audience;
    }

    public string Issuer { get; }

    public string Audience { get; }

    public byte[] Key { get; }

    public TokenValidationParameters TokenValidationParameters => new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Key)
    };

    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        var issuser = configuration["Authentication:JwtBearer:Issuer"] ?? "defaultissuer";
        var auidence = configuration["Authentication:JwtBearer:Audience"] ?? "defaultauidence";
        var base64Key = configuration["Authentication:JwtBearer:SecurityKey"];

        byte[] key;
        if (!string.IsNullOrEmpty(base64Key))
        {
            key = Convert.FromBase64String(base64Key);
        }
        else
        {
            // In real life this would come from configuration
            key = new byte[32];
            RandomNumberGenerator.Fill(key);
        }

        return new JwtSettings(key, issuser, auidence);
    }
}
