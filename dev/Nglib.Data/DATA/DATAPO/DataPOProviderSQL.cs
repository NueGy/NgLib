using Nglib.DATA.ACCESSORS;
using Nglib.DATA.CONNECTOR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nglib.DATA.COLLECTIONS;
using Nglib.DATA.BASICS;
using Nglib.SECURITY.TENANTS;
using Nglib.DATA.CONNECTOR.QUERYBUILDER;

namespace Nglib.DATA.DATAPO
{

    /// <summary>
    /// Manipulation des dataPo en base
    /// </summary>
    /// <typeparam name="TPo">DataPO</typeparam>
    public class DataPOProviderSQL<TPo> : IDataPOProvider 
                where TPo : DATAPO.DataPO, new()
    {

        //todo : ajout helper/filter custom,  tenantid dans les requetes...



        /// <summary>
        /// Désactive AcceptChange
        /// </summary>
        public bool DisableAcceptChange { get; set; } = false;

        /// <summary>
        /// Connecteur principal qui sera utilisé pour ce provider
        /// </summary>
        public DATA.CONNECTOR.IDataConnector Connector { get; protected set; }

        /// <summary>
        /// Timeout d'insertion par défaut
        /// </summary>
        public int InsertPODefaultTimeOut { get; set; } = 600;


        /// <summary>
        /// Schéma de l'objet DataPO (DataTable)
        /// </summary>
        public System.Data.DataTable SchemaPo { get; protected set; }



        /// <summary>
        /// Des paramètres par défauts qui seront ajoutés à chaque requête de lecture
        /// </summary>
        [Obsolete("SOON")]
        protected Dictionary<string, object> defaultParameters = null;


        /// <summary>
        /// Objet qui permet de gérer la sécurité et obtenir les clefs de cryptages
        /// </summary>
        protected IDataPOSecurityFilter securityFilter { get;  set; }

        /// <summary>
        /// Configure le filtre de sécurité pour ce provider
        /// </summary>
        /// <param name="filter">Filtre de sécurité à utiliser</param>
        public virtual void SetSecurityFilter(IDataPOSecurityFilter filter)
        {
            this.securityFilter = filter;
        }


        /// <summary>
        /// manipulation des dataPo en base
        /// </summary>
        [Obsolete("use constructor with param")]
        public DataPOProviderSQL()
        {
  
        }


        /// <summary>
        /// manipulation des dataPo en base
        /// </summary>
        public DataPOProviderSQL(DATA.CONNECTOR.IDataConnector connector)
        {
            this.Connector = connector;
            if (this.Connector == null) throw new Exception("provider.connector Not loaded");
        }

        /// <summary>
        /// manipulation des dataPo en base
        /// </summary>
        public DataPOProviderSQL(APP.ENV.IMasterEnv env)
        {
            this.Connector = env?.ConnectorDatabase;
            if (this.Connector == null) throw new Exception("provider.connector Not loaded");
        }



        #region ------- Outils SQL -------


        protected virtual void InitSchema()
        {
            Type potype = null;
            try
            {
                potype = typeof(TPo);
                this.SchemaPo = DataPOSchemaTools.GetSchemaOnPO(potype);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"InitSchema('{typeof(TPo).Name}') Error: "+ex.Message);
            }

            DataPOProviderTools.ValidateSchema(this.SchemaPo, potype);
        }



        /// <summary>
        /// Obtient le nom de la table SQL de l'objet DataPO avec validation
        /// </summary>
        /// <returns>Le nom de la table SQL</returns>
        /// <exception cref="InvalidOperationException">Si le schéma n'est pas défini ou invalide</exception>
        protected string GetTableName()
        {
            if (this.SchemaPo == null) this.InitSchema();
            return this.SchemaPo?.TableName;
        }
           
        

        /// <summary>
        /// Exécute une requête SQL sur le connecteur configuré
        /// </summary>
        /// <param name="query">Contexte de la requête contenant le SQL et les paramètres</param>
        /// <returns>La DataTable résultante</returns>
        /// <exception cref="ArgumentNullException">Si query est null</exception>
        /// <exception cref="InvalidOperationException">Si aucun connecteur n'est configuré ou si aucune table n'est retournée</exception>
        protected async Task<System.Data.DataTable> ExecuteQueryAsync(QueryContext query)
        {
            DataPOProviderTools.ValidateQuery(query, "ExecuteQueryAsync");
            DataPOProviderTools.ValidateConnector(this.Connector, typeof(TPo));
            
            var dataserresult = await this.Connector.QueryDataSetAsync(query);
            
            if (dataserresult.Tables.Count == 0)
                throw new InvalidOperationException($"ExecuteQueryAsync: Aucune table retournée par la requête SQL. Query: {query.SqlQuery}");
            
            return dataserresult.Tables[0];
        }


        /// <summary>
        /// Extrait les clés primaires d'un modèle pour construire les requêtes SQL de recherche/mise à jour/suppression.
        /// Le dictionnaire retourné est utilisé dans les clauses WHERE des requêtes SQL (ex: WHERE id=@id AND tenantid=@tenantid).
        /// </summary>
        /// <param name="item">DataPO pour lequel extraire les clés primaires</param>
        /// <returns>Dictionnaire nom_colonne → valeur pour identifier l'enregistrement en base</returns>
        protected virtual Dictionary<string, object> GetPOKeys(TPo item)
        {
            if (item == null) return null;

            if (this.SchemaPo == null)
                this.InitSchema();

            // Obtenir les colonnes de clés primaires depuis le schéma
            var primaryKeys = this.SchemaPo?.PrimaryKey;
            if (primaryKeys == null || primaryKeys.Length == 0)
                return new Dictionary<string, object>(); // Pas de clés primaires définies

            // Créer un dictionnaire avec les valeurs des clés depuis le Model
            var keys = new Dictionary<string, object>();
            foreach (var pkCol in primaryKeys)
            {
                var value = item.GetData(pkCol.ColumnName, DataAccessorOptionEnum.None); // Récupérer la valeur de la clé, GetData directement sur le PO (et non GetObject qui transforme les types)
                keys.Add(pkCol.ColumnName, value);
            }

            return  keys;

            // Cette méthode doit être surchargée dans les classes dérivées
            // car elle dépend de la structure du modèle et de ses clés primaires
            //throw new NotImplementedException($"GetModelKeys doit être surchargée dans {this.GetType().Name} pour définir comment extraire les clés de {typeof(TModel).Name}");
        }



        #endregion






        #region ------- Lectures SQL -------

        /// <summary>
        /// Obtient un objet DataPO par son ID auto-incrémenté
        /// </summary>
        /// <param name="idIncrement">L'ID auto-incrémenté de l'enregistrement</param>
        /// <param name="TenantId">ID du tenant pour le filtrage multi-tenant (0 = pas de filtre)</param>
        /// <returns>L'objet DataPO correspondant ou null si non trouvé</returns>
        /// <exception cref="InvalidOperationException">Si le schéma n'est pas défini ou si aucune colonne auto-incrémentée n'existe</exception>
        public async Task<TPo> GetPOAsync(long idIncrement, int TenantId = 0)
        {
            Type potype = typeof(TPo);
            if(this.SchemaPo==null) this.InitSchema();

            string fieldkeyName = DataPOProviderTools.GetAutoIncrementColumnName(this.SchemaPo);
            if (string.IsNullOrWhiteSpace(fieldkeyName))
                throw new InvalidOperationException($"GetPOByIdAsync: Aucune colonne auto-incrémentée trouvée dans le schéma du type '{potype.Name}'.");
            
            Dictionary<string, object> paramKeys = new Dictionary<string, object>();
            paramKeys.Add("p1", idIncrement);
            string sql = $"SELECT * FROM {this.SchemaPo.TableName} WHERE {fieldkeyName}=@p1";
            
            if (TenantId > 0) 
            { 
                paramKeys.Add("p2", TenantId); 
                sql += " AND tenantid=@p2"; 
            }
            
            Nglib.DATA.CONNECTOR.QueryContext query = new Nglib.DATA.CONNECTOR.QueryContext(sql, paramKeys);
            return (await this.QueryPOAsync(query)).FirstOrDefault();
        }

        /// <summary>
        /// Obtient un objet DataPO par des paramètres de recherche personnalisés
        /// </summary>
        /// <param name="paramKeys">Dictionnaire des colonnes et valeurs à rechercher (ex: {"email": "test@test.com"})</param>
        /// <returns>Le premier objet DataPO correspondant ou null si non trouvé</returns>
        /// <exception cref="InvalidOperationException">Si le schéma n'est pas défini</exception>
        public async Task<TPo> GetPOAsync(Dictionary<string, object> paramKeys)
        {
            if (this.SchemaPo == null) this.InitSchema();

            string wheresql = string.Empty;
            if (paramKeys != null && paramKeys.Count > 0)
            {
                List<string> wheres = new List<string>();
                paramKeys.Keys.ToList().ForEach(k => wheres.Add(string.Format("{0}=@{0}", k)));
                wheresql = " WHERE " + string.Join(" AND ", wheres.ToArray());
            }
            
            Nglib.DATA.CONNECTOR.QueryContext query = new Nglib.DATA.CONNECTOR.QueryContext(
                string.Format("SELECT * FROM {0} {1};", this.SchemaPo.TableName, wheresql), 
                paramKeys);

            return (await this.QueryPOAsync(query)).FirstOrDefault();
        }

        /// <summary>
        /// Obtient un objet DataPO en utilisant la clé B36 (Format Nglib)
        /// </summary>
        /// <param name="fullB36Key">Clé au format B36 contenant ItemId, TenantId, etc.</param>
        /// <returns>L'objet DataPO correspondant ou null si non trouvé ou clé invalide</returns>
        /// <exception cref="InvalidOperationException">Si le schéma n'est pas défini ou si aucune colonne auto-incrémentée n'existe</exception>
        /// <remarks>Note: Il faut avoir préalablement vérifié le TenantId pour la sécurité</remarks>
        public async virtual Task<TPo> GetPOByKeyAsync(string fullB36Key)
        {
            var key = FORMAT.KeyTools.ParseKeyB36(fullB36Key);
            if (!key.IsValid) return default(TPo);

            if (this.SchemaPo == null) this.InitSchema();

            string fieldkeyName = DataPOProviderTools.GetAutoIncrementColumnName(this.SchemaPo);
            if (string.IsNullOrWhiteSpace(fieldkeyName))
                throw new InvalidOperationException($"GetPOByKeyAsync: Aucune colonne auto-incrémentée trouvée dans le schéma.");

            Dictionary<string, object> paramKeys = new Dictionary<string, object>();
            paramKeys.Add("p1", key.ItemId);
            string sql = $"SELECT * FROM {this.SchemaPo.TableName} WHERE {fieldkeyName}=@p1";

            if (key.TenantId > 0)
            {
                paramKeys.Add("p2", key.TenantId);
                sql += " AND tenantid=@p2";
            }
            
            Nglib.DATA.CONNECTOR.QueryContext query = new Nglib.DATA.CONNECTOR.QueryContext(sql, paramKeys);
            return (await this.QueryPOAsync(query)).FirstOrDefault();
        }

        /// <summary>
        /// Vérifie l'existence d'un enregistrement en base via ses clés.
        /// Effectue une requête COUNT optimisée au lieu de charger l'objet complet.
        /// Utilisé pour valider l'existence avant insertion ou pour les contrôles de sécurité.
        /// </summary>
        /// <param name="keys">Dictionnaire des colonnes et valeurs pour identifier l'enregistrement (ex: {"id": 123, "tenantid": 1})</param>
        /// <returns>true si l'enregistrement existe, false sinon</returns>
        /// <exception cref="ArgumentNullException">Si keys est null ou vide</exception>
        /// <exception cref="InvalidOperationException">Si le schéma n'est pas défini</exception>
        public virtual async Task<bool> ExistAsync(Dictionary<string, object> keys)
        {
            try
            {
                if (keys == null || keys.Count == 0) return false;

                if (this.SchemaPo == null) this.InitSchema();

                // Construction de la clause WHERE
                List<string> wheres = new List<string>();
                keys.Keys.ToList().ForEach(k => wheres.Add(string.Format("{0}=@{0}", k)));
                string wheresql = " WHERE " + string.Join(" AND ", wheres.ToArray());

                // Requête COUNT optimisée (ne charge pas les données)
                string sql = string.Format("SELECT COUNT(*) FROM {0} {1};", this.SchemaPo.TableName, wheresql);
                Nglib.DATA.CONNECTOR.QueryContext query = new Nglib.DATA.CONNECTOR.QueryContext(sql, keys);

                // Note pas besoin de filtre de sécurité sur les exists.

                // Exécution de la requête
                var dataTable = await ExecuteQueryAsync(query);
                
                if (dataTable.Rows.Count == 0)
                    return false;

                // Récupération du COUNT
                long count = Convert.ToInt64(dataTable.Rows[0][0]);
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"ExistAsync {ex.Message}", ex);
            }
        }





        /// <summary>
        /// Recherche des objets depuis une requête SQL et retourne une collection personnalisée
        /// Méthode principale pour l'exécution des requêtes SQL
        /// </summary>
        /// <typeparam name="TCollection">Type de collection spécifique héritant de CollectionPO</typeparam>
        /// <param name="query">Contexte de requête contenant le SQL et les paramètres</param>
        /// <returns>Collection personnalisée d'objets DataPO</returns>
        /// <exception cref="ArgumentNullException">Si query est null</exception>
        /// <exception cref="InvalidOperationException">Si le schéma n'est pas défini ou si le connecteur n'est pas configuré</exception>
        public async Task<TCollection> QueryPOAsync<TCollection>(Nglib.DATA.CONNECTOR.QueryContext query)
            where TCollection : CollectionPO<TPo>, new()
        {

            DataPOProviderTools.ValidateQuery(query, "QueryPOAsync");

            // Appliquer le filtre de sécurité avant l'exécution
            if (this.securityFilter != null)
                if (!this.securityFilter.ApplyQueryFilter(query))
                    return new TCollection();   // Query bloquée par le filtre de sécurité, 


            // On obtient la table du PO
            string TableDefault = this.GetTableName();
             

            // Finir la Construction de la requette
            try
            {
                DataPOProviderTools.CompleteSqlQuery(query, TableDefault);
                this.OnBeforeRead(query);

                System.Data.DataTable sqlResult = await this.ExecuteQueryAsync(query);
                if (sqlResult == null) return new TCollection();

                // Assigner le nom de la table mais pas les clés primaires pour permettre des requetes libre multi-tables
                sqlResult.TableName = TableDefault;

                //Mapping 
                TCollection retour = new TCollection();
                retour.LoadFromDataTable(sqlResult);

                // Appliquer le contexte crypto et vérifier les permissions de lecture
                if (this.securityFilter != null)
                    this.ApplySecurityPostLoad(retour);
                
                retour.ExecuteTimeElapsed = query.watchAll.ElapsedMilliseconds;
                return retour;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(DataPOProviderTools.FormatError("QueryPOAsync", $"Erreur lors de l'exécution de la requête SQL", ex), ex);
            }
        }



        /// <summary>
        /// Recherche des objets depuis une requête SQL et retourne une collection standard
        /// </summary>
        /// <param name="query">Contexte de requête contenant le SQL et les paramètres</param>
        /// <returns>Collection d'objets DataPO</returns>
        public async Task<CollectionPO<TPo>> QueryPOAsync(Nglib.DATA.CONNECTOR.QueryContext query)
            => await this.QueryPOAsync<CollectionPO<TPo>>(query);





        /// <summary>
        /// Recherche des objets avec limite et paramètres optionnels
        /// </summary>
        /// <param name="limitResult">Nombre maximum de résultats à retourner</param>
        /// <param name="SqlParams">Paramètres de filtrage (WHERE columnName = value)</param>
        /// <returns>Collection d'objets DataPO correspondant aux critères</returns>
        /// <exception cref="InvalidOperationException">Si le schéma n'est pas défini</exception>
        public virtual async Task<CollectionPO<TPo>> QueryPOAsync(int limitResult = 1000, Dictionary<string, object> SqlParams = null)
        {

            // Obtenir la table par défaut
            string TableDefault = this.GetTableName();

            // Construction de la requette et retour 
            var engine = ConnectorTools.ParseEngineName(this.Connector?.EngineName);
            var sqlBuilder = this.Connector.CreateQueryBuilder().From(TableDefault);
            if (SqlParams != null) SqlParams.ForEach(kvp => sqlBuilder.WhereEqual(kvp.Key, kvp.Value));
            sqlBuilder.Limit(limitResult);
            string SqlQuery = sqlBuilder.ToString();
            Dictionary<string, object> paramKeySearch = sqlBuilder.GetParameters();
            Nglib.DATA.CONNECTOR.QueryContext query = new Nglib.DATA.CONNECTOR.QueryContext(SqlQuery, paramKeySearch);

            return await this.QueryPOAsync(query);
        }


        /// <summary>
        /// Recherche des objets en base et retourne la collection
        /// Utilise un formulaire de recherche pour construire la requête SQL
        /// </summary>
        /// <param name="form">Formulaire de recherche contenant les critères</param>
        /// <param name="tenant">Filtrer sur un tenant/cloisonnement (null = pas de filtre tenant)</param>
        /// <returns>Collection d'objets DataPO correspondant aux critères du formulaire</returns>
        /// <exception cref="ArgumentNullException">Si form est null</exception>
        /// <exception cref="InvalidOperationException">Si le schéma n'est pas défini</exception>
        public virtual async Task<CollectionPO<TPo>> SearchPOAsync(DATA.BASICS.ISearchForm form, ITenant2 tenant = null)
        {
            if (form == null) 
                throw new ArgumentNullException(nameof(form), "SearchPOAsync: Le formulaire de recherche ne peut pas être null.");

            // Obtenir la table par défaut
            string TableDefault = this.GetTableName();

            // Construction de la requette et retour 
            var enginetype = ConnectorTools.ParseEngineName(this.Connector?.EngineName);
            var sqlBuilder = this.Connector.CreateQueryBuilder().From(TableDefault);
            if (tenant != null) sqlBuilder.WhereEqual("tenantid", tenant.TenantId);
            sqlBuilder.ApplySearchForm(form);
            string SqlQuery = sqlBuilder.ToString();
            Dictionary<string, object> paramKeySearch = sqlBuilder.GetParameters();
            Nglib.DATA.CONNECTOR.QueryContext query = new Nglib.DATA.CONNECTOR.QueryContext(SqlQuery, paramKeySearch);

            return await this.QueryPOAsync(query);
        }



        /// <summary>
        /// Lancé avant chaque lecture de données (surchargeable)
        /// </summary>
        protected virtual void OnBeforeRead(Nglib.DATA.CONNECTOR.QueryContext query) { }

        /// <summary>
        /// Applique la sécurité post-chargement : injection du contexte crypto et vérification des permissions de lecture
        /// </summary>
        /// <param name="collection">Collection d'objets chargés depuis la base</param>
        protected virtual void ApplySecurityPostLoad<TCollection>(TCollection collection) 
            where TCollection : CollectionPO<TPo>
        {
            if (this.securityFilter == null || collection == null || collection.Count == 0) return;

            // Filtrer les objets non autorisés en lecture et injecter le contexte crypto
            var filteredItems = new List<TPo>();
            
            foreach (var item in collection)
            {
                // Vérifier les permissions de lecture
                if (this.securityFilter.IsAllowedRead(item))
                {
                    // Injecter le contexte crypto pour décryptage transparent
                    var cryptoContext = this.securityFilter.GetCryptoContext(item);
                    if (cryptoContext != null)
                    {
                        item.SetCryptoOptions(cryptoContext);
                    }
                    filteredItems.Add(item);
                }
            }

            // Remplacer la collection par les éléments autorisés
            collection.Clear();
            collection.AddRange(filteredItems);
        }



        #endregion





        #region ------- Ecritures SQL -------

        /// <summary>
        /// Met à jour (UPDATE) des objets en base de données
        /// </summary>
        /// <param name="items">Tableau d'objets DataPO à sauvegarder</param>
        /// <param name="ForceEvenIfNotModified">true: Force la mise à jour de tous les champs même non modifiés</param>
        /// <returns>true si au moins un objet a été sauvegardé, false sinon</returns>
        /// <exception cref="ArgumentNullException">Si bubbles est null ou vide</exception>
        /// <exception cref="InvalidOperationException">Si aucun connecteur configuré ou si les types sont différents</exception>
        /// <exception cref="UnauthorizedAccessException">Si les permissions d'écriture sont refusées</exception>
        public async Task<bool> SavePOAsync(TPo[] items, bool ForceEvenIfNotModified = false)
        {
            if (items == null || items.Count() == 0) return false;
            
            // Vérifier les permissions d'écriture
            if (this.securityFilter != null && !this.securityFilter.IsAllowedWrite(items))
                throw new UnauthorizedAccessException("SavePOAsync: Permissions d'écriture refusées par le filtre de sécurité");

            DataPOProviderTools.ValidateConnector(this.Connector, typeof(TPo));
            
            List<TPo> bubblesNeedToSave;
            try
            {
                // Vérifier qu'il s'agit toujours du même type d'objets
                DataPOProviderTools.ValidateSameType(items, "SavePOAsync");
                
                var firstItem = items.FirstOrDefault();
                var keys = this.GetPOKeys(firstItem);
                DataPOProviderTools.ValidatePrimaryKeys(keys, firstItem.GetType(), "SavePOAsync");
                this.OnBeforeWrite(items,"SAVE");

                // Detecter et préparer les objets qui nécessitent une modification
                if (ForceEvenIfNotModified) 
                    bubblesNeedToSave = items.ToList();
                else 
                    bubblesNeedToSave = items.Where(bubble => bubble.IsChanges()).ToList();
                
                if (bubblesNeedToSave.Count == 0) return false;

                await this.SavePo_LineByLinesAsync(bubblesNeedToSave, ForceEvenIfNotModified);

                return true;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(DataPOProviderTools.FormatError("SavePOAsync", "Erreur lors de la sauvegarde des objets", e), e);
            }
        }

        /// <summary>
        /// Met à jour (UPDATE) un objet en base de données
        /// </summary>
        /// <param name="bubble">Objet DataPO à sauvegarder</param>
        /// <param name="ForceEvenIfNotModified">false: Ne mettra à jour que si modifié, true: Force la mise à jour</param>
        /// <returns>true si l'objet a été sauvegardé</returns>
        public async Task<bool> SavePOAsync(params TPo[] items) 
            => await this.SavePOAsync(items, false); 
        


        

        /// <summary>
        /// Exécute plusieurs UPDATE en une transaction (méthode privée optimisée)
        /// </summary>
        private async Task SavePo_LineByLinesAsync(List<TPo> bubblesNeedToSave, bool ForceEvenIfNotModified)
        {
            bool openedtransact = false;
            try
            {
                if (bubblesNeedToSave.Count > 1) // si nécessaire d'ouvrir une transaction pour optimiser les performances
                    openedtransact = this.Connector.BeginTransaction(); //this.Connector.Open(true); 
                foreach (var bubble in bubblesNeedToSave)
                {
                    Dictionary<string, object> vals = ForceEvenIfNotModified ? bubble.GetValues() : bubble.GetChangedValues();
                    if (vals.Count == 0 && ForceEvenIfNotModified) 
                        throw new InvalidOperationException($"SavePo_LineByLinesAsync: Aucune valeur à mettre à jour pour {bubble.GetType().Name}.");
                    else if (vals.Count == 0) 
                        continue;

                    Dictionary<string, object> keys = this.GetPOKeys(bubble);
                    DataPOProviderTools.ValidatePrimaryKeys(keys, bubble.GetType(), "SavePo_LineByLinesAsync");
                    // Suppression des clés des valeurs à mettre à jour en mode ignorecase
                    foreach (var k in keys.Keys)
                    {
                        var keyInVals = vals.Keys.FirstOrDefault(vk => vk.Equals(k, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrWhiteSpace(keyInVals)) vals.Remove(keyInVals);
                    }

                    System.Data.DataRow rowb = bubble.GetRow();
                    string tablename = this.GetTableName();
                    await this.Connector.UpdateAsync(tablename, keys, vals);

                    if (!DisableAcceptChange)
                        bubble.AcceptChanges();
                }
                if (openedtransact) this.Connector.CommitTransaction();
            }
            catch (Exception)
            {
                if (openedtransact) this.Connector.RollBackTransaction();
                throw;
            }
            finally
            {
                if(openedtransact)this.Connector.Close();
            }
        }

        /// <summary>
        /// Met à jour plusieurs objets simultanément avec les mêmes valeurs
        /// </summary>
        /// <param name="items">Tableau d'objets DataPO à mettre à jour</param>
        /// <param name="valeursParameters">Dictionnaire des colonnes et valeurs à appliquer à tous les objets</param>
        /// <exception cref="ArgumentNullException">Si bubbles ou valeursParameters est null</exception>
        /// <exception cref="InvalidOperationException">Si aucun connecteur configuré ou si les types sont différents</exception>
        /// <exception cref="UnauthorizedAccessException">Si les permissions d'écriture sont refusées</exception>
        public async Task UpdatePOAsync(TPo[] items, Dictionary<string, object> valeursParameters)
        {
            // Vérifier les permissions d'écriture
            if (this.securityFilter != null && !this.securityFilter.IsAllowedWrite(items))
                throw new UnauthorizedAccessException("UpdatePOAsync: Permissions d'écriture refusées par le filtre de sécurité");
            
            try
            {
                DataPOProviderTools.ValidateConnector(this.Connector, typeof(TPo));
                DataPOProviderTools.ValidateSameType(items, "UpdatePOAsync");
                this.OnBeforeWrite(items,"UPDATE");

                foreach (TPo item in items)
                {
                    DataPOProviderTools.ValidateNotNull(item, nameof(item), "UpdatePOAsync");
                    
                    System.Data.DataRow rowb = item.GetRow();
                    Dictionary<string, object> keys = this.GetPOKeys(item);
                    DataPOProviderTools.ValidatePrimaryKeys(keys, item.GetType(), "UpdatePOAsync");
                    
                    await this.Connector.UpdateAsync(this.GetTableName(), keys, valeursParameters);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(DataPOProviderTools.FormatError("UpdatePOAsync", "Erreur lors de la mise à jour des objets", e), e);
            }
        }

        /// <summary>
        /// Met à jour des valeurs sur un objet
        /// </summary>
        public async Task UpdatePOAsync(TPo item, Dictionary<string, object> valeursParameters)
            => await this.UpdatePOAsync(new TPo[] { item }, valeursParameters);

        /// <summary>
        /// Insère des objets DataPO en base de données (INSERT)
        /// </summary>
        /// <param name="items">Tableau d'objets DataPO à insérer</param>
        /// <exception cref="ArgumentNullException">Si bubbles est null ou vide</exception>
        /// <exception cref="InvalidOperationException">Si aucun connecteur configuré, si les types sont différents, ou erreur d'insertion</exception>
        /// <exception cref="UnauthorizedAccessException">Si les permissions d'écriture sont refusées</exception>
        public async Task InsertPOAsync(params TPo[] items)
        {
            if (items == null || items.Length == 0) return;
            
            // Vérifier les permissions d'écriture
            if (this.securityFilter != null && !this.securityFilter.IsAllowedWrite(items))
            {
                throw new UnauthorizedAccessException("InsertPOAsync: Permissions d'écriture refusées par le filtre de sécurité");
            }
            
            try
            {
                DataPOProviderTools.ValidateConnector(this.Connector, typeof(TPo));
                DataPOProviderTools.ValidateSameType(items, "InsertPOAsync");
                this.OnBeforeWrite(items,"INSERT");

                System.Data.DataTable tabinsert = DataPOTools.CloneDataTable(items);

                // Corriger le nom de table si nécessaire
                if(this.SchemaPo==null) this.InitSchema();
                tabinsert.TableName = this.GetTableName();

                // Obtenir et retirer les colonnes auto-incrémentées
                List<System.Data.DataColumn> autoincrementedColumns = this.SchemaPo.GetColumns()
                    .Where(c => c.AutoIncrement)
                    .ToList();
                    
                autoincrementedColumns.ForEach(col => 
                {
                    if (tabinsert.Columns.Contains(col.ColumnName)) 
                        tabinsert.Columns.Remove(col.ColumnName);
                });

                string colautoincrement = autoincrementedColumns.Count() > 0 
                    ? autoincrementedColumns.FirstOrDefault().ColumnName 
                    : null;

                // --- INSERT ---
                List<long> valsincrement = await this.Connector.InsertTableAsync(
                    tabinsert, 
                    InsertPODefaultTimeOut, 
                    colautoincrement);

                // Réaffecter les IDs auto-incrémentés aux objets
                if (!string.IsNullOrWhiteSpace(colautoincrement))
                {
                    if (valsincrement.Count != items.Count()) 
                        throw new InvalidOperationException($"InsertPOAsync: Erreur d'incrémentation. {valsincrement.Count} IDs retournés pour {items.Count()} objets.");
                    
                    for (int i = 0; i < valsincrement.Count; i++)
                        items[i].SetObject(colautoincrement, valsincrement[i]);
                }

                // Déclarer les objets insérés avec AcceptChanges
                foreach (DataPO item in items)
                {
                    try
                    {
                        System.Data.DataRow rowb = item.GetRow();
                        if (rowb.RowState == System.Data.DataRowState.Detached)
                            rowb.Table.Rows.Add(rowb);
                        if (!DisableAcceptChange)
                            rowb.AcceptChanges();
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(DataPOProviderTools.FormatError("InsertPOAsync", "Erreur lors de l'insertion des objets", e), e);
            }
        }





        /// <summary>
        /// Supprime des objets DataPO de la base de données (DELETE)
        /// </summary>
        /// <param name="items">Objets DataPO à supprimer</param>
        /// <exception cref="ArgumentNullException">Si un objet est null</exception>
        /// <exception cref="InvalidOperationException">Si aucun connecteur configuré ou clé primaire manquante</exception>
        /// <exception cref="UnauthorizedAccessException">Si les permissions d'écriture sont refusées</exception>
        public async Task DeletePOAsync(params TPo[] items)
        {
            // Vérifier les permissions d'écriture
            if (this.securityFilter != null && !this.securityFilter.IsAllowedWrite(items))
            {
                throw new UnauthorizedAccessException("DeletePOAsync: Permissions d'écriture refusées par le filtre de sécurité");
            }
            
            try
            {
                DataPOProviderTools.ValidateConnector(this.Connector, typeof(TPo));
                this.OnBeforeWrite(items, "DELETE");

                foreach (var item in items)
                {
                    DataPOProviderTools.ValidateNotNull(item, nameof(item), "DeletePOAsync");
                    
                    Dictionary<string, object> keys = this.GetPOKeys(item);
                    DataPOProviderTools.ValidatePrimaryKeys(keys, item.GetType(), "DeletePOAsync");
                    
                    System.Data.DataRow rowb = item.GetRow();
                    await this.Connector.DeleteAsync(this.GetTableName(), keys);
                    
                    try { rowb.Delete(); }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(DataPOProviderTools.FormatError("DeletePOAsync", "Erreur lors de la suppression des objets", e), e);
            }
        }


        /// <summary>
        /// Lancé avant chaque lecture de données (surchargeable)
        /// </summary>
        protected virtual void OnBeforeWrite(TPo[] items, string mode) { }



        #endregion







        #region ==== Obsolete Methods  ====

        /// <summary>
        /// Obtenir un provider SQL générique (non typé)
        /// </summary>
        //[Obsolete("RetroCompatibility")]
        public DataPOGenericProvider GetGenericProvider()
        {
            return new DataPOGenericProvider(this.Connector);
        }




        #endregion



    }
}
