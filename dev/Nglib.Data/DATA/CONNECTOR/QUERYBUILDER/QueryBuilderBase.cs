using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// Classe de base pour tous les QueryBuilders.
    /// Fournit l'API Fluent pour construire des requêtes SQL de manière sûre et portable entre différents moteurs (PostgreSQL, MSSQL, SQLite).
    /// </summary>
    public abstract partial class QueryBuilderBase : IQueryBuilder
    {
        /// <summary>
        /// Nom du moteur SQL cible (PostgreSQL, MSSQL, SQLite) sera défini dans les classes dérivées
        /// </summary>
        public abstract string EngineName { get; }
        
        /// <summary>
        /// Contexte contenant l'état de la requête
        /// </summary>
        protected QueryBuilderContext context { get; set; }

        /// <summary>
        /// Nom de la table principale (accès via contexte)
        /// </summary>
        public string TableName 
        { 
            get => context.TableName; 
            protected set => context.TableName = value; 
        }

        /// <summary>
        /// Paramètres de la requête (accès via contexte)
        /// </summary>
        public Dictionary<string, object> Parameters 
        { 
            get => context.Parameters; 
            protected set => context.Parameters = value; 
        }

        /// <summary>
        /// Options de configuration du QueryBuilder (accès via contexte)
        /// </summary>
        public QueryBuilderOptionModel Options 
        { 
            get => context.Options; 
            protected set => context.Options = value; 
        }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        protected QueryBuilderBase()
        {
            context = new QueryBuilderContext();
        }

        /// <summary>
        /// Constructeur avec contexte existant
        /// </summary>
        /// <param name="existingContext">Contexte à utiliser</param>
        protected QueryBuilderBase(QueryBuilderContext existingContext)
        {
            context = existingContext ?? new QueryBuilderContext();
        }

        /// <summary>
        /// Accès interne au contexte pour les outils techniques
        /// </summary>
        /// <returns>Le contexte de la requête</returns>
        internal QueryBuilderContext GetContext() => context;

        /// <summary>
        /// Sélectionne les colonnes à récupérer
        /// </summary>
        public virtual IQueryBuilder Select(params string[] columns)
        {
            context.CommandType = SqlCommandTypeEnum.Select;
            context.IsDistinct = false;
            if (columns?.Length > 0)
            {
                // Valider tous les noms de colonnes en une seule fois
                QueryBuilderTools.ValidateColumns(columns);
                context.SelectColumns = columns.ToList();
            }
            return this;
        }

        /// <summary>
        /// Sélectionne les colonnes distinctes
        /// </summary>
        public virtual IQueryBuilder SelectDistinct(params string[] columns)
        {
            context.CommandType = SqlCommandTypeEnum.Select;
            context.IsDistinct = true;
            if (columns?.Length > 0)
            {
                // Valider tous les noms de colonnes en une seule fois
                QueryBuilderTools.ValidateColumns(columns);
                context.SelectColumns = columns.ToList();
            }
            return this;
        }

        /// <summary>
        /// Sélection SQL brute
        /// </summary>
        public virtual IQueryBuilder SelectRaw(string rawSql)
        {
            if (string.IsNullOrWhiteSpace(rawSql))
                throw new ArgumentException("Raw SQL cannot be null or empty", nameof(rawSql));
            
            context.CommandType = SqlCommandTypeEnum.Select;
            context.SelectColumns.Add(rawSql);
            return this;
        }

        /// <summary>
        /// Sélection avec fonction fenêtre SQL
        /// </summary>
        public virtual IQueryBuilder SelectWindowFunction(string function, string alias, string partitionBy = null, string orderBy = null)
        {
            if (string.IsNullOrWhiteSpace(function))
                throw new ArgumentException("Function cannot be null or empty", nameof(function));
            
            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentException("Alias cannot be null or empty", nameof(alias));
            
            context.CommandType = SqlCommandTypeEnum.Select;
            
            var windowFunction = new StringBuilder();
            windowFunction.Append(function);
            windowFunction.Append(" OVER (");
            
            if (!string.IsNullOrWhiteSpace(partitionBy))
                windowFunction.Append($"PARTITION BY {partitionBy}");
            
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                if (!string.IsNullOrWhiteSpace(partitionBy))
                    windowFunction.Append(" ");
                windowFunction.Append($"ORDER BY {orderBy}");
            }
            
            windowFunction.Append($") AS {alias}");
            
            context.SelectColumns.Add(windowFunction.ToString());
            return this;
        }

        /// <summary>
        /// Insère des données
        /// </summary>
        public virtual IQueryBuilder Insert(Dictionary<string, object> values)
        {
            context.CommandType = SqlCommandTypeEnum.Insert;
            context.UpdateValues = values ?? new Dictionary<string, object>();
            return this;
        }

        /// <summary>
        /// Met à jour des données
        /// </summary>
        public virtual IQueryBuilder Update(Dictionary<string, object> values)
        {
            context.CommandType = SqlCommandTypeEnum.Update;
            context.UpdateValues = values ?? new Dictionary<string, object>();
            return this;
        }

        /// <summary>
        /// Met à jour à partir d'une ligne DataRow
        /// </summary>
        public virtual IQueryBuilder UpdateWithDataRow(System.Data.DataRow row, bool allColumns = false, string[] explicitWhereColumns = null)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (row.Table == null) throw new ArgumentException("DataRow must belong to a DataTable", nameof(row));

            context.CommandType = SqlCommandTypeEnum.Update;

            // Définir le nom de la table depuis le DataRow
            if (string.IsNullOrWhiteSpace(TableName) && !string.IsNullOrWhiteSpace(row.Table.TableName))
                TableName = row.Table.TableName;

            // Obtenir les valeurs à mettre à jour (modifiées ou toutes)
            if (allColumns)
                context.UpdateValues = Nglib.DATA.COLLECTIONS.DataSetTools.GetValues(row, false, true); // Toutes colonnes sauf primary keys
            else
                context.UpdateValues = Nglib.DATA.COLLECTIONS.DataSetTools.GetChangedValues(row, true); // Seulement colonnes modifiées

            if (context.UpdateValues.Count == 0)  
                throw new InvalidOperationException("No columns to update");

            // Construire la clause WHERE
            Dictionary<string, object> whereValues;
            
            if (explicitWhereColumns != null && explicitWhereColumns.Length > 0)
            {
                // Utiliser les colonnes explicites pour WHERE
                whereValues = Nglib.DATA.COLLECTIONS.DataSetTools.GetValues(row, explicitWhereColumns);
            }
            else if (row.Table.PrimaryKey != null && row.Table.PrimaryKey.Length > 0)
            {
                // Utiliser les Primary Keys
                whereValues = Nglib.DATA.COLLECTIONS.DataSetTools.GetValues(row, true, false);
            }
            else
            {
                throw new InvalidOperationException("No WHERE clause possible: no PrimaryKey defined and no explicitWhereColumns provided");
            }

            if (whereValues.Count == 0)
                throw new InvalidOperationException("WHERE clause is empty");

            // Ajouter les conditions WHERE
            foreach (var kvp in whereValues)
            {
                Where(kvp.Key, "=", kvp.Value);
            }

            return this;
        }

        /// <summary>
        /// Insère plusieurs lignes depuis une DataTable
        /// </summary>
        public virtual IQueryBuilder InsertWithDataTable(System.Data.DataTable table, string[] excludeColumns = null)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (table.Rows.Count == 0) throw new ArgumentException("DataTable has no rows", nameof(table));
            if (string.IsNullOrWhiteSpace(table.TableName)) throw new ArgumentException("DataTable.TableName is empty", nameof(table));

            context.CommandType = SqlCommandTypeEnum.Insert;
            context.IsMultiInsert = true;
            
            // Définir le nom de la table
            if (string.IsNullOrWhiteSpace(TableName))
                TableName = table.TableName;

            // Identifier les colonnes à insérer
            context.MultiInsertColumns = table.Columns.Cast<System.Data.DataColumn>()
                .Where(col => excludeColumns == null || !excludeColumns.Contains(col.ColumnName))
                .Where(col => !col.AutoIncrement) // Exclure automatiquement les colonnes auto-increment
                .Select(col => col.ColumnName)
                .ToList();

            if (context.MultiInsertColumns.Count == 0)
                throw new InvalidOperationException("No columns to insert");

            // Construire les valeurs multi-lignes
            // On stocke toutes les valeurs avec des noms de paramètres lisibles : {column}_{rowIndex}
            int rowIndex = 1;
            context.MultiInsertRows.Clear();

            foreach (System.Data.DataRow row in table.Rows)
            {
                var rowParamMapping = new Dictionary<string, string>();
                
                foreach (var colName in context.MultiInsertColumns)
                {
                    var paramName = $"{colName}_{rowIndex}";
                    
                    Parameters[paramName] = row[colName];
                    rowParamMapping[colName] = paramName;
                }
                
                context.MultiInsertRows.Add(rowParamMapping);
                rowIndex++;
            }

            return this;
        }

        /// <summary>
        /// Supprime des données
        /// </summary>
        public virtual IQueryBuilder Delete()
        {
            context.CommandType = SqlCommandTypeEnum.Delete;
            return this;
        }

        /// <summary>
        /// Définit la table source
        /// </summary>
        public virtual IQueryBuilder From(string tableName, string alias = null)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            context.TableAlias = alias ?? "";
            return this;
        }

        /// <summary>
        /// Définit la table cible
        /// </summary>
        public virtual IQueryBuilder Into(string tableName)
        {
            context.IntoTable = tableName ?? "";
            return this;
        }

        /// <summary>
        /// Ajoute une jointure
        /// </summary>
        public virtual IQueryBuilder Join(string table, string condition, JoinTypeEnum joinType = JoinTypeEnum.Inner)
        {
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(condition)) return this;
            var joinStr = QueryBuilderTools.GetJoinTypeString(joinType);
            context.JoinClauses.Add($"{joinStr} {table} ON {condition}");
            return this;
        }

        /// <summary>
        /// Jointure SQL brute
        /// </summary>
        public virtual IQueryBuilder JoinRaw(string rawJoinClause, Dictionary<string, object> parameters = null)
        {
            QueryBuilderBaseTools.AddRawWithParams(context, rawJoinClause, parameters, 
                clause => context.JoinClauses.Add(clause), 
                "Raw JOIN clause");
            return this;
        }

        /// <summary>
        /// Ajoute une condition WHERE
        /// </summary>
        public virtual IQueryBuilder Where(string column, string whereOperator, object value, IfNullEmptyEnum ifNullEmpty = IfNullEmptyEnum.Nullable)
        {
            if (string.IsNullOrWhiteSpace(column) || string.IsNullOrWhiteSpace(whereOperator)) return this;

            // Valider le nom de colonne pour éviter l'injection SQL
            QueryBuilderTools.ValidateColumnName(column);

            var normalizedOperator = whereOperator.Trim();
            if (!QueryBuilderTools.IsValidOperator(normalizedOperator))
                throw new ArgumentException($"Operator '{whereOperator}' is not supported", nameof(whereOperator));

            var sanitizedValue = QueryBuilderTools.SanitizeValue(value, ifNullEmpty);
            if (sanitizedValue == null && ifNullEmpty == IfNullEmptyEnum.Ignore) return this;

            var operatorUpper = normalizedOperator.ToUpperInvariant();

            if (sanitizedValue == null && operatorUpper == "=")
            {
                context.WhereClauses.Add($"{column} IS NULL");
                return this;
            }

            if (sanitizedValue == null && (operatorUpper == "!=" || operatorUpper == "<>"))
            {
                context.WhereClauses.Add($"{column} IS NOT NULL");
                return this;
            }

            var paramName = QueryBuilderTools.GenerateParameterName(column, Parameters);
            Parameters.Add(paramName, sanitizedValue!);
            context.WhereClauses.Add($"{column} {operatorUpper} @{paramName}");
            return this;
        }

        /// <summary>
        /// Ajoute plusieurs conditions WHERE avec l'opérateur = (égalité)
        /// </summary>
        public virtual IQueryBuilder WhereEquals(Dictionary<string, object> values, IfNullEmptyEnum ifNullEmpty = IfNullEmptyEnum.Nullable)
        {
            if (values == null || values.Count == 0) return this;

            foreach (var kvp in values)
            {
                Where(kvp.Key, "=", kvp.Value, ifNullEmpty);
            }

            return this;
        }

        /// <summary>
        /// Ajoute une condition WHERE avec l'opérateur = (égalité)
        /// </summary>
        public virtual IQueryBuilder WhereEqual(string column, object value) => Where(column, "=", value);

        /// <summary>
        /// Ajoute une condition WHERE avec l'opérateur != (différent)
        /// </summary>
        public virtual IQueryBuilder WhereNotEqual(string column, object value) => Where(column, "!=", value);

        /// <summary>
        /// Ajoute une condition WHERE avec l'opérateur > (supérieur)
        /// </summary>
        public virtual IQueryBuilder WhereGreater(string column, object value) => Where(column, ">", value);

        /// <summary>
        /// Ajoute une condition WHERE avec l'opérateur < (inférieur)
        /// </summary>
        public virtual IQueryBuilder WhereLess(string column, object value) => Where(column, "<", value);

        /// <summary>
        /// Ajoute une condition WHERE avec l'opérateur LIKE
        /// </summary>
        public virtual IQueryBuilder WhereLike(string column, string pattern) => Where(column, "LIKE", pattern);

        /// <summary>
        /// Condition WHERE BETWEEN
        /// </summary>
        public virtual IQueryBuilder WhereBetween(string column, object min, object max)
        {
            if (string.IsNullOrEmpty(column)) return this;

            if (min == null && max == null) return this;

            if (min != null && max == null)
            {
                var paramName = QueryBuilderTools.GenerateParameterName($"{column}_min", Parameters);
                Parameters.Add(paramName, min);
                context.WhereClauses.Add($"{column} >= @{paramName}");
            }
            else if (max != null && min == null)
            {
                var paramName = QueryBuilderTools.GenerateParameterName($"{column}_max", Parameters);
                Parameters.Add(paramName, max);
                context.WhereClauses.Add($"{column} <= @{paramName}");
            }
            else
            {
                var minParam = QueryBuilderTools.GenerateParameterName($"{column}_min", Parameters);
                var maxParam = QueryBuilderTools.GenerateParameterName($"{column}_max", Parameters);
                Parameters.Add(minParam, min!);
                Parameters.Add(maxParam, max!);
                context.WhereClauses.Add($"{column} BETWEEN @{minParam} AND @{maxParam}");
            }
            return this;
        }

        /// <summary>
        /// Condition WHERE IN
        /// </summary>
        public virtual IQueryBuilder WhereIn(string column, IEnumerable<object> values, bool isNotCondition = false)
        {
            if (string.IsNullOrEmpty(column) || values == null) return this;

            var valuesList = values.ToList();
            if (!valuesList.Any()) return this;

            var inClause = QueryBuilderTools.ConvertToInSql(valuesList, Parameters, column);
            var notStr = isNotCondition ? "NOT " : "";
            context.WhereClauses.Add($"{column} {notStr}IN {inClause}");
            return this;
        }

        /// <summary>
        /// Condition WHERE NULL
        /// </summary>
        public virtual IQueryBuilder WhereNull(string column, bool isNotCondition = false)
        {
            if (string.IsNullOrEmpty(column)) return this;
            var nullStr = isNotCondition ? "IS NOT NULL" : "IS NULL";
            context.WhereClauses.Add($"{column} {nullStr}");
            return this;
        }

        /// <summary>
        /// Clause WHERE personnalisée
        /// </summary>
        public virtual IQueryBuilder WhereRaw(string sqlClause, Dictionary<string, object>? parameters = null)
        {
            if (string.IsNullOrEmpty(sqlClause)) return this;
            context.WhereClauses.Add(sqlClause);
            if (parameters != null)
                foreach (var param in parameters)
                    Parameters.TryAdd(param.Key, param.Value);
            return this;
        }

 

        /// <summary>
        /// WHERE avec sous-requête
        /// </summary>
        public virtual IQueryBuilder WhereSubquery(string column, string whereOperator, IQueryBuilder subquery)
        {
            // Valider le nom de colonne pour éviter l'injection SQL
            QueryBuilderTools.ValidateColumnName(column);
            
            if (string.IsNullOrWhiteSpace(whereOperator))
                throw new ArgumentException("Operator cannot be empty", nameof(whereOperator));
            
            if (subquery == null)
                throw new ArgumentNullException(nameof(subquery));
            
            var normalizedOperator = whereOperator.Trim();
            if (!QueryBuilderTools.IsValidOperator(normalizedOperator))
                throw new ArgumentException($"Operator '{whereOperator}' is not supported", nameof(whereOperator));
            var operatorUpper = normalizedOperator.ToUpperInvariant();

            // Utiliser QueryBuilderBaseTools pour traiter les paramètres de la sous-requête
            var (modifiedSubquery, _) = QueryBuilderBaseTools.ProcessSubqueryParameters(context, subquery);
            
            context.WhereClauses.Add($"{column} {operatorUpper} ({modifiedSubquery})");
            return this;
        }

        /// <summary>
        /// WHERE EXISTS
        /// </summary>
        public virtual IQueryBuilder WhereExists(IQueryBuilder subquery, bool isNotCondition = false)
        {
            if (subquery == null)
                throw new ArgumentNullException(nameof(subquery));
            
            // Utiliser QueryBuilderBaseTools pour traiter les paramètres de la sous-requête
            var (modifiedSubquery, _) = QueryBuilderBaseTools.ProcessSubqueryParameters(context, subquery);
            
            var existsOperator = isNotCondition ? "NOT EXISTS" : "EXISTS";
            context.WhereClauses.Add($"{existsOperator} ({modifiedSubquery})");
            return this;
        }

        /// <summary>
        /// Tri ORDER BY
        /// </summary>
        public virtual IQueryBuilder OrderBy(params string[] columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            
            if (columns.Length == 0)
                throw new ArgumentException("At least one column must be specified for OrderBy", nameof(columns));
            
            // Valider tous les noms de colonnes (strict=false permet ASC/DESC)
            QueryBuilderTools.ValidateColumns(columns, strict: false);
            
            context.OrderByClauses = columns.ToList();
            return this;
        }

        /// <summary>
        /// Groupement GROUP BY
        /// </summary>
        public virtual IQueryBuilder GroupBy(params string[] columns)
        {
            if (columns?.Length > 0)
            {
                // Valider tous les noms de colonnes en une seule fois
                QueryBuilderTools.ValidateColumns(columns);
                context.GroupByClauses = columns.ToList();
            }
            return this;
        }

        /// <summary>
        /// Condition HAVING
        /// </summary>
        public virtual IQueryBuilder Having(string condition)
        {
            context.HavingClause = condition ?? "";
            return this;
        }

        /// <summary>
        /// HAVING SQL brut
        /// </summary>
        public virtual IQueryBuilder HavingRaw(string rawSql, Dictionary<string, object> parameters = null)
        {
            QueryBuilderBaseTools.AddRawWithParams(context, rawSql, parameters, 
                sql => context.HavingClause = sql, 
                "Raw HAVING clause");
            return this;
        }

        /// <summary>
        /// CTE (WITH)
        /// </summary>
        public virtual IQueryBuilder With(string alias, IQueryBuilder subquery)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentException("CTE alias cannot be null or empty", nameof(alias));
            
            if (subquery == null)
                throw new ArgumentNullException(nameof(subquery));
            
            // Caster vers BaseQueryBuilder pour accéder aux propriétés internes
            var baseSubquery = subquery as QueryBuilderBase;
            if (baseSubquery == null)
                throw new ArgumentException("Subquery must be a BaseQueryBuilder instance", nameof(subquery));
            
            // Valider que la sous-requête est une SELECT
            if (baseSubquery.context.CommandType != SqlCommandTypeEnum.Select)
                throw new ArgumentException("CTE subquery must be a SELECT query", nameof(subquery));
            
            context.CteClauses.Add((alias, baseSubquery));
            return this;
        }

        /// <summary>
        /// Limite le nombre de résultats
        /// </summary>
        public virtual IQueryBuilder Limit(int count, int? offset = null)
        {
            context.LimitCount = count > 0 ? count : null;
            context.OffsetCount = offset.HasValue && offset.Value >= 0 ? offset.Value : null;
            return this;
        }

        /// <summary>
        /// Ajoute un paramètre
        /// </summary>
        public virtual IQueryBuilder AddParameter(string name, object value)
        {
            if (!string.IsNullOrEmpty(name)) Parameters.TryAdd(name, value);
            return this;
        }

        /// <summary>
        /// Ajoute plusieurs paramètres
        /// </summary>
        public virtual IQueryBuilder AddParameters(Dictionary<string, object> parameters)
        {
            if (parameters != null)
                foreach (var param in parameters)
                    Parameters.TryAdd(param.Key, param.Value);
            return this;
        }

        /// <summary>
        /// Récupère les paramètres
        /// </summary>
        public virtual Dictionary<string, object> GetParameters() => new(Parameters);

        /// <summary>
        /// Applique les critères d'un formulaire de recherche (pagination, tri, etc.)
        /// </summary>
        public virtual IQueryBuilder ApplySearchForm(DATA.BASICS.ISearchForm searchForm)
        {
            if (searchForm == null) return this;

            try
            {
                // Pagination
                if (searchForm.LimitResults > 0)
                {
                    int offset = 0;
                    if (searchForm.CurrentPage > 1)
                        offset = (searchForm.CurrentPage - 1) * searchForm.LimitResults;
                    
                    Limit(searchForm.LimitResults, offset);
                }

                // Tri
                if (!string.IsNullOrWhiteSpace(searchForm.ShowOrderBy))
                {
                    OrderBy(searchForm.ShowOrderBy);
                }

                return this;
            }
            catch (Exception ex)
            {
                throw new Exception($"ApplySearchForm failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Définit les options
        /// </summary>
        public virtual IQueryBuilder WithOptions(QueryBuilderOptionModel options)
        {
            if (options != null)
                context.Options = options;
            return this;
        }

        /// <summary>
        /// Clone le QueryBuilder
        /// </summary>
        public virtual IQueryBuilder Clone()
        {
            try
            {
                var cloneType = this.GetType();
                var clone = (QueryBuilderBase)(Activator.CreateInstance(cloneType) 
                    ?? throw new InvalidOperationException($"Cannot create instance of {cloneType.Name}. Ensure it has a parameterless constructor."));
                
                // Clone du contexte (utilise la méthode Clone du contexte)
                clone.context = context.Clone();
                
                return clone;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"BaseQueryBuilder.Clone failed for type {GetType().Name}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Réinitialise le QueryBuilder
        /// </summary>
        public virtual IQueryBuilder Reset()
        {
            context.Reset();
            return this;
        }

        /// <summary>
        /// Valide la requête et retourne true si valide, false sinon
        /// </summary>
        /// <returns>True si la requête est valide et peut être construite</returns>
        public virtual bool ValidateQuery() => context.IsValid();

        /// <summary>
        /// Valide la requête et lève une exception détaillée si invalide
        /// </summary>
        /// <exception cref="InvalidOperationException">Si la requête est invalide avec détails de l'erreur</exception>
        public virtual void ValidateQueryOrThrow()
        {
            // === VALIDATIONS DE BASE ===
            if (string.IsNullOrEmpty(TableName))
                throw new InvalidOperationException("QueryBuilder validation failed: TableName is required");

            // === VALIDATIONS PAR TYPE DE COMMANDE ===
            switch (context.CommandType)
            {
                case SqlCommandTypeEnum.Select:
                    // SELECT est généralement permissif (SELECT * si pas de colonnes)
                    
                    // HAVING sans GROUP BY
                    if (!string.IsNullOrEmpty(context.HavingClause) && !context.GroupByClauses.Any())
                        throw new InvalidOperationException("QueryBuilder validation failed: HAVING clause requires GROUP BY");
                    break;

                case SqlCommandTypeEnum.Insert:
                    if (!context.IsMultiInsert && !context.UpdateValues.Any())
                        throw new InvalidOperationException("QueryBuilder validation failed: INSERT requires values (use Insert() method)");
                    
                    if (context.IsMultiInsert && (!context.MultiInsertColumns.Any() || !context.MultiInsertRows.Any()))
                        throw new InvalidOperationException("QueryBuilder validation failed: Multi-INSERT requires columns and rows (use InsertWithDataTable() method)");
                    break;

                case SqlCommandTypeEnum.Update:
                    if (!context.UpdateValues.Any())
                        throw new InvalidOperationException("QueryBuilder validation failed: UPDATE requires values (use Update() method)");
                    
                    if (!context.WhereClauses.Any())
                        throw new InvalidOperationException("QueryBuilder validation failed: UPDATE requires WHERE clause for safety (use Where() method)");
                    break;

                case SqlCommandTypeEnum.Delete:
                    if (!context.WhereClauses.Any())
                        throw new InvalidOperationException("QueryBuilder validation failed: DELETE requires WHERE clause for safety (use Where() method)");
                    break;

                default:
                    throw new InvalidOperationException($"QueryBuilder validation failed: Unknown command type '{context.CommandType}'");
            }

            // === VALIDATIONS DE COHÉRENCE ===
            
            // DISTINCT seulement pour SELECT
            if (context.IsDistinct && context.CommandType != SqlCommandTypeEnum.Select)
                throw new InvalidOperationException("QueryBuilder validation failed: DISTINCT is only valid for SELECT queries");

            // SELECT INTO seulement pour SELECT
            if (!string.IsNullOrEmpty(context.IntoTable) && context.CommandType != SqlCommandTypeEnum.Select)
                throw new InvalidOperationException("QueryBuilder validation failed: INTO clause is only valid for SELECT queries");

            // LIMIT/OFFSET seulement pour SELECT
            if ((context.LimitCount.HasValue || context.OffsetCount.HasValue) && context.CommandType != SqlCommandTypeEnum.Select)
                throw new InvalidOperationException("QueryBuilder validation failed: LIMIT/OFFSET is only valid for SELECT queries");

            // CTE (WITH clause) seulement pour SELECT
            if (context.CteClauses.Any() && context.CommandType != SqlCommandTypeEnum.Select)
                throw new InvalidOperationException("QueryBuilder validation failed: WITH clause (CTE) is only valid for SELECT queries");
        }

        /// <summary>
        /// Construit uniquement la clause WHERE
        /// </summary>
        /// <returns>Tuple contenant la clause WHERE et ses paramètres</returns>
        public virtual Tuple<string, Dictionary<string, object>> BuildWhereClause()
        {
            try
            {
                var whereParameters = new Dictionary<string, object>();
                
                if (!context.WhereClauses.Any())
                    return new Tuple<string, Dictionary<string, object>>("", whereParameters);
                
                // Copier uniquement les paramètres utilisés dans les clauses WHERE
                foreach (var param in Parameters)
                {
                    var whereClausesString = string.Join(" AND ", context.WhereClauses);
                    if (whereClausesString.Contains($"@{param.Key}"))
                    {
                        whereParameters.Add(param.Key, param.Value);
                    }
                }
                
                var whereClause = string.Join(" AND ", context.WhereClauses);
                return new Tuple<string, Dictionary<string, object>>(whereClause, whereParameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"BaseQueryBuilder.BuildWhereClause: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual Nglib.DATA.CONNECTOR.QueryContext BuildQuery()
        {
            var sqlp = Build();
            try
            {
                if (string.IsNullOrWhiteSpace(sqlp?.Item1)) return null!;
                var query = new Nglib.DATA.CONNECTOR.QueryContext();
                query.SqlQuery = sqlp.Item1;
                query.Parameters = sqlp.Item2 ?? new Dictionary<string, object>();
                return query;
            }
            catch (Exception ex)
            {
                throw new Exception("QueryBuilder.BuildQuery :" + ex.Message);
            }
        }
         
    }
}
