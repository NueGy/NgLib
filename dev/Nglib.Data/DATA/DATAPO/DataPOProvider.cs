using Nglib.DATA.ACCESSORS;
using Nglib.DATA.CONNECTOR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Nglib.DATA.COLLECTIONS;
using Nglib.SECURITY.IDENTITY;
using Nglib.SECURITY.TENANTS;
namespace Nglib.DATA.DATAPO
{

    /// <summary>
    /// Fournisseur CRUD simplifié pour DataPO générique.
    /// Utilise DataPO comme type de persistance par défaut sans typage fort.
    /// Pour un typage fort du DataPO, utiliser la version DataPOProviderCRUD&lt;TPo, TModel&gt;
    /// </summary>
    /// <typeparam name="TModel">Type de modèle métier exposé via API</typeparam>
    public class DataPOProvider<TModel> : DataPOProvider<DataPO, TModel>
        where TModel : class, new()
    {
        /// <summary>
        /// Initialise le fournisseur CRUD avec l'environnement global
        /// </summary>
        public DataPOProvider(APP.ENV.IGlobalEnv env) : base(env) { }
        
        /// <summary>
        /// Initialise le fournisseur CRUD avec un connecteur de données spécifique
        /// </summary>
        public DataPOProvider(CONNECTOR.IDataConnector connector) : base(connector) { }

 
         
    }


    /// <summary>
    /// Fournisseur CRUD typé pour la manipulation des modèles métier via DataPO.
    /// Permet la conversion bidirectionnelle entre objets de persistance (TPo) et modèles API (TModel).
    /// Implémente le pattern Repository avec mapping automatique et opérations CRUD complètes.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_datapo"/></para>
    /// </summary>
    /// <typeparam name="TPo">Type DataPO de persistance, doit hériter de DataPO</typeparam>
    /// <typeparam name="TModel">Type de modèle métier exposé via API</typeparam>
    public class DataPOProvider<TPo, TModel> : DataPOProviderSQL<TPo>
        where TPo : DATA.DATAPO.DataPO, new()
        where TModel : class, new()
    {

        /// <summary>
        /// Authorise le mapping automatique par réflexion dans MapToPO si aucune implémentation spécifique n'est fournie.
        /// Attention : Le mapping par réflexion peut être moins performant et plus risqué qu'une implémentation explicite.
        /// </summary>
        public bool DefaultMappingByReflexion { get; set; } = false;

        /// <summary>
        /// Initialise le fournisseur CRUD avec l'environnement global
        /// </summary>
        public DataPOProvider(APP.ENV.IGlobalEnv env) : base(env) { }
        
        /// <summary>
        /// Initialise le fournisseur CRUD avec un connecteur de données spécifique
        /// </summary>
        public DataPOProvider(CONNECTOR.IDataConnector connector) : base(connector) { }


        /// <summary>
        /// Convertit un objet DataPO vers un modèle métier (mapping PO → Model).
        /// IMPORTANT : Cette méthode doit être surchargée pour implémenter le mapping réel des propriétés.
        /// L'implémentation par défaut retourne un objet vide.
        /// </summary>
        /// <param name="item">Objet DataPO typé source provenant de la base de données</param>
        /// <returns>Modèle métier mappé ou null si item est null</returns>
        /// <example>
        /// <code>
        /// public override UserModel MapFromPO(UserPO item) 
        /// {
        ///     if (item == null) return null;
        ///     return new UserModel 
        ///     {
        ///         Id = item.Id,
        ///         Name = item.Name,
        ///         Email = item.Email
        ///     };
        /// }
        /// </code>
        /// </example>
        public virtual TModel MapFromPO(TPo item)
        {
            if (item == null) return default;
            TModel retour = new TModel();
            // À surcharger pour implémenter le mapping réel des propriétés
            item.ToReflectionProperties(retour);
            return retour;
        }

        /// <summary>
        /// Convertit un modèle métier vers un objet DataPO (mapping Model → PO).
        /// OBLIGATOIRE : Cette méthode DOIT être surchargée pour définir le mapping inverse.
        /// Utilisée lors des opérations d'insertion et de mise à jour.
        /// </summary>
        /// <param name="model">Modèle métier source contenant les données à persister</param>
        /// <param name="item">Objet DataPO de destination (peut être vide pour insertion ou existant pour mise à jour)</param>
        /// <returns>Objet DataPO mappé prêt à être sauvegardé</returns>
        /// <exception cref="NotImplementedException">Si la méthode n'est pas surchargée dans la classe dérivée</exception>
        /// <example>
        /// <code>
        /// public override UserPO MapToPO(UserModel model, UserPO item) 
        /// {
        ///     item.Name = model.Name;
        ///     item.Email = model.Email;
        ///     item.UpdatedAt = DateTime.UtcNow;
        ///     return item;
        /// }
        /// </code>
        /// </example>
        public virtual TPo MapToPO(TModel model, TPo item)
        {
            if (model == null) return null;
            // À surcharger pour implémenter le mapping réel des propriétés
            if (!DefaultMappingByReflexion)
                //throw new NotImplementedException($"MapToPO doit être surchargée dans {this.GetType().Name} pour définir comment mapper {typeof(TModel).Name} vers {typeof(TPo).Name}");

            if (item == null) item = new TPo(); // Crée un nouveau PO si null (utile pour insert)
            item.FromReflectionProperties(model);
            return item;
        }



        protected override void InitSchema()
        {
            Type potype = typeof(TPo);
            Type modeltype = typeof(TModel);
            
            try
            {
                // Tenter d'obtenir le schéma depuis le DataPO typé
                this.SchemaPo = DataPOSchemaTools.GetSchemaOnPO(potype);

                // Si le schéma n'est pas défini dans le DataPO alors on tente avec les attributs du Model
                if (this.SchemaPo ==null)
                    this.SchemaPo = DataPOSchemaTools.CreateSchemaWithAttributes(modeltype);

                //Validation finale pour les DataPO typés
                DataPOProviderTools.ValidateSchema(this.SchemaPo, potype);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"InitSchema('{typeof(TPo).Name}') Error: {ex.Message}. Si vous utilisez DataPO générique, ajoutez l'attribut [Table(\"nom_table\")] sur {typeof(TModel).Name}.");
            }
        }



        /// <summary>
        /// Recherche des modèles en base avec limitation du nombre de résultats.
        /// Convertit automatiquement les DataPO en modèles métier via MapFromPO.
        /// </summary>
        /// <param name="count">Nombre maximum de résultats (par défaut: 100)</param>
        /// <param name="listMode">Mode de listing optionnel (non utilisé actuellement, réservé pour évolutions futures)</param>
        /// <returns>Collection de modèles avec métadonnées (ListResult)</returns>
        public virtual async Task<ListResult<TModel>> QueryModelAsync(int count = 100, string listMode = null)
        {
            Dictionary<string, object> ins = new Dictionary<string, object>();
            var items = await this.QueryPOAsync(count, ins);
            ListResult<TModel> retour = new ListResult<TModel>();
            retour.data.AddRange(items.Select(item => MapFromPO(item)).ToList());
            return retour;
        }


        /// <summary>
        /// Obtient un modèle unique par son ID auto-incrémenté.
        /// Effectue une requête SQL avec filtre sur la clé primaire.
        /// </summary>
        /// <param name="id">ID auto-incrémenté de l'enregistrement (doit être > 0)</param>
        /// <returns>Le modèle correspondant ou null si non trouvé ou ID invalide</returns>
        public virtual async Task<TModel> GetModelAsync(long id)
        {
            try
            {
                if (id <= 0) return default(TModel);
                
                var po = await this.GetPOAsync(id);
                return MapFromPO(po);
            }
            catch (Exception ex)
            {
                throw new Exception($"GetModelAsync {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtient un modèle unique par des paramètres de recherche personnalisés.
        /// Construit une requête SQL WHERE avec égalité sur les colonnes spécifiées.
        /// </summary>
        /// <param name="keys">Dictionnaire des colonnes et valeurs pour la recherche (ex: {"email": "test@test.com", "tenantid": 1})</param>
        /// <returns>Le premier modèle correspondant aux critères ou null si non trouvé</returns>
        public virtual async Task<TModel> GetModelAsync(Dictionary<string, object> keys)
        {
            try
            {
                if (keys == null || keys.Count == 0) return default(TModel);
                
                var po = await this.GetPOAsync(keys);
                return MapFromPO(po);
            }
            catch (Exception ex)
            {
                throw new Exception($"GetModelAsync {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sauvegarde (met à jour) un modèle existant en base de données (opération UPDATE).
        /// Processus : Extraction des clés → Lecture du PO existant → Mapping → UPDATE.
        /// IMPORTANT : Le modèle doit contenir les clés primaires valides (voir GetModelKeys).
        /// </summary>
        /// <param name="model">Modèle contenant les données à mettre à jour (doit inclure les clés primaires)</param>
        /// <returns>Le modèle mis à jour avec les éventuelles valeurs recalculées par la base</returns>
        /// <exception cref="ArgumentNullException">Si model est null</exception>
        /// <exception cref="InvalidOperationException">Si les clés sont invalides ou si l'enregistrement n'existe pas</exception>
        public virtual async Task<TModel> SaveModelAsync(TModel model)
        {
            try
            {
                if (model == null) 
                    throw new ArgumentNullException(nameof(model));

                // Récupérer les clés du modèle pour identifier l'enregistrement
                var modelKeys = GetModelKeys(model);
                if (modelKeys == null || modelKeys.Count == 0)
                    throw new InvalidOperationException("Impossible d'obtenir les clés du modèle pour la mise à jour");

                // S'assurer que le schema est défini sur le DataPO (nécessaire pour les DataPO génériques)
                if (this.SchemaPo == null) this.InitSchema();

                // Récupérer le DataPO existant
                var existingPO = await this.GetPOAsync(modelKeys);
                if (existingPO == null)
                    throw new InvalidOperationException("Impossible de trouver l'enregistrement existant pour la mise à jour");

                // Mapper le modèle vers le DataPO existant
                var updatedPO = MapToPO(model, existingPO);
 
                // Sauvegarder en base
                bool saved = await this.SavePOAsync(updatedPO);
                if (!saved)
                    throw new InvalidOperationException("Échec de la sauvegarde en base");

                // Retourner le modèle mis à jour
                return MapFromPO(updatedPO);
            }
            catch (Exception ex)
            {
                throw new Exception($"SaveModelAsync {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Insère un nouveau modèle en base de données (opération INSERT).
        /// Processus : Création d'un nouveau PO → Mapping → INSERT → Retour avec ID généré.
        /// L'ID auto-incrémenté est automatiquement récupéré et disponible dans le modèle retourné.
        /// </summary>
        /// <param name="model">Modèle contenant les données à insérer (l'ID sera généré automatiquement)</param>
        /// <returns>Le modèle inséré avec son ID auto-généré par la base de données</returns>
        /// <exception cref="ArgumentNullException">Si model est null</exception>
        public virtual async Task<TModel> InsertModelAsync(TModel model)
        {
            try
            {
                if (model == null) 
                    throw new ArgumentNullException(nameof(model));

                //  S'assurer que le schéma est initialisé
                if (this.SchemaPo == null)
                    this.InitSchema();

                // Créer un nouveau DataPO avec le schéma correct
                var newPO = new TPo();
                newPO.DefineSchemaPO(this.SchemaPo); // On assigne le schéma avec toutes les primarykey propres, car le constructeur par défaut de TPo ne le fait pas forcément

                // Mapper le modèle vers le DataPO
                var mappedPO = MapToPO(model, newPO);
                
                // Insérer en base
                await this.InsertPOAsync(mappedPO);

                // Retourner le modèle avec l'ID généré
                return MapFromPO(mappedPO);
            }
            catch (Exception ex)
            {
                throw new Exception($"InsertModelAsync {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Supprime un modèle de la base de données (opération DELETE).
        /// Processus : Extraction des clés → Lecture du PO existant → DELETE.
        /// Si l'enregistrement n'existe pas, retourne false sans générer d'exception.
        /// </summary>
        /// <param name="model">Modèle à supprimer (doit contenir les clés primaires valides)</param>
        /// <returns>true si supprimé avec succès, false si déjà supprimé ou inexistant</returns>
        /// <exception cref="ArgumentNullException">Si model est null</exception>
        public virtual async Task<bool> DeleteModelAsync(TModel model)
        {
            try
            {
                if (model == null) 
                    throw new ArgumentNullException(nameof(model));

                // Récupérer les clés du modèle pour identifier l'enregistrement
                var modelKeys = GetModelKeys(model);
                if (modelKeys == null || modelKeys.Count == 0)
                    return false; // Pas de clés valides


                // Récupérer le DataPO correspondant
                var existingPO = await this.GetPOAsync(modelKeys);
                if (existingPO == null)
                    return false; // Déjà supprimé ou inexistant

                // S'assurer que le schema est défini sur le DataPO (nécessaire pour les DataPO génériques)
                if (this.SchemaPo == null) this.InitSchema();

                // TODO Optimiser : Utiliser une suppression directe par clés si possible
                // Supprimer de la base
                await this.DeletePOAsync(existingPO);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"DeleteModelAsync {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extrait les clés primaires d'un modèle pour construire les requêtes SQL de recherche/mise à jour/suppression.
        /// Le dictionnaire retourné est utilisé dans les clauses WHERE des requêtes SQL (ex: WHERE id=@id AND tenantid=@tenantid).
        /// </summary>
        /// <param name="model">Modèle pour lequel extraire les clés primaires</param>
        /// <returns>Dictionnaire nom_colonne → valeur pour identifier l'enregistrement en base</returns>
        /// <exception cref="NotImplementedException">Si la méthode n'est pas surchargée dans la classe dérivée</exception>
        protected virtual Dictionary<string, object> GetModelKeys(TModel model)
        {
            if (model == null) return null;

            if (this.SchemaPo == null)
                this.InitSchema();

            // Obtenir les colonnes de clés primaires depuis le schéma
            var primaryKeys = this.SchemaPo?.PrimaryKey;
            if (primaryKeys == null || primaryKeys.Length == 0)
                return new Dictionary<string, object>(); ; // Pas de clés primaires définies

            // Créer un dictionnaire avec les valeurs des clés depuis le Model
            var keys = new Dictionary<string, object>();
            foreach (var pkCol in primaryKeys)
            {
                // Trouver la propriété correspondante dans le Model (insensible à la casse)
                var prop = typeof(TModel).GetProperty(pkCol.ColumnName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase);


                var value = prop?.GetValue(model);
                keys.Add(pkCol.ColumnName, value);

            }

            return keys ;

            // Cette méthode doit être surchargée dans les classes dérivées
            // car elle dépend de la structure du modèle et de ses clés primaires
            //throw new NotImplementedException($"GetModelKeys doit être surchargée dans {this.GetType().Name} pour définir comment extraire les clés de {typeof(TModel).Name}");
        }








    }
}
