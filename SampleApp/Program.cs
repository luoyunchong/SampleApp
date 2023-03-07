using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using Humanizer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleApp.Options;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = AppStartup();

            var app = host.Services.GetService<App>();

            await app.RunAsync(args);
        }

        static IHost AppStartup()
        {
            var host = Host.CreateDefaultBuilder() // Initialising the Host 
                        .ConfigureServices((context, services) =>
                        {
                            // Adding the DI container for configuration
                            ConfigureServices(context, services);
                            services.AddTransient<App>(); // Add transiant mean give me an instance each it is being requested
                            services.AddHttpClient();
                        })
                        .ConfigureAppConfiguration((host, config) =>
                        {
                            config.AddJsonFile($"settings.json", optional: true, reloadOnChange: true);
                        })
                        .UseSerilog() // Add Serilog
                        .Build(); // Build the Host

            return host;
        }


        static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;

            Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                            .ReadFrom.Configuration(configuration) // connect serilog to our configuration folder
                            .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                            .CreateLogger(); //initialise the logger

            Log.Logger.Information("Application Starting");

            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                            .UseConnectionString(FreeSql.DataType.Sqlite, configuration["ConnectionStrings:DefaultConnection"])
                                // .UseConnectionString(FreeSql.DataType.MySql, configuration["ConnectionStrings:MySql"])
                                //.UseConnectionString(FreeSql.DataType.SqlServer, configuration["ConnectionStrings:SqlServer"])
                             .UseMappingPriority(MappingPriorityType.Attribute, MappingPriorityType.FluentApi, MappingPriorityType.Aop)
                             .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
                             .UseAutoSyncStructure(true)
                             //.UseNoneCommandParameter(true)
                             //.UseGenerateCommandParameterWithLambda(true)
                             //.UseLazyLoading(false)
                             .UseMonitorCommand(
                                 cmd => Console.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
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
            services.Configure<AppOption>(configuration.GetSection(nameof(AppOption)));
        }
    }
}
