using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// Classe partielle QueryBuilderBase - Méthodes de construction SQL (Build)
    /// </summary>
    public abstract partial class QueryBuilderBase
    {
        #region Build Methods - Méthodes de construction SQL

        /// <summary>
        /// Surcharge de ToString() pour retourner le SQL généré
        /// </summary>
        /// <returns>La requête SQL générée</returns>
        public override string ToString()
        {
            try
            {
                var result = Build();
                return result.Item1;
            }
            catch
            {
                return base.ToString();
            }
        }

        /// <summary>
        /// Construit la requête SQL complète
        /// </summary>
        /// <returns>Tuple contenant la requête SQL et ses paramètres</returns>
        public virtual Tuple<string, Dictionary<string, object>> Build()
        {
            try
            {
                if (string.IsNullOrEmpty(TableName)) throw new Exception("TableName required");

                var sql = new StringBuilder();

                // Générer les CTE (WITH clause) si présentes
                sql.Append(QueryBuilderBaseTools.BuildCTEClause(context));

                if (context.CommandType == SqlCommandTypeEnum.Select)
                    BuildSelectSql(sql);
                else if (context.CommandType == SqlCommandTypeEnum.Insert)
                    BuildInsertSql(sql);
                else if (context.CommandType == SqlCommandTypeEnum.Update)
                    BuildUpdateSql(sql);
                else if (context.CommandType == SqlCommandTypeEnum.Delete)
                    BuildDeleteSql(sql);

                return new Tuple<string, Dictionary<string, object>>(sql.ToString(), Parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"QueryBuilder.Build ({EngineName}): {ex.Message}", ex);
            }
        }

        #endregion

        #region SELECT - Construction de requêtes SELECT

        /// <summary>
        /// Construit la requête SELECT (peut être overridée pour dialectes spécifiques)
        /// </summary>
        protected virtual void BuildSelectSql(StringBuilder sql)
        {
            sql.Append("SELECT ");
            
            // DISTINCT
            if (context.IsDistinct)
                sql.Append("DISTINCT ");

            // TOP pour SQL Server (si LIMIT sans OFFSET)
            BuildTopClause(sql);
            
            // Colonnes (utilise helper QueryBuilderTools)
            sql.Append(QueryBuilderTools.BuildColumnList(context.SelectColumns, "*"));

            // INTO (SELECT INTO pour PostgreSQL principalement)
            if (!string.IsNullOrEmpty(context.IntoTable))
                sql.Append($" INTO {context.IntoTable}");

            // FROM + JOINs (utilise helper QueryBuilderBaseTools)
            sql.Append(QueryBuilderBaseTools.BuildFromAndJoins(context, TableName));

            // WHERE (utilise helper QueryBuilderTools)
            sql.Append(QueryBuilderTools.BuildWhereClause(context.WhereClauses));

            // GROUP BY (utilise helper QueryBuilderTools)
            sql.Append(QueryBuilderTools.BuildGroupByClause(context.GroupByClauses));

            // HAVING (utilise helper QueryBuilderTools)
            sql.Append(QueryBuilderTools.BuildHavingClause(context.HavingClause));

            // ORDER BY (utilise helper QueryBuilderTools)
            sql.Append(QueryBuilderTools.BuildOrderByClause(context.OrderByClauses));

            // Pagination (LIMIT/OFFSET ou OFFSET/FETCH selon moteur)
            BuildPaginationClause(sql);
        }

        /// <summary>
        /// Construit la clause TOP pour SQL Server (override dans QueryBuilderMssql)
        /// </summary>
        protected virtual void BuildTopClause(StringBuilder sql)
        {
            // Par défaut : rien (Postgres/SQLite n'utilisent pas TOP)
        }

        /// <summary>
        /// Construit la clause de pagination (LIMIT/OFFSET par défaut, override pour SQL Server)
        /// </summary>
        protected virtual void BuildPaginationClause(StringBuilder sql)
        {
            // PostgreSQL et SQLite : LIMIT ... OFFSET ...
            if (context.LimitCount.HasValue)
            {
                sql.Append($" LIMIT {context.LimitCount.Value}");
                if (context.OffsetCount.HasValue)
                    sql.Append($" OFFSET {context.OffsetCount.Value}");
            }
        }

        #endregion

        #region INSERT - Construction de requêtes INSERT

        /// <summary>
        /// Construit la requête SQL INSERT (single ou multi-row)
        /// </summary>
        /// <param name="sql">StringBuilder pour construire la requête</param>
        protected virtual void BuildInsertSql(StringBuilder sql)
        {
            if (context.IsMultiInsert)
            {
                // Multi-INSERT (InsertWithDataTable) - Utilise la méthode virtuelle
                if (context.MultiInsertColumns.Count == 0 || context.MultiInsertRows.Count == 0)
                    throw new Exception("No values for multi-INSERT");

                sql.Append($"INSERT INTO {TableName} ");
                sql.Append(BuildMultiInsertValues());
            }
            else
            {
                // INSERT simple (single row) - Utilise la méthode virtuelle
                if (!context.UpdateValues.Any()) throw new Exception("No values for INSERT");

                sql.Append($"INSERT INTO {TableName} ");
                sql.Append(BuildSingleInsertValues(context.UpdateValues));

                // Copier les paramètres
                foreach (var kv in context.UpdateValues)
                    Parameters.TryAdd(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Construit une clause INSERT multi-lignes. Override pour personnaliser
        /// </summary>
        /// <returns>Clause INSERT VALUES complète</returns>
        protected virtual string BuildMultiInsertValues()
        {
            var sql = new StringBuilder();
            
            // Colonnes
            sql.Append($"({string.Join(", ", context.MultiInsertColumns)}) ");
            sql.Append("VALUES ");
            
            // Valeurs de chaque ligne
            var rowSqls = new List<string>();
            foreach (var row in context.MultiInsertRows)
            {
                var placeholders = new List<string>();
                foreach (var colName in context.MultiInsertColumns)
                {
                    var paramName = row[colName];
                    var placeholder = FormatPlaceholder(colName, paramName);
                    placeholders.Add(placeholder);
                }
                rowSqls.Add($"({string.Join(", ", placeholders)})");
            }
            
            sql.Append(string.Join(", ", rowSqls));
            return sql.ToString();
        }

        /// <summary>
        /// Construit une clause INSERT simple (une ligne). Override pour personnaliser
        /// </summary>
        /// <param name="values">Dictionnaire des colonnes et leurs valeurs</param>
        /// <returns>Clause INSERT VALUES complète</returns>
        protected virtual string BuildSingleInsertValues(Dictionary<string, object> values)
        {
            var sql = new StringBuilder();
            
            // Colonnes
            sql.Append($"({string.Join(", ", values.Keys)}) ");
            sql.Append("VALUES ");
            
            // Placeholders
            var placeholders = GeneratePlaceholders(values.Keys);
            sql.Append($"({string.Join(", ", placeholders)})");
            
            return sql.ToString();
        }

        #endregion

        #region UPDATE - Construction de requêtes UPDATE

        /// <summary>
        /// Construit la requête UPDATE
        /// </summary>
        protected virtual void BuildUpdateSql(StringBuilder sql)
        {
            if (!context.UpdateValues.Any()) 
                throw new Exception("No values for UPDATE");
            if (!context.WhereClauses.Any()) 
                throw new Exception("UPDATE query must have WHERE clause");

            sql.Append($"UPDATE {TableName} SET ");
            
            // Utilise la méthode virtuelle pour permettre l'override par moteur SQL
            sql.Append(BuildUpdateSetClause(context.UpdateValues));

            // WHERE (utilise helper QueryBuilderTools)
            sql.Append(QueryBuilderTools.BuildWhereClause(context.WhereClauses));
            
            // Copier les paramètres depuis context.UpdateValues vers Parameters
            foreach (var kv in context.UpdateValues)
                Parameters.TryAdd(kv.Key, kv.Value);
        }

        /// <summary>
        /// Construit une clause SET pour UPDATE. Override pour personnaliser le comportement
        /// </summary>
        /// <param name="updateValues">Dictionnaire des colonnes à mettre à jour</param>
        /// <returns>Chaîne "col1 = @col1, col2 = @col2"</returns>
        protected virtual string BuildUpdateSetClause(Dictionary<string, object> updateValues)
        {
            var setClauses = new List<string>();
            
            foreach (var kvp in updateValues)
            {
                var placeholder = FormatPlaceholder(kvp.Key, kvp.Key);
                setClauses.Add($"{kvp.Key} = {placeholder}");
            }
            
            return string.Join(", ", setClauses);
        }

        #endregion

        #region DELETE - Construction de requêtes DELETE

        /// <summary>
        /// Construit la requête DELETE
        /// </summary>
        protected virtual void BuildDeleteSql(StringBuilder sql)
        {
            if (!context.WhereClauses.Any()) 
                throw new Exception("DELETE query must have WHERE clause");

            sql.Append($"DELETE FROM {TableName}");

            // WHERE (utilise helper QueryBuilderTools)
            sql.Append(QueryBuilderTools.BuildWhereClause(context.WhereClauses));
        }

        #endregion

        #region Helpers - Méthodes utilitaires

        /// <summary>
        /// Formate un placeholder pour INSERT/UPDATE. Override pour personnaliser (ex: PostgreSQL ::jsonb)
        /// </summary>
        /// <param name="columnName">Nom de la colonne</param>
        /// <param name="paramName">Nom du paramètre</param>
        /// <returns>Placeholder formaté (ex: "@param" ou "@param::jsonb")</returns>
        protected virtual string FormatPlaceholder(string columnName, string paramName)
        {
            return $"@{paramName}";
        }

        /// <summary>
        /// Génère une liste de placeholders formatés pour les colonnes. Override pour personnaliser
        /// </summary>
        /// <param name="columns">Colonnes pour lesquelles générer les placeholders</param>
        /// <returns>Liste de placeholders formatés</returns>
        protected virtual List<string> GeneratePlaceholders(IEnumerable<string> columns)
        {
            var placeholders = new List<string>();
            
            foreach (var col in columns)
            {
                var placeholder = FormatPlaceholder(col, col);
                placeholders.Add(placeholder);
            }
            
            return placeholders;
        }

        #endregion
    }
}
