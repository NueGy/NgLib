using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Various HTTP utility methods for URL manipulation, query string building, and HTTP method conversion
    /// </summary>
    public static class HttpTools
    {

        /// <summary>
        /// Combines a base URL with a path
        /// </summary>
        public static string CombineRootUrl(string rootUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(rootUrl)) return path; // no root
            if (string.IsNullOrWhiteSpace(path)) return null; // no path
            if(path.StartsWith("http")) return path; // already a full URL
            path = path.Trim();
            if (rootUrl.EndsWith("/") && path.StartsWith("/")) return rootUrl + path.Substring(1);
            if (!rootUrl.EndsWith("/") && !path.StartsWith("/")) return rootUrl + "/" + path;
            return rootUrl + path;
        }



        /// <summary>
        /// Builds a querystring for URL from a parameter dictionary
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
        /// Appends querystring parameters to an existing URL from a parameter dictionary
        /// </summary>
        public static string AppendQueryToUrl(string originalUrl, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrWhiteSpace(originalUrl)) return string.Empty;
            if (parameters == null || parameters.Count == 0) return originalUrl;
            string query = GetQueryString(parameters);
            if (originalUrl.Contains("?")) return originalUrl + "&" + query;
            else return originalUrl + "?" + query;
        }




        /// <summary>
        /// Converts a string to HttpMethod type
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
