using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.FILES
{
    /// <summary>
    /// Permet de gérer un répertoire temporaire
    /// </summary>
    public class TempDirectory : IDisposable
    {
        /// <summary>
        /// Chemin racine des répertoires temporaires
        /// </summary>
        public static string RootDirectoryPath = null;

        /// <summary>
        /// Si le répertoire temporaire a été initialisé
        /// </summary>
        public bool IsCreated { get; private set; }

        /// <summary>
        /// Chemin du répertoire temporaire
        /// Son acces forcera son Initialisation
        /// </summary>
        public System.IO.DirectoryInfo Directory { get { if (!IsCreated) this.Create(); return _Directory; } }
        private System.IO.DirectoryInfo _Directory { get; set; }


        public TempDirectory()
        {
            string tempdirPath = null;
            string tempdirName = $"ZTEMP{DateTime.Now.ToString("yyyyMMdd")}_" + System.Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(RootDirectoryPath))
                tempdirPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), tempdirName);
            else
                tempdirPath = System.IO.Path.Combine(RootDirectoryPath, tempdirName);

            _Directory = new System.IO.DirectoryInfo(tempdirPath);
        }


        /// <summary>
        /// Création du répertoire temporaire si pas déja fait
        /// </summary>
        public void Create()
        {
            if (this.IsCreated) return;
            try
            {
                Directory.Create();
                this.IsCreated = true;
            }
            catch (Exception e)
            {
                throw new Exception("TempDirectory.Create():" + e.Message, e);
            }
        }


        /// <summary>
        /// Supprimer le répertoire temporaire
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Delete()
        {
            try
            {
                if (Directory.Exists) Directory.Delete(true);
            }
            catch (Exception ex)
            {
                throw new Exception("TempDirectory.Delete() " + ex.Message, ex);
            }
        }


        /// <summary>
        /// Supprimer le répertoire temporaire
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.Delete();
            }
            catch (Exception)
            {
                return;//safe
            }
        }





    }
}




