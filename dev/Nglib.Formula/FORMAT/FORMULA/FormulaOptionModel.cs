using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Option paramétrable pour le moteur de formule
    /// </summary>
    public class FormulaOptionModel
    {
        /// <summary>
        ///  Permet d'autoriser l'utilisation de formules non sécurisées ( Accès fichiers, ...)
        /// </summary>
        public static bool AllowUnsecureFormula = false;


        // Concaténation Laxiste = Si un nombre est dans une chaine, on converti en numérique
        // Debug mode

    }
}
