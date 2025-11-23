using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Defines the authentication type configuration. Use with HttpClientTools.CreateHttpClient()
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
        /// Free text
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Base URL
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Authentication method
        /// </summary>
        public TokenAuthTypeEnum AuthType { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
 
        /// <summary>
        /// Client ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Client Secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Authentication endpoint URL
        /// </summary>
        public string TokenEndpointUrl { get; set; }


        /// <summary>
        /// Pre-defined token (use with AuthType=none)
        /// </summary>
        public string FixedToken { get; set; }

        /// <summary>
        /// Additional options
        /// </summary>
        public Dictionary<string, string> MoreParameters { get; set; }


        /// <summary>
        /// Define a proxy (Only if not using inner handler). Example: http://proxy:8080
        /// </summary>
        public string ProxyUrl { get; set; }

        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }


        /// <summary>
        /// Disable SSL validation (Only if not using inner handler). Sets: httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        /// </summary>
        public bool DisableSslValidation { get; set; }

        /// <summary>
        /// Handler should follow redirection responses
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
