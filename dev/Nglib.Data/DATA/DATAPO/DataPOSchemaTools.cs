using Nglib.DATA.ACCESSORS;
using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Outils de gestion des schémas DataTable pour les objets DATAPO
    /// Thread-safe avec cache optimisé, Outils Fortement couplés avec le DataPO
    /// </summary>
    public static class DataPOSchemaTools
    {

        /// <summary>
        /// Cache thread-safe des structures de tables pour optimiser la création des DataPO
        /// Attention : Les DataRow ne doivent PAS être attachés aux tables du cache
        /// </summary>
        private static readonly ConcurrentDictionary<Type, DataTable> TablePoSchemasCache 
            = new ConcurrentDictionary<Type, DataTable>();






        /// <summary>
        /// Obtient le schéma d'un DataPO avec gestion du cache thread-safe
        /// </summary>
        /// <param name="potype">Type du DataPO</param>
        /// <param name="allowCache">Autoriser l'utilisation du cache (false pour DataPO de base)</param>
        /// <returns>DataTable représentant le schéma</returns>
        public static DataTable GetSchemaOnPO(Type potype, bool allowCache = true)
        {
            // Si c'est un DataPO pur, on ne peut pas utiliser le cache, car le type est trop générique
            if (potype == typeof(DataPO) || potype == typeof(IDataPO))
                allowCache = false;

            // Utiliser GetOrAdd pour une opération atomique thread-safe
            if (allowCache && TablePoSchemasCache.ContainsKey(potype))
            {
                TablePoSchemasCache.TryGetValue(potype, out DataTable schema);
                if (schema != null) return schema;
            }


            // Pas de cache : créer directement
            DataTable tableStd = CreateSchemaOnPO(potype);
            if (tableStd != null && allowCache && !TablePoSchemasCache.ContainsKey(potype))
                TablePoSchemasCache.TryAdd(potype, tableStd); // Ajoute au cache

            return tableStd;
        }


        /// <summary>
        /// Vide le cache des schémas (utile pour tests ou rechargement dynamique)
        /// </summary>
        public static void ClearSchemaCache()
        {
            TablePoSchemasCache.Clear();
        }

        /// <summary>
        /// Supprime le schéma d'un type spécifique du cache
        /// </summary>
        /// <param name="potype">Type à supprimer du cache</param>
        /// <returns>True si le type était présent dans le cache et a été supprimé, false sinon</returns>
        public static bool RemoveSchemaFromCache(Type potype)
        {
            return TablePoSchemasCache.TryRemove(potype, out _);
        }






        /// <summary>
        /// Crée le schéma d'un DataPO à partir de son Type (sans cache)
        /// Tente d'abord d'utiliser InitSchema() de l'instance, puis les attributs si échec
        /// </summary>
        /// <param name="potype">Type du DataPO à analyser</param>
        /// <returns>DataTable représentant le schéma, ou null si aucune définition trouvée</returns>
        private static System.Data.DataTable CreateSchemaOnPO(Type potype)
        {
            System.Data.DataTable tableStd = null;

            // La structure est defini dans une methode hérité et le datapo est instanciable new
            if (typeof(IDataPO).IsAssignableFrom(potype) && potype.GetConstructor(Type.EmptyTypes) != null)
            {
                // sinon on tente avec la prédéinition hérité, mais pour cela il faut instancier un objet (!!! gérer si constructeur non vide)
                IDataPO pofictif = APP.CODE.ReflectionTools.CreateInstance(potype) as IDataPO;
                tableStd = pofictif.CreateSchema();
            }
            // sinon on tente de voir si l'objet dispose d'attributs spécifiant sa structure
            if (tableStd == null)
                tableStd = CreateSchemaWithAttributes(potype);
            return tableStd;
        }



        /// <summary>
        /// Crée le schéma en analysant les attributs DataAnnotations d'un Type (DataPO ou Model)
        /// Analyse les attributs [Table], [Key], [Column], [DatabaseGenerated] pour construire le DataTable
        /// </summary>
        /// <param name="potype">Type du DataPO ou Model à analyser</param>
        /// <returns>DataTable avec le schéma complet (colonnes, clés primaires), ou null si pas d'attribut [Table]</returns>
        public static System.Data.DataTable CreateSchemaWithAttributes(Type potype)
        {
            System.Data.DataTable table = null;
            try
            {
                // ✅ Utilisation directe de TableAttribute
                var tableAttribute = potype.GetCustomAttribute<TableAttribute>();
                if (string.IsNullOrWhiteSpace(tableAttribute?.Name)) return null;
                string tableName = tableAttribute.Name;

                List<System.Data.DataColumn> Cols = new List<System.Data.DataColumn>();
                List<System.Data.DataColumn> ColKeys = new List<System.Data.DataColumn>();
                table = new DataTable();
                table.TableName = tableName;

                // Parcours des propriétés (cols)
                PropertyInfo[] props = potype.GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    // ✅ Récupération des attributs natifs
                    var keyAttribute = prop.GetCustomAttribute<KeyAttribute>();
                    var columnAttribute = prop.GetCustomAttribute<ColumnAttribute>();
                    var databaseGeneratedAttribute = prop.GetCustomAttribute<DatabaseGeneratedAttribute>();

                    // Ignorer les propriétés sans attributs [Key] ou [Column]
                    if (keyAttribute == null && columnAttribute == null)
                        continue;

                    // Créer la colonne
                    System.Data.DataColumn col = new DataColumn();
                    col.ColumnName = prop.Name.ToLower();
                    
                    // ✅ Définir le type de données avec gestion des nullables
                    Type propType = prop.PropertyType;
                    if (propType.ToString().Contains("ParamValuesPOFlux"))
                    {
                        // Les flux seront considérés comme des string
                        col.DataType = typeof(string);
                    }
                    else
                    {
                        // Convertir les types nullables en types non-nullables pour DataColumn
                        Type underlyingType = Nullable.GetUnderlyingType(propType);
                        col.DataType = underlyingType ?? propType;
                    }

                    // ✅ Appliquer le nom personnalisé de [Column]
                    if (columnAttribute != null && !string.IsNullOrWhiteSpace(columnAttribute.Name))
                    {
                        col.ColumnName = columnAttribute.Name;
                    }

                    // ✅ Appliquer [DatabaseGenerated]
                    if (databaseGeneratedAttribute != null && 
                        databaseGeneratedAttribute.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                    {
                        col.AutoIncrement = true;
                    }

                    // Ajouter la colonne à la liste
                    Cols.Add(col);

                    // ✅ Si c'est une clé, l'ajouter aussi à la liste des clés
                    if (keyAttribute != null)
                    {
                        ColKeys.Add(col);
                    }
                }

                // défini les colonnes sur la table
                DataSetTools.SetColumns(table, Cols.ToArray());
                DataSetTools.SetPrimaryKeys(table, ColKeys.ToArray());
                return table;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur dans CreateSchemaWithAttributes pour le type {potype?.Name}: {ex.Message}", ex);
            }
        }



         



    }
}
