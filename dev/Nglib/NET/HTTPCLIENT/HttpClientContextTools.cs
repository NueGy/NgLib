using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Permet de manipuler un contexte de client http IHttpClientContext
    /// </summary>
    [Obsolete("Use HttpClientTokenHandler, Deleted in next version")]
    public static class HttpClientContextTools
    {
        /// <summary>
        /// Si le token à expiré
        /// </summary>
        public static bool IsTokenExpired(this IHttpClientContext httpClientContext)
        {
            if (httpClientContext == null) return false;
            if (string.IsNullOrEmpty(httpClientContext.LastToken)) return true; // si pas de token alors forcé de le renouveller
            if (httpClientContext.LastToken == " ") return false;//??
            if (!httpClientContext.LastTokenDate.HasValue || httpClientContext.LastTokenDate.Value.Year < 2000)
                return true;
            if (httpClientContext.ExpireTokenSeconds < 1) return true; // non géré
            if (httpClientContext.LastTokenDate.Value.AddSeconds(httpClientContext.ExpireTokenSeconds) <
                DateTime.Now) return true; // le token est expiré
            return false;
        }

        /// <summary>
        /// Définir un token
        /// </summary>
        public static void SetToken(this IHttpClientContext httpClientContext, string token, int? expireSeconds = null)
        {
            if (httpClientContext == null) return;
            httpClientContext.LastToken = token;
            httpClientContext.LastTokenDate = DateTime.Now;
            if (expireSeconds.HasValue) httpClientContext.ExpireTokenSeconds = expireSeconds.Value;

            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

        }




        /// <summary>
        /// Execute HTTP Request : Context.Client.SendAsync
        /// </summary>
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


        /// <summary>
        /// Permet de mapper avec les nouveaux objets
        /// </summary>
        public static HttpClientConfigModel MapToConfig(IHttpClientContext context)
        {
            HttpClientConfigModel conf = new HttpClientConfigModel();
            conf.AuthType = TokenAuthTypeEnum.none;
            conf.ClientId = context.AppKey;
            conf.ClientSecret = context.AppSecret;
            conf.BaseUrl = context.RootUrl;
            conf.FixedToken = context.LastToken;
            if(!string.IsNullOrWhiteSpace(conf.ClientSecret)) conf.AuthType = TokenAuthTypeEnum.OAuth2Client;
            if(!string.IsNullOrWhiteSpace(conf.FixedToken)) conf.AuthType = TokenAuthTypeEnum.FixedBearerToken;
            return conf;

        }



    }
}