using FreeSql.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleApp.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleApp
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppOption _appOption;
        private readonly IConfiguration configuration;
        private readonly IFreeSql fsql;
        private readonly IHttpClientFactory httpClientFactory;
        public App(ILogger<App> logger, IOptions<AppOption> appOption, IConfiguration configuration, IFreeSql fsql, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _appOption = appOption.Value;
            this.configuration = configuration;
            this.fsql = fsql;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task RunAsync(string[] args)
        {
            _logger.LogInformation("App Run Start");
            await Task.FromResult(0);

            using (var ctx = fsql.CreateDbContext())
            {
                //var db1 = ctx.Set<Song>();
                //var db2 = ctx.Set<Tag>();

                var item1 = new Song { Id = Guid.NewGuid().ToString() };
                var item2 = new Song { Id = Guid.NewGuid().ToString() };
                await ctx.AddRangeAsync(new List<Song> { item1, item2 });
                ctx.SaveChanges();
            }
            fsql.Select<Song>().ToList();
            _logger.LogInformation("App Run End!");
        }


        public async Task SaveDBAsync()
        {

        }
    }

    [Table(Name = "Order")]
    internal class Song
    {
        [Column(IsPrimary = true)]
        public string Id { get; set; }
        public string NN { get; set; }
    }
}
