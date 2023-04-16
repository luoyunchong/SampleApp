using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp;

class Util
{
    public static string Md5(string inputValue)
    {
        using (var md5 = MD5.Create())
        {
            var result = md5.ComputeHash(Encoding.UTF8.GetBytes(inputValue));
            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }

    }
    public static HtmlWeb GetHtmlWeb(Dictionary<string, string> _headers)
    {
        HtmlWeb web = new HtmlWeb();

        HtmlWeb.PreRequestHandler handler = delegate (HttpWebRequest request)
        {
            foreach (var header in _headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.CookieContainer = new CookieContainer();
            return true;
        };
        web.PreRequest += handler;
        web.OverrideEncoding = Encoding.Default;

        return web;
    }

    /// <summary>
    /// 发送POST请求，支持gzip压缩
    /// </summary>
    /// <param name="url"></param>
    /// <param name="postData">post请求数据，序列化好的数据</param>
    /// <returns></returns>
    public static async Task<string> PostDataAsync(Dictionary<string, string> _headers, string url, string postData, bool needZip = true)
    {
        string contentType = "application/x-www-form-urlencoded";
        using (HttpClient client = new HttpClient())
        {
            if (_headers != null)
            {
                foreach (var header in _headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            using (HttpContent httpContent = new StringContent(postData, Encoding.UTF8))
            {
                string result = "";
                if (contentType != null)
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                HttpResponseMessage response = await client.PostAsync(url, httpContent);

                if (needZip)
                {
                    Stream myResponseStream = await response.Content.ReadAsStreamAsync();

                    GZipStream gzip = new GZipStream(myResponseStream, CompressionMode.Decompress);//解压缩
                    StreamReader myStreamReader = new StreamReader(gzip, Encoding.UTF8);
                    result = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();
                    return result;
                }
                else
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }

}
