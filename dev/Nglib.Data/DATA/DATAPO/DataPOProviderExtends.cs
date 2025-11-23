using Nglib.DATA.CONNECTOR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Méthodes d'extension pour les providers de PO
    /// </summary>
    public static class DataPOProviderExtends
    {


        /// <summary>
        /// Mettre à jours plusieurs objets en même temps avec les mêmes valeurs
        /// </summary>
        public static async Task UpdatePOAsync<Tobj>(this DataPOProviderSQL<Tobj> provider, Tobj[] bubbles, string columnKey, object columnValue)
             where Tobj : DATAPO.DataPO, new()
        {
            await provider.UpdatePOAsync(bubbles, new Dictionary<string, object>() { { columnKey, columnValue } });
        }

        /// <summary>
        /// Permet de retourner une liste d'objets depuis une requête SQL
        /// </summary>
        public static Task<CollectionPO<Tobj>> QueryPOAsync<Tobj>(this DataPOProviderSQL<Tobj> provider, string sqlQuery, Dictionary<string, object> paramKeySearch)
            where Tobj : DATAPO.DataPO, new()
        {
            var query = new QueryContext(sqlQuery, paramKeySearch);
            return provider.QueryPOAsync(query);
        }


        /// <summary>
        /// Recherche des objets avec requête SQL paramétrée (params)
        /// </summary>
        /// <param name="sqlQuery">Requête SQL avec paramètres @p1, @p2, etc.</param>
        /// <param name="insparam">Valeurs des paramètres dans l'ordre</param>
        /// <returns>Collection d'objets DataPO</returns>
        /// <example>
        /// await QueryPOWithParamsAsync("WHERE tenantid=@p1 AND active=@p2", 123, true)
        /// </example>
        public static Task<CollectionPO<Tobj>> QueryPOAsync<Tobj>(this DataPOProviderSQL<Tobj> provider, string sqlQuery, params object[] insparam)
             where Tobj : DATAPO.DataPO, new()
        {
            Dictionary<string, object> paramKeySearch = new Dictionary<string, object>();
            if (insparam != null)
            {
                int ii = 1;
                foreach (var item in insparam)
                {
                    paramKeySearch.Add("p" + ii, item);
                    ii++;
                }
            }
            var qry = new QueryContext(sqlQuery, paramKeySearch);
            return provider.QueryPOAsync(qry);
        }





        #region --- Méthodes synchrones (NOT ASYNC) - Wrappers des méthodes async - OBSOLETE ---


        /// <summary>
        /// Insère un objet DataPO en base de données (INSERT)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public static void InsertPO<Tobj>(this DataPOProviderSQL<Tobj> provider, Tobj poItem) 
            where Tobj : DATAPO.DataPO, new()
           => provider.InsertPOAsync(new Tobj[] { poItem }).GetAwaiter().GetResult();


        /// <summary>
        /// Insère un objet DataPO en base de données (INSERT)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public static bool SavePO<Tobj>(this DataPOProviderSQL<Tobj> provider, Tobj poItem) 
            where Tobj : DATAPO.DataPO, new()
            => provider.SavePOAsync(new Tobj[] { poItem }).GetAwaiter().GetResult();


        /// <summary>
        /// Insère un objet DataPO en base de données (INSERT)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public static void DeletePO<Tobj>(this DataPOProviderSQL<Tobj> provider, Tobj poItem) 
            where Tobj : DATAPO.DataPO, new()
            => provider.DeletePOAsync(new Tobj[] { poItem }).GetAwaiter().GetResult();
 
 


        /// <summary>
        /// Obtient un objet par son ID auto-incrémenté (synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public static Tobj GetPO<Tobj>(this DataPOProviderSQL<Tobj> provider, long idIncrement, int tenantId = 0)
            where Tobj : DATAPO.DataPO, new()
            => provider.GetPOAsync(idIncrement, tenantId).GetAwaiter().GetResult();

        /// <summary>
        /// Obtient un objet par des paramètres (synchrone)
        [Obsolete("Use Async method instead")]
        public static Tobj GetPO<Tobj>(this DataPOProviderSQL<Tobj> provider, Dictionary<string, object> sqlParams)
            where Tobj : DATAPO.DataPO, new()
            => provider.GetPOAsync(sqlParams).GetAwaiter().GetResult();

        /// <summary>
        /// Recherche des objets et retourne une collection spécifique (synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public static TCollection QueryPO<TCollection, Tobj>(this DataPOProviderSQL<Tobj> provider, string sqlQuery, Dictionary<string, object> paramKeySearch)
            where TCollection : CollectionPO<Tobj>, new()
            where Tobj : DATAPO.DataPO, new()
            => provider.QueryPOAsync<TCollection>(new QueryContext(sqlQuery, paramKeySearch)).GetAwaiter().GetResult();

        /// <summary>
        /// Recherche des objets depuis une requête SQL (synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public static CollectionPO<Tobj> QueryPO<Tobj>(this DataPOProviderSQL<Tobj> provider, string sqlQuery, Dictionary<string, object> paramKeySearch)
            where Tobj : DATAPO.DataPO, new()
        {
            var query = new QueryContext(sqlQuery, paramKeySearch);
            return provider.QueryPOAsync(query).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Recherche des objets avec limite et paramètres (synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public static CollectionPO<Tobj> QueryPO<Tobj>(this DataPOProviderSQL<Tobj> provider, int nbMax = 1000, Dictionary<string, object> paramKeySearch = null)
            where Tobj : DATAPO.DataPO, new()
        {
            return provider.QueryPOAsync(nbMax, paramKeySearch).GetAwaiter().GetResult();
        }

        ///// <summary>
        ///// Recherche des objets et cast vers une collection spécifique (synchrone)
        ///// </summary>
        //[Obsolete("Use Async method instead")]
        //public static CollectionPO<Tobj> QueryPOCollection<Tcollection, Tobj>(this DataPOProviderSQL<Tobj> provider, string sqlQuery, Dictionary<string, object> paramKeySearch)
        //    where Tcollection : CollectionPO<Tobj>, new()
        //    where Tobj : DATAPO.DataPO, new()
        //    => provider.QueryPO(sqlQuery, paramKeySearch).CastTo<Tcollection>();


        #endregion



    }
}
