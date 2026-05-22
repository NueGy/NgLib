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
        /// Connecteur de base de données principal
        /// </summary>
        DATA.CONNECTOR.IDataConnector ConnectorDatabase { get; }

        /// <summary>
        /// Configuration
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Convertit un chemin relatif en chemin absolu basé sur le répertoire de l'environnement
        /// </summary>
        /// <param name="path">Chemin relatif ou absolu</param>
        /// <returns>Chemin absolu</returns>
        string GetAbsolutePath(string path);
    }
}
