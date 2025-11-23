using Microsoft.Extensions.Configuration;
using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.CONFIG
{
    /// <summary>
    /// Extension tools to use IConfiguration as an IDataAccessor for unified data access
    /// </summary>
    public static class ConfigurationTools
    {


        /// <summary>
        /// Wraps an IConfiguration instance as an IDataAccessor for unified access patterns
        /// </summary>
        /// <param name="Configuration">ASP.NET Core configuration instance</param>
        /// <returns>Data accessor wrapper for configuration</returns>
        public static IDataAccessor GetConfigurationAccessor(this IConfiguration Configuration)
        {
            ConfigurationAccessor configurationAccessor = new ConfigurationAccessor(Configuration);
            return configurationAccessor;
        }




        /// <summary>
        /// Internal adapter class that wraps IConfiguration to implement IDataAccessor interface
        /// </summary>
        public class ConfigurationAccessor : IDataAccessor
        {
            /// <summary>
            /// Underlying ASP.NET Core configuration instance
            /// </summary>
            public IConfiguration Configuration;
            
            /// <summary>
            /// Initializes a new configuration accessor wrapper
            /// </summary>
            /// <param name="Configuration">Configuration instance to wrap</param>
            public ConfigurationAccessor(IConfiguration Configuration)
            {
                this.Configuration = Configuration;
            }

            /// <summary>
            /// Gets crypto context (always null for configuration - no encryption support)
            /// </summary>
            /// <returns>Always returns null</returns>
            public IDataAccessorCryptoContext GetCryptoContext() => null;


            /// <summary>
            /// Gets configuration value by key with case-insensitive lookup
            /// </summary>
            /// <param name="nameValue">Configuration key to retrieve</param>
            /// <param name="AccesOptions">Access options (ignored for configuration)</param>
            /// <returns>Configuration value or null if not found</returns>
            public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
            {
                // Récupérer la valeur de Configuration avec ignorecase
                return Configuration[nameValue];
            }

            /// <summary>
            /// Gets all available configuration keys
            /// </summary>
            /// <returns>Array of all configuration keys</returns>
            public string[] ListFieldsKeys()
            {
                // Enumérer la liste des clés de Configuration 
                return Configuration.AsEnumerable(true).Select(x => x.Key).ToArray();
            }

            /// <summary>
            /// Sets configuration value by key
            /// </summary>
            /// <param name="nameValue">Configuration key to set</param>
            /// <param name="obj">Value to store (converted to string)</param>
            /// <param name="AccesOptions">Access options (ignored for configuration)</param>
            /// <returns>Always returns true</returns>
            public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
            {
                Configuration[nameValue] = obj.ToString();
                return true;
            }
        }



    }
}
