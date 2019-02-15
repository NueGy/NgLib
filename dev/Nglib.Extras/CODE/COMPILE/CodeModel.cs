using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Nglib.CODE.COMPILE
{
    /// <summary>
    /// Représente un code Source
    /// </summary>
    public class CodeModel : ICompiledCode
    {



        /// <summary>
        /// Le nom du code Source (clef pour obtenir le cache de compilation)
        /// </summary>
        public string SourceCodeName { get; set; }


        /// <summary>
        /// Chemin de la classe
        /// </summary>
        public string ClassFullName { get; set; }

        /// <summary>
        /// Résultat du code source compié
        /// </summary>
        public Type CompiledType { get; set; }



        /// <summary>
        /// Assempbly qui contient la méthode
        /// </summary>
        public Assembly Assembly { get; set; }


        /// <summary>
        /// Code Source
        /// </summary>
        public SyntaxTree SourceCode { get; set; }



        /// <summary>
        /// Références lié au codes
        /// </summary>
        public List<MetadataReference> References { get; set; }

        /// <summary>
        /// CodeBuilder.MasterMethodForExecute;
        /// </summary>
        public string CallMasterMethod => CodeBuilder.MasterMethodForExecute;
    }
}
