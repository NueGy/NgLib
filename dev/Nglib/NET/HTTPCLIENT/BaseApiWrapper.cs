using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Nglib.SECURITY.CRYPTO;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    ///     Permet de conce
    /// </summary>
    public abstract class BaseApiWrapper
    {
        public IHttpClientContext apiAuthContext;


        public BaseApiWrapper(IHttpClientContext apiAuthContext, string baseparturl = null)
        {
            BasePartUrl = baseparturl;
            this.apiAuthContext = apiAuthContext;
        }

        public string BasePartUrl { get; set; }


        public virtual HttpRequestMessage PrepareRequest(HttpMethod method, string ServicePartUrl)
        {
            var requestMessage = HttpClientTools.PrepareRequest(method, apiAuthContext?.RootUrl, ServicePartUrl);


            string bearertoken = null;
            if (!string.IsNullOrWhiteSpace(apiAuthContext?.LastToken))
            {
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("bearer", apiAuthContext?.LastToken);
            }
            //else if (!string.IsNullOrWhiteSpace(apiAuthContext?.AppSecret) && false)
            //{
            //    bearertoken = TokenJwtTools.EncodeBasicJWT(apiAuthContext?.AppSecret, "na", apiAuthContext?.AppKey);
            //    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", bearertoken);
            //}

            return requestMessage;
        }

        public virtual JsonSerializerOptions DefaultJsonSerializerOptions()
        {
            var retour = new JsonSerializerOptions();

            return retour;
        }
    }
}