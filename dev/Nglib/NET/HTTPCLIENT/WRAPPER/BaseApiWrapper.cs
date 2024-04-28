using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Nglib.SECURITY.CRYPTO;

namespace Nglib.NET.HTTPCLIENT.WRAPPER
{
    /// <summary>
    ///  Wrapper de base pour les API
    /// </summary>
    public abstract class BaseApiWrapper : IApiWrapper
    {
        /// <summary>
        /// Objet Principal de connexion
        /// </summary>
        public HttpClient HttpClient { get; protected set; }

        /// <summary>
        /// Configuration de connexion (Uniquement si instancié par le constructeur)
        /// </summary>
        public HttpClientConfigModel ApiAuthConfig { get; protected set; }






        [Obsolete("Deleted in next version")]
        public BaseApiWrapper(IHttpClientContext apiAuthContext, string baseparturl = null)
        {
            HttpClientConfigModel apiAuthConfig = HttpClientContextTools.MapToConfig(apiAuthContext);
        }

        public BaseApiWrapper(string rootUrl)
        {
            this.ApiAuthConfig = new HttpClientConfigModel(rootUrl);
            this.HttpClient = HttpClientTools.CreateNewClient(ApiAuthConfig);
        }

        public BaseApiWrapper(HttpClientConfigModel apiAuthConfig)
        {
            if (apiAuthConfig == null) throw new NullReferenceException("ApiWrapper: HttpClientConfigModel is null ");
            this.HttpClient = HttpClientTools.CreateNewClient(apiAuthConfig);
        }

        public BaseApiWrapper(HttpClient httpClient)
        {
            if (httpClient == null) throw new NullReferenceException("ApiWrapper: HttpClient is null ");
            this.HttpClient = httpClient;
        }



        /// <summary>
        /// SendAsync
        /// Equivalent à this.httpClient.SendAsync(request)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request)
        {
            if(request==null)return null;
            if (this.HttpClient == null)
                throw new Exception("HttpClient is null");

            return this.HttpClient.SendAsync(request);
        }
     


        protected virtual HttpRequestMessage PrepareRequest(HttpMethod method, string ServicePartUrl)
        {
            var requestMessage = HttpClientTools.PrepareRequest(method,  ServicePartUrl);
            return requestMessage;
        }

        protected virtual JsonSerializerOptions DefaultJsonSerializerOptions()
        {
            var retour = new JsonSerializerOptions();

            return retour;
        }
    }
}