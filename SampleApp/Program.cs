using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using Humanizer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleApp.Extensions;
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
            using IHost host = AppStartup();
            var app = host.Services.GetService<App>();
            await app.RunAsync(args);
        }

        static IHost AppStartup()
        {
            var host = Host.CreateDefaultBuilder() // Initialising the Host 
                        .ConfigureServices((context, services) =>
                        {
                            Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                                            .ReadFrom.Configuration(context.Configuration) // connect serilog to our configuration folder
                                            .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                                            .CreateLogger(); //initialise the logger

                            services.Init(context.Configuration);
                        })
                        .ConfigureAppConfiguration((host, config) =>
                        {
                            config.AddJsonFile($"settings.json", optional: true, reloadOnChange: true);
                        })
                        .UseSerilog() // Add Serilog
                        .Build(); // Build the Host

            Log.Logger.Information("Application Starting");

            return host;
        }
    }
}
