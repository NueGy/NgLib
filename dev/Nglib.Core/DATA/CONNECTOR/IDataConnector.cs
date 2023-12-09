// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR
{



    /// <summary>
    /// Objet pour réaliser des requettes sql
    /// </summary>
    public interface IDataConnector : IDisposable, ICloneable
    {

        /// <summary>
        /// Nom du connecteur
        /// </summary>
        string ConnectorName { get; }

        /// <summary>
        /// Le connecteur peut être utilisé en lecture seulement
        /// </summary>
        bool ReadOnly { get; }

        /// <summary>
        /// Les threads ne partagerons pas les connection ouverte.
        /// Il faudra attendre que la connection soit fermer pour en ouvrir une nouvelle
        /// </summary>
        bool MultiThreadingSafe { get;}


        /// <summary>
        /// Nom du moteur SGBD
        /// </summary>
        string EngineName { get; }


        /// <summary>
        /// Obtient les context de toutes les requetes arpès l'éxecution
        /// </summary>
        event QueryCompletedHandler QueryCompleted;


        /// <summary>
        /// evenement avant l'execution de la requète
        /// </summary>
        event QueryCompletedHandler QueryBegin;



        /// <summary>
        /// Définir la chaine de connection
        /// </summary>
        /// <param name="str"></param>
        void SetConnectionString(string connectionString, string defaultEngine);


        /// <summary>
        /// Ouvrir la connexion
        /// </summary>
        /// <param name="keepOpen">Garder la connexion ouverte après la prochaine requette</param>
        /// <returns></returns>
        bool Open(bool keepOpen=true);

        /// <summary>
        /// fermer la connexion (rollbakc si nécessaire)
        /// </summary>
        /// <returns></returns>
        bool Close(bool safe = true);

        /// <summary>
        /// Ouvrir une transaction SQL
        /// </summary>
        /// <param name="transactionName"></param>
        /// <returns></returns>
        bool BeginTransaction(string transactionName = null);

        /// <summary>
        /// ROLLBACK transaction SQL
        /// </summary>
        /// <param name="transactionName"></param>
        /// <returns></returns>
        bool RollBackTransaction(bool safe = false);

        /// <summary>
        /// Commit transaction SQL
        /// </summary>
        /// <returns></returns>
        bool CommitTransaction();


        /// <summary>
        /// Obtient l'objet connection standard
        /// </summary>
        /// <returns></returns>
        System.Data.IDbConnection GetDbConnection();


        /// <summary>
        /// Execution d'une requete classique, qui retourne un Dataset
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<System.Data.DataSet> QueryDataSetAsync(QueryContext queryContext);

        /// <summary>
        /// Execution d'une requete scalar simple
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<object> QueryScalarAsync(QueryContext queryContext);


        /// <summary>
        /// Insertion d'une table entiere
        /// </summary>
        /// <returns></returns>
        Task<List<long>> InsertTableAsync(System.Data.DataTable dataTable, int SpecialTimeOut = 600, string AutoIncrementColumn = null);

        ///// <summary>
        ///// Permet l'export de tous le schéma de la base (toutes les tables et colonnes)
        ///// </summary>
        ///// <returns></returns>
        //Task<System.Data.DataSet> ExportSchemaAsync();



    }


}
