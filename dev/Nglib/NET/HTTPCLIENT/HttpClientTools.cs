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
        /// <summary>
        ///     Permet de composer une request avec une Url Propre
        /// </summary>
        /// <param name="method"></param>
        /// <param name="RootUrl"></param>
        /// <param name="ServicePartUrl"></param>
        /// <returns></returns>
        public static HttpRequestMessage PrepareRequest(HttpMethod method, string RootUrl, string ServicePartUrl)
        {
            if (RootUrl == null) RootUrl = "";
            RootUrl = RootUrl.TrimEnd('/') + "/";
            ServicePartUrl = ServicePartUrl.TrimStart('/');
            var finalUrl = ServicePartUrl.StartsWith("http") ? ServicePartUrl : RootUrl + ServicePartUrl;
            return PrepareRequest(method, finalUrl);
        }


        public static HttpRequestMessage PrepareRequest(HttpMethod method, string ServicePartUrl)
        {
            ServicePartUrl = ServicePartUrl.TrimStart('/');
            var req = new HttpRequestMessage(method, ServicePartUrl);
            //req.Headers.Accept.Clear();
            //req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return req;
        }


        public static HttpRequestMessage PrepareRequestFromModel(object model, HttpMethod method)
        {
            if (model == null) return null;
            var modelAttribute = AttributesTools.FindObjectAttribute<DataModelConfigAttribute>(model);
            if (modelAttribute == null) throw new Exception("Invalid Model");

            var modeltype = model.GetType();
            var QueryUrl = modelAttribute.ApiPartUrl;

            var properties =
                AttributesTools.FindPropertiesValueAttribute<HttpRequestValueAttribute>(model);
            properties.Where(p => p.Value.Item1.Type == HttpRequestParameterType.Path).ToList().ForEach(p =>
            {
                QueryUrl.Replace("{" + p.Value.Item1.RealName + "}",
                    TransformValue(p.Value.Item1, p.Value.Item2) as string);
            });

            properties.Where(p => p.Value.Item1.Type == HttpRequestParameterType.Query).ToList().ForEach(p =>
            {
                var val = TransformValue(p.Value.Item1, p.Value.Item2);
                if (val != null)
                {
                    if (!QueryUrl.Contains("?")) QueryUrl += "?";
                    else QueryUrl += "&";
                    QueryUrl += $"{p.Value.Item1.RealName}={val}";
                }
            });

            var req = new HttpRequestMessage(method, QueryUrl);

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


        public static HttpClient CreateNewClient(string rootUrl, string bearerToken) // !!! use factory standard
        {
            var client = new HttpClient();
            rootUrl = rootUrl.TrimEnd('/') + "/";
            client.BaseAddress = new Uri(rootUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return client;
        }


        /// <summary>
        ///     Permet d'ajouter un model (utilisation des attributs HttpRequestValueAttribute)
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static void AddValueModelContent<Tmodel>(this HttpRequestMessage request, Tmodel model)
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


        public static async Task<TResponseModel> ReadWithModelAsync<TResponseModel>(this HttpResponseMessage resp)
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


        public static async Task<TResponseModel> ExecuteWithModelAsync<TResponseModel>(this HttpClient client,
            object requestModel, HttpMethod method)
        {
            var req = PrepareRequestFromModel(requestModel, method);
            var resp = await client.SendAsync(req);
            resp.Validate();

            var retour = await ReadWithModelAsync<TResponseModel>(resp);
            return retour;
        }

        public static async Task<TResponseModel> ExecuteWithModelAsync<TResponseModel>(this HttpClient client,
            HttpMethod method, string urlPart, Dictionary<string, object> formdata = null, object requestModel = null)
        {
            HttpRequestMessage req = null;
            req = new HttpRequestMessage(method, urlPart);
            if (requestModel != null) req.Content = PrepareJsonContent(requestModel);
            var resp = await client.SendAsync(req);
            if (resp.StatusCode == HttpStatusCode.NotFound) return default; // return null si 404
            resp.Validate();

            var retour = await ReadWithModelAsync<TResponseModel>(resp);
            return retour;
        }


        public static HttpContent PrepareJsonContent(object model)
        {
            if (model == null) return null;
            if (model is string) return new StringContent(model as string, Encoding.UTF8, "application/json");
            var jsonSerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true };
            var bodyjsoncontent = JsonSerializer.Serialize(model, model.GetType(), jsonSerializerOptions);
            return new StringContent(bodyjsoncontent, Encoding.UTF8, "application/json");
        }


        public static void SetDictionaryContent(this HttpRequestMessage httpRequestMessage,
            Dictionary<string, object> values)
        {
            if (httpRequestMessage.Method == HttpMethod.Get)
            {
                // !!! dev 
            }
            else
            {
                httpRequestMessage.Content =
                    new FormUrlEncodedContent(values.ToDictionary(d => d.Key, d => Convert.ToString(d.Value)));
            }
        }


        public static void Validate(this HttpResponseMessage resp, string msgPrefix = null)
        {
            if (resp == null) throw new Exception($"{msgPrefix} HTTPResponseMessage null");
            if (resp.IsSuccessStatusCode) return;
            string bodymsg = null;
            try
            {
                bodymsg = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                //safe
            }

            bodymsg = StringTools.Limit(bodymsg, 256);

            string resqEndUrl = null;
            if (resp.RequestMessage != null && resp.RequestMessage.RequestUri != null)
                resqEndUrl = $"{resp.RequestMessage.RequestUri} [{resp.RequestMessage.Method}]";

            throw new Exception(
                $"{msgPrefix} HTTP {resqEndUrl} ({(int)resp.StatusCode}) {resp.ReasonPhrase} : {bodymsg}");
        }


        public static void SetBearerToken(this HttpRequestMessage httpRequestMessage, string token)
        {
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
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


        public static string PrepareUrlQueryContent(this object objModel)
        {
            if (objModel == null) return "";
            try
            {
                var keyvalues = new List<string>();
                foreach (var itemprp in objModel.GetType().GetProperties())
                {
                    //!!! filter
                    var obj = itemprp.GetValue(objModel);
                    if (obj == null) continue;
                    if (obj is int && (int)obj == 0) continue;
                    if (!(obj is string)) obj = obj.ToString();
                    if (string.IsNullOrWhiteSpace((string)obj)) continue;
                    keyvalues.Add($"{itemprp.Name}={obj}");
                }

                if (keyvalues.Count == 0) return "";
                return string.Join("&", keyvalues);
            }
            catch (Exception ex)
            {
                throw new Exception("PrepareUrlQueryContent " + ex.Message, ex);
            }
        }

        public static HttpMethod ConvertToHttpMethod(string method)
        {
            if (string.IsNullOrWhiteSpace(method) || method.Equals("Get", StringComparison.OrdinalIgnoreCase))
                return HttpMethod.Get;
            if (method.Equals("Post", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Post;
            if (method.Equals("Put", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Put;
            if (method.Equals("Delete", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Delete;
            if (method.Equals("Head", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Head;
            if (method.Equals("Options", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Options;
            if (method.Equals("Trace", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Trace;
            throw new Exception("method invalid");
        }
    }
}