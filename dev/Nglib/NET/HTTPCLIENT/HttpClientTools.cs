using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nglib.APP.CODE;
using Nglib.FORMAT;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// HTTP client helper tools for making web API requests. Provides methods for creating requests, handling content serialization, reading responses, and validating responses.
    /// See: https://github.com/NueGy/NgLib/docs/wiki_components_httpclient
    /// </summary>
    public static class HttpClientTools
    {


        #region ---- CLIENT MANAGEMENT ----


        /// <summary>
        /// Creates an HTTP client with Bearer token authentication
        /// </summary>
        [Obsolete("Use HttpClientFactory instead")]
        public static HttpClient CreateNewClient(HttpClientConfigModel tokenConfig=null, string rootUrl=null)
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(rootUrl))
            {
                rootUrl = rootUrl.TrimEnd('/') + "/";
                client.BaseAddress = new Uri(rootUrl);
            }
            if (tokenConfig != null)
            {
                var tokenHandler = new HttpClientTokenHandler(tokenConfig);
                client = new HttpClient(tokenHandler);
            }
            return client;
        }

        #endregion



        #region ---- CONTENT AND REQUESTS MANAGEMENT ----

        /// <summary>
        /// Prepares an HTTP request message
        /// </summary>
        public static HttpRequestMessage PrepareRequest(HttpMethod method, string ServicePartUrl)
        {
            ServicePartUrl = ServicePartUrl.Trim();
            var req = new HttpRequestMessage(method, ServicePartUrl);
  
            //req.Headers.Accept.Clear();
            //req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return req;
        }
        
        
        /// <summary>
        /// Calls an API using a model decorated with the HttpEndpointAttribute
        /// </summary>
        /// <typeparam name="TResponseModel">Response model object</typeparam>
        /// <param name="client">HttpClient instance</param>
        /// <param name="requestModel">Must be an object with the HttpEndpointAttribute</param>
        /// <returns>Deserialized response model</returns>
        [Obsolete("BETA")]  
        public static async Task<TResponseModel> SendWithAttributeModelAsync<TResponseModel>(this HttpClient client,object requestModel)
        {
            var req = EndpointTools.CreateRequestFromModel(requestModel);
            var resp = await client.SendAsync(req);
            resp.Validate();

            var retour = await ReadAsync<TResponseModel>(resp);
            return retour;
        }
         
         

        /// <summary>
        /// Creates an API call with request and response models. Uses PrepareRequest+PrepareJsonContent+ReadWithModelAsync internally
        /// </summary>
        public static async Task<TResponseModel> SendWithModelAsync<TResponseModel>(this HttpClient client,
            HttpMethod method, string urlPart, object requestModel = null)
        {
            HttpRequestMessage req = null;
            req = new HttpRequestMessage(method, urlPart);
            req.SetContent(requestModel);
            var resp = await client.SendAsync(req);
            if (resp.StatusCode == HttpStatusCode.NotFound) return default; // return null si 404
            resp.Validate();

            var retour = await ReadAsync<TResponseModel>(resp);
            return retour;
        }
        

        

        /// <summary>
        /// Sets a Bearer token in the Authorization header of an HTTP request
        /// </summary>
        public static void SetBearerToken(this HttpRequestMessage httpRequestMessage, string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            if (httpRequestMessage == null) throw new ArgumentNullException(nameof(httpRequestMessage));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        /// <summary>
        /// Sets a Basic Authorization header in an HTTP request
        /// </summary>  
        public static void SetBasicAuth(this HttpRequestMessage httpRequestMessage, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;
            if (httpRequestMessage == null) throw new ArgumentNullException(nameof(httpRequestMessage));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
        }


        /// <summary>
        /// Adds a header to an HTTP request
        /// </summary>
        public static bool SetParameterHeader(HttpRequestMessage request, string key, string value)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(key)) return false;
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (request.Headers.Contains(key)) return false; // already exists
            request.Headers.Add(key.ToLower(), value);
            return true;
        }


        /// <summary>
        /// Creates JSON content by serializing the model. For POST requests, uses application/json content type; for GET, appends to querystring
        /// </summary>
        public static HttpContent SetContent(this HttpRequestMessage httpRequestMessage, object formToSerialize, bool ForcePostFormUrlContent=false)
        {
            if (formToSerialize == null) return null;
            try
            {
                HttpContent httpContent = null;
                // For GET or HEAD methods, no body allowed, so append content to URL
                List<HttpMethod> methodsWithNoBody = new List<HttpMethod> { HttpMethod.Get, HttpMethod.Head };
                if (httpRequestMessage!=null && methodsWithNoBody.Contains(httpRequestMessage.Method))
                {
                    var values = APP.CODE.PropertiesTools.GetValues(formToSerialize);
                    string fullurl = HttpTools.AppendQueryToUrl(httpRequestMessage.RequestUri.ToString(), values.ToDictionary(k => k.Key, v => v.Value?.ToString()));
                    httpRequestMessage.RequestUri = new Uri(fullurl);
                }
                else if (formToSerialize is string) // Already a primitive type
                {
                    httpContent= new StringContent(formToSerialize as string, Encoding.UTF8, "application/json");
                }
                else if(ForcePostFormUrlContent) // Explicit request for FormUrlEncodedContent
                {
                    var values = APP.CODE.PropertiesTools.GetValues(formToSerialize);
                    httpContent = new FormUrlEncodedContent(values.ToDictionary(d => d.Key, d => Convert.ToString(d.Value)));
                }
                else // Standard serialization
                {
                    var jsonSerializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
                    var bodyjsoncontent = JsonSerializer.Serialize(formToSerialize, formToSerialize.GetType(), jsonSerializerOptions);
                    httpContent= new StringContent(bodyjsoncontent, Encoding.UTF8, "application/json");
                }



                if (httpRequestMessage != null) httpRequestMessage.Content = httpContent;
                return httpContent;
            }
            catch (Exception ex)
            {
                throw new Exception("HttpClient.SetContent() " + ex.Message, ex);
            }
        }


        public static HttpContent PrepareJsonContent(object formToSerialize)
        {
            if (formToSerialize == null) return null;
            try
            {
                var jsonSerializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
                var bodyjsoncontent = JsonSerializer.Serialize(formToSerialize, formToSerialize.GetType(), jsonSerializerOptions);
                return new StringContent(bodyjsoncontent, Encoding.UTF8, "application/json");
            }
            catch (Exception ex)
            {
                throw new Exception("HttpClient.PrepareJsonContent() " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Adds dictionary values to a request. For GET, appends to URL; for POST, uses FormUrlEncodedContent
        /// </summary>
        /// <param name="values">Dictionary of values</param>
        public static FormUrlEncodedContent PrepareFormUrlContent(Dictionary<string, object> values)
        {
                    return new FormUrlEncodedContent(values.ToDictionary(d => d.Key, d => Convert.ToString(d.Value)));
        }


        #endregion



        #region ---- RESPONSE MANAGEMENT ----


        /// <summary>
        /// Validates the server response. Throws an exception if invalid. More precise than EnsureSuccessStatusCode
        /// </summary>
        public static void Validate(this HttpResponseMessage resp, string msgPrefix = null)
        {
            if (resp == null) throw new Exception($"{msgPrefix} HTTPResponseMessage null");
            if (resp.IsSuccessStatusCode) return;
            string bodymsg = ReadResponseTextSafe(resp);
            //TODO: Remove any HTML tags
            bodymsg = StringTools.Limit(bodymsg, 256);
            string resqEndUrl = null;
            if (resp.RequestMessage != null && resp.RequestMessage.RequestUri != null)
                resqEndUrl = $"{resp.RequestMessage.RequestUri} [{resp.RequestMessage.Method}]";

            var ex = new HttpRequestException(
                $"{msgPrefix} HTTP {resqEndUrl} ({(int)resp.StatusCode}) {resp.ReasonPhrase} : {bodymsg}");
            ex.Data["Response"] = resp;
            ex.Data["StatusCode"] = (int)resp.StatusCode;
            ex.Data["ResponseBody"] = bodymsg;
            throw ex;
        }


        /// <summary>
        /// Reads a response and deserializes JSON content
        /// </summary>
        public static async Task<TResponseModel> ReadAsync<TResponseModel>(this HttpResponseMessage resp)
        {
            try
            {
                if (resp.StatusCode == HttpStatusCode.NoContent) return default; // return null si vide
                var txtContent = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions();
                options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                options.PropertyNameCaseInsensitive = true;
                var retour = JsonSerializer.Deserialize<TResponseModel>(txtContent, options);
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception($"ReadWithModelAsync({typeof(TResponseModel).Name}) {ex.Message}", ex);
            }
        }

        public static TResponseModel Read<TResponseModel>(this HttpResponseMessage resp) => ReadAsync<TResponseModel>(resp).GetAwaiter().GetResult();


        /// <summary>
        /// Gets a response header by name
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <param name="headername">Header name</param>
        public static string GetResponseHeader(HttpResponseMessage response, string headername)
        {
            if (response == null) return null;
            if (response.Headers.TryGetValues(headername, out IEnumerable<string> values))
                return values.FirstOrDefault();
            return null;
        }


        /// <summary>
        /// Same as ReadAsStringAsync()
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static string ReadResponseTextSafe(HttpResponseMessage res)
        {
            try
            {
                var txtdata = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return txtdata;
            }
            catch (Exception)
            {
                return null;
            }
        }

 


        #endregion






 

    }
}