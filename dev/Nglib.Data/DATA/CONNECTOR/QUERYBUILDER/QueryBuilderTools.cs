using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{

    /// <summary>
    /// Outils techniques pour la construction de requêtes SQL
    /// </summary>
    public static class QueryBuilderTools
    {
        /// <summary>
        /// Liste des opérateurs SQL valides supportés par le QueryBuilder
        /// </summary>
        private static readonly HashSet<string> ValidOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "=","!=","<>",">","<",">=","<=",
            "LIKE","NOT LIKE","ILIKE","NOT ILIKE",
            "IN", "NOT IN", "IS", "IS NOT"
        };

        /// <summary>
        /// Valide un nom de colonne et lève une exception ArgumentException s'il est invalide.
        /// Vérifie que le nom n'est pas vide et qu'il ne contient pas de caractères dangereux (injection SQL).
        /// </summary>
        /// <param name="columnName">Nom de la colonne à valider</param>
        /// <param name="strict">Si false, permet les suffixes ASC/DESC pour les clauses ORDER BY</param>
        /// <exception cref="ArgumentException">Le nom de colonne est vide ou contient des caractères dangereux</exception>
        public static void ValidateColumnName(string columnName, bool strict = true)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name cannot be null or empty", nameof(columnName));
            
            // Si non-strict, nettoyer les suffixes ASC/DESC avant validation
            string cleanColumnName = columnName;
            if (!strict)
            {
                cleanColumnName = columnName.Replace(" ASC", "", StringComparison.OrdinalIgnoreCase)
                                           .Replace(" DESC", "", StringComparison.OrdinalIgnoreCase)
                                           .Trim();
            }
            
            if (!SanitizeColumnName(cleanColumnName))
                throw new ArgumentException(
                    $"Invalid column name '{columnName}'. Column name contains potentially dangerous characters.", 
                    nameof(columnName));
        }

        /// <summary>
        /// Valide plusieurs noms de colonnes et lève une exception si l'un d'eux est invalide
        /// </summary>
        /// <param name="columns">Tableau de noms de colonnes à valider</param>
        /// <param name="strict">Si false, permet les suffixes ASC/DESC pour les clauses ORDER BY</param>
        /// <exception cref="ArgumentNullException">Si le tableau de colonnes est null</exception>
        /// <exception cref="ArgumentException">Si un nom de colonne est null, vide ou contient des caractères dangereux</exception>
        public static void ValidateColumns(string[] columns, bool strict = true)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns), "Columns array cannot be null");

            for (int i = 0; i < columns.Length; i++)
            {
                try
                {
                    ValidateColumnName(columns[i], strict);
                }
                catch (ArgumentException ex)
                {
                    // Enrichir le message d'erreur avec l'index pour faciliter le débogage
                    throw new ArgumentException(
                        $"Column at index {i}: {ex.Message}", 
                        nameof(columns), 
                        ex);
                }
            }
        }

        /// <summary>
        /// Génère un nom de paramètre unique pour éviter les conflits dans les requêtes SQL
        /// </summary>
        /// <param name="baseName">Nom de base (sera nettoyé et peut être incrémenté si déjà utilisé)</param>
        /// <param name="existingParams">Dictionnaire des paramètres existants pour vérifier l'unicité</param>
        /// <returns>Nom de paramètre unique (sans le préfixe @)</returns>
        public static string GenerateParameterName(string baseName, Dictionary<string, object> existingParams)
        {
            var cleanName = baseName.Replace(".", "_").Replace(" ", "_");
            var paramName = cleanName;
            var counter = 1;
            while (existingParams.ContainsKey(paramName))
            {
                paramName = $"{cleanName}_{counter}"; 
                counter++;
            }
            return paramName;
        }

        /// <summary>
        /// Convertit une collection de valeurs en clause SQL IN avec paramètres
        /// </summary>
        /// <param name="values">Collection de valeurs à inclure dans la clause IN</param>
        /// <param name="parameters">Dictionnaire de paramètres où seront ajoutés les nouveaux paramètres</param>
        /// <param name="baseName">Nom de base pour générer les noms de paramètres</param>
        /// <returns>Clause SQL formatée : (paramètre1, paramètre2, ...)</returns>
        public static string ConvertToInSql(IEnumerable<object> values, Dictionary<string, object> parameters, string baseName)
        {
            var valuesList = values.ToList();
            if (!valuesList.Any()) return "('')";

            var paramNames = new List<string>();
            for (int i = 0; i < valuesList.Count; i++)
            {
                var paramName = GenerateParameterName($"{baseName}_{i}", parameters);
                parameters.Add(paramName, valuesList[i]);
                paramNames.Add($"@{paramName}");
            }
            return $"({string.Join(", ", paramNames)})";
        }

        /// <summary>
        /// Nettoie et valide une valeur selon la politique de gestion des nulls/vides
        /// </summary>
        /// <param name="value">Valeur à valider</param>
        /// <param name="nullHandling">Politique de gestion : Error (exception), Nullable (accepter), Ignore (retourner null)</param>
        /// <returns>Valeur nettoyée ou null si ignorée</returns>
        /// <exception cref="ArgumentNullException">Si nullHandling=Error et value est null</exception>
        /// <exception cref="ArgumentException">Si nullHandling=Error et value est une chaîne vide</exception>
        public static object SanitizeValue(object value, QueryBuilderModels.IfNullEmptyEnum nullHandling)
        {
            if (value == null)
            {
                if (nullHandling == QueryBuilderModels.IfNullEmptyEnum.Error)
                    throw new ArgumentNullException(nameof(value), "Value cannot be null");
                return null;
            }

            if (value is string str && string.IsNullOrEmpty(str))
            {
                if (nullHandling == QueryBuilderModels.IfNullEmptyEnum.Error)
                    throw new ArgumentException("String value cannot be empty", nameof(value));
                if (nullHandling == QueryBuilderModels.IfNullEmptyEnum.Ignore)
                    return null;
            }

            return value;
        }

        /// <summary>
        /// Valide qu'un nom de colonne SQL est sécurisé (pas d'injection SQL).
        /// Validation hybride : base KeyTools + caractères SQL spécifiques.
        /// </summary>
        /// <param name="columnName">Nom de colonne à valider (peut inclure alias, fonctions, etc.)</param>
        /// <returns>True si le nom est sécurisé, False sinon</returns>
        /// <remarks>
        /// Accepte : alphanumériques, underscore, point, tiret (KeyTools)
        /// + caractères SQL : espace, parenthèses, virgule, astérisque, crochets, backticks
        /// Bloque les séquences d'injection : --, /*, */, ;, xp_, sp_, EXEC
        /// </remarks>
        public static bool SanitizeColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName)) return false;

            // Détection des patterns d'injection SQL dangereux
            if (columnName.Contains("--") || 
                columnName.Contains("/*") || 
                columnName.Contains("*/") || 
                columnName.Contains(";"))
                return false;

            // Détection des commandes système dangereuses (insensible à la casse)
            string upperColumn = columnName.ToUpperInvariant();
            if (upperColumn.Contains("XP_") || 
                upperColumn.Contains("SP_") || 
                upperColumn.Contains("EXEC") ||
                upperColumn.Contains("EXECUTE"))
                return false;

            // Validation de base avec KeyTools (alphanum + . _ -)
            // Si validé par KeyTools, c'est OK
            if (Nglib.FORMAT.KeyTools.IsValidKey(columnName))
                return true;

            // Sinon, vérifier si c'est une expression SQL valide avec caractères étendus
            // Autorise en plus : espace, parenthèses (), virgule, astérisque *, crochets [], backticks `
            return System.Text.RegularExpressions.Regex.IsMatch(
                columnName, 
                @"^[a-zA-Z0-9\s._*(),\[\]`-]+$");
        }

        /// <summary>
        /// Valide qu'un nom de table SQL est sécurisé (pas d'injection SQL).
        /// Utilise KeyTools.IsValidKey() - validation stricte pour noms de tables.
        /// </summary>
        /// <param name="tableName">Nom de table à valider (schema.table)</param>
        /// <returns>True si le nom est sécurisé, False sinon</returns>
        /// <remarks>
        /// Accepte uniquement : alphanumériques, underscore, point, tiret
        /// Les noms de tables ne doivent pas contenir de caractères SQL complexes
        /// </remarks>
        public static bool SanitizeTableName(string tableName)
        {
            return Nglib.FORMAT.KeyTools.IsValidKey(tableName);
        }

        /// <summary>
        /// Vérifie si un opérateur SQL est valide et supporté
        /// </summary>
        /// <param name="op">Opérateur à valider (ex: "=", "LIKE", "IN", etc.)</param>
        /// <returns>True si l'opérateur est valide, False sinon</returns>
        public static bool IsValidOperator(string op)
        {
            if (string.IsNullOrWhiteSpace(op)) return false;
            return ValidOperators.Contains(op.ToUpper().Trim());
        }

        /// <summary>
        /// Convertit un enum JoinType en chaîne SQL correspondante
        /// </summary>
        /// <param name="joinType">Type de jointure</param>
        /// <returns>Chaîne SQL du type de jointure (ex: "INNER JOIN", "LEFT JOIN")</returns>
        public static string GetJoinTypeString(QueryBuilderModels.JoinTypeEnum joinType)
        {
            return joinType switch
            {
                QueryBuilderModels.JoinTypeEnum.Inner => "INNER JOIN",
                QueryBuilderModels.JoinTypeEnum.Left => "LEFT JOIN",
                QueryBuilderModels.JoinTypeEnum.Right => "RIGHT JOIN",
                QueryBuilderModels.JoinTypeEnum.Full => "FULL OUTER JOIN",
                QueryBuilderModels.JoinTypeEnum.Cross => "CROSS JOIN",
                _ => "INNER JOIN"
            };
        }

        /// <summary>
        /// Factory pour créer une instance de QueryBuilder selon le moteur SQL spécifié
        /// </summary>
        /// <param name="engineName">Nom du moteur : "postgresql", "mssql", "sqlserver", "sqlite"</param>
        /// <returns>Instance de IQueryBuilder configurée pour le moteur demandé, ou null si non supporté</returns>
        public static IQueryBuilder CreateQueryBuilder(string engineName)
        {
            return engineName?.ToLower().Trim() switch
            {
                "postgresql" or "postgres" or "pgsql" => new QueryBuilderPostgres(),
                "mssql" or "sqlserver" or "sql-server" => new QueryBuilderMssql(),
                "sqlite" or "sqlite3" => new QueryBuilderSqlite(),
                _ => null
            };
        }

        // ============================================
        // OPTIMISATION #1 & #2 : Helpers de formatage SQL
        // ============================================

        /// <summary>
        /// Formate une clause WHERE à partir d'une liste de conditions
        /// </summary>
        /// <param name="whereClauses">Liste des conditions WHERE</param>
        /// <returns>Chaîne formatée "WHERE condition1 AND condition2 AND ..." ou chaîne vide si pas de conditions</returns>
        public static string BuildWhereClause(List<string> whereClauses)
        {
            if (whereClauses == null || whereClauses.Count == 0)
                return "";
            
            return $" WHERE {string.Join(" AND ", whereClauses)}";
        }

        /// <summary>
        /// Formate une liste de colonnes avec séparateur virgule
        /// </summary>
        /// <param name="columns">Liste des colonnes</param>
        /// <param name="defaultValue">Valeur par défaut si la liste est vide (ex: "*")</param>
        /// <returns>Chaîne formatée "col1, col2, col3" ou la valeur par défaut</returns>
        public static string BuildColumnList(List<string> columns, string defaultValue = "*")
        {
            if (columns == null || columns.Count == 0)
                return defaultValue;
            
            return string.Join(", ", columns);
        }

        /// <summary>
        /// Formate une clause GROUP BY à partir d'une liste de colonnes
        /// </summary>
        /// <param name="groupByClauses">Liste des colonnes pour GROUP BY</param>
        /// <returns>Chaîne formatée " GROUP BY col1, col2" ou chaîne vide si pas de colonnes</returns>
        public static string BuildGroupByClause(List<string> groupByClauses)
        {
            if (groupByClauses == null || groupByClauses.Count == 0)
                return "";
            
            return $" GROUP BY {string.Join(", ", groupByClauses)}";
        }

        /// <summary>
        /// Formate une clause ORDER BY à partir d'une liste de colonnes
        /// </summary>
        /// <param name="orderByClauses">Liste des colonnes pour ORDER BY (peut inclure ASC/DESC)</param>
        /// <returns>Chaîne formatée " ORDER BY col1, col2 DESC" ou chaîne vide si pas de colonnes</returns>
        public static string BuildOrderByClause(List<string> orderByClauses)
        {
            if (orderByClauses == null || orderByClauses.Count == 0)
                return "";
            
            return $" ORDER BY {string.Join(", ", orderByClauses)}";
        }

        /// <summary>
        /// Formate une liste de clauses JOIN
        /// </summary>
        /// <param name="joinClauses">Liste des clauses JOIN complètes</param>
        /// <returns>Chaîne formatée avec toutes les JOINs concaténées, ou chaîne vide</returns>
        public static string BuildJoinClauses(List<string> joinClauses)
        {
            if (joinClauses == null || joinClauses.Count == 0)
                return "";
            
            return " " + string.Join(" ", joinClauses);
        }

        /// <summary>
        /// Formate une clause HAVING
        /// </summary>
        /// <param name="havingClause">Condition HAVING</param>
        /// <returns>Chaîne formatée " HAVING condition" ou chaîne vide</returns>
        public static string BuildHavingClause(string havingClause)
        {
            if (string.IsNullOrWhiteSpace(havingClause))
                return "";
            
            return $" HAVING {havingClause}";
        }

 
    }
}
