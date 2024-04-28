using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FILES.STORAGE.REPOSITORY
{
    /// <summary>
    /// Solutions de stockage de fichiers cryptés basés sur les ressources nglib
    /// Représente un container de stockage
    /// </summary>
    public interface IStoreRepository
    {

        /// <summary>
        /// Données Nosql supplémentaires
        /// </summary>
        DATA.ACCESSORS.IDataAccessor DataFlow { get; }


        /// <summary>
        /// Création de ce container si il n'existe pas
        /// </summary>
        Task CreateRepositoryAsync();

        /// <summary>
        /// Suppression de ce container si il existe
        /// </summary>
        Task DeleteRepositoryAsync();


        /// <summary>
        /// Mettre à jours les informations du container sur la ressource de stockage
        /// </summary>
        /// <returns>status false:Invalide, true: Valide</returns>
        Task<bool> SyncRepositoryStatusAsync();


        /// <summary>
        /// Téléchargement d'un fichier
        /// </summary>
        Task DownloadAsync(IStoreFile fileobj);

        /// <summary>
        /// Upload d'un fichier
        /// </summary>
        /// <param name="filedata"></param>
        /// <returns></returns>
        Task UploadAsync(IStoreFile filedata); //string fileName,

        /// <summary>
        /// Suppression d'un fichier
        /// </summary>
        Task<bool> DeleteAsync(IStoreFile filedata, bool safe = false);

        /// <summary>
        /// Liste des fichiers
        /// </summary>
        Task<List<IStoreFile>> ListFileAsync(object form);

        /// <summary>
        /// Information sur un fichier
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<IStoreFile> GetFileInformationAsync(string fileName);
    }


}

