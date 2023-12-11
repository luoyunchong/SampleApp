using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleApp.Services;

public interface IRestClient
{
    Task<bool> DownLoadAsync(string uri, string localFileName);
    Task<string> PostDataAsync(Dictionary<string, string> _headers, string url, string postData, bool needZip = true);
    Task<HttpResponseMessage> PostResponseMessageAsync(Dictionary<string, string> headers, string url, string postData);

    Task<T> GetAsync<T>(Dictionary<string, string> _headers, string url);
}