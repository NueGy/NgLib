using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    public static class HttpClientContextTools
    {
        public static bool IsTokenExpired(this IHttpClientContext httpClientContext)
        {
            if (httpClientContext == null) return false;
            if (string.IsNullOrEmpty(httpClientContext.LastToken)) return true;
            if (httpClientContext.LastToken == " ") return false;
            if (!httpClientContext.LastTokenDate.HasValue || httpClientContext.LastTokenDate.Value.Year < 2000)
                return true;
            if (httpClientContext.ExpireTokenSeconds < 1) return true; // non géré
            if (httpClientContext.LastTokenDate.Value.AddSeconds(httpClientContext.ExpireTokenSeconds) <
                DateTime.Now) return true;
            return false;
        }

        public static void SetToken(this IHttpClientContext httpClientContext, string token, int? expireSeconds = null)
        {
            if (httpClientContext == null) return;
            httpClientContext.LastToken = token;
            httpClientContext.LastTokenDate = DateTime.Now;
            if (expireSeconds.HasValue) httpClientContext.ExpireTokenSeconds = expireSeconds.Value;
        }


        public static async Task<HttpResponseMessage> ExecuteAsync(this IHttpClientContext httpClientContext,
            HttpRequestMessage requestMessage)
        {
            if (requestMessage == null) return null;
            try
            {
                if (httpClientContext.Client == null)
                    httpClientContext
                        .SetHttpClient(new HttpClient()); //throw new Exception("httpClientContext.Client NULL");
                var retour = await httpClientContext.Client.SendAsync(requestMessage);
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"HttpSendAsync({requestMessage.RequestUri} [{requestMessage.Method}]) " + ex.Message, ex);
            }
        }
    }
}