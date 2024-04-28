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
    /// Permet de manipuler l'objet IConfiguration comme un IDataAccessor
    /// </summary>
    public static class ConfigurationTools
    {


        /// <summary>
        /// Enveloppe un IConfiguration en IDataAccessor
        /// </summary>
        /// <param name="Configuration"></param>
        /// <returns></returns>
        public static IDataAccessor GetConfigurationAccessor(this IConfiguration Configuration)
        {
            ConfigurationAccessor configurationAccessor = new ConfigurationAccessor(Configuration);
            return configurationAccessor;
        }



        public class ConfigurationAccessor : IDataAccessor
        {
            public IConfiguration Configuration;
            public ConfigurationAccessor(IConfiguration Configuration)
            {
                this.Configuration = Configuration;
            }

            public IDataAccessorCryptoContext GetCryptoContext() => null;


            public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
            {
                // Récupérer la valeur de Configuration avec ignorecase
                return Configuration[nameValue];
            }

            public string[] ListFieldsKeys()
            {
                // Enumérer la liste des clés de Configuration 
                return Configuration.AsEnumerable(true).Select(x => x.Key).ToArray();
            }

            public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
            {
                Configuration[nameValue] = obj.ToString();
                return true;
            }
        }



    }
}
