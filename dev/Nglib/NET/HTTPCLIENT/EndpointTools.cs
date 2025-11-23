using Nglib.APP.CODE;
using Nglib.DATA.BASICS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Provides tools to build HTTP requests from models decorated with Endpoint attributes.
    /// Use [Endpoint] and [EndpointParameter] attributes on your request classes,
    /// then call CreateRequestFromModel to generate the HttpRequestMessage.
    /// </summary>
    /// <example>
    /// <code>
    /// [Endpoint("GET", "/api/users/{userId}")]
    /// public class GetUserRequest 
    /// {
    ///     [Required]
    ///     [EndpointParameter(HttpParameterTypeEnum.Path)]
    ///     public int UserId { get; set; }
    ///     
    ///     [EndpointParameter("filter", HttpParameterTypeEnum.Query)]
    ///     public string Filter { get; set; }
    /// }
    /// 
    /// var request = new GetUserRequest { UserId = 123 };
    /// var httpRequest = EndpointTools.CreateRequestFromModel(request, "https://api.example.com");
    /// var response = await client.SendAsync(httpRequest);
    /// </code>
    /// </example>
    public static class EndpointTools
    {
        /// <summary>
        /// Creates an HttpRequestMessage from a model decorated with [Endpoint] attribute.
        /// The model properties decorated with [EndpointParameter] are mapped to the request.
        /// </summary>
        /// <param name="model">The request model decorated with [Endpoint] attribute</param>
        /// <param name="rootUrl">Optional root URL to prepend to the path</param>
        /// <returns>A configured HttpRequestMessage ready to be sent</returns>
        /// <exception cref="ArgumentNullException">Thrown when model is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when model is missing [Endpoint] attribute</exception>
        public static HttpRequestMessage CreateRequestFromModel(object model, string rootUrl = null)
        {
            if (model == null) return null;
            var modelAttribute = AttributesTools.GetAttribute<EndpointAttribute>(model);
            if (modelAttribute == null) 
                throw new InvalidOperationException("Model must be decorated with [Endpoint] attribute");

            string queryUrl = modelAttribute.Path;
            if (!string.IsNullOrEmpty(rootUrl)) 
                queryUrl = HttpTools.CombineRootUrl(rootUrl, queryUrl);

            var req = new HttpRequestMessage(modelAttribute.Method, queryUrl);

            SetRequestParameters(model, req);

            return req;
        }

        /// <summary>
        /// Maps model properties decorated with [EndpointParameter] to HTTP request parameters.
        /// Supports Path, Query, Header, FormData, and Body parameter types.
        /// Validates [Required] attributes on properties.
        /// </summary>
        /// <param name="model">The request model with decorated properties</param>
        /// <param name="http">The HTTP request message to configure</param>
        /// <exception cref="ArgumentNullException">Thrown when http is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when required parameter is missing or path placeholder not found</exception>
        /// <exception cref="APP.DIAG.CascadeException">Thrown when parameter mapping fails</exception>
        public static void SetRequestParameters(object model, HttpRequestMessage http) 
        {
            try
            {
                if (model == null) return;
                if (http == null) throw new ArgumentNullException(nameof(http));
                
                var parametersWithInfo = AttributesTools.GetPropertiesWithAttribute<EndpointParameterAttribute>(model.GetType());
                var queryParameters = new Dictionary<string, string>();
                var formDataParameters = new Dictionary<string, string>();
                var uriBuilder = new StringBuilder(http.RequestUri.ToString());
                object bodyContent = null;

                foreach (var kvp in parametersWithInfo)
                {
                    var property = kvp.Key;
                    var attribute = kvp.Value;
                    var propertyValue = property.GetValue(model);
                    
                    // P8: Auto-detect name from property if not specified
                    var parameterName = string.IsNullOrEmpty(attribute.Name) ? property.Name : attribute.Name;
                    
                    // P3: Validate [Required] attribute
                    var isRequired = property.GetCustomAttribute<RequiredAttribute>() != null;
                    if (isRequired && propertyValue == null)
                    {
                        throw new InvalidOperationException($"Required parameter '{parameterName}' is null");
                    }

                    // P4: Apply StringFormat if specified
                    string serializedValue = SerializeValue(propertyValue, attribute.StringFormat);

                    if (attribute.ParameterType == HttpParameterTypeEnum.Path)
                    {
                        var placeholder = "{" + parameterName + "}";
                        var currentUri = uriBuilder.ToString();
                        
                        // P1: Validate path placeholder exists (case-insensitive)
                        int placeholderIndex = currentUri.IndexOf(placeholder, StringComparison.OrdinalIgnoreCase);
                        if (placeholderIndex == -1)
                        {
                            throw new InvalidOperationException($"Path parameter '{parameterName}' not found in URI template: {currentUri}");
                        }
                        
                        if (serializedValue == null)
                        {
                            throw new InvalidOperationException($"Path parameter '{parameterName}' cannot be null");
                        }
                        
                        // Replace with actual case from URI (not placeholder name)
                        uriBuilder.Remove(placeholderIndex, placeholder.Length);
                        uriBuilder.Insert(placeholderIndex, serializedValue);
                    }
                    else if (attribute.ParameterType == HttpParameterTypeEnum.Query)
                    {
                        // Skip null or empty query parameters (unless required)
                        if (propertyValue == null || string.IsNullOrEmpty(serializedValue)) continue;
                        queryParameters.Add(parameterName, serializedValue);
                    }
                    else if (attribute.ParameterType == HttpParameterTypeEnum.Header)
                    {
                        // Skip null or empty headers
                        if (propertyValue == null || string.IsNullOrEmpty(serializedValue)) continue;
                        http.Headers.TryAddWithoutValidation(parameterName, serializedValue);
                    }
                    else if (attribute.ParameterType == HttpParameterTypeEnum.FormData)
                    {
                        // Skip null or empty form data
                        if (propertyValue == null || string.IsNullOrEmpty(serializedValue)) continue;
                        formDataParameters.Add(parameterName, serializedValue);
                    }
                    else if (attribute.ParameterType == HttpParameterTypeEnum.Body)
                    {
                        // P2: Support Body parameter
                        if (bodyContent != null)
                        {
                            throw new InvalidOperationException("Multiple Body parameters found. Only one Body parameter is allowed per request.");
                        }
                        bodyContent = propertyValue;
                    }
                }

                // P7: Build final URI once (optimized)
                if (queryParameters.Count > 0)
                {
                    var query = HttpTools.GetQueryString(queryParameters);
                    string separator = uriBuilder.ToString().Contains("?") ? "&" : "?";
                    uriBuilder.Append(separator).Append(query);
                }
                
                http.RequestUri = new Uri(uriBuilder.ToString());
                
                // Set content (Body has priority over FormData)
                if (bodyContent != null)
                {
                    var jsonOptions = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var jsonContent = JsonSerializer.Serialize(bodyContent, jsonOptions);
                    http.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                }
                else if (formDataParameters.Count > 0)
                {
                    http.Content = new FormUrlEncodedContent(formDataParameters);
                }

            }
            catch (Exception ex)
            {
                throw new APP.DIAG.CascadeException("SetRequestParameters", ex);
            }
        }

        /// <summary>
        /// Serializes a value to string, applying optional format string.
        /// Handles DateTime, IEnumerable, and standard ToString() conversion.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="format">Optional format string (e.g., "yyyy-MM-dd" for DateTime)</param>
        /// <returns>Serialized string value or null</returns>
        private static string SerializeValue(object value, string format = null)
        {
            if (value == null) return null;

            // Apply custom format if specified
            if (!string.IsNullOrEmpty(format))
            {
                if (value is IFormattable formattable)
                {
                    return formattable.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            // Handle DateTime with default ISO format
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
            }

            // Handle collections (for query parameters like ?tags=a&tags=b)
            if (value is IEnumerable enumerable && !(value is string))
            {
                var items = enumerable.Cast<object>().Select(x => x?.ToString()).Where(x => !string.IsNullOrEmpty(x));
                return string.Join(",", items); // Note: Comma-separated for single parameter
            }

            return value.ToString();
        }

        /// <summary>
        /// Parses HTTP response headers back into model properties.
        /// Properties decorated with [EndpointParameter(HttpParameterTypeEnum.Header)] will be populated
        /// with values from response headers.
        /// </summary>
        /// <typeparam name="TModel">The model type to populate</typeparam>
        /// <param name="model">The model instance to populate with response data</param>
        /// <param name="http">The HTTP response message containing headers</param>
        /// <exception cref="ArgumentNullException">Thrown when http is null</exception>
        /// <exception cref="APP.DIAG.CascadeException">Thrown when response parsing fails</exception>
        public static void ParseResponseParameters<TModel>(TModel model, HttpResponseMessage http)
        {
            try
            {
                if (model == null) return;
                if (http == null) throw new ArgumentNullException(nameof(http));
                
                var parameters = AttributesTools.GetPropertiesWithAttribute<EndpointParameterAttribute>(model?.GetType());

                foreach (var kvp in parameters)
                {
                    // Only parse header parameters from response
                    if (kvp.Value.ParameterType == HttpParameterTypeEnum.Header)
                    {
                        // P8: Auto-detect name from property if not specified
                        var parameterName = string.IsNullOrEmpty(kvp.Value.Name) ? kvp.Key.Name : kvp.Value.Name;
                        
                        // Try response headers first
                        var headerValue = http.Headers
                            .FirstOrDefault(x => x.Key.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                            .Value?.FirstOrDefault();
                        
                        // If not found, try content headers (e.g., Content-Type, Content-Length)
                        if (headerValue == null && http.Content?.Headers != null)
                        {
                            headerValue = http.Content.Headers
                                .FirstOrDefault(x => x.Key.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                                .Value?.FirstOrDefault();
                        }
                        
                        if (headerValue != null)
                        {
                            kvp.Key.SetValue(model, headerValue);
                        }
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
