using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Erreur pour les formules
    /// </summary>
    public class FormulaException : Exception
    {
        /// <summary>
        /// Nom de la formule
        /// </summary>
        public string Command { get; set; }
        
        public FormulaException(string command, string message) : base(message)
        {
            Command = command;
        }

        public FormulaException(string command, string message, Exception innerException) : base(message, innerException)
        {
            Command = command;
        }
    }
}
