using Nglib.DATA.COLLECTIONS;
using Nglib.DATA.CONNECTOR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Provider générique multi-types permettant de manipuler plusieurs types de Models et DataPO en base de données.
    /// Préférer l'utilisation de DataPOProvider<> avec surcharge pour un usage type-safe strict!
    /// </summary>
    public class DataPOGenericProvider: IDataPOProvider
    {
        /// <summary>
        /// Connecteur de données partagé par toutes les opérations
        /// </summary>
        private IDataConnector Connector { get; set; }

        /// <summary>
        /// Initialise le provider générique avec un connecteur de données spécifique
        /// </summary>
        public DataPOGenericProvider(IDataConnector connector)
        {
            this.Connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        /// <summary>
        /// Initialise le provider générique avec l'environnement global
        /// </summary>
        public DataPOGenericProvider(APP.ENV.IGlobalEnv env)
        {
            this.Connector = env?.Connectors?.GetDefaultConnector();
            if (this.Connector == null)
                throw new ArgumentException("Impossible d'obtenir un connecteur depuis l'environnement", nameof(env));
        }


        /// <summary>
        /// Crée un provider générique pour Model (DataPO non typé) avec mapping par réflexion activé
        /// </summary>
        private DataPOProvider<TModel> CreateGenericProviderForModel<TModel>() where TModel : class, new()
        {
            var provider = new DataPOProvider<TModel>(Connector);
            provider.DefaultMappingByReflexion = true; // On active le mapping par réflexion pour le provider générique
            return provider;
        }

        /// <summary>
        /// Crée un provider typé pour DataPO
        /// </summary>
        private DataPOProviderSQL<TPo> CreateGenericProviderForPO<TPo>() where TPo : DataPO, new()
        {
            return new DataPOProviderSQL<TPo>(Connector);
        }


        #region ------- Méthodes Model Génériques -------

        /// <summary>
        /// Recherche des modèles en base avec limitation du nombre de résultats (version générique).
        /// Convertit automatiquement les DataPO en modèles métier via mapping par réflexion.
        /// </summary>
        /// <typeparam name="TModel">Type de modèle métier</typeparam>
        /// <param name="count">Nombre maximum de résultats (par défaut: 100)</param>
        /// <param name="listMode">Mode de listing optionnel</param>
        /// <returns>Collection de modèles avec métadonnées (ListResult)</returns>
        public Task<ListResult<TModel>> QueryModelAsync<TModel>(int count = 100, string listMode = null)
            where TModel : class, new()
        {
            return CreateGenericProviderForModel<TModel>().QueryModelAsync(count, listMode);
        }

        /// <summary>
        /// Obtient un modèle unique par son ID auto-incrémenté (version générique).
        /// </summary>
        /// <typeparam name="TModel">Type de modèle métier</typeparam>
        /// <param name="id">ID auto-incrémenté de l'enregistrement (doit être > 0)</param>
        /// <returns>Le modèle correspondant ou null si non trouvé ou ID invalide</returns>
        public Task<TModel> GetModelAsync<TModel>(long id)
            where TModel : class, new()
        {
            return CreateGenericProviderForModel<TModel>().GetModelAsync(id);
        }

        /// <summary>
        /// Obtient un modèle unique par des paramètres de recherche personnalisés (version générique).
        /// </summary>
        /// <typeparam name="TModel">Type de modèle métier</typeparam>
        /// <param name="keys">Dictionnaire des colonnes et valeurs pour la recherche</param>
        /// <returns>Le premier modèle correspondant aux critères ou null si non trouvé</returns>
        public Task<TModel> GetModelAsync<TModel>(Dictionary<string, object> keys)
            where TModel : class, new()
        {
            return CreateGenericProviderForModel<TModel>().GetModelAsync(keys);
        }

        /// <summary>
        /// Sauvegarde (met à jour) un modèle existant en base de données (opération UPDATE, version générique).
        /// IMPORTANT : Le modèle doit contenir les clés primaires valides.
        /// </summary>
        /// <typeparam name="TModel">Type de modèle métier</typeparam>
        /// <param name="model">Modèle contenant les données à mettre à jour</param>
        /// <returns>Le modèle mis à jour avec les éventuelles valeurs recalculées par la base</returns>
        public Task<TModel> SaveModelAsync<TModel>(TModel model)
            where TModel : class, new()
        {
            return CreateGenericProviderForModel<TModel>().SaveModelAsync(model);
        }

        /// <summary>
        /// Insère un nouveau modèle en base de données (opération INSERT, version générique).
        /// L'ID auto-incrémenté est automatiquement récupéré et disponible dans le modèle retourné.
        /// </summary>
        /// <typeparam name="TModel">Type de modèle métier</typeparam>
        /// <param name="model">Modèle contenant les données à insérer</param>
        /// <returns>Le modèle inséré avec son ID auto-généré par la base de données</returns>
        public Task<TModel> InsertModelAsync<TModel>(TModel model)
            where TModel : class, new()
        {
            return CreateGenericProviderForModel<TModel>().InsertModelAsync(model);
        }

        /// <summary>
        /// Supprime un modèle de la base de données (opération DELETE, version générique).
        /// Si l'enregistrement n'existe pas, retourne false sans générer d'exception.
        /// </summary>
        /// <typeparam name="TModel">Type de modèle métier</typeparam>
        /// <param name="model">Modèle à supprimer</param>
        /// <returns>true si supprimé avec succès, false si déjà supprimé ou inexistant</returns>
        public Task<bool> DeleteModelAsync<TModel>(TModel model)
            where TModel : class, new()
        {
            return CreateGenericProviderForModel<TModel>().DeleteModelAsync(model);
        }

        #endregion


        #region ------- Méthodes DataPO Génériques -------

        /// <summary>
        /// Obtient un objet DataPO par son ID auto-incrémenté (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à récupérer</typeparam>
        /// <param name="idIncrement">L'ID auto-incrémenté de l'enregistrement</param>
        /// <param name="TenantId">ID du tenant pour le filtrage multi-tenant (0 = pas de filtre)</param>
        /// <returns>L'objet DataPO correspondant ou null si non trouvé</returns>
        public Task<TPo> GetPOAsync<TPo>(long idIncrement, int TenantId = 0)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().GetPOAsync(idIncrement, TenantId);
        }

        /// <summary>
        /// Obtient un objet DataPO en utilisant la clé B36 (Format Nglib, version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à récupérer</typeparam>
        /// <param name="fullB36Key">Clé au format B36 contenant ItemId, TenantId, etc.</param>
        /// <returns>L'objet DataPO correspondant ou null si non trouvé ou clé invalide</returns>
        public Task<TPo> GetPOByKeyAsync<TPo>(string fullB36Key)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().GetPOByKeyAsync(fullB36Key);
        }

        /// <summary>
        /// Obtient un objet DataPO par des paramètres de recherche personnalisés (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à récupérer</typeparam>
        /// <param name="paramKeys">Dictionnaire des colonnes et valeurs à rechercher</param>
        /// <returns>Le premier objet DataPO correspondant ou null si non trouvé</returns>
        public Task<TPo> GetPOAsync<TPo>(Dictionary<string, object> paramKeys)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().GetPOAsync(paramKeys);
        }

        /// <summary>
        /// Vérifie l'existence d'un enregistrement en base via ses clés (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à vérifier</typeparam>
        /// <param name="keys">Dictionnaire des colonnes et valeurs pour identifier l'enregistrement</param>
        /// <returns>true si l'enregistrement existe, false sinon</returns>
        public Task<bool> ExistAsync<TPo>(Dictionary<string, object> keys)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().ExistAsync(keys);
        }

        /// <summary>
        /// Recherche des objets depuis une requête SQL et retourne une collection personnalisée (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à récupérer</typeparam>
        /// <typeparam name="TCollection">Type de collection spécifique héritant de CollectionPO</typeparam>
        /// <param name="query">Contexte de requête contenant le SQL et les paramètres</param>
        /// <returns>Collection personnalisée d'objets DataPO</returns>
        public Task<TCollection> QueryPOAsync<TPo, TCollection>(QueryContext query)
            where TPo : DataPO, new()
            where TCollection : CollectionPO<TPo>, new()
        {
            return CreateGenericProviderForPO<TPo>().QueryPOAsync<TCollection>(query);
        }

        /// <summary>
        /// Recherche des objets depuis une requête SQL et retourne une collection standard (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à récupérer</typeparam>
        /// <param name="query">Contexte de requête contenant le SQL et les paramètres</param>
        /// <returns>Collection d'objets DataPO</returns>
        public Task<CollectionPO<TPo>> QueryPOAsync<TPo>(QueryContext query)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().QueryPOAsync(query);
        }

        /// <summary>
        /// Recherche des objets avec limite et paramètres optionnels (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à récupérer</typeparam>
        /// <param name="limitResult">Nombre maximum de résultats à retourner</param>
        /// <param name="SqlParams">Paramètres de filtrage (WHERE columnName = value)</param>
        /// <returns>Collection d'objets DataPO correspondant aux critères</returns>
        public Task<CollectionPO<TPo>> QueryPOAsync<TPo>(int limitResult = 1000, Dictionary<string, object> SqlParams = null)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().QueryPOAsync(limitResult, SqlParams);
        }

        /// <summary>
        /// Recherche des objets en base et retourne la collection (version générique)
        /// Utilise un formulaire de recherche pour construire la requête SQL
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à récupérer</typeparam>
        /// <param name="form">Formulaire de recherche contenant les critères</param>
        /// <param name="tenant">Filtrer sur un tenant/cloisonnement (null = pas de filtre tenant)</param>
        /// <returns>Collection d'objets DataPO correspondant aux critères du formulaire</returns>
#pragma warning disable CS0618 // Type or member is obsolete
        public Task<CollectionPO<TPo>> SearchPOAsync<TPo>(DATA.BASICS.ISearchForm form, SECURITY.TENANTS.ITenant2 tenant = null)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().SearchPOAsync(form, tenant);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Met à jour (UPDATE) des objets en base de données (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à sauvegarder</typeparam>
        /// <param name="items">Tableau d'objets DataPO à sauvegarder</param>
        /// <param name="ForceEvenIfNotModified">true: Force la mise à jour de tous les champs même non modifiés</param>
        /// <returns>true si au moins un objet a été sauvegardé, false sinon</returns>
        public Task<bool> SavePOAsync<TPo>(TPo[] items, bool ForceEvenIfNotModified = false)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().SavePOAsync(items, ForceEvenIfNotModified);
        }

        /// <summary>
        /// Met à jour (UPDATE) un ou plusieurs objets en base de données (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à sauvegarder</typeparam>
        /// <param name="items">Objets DataPO à sauvegarder</param>
        /// <returns>true si les objets ont été sauvegardés</returns>
        public Task<bool> SavePOAsync<TPo>(params TPo[] items)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().SavePOAsync(items);
        }

        /// <summary>
        /// Met à jour plusieurs objets simultanément avec les mêmes valeurs (version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à mettre à jour</typeparam>
        /// <param name="items">Tableau d'objets DataPO à mettre à jour</param>
        /// <param name="valeursParameters">Dictionnaire des colonnes et valeurs à appliquer à tous les objets</param>
        public Task UpdatePOAsync<TPo>(TPo[] items, Dictionary<string, object> valeursParameters)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().UpdatePOAsync(items, valeursParameters);
        }

        /// <summary>
        /// Insère des objets DataPO en base de données (INSERT, version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à insérer</typeparam>
        /// <param name="items">Tableau d'objets DataPO à insérer</param>
        public Task InsertPOAsync<TPo>(params TPo[] items)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().InsertPOAsync(items);
        }

        /// <summary>
        /// Supprime des objets DataPO de la base de données (DELETE, version générique)
        /// </summary>
        /// <typeparam name="TPo">Type de DataPO à supprimer</typeparam>
        /// <param name="items">Objets DataPO à supprimer</param>
        public Task DeletePOAsync<TPo>(params TPo[] items)
            where TPo : DataPO, new()
        {
            return CreateGenericProviderForPO<TPo>().DeletePOAsync(items);
        }

        #endregion


        #region ------- Méthodes synchrones Model (NOT ASYNC) - OBSOLETE -------

        /// <summary>
        /// Recherche des modèles en base avec limitation (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public ListResult<TModel> QueryModel<TModel>(int count = 100, string listMode = null)
            where TModel : class, new()
            => QueryModelAsync<TModel>(count, listMode).GetAwaiter().GetResult();

        /// <summary>
        /// Obtient un modèle unique par son ID auto-incrémenté (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public TModel GetModel<TModel>(long id)
            where TModel : class, new()
            => GetModelAsync<TModel>(id).GetAwaiter().GetResult();

        /// <summary>
        /// Obtient un modèle unique par des paramètres de recherche (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public TModel GetModel<TModel>(Dictionary<string, object> keys)
            where TModel : class, new()
            => GetModelAsync<TModel>(keys).GetAwaiter().GetResult();

        /// <summary>
        /// Sauvegarde (UPDATE) un modèle existant (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public TModel SaveModel<TModel>(TModel model)
            where TModel : class, new()
            => SaveModelAsync<TModel>(model).GetAwaiter().GetResult();

        /// <summary>
        /// Insère un nouveau modèle en base (INSERT, version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public TModel InsertModel<TModel>(TModel model)
            where TModel : class, new()
            => InsertModelAsync<TModel>(model).GetAwaiter().GetResult();

        /// <summary>
        /// Supprime un modèle de la base (DELETE, version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public bool DeleteModel<TModel>(TModel model)
            where TModel : class, new()
            => DeleteModelAsync<TModel>(model).GetAwaiter().GetResult();

        #endregion


        #region ------- Méthodes synchrones DataPO (NOT ASYNC) - OBSOLETE -------

        /// <summary>
        /// Obtient un objet DataPO par son ID auto-incrémenté (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public TPo GetPO<TPo>(long idIncrement, int TenantId = 0)
            where TPo : DataPO, new()
            => GetPOAsync<TPo>(idIncrement, TenantId).GetAwaiter().GetResult();

        /// <summary>
        /// Obtient un objet DataPO par clé B36 (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public TPo GetPOByKey<TPo>(string fullB36Key)
            where TPo : DataPO, new()
            => GetPOByKeyAsync<TPo>(fullB36Key).GetAwaiter().GetResult();

        /// <summary>
        /// Obtient un objet DataPO par paramètres de recherche (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public TPo GetPO<TPo>(Dictionary<string, object> paramKeys)
            where TPo : DataPO, new()
            => GetPOAsync<TPo>(paramKeys).GetAwaiter().GetResult();

        /// <summary>
        /// Vérifie l'existence d'un enregistrement (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public bool Exist<TPo>(Dictionary<string, object> keys)
            where TPo : DataPO, new()
            => ExistAsync<TPo>(keys).GetAwaiter().GetResult();

        /// <summary>
        /// Recherche des objets DataPO avec collection personnalisée (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public TCollection QueryPO<TPo, TCollection>(QueryContext query)
            where TPo : DataPO, new()
            where TCollection : CollectionPO<TPo>, new()
            => QueryPOAsync<TPo, TCollection>(query).GetAwaiter().GetResult();

        /// <summary>
        /// Recherche des objets DataPO (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public CollectionPO<TPo> QueryPO<TPo>(QueryContext query)
            where TPo : DataPO, new()
            => QueryPOAsync<TPo>(query).GetAwaiter().GetResult();

        /// <summary>
        /// Recherche des objets avec limite et paramètres (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public CollectionPO<TPo> QueryPO<TPo>(int limitResult = 1000, Dictionary<string, object> SqlParams = null)
            where TPo : DataPO, new()
            => QueryPOAsync<TPo>(limitResult, SqlParams).GetAwaiter().GetResult();

        /// <summary>
        /// Recherche avec formulaire de recherche (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
#pragma warning disable CS0618 // Type or member is obsolete
        public CollectionPO<TPo> SearchPO<TPo>(DATA.BASICS.ISearchForm form, SECURITY.TENANTS.ITenant2 tenant = null)
            where TPo : DataPO, new()
            => SearchPOAsync<TPo>(form, tenant).GetAwaiter().GetResult();
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Sauvegarde (UPDATE) des objets DataPO (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public bool SavePO<TPo>(TPo[] items, bool ForceEvenIfNotModified = false)
            where TPo : DataPO, new()
            => SavePOAsync<TPo>(items, ForceEvenIfNotModified).GetAwaiter().GetResult();

        /// <summary>
        /// Sauvegarde (UPDATE) un ou plusieurs objets DataPO (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public bool SavePO<TPo>(params TPo[] items)
            where TPo : DataPO, new()
            => SavePOAsync<TPo>(items).GetAwaiter().GetResult();

        /// <summary>
        /// Met à jour plusieurs objets avec les mêmes valeurs (version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public void UpdatePO<TPo>(TPo[] items, Dictionary<string, object> valeursParameters)
            where TPo : DataPO, new()
            => UpdatePOAsync<TPo>(items, valeursParameters).GetAwaiter().GetResult();

        /// <summary>
        /// Insère des objets DataPO en base (INSERT, version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public void InsertPO<TPo>(params TPo[] items)
            where TPo : DataPO, new()
            => InsertPOAsync<TPo>(items).GetAwaiter().GetResult();

        /// <summary>
        /// Supprime des objets DataPO (DELETE, version synchrone)
        /// </summary>
        [Obsolete("Use Async method instead")]
        public void DeletePO<TPo>(params TPo[] items)
            where TPo : DataPO, new()
            => DeletePOAsync<TPo>(items).GetAwaiter().GetResult();

        #endregion
    }
}
