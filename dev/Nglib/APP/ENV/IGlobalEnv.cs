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
        /// Chargement de l'environnement
        /// </summary>
        /// <returns></returns>
        bool Load();

        /// <summary>
        /// Données sur l'environnement
        /// </summary>
        DATA.ACCESSORS.IDataAccessor EnvDatas { get; set; }

    }
}
