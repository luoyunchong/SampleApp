using FreeSql.DataAnnotations;

namespace SampleApi.Models;

public class SysUser
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    public string UserName { get; set; }
}
