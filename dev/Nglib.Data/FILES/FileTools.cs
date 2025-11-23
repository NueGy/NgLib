using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Text;

namespace Nglib.FILES
{
    /// <summary>
    /// Outils génériques pour la gestion de fichiers
    /// </summary>
    public static class FileTools
    {

        /// <summary>
        /// Ajoute une extension à un fichier
        /// monfichier.csv.OK
        /// </summary>
        public static void AddExtention(System.IO.FileInfo file, string ext, bool overwrite=true)
        {
            if(string.IsNullOrEmpty(ext) || file==null) return;
            ext = ext.Trim().Trim('.');
            if (file.Extension == "." + ext) return; //déjà la bonne extension
            string[] exts = file.Name.Split('.');
            if (exts.Length > 2) return;    //n'accepte pas plus de 2 extensions

            string newfile = file.FullName.Substring(0, file.FullName.Length - file.Extension.Length) + "." + ext;
            file.MoveTo(newfile, overwrite);
        }

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

        /// <summary>
        /// Obtenir des fichiers selon des extensions
        /// </summary>
        public static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, SearchOption searchOption, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = Enumerable.Empty<FileInfo>();
            foreach (string ext in extensions)
            {
                files = files.Concat(dir.GetFiles(ext, searchOption));
            }
            return files;
        }

        /// <summary>
        /// Si une expression pattern correspond à un fichier
        /// </summary>
        public static bool IsMatchPattern(string expression, string filename)
        {
            if (string.IsNullOrWhiteSpace(expression) || string.IsNullOrWhiteSpace(filename)) return false;
            bool ignoreCase = true;
            EnumerationOptions options = new EnumerationOptions { MatchType = MatchType.Win32, AttributesToSkip = 0, IgnoreInaccessible = false };
            //  new EnumerationOptions { RecurseSubdirectories = true, MatchType = MatchType.Win32, AttributesToSkip = 0, IgnoreInaccessible = false };
            return options.MatchType switch
            {
                MatchType.Simple => FileSystemName.MatchesSimpleExpression(expression.AsSpan(), filename.AsSpan(), ignoreCase),
                MatchType.Win32 => FileSystemName.MatchesWin32Expression(expression.AsSpan(), filename.AsSpan(), ignoreCase),
                _ => throw new ArgumentOutOfRangeException(nameof(options)),
            };
        }

    }
}
