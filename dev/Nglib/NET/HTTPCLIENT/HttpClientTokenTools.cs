using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Tools for managing HTTP client token context (IHttpClientContext)
    /// </summary>
    public static class HttpClientTokenTools
    {
        /// <summary>
        /// Checks if the token has expired
        /// </summary>
        public static bool IsTokenExpired(this HttpClientTokenHandler httpClientContext)
        {
            if (httpClientContext == null) return false;
            if (string.IsNullOrEmpty(httpClientContext.LastToken)) return true; // if no token, must renew
            if (httpClientContext.LastToken == " ") return false;//??
            if (!httpClientContext.LastTokenDate.HasValue || httpClientContext.LastTokenDate.Value.Year < 2000)
                return true;
            if (httpClientContext.LastTokenExpireSeconds < 1) return true; // not managed
            if (httpClientContext.LastTokenDate.Value.AddSeconds(httpClientContext.LastTokenExpireSeconds) <
                DateTime.Now) return true; // token is expired
            return false;
        }

        /// <summary>
        /// Updates the token in the handler
        /// </summary>
        /// <param name="httpClientContext"></param>
        /// <param name="token"></param>
        /// <param name="expireIn"></param>
        public static void SetToken(this HttpClientTokenHandler httpClientContext, string token, int expireIn = 0)
        {
            if (httpClientContext == null) return;
            httpClientContext.LastToken = token;
            httpClientContext.LastTokenDate = DateTime.Now;
            httpClientContext.LastTokenExpireSeconds = expireIn;
        }


        /// <summary>
        /// Generates an HMAC HS256 token
        /// </summary>
        /// <param name="httpClientContext"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [Obsolete("SOON")]
        public static async Task<string> GenerateHmacTokenAsync(this HttpClientTokenHandler httpClientContext)
        {
            if (httpClientContext?.Config == null) return null;
            throw new NotImplementedException();
        }





        /// <summary>
        /// Obtains a new token via an HTTP call
        /// </summary>
        public static async Task<bool> RefreshTokenOAuth2Async(this HttpClientTokenHandler httpClientContext)
        {
            if (httpClientContext?.Config == null) return false;
            try
            {
                // Obtain a new token via an HTTP call
                //if (string.IsNullOrEmpty(httpClientContext.TokenConfig.ClientId)) throw new Exception("ClientId is required");
                //if (string.IsNullOrEmpty(httpClientContext.TokenConfig.ClientSecret)) throw new Exception("ClientSecret is required");
                if (string.IsNullOrEmpty(httpClientContext.Config.TokenEndpointUrl)) throw new Exception("TokenEndpointurl is required");

                Dictionary<string,object> reqParameters = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(httpClientContext.LastRefreshToken))
                {
                    if (string.IsNullOrWhiteSpace(httpClientContext.Config.ClientId)) throw new Exception("ClientId is required for OAuth2Refresh");
                    if (string.IsNullOrWhiteSpace(httpClientContext.Config.ClientSecret)) throw new Exception("ClientSecret is required for OAuth2Refresh");

                }
                else if (httpClientContext.Config.AuthType ==  TokenAuthTypeEnum.OAuth2Client)
                {
                    if(string.IsNullOrWhiteSpace(httpClientContext.Config.ClientId)) throw new Exception("ClientId is required for OAuth2Client");
                    if(string.IsNullOrWhiteSpace(httpClientContext.Config.ClientSecret)) throw new Exception("ClientSecret is required for OAuth2Client");
                    reqParameters.Add("grant_type", "client_credentials");
                    reqParameters.Add("client_id", httpClientContext.Config.ClientId);
                    reqParameters.Add("client_secret", httpClientContext.Config.ClientSecret);
                }
                else if (httpClientContext.Config.AuthType == TokenAuthTypeEnum.OAuth2Password)
                {
                    if (string.IsNullOrWhiteSpace(httpClientContext.Config.Username)) throw new Exception("Username is required for OAuth2Password");
                    if (string.IsNullOrWhiteSpace(httpClientContext.Config.Password)) throw new Exception("Password is required for OAuth2Password");
                    reqParameters.Add("grant_type", "password");
                    reqParameters.Add("username", httpClientContext.Config.Username);
                    reqParameters.Add("password", httpClientContext.Config.Password);
                    if (string.IsNullOrWhiteSpace(httpClientContext.Config.ClientId))
                        reqParameters.Add("client_id", httpClientContext.Config.ClientId);
                    if (string.IsNullOrWhiteSpace(httpClientContext.Config.ClientSecret))
                        reqParameters.Add("client_secret", httpClientContext.Config.ClientSecret);
                }
                else
                    throw new Exception("AuthType not supported in Oauth2");

                var content = HttpClientTools.PrepareFormUrlContent(reqParameters);

                HttpClient client = new HttpClient();// use factory?
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "HttpClientTokenHandler");


                var response = await client.PostAsync(httpClientContext.Config.TokenEndpointUrl, content);
                if (!response.IsSuccessStatusCode) throw new Exception("Error on token request: " + response.StatusCode + " " + response.ReasonPhrase);
                var tokenResponse = await HttpClientTools.ReadAsync<OAuthModels.Oauth2TokenResponse>(response);
                if (tokenResponse == null) throw new Exception("Error on token request: no response");
                if (string.IsNullOrWhiteSpace(tokenResponse.access_token)) throw new Exception("Error on token request: no access_token");
               
                SetToken(httpClientContext, tokenResponse.access_token, tokenResponse.expires_in);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("RefreshTokenOAuth2Async " + ex.Message, ex);
            }
        }




    }
}