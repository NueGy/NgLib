using System;
using System.Net.Http;

namespace Nglib.NET.HTTPCLIENT
{
    public class HttpClientContext : IHttpClientContext
    {
        public HttpClient Client { get; set; }


        public string RootUrl { get; set; }

        public string AppKey { get; set; }

        public string AppSecret { get; set; }

        public string LastToken { get; set; }
        public DateTime? LastTokenDate { get; set; }
        public int ExpireTokenSeconds { get; set; }

        public void SetHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}