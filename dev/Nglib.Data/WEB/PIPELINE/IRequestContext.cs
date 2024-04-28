using System;
using System.Collections.Generic;
using System.Text;
using Nglib.SOLUTIONS.IDENTITY.TENANTS;

namespace Nglib.WEB.PIPELINE
{
    /// <summary>
    /// Représente une Requête sur le pipeline du Serveur MVC
    /// </summary>
    public interface IRequestContext :  IDisposable
    {
        /// <summary>
        /// Identifiant Unique de la requête
        /// HttpContext.TraceIdentifier
        /// </summary>
        string RequestId { get; }

        /// <summary>
        /// Route utilisé
        /// </summary>
        string RoutePath { get; set; }

        /// <summary>
        /// Temps
        /// </summary>
        long TotalElapsedMs { get; set; }

        /// <summary>
        /// Mode de stockage
        /// </summary>
        RequestPersistantModeEnum PersistantMode { get; set; }
       

        /// <summary>
        /// Status de la requête
        /// </summary>
        RequestStateEnum State { get; set; }
        string StateText { get; set; }

        /// <summary>
        /// Date de création de la requête
        /// </summary>
        DateTime DateCreate { get; }


        string AppName { get; set; }
        //IDENTITY.APPCLIENTS.IAppClient AppClient { get; set; }


        /// <summary>
        /// Tenant identifier via l'url
        /// </summary>
        ITenant2 TenantOnUrl { get; set; }

        /// <summary>
        /// Tenant Identifié via l'utilsateur
        /// </summary>
        ITenant2 TenantOnUser { get; set; }
        ////int TenantId { get; set; }

        int UserId { get; set; }


        /// <summary>
        /// Données NoSql supplémentaires
        /// </summary>
        Nglib.DATA.ACCESSORS.IDataAccessor Flux { get; }


        [Obsolete]
        Exception MasterException { get; set; }

        [Obsolete]
        System.Diagnostics.Stopwatch stopWatch { get; }

  


        /// <summary>
        /// Ajouter un log dans le stack de la requête
        /// </summary>
        /// <param name="msg"></param>
        void AddRequestLog(string msg);

        /// <summary>
        /// Vider le cache de l'objet, sans effacer les valeurs importante
        /// </summary>
        void DisposeLarge();

    }
}
