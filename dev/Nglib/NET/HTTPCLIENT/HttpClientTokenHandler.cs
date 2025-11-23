using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Manages authentication tokens for HttpClient
    /// </summary>
    public class HttpClientTokenHandler : System.Net.Http.DelegatingHandler
    {
        /// <summary>
        /// Configuration elements for token generation
        /// </summary>
        public HttpClientConfigModel Config { get; set; }

        public HttpClientTokenHandler() : base()
        {
            InnerHandler = GetDefaultInnerHandler();
        }

        public HttpClientTokenHandler(System.Net.Http.HttpMessageHandler innerHandler) : base(innerHandler)
        {

        }

        [Obsolete("SOON")]
        public HttpClientTokenHandler(HttpClientConfigModel conf) : base()
        {
            this.Config = conf;
            InnerHandler = GetDefaultInnerHandler();
        }

        [Obsolete("SOON")]
        public HttpClientTokenHandler(System.Net.Http.HttpMessageHandler innerHandler, HttpClientConfigModel conf) : base(innerHandler)
        {
            this.Config = conf;
        }


        /// <summary>
        /// Last generated token
        /// </summary>
        public string LastToken { get; set; }

        /// <summary>
        /// Date when the last token was loaded
        /// </summary>
        public DateTime? LastTokenDate { get; set; }

        /// <summary>
        /// Token expiration time. If 0, the token does not expire or expiration is not managed
        /// </summary>
        public int LastTokenExpireSeconds { get; set; }


        /// <summary>
        /// For OAuth2 refresh tokens
        /// </summary>
        public string LastRefreshToken { get; set; }


        // TODO: Add mutex to avoid parallel refresh token if OAuth2



        

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            if(this.Config ==null || this.Config.AuthType == TokenAuthTypeEnum.none)
            {
                // No new token
                // Add token to request if it exists but will not generate a new one
                HttpClientTools.SetBearerToken(request, this.LastToken); 
            }
            else if (this.Config.AuthType == TokenAuthTypeEnum.FixedBearerToken)
            {
                // Bearer: Add token to header
                if(string.IsNullOrEmpty(this.Config.FixedToken)) throw new Exception("FixedToken is empty in HttpClientConfigModel");
                HttpClientTools.SetBearerToken(request, this.Config.FixedToken);
            }
            else if (this.Config.AuthType == TokenAuthTypeEnum.Basic)
            {
                // Basic: Add username/password to header
                HttpClientTools.SetBasicAuth(request, this.Config.Username, this.Config.Password);
            }
            else if (this.Config.AuthType == TokenAuthTypeEnum.JwtHmac)
            {
                // JwtHmac: Generate a new HS256 token
                throw new NotImplementedException("JwtHmac not implemented");
                //this.LastToken = TokenJwtTools.EncodeBasicJWT(this.TokenConfig.AppSecret, "na", this)
               // HttpClientTools.SetBasicAuth(request, this.TokenConfig.Username, this.TokenConfig.Password);
            }
            else
            {
                    // OAuth2
                 if (HTTPCLIENT.HttpClientTokenTools.IsTokenExpired(this))
                        await HTTPCLIENT.HttpClientTokenTools.RefreshTokenOAuth2Async(this);
                
            }


            return await base.SendAsync(request, cancellationToken);
        }



        override protected void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

 

        protected virtual HttpMessageHandler GetDefaultInnerHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (!string.IsNullOrEmpty(this.Config?.ProxyUrl))
            {
                 handler.Proxy = new System.Net.WebProxy(this.Config.ProxyUrl);
                if (!string.IsNullOrWhiteSpace(this.Config.ProxyUsername))
                    handler.Proxy.Credentials = new System.Net.NetworkCredential(this.Config.ProxyUsername, this.Config.ProxyPassword);
            }

            if (this.Config != null)
            {
                if (this.Config?.DisableSslValidation == true)
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                handler.AllowAutoRedirect = !this.Config.DisableAutoRedirect;// No redirect for APIs?
            }
            return handler;
        }


    }
}
