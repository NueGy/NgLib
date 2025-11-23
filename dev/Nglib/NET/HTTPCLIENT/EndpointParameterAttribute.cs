using System;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Defines how a property should be mapped to an HTTP request parameter.
    /// Supports Path, Query, Header, and FormData parameters.
    /// </summary>
    /// <example>
    /// <code>
    /// [Endpoint("GET", "/api/users/{userId}")]
    /// public class GetUserRequest {
    ///     [EndpointParameter(HttpParameterTypeEnum.Path)]
    ///     public int UserId { get; set; }
    ///     
    ///     [EndpointParameter("filter", HttpParameterTypeEnum.Query)]
    ///     public string Filter { get; set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EndpointParameterAttribute : Attribute
    {
        /// <summary>
        /// Creates a parameter attribute with auto-detected name from property
        /// </summary>
        /// <param name="type">The parameter type (Path, Query, Header, FormData)</param>
        public EndpointParameterAttribute(HttpParameterTypeEnum type)
        {
            ParameterType = type;
        }

        /// <summary>
        /// Creates a parameter attribute with explicit name
        /// </summary>
        /// <param name="name">The parameter name in the request (defaults to property name if not specified)</param>
        /// <param name="type">The parameter type (Path, Query, Header, FormData)</param>
        public EndpointParameterAttribute(string name, HttpParameterTypeEnum type)
        {
            ParameterType = type;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the parameter name.
        /// If not specified, the property name will be used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the format string for value serialization.
        /// Useful for dates, numbers, etc. (e.g., "yyyy-MM-dd" for DateTime)
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// Gets or sets the parameter type (Path, Query, Header, FormData)
        /// </summary>
        public HttpParameterTypeEnum ParameterType { get; set; }
    }
}