using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.APP.ENV
{
    /// <summary>
    /// Environnement d'execution
    /// </summary>
    public interface IMasterEnv : IEnv
    {


        /// <summary>
        /// Une liste de connecteurs SGBD
        /// </summary>
        DATA.CONNECTOR.ConnectorCollection Connectors { get; set; }





        /// <summary>
        /// Configuration
        /// </summary>
        IConfiguration Configuration { get; }




    }
}
