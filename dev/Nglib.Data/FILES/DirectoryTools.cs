using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FILES
{
    public static class DirectoryTools
    {


        /// <summary>
        /// Déplacer le contenu d'un répertoire vers un autre
        /// </summary>
        public static void MoveDirectoryContent(System.IO.DirectoryInfo srcDirectory, DirectoryInfo destDirectory, bool overide = false, string[] Ignores = null)
        {
            try
            {
                if (srcDirectory == null || !srcDirectory.Exists) throw new Exception("Répertoire source inexistant");
                if (destDirectory == null || !destDirectory.Exists) throw new Exception("Répertoire destination inexistant");

                foreach (var file in srcDirectory.GetFiles())
                {
                    if (Ignores != null && Ignores.Contains(file.Name, true)) continue;
                    file.MoveTo(System.IO.Path.Combine(destDirectory.FullName, file.Name), overide);
                }

                foreach (var dir in srcDirectory.GetDirectories())
                {
                    if (Ignores != null && Ignores.Contains(dir.Name, true)) continue;
                    dir.MoveTo(System.IO.Path.Combine(destDirectory.FullName, dir.Name));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("MoveDirectoryContent " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Obtenir les fichiers d'un répertoire en fonction de plusieurs patterns
        /// </summary>
        public static FileInfo[] GetMultiFiles(this System.IO.DirectoryInfo srcDirectory, SearchOption searchOption, params string[] searchPaterns)
        {
            List<FileInfo> files = new List<FileInfo>();
            foreach (string patern in searchPaterns)
            {
                if (string.IsNullOrWhiteSpace(patern)) continue;
                files.AddRange(srcDirectory.GetFiles(patern, searchOption));
            }
            return files.Distinct().ToArray();
        }

        /// <summary>
        /// Obtenir les fichiers d'un répertoire en fonction de plusieurs patterns
        /// </summary>
        public static FileInfo[] GetMultiFiles(this System.IO.DirectoryInfo srcDirectory, params string[] searchPaterns)
            => GetMultiFiles(srcDirectory, SearchOption.TopDirectoryOnly, searchPaterns);



        /// <summary>
        /// Obtenir la taille d'un répertoire
        /// </summary>
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
                throw new Exception("GetDirectorySize " + ex.Message, ex);
            }
        }



        public static string TrimDirectoryPath(string path)
        {
            if (path == null) return null;
            if (string.IsNullOrWhiteSpace(path)) return "";
            path = path.Trim();
            if (!path.EndsWith("/")) path += "/";
            return path;
        }


    }
}
