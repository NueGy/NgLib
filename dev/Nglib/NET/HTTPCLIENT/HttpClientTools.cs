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
using Nglib.DATA.DATAMODEL;
using Nglib.FORMAT;

namespace Nglib.NET.HTTPCLIENT
{
    public static class HttpClientTools
    {

        [Obsolete("todelete")]
        public static HttpRequestMessage PrepareRequest(HttpMethod method, string RootUrl, string ServicePartUrl)
        {
            var finalUrl = HttpTools.CombineRootUrl(RootUrl, ServicePartUrl);
            return PrepareRequest(method, finalUrl);
        }



        /// <summary>
        /// Permet de composer une requête HTTP
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
        ///     Génération d'un client HTTP avec un token Bearer
        /// </summary>
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

 

        [Obsolete("BETA")]  
        public static async Task<TResponseModel> SendWithAttributeModelAsync<TResponseModel>(this HttpClient client,object requestModel)
        {
            var req = HttpAttributesTools.CreateRequestFromModel(requestModel);
            var resp = await client.SendAsync(req);
            resp.Validate();

            var retour = await ReadAsync<TResponseModel>(resp);
            return retour;
        }

        /// <summary>
        /// Création d'un appel avec un model. use  PrepareRequest+PrepareJsonContent+ReadWithModelAsync
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
        /// Permet de définir un token Bearer dans une requête
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <param name="token"></param>
        public static void SetBearerToken(this HttpRequestMessage httpRequestMessage, string token)
        {
            if(string.IsNullOrEmpty(token)) return;
            if (httpRequestMessage == null) throw new ArgumentNullException(nameof(httpRequestMessage));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        public static void SetBasicAuth(this HttpRequestMessage httpRequestMessage, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;
            if (httpRequestMessage == null) throw new ArgumentNullException(nameof(httpRequestMessage));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
        }


        /// <summary>
        /// Ajouter un header dans une requête
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
        /// Pemet de créer un contenu JSON. Sérialisation du model, Si post Alors application/json si Get alors querystring
        /// </summary>
        public static HttpContent SetContent(this HttpRequestMessage httpRequestMessage, object formToSerialize, bool ForcePostFormUrlContent=false)
        {
            if (formToSerialize == null) return null;
            try
            {
                HttpContent httpContent = null;
                // Si Get ou Head, pas de body donc on passe le content dans l'url
                List<HttpMethod> methodsWithNoBody = new List<HttpMethod> { HttpMethod.Get, HttpMethod.Head };
                if (httpRequestMessage!=null && methodsWithNoBody.Contains(httpRequestMessage.Method))
                {
                    var values = APP.CODE.PropertiesTools.GetValues(formToSerialize);
                    string fullurl = HttpTools.AppendQueryToUrl(httpRequestMessage.RequestUri.ToString(), values.ToDictionary(k => k.Key, v => v.Value?.ToString()));
                    httpRequestMessage.RequestUri = new Uri(fullurl);
                }
                else if (formToSerialize is string) // C'est déja un type primitif
                {
                    httpContent= new StringContent(formToSerialize as string, Encoding.UTF8, "application/json");
                }
                else if(ForcePostFormUrlContent) // demande explicite de FormUrlEncodedContent
                {
                    var values = APP.CODE.PropertiesTools.GetValues(formToSerialize);
                    httpContent = new FormUrlEncodedContent(values.ToDictionary(d => d.Key, d => Convert.ToString(d.Value)));
                }
                else // Serialisation standard
                {
                    var jsonSerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true };
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
                var jsonSerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true };
                var bodyjsoncontent = JsonSerializer.Serialize(formToSerialize, formToSerialize.GetType(), jsonSerializerOptions);
                return new StringContent(bodyjsoncontent, Encoding.UTF8, "application/json");
            }
            catch (Exception ex)
            {
                throw new Exception("HttpClient.PrepareJsonContent() " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Permet d'ajouter les valeurs d'un dictionnaire dans une requête
        /// Si get, ajout dans l'url si post FormUrlEncodedContent
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <param name="values"></param>
        public static FormUrlEncodedContent PrepareFormUrlContent(Dictionary<string, object> values)
        {
                    return new FormUrlEncodedContent(values.ToDictionary(d => d.Key, d => Convert.ToString(d.Value)));
        }

        /// <summary>
        /// Permet de valider le retour du serveur. Génère une exception si invalide
        /// Plus précis que EnsureSuccessStatusCode
        /// </summary>
        public static void Validate(this HttpResponseMessage resp, string msgPrefix = null)
        {
            if (resp == null) throw new Exception($"{msgPrefix} HTTPResponseMessage null");
            if (resp.IsSuccessStatusCode) return;
            string bodymsg = ReadResponseTextSafe(resp);
            //todo!!! : Supprimer les eventuels balises html
            bodymsg = StringTools.Limit(bodymsg, 256);
            string resqEndUrl = null;
            if (resp.RequestMessage != null && resp.RequestMessage.RequestUri != null)
                resqEndUrl = $"{resp.RequestMessage.RequestUri} [{resp.RequestMessage.Method}]";
 
            throw new Exception(
                $"{msgPrefix} HTTP {resqEndUrl} ({(int)resp.StatusCode}) {resp.ReasonPhrase} : {bodymsg}");
        }


        /// <summary>
        /// Lecture d'une réponse, Désérialisation JSON
        /// </summary>
        public static async Task<TResponseModel> ReadAsync<TResponseModel>(this HttpResponseMessage resp)
        {
            try
            {
                if (resp.StatusCode == HttpStatusCode.NoContent) return default; // return null si vide
                var txtContent = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions();
                options.IgnoreNullValues = true;
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
        /// Obtenir un header de la réponse
        /// </summary>
        /// <param name="response"></param>
        public static string GetResponseHeader(HttpResponseMessage response, string headername)
        {
            if (response == null) return null;
            if (response.Headers.TryGetValues(headername, out IEnumerable<string> values))
                return values.FirstOrDefault();
            return null;
        }


        /// <summary>
        /// Identique à ReadAsStringAsync()
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

 
        /// <summary>
        /// Obtenir le type HttpMethod depuis un string
        /// </summary>
        public static HttpMethod ConvertToHttpMethod(string method)
        {
            if (string.IsNullOrWhiteSpace(method) || method.Equals("Get", StringComparison.OrdinalIgnoreCase))
                return HttpMethod.Get;
            else if (method.Equals("Post", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Post;
            else if (method.Equals("Put", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Put;
            else if (method.Equals("Delete", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Delete;
            else if (method.Equals("Head", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Head;
            else if (method.Equals("Options", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Options;
            else if (method.Equals("Trace", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Trace;
            else if (method.Equals("Patch", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Patch;
            else throw new Exception("method invalid");
             
        }
    }
}