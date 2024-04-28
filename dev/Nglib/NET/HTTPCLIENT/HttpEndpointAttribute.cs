using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Permet de définir le comportement d'un Api. Url, méthode, ...
    /// </summary>
    public class HttpEndpointAttribute : Attribute
    {
        public HttpMethod Method { get; set; }
        public string Path { get; set; }

        public string Documentation { get; set; }

        public HttpEndpointAttribute(string method, string path)
        {
            this.Method = HttpClientTools.ConvertToHttpMethod(method);
            this.Path = path;
        }
        //public HttpEndpointAttribute(HttpMethod method, string path)
        //{
        //    this.Method = method;
        //    this.Path = path;
        //}
    }
}
