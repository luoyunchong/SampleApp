using SampleApp.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SampleAppTests
{
    public class RestClientTest
    {
        IRestClient restClient;

        public RestClientTest(IRestClient restClient)
        {
            this.restClient = restClient;
        }

        [Fact]
        public async Task Test1()
        {
            //await restClient.GetAsync<>(null, null);
        }
    }
}
