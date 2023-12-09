using System;

namespace Nglib.NET.HTTPCLIENT
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HttpRequestValueAttribute : Attribute
    {
        public HttpRequestValueAttribute(HttpRequestParameterType type)
        {
            Type = type;
        }

        public HttpRequestValueAttribute(string realName, HttpRequestParameterType type)
        {
            Type = type;
            RealName = realName;
        }

        public string RealName { get; set; }

        public string StringFormat { get; set; }

        public HttpRequestParameterType Type { get; set; }
    }
}