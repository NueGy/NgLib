//using System;
//using System.Collections.Generic;
//using System.Security.Principal;
//using System.Text;

//namespace Nglib.WEB.PIPELINE
//{
//    /// <summary>
//    /// Représente une Requête sur le pipeline du Serveur MVC 
//    /// </summary>
//    public interface IPipelineRequestContext : IDisposable
//    {
//        /// <summary>
//        /// Identifiant Unique de la requête HTTP
//        /// </summary>
//        string RequestId { get; }

//        /// <summary>
//        /// Route utilisé
//        /// </summary>
//        string RoutePath { get; set; }

//        /// <summary>
//        /// Temps
//        /// </summary>
//        long TotalElapsedMs { get; set; }

//        /// <summary>
//        /// Mode de stockage
//        /// </summary>
//        RequestPersistantModeEnum PersistantMode { get; set; }
       

//        /// <summary>
//        /// Status de la requete
//        /// </summary>
//        RequestStateEnum State { get; set; }

//        /// <summary>
//        /// Status text de la requete
//        /// </summary>
//        string StateText { get; set; }

//        /// <summary>
//        /// Date de création de la requette
//        /// </summary>
//        DateTime DateCreate { get; }


//        /// <summary>
//        /// Données NoSql supplémentaires
//        /// </summary>
//        Nglib.DATA.ACCESSORS.IDataAccessor Flux { get; }

 

//        /// <summary>
//        /// Erreur sur la requette
//        /// </summary>
//        Exception MasterException { get; set; }
         

//        /// <summary>
//        /// Ajouter un log dans l'objet
//        /// </summary>
//        /// <param name="msg"></param>
//        void AddRequestLog(string msg);

//        /// <summary>
//        /// Vider le cache de l'objet, sans effacer les valeurs importante
//        /// </summary>
//        void DisposeLarge();

//    }
//}
