using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Nglib.CODE.COMPILE
{
    public interface  ICompiledCode
    {

        /// <summary>
        /// Uniq Key
        /// </summary>
        string SourceCodeName { get;  }


        /// <summary>
        /// Chemin de la classe
        /// </summary>
        string ClassFullName { get; }



        /// <summary>
        /// Methode d'appel principal
        /// </summary>
        string CallMasterMethod { get; }


        /// <summary>
        /// Résultat du code source compié
        /// </summary>
        Type CompiledType { get; }



        /// <summary>
        /// Assempbly qui contient la méthode
        /// </summary>
        Assembly Assembly { get; }

    }
}
