using Nglib.DATA.ACCESSORS;
using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Outil pour les objets DATAPO
    /// </summary>
    public static class DataPOTools
    {



        /// <summary>
        /// Création d'un nouvel objet
        /// </summary>
        public static T Create<T>(System.Data.DataRow row) where T : Nglib.DATA.DATAPO.DataPO, new()
        {
            if (row == null) return null;
            T retour = new T();
            retour.SetRow(row);
            return retour;
        }
        public static DataPO Create(System.Data.DataRow row) { return Create<DataPO>(row); }


        /// <summary>
        /// Création d'un nouvel objet (le premier de la table)
        /// </summary>
        public static T CreateFirst<T>(System.Data.DataTable table) where T : Nglib.DATA.DATAPO.DataPO, new()
        {
            if (table == null || table.Rows.Count < 1) return null;
            return Create<T>(table.Rows[0]);
        }
        public static DataPO CreateFirst(System.Data.DataTable table) { return CreateFirst<DataPO>(table); }









        /// <summary>
        /// Présence de changements
        /// </summary>
        /// <returns></returns>
        public static bool IsChanges(this DataPO item)
        {
            // On regarde si il y as eu un changement dans les flow
            if (item.flows != null && item.flows.Count(f => f.IsChanges()) > 0) return true;

            // on regarde si il y as eu un changement dans le datarow
            if (item.localRow == null) return false;
            else if (item.localRow.RowState.HasFlag(System.Data.DataRowState.Modified)) return true;
            else if (item.localRow.RowState.HasFlag(System.Data.DataRowState.Detached)) return true; // on considere que les datarow Detached nécesite un Insert donc ils nécessiteront un traitement
            else return false;
        }

        /// <summary>
        /// Obtenir les données du DataPO
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetValues(this DataPO po)
        {
            System.Data.DataRow row = po.GetRow(true);// Si les valeurs sont demandé on refresh les flow
            return DataSetTools.GetValues(row, null);
        }


        /// <summary>
        /// Obtenir les données des clefs et/ou les valeurs du DataPO
        /// Cela implique de mettre à jour le schéma du DataPO
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetKeyOrValues(this DataPO po, bool includeKey, bool includeValues)
        {
            System.Data.DataRow row = po.GetRow(!includeValues);// Si les valeurs sont demandé on refresh les flow
            if (!po.IsDefinedSchema()) po.DefineSchemaPO();// throw new Exception("GetValues returnKeys require a defined schema on the PO");
            return DataSetTools.GetValues(row, includeKey, includeValues);
        }




        /// <summary>
        /// Obtientir les données du DataPO
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetValues(this DataPO po, params string[] ColNames)
        {
            if (ColNames == null || ColNames.Length == 0) return new Dictionary<string, object>();
            bool refreshflow = false;
            if(po.flows!=null && po.flows.Count>0)
                refreshflow = po.flows.Select(f => f.GetFieldName()).Where(fn => !string.IsNullOrWhiteSpace(fn)).Any(fn => ColNames.Any(eq => fn.Equals(eq, StringComparison.OrdinalIgnoreCase)));
            
            System.Data.DataRow row = po.GetRow(refreshflow);
            return DataSetTools.GetValues(row, ColNames);
        }

        public static Dictionary<string, object> GetChangedValues(this DataPO po)
        {
            bool refreshflow = (po.flows != null && po.flows.Count(f => f.IsChanges()) > 0);
            System.Data.DataRow row = po.GetRow(refreshflow);
            return DataSetTools.GetChangedValues(row);
        }


        /// <summary>
        /// Définit les valeur dans le datarow dans l'objet
        /// </summary>
        /// <param name="po">L'objet de données</param>
        /// <param name="DicDataRow">Dictionnaire des valeurs</param>
        public static void SetValues(this DataPO po, Dictionary<string, object> DicDataRow)
        {
            try
            {
                foreach (var item in DicDataRow)
                    po.SetData(item.Key, item.Value, DataAccessorOptionEnum.Default);
                //po._isLoaded = false; // N'a pas été chargé depuis une base de données !!! voir comment on fait
            }
            catch (Exception ex)
            {
                throw new Exception("SetValues " + ex.Message, ex);
            }
        }



        /// <summary>
        /// Permet de clonner les données des datarow en une seule datatable
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static System.Data.DataTable CloneDataTable(params DATAPO.DataPO[] datas)
        {
            
            try
            {
                List<System.Data.DataTable> alltables = datas.Select(dt => dt.GetRow().Table).Distinct().ToList();
                System.Data.DataTable tabinsert = DataSetTools.DataTableMergeSchemas(alltables.ToArray());
                DataSetTools.DataTableRemoveConstraints(tabinsert);
                foreach (var itemdata in datas)
                {
                    System.Data.DataRow newrow = tabinsert.NewRow();
                    System.Data.DataRow oldrow = itemdata.GetRow(true);
                    oldrow.CopyRow(newrow);
                    tabinsert.Rows.Add(newrow);
                }
                return tabinsert;
            }
            catch (Exception ex)
            {
                throw new Exception("CloneDataTable " + ex.Message);
            }
        }






        /// <summary>
        /// Permet de charger une table dans une liste de PO
        /// </summary>
        /// <typeparam name="Tpo"></typeparam>
        /// <param name="listPO"></param>
        /// <param name="table"></param>
        public static void LoadFromDataTable<Tpo>(this IList<Tpo> listPO, System.Data.DataTable table) where Tpo : DataPO, new()
        {
          
            try
            {
                int iiadd = 0;
                foreach (System.Data.DataRow row in table.Rows)
                {
                    Tpo ee = new Tpo();// Specifique, peut pas utiliser le constructeur classique
                    ee.SetRow(row);
                    iiadd++;
                    listPO.Add(ee);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static IList<Tpo> LoadFromDataTable<Tpo>(System.Data.DataTable table) where Tpo : DataPO, new()
        {
            List<Tpo> retour = new List<Tpo>();
            LoadFromDataTable<Tpo>(retour, table);
            return retour;
        }




        /// <summary>
        /// Obtenir le schema d'un PO
        /// Retrocompatibilite
        /// </summary>
        /// <param name="potype"></param>
        /// <returns></returns>
        public static System.Data.DataTable GetSchemaOnPO(Type potype)
            => DataPOSchemaTools.GetSchemaOnPO(potype);



    }
}
