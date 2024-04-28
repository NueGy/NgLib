using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.ENV
{
    /// <summary>
    /// Représente un environnement/context d'execution avec tous les paramètres, connecteurs, providers ...
    /// </summary>
    public interface IEnv : IDisposable
    {

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
