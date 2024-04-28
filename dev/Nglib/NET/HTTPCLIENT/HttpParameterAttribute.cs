using System;

namespace Nglib.NET.HTTPCLIENT
{

    /// <summary>
    /// Type de paramètre HTTP
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HttpParameterAttribute : Attribute
    {
        public HttpParameterAttribute(HttpParameterTypeEnum type)
        {
            ParameterType = type;
        }

        public HttpParameterAttribute(string name, HttpParameterTypeEnum type)
        {
            ParameterType = type;
            Name = name;
        }

        public string Name { get; set; }

        public string StringFormat { get; set; }

        public HttpParameterTypeEnum ParameterType { get; set; }
    }
}