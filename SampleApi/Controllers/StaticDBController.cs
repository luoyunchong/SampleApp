using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaticDBController : ControllerBase
    {
        private readonly ILogger<StaticDBController> _logger;
        private readonly IFreeSql _fsql = StaticDB.Instance;

        public StaticDBController(ILogger<StaticDBController> logger)
        {
            _logger = logger;
        }

        [HttpGet("get1")]
        public IEnumerable<SysUser> Get1()
        {
            //查询 db1
            var b0 = _fsql.Select<SysUser>().ToList();

            var b1 = _fsql.Change("db2");
            //查询 db2
            var b2 = _fsql.Select<SysUser>().ToList();
            //插入到 db2
            var c0 = _fsql.Insert(new SysUser { Name = "db2" }).ExecuteAffrows();
            using (_fsql.Change("db1"))
            {
                //查询 db1
                var b3 = _fsql.Select<SysUser>().ToList();
                //插入到 db1
                _fsql.Insert(new SysUser { Name = "db1" }).ExecuteAffrows();
            }

            //查询 db2
            var db2 = _fsql.Select<SysUser>().ToList();
            _fsql.Change("db2");
            return db2;
        }

        [HttpGet("getdb3")]
        public IEnumerable<SysUser> Get3()
        {
            _fsql.Change("db3");
            //查询 db1
            var b0 = _fsql.Select<SysUser>().ToList();
            return b0;
        }

        [HttpGet("change")]
        public int Change(string dbname)
        {
            //仅对本次请求有效
            _fsql.Change(dbname);
            return 1;
        }

        [HttpGet("getdb1")]
        public IEnumerable<SysUser> GetDB1()
        {
            //查询 db1
            var b0 = _fsql.Select<SysUser>().ToList();
            return b0;
        }
        [HttpGet("register")]
        public void Register()
        {
            _fsql.Register("db3", () =>
            {
                return new FreeSqlBuilder().UseAutoSyncStructure(true)
                .UseMonitorCommand(
                cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
                )
                .UseConnectionString(DataType.Sqlite, "Data Source=|DataDirectory|\\SampleApp3.db;")
                .Build();
            });
        }
    }
}
