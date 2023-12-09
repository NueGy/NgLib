using System;
using System.Net.Http;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    ///     Contient tous le nécessaires pour traiter un api distant
    /// </summary>
    public interface IHttpClientContext
    {
        /// <summary>
        ///     Url de base du webservice distant
        /// </summary>
        string RootUrl { get; set; }

        /// <summary>
        ///     ClientId pour Oauth2
        /// </summary>
        string AppKey { get; set; }

        /// <summary>
        ///     ClientSecret pour Oauth2
        /// </summary>
        string AppSecret { get; set; }

        /// <summary>
        ///     Dernier Bearer Token
        /// </summary>
        string LastToken { get; set; }

        /// <summary>
        ///     Date de chargement du dernier token
        /// </summary>
        DateTime? LastTokenDate { get; set; }

        /// <summary>
        ///     Temps d'expiration du toeken
        /// </summary>
        int ExpireTokenSeconds { get; set; }


        /// <summary>
        ///     HttpClient
        /// </summary>
        HttpClient Client { get; }


        //System.Net.Http.HttpRequestMessage PrepareRequest(System.Net.Http.HttpMethod method, string reqUri);

        //Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage requestMessage);


        //System.Text.Json.JsonSerializerOptions DefaultJsonSerializerOptions();

        /// <summary>
        ///     Permet de définir le client dans le context
        /// </summary>
        /// <param name="client"></param>
        void SetHttpClient(HttpClient client);


        // EVENTS
        // -OK
        // -ERROR
    }
}