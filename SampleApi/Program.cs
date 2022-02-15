using IGeekFan.AspNetCore.RapiDoc;
using SampleApi;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var services = builder.Services;
var Configuration = builder.Configuration;

#region Serilog配置
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)
       .Enrich.FromLogContext()
       .CreateLogger();
#if DEBUG
Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
#endif
#endregion

services
    .AddJwt(Configuration)
    .AddSwagger(Configuration);

services.AddMultiFreeSql().AddControllers();

var app = builder.Build();

// 配置 HTTP请求中间件
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApi v1"));
    //https://github.com/luoyunchong/IGeekFan.AspNetCore.RapiDoc
    app.UseRapiDocUI(c =>
    {
        c.RoutePrefix = ""; // serve the UI at root
        c.GenericRapiConfig = new GenericRapiConfig()
        {
            RenderStyle = "focused",
            Theme = "light",//light,dark,focused   
        };
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApi v1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
