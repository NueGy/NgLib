using Microsoft.Extensions.Configuration;
using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.COLLECTIONS
{
    public static class ConfigurationTools
    {

 

        public static ACCESSORS.IDataAccessor GetConfigurationAccessor(this IConfiguration Configuration)
        {
            ConfigurationAccessor configurationAccessor = new ConfigurationAccessor(Configuration);
            return configurationAccessor;
        }



        public class ConfigurationAccessor : ACCESSORS.IDataAccessor
        {
            public IConfiguration Configuration;
            public ConfigurationAccessor(IConfiguration Configuration)
            {
                this.Configuration = Configuration;
            }

            public IDataAccessorCryptoContext GetCryptoContext() =>null;
        

            public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
            {
                // Récupérer la valeur de Configuration avec ignorecase
                return this.Configuration[nameValue];
            }

            public string[] ListFieldsKeys()
            {
                // Enumérer la liste des clés de Configuration 
                return this.Configuration.AsEnumerable().Select(x => x.Key).ToArray(); 
            }

            public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
            {
                this.Configuration[nameValue] = obj.ToString();
                return true;
            }
        }



    }
}
