using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.NET.HTTPCLIENT
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HttpRequestValueAttribute : Attribute
    {

        public HttpRequestValueAttribute(HttpRequestParameterType type) { this.Type = type; }
        public HttpRequestValueAttribute(string realName,HttpRequestParameterType type) { this.Type = type;  this.RealName = realName; }

        public string RealName { get; set; }

        public string StringFormat { get; set; }

        public HttpRequestParameterType Type { get; set; }




    }
}
