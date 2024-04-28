using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nglib.FILES
{
    /// <summary>
    /// Outils génériques pour la gestion de fichiers
    /// </summary>
    public static class FileTools
    {

        // Rename OK or KO


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
            chaine = Nglib.FORMAT.StringTools.ReplaceDiacritics(chaine);
            //!!!
            return chaine;
        }

        public static string PrettySizeString(long byteCount)
        {
            if (byteCount == 0) return "";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (byteCount >= 1024 && order < sizes.Length - 1)
            {
                order++;
                byteCount = byteCount / 1024;
            }
            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", byteCount, sizes[order]);
        }

        public static long GetDirectorySize(DirectoryInfo folder)
        {
            try
            {
                if (folder == null) return 0;
                var inefiles = folder.EnumerateFiles("*", SearchOption.AllDirectories);
                return inefiles.Sum(fi => fi.Length);
            }
            catch (Exception ex)
            {
                throw new Exception("GetDirectorySize "+ex.Message,ex);
            }
        }


    }
}
