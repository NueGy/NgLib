using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Outils techniques internes pour les DataPOProvider
    /// </summary>
    internal static class DataPOProviderTools
    {
        /// <summary>
        /// Valide qu'un connecteur est configuré
        /// </summary>
        internal static void ValidateConnector(CONNECTOR.IDataConnector connector, Type providerType)
        {
            if (connector == null)
                throw new InvalidOperationException($"DataPOProvider<{providerType.Name}>: Aucun connecteur configuré. Veuillez définir la propriété Connectors.");
        }

        /// <summary>
        /// Valide qu'un schéma est défini pour un type DataPO
        /// </summary>
        internal static void ValidateSchema(DataTable schema, Type poType)
        {
            if (schema == null || string.IsNullOrWhiteSpace(schema.TableName))
                throw new InvalidOperationException($"ValidateSchema: Le schéma du type '{poType.Name}' n'est pas défini. Utilisez InitSchema() dans la classe {poType.Name}.");
            
            if (schema.TableName.Length < 2)
                throw new InvalidOperationException($"ValidateSchema: Le nom de table '{schema.TableName}' du type '{poType.Name}' est invalide (trop court).");
        }

        /// <summary>
        /// Valide que tous les objets sont du même type
        /// </summary>
        internal static void ValidateSameType<T>(T[] items, string operationName) where T : DataPO
        {
            if (items == null || items.Length == 0)
                return; // Pas d'erreur pour les tableaux vides - les méthodes appelantes gèrent ce cas

            // Vérifier la présence d'éléments null
            if (items.Any(item => item == null))
                throw new ArgumentException($"{operationName}: Le tableau contient des éléments null.", nameof(items));

            var distinctTypes = items.Select(b => b.GetType()).Distinct().Count();
            if (distinctTypes != 1)
                throw new ArgumentException($"{operationName}: Tous les objets DataPO doivent être du même type. {distinctTypes} types différents détectés.", nameof(items));
        }

        /// <summary>
        /// Valide qu'un objet n'est pas null
        /// </summary>
        internal static void ValidateNotNull<T>(T item, string paramName, string operationName) where T : class
        {
            if (item == null)
                throw new ArgumentNullException(paramName, $"{operationName}: L'objet '{paramName}' ne peut pas être null.");
        }

        /// <summary>
        /// Valide la présence de clés primaires
        /// </summary>
        internal static void ValidatePrimaryKeys(Dictionary<string, object> keys, Type poType, string operationName)
        {
            if (keys == null || keys.Count == 0)
                throw new InvalidOperationException($"{operationName}: Aucune clé primaire trouvée sur le type '{poType.Name}'. Vérifiez la définition du schéma.");
        }

        /// <summary>
        /// Obtient le nom de la colonne auto-incrémentée d'un schéma
        /// </summary>
        internal static string GetAutoIncrementColumnName(DataTable schema)
        {
            if (schema == null) return null;
            
            var autoIncrementColumn = schema.Columns.Cast<DataColumn>()
                .Where(col => col.AutoIncrement)
                .Select(col => col.ColumnName)
                .FirstOrDefault();
            
            return autoIncrementColumn;
        }

        /// <summary>
        /// Valide qu'une requête n'est pas null
        /// </summary>
        internal static void ValidateQuery(CONNECTOR.QueryContext query, string operationName)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), $"{operationName}: Le QueryContext ne peut pas être null.");
        }

        /// <summary>
        /// Complète une requête SQL partielle en ajoutant SELECT FROM si nécessaire
        /// </summary>
        internal static void CompleteSqlQuery(CONNECTOR.QueryContext query, string tableName)
        {
            if (query == null || string.IsNullOrWhiteSpace(tableName))
                return;

            var sqlUpper = query.SqlQuery?.ToUpper() ?? string.Empty;

            // Ajouter WHERE si c'est juste une condition
            if (!string.IsNullOrWhiteSpace(query.SqlQuery) && 
                !sqlUpper.Contains("SELECT ") && 
                !sqlUpper.Contains("WHERE "))
            {
                query.SqlQuery = " WHERE " + query.SqlQuery;
            }

            // Ajouter SELECT FROM si nécessaire
            if (string.IsNullOrWhiteSpace(query.SqlQuery) || !sqlUpper.Contains("SELECT "))
            {
                query.SqlQuery = "SELECT * FROM " + tableName + " " + (query.SqlQuery ?? string.Empty);
            }
        }

        /// <summary>
        /// Crée un message d'erreur détaillé avec contexte
        /// </summary>
        internal static string FormatError(string operation, string message, Exception innerException = null)
        {
            var error = $"{operation}: {message}";
            if (innerException != null)
                error += $" Détails: {innerException.Message}";
            return error;
        }
    }
}
