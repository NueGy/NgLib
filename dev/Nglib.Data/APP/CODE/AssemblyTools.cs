using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.CODE
{
    /// <summary>
    /// Outils pour manipuler les assemblies et gérer les disposables
    /// </summary>
    public static class AssemblyTools
    {



        /// <summary>
        /// Forcer le chargement d'une dll dans le contexte de l'application si n'existe pas déjà
        /// </summary>
        /// <returns>True si chargé, False si déjà présent ou erreur en mode safe</returns>
        public static bool LoadAssembly(string dllName, bool safe = false)
        {
            if (string.IsNullOrEmpty(dllName)) return false;
            string RealPath = null;
            try
            {
                if (!dllName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("Le nom de fichier doit se terminer par .dll");
                RealPath = dllName;
                if (!RealPath.Contains("/") && !RealPath.Contains("\\"))
                {
                    var fiopendll = new System.IO.FileInfo(typeof(AssemblyTools).Assembly.Location);
                    RealPath = Path.Combine(fiopendll.Directory.FullName, dllName);
                }


                var assembyName = AssemblyName.GetAssemblyName(RealPath);
                // Vérifier si l'assembly est déja chargé
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == assembyName.FullName))
                    return false;

                //Chargement
                System.Reflection.Assembly.Load(assembyName);
                return true;

            }
            catch (Exception ex)
            {
                if (safe) return false;
                throw new Exception($"LoadAssembly({dllName})({RealPath}):" + ex.Message, ex);
            }
        }


        /// <summary>
        /// Dispose une classe manière safe (Aucune Erreur)
        /// </summary>
        public static bool DisposeSafe(this IDisposable disposable)
        {
            try
            {
                if (disposable == null) return false;
                disposable?.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
