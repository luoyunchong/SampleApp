using FreeSql.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleApp.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SampleApp.Extensions;

namespace SampleApp
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppOption _appOption;
        private readonly IConfiguration _configuration;
        private readonly IFreeSql _fsql;
        private readonly IFreeSql<MySqlFlag> _mysql;
        public App(ILogger<App> logger, IOptions<AppOption> appOption, IConfiguration configuration, IFreeSql fsql, IFreeSql<MySqlFlag> mysql)
        {
            _logger = logger;
            _appOption = appOption.Value;
            _configuration = configuration;
            _fsql = fsql;
            _mysql = mysql;
        }

        public async Task RunAsync(string[] args)
        {
            _logger.LogInformation("App Run Start");
            
            
            await Task.FromResult(0);
            _logger.LogInformation("App Run End!");
        }


        public async Task SaveDBAsync()
        {

        }
    }

}
