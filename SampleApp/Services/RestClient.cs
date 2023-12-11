using System.Text;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace SampleApp.Services;

public class RestClient : IRestClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RestClient> _logger;
    public RestClient(IHttpClientFactory httpClientFactory, ILogger<RestClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    ///<summary>
    /// 下载文件
    /// </summary>
    /// <param name="serverFileName">服务器上文件名 如 close.png</param>
    /// <param name="localFileName">要保存到本地的路径全名 如：C://Download/close.png</param>
    /// <returns></returns>
    public async Task<bool> DownLoadAsync(string uri, string localFileName)
    {
        if (uri.StartsWith("//"))
        {
            uri = "http:" + uri;
        }
        if (!uri.StartsWith("http"))
        {
            return false;
        }
        try
        {
            Uri server = new Uri(uri);
            string? p = Path.GetDirectoryName(localFileName);
            if (!Directory.Exists(p)) Directory.CreateDirectory(p);

            // 发起请求并异步等待结果
            using var httpClient = _httpClientFactory.CreateClient();
            var responseMessage = await httpClient.GetAsync(server);
            if (responseMessage.IsSuccessStatusCode)
            {
                await using var fs = File.Create(localFileName);
                // 获取结果，并转成 stream 保存到本地。
                var stream = await responseMessage.Content.ReadAsStreamAsync();
                await stream.CopyToAsync(fs);
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            _logger.LogError($"Message:{e.Message},StackTrace:{e.StackTrace}");
            return false;
        }
    }
    /// <summary>
    /// 发送POST请求，支持gzip压缩
    /// </summary>
    /// <param name="url"></param>
    /// <param name="postData">post请求数据，序列化好的数据</param>
    /// <returns></returns>
    public async Task<string> PostDataAsync(Dictionary<string, string> _headers, string url, string postData, bool needZip = true)
    {
        string contentType = "application/x-www-form-urlencoded";
        using HttpClient client = _httpClientFactory.CreateClient();
        if (_headers != null)
        {
            foreach (var header in _headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
        using HttpContent httpContent = new StringContent(postData, Encoding.UTF8);
        string result = "";
        if (contentType != null)
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        HttpResponseMessage response = await client.PostAsync(url, httpContent);
        if (needZip)
        {
            Stream myResponseStream = await response.Content.ReadAsStreamAsync();

            GZipStream gzip = new GZipStream(myResponseStream, CompressionMode.Decompress);//解压缩
            StreamReader myStreamReader = new StreamReader(gzip, Encoding.UTF8);
            result = await myStreamReader.ReadToEndAsync();
            myStreamReader.Close();
            myResponseStream.Close();
            return result;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<HttpResponseMessage> PostResponseMessageAsync(Dictionary<string, string> headers, string url, string postData)
    {
        string contentType = "application/x-www-form-urlencoded";
        using HttpClient client = _httpClientFactory.CreateClient();
        if (headers != null)
        {
            foreach (var header in headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
        using HttpContent httpContent = new StringContent(postData, Encoding.UTF8);
        if (contentType != null)
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        HttpResponseMessage response = await client.PostAsync(url, httpContent);

        return response;
    }

    public async Task<T> GetAsync<T>(Dictionary<string, string> headers, string url)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
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