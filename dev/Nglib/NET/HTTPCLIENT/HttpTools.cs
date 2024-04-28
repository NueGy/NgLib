using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Divers 
    /// </summary>
    public static class HttpTools
    {

        /// <summary>
        /// permet de combiner une url de base avec un path
        /// </summary>
        public static string CombineRootUrl(string rootUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(rootUrl)) return path; // pas de root
            if (string.IsNullOrWhiteSpace(path)) return null; // pas de path
            if(path.StartsWith("http")) return path; // déja une url complète
            path = path.Trim();
            if (rootUrl.EndsWith("/") && path.StartsWith("/")) return rootUrl + path.Substring(1);
            if (!rootUrl.EndsWith("/") && !path.StartsWith("/")) return rootUrl + "/" + path;
            return rootUrl + path;
        }



        /// <summary>
        /// Permet de composer une querystring pour Url à partir d'un dictionnaire de paramètres
        /// </summary>
        public static string GetQueryString(Dictionary<string, string> parameters)
        {
            if (parameters == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in parameters)
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append(kvp.Key + "=" + Uri.EscapeDataString(kvp.Value));
            }
            return sb.ToString();
        }


        /// <summary>
        /// Permet de composer une querystring pour Url à partir d'un dictionnaire de paramètres
        /// </summary>
        public static string AppendQueryToUrl(string originalUrl, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrWhiteSpace(originalUrl)) return string.Empty;
            if (parameters == null || parameters.Count == 0) return originalUrl;
            string query = GetQueryString(parameters);
            if (originalUrl.Contains("?")) return originalUrl + "&" + query;
            else return originalUrl + "?" + query;
        }



    }
}
