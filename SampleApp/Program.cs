using FreeSql;
using FreeSql.DataAnnotations;
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
                             //.UseConnectionString(FreeSql.DataType.Sqlite, configuration["ConnectionStrings:DefaultConnection"])
                             .UseConnectionString(FreeSql.DataType.MySql, configuration["ConnectionStrings:MySql"])
                             //.UseConnectionString(FreeSql.DataType.SqlServer, configuration["ConnectionStrings:SqlServer"])
                             .UseAutoSyncStructure(true)
                             //.UseNoneCommandParameter(true)
                             //.UseGenerateCommandParameterWithLambda(true)
                             .UseLazyLoading(false)
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

            services.AddSingleton(fsql);
            services.AddFreeRepository();
            services.AddScoped<UnitOfWorkManager>();
            services.Configure<AppOption>(configuration.GetSection(nameof(AppOption)));
        }
    }
}
