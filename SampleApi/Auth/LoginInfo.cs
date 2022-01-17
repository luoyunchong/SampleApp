using System.ComponentModel.DataAnnotations;

namespace SampleApi.Auth;

public class LoginInfo
{
    public LoginInfo(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
}
