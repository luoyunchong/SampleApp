using FreeSql.DataAnnotations;
using System.Security.Principal;

namespace SampleApi.Controllers
{
    public class SysUser
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}