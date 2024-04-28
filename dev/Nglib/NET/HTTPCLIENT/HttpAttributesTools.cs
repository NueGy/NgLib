using Nglib.APP.CODE;
using Nglib.DATA.BASICS;
using Nglib.DATA.DATAMODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Utilisation avec HttpEndpointAttribute
    /// </summary>
    public static class HttpAttributesTools
    {



        /// <summary>
        /// Permet de créer une requête http à partir d'un model décoré avec un HttpEndpointAttribute
        /// </summary>
        /// <param name="model">Doit contenir l'attribut HttpEndpointAttribute</param>
        /// <param name="rootUrl"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static HttpRequestMessage CreateRequestFromModel(object model, string rootUrl = null)
        {
            if (model == null) return null;
            var modelAttribute = AttributesTools.GetAttribute<HttpEndpointAttribute>(model);
            if (modelAttribute == null) throw new Exception("Invalid Model, HttpEndpointAttribute is required");

            string QueryUrl = modelAttribute.Path;
            if (!string.IsNullOrEmpty(rootUrl)) QueryUrl = HttpTools.CombineRootUrl(rootUrl, QueryUrl);

            var req = new HttpRequestMessage(modelAttribute.Method, QueryUrl);

            SetRequestParameters(model, req);

            return req;
        }



        /// <summary>
        /// définir les paramètres de la requête http à partir d'un model décoré avec des HttpParameterAttribute
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="model"></param>
        /// <param name="http"></param>
        /// <exception cref="APP.DIAG.CascadeException"></exception>
        public static void SetRequestParameters(object model, HttpRequestMessage http) 
        {
            try
            {
                if (model == null) return;
                if (http == null) throw new ArgumentNullException(nameof(http));
                var parameters = AttributesTools.GetValuesWithAttribute<HttpParameterAttribute>(model);
                Dictionary<string, string> Queryparameters = new Dictionary<string, string>();
                Dictionary<string, string> FormDataparameters = new Dictionary<string, string>();

                foreach (var kvp in parameters)
                {
                    if (kvp.Key.ParameterType == HttpParameterTypeEnum.Path)
                    {
                        http.RequestUri = new Uri(http.RequestUri.ToString().Replace("{" + kvp.Key.Name + "}", kvp.Value?.ToString(), StringComparison.OrdinalIgnoreCase));
                    }
                    else if (kvp.Key.ParameterType == HttpParameterTypeEnum.Query)
                    {
                        if (kvp.Value == null || string.IsNullOrEmpty(kvp.Value.ToString())) continue; //si vide ne pas ajouter
                        Queryparameters.Add(kvp.Key.Name, kvp.Value?.ToString());
                    }
                    else if (kvp.Key.ParameterType == HttpParameterTypeEnum.Header)
                    {
                        if (kvp.Value == null || string.IsNullOrEmpty(kvp.Value.ToString())) continue; //si vide ne pas ajouter
                        http.Headers.TryAddWithoutValidation(kvp.Key.Name, kvp.Value?.ToString());
                    }
                    else if (kvp.Key.ParameterType == HttpParameterTypeEnum.FormData)
                    {
                        if (kvp.Value == null || string.IsNullOrEmpty(kvp.Value.ToString())) continue; //si vide ne pas ajouter
                        FormDataparameters.Add(kvp.Key.Name, kvp.Value?.ToString());
                    }
                }

                if (Queryparameters.Count > 0)
                {
                    var query = HttpTools.GetQueryString(Queryparameters);
                    string firstSeparator = http.RequestUri.ToString().Contains("?") ? "?" : "&";
                    http.RequestUri = new Uri(http.RequestUri.ToString() + firstSeparator + query);
                }
                if (FormDataparameters.Count > 0)
                {
                    http.Content = new FormUrlEncodedContent(FormDataparameters);
                }

            }
            catch (Exception ex)
            {
                throw new APP.DIAG.CascadeException("SetRequestParameters", ex);
            }
        }



        public static async Task ParseResponseParametersAsync<Tmodel>(Tmodel model, HttpResponseMessage http)
        {
            try
            {
                if (model == null) return;
                if (http == null) throw new ArgumentNullException(nameof(http));
                var parameters = AttributesTools.GetPropertiesWithAttribute<HttpParameterAttribute>(model?.GetType());

                Dictionary<string, object> Values = new Dictionary<string, object>();

                foreach (var kvp in parameters)
                {
                    if (kvp.Value.ParameterType == HttpParameterTypeEnum.Header)
                    {
                        var val = http.Headers.FirstOrDefault(x => x.Key == kvp.Key.Name).Value?.FirstOrDefault();
                        kvp.Key.SetValue(model, val);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new APP.DIAG.CascadeException("ParseResponseParameters", ex);
            }
        }   







    }
}
