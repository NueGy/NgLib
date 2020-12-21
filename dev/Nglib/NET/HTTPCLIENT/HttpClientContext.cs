using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.NET.HTTPCLIENT
{

    public interface IHttpClientContext
    {
        string ClientId { get;  }
        string ClientSecret { get;  }
        string TokenUrl { get;  }
        string RootUrl { get;  }
        Oauth2TokenResponse Oauth2TokenResponse { get; set; }
        HttpClientTokenModeEnum TokenMode { get; }

        Dictionary<string, object> JwtPayload { get; }


    }

    public class HttpClientContext : IHttpClientContext
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TokenUrl { get; set; }
        public string RootUrl { get; set; }
        public Dictionary<string, object> JwtPayload { get; set; } = new Dictionary<string, object>();
        public Oauth2TokenResponse Oauth2TokenResponse { get; set; }
        public HttpClientTokenModeEnum TokenMode { get; set; } 


    }

    public class Oauth2TokenResponse
    {
        public string Token { get; set; }
        public DateTime TokenCreate { get; set; }
        public int TokenExpire { get; set; }

    }


    public enum HttpClientTokenModeEnum 
    {
        /// <summary>
        /// pas de toke,
        /// </summary>
        NO,
        /// <summary>
        /// Envoi le login password en mode basic
        /// </summary>
        BASIC,
        /// <summary>
        /// Génération d'un token unique JWT hs256
        /// </summary>
        BEARERJWTHS256,
        /// <summary>
        /// Génération d'un token unique JWT RS256 RSA ASYNC
        /// </summary>
        BEARERJWTRS256,
        /// <summary>
        /// Envoi du login password en échange d'un token
        /// </summary>
        OAUTH2CLIENTCREDENTIAL,
        /// <summary>
        /// Echange un token HS256 contre un nouveau token
        /// </summary>
        OAUTH2BEARER,
    }

}
