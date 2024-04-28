using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// permet de définir le type d'authentification
    /// Use with HttpClientTools.CreateHttpClient()
    /// </summary>
    public class HttpClientConfigModel
    {
        public HttpClientConfigModel()
        {
            this.AuthType =TokenAuthTypeEnum.none;
        }

        public HttpClientConfigModel(string baseUrl)
        {
            this.AuthType = TokenAuthTypeEnum.none;
            this.BaseUrl = baseUrl;
        }
        public HttpClientConfigModel(TokenAuthTypeEnum authType)
        {
            this.AuthType = authType;
        }


        /// <summary>
        /// Texte libre
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Url de base
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Méthode d'authentification
        /// </summary>
        public TokenAuthTypeEnum AuthType { get; set; }

        /// <summary>
        /// Username de l'utilisateur
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password de l'utilisateur
        /// </summary>
        public string Password { get; set; }
 
        /// <summary>
        /// ClientId
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// ClientSecret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Url de l'endpoint de l'authentification
        /// </summary>
        public string TokenEndpointUrl { get; set; }


        /// <summary>
        /// Token pré-défini (use with AuthType=none)
        /// </summary>
        public string FixedToken { get; set; }

        /// <summary>
        /// Options supplémentaires 
        /// </summary>
        public Dictionary<string, string> MoreParameters { get; set; }


        /// <summary>
        /// Définir un proxy (Only if not use inner handler)
        /// exemple: http://proxy:8080
        /// </summary>
        public string ProxyUrl { get; set; }

        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }


        /// <summary>
        /// Désactiver la validation SSL (Only if not use inner handler)
        /// httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        /// </summary>
        public bool DisableSslValidation { get; set; }

        /// <summary>
        /// handler should follow redirection Responses
        /// </summary>
        public bool DisableAutoRedirect { get; set; }


        public static HTTPCLIENT.HttpClientConfigModel PrepareWithFixedToken(string token, string baseurl=null)
        {
            HttpClientConfigModel retour = new HTTPCLIENT.HttpClientConfigModel();
            retour.AuthType = TokenAuthTypeEnum.FixedBearerToken;
            retour.FixedToken = token;
            retour.BaseUrl = baseurl;
            return retour;
        }

    }
}
