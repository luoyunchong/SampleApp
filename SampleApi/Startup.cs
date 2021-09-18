using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using IGeekFan.AspNetCore.RapiDoc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;

namespace SampleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //IFreeSql fsql = new FreeSql.FreeSqlBuilder()
            ////.UseConnectionString(FreeSql.DataType.Sqlite, Configuration["ConnectionStrings:DefaultConnection"])
            //.UseConnectionString(FreeSql.DataType.MySql, Configuration["ConnectionStrings:MySql"])
            //.UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower)
            //.UseAutoSyncStructure(true)
            ////.UseGenerateCommandParameterWithLambda(true)
            //.UseLazyLoading(true)
            //.UseMonitorCommand(
            //    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText)
            //    )
            //.Build();

            //db 是一个静态类，并非实例化
            //事先或运行中注册 IFreeSql

            var db= StaticDB.Instance;

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


            var fsql2 = new MultiFreeSql();
            fsql2.Register("db1", () => new FreeSqlBuilder().UseAutoSyncStructure(true).UseConnectionString(DataType.Sqlite, "Data Source=|DataDirectory|\\SampleApp1.db;").Build());
            fsql2.Register("db2", () => new FreeSqlBuilder().UseAutoSyncStructure(true).UseConnectionString(DataType.Sqlite, "Data Source=|DataDirectory|\\SampleApp2.db;").Build());
            services.AddSingleton<IFreeSql>(fsql2);

            //services.AddSingleton(fsql);
            //services.AddFreeRepository();
            //services.AddScoped<UnitOfWorkManager>();

            //fsql.CodeFirst.Entity<SysUser>(eb =>
            //{
            //    eb.HasData(new List<SysUser>() { new SysUser() { Name = "ceshi" } });
            //});
            //fsql.CodeFirst.SyncStructure<SysUser>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApi v1"));
            }

            var d = JsonConvert.SerializeObject(new { code = true });

            app.UseRapiDocUI(c =>
            {
                c.RoutePrefix = ""; // serve the UI at root
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApi v1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
