﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Models Oauth2 pour sérialiation
    /// </summary>
    public static class OAuthModels
    {
        //https://www.oauth.com/oauth2-servers/access-tokens/access-token-response/

        public class Oauth2TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public string scope { get; set; }
            public int expires_in { get; set; }

        }


        public class Oauth2ErrorResponse
        {
            public string error { get; set; }
            public string error_description { get; set; }
            public string error_uri { get; set; }

        }

        /// <summary>
        /// HTTPGET
        /// </summary>
        public class Oauth2TokenRequest
        {
            /// <summary>
            /// client_credentials
            /// </summary>
            public string grant_type { get; set; }

            public string client_id { get; set; }
            public string client_secret { get; set; }
            public string username { get; set; }
            public string password { get; set; }

            public string refresh_token { get; set; }
            

            /// <summary>
            /// 
            /// </summary>
            public string redirect_uri { get; set; }

            /// <summary>
            /// authorization_code
            /// </summary>
            public string code { get; set; }


            public Dictionary<string, string> customs { get; set; }
        }

      

        public static bool Validate(Oauth2TokenResponse model)
        {
            if (string.IsNullOrWhiteSpace(model?.access_token)) throw new Exception("Oauth2TokenResponse.access_token Empty");
            return true;
        }



        public static async Task<Oauth2TokenResponse> HttpGetTokenAsync(string tokenEndpoint, Oauth2TokenRequest requestModel)
        {
            try
            {
                if (string.IsNullOrEmpty(requestModel.grant_type)) return null;
                var req = HTTPCLIENT.HttpClientTools.PrepareRequest(HttpMethod.Post, tokenEndpoint);
                HTTPCLIENT.HttpClientTools.SetContent(req, requestModel);
                HttpClient client = new HttpClient();
                var res = await client.SendAsync(req);
                HTTPCLIENT.HttpClientTools.Validate(res);
                var model = await HTTPCLIENT.HttpClientTools.ReadAsync<Oauth2TokenResponse>(res);
                return model;
            }
            catch (Exception rc)
            {
                throw new Exception("HttpGetToken "+rc.Message,rc);
            }
        }



    }
}
