using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.APP.ENV
{
    /// <summary>
    /// Environnement d'execution
    /// </summary>
    public interface IGlobalEnv : IDisposable
    {
        /// <summary>
        /// Une liste de connecteurs SGBD
        /// </summary>
        DATA.CONNECTOR.ConnectorCollection Connectors { get; set; }

        /// <summary>
        /// (re)Chargement de l'environnement
        /// </summary>
        /// <returns></returns>
        bool Load();

        /// <summary>
        /// Obtenir un parametre de la configuration
        /// </summary>
        string GetConfig(string name);


         



    }
}
