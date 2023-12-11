using FreeSql.Internal;
using FreeSql;
using SampleApp.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleApp.Services;
using System.Data.SQLite;
using FreeRedis;
using Newtonsoft.Json;

namespace SampleApp.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection Init(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<App>();
            services.AddTransient<IRestClient, RestClient>();

            services.AddHttpClient();
            services.Configure<AppOption>(configuration.GetSection(nameof(AppOption)));


            services.AddFreeSql(configuration);
            services.AddRedisClient(configuration);

            return services;
        }

        public static IServiceCollection AddFreeSql(this IServiceCollection services, IConfiguration configuration)
        {
            IFreeSql fsql = new FreeSqlBuilder()
                               .UseConnectionString(DataType.Sqlite, configuration["ConnectionStrings:Sqlite"])
                                // .UseConnectionString(FreeSql.DataType.MySql, configuration["ConnectionStrings:MySql"])
                                //.UseConnectionString(FreeSql.DataType.SqlServer, configuration["ConnectionStrings:SqlServer"])
                                .UseMappingPriority(MappingPriorityType.Attribute, MappingPriorityType.FluentApi, MappingPriorityType.Aop)
                                .UseNameConvert(NameConvertType.ToUpper)
                                .UseAutoSyncStructure(true)
                                //.UseNoneCommandParameter(true)
                                //.UseGenerateCommandParameterWithLambda(true)
                                //.UseLazyLoading(false)
                                .UseMonitorCommand(
                                    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
                                    )
                                .Build();
            fsql.Aop.ConfigEntityProperty += (s, e) =>
            {
                if (e.Property.PropertyType == typeof(decimal) || e.Property.PropertyType == typeof(decimal?))
                {
                    e.ModifyResult.Precision = 18;
                    e.ModifyResult.Scale = 6;
                    e.ModifyResult.DbType = "decimal";
                }
            };

            //支持雪花Id
            var options = new IdGeneratorOptions(1);
            // options.WorkerIdBitLength = 10; // 默认值6，限定 WorkerId 最大值为2^6-1，即默认最多支持64个节点。
            // options.SeqBitLength = 6; // 默认值6，限制每毫秒生成的ID个数。若生成速度超过5万个/秒，建议加大 SeqBitLength 到 10。
            // options.BaseTime = Your_Base_Time; // 如果要兼容老系统的雪花算法，此处应设置为老系统的BaseTime。
            // ...... 其它参数参考 IdGeneratorOptions 定义。
            YitIdHelper.SetIdGenerator(options);
            // 保存参数（务必调用，否则参数设置不生效）

            fsql.Aop.AuditValue += (s, e) =>
            {
                if (e.Column.CsType == typeof(long) && e.Property.Name == "Id" && e.Value?.ToString() == "0")
                {
                    e.Value = YitIdHelper.NextId();
                    //e.Column.Attribute.IsIdentity = false;
                }
            };
            fsql.Aop.ConfigEntityProperty += (s, e) =>
            {
                if (e.Property.Name == "Id")
                    e.ModifyResult.IsIdentity = false;
            };

            services.AddSingleton(fsql);
            services.AddFreeRepository();
            services.AddScoped<UnitOfWorkManager>();

            IFreeSql<MySqlFlag> mysql = new FreeSqlBuilder()
                        .UseConnectionString(DataType.MySql, configuration["ConnectionStrings:MySql"])
                         .UseMappingPriority(MappingPriorityType.Attribute, MappingPriorityType.FluentApi, MappingPriorityType.Aop)
                         .UseNameConvert(NameConvertType.ToLower)
                         .UseAutoSyncStructure(true)
                         .UseMonitorCommand(
                             cmd => Console.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
                             )
                         .Build<MySqlFlag>();

            services.AddSingleton<IFreeSql<MySqlFlag>>(mysql);

            return services;
        }

        public static IServiceCollection AddRedisClient(this IServiceCollection services, IConfiguration configuration)
        {
            RedisClient cli = new RedisClient(configuration["ConnectionStrings:Redis"]);
            cli.Serialize = obj => JsonConvert.SerializeObject(obj);
            cli.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);
            cli.Notice += (s, e) => Console.WriteLine(e.Log); //打印命令日志

            services.AddSingleton<IRedisClient>(cli);

            return services;
        }
    }
    
    public class MySqlFlag { }
}
