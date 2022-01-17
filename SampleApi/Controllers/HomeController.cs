using FreeSql;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;
using System.Diagnostics;

namespace SampleApi.Controllers;

/// <summary>
/// 使用IdleBus管理多个FreeSql:依赖注入方式 
/// </summary>

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly IFreeSql _fsql;

    public HomeController(ILogger<HomeController> logger, IFreeSql fsql)
    {
        _logger = logger;
        _fsql = fsql;
    }

    /// <summary>
    /// 获取db1,db2，插入db1,db2
    /// </summary>
    /// <returns></returns>
    [HttpGet("get1")]
    public IEnumerable<SysUser> Get1()
    {
        //查询 db1
        var b0 = _fsql.Select<SysUser>().ToList();

        var b1 = _fsql.Change("db2");
        //查询 db2
        var b2 = _fsql.Select<SysUser>().ToList();
        //插入到 db2
        var c0 = _fsql.Insert(new SysUser { UserName = "db2" }).ExecuteAffrows();
        using (_fsql.Change("db1"))
        {
            //查询 db1
            var b3 = _fsql.Select<SysUser>().ToList();
            //插入到 db1
            _fsql.Insert(new SysUser { UserName = "db1" }).ExecuteAffrows();
        }

        //查询 db2
        var db2 = _fsql.Select<SysUser>().ToList();
        _fsql.Change("db2");
        return db2;
    }

    /// <summary>
    /// 获取DB3，在未注册前，会报错
    /// </summary>
    /// <returns></returns>
    [HttpGet("getdb3")]
    public IEnumerable<SysUser> Get3()
    {
        _fsql.Change("db3");
        //查询 db1
        var b0 = _fsql.Select<SysUser>().ToList();
        return b0;
    }

    /// <summary>
    /// change之后，只对本次请求有效
    /// </summary>
    /// <param name="dbname"></param>
    /// <returns></returns>
    [HttpGet("change")]
    public int Change(string dbname)
    {
        //仅对本次请求有效
        _fsql.Change(dbname);
        return 1;
    }

    /// <summary>
    /// 默认从第一个数据库中获取
    /// </summary>
    /// <returns></returns>
    [HttpGet("getdb1")]
    public IEnumerable<SysUser> GetDB1()
    {
        //查询 db1
        var b0 = _fsql.Select<SysUser>().ToList();
        return b0;
    }
    
    /// <summary>
    /// 注册第3个数据库
    /// </summary>
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
