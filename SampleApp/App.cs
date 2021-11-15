using FreeSql.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleApp.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
            _logger.LogInformation("App Run End!");
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

        public async Task SaveDBAsync()
        {
            var time = DateTime.Now;

            await fsql.Insert<TT>(new TT { Create = time }).ExecuteAffrowsAsync();
            await fsql.Insert<TT2>(new TT2 { Create = time }).ExecuteAffrowsAsync();

            fsql.Transaction(() =>
           {
               fsql.Insert<TT>(new TT { Create = time }).ExecuteAffrows();
               fsql.Update<TT2>().SetSource(new TT2 { Id = 1, Create = time }).ExecuteAffrows();
           });
        }
    }

    public class TT
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime Create { get; set; }
    }

    public class TT2
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime Create { get; set; }
    }
}
