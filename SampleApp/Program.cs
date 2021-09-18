using FreeSql;
using FreeSql.DataAnnotations;
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

            var service = ActivatorUtilities.CreateInstance<App>(host.Services);

            await service.RunAsync(args);

        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            // Check the current directory that the application is running on 
            // Then once the file 'appsetting.json' is found, we are adding it.
            // We add env variables, which can override the configs in appsettings.json
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                ;
        }

        static IHost AppStartup()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            BuildConfig(builder);
            IConfigurationRoot configuration = builder.Build();
            // Specifying the configuration for serilog
            Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                            .ReadFrom.Configuration(configuration) // connect serilog to our configuration folder
                            .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                            .CreateLogger(); //initialise the logger

            Log.Logger.Information("Application Starting");

            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
             //.UseConnectionString(FreeSql.DataType.Sqlite, configuration["ConnectionStrings:DefaultConnection"])
             .UseConnectionString(FreeSql.DataType.MySql, configuration["ConnectionStrings:MySql"])
             .UseAutoSyncStructure(true)
             //.UseNoneCommandParameter(true)
             //.UseGenerateCommandParameterWithLambda(true)
             .UseLazyLoading(true)
             .UseMonitorCommand(
                 cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
                 )
             .Build();


            var host = Host.CreateDefaultBuilder() // Initialising the Host 
                        .ConfigureServices((context, services) =>
                        { // Adding the DI container for configuration
                          // 添加 services:
                            services.AddSingleton(fsql);
                            services.AddFreeRepository();
                            services.AddScoped<UnitOfWorkManager>();
                            services.Configure<AppOption>(configuration.GetSection(AppOption.Name));
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
    }
}
