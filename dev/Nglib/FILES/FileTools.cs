using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.FILES
{
    /// <summary>
    /// Outils génériques pour la gestion de fichiers
    /// </summary>
    public static class FileTools
    {


        /// <summary>
        /// transforme une chaine pour un file path
        /// </summary>
        /// <param name="chaine"></param>
        /// <returns></returns>
        public static string FormatNameFile(string chaine)
        {
            chaine = chaine.Replace("/", "");
            chaine = chaine.Replace(@"\", "");
            chaine = chaine.Replace(":", "");
            chaine = chaine.Replace(" ", "");
            //!!!
            return chaine;
        }




    }
}
