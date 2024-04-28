using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FILES.STORAGE
{
    public interface IStoreFile
    {
        /// <summary>
        /// Nom du fichier
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Nom complet du fichier (avec le chemin de stockage)
        /// </summary>
        string FullName { get; set; }

        /// <summary>
        /// Type de contenu (MIME)
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Définir le stream du fichier
        /// </summary>
        bool SetStream(Stream stream);

        /// <summary>
        /// Obtenir le stream du fichier
        /// </summary>
        /// <returns></returns>
        Stream GetStream();


        /// <summary>
        /// Taille du fichier
        /// </summary>
        long Length { get; set; }

        /// <summary>
        /// Hash
        /// </summary>
        string Hash { get;  }
        
        /// <summary>
        /// Si besoin de mettre à jour le fichier
        /// </summary>
        bool NeedUpdate { get; set; }

    }
}
