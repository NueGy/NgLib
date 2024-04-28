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
    /// Permet de gérer les tokens d'authentification sur un HttpClient
    /// </summary>
    public class HttpClientTokenHandler : System.Net.Http.DelegatingHandler
    {
        /// <summary>
        /// Elements de configuration pour la génération du token
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
        /// Dernier Token généré
        /// </summary>
        public string LastToken { get; set; }

        /// <summary>
        /// Date de chargement du dernier token
        /// </summary>
        public DateTime? LastTokenDate { get; set; }

        /// <summary>
        /// Temps d'expiration du token
        /// Si 0, le token n'expire pas ou expiration non géré
        /// </summary>
        public int LastTokenExpireSeconds { get; set; }


        /// <summary>
        /// Pour les tokens de rafraichissement Oauth2
        /// </summary>
        public string LastRefreshToken { get; set; }


        // todo: Ajouter un mutex pout éviter les refresh token en parallèle si Oauth2



        

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            if(this.Config ==null || this.Config.AuthType == TokenAuthTypeEnum.none)
            {
                // no new token
                // Ajoute le token dans la requête si il existe mais n'en générera pas de nouveau
                HttpClientTools.SetBearerToken(request, this.LastToken); 
            }
            else if (this.Config.AuthType == TokenAuthTypeEnum.FixedBearerToken)
            {
                // Bearer : Ajouter le token dans le header
                if(string.IsNullOrEmpty(this.Config.FixedToken)) throw new Exception("FixedToken is empty in HttpClientConfigModel");
                HttpClientTools.SetBearerToken(request, this.Config.FixedToken);
            }
            else if (this.Config.AuthType == TokenAuthTypeEnum.Basic)
            {
                // Basic : Ajouter le username/password dans le header
                HttpClientTools.SetBasicAuth(request, this.Config.Username, this.Config.Password);
            }
            else if (this.Config.AuthType == TokenAuthTypeEnum.JwtHmac)
            {
                // JwtHmac : Génération d'un nouveau token Hs256
                throw new NotImplementedException("JwtHmac not implemented");
                //this.LastToken = TokenJwtTools.EncodeBasicJWT(this.TokenConfig.AppSecret, "na", this)
               // HttpClientTools.SetBasicAuth(request, this.TokenConfig.Username, this.TokenConfig.Password);
            }
            else
            {
                    // Oauth2
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

                handler.AllowAutoRedirect = !this.Config.DisableAutoRedirect;// pas de redirect pour les api ?
            }
            return handler;
        }


    }
}
