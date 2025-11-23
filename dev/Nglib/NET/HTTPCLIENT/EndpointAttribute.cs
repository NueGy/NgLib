using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Defines an HTTP endpoint for API client requests.
    /// Specify the HTTP method (GET, POST, PUT, DELETE, etc.) and the URL path.
    /// </summary>
    /// <example>
    /// <code>
    /// [Endpoint("GET", "/api/users/{userId}")]
    /// public class GetUserRequest {
    ///     [EndpointParameter("userId", HttpParameterTypeEnum.Path)]
    ///     public int UserId { get; set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EndpointAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the HTTP method (GET, POST, PUT, DELETE, PATCH, etc.)
        /// </summary>
        public HttpMethod Method { get; set; }
        
        /// <summary>
        /// Gets or sets the URL path or template.
        /// Can include path parameters like: "/api/users/{userId}"
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets optional documentation for this endpoint
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates a new endpoint attribute with string method name
        /// </summary>
        /// <param name="method">HTTP method as string: "GET", "POST", "PUT", "DELETE", "PATCH"</param>
        /// <param name="path">URL path or template (e.g., "/api/users/{id}")</param>
        public EndpointAttribute(string method, string path)
        {
            this.Method = HttpTools.ConvertToHttpMethod(method);
            this.Path = path;
        }
        
        /// <summary>
        /// Creates a new endpoint attribute with HttpMethod
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="path">URL path or template (e.g., "/api/users/{id}")</param>
        public EndpointAttribute(HttpMethod method, string path)
        {
            this.Method = method;
            this.Path = path;
        }
    }
}
