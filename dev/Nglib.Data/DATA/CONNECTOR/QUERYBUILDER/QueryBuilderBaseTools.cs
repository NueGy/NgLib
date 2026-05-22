using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// Outils techniques internes pour QueryBuilder
    /// </summary>
    internal static class QueryBuilderBaseTools
    {
        /// <summary>
        /// Traite les paramètres d'une sous-requête pour éviter les conflits de noms
        /// </summary>
        /// <param name="context">Contexte du QueryBuilder parent</param>
        /// <param name="subquery">Sous-requête à traiter</param>
        /// <param name="prefix">Préfixe pour les paramètres (ex: "sub", "cte")</param>
        /// <returns>Tuple contenant la requête modifiée et le mapping des paramètres</returns>
        internal static (string modifiedQuery, Dictionary<string, string> parameterMap) ProcessSubqueryParameters(
            QueryBuilderContext context, 
            IQueryBuilder subquery, 
            string prefix = "sub")
        {
            var subqueryResult = subquery.Build();
            if (string.IsNullOrWhiteSpace(subqueryResult.Item1))
                throw new InvalidOperationException("Subquery cannot be empty");
            
            var parameterMap = new Dictionary<string, string>();
            foreach (var param in subqueryResult.Item2)
            {
                var prefixedKey = $"{prefix}_{param.Key}";
                var uniqueKey = QueryBuilderTools.GenerateParameterName(prefixedKey, context.Parameters);
                context.Parameters.Add(uniqueKey, param.Value);
                parameterMap[param.Key] = uniqueKey;
            }

            var modifiedSubquery = subqueryResult.Item1;
            foreach (var map in parameterMap.OrderByDescending(p => p.Key.Length))
                modifiedSubquery = modifiedSubquery.Replace($"@{map.Key}", $"@{map.Value}");
            
            return (modifiedSubquery, parameterMap);
        }

        /// <summary>
        /// Ajoute du SQL brut avec des paramètres optionnels (pattern générique pour méthodes *Raw)
        /// </summary>
        /// <param name="context">Contexte du QueryBuilder</param>
        /// <param name="rawSql">SQL brut à ajouter</param>
        /// <param name="parameters">Paramètres optionnels</param>
        /// <param name="addAction">Action pour ajouter le SQL à la collection appropriée</param>
        /// <param name="paramName">Nom du paramètre pour les messages d'erreur</param>
        internal static void AddRawWithParams(
            QueryBuilderContext context,
            string rawSql, 
            Dictionary<string, object> parameters, 
            Action<string> addAction, 
            string paramName)
        {
            if (string.IsNullOrWhiteSpace(rawSql))
                throw new ArgumentException($"{paramName} cannot be null or empty", nameof(rawSql));
            
            addAction(rawSql);
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                    context.Parameters.TryAdd(param.Key, param.Value);
            }
        }

        /// <summary>
        /// Construit la clause WITH pour les CTEs (Common Table Expressions)
        /// </summary>
        /// <param name="context">Contexte du QueryBuilder</param>
        /// <returns>Clause WITH complète ou chaîne vide si pas de CTEs</returns>
        internal static string BuildCTEClause(QueryBuilderContext context)
        {
            if (!context.CteClauses.Any()) return "";
            
            var sql = new StringBuilder("WITH ");
            var cteStrings = new List<string>();
            
            foreach (var cte in context.CteClauses)
            {
                var subqueryResult = cte.query.Build();
                
                // Ajouter les paramètres de la sous-requête avec un préfixe
                foreach (var param in subqueryResult.Item2)
                {
                    var prefixedKey = $"cte_{cte.alias}_{param.Key}";
                    context.Parameters.TryAdd(prefixedKey, param.Value);
                }
                
                // Remplacer les paramètres dans la sous-requête
                var modifiedSubquery = subqueryResult.Item1;
                foreach (var param in subqueryResult.Item2.OrderByDescending(p => p.Key.Length))
                {
                    var prefixedKey = $"cte_{cte.alias}_{param.Key}";
                    modifiedSubquery = modifiedSubquery.Replace($"@{param.Key}", $"@{prefixedKey}");
                }
                
                cteStrings.Add($"{cte.alias} AS ({modifiedSubquery})");
            }
            
            sql.Append(string.Join(", ", cteStrings));
            sql.Append(" ");
            return sql.ToString();
        }

        // ============================================
        // OPTIMISATION #6 : Plus de logique technique déplacée
        // ============================================

        /// <summary>
        /// Génère une liste de placeholders SQL pour des colonnes (@col1, @col2, ...)
        /// </summary>
        /// <param name="columns">Colonnes ou clés pour lesquelles générer les placeholders</param>
        /// <param name="formatPlaceholder">Fonction optionnelle pour formater le placeholder (ex: PostgreSQL JSONB)</param>
        /// <returns>Liste de placeholders formatés</returns>
        internal static List<string> GeneratePlaceholders(
            IEnumerable<string> columns,
            Func<string, string, string> formatPlaceholder = null)
        {
            var placeholders = new List<string>();
            
            foreach (var col in columns)
            {
                var placeholder = formatPlaceholder != null 
                    ? formatPlaceholder(col, col) 
                    : $"@{col}";
                placeholders.Add(placeholder);
            }
            
            return placeholders;
        }

        /// <summary>
        /// Construit une clause INSERT multi-lignes
        /// </summary>
        /// <param name="context">Contexte du QueryBuilder</param>
        /// <param name="formatPlaceholder">Fonction optionnelle pour formater le placeholder</param>
        /// <returns>Clause INSERT VALUES complète</returns>
        internal static string BuildMultiInsertValues(
            QueryBuilderContext context,
            Func<string, string, string> formatPlaceholder = null)
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
                    // IMPORTANT : Pass le nom de la colonne (colName), pas le paramName
                    var placeholder = formatPlaceholder != null 
                        ? formatPlaceholder(colName, paramName)
                        : $"@{paramName}";
                    placeholders.Add(placeholder);
                }
                rowSqls.Add($"({string.Join(", ", placeholders)})");
            }
            
            sql.Append(string.Join(", ", rowSqls));
            return sql.ToString();
        }

        /// <summary>
        /// Construit une clause INSERT simple (une ligne)
        /// </summary>
        /// <param name="updateValues">Dictionnaire des colonnes et leurs valeurs</param>
        /// <param name="formatPlaceholder">Fonction optionnelle pour formater le placeholder</param>
        /// <returns>Clause INSERT VALUES complète</returns>
        internal static string BuildSingleInsertValues(
            Dictionary<string, object> updateValues,
            Func<string, string, string> formatPlaceholder = null)
        {
            var sql = new StringBuilder();
            
            // Colonnes
            sql.Append($"({string.Join(", ", updateValues.Keys)}) ");
            sql.Append("VALUES ");
            
            // Placeholders
            var placeholders = GeneratePlaceholders(updateValues.Keys, formatPlaceholder);
            sql.Append($"({string.Join(", ", placeholders)})");
            
            return sql.ToString();
        }

        /// <summary>
        /// Construit une clause SET pour UPDATE
        /// </summary>
        /// <param name="updateValues">Dictionnaire des colonnes à mettre à jour</param>
        /// <param name="formatPlaceholder">Fonction optionnelle pour formater le placeholder</param>
        /// <returns>Chaîne "col1 = @col1, col2 = @col2"</returns>
        internal static string BuildUpdateSetClause(
            Dictionary<string, object> updateValues,
            Func<string, string, string> formatPlaceholder = null)
        {
            var setClauses = new List<string>();
            
            foreach (var kvp in updateValues)
            {
                var placeholder = formatPlaceholder != null 
                    ? formatPlaceholder(kvp.Key, kvp.Key)
                    : $"@{kvp.Key}";
                setClauses.Add($"{kvp.Key} = {placeholder}");
            }
            
            return string.Join(", ", setClauses);
        }

        /// <summary>
        /// Valide qu'une requête DELETE/UPDATE a bien une clause WHERE (sécurité)
        /// </summary>
        /// <param name="context">Contexte du QueryBuilder</param>
        /// <param name="commandType">Type de commande SQL</param>
        /// <exception cref="InvalidOperationException">Si WHERE manquant sur DELETE/UPDATE</exception>
        internal static void ValidateWhereClauseRequired(QueryBuilderContext context, SqlCommandTypeEnum commandType)
        {
            if ((commandType == SqlCommandTypeEnum.Delete || commandType == SqlCommandTypeEnum.Update) 
                && !context.WhereClauses.Any())
            {
                var operation = commandType == SqlCommandTypeEnum.Delete ? "DELETE" : "UPDATE";
                throw new InvalidOperationException(
                    $"{operation} sans clause WHERE est dangereux. " +
                    $"Utilisez Where() ou désactivez cette protection avec Options.AllowUnsafeOperations = true");
            }
        }

        /// <summary>
        /// Construit la partie principale d'une requête SELECT (FROM + JOINs)
        /// </summary>
        /// <param name="context">Contexte du QueryBuilder</param>
        /// <param name="tableName">Nom de la table</param>
        /// <returns>Chaîne " FROM table JOIN ..."</returns>
        internal static string BuildFromAndJoins(QueryBuilderContext context, string tableName)
        {
            var sql = new StringBuilder();
            sql.Append($" FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(context.TableAlias))
                sql.Append($" AS {context.TableAlias}");
            sql.Append(QueryBuilderTools.BuildJoinClauses(context.JoinClauses));
            return sql.ToString();
        }








        /// <summary>
        /// Valide la cohérence logique d'un contexte de requête QueryBuilder
        /// </summary>
        /// <param name="context">Contexte à valider</param>
        /// <returns>True si le contexte est valide et cohérent</returns>
        internal static bool ValidateQueryContext(QueryBuilderContext context)
        {
            if (context == null) return false;

            // === VALIDATIONS DE BASE ===

            // TableName obligatoire pour toutes les commandes
            if (string.IsNullOrEmpty(context.TableName)) return false;

            // === VALIDATIONS PAR TYPE DE COMMANDE ===

            switch (context.CommandType)
            {
                case SqlCommandTypeEnum.Select:
                    // SELECT : Aucune colonne spécifiée = SELECT * (valide)
                    // Mais si GROUP BY existe, vérifier cohérence avec SELECT
                    if (context.GroupByClauses.Any() && context.SelectColumns.Any())
                    {
                        // Vérification basique : si GROUP BY utilisé, au moins une colonne SELECT devrait être agrégée ou dans GROUP BY
                        // (validation approfondie pourrait être trop stricte pour des cas avancés)
                    }
                    break;

                case SqlCommandTypeEnum.Insert:
                    // INSERT simple : UpdateValues requis
                    if (!context.IsMultiInsert && !context.UpdateValues.Any())
                        return false;

                    // INSERT multi : colonnes et lignes requises
                    if (context.IsMultiInsert && (!context.MultiInsertColumns.Any() || !context.MultiInsertRows.Any()))
                        return false;
                    break;

                case SqlCommandTypeEnum.Update:
                    // UPDATE : Valeurs requises
                    if (!context.UpdateValues.Any())
                        return false;

                    // UPDATE : WHERE clause obligatoire (sécurité)
                    if (!context.WhereClauses.Any())
                        return false;
                    break;

                case SqlCommandTypeEnum.Delete:
                    // DELETE : WHERE clause obligatoire (sécurité)
                    if (!context.WhereClauses.Any())
                        return false;
                    break;

                default:
                    return false; // Type de commande inconnu
            }

            // === VALIDATIONS DE COHÉRENCE ===

            // HAVING sans GROUP BY n'est pas valide
            if (!string.IsNullOrEmpty(context.HavingClause) && !context.GroupByClauses.Any())
                return false;

            // DISTINCT seulement pour SELECT
            if (context.IsDistinct && context.CommandType != SqlCommandTypeEnum.Select)
                return false;

            // SELECT INTO seulement pour SELECT
            if (!string.IsNullOrEmpty(context.IntoTable) && context.CommandType != SqlCommandTypeEnum.Select)
                return false;

            // LIMIT/OFFSET seulement pour SELECT
            if ((context.LimitCount.HasValue || context.OffsetCount.HasValue) && context.CommandType != SqlCommandTypeEnum.Select)
                return false;

            // CTE (WITH clause) seulement pour SELECT
            if (context.CteClauses.Any() && context.CommandType != SqlCommandTypeEnum.Select)
                return false;

            // ORDER BY généralement pour SELECT (mais peut être utilisé avec LIMIT dans DELETE/UPDATE selon SGBD)
            // On ne valide pas strictement car certains SGBD le permettent

            return true;
        }



        /// <summary>
        /// Détermine si une colonne doit être traitée comme du JSON/JSONB
        /// </summary>
        /// <param name="columnName">Nom de la colonne</param>
        /// <param name="value">Valeur à insérer</param>
        /// <returns>True si la colonne doit être convertie en JSONB</returns>
        internal static bool IsJsonColumn(string columnName, object value)
        {
            var lowerName = columnName.ToLowerInvariant();

            // Vérification par nom de colonne (conventions courantes)
            if (lowerName.Contains("json") ||
                lowerName.EndsWith("_json") ||
                lowerName.StartsWith("json_") ||
                lowerName.Equals("fluxjson") ||
                lowerName.Contains("flux"))  // Support pour les variantes de flux
            {
                return true;
            }

            // Vérification par contenu (chaîne JSON valide)
            if (value is string strValue && !string.IsNullOrWhiteSpace(strValue))
            {
                strValue = strValue.Trim();
                if ((strValue.StartsWith("{") && strValue.EndsWith("}")) ||
                    (strValue.StartsWith("[") && strValue.EndsWith("]")))
                {
                    return true; // Retour rapide sans parsing pour optimisation
                }
            }

            return false;
        }


    }
}