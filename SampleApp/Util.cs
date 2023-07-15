using HtmlAgilityPack;
using System.Net;
using System.Security.Cryptography;
using System.Text;

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

}
