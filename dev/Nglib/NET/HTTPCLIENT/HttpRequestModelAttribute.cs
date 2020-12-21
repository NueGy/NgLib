using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Nglib.NET.HTTPCLIENT
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HttpRequestModelAttribute : Attribute
    {

        public HttpRequestModelAttribute(string method, string ServicePartUrl) {

            if (string.IsNullOrWhiteSpace(method) || method.Equals("Get", StringComparison.OrdinalIgnoreCase)) this.Method = HttpMethod.Get;
            else if (method.Equals("Post", StringComparison.OrdinalIgnoreCase)) this.Method = HttpMethod.Post;
            else if (method.Equals("Put", StringComparison.OrdinalIgnoreCase)) this.Method = HttpMethod.Put;
            else if (method.Equals("Delete", StringComparison.OrdinalIgnoreCase)) this.Method = HttpMethod.Delete;
            else if (method.Equals("Head", StringComparison.OrdinalIgnoreCase)) this.Method = HttpMethod.Head;
            else if (method.Equals("Options", StringComparison.OrdinalIgnoreCase)) this.Method = HttpMethod.Options;
            else if (method.Equals("Trace", StringComparison.OrdinalIgnoreCase)) this.Method = HttpMethod.Trace;
            else throw new Exception("method invalid");
            this.PartUrl = ServicePartUrl; 
        }


        public HttpMethod Method { get; set; }

        public string PartUrl { get; set; }  

    }
}
