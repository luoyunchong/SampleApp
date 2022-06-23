using FreeSql;
using FreeSql.Internal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using SampleApi.Auth;
using SampleApi.Models;
using Serilog;
using System;
using System.Diagnostics;

namespace SampleApi;

public static class ServiceCollectionExtensions
{
    #region AddJwt
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IStorageUserService, StorageUserService>();

        var jwtSettings = JwtSettings.FromConfiguration(configuration);
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = jwtSettings.TokenValidationParameters);

        return services;
    }
    #endregion

    #region FreeSql
    public static IServiceCollection AddMultiFreeSql(this IServiceCollection services)
    {
        //db 是一个静态类，并非实例化
        //可通过事先注册 使用的数据库，或运行中使用Register动态注册

        #region 1.静态类的注册方式
        IFreeSql db = StaticDB.Instance;

        db.Register("db1", () =>
        {
            return new FreeSqlBuilder()
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(
                cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
                )
                .UseConnectionString(DataType.Sqlite, "Data Source=|DataDirectory|\\SampleApp1.db;")
                .Build();
        });
        db.Register("db2", () =>
        {
            return new FreeSqlBuilder()
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(
                cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
                )
                .UseConnectionString(DataType.Sqlite, "Data Source=|DataDirectory|\\SampleApp2.db;")
                .Build();
        });
        #endregion

        #region 2.依赖注入的使用方式
        //直接配置时间，无法配置Notice事件
        var fsql3 = new MultiFreeSql(TimeSpan.FromHours(2));

        //可传递IdleBus
        var idlebus  = new IdleBus<string, IFreeSql>(TimeSpan.FromHours(2));
        idlebus.Notice += (_, __) => { };
        var fsql2 = new MultiFreeSql(idlebus);

        fsql2.Register("db1", () => new FreeSqlBuilder().UseAutoSyncStructure(true).UseConnectionString(DataType.Sqlite, "Data Source=|DataDirectory|\\SampleApp1.db;").Build());
        fsql2.Register("db2", () => new FreeSqlBuilder().UseAutoSyncStructure(true).UseConnectionString(DataType.Sqlite, "Data Source=|DataDirectory|\\SampleApp2.db;").Build());

        services.AddSingleton<IFreeSql>(fsql2);
        #endregion

        return services;
    }
    public static IServiceCollection AddFreeSql(this IServiceCollection services, IConfiguration Configuration)
    {
        IFreeSql fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Sqlite, Configuration["ConnectionStrings:DefaultConnection"])
                    //.UseConnectionString(DataType.MySql, Configuration["ConnectionStrings:MySql"])
                    .UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower)
                    .UseAutoSyncStructure(true)
                    //.UseGenerateCommandParameterWithLambda(true)
                    .UseLazyLoading(false)
                    .UseMonitorCommand(
                        cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
                        )
                    .Build();

        services.AddSingleton(fsql);
        services.AddFreeRepository();
        services.AddScoped<UnitOfWorkManager>();
        fsql.CodeFirst.Entity<SysUser>(eb =>
        {
            eb.HasData(new List<SysUser>() { new SysUser() { UserName = "admin" } });
        });
        fsql.CodeFirst.SyncStructure<SysUser>();

        return services;
    }
    #endregion

    #region Swagger
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            try
            {
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"), true);
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
            }
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SampleApp - HTTP API",
                Version = "v1",
                Description = "The SampleApp Microservice HTTP API. This is a Data-Driven/CRUD microservice sample"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference()
                            {
                                Id =  "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "JWT授权(数据将在请求头中进行传输) 参数结构: \"Authorization: Bearer {token}\"",
                Name = "Authorization", //jwt默认的参数名称
                In = ParameterLocation.Header, //jwt默认存放Authorization信息的位置(请求头中)
                Type = SecuritySchemeType.ApiKey
            });

        });
        services.AddEndpointsApiExplorer();

        return services;

    }
    #endregion
}
