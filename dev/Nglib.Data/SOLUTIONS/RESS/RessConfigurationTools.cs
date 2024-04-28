using Microsoft.Extensions.Configuration;
using Nglib.DATA.ACCESSORS;
using Nglib.SOLUTIONS.RESS.RESSOURCES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.SOLUTIONS.RESS
{
    public static class RessConfigurationTools
    {


        public static void MapConfigurationToRessource(IConfiguration config, string configSectionPrefix, IRessourceItem ressource)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (ressource == null) throw new ArgumentNullException(nameof(ressource));

            var configSection = config.GetSection(configSectionPrefix);
            if (configSection == null) throw new Exception("Configuration section not found : " + configSectionPrefix);
            var confsectionvalues = APP.CONFIG.ConfigurationTools.GetConfigurationAccessor(configSection);
            DataAccessorTools.CopyTo(confsectionvalues, ressource.DataFlow);
          

        }




    }
}
