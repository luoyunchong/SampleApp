using System;
using System.Diagnostics;
using System.Threading;
using Humanizer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleApp;
using SampleApp.Options;

namespace SampleAppTests
{
    public class Startup
    {
        IConfiguration configuration;
        // 自定义 host 构建
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureAppConfiguration((context, builder) =>
                {
                    // 注册配置
                    builder
                          .AddJsonFile("appsettings.json")
                          ;
                })
                .ConfigureServices((context, services) =>
                {
                    configuration = context.Configuration;
                    // 注册自定义服务
                })
                ;
        }

        // 支持的形式：
        // ConfigureServices(IServiceCollection services)
        // ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        // ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        {
            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
     .UseConnectionString(FreeSql.DataType.Sqlite, configuration["ConnectionStrings:DefaultConnection"])
     .UseAutoSyncStructure(true)
     //.UseGenerateCommandParameterWithLambda(true)
     .UseLazyLoading(true)
     .UseMonitorCommand(
         cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
         )
     .Build();
            // 配置日志
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
            services.AddSingleton(fsql);
            services.Configure<AppOption>(configuration.GetSection(AppOption.Name));
            services.AddTransient<App>(); // Add transiant mean give me an instance each it is being requested
            services.AddHttpClient();
        }

        // 可以添加要用到的方法参数，会自动从注册的服务中获取服务实例，类似于 asp.net core 里 Configure 方法
        public void Configure(IServiceProvider applicationServices)
        {
            // 有一些测试数据要初始化可以放在这里
            // InitData();
        }
    }
}