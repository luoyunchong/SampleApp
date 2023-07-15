

## SampleApp

Console集成依赖注入，Serilog日志

技术要点

- Scriban
- HTTP Request
- Serilog
- Humanizer
- HtmlAgilityPack
- FreeSql
- Newtonsoft.Json
- Json Web Token

xUnit集成依赖注入

- Xunit.DependencyInjection

## 新建一个console项目

引用包
```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="7.0.1" />
```

初始化Host
```csharp
static IHost AppStartup()
{
    var host = Host.CreateDefaultBuilder() // Initialising the Host 
                .ConfigureServices((context, services) =>
                {
                    // Adding the DI container for configuration
                    services.AddTransient<App>(); // Add transiant mean give me an instance each it is being requested
                })
                .ConfigureAppConfiguration((host, config) =>
                {
                    //config.AddJsonFile($"settings.json", optional: true, reloadOnChange: true);
                })
                .Build(); // Build the Host

    return host;
}
```

一个简单的服务，没有接口
```csharp
public class App
{
    private readonly ILogger<App> _logger;
    public App(ILogger<App> logger)
    {
        _logger = logger;
    }

    public async Task RunAsync(string[] args)
    {
        _logger.LogInformation("App Run Start");
        await Task.FromResult(0);
        _logger.LogInformation("App Run End!");
    }
}
```

调用 
```csharp
static async Task Main(string[] args)
{
    var host = AppStartup();

    var app = host.Services.GetService<App>();

    await app.RunAsync(args);
}
```


### 集成 Serilog

引用包
```xml
<PackageReference Include="Serilog.Extensions.Hosting" Version="4.1.2" />
<PackageReference Include="Serilog.Settings.Configuration" Version="3.2.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="4.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```


在Build方法调用 之前调用 `UseSerilog`
```diff
static IHost AppStartup()
{
    var host = Host.CreateDefaultBuilder() // Initialising the Host 
                .ConfigureServices((context, services) =>
                {
                    // Adding the DI container for configuration
                    ConfigureServices(context, services);
                    services.AddTransient<App>(); // Add transiant mean give me an instance each it is being requested
                })
                .ConfigureAppConfiguration((host, config) =>
                {
                    config.AddJsonFile($"settings.json", optional: true, reloadOnChange: true);
                })
+                .UseSerilog() // Add Serilog
                .Build(); // Build the Host

    return host;
}
```


增加一个单独的方法配置服务
```csharp
static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    var configuration = context.Configuration;

    Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                    .ReadFrom.Configuration(configuration) // connect serilog to our configuration folder
                    .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                    .CreateLogger(); //initialise the logger

    Log.Logger.Information("ConfigureServices Starting");

}
```  

appsettings.json中配置
```json
   "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File"
    ],
    "MinimalLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Information",
        "System": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log.txt",
          "rollingInterval": "Day"
        }
      },
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}"
        }
      }
    ]
  },
```


## 发送HTTP请求

增加包

```xml
    <PackageReference Include="Microsoft.Extensions.Http" Version="5.0.0" />
```
 
在ConfigureServices增加配置服务项

```csharp
    services.AddHttpClient();
```    
       
在App.cs中就可以调用了

```csharp
public class App
{
    private readonly ILogger<App> _logger;
    private readonly IHttpClientFactory httpClientFactory;
    public App(ILogger<App> loggerIHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        this.httpClientFactory = httpClientFactory;
    }
    public async Task<T> GetAsync<T>(string url)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
{"accept", "application/json, text/plain, */*"},
{"Accept-Language", "zh-CN,zh;q=0.9" },
{"Cookie", ""},
{ "Proxy-Connection"," keep-alive"},
{"Upgrade-Insecure-Requests", "1"},
{"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36"}
        };

        using (HttpClient client = httpClientFactory.CreateClient())
        {
            if (headers != null)
            {
                foreach (var header in headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            T result = default(T);
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadFromJsonAsync<T>();
            }

            return result;
        }
    }
}
```

## 集成 FreeSql

引用包
```xml
	<PackageReference Include="FreeSql" Version="2.6.100" />
	<PackageReference Include="FreeSql.Provider.Sqlite" Version="2.6.100" />
	<PackageReference Include="FreeSql.Provider.MySqlConnector" Version="2.6.100" />
	<PackageReference Include="FreeSql.Provider.SqlServer" Version="2.6.100" />
	<PackageReference Include="FreeSql.Repository" Version="2.6.100" />
```

 
在方法ConfigureServices配置FreeSql的服务
```csharp
static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                        //.UseConnectionString(FreeSql.DataType.Sqlite, configuration["ConnectionStrings:DefaultConnection"])
                        .UseConnectionString(FreeSql.DataType.MySql, configuration["ConnectionStrings:MySql"])
                        //.UseConnectionString(FreeSql.DataType.SqlServer, configuration["ConnectionStrings:SqlServer"])
                        .UseAutoSyncStructure(true)
                        //.UseNoneCommandParameter(true)
                        //.UseGenerateCommandParameterWithLambda(true)
                        .UseLazyLoading(true)
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
}
```

appsettings.json配置数据库链接
```json
 "ConnectionStrings": {
    "DefaultConnection": "Data Source=|DataDirectory|\\SampleApp.db;",
    "MySql": "Data Source=localhost;Port=3306;User ID=root;Password=root;Initial Catalog=sampleapp;Charset=utf8mb4;SslMode=none;Max pool size=1;Connection LifeTime=20",
    "SqlServer": "Data Source=192.168.1.19;User Id=sa;Password=123456;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=1"
  },
```


强类型绑定对象
```csharp
    services.Configure<AppOption>(configuration.GetSection(nameof(AppOption)));
```

实体
```csharp
public class AppOption
{
    public string TemplatesPath { get; set; }
    public string OutputDirectory { get; set; }
}
```

appsettings.json配置json对象
```json
 "AppOption": {
    "TemplatesPath": "./Templates", //相对路径，当前项目下的Templates目录
    "OutputDirectory": "../../../Output" //可以是相对路径，也可以是绝对路径
  }
```


使用
```csharp
public class App
{
    private readonly ILogger<App> _logger;
    private readonly AppOption _appOption;
    public App(ILogger<App> logger, IOptions<AppOption> appOption)
    {
        _logger = logger;
        _appOption = appOption.Value;
    }
}
```


爬虫`HtmlAgilityPack`，Josn序列化`Newtonsoft.Json`，友好的帮助类`Humanizer`，模板引擎`Scriban`
```xml
	<PackageReference Include="HtmlAgilityPack" Version="1.11.36" />
	<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
	<PackageReference Include="Humanizer.Core" Version="2.11.10" />
	<PackageReference Include="Scriban" Version="4.0.1" />
```