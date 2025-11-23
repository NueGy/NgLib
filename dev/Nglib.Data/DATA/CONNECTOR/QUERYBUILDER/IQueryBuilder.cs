using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// Pour la construction de requêtes SQL multi-SGBD (Fluent API)
    /// </summary>
    public interface IQueryBuilder
    {

        /// <summary>
        /// Nom du moteur de base de données
        /// </summary>
        string EngineName { get; }

        /// <summary>
        /// Nom de la table principale
        /// </summary>
        string TableName { get; }



        /// <summary>
        /// Options de configuration du QueryBuilder
        /// </summary>
        QueryBuilderOptionModel Options { get; }



        #region ---- ComposeQuery FluentFunctions ----


        /// <summary>
        /// Définit les colonnes à sélectionner
        /// </summary>
        /// <param name="columns">Colonnes à sélectionner, vide pour *</param>
        IQueryBuilder Select(params string[] columns);

        /// <summary>
        /// Définit les colonnes à sélectionner avec DISTINCT
        /// </summary>
        /// <param name="columns">Colonnes à sélectionner, vide pour *</param>
        IQueryBuilder SelectDistinct(params string[] columns);

        /// <summary>
        /// Ajoute une expression SQL brute dans la clause SELECT
        /// </summary>
        /// <param name="rawSql">Expression SQL brute (ex: "COUNT(*) as total")</param>
        IQueryBuilder SelectRaw(string rawSql);

        /// <summary>
        /// Ajoute une fonction de fenêtre (window function) dans la clause SELECT
        /// </summary>
        /// <param name="function">Fonction d'agrégation (ex: "ROW_NUMBER()", "RANK()", "SUM(amount)")</param>
        /// <param name="alias">Alias pour la fonction</param>
        /// <param name="partitionBy">Colonnes pour PARTITION BY (optionnel)</param>
        /// <param name="orderBy">Colonnes pour ORDER BY (optionnel)</param>
        IQueryBuilder SelectWindowFunction(string function, string alias, string partitionBy = null, string orderBy = null);

        /// <summary>
        /// Prépare une insertion
        /// </summary>
        /// <param name="values">Valeurs à insérer</param>
        IQueryBuilder Insert(Dictionary<string, object> values);

        /// <summary>
        /// Prépare une insertion multi-lignes depuis un DataTable
        /// </summary>
        /// <param name="table">DataTable contenant les lignes à insérer</param>
        /// <param name="excludeColumns">Colonnes à exclure (ex: colonnes auto-increment)</param>
        IQueryBuilder InsertWithDataTable(System.Data.DataTable table, string[] excludeColumns = null);

        /// <summary>
        /// Prépare une mise à jour
        /// </summary>
        /// <param name="values">Valeurs à mettre à jour</param>
        IQueryBuilder Update(Dictionary<string, object> values);

        /// <summary>
        /// Prépare une mise à jour depuis un DataRow
        /// </summary>
        /// <param name="row">DataRow contenant les données</param>
        /// <param name="allColumns">Si true, met à jour toutes les colonnes. Si false, seulement les modifiées</param>
        /// <param name="explicitWhereColumns">Colonnes explicites pour WHERE (null = utilise PrimaryKey)</param>
        IQueryBuilder UpdateWithDataRow(System.Data.DataRow row, bool allColumns = false, string[] explicitWhereColumns = null);

        /// <summary>
        /// Prépare une suppression
        /// </summary>
        IQueryBuilder Delete();

        /// <summary>
        /// Définit la table principale
        /// </summary>
        /// <param name="tableName">Nom de la table</param>
        IQueryBuilder From(string tableName);

        /// <summary>
        /// Ajoute une clause INTO (SELECT uniquement)
        /// </summary>
        /// <param name="tableName">Table de destination</param>
        IQueryBuilder Into(string tableName);

        /// <summary>
        /// Ajoute une jointure personnalisée
        /// </summary>
        /// <param name="joinType">Type de jointure (INNER, LEFT, RIGHT, FULL)</param>
        /// <param name="table">Table à joindre</param>
        /// <param name="condition">Condition de jointure</param>
        IQueryBuilder Join(string table, string condition, JoinTypeEnum joinType = JoinTypeEnum.Inner);

        /// <summary>
        /// Ajoute une clause JOIN brute en SQL
        /// </summary>
        /// <param name="rawJoinClause">Clause JOIN complète (ex: "INNER JOIN users u ON u.id = orders.user_id")</param>
        /// <param name="parameters">Paramètres optionnels pour la clause JOIN</param>
        IQueryBuilder JoinRaw(string rawJoinClause, Dictionary<string, object> parameters = null);



        /// <summary>
        /// Ajoute une condition WHERE avec opérateur
        /// </summary>
        /// <param name="column">Colonne</param>
        /// <param name="whereOperator">Opérateur (=, &gt;, &lt;, &gt;=, &lt;=, !=, LIKE, NOT LIKE)</param>
        /// <param name="value">Valeur</param>
        /// <param name="ifNullEmpty">Ignore la condition si la valeur est null ou vide (string) sinon erreur</param>
        IQueryBuilder Where(string column, string whereOperator, object value, IfNullEmptyEnum ifNullEmpty = IfNullEmptyEnum.Nullable);

        /// <summary>
        /// Ajoute plusieurs conditions WHERE avec l'opérateur = (égalité)
        /// </summary>
        /// <param name="values">Dictionnaire de colonnes et valeurs (colonne = valeur)</param>
        /// <param name="ifNullEmpty">Ignore les conditions si la valeur est null ou vide (string)</param>
        IQueryBuilder WhereEquals(Dictionary<string, object> values, IfNullEmptyEnum ifNullEmpty = IfNullEmptyEnum.Nullable);


        /// <summary>
        /// Ajoute une condition WHERE BETWEEN
        /// Gestion automatique des valeurs nulles (min ou max) pour créer une condition >= ou <=
        /// </summary>
        /// <param name="column">Colonne</param>
        /// <param name="min">Valeur minimale</param>
        /// <param name="max">Valeur maximale</param>
        IQueryBuilder WhereBetween(string column, object min, object max);


        /// <summary>
        /// Ajoute une condition WHERE IN
        /// </summary>
        /// <param name="column">Colonne</param>
        /// <param name="values">Liste de valeurs</param>
        /// <param name="isNotCondition">Si true, utilise NOT IN</param>
        IQueryBuilder WhereIn(string column, IEnumerable<object> values, bool isNotCondition = false);

        /// <summary>
        /// Ajoute une condition WHERE IS NULL
        /// </summary>
        /// <param name="column">Colonne</param>
        /// <param name="isNotCondition">Si true, utilise IS NOT NULL</param>
        IQueryBuilder WhereNull(string column, bool isNotCondition = false);

        /// <summary>
        /// Ajoute une clause WHERE personnalisée avec paramètres
        /// </summary>
        /// <param name="sqlClause">Clause SQL</param>
        /// <param name="parameters">Paramètres (optionnel)</param>
        IQueryBuilder WhereRaw(string sqlClause, Dictionary<string, object>? parameters = null);
 

        /// <summary>
        /// Ajoute une condition WHERE avec sous-requête
        /// </summary>
        /// <param name="column">Colonne</param>
        /// <param name="whereOperator">Opérateur (=, &gt;, &lt;, IN, etc.)</param>
        /// <param name="subquery">Sous-requête</param>
        IQueryBuilder WhereSubquery(string column, string whereOperator, IQueryBuilder subquery);

        /// <summary>
        /// Ajoute une condition WHERE EXISTS avec sous-requête
        /// </summary>
        /// <param name="subquery">Sous-requête</param>
        /// <param name="isNotCondition">Si true, utilise NOT EXISTS</param>
        IQueryBuilder WhereExists(IQueryBuilder subquery, bool isNotCondition = false);

        #endregion



        #region Tri, groupement et limite

        /// <summary>
        /// Ajoute un tri ascendant
        /// </summary>
        /// <param name="columns">Colonnes à trier avec eventuellement le suffixe ASC ou DESC</param>
        IQueryBuilder OrderBy(params string[] columns);


        /// <summary>
        /// Ajoute un groupement
        /// </summary>
        /// <param name="columns">Colonnes de groupement</param>
        IQueryBuilder GroupBy(params string[] columns);

        /// <summary>
        /// Ajoute une clause HAVING
        /// </summary>
        /// <param name="condition">Condition HAVING</param>
        IQueryBuilder Having(string condition);

        /// <summary>
        /// Ajoute une clause HAVING brute en SQL
        /// </summary>
        /// <param name="rawSql">Expression SQL brute pour la condition HAVING</param>
        /// <param name="parameters">Paramètres optionnels pour la condition</param>
        IQueryBuilder HavingRaw(string rawSql, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Ajoute une Common Table Expression (CTE) avec la clause WITH
        /// </summary>
        /// <param name="alias">Nom de la CTE</param>
        /// <param name="subquery">Sous-requête définissant la CTE</param>
        IQueryBuilder With(string alias, IQueryBuilder subquery);


        /// <summary>
        /// Limite le nombre de résultats
        /// </summary>
        /// <param name="count">Nombre maximum</param>
        /// <param name="offset">Décalage optionnel</param>
        IQueryBuilder Limit(int count, int? offset = null);


        #endregion




        #region ----- Utilitaire et paramètres -----

        /// <summary>
        /// Applique les critères d'un formulaire de recherche (pagination, tri, etc.)
        /// </summary>
        /// <param name="searchForm">Formulaire de recherche</param>
        IQueryBuilder ApplySearchForm(DATA.BASICS.ISearchForm searchForm);

        /// <summary>
        /// Ajoute un paramètre
        /// </summary>
        /// <param name="name">Nom du paramètre</param>
        /// <param name="value">Valeur</param>
        IQueryBuilder AddParameter(string name, object value);

        /// <summary>
        /// Ajoute plusieurs paramètres
        /// </summary>
        /// <param name="parameters">Dictionnaire des paramètres</param>
        IQueryBuilder AddParameters(Dictionary<string, object> parameters);

        /// <summary>
        /// Récupère tous les paramètres
        /// </summary>
        Dictionary<string, object> GetParameters();


        /// <summary>
        /// Configure les options du QueryBuilder (fluent)
        /// </summary>
        /// <param name="options">Options à appliquer</param>
        IQueryBuilder WithOptions(QueryBuilderOptionModel options);

        /// <summary>
        /// Clone le query builder actuel
        /// </summary>
        IQueryBuilder Clone();

        /// <summary>
        /// Remet à zéro le query builder
        /// </summary>
        IQueryBuilder Reset();

        /// <summary>
        /// Valide la requête et retourne true si valide, false sinon
        /// </summary>
        /// <returns>True si la requête peut être construite</returns>
        bool ValidateQuery();

        /// <summary>
        /// Valide la requête et lève une exception détaillée si invalide
        /// </summary>
        /// <exception cref="InvalidOperationException">Si la requête est invalide</exception>
        void ValidateQueryOrThrow();


        /// <summary>
        /// Construit la requête SQL finale
        /// </summary>
        Tuple<string, Dictionary<string, object>> Build();

        /// <summary>
        /// Construit uniquement la clause WHERE avec ses paramètres
        /// </summary>
        Tuple<string, Dictionary<string, object>> BuildWhereClause();

        /// <summary>
        /// Construit le contexte de requête NgLib
        /// </summary>
        Nglib.DATA.CONNECTOR.QueryContext BuildQuery();

        #endregion



    }
}
