using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    public static class HttpClientTools
    {

        [Obsolete]
        public static HttpRequestMessage PrepareRequest(HttpMethod method, string RootUrl, string ServicePartUrl)
        {
            RootUrl = RootUrl.TrimEnd('/') + "/";
            ServicePartUrl = ServicePartUrl.TrimStart('/');
            string finalUrl = (ServicePartUrl.StartsWith("http")) ? ServicePartUrl : RootUrl + ServicePartUrl;
            HttpRequestMessage req = new HttpRequestMessage(method, finalUrl);
            //req.Headers.Accept.Clear();
            //req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return req;
        }


        public static HttpRequestMessage PrepareRequest(HttpMethod method, string ServicePartUrl)
        {
            ServicePartUrl = ServicePartUrl.TrimStart('/');
            HttpRequestMessage req = new HttpRequestMessage(method, ServicePartUrl);
            //req.Headers.Accept.Clear();
            //req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return req;
        }


        public static HttpRequestMessage PrepareRequestFromModel(object model, HttpMethod method)
        {
            if (model == null) return null;
            DATA.BASICS.ModelConfigAttribute modelAttribute = Nglib.APP.CODE.AttributesTools.FindObjectAttribute<DATA.BASICS.ModelConfigAttribute>(model);
            if (modelAttribute == null) throw new Exception("Invalid Model");

            Type modeltype = model.GetType();
            string QueryUrl = modelAttribute.ApiPartUrl;

            Dictionary<string, Tuple<HttpRequestValueAttribute, object>> properties =
                        Nglib.APP.CODE.AttributesTools.FindPropertiesValueAttribute<HTTPCLIENT.HttpRequestValueAttribute>(model);
            properties.Where(p => p.Value.Item1.Type == HttpRequestParameterType.Path).ToList().ForEach(p => { QueryUrl.Replace("{" + p.Value.Item1.RealName + "}", TransformValue(p.Value.Item1, p.Value.Item2) as string); });

            properties.Where(p => p.Value.Item1.Type == HttpRequestParameterType.Query).ToList().ForEach(p => {
                object val = TransformValue(p.Value.Item1, p.Value.Item2);
                if (val != null)
                {
                    if (!QueryUrl.Contains("?")) QueryUrl += "?"; else QueryUrl += "&";
                    QueryUrl += $"{p.Value.Item1.RealName}={val}";
                }
            });

            HttpRequestMessage req = new HttpRequestMessage(method, QueryUrl);

            return req;
        }

        private static object TransformValue(HttpRequestValueAttribute attr, object val)
        {
            if (val == null || val == DBNull.Value) return null;
            if (val is DateTime && !string.IsNullOrEmpty(attr.StringFormat))
                val = ((DateTime)val).ToString(attr.StringFormat);

            if (val is List<string> && !string.IsNullOrEmpty(attr.StringFormat))
                val = string.Join(attr.StringFormat, (List<string>)val);

            return val;
        }


        public static async Task<TResponseModel> ReadWithModelAsync<TResponseModel>(this HttpResponseMessage resp)
        {
            try
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default(TResponseModel); // return null si vide
                string txtContent = await resp.Content.ReadAsStringAsync();
                System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions();
                options.IgnoreNullValues = true;
                options.PropertyNameCaseInsensitive = true;
                TResponseModel retour = System.Text.Json.JsonSerializer.Deserialize<TResponseModel>(txtContent, options);
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception($"ReadWithModelAsync({typeof(TResponseModel).Name}) {ex.Message}", ex);
            }
        }


        public static async Task<TResponseModel> ExecuteWithModelAsync<TResponseModel>(this HttpClient client, object requestModel, HttpMethod method)
        {
            try
            {
                HttpRequestMessage req = PrepareRequestFromModel(requestModel, method);
                HttpResponseMessage resp = await client.SendAsync(req);
                resp.Validate();

                TResponseModel retour = await ReadWithModelAsync<TResponseModel>(resp);
                return retour;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<TResponseModel> ExecuteWithModelAsync<TResponseModel>(this HttpClient client, HttpMethod method, string urlPart, Dictionary<string, object> formdata = null, object requestModel = null)
        {
            try
            {
                HttpRequestMessage req = null;
                req = new HttpRequestMessage(method, urlPart);
                if (requestModel != null) req.Content = PrepareJsonContent(requestModel);
                HttpResponseMessage resp = await client.SendAsync(req);
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return default(TResponseModel); // return null si 404
                resp.Validate();

                TResponseModel retour = await ReadWithModelAsync<TResponseModel>(resp);
                return retour;
            }
            catch (Exception)
            {
                throw;
            }
        }




        public static HttpClient CreateNewClient(string rootUrl, string bearerToken) // !!! use factory standard
        {
            HttpClient client = new HttpClient();
            rootUrl = rootUrl.TrimEnd('/') + "/";
            client.BaseAddress = new Uri(rootUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            return client;
        }



        /// <summary>
        /// Permet d'ajouter un model (utilisation des attributs HttpRequestValueAttribute)
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static void AddValueModelContent<Tmodel>(this HttpRequestMessage request, Tmodel model)
        {
            try
            {



                //// Path properties
                //if (properties.Any(p => p.Value.Type == HttpRequestParameterType.Path || p.Value.Type == HttpRequestParameterType.Query))
                //{
                //    string newpath = request.RequestUri.ToString();
                //    properties.Where(p => p.Value.Type == HttpRequestParameterType.Path).ToList().ForEach(p => { newpath.Replace("{"+p.Value.RealName+"}",) });

                //    Uri newUri = new Uri(newpath);
                //    request.RequestUri = newUri;
                //}


                // Query Properties


            }
            catch (Exception)
            {

                throw;
            }
        }














        [Obsolete("use PrepareRequestFromModel")]
        public static async Task<HttpRequestMessage> PrepareRequestWithContextAsync(IHttpClientContext httpClientContext, HttpMethod method, string ServicePartUrl)
        {
            try
            {
                HttpRequestMessage httpRequest = null;
                // préparation REQ
                httpRequest = PrepareRequest(method, httpClientContext.RootUrl, ServicePartUrl);

                //gestion du token
                string tokenGenerated = await GenerateTokenAsync(httpClientContext);
                if (!string.IsNullOrEmpty(tokenGenerated))
                {
                    if (httpClientContext.TokenMode == HttpClientTokenModeEnum.BASIC)
                        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("basic", tokenGenerated);
                    else
                        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", tokenGenerated);
                }



                return httpRequest;
            }
            catch (Exception ex)
            {
                throw new Exception("PrepareRequestWithContext " + ex.Message, ex);
            }
        }

        [Obsolete("use PrepareRequestFromModel")]
        public static HttpRequestMessage PrepareRequestWithContext(IHttpClientContext httpClientContext, HttpMethod method, string ServicePartUrl) 
        { return PrepareRequestWithContextAsync(httpClientContext, method, ServicePartUrl).GetAwaiter().GetResult(); }


        public static async Task<string> GenerateTokenAsync(IHttpClientContext httpClientContext)
        {

            if (httpClientContext.TokenMode == HttpClientTokenModeEnum.NO) return null;
            if (httpClientContext.TokenMode == HttpClientTokenModeEnum.BASIC)
            {
                return $"basic {Nglib.FORMAT.CryptHashTools.ToBase64(httpClientContext.ClientId)}:{Nglib.FORMAT.CryptHashTools.ToBase64(httpClientContext.ClientSecret)}";
            }
            else if (httpClientContext.TokenMode == HttpClientTokenModeEnum.BEARERJWTHS256) //aes
            {
                //if(!string.IsNullOrWhiteSpace)
                Dictionary<string, object> ins = httpClientContext.JwtPayload.Clone();
                ins.AddOrReplace("client_id", httpClientContext.ClientId);
                return Nglib.SECURITY.CRYPTO.TokenJwtTools.EncodeBasicJWT(httpClientContext.ClientSecret, null, null, null, true, 3, ins);
            }
            else if (httpClientContext.TokenMode == HttpClientTokenModeEnum.BEARERJWTRS256) // rsa
            {
            }
            else if (httpClientContext.TokenMode == HttpClientTokenModeEnum.OAUTH2BEARER)
            {
                if (!httpClientContext.Oauth2TokenResponse.IsTokenTimeOut()) return httpClientContext.Oauth2TokenResponse.Token;
                Dictionary<string, object> ins = httpClientContext.JwtPayload.Clone();
                ins.AddOrReplace("client_id", httpClientContext.ClientId);
                string exhToken = Nglib.SECURITY.CRYPTO.TokenJwtTools.EncodeBasicJWT(httpClientContext.ClientSecret, null, null, null, true, 3, ins);

                HttpClient httpClientForToken = new HttpClient();
                Dictionary<string, object> insfortoken = new Dictionary<string, object>();
                ins.Add("client_id", httpClientContext.ClientId);
                ins.Add("assertion", exhToken);
                ins.Add("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer");
                HttpRequestMessage requestMessageForToken = new HttpRequestMessage(HttpMethod.Post, httpClientContext.TokenUrl);
                requestMessageForToken.SetBearerToken(exhToken);// !!! utile ???
                requestMessageForToken.SetDictionaryContent(ins);
                HttpResponseMessage responseMessageForToken = await httpClientForToken.SendAsync(requestMessageForToken);
                responseMessageForToken.Validate();
                string jsonresponse = await responseMessageForToken.Content.ReadAsStringAsync();
                var oauth2response = System.Text.Json.JsonSerializer.Deserialize<HTTPCLIENT.Oauth2TokenResponse>(jsonresponse);
                if (oauth2response == null) throw new Exception("oauth2response empty");
                oauth2response.TokenCreate = DateTime.Now;
                httpClientContext.Oauth2TokenResponse = oauth2response;
                return oauth2response.Token;
            }
            else if (httpClientContext.TokenMode == HttpClientTokenModeEnum.OAUTH2CLIENTCREDENTIAL)
            {
                if (!httpClientContext.Oauth2TokenResponse.IsTokenTimeOut()) return httpClientContext.Oauth2TokenResponse.Token;
                HttpClient httpClientForToken = new HttpClient();
                Dictionary<string, object> ins = new Dictionary<string, object>();
                ins.Add("client_id", httpClientContext.ClientId);
                ins.Add("client_secret", httpClientContext.ClientSecret);
                ins.Add("grant_type", "client_credentials");
                HttpRequestMessage requestMessageForToken = new HttpRequestMessage(HttpMethod.Post, httpClientContext.TokenUrl);
                requestMessageForToken.SetDictionaryContent(ins);
                HttpResponseMessage responseMessageForToken = await httpClientForToken.SendAsync(requestMessageForToken);
                responseMessageForToken.Validate();
                string jsonresponse = await responseMessageForToken.Content.ReadAsStringAsync();
                var oauth2response = System.Text.Json.JsonSerializer.Deserialize<HTTPCLIENT.Oauth2TokenResponse>(jsonresponse);
                if (oauth2response == null) throw new Exception("oauth2response empty");
                oauth2response.TokenCreate = DateTime.Now;
                httpClientContext.Oauth2TokenResponse = oauth2response;
                return oauth2response.Token;
            }


            return null;
        }
     
        public static bool IsTokenTimeOut(this Oauth2TokenResponse oauth2TokenResponse)
        {
            if (oauth2TokenResponse == null) return true;
            if (string.IsNullOrEmpty(oauth2TokenResponse.Token)) return true;
            if (oauth2TokenResponse.TokenCreate.AddSeconds(oauth2TokenResponse.TokenExpire) < DateTime.Now) return true;
            return false;
        }


        public static void SetDictionaryContent(this HttpRequestMessage httpRequestMessage, Dictionary<string,object> values)
        {
            if(httpRequestMessage.Method == HttpMethod.Get)
            {
                // !!! dev 
            }
            else
                httpRequestMessage.Content = new FormUrlEncodedContent(values.ToDictionary(d=>d.Key,d=>Convert.ToString(d.Value)));
        }


        public static void Validate(this HttpResponseMessage resp)
        {
            if (resp == null) throw new Exception("HTTP ResponseMessage null");
            if (resp.IsSuccessStatusCode) return;
            string bodymsg = null;
            try
            {
                bodymsg = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ValidateHttpResponseMessage" + ex.Message);
            }
            bodymsg = Nglib.FORMAT.StringTools.Limit(bodymsg, 256, true);

            string resqEndUrl = null;
            if (resp.RequestMessage != null && resp.RequestMessage.RequestUri != null)
                resqEndUrl = $"{resp.RequestMessage.RequestUri.ToString()} [{resp.RequestMessage.Method}]";

            throw new Exception($"HTTP {resqEndUrl} ({((int)resp.StatusCode)}) {resp.ReasonPhrase} : {bodymsg}");
        }



        public static void SetBearerToken(this HttpRequestMessage httpRequestMessage, string token)
        {
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
        }


 

        public static string ReadResponseText(HttpResponseMessage res)
        {
            var txtdata = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return txtdata;
        }
        public static string ReadResponseByte(HttpResponseMessage res)
        {
            var txtdata = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return txtdata;
        }

 

        public static System.Net.Http.HttpContent PrepareJsonContent(object model)
        {
            if (model == null) return null;
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { IgnoreNullValues = true };
            string bodyjsoncontent = JsonSerializer.Serialize(model, model.GetType(), jsonSerializerOptions);
            return new System.Net.Http.StringContent(bodyjsoncontent, Encoding.UTF8, "application/json");
        }



        public static HttpMethod ConvertToHttpMethod(string method)
        {
            if (string.IsNullOrWhiteSpace(method) || method.Equals("Get", StringComparison.OrdinalIgnoreCase)) return  HttpMethod.Get;
            else if (method.Equals("Post", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Post;
            else if (method.Equals("Put", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Put;
            else if (method.Equals("Delete", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Delete;
            else if (method.Equals("Head", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Head;
            else if (method.Equals("Options", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Options;
            else if (method.Equals("Trace", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Trace;
            else throw new Exception("method invalid");
        }



    }
}
