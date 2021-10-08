// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Liste de DataPO
    /// </summary>
    public class CollectionPO : CollectionPO<Nglib.DATA.DATAPO.DataPO> { }


    /// <summary>
    /// Liste de DataPo
    /// </summary>
    /// <typeparam name="Tpo"></typeparam>
    public class CollectionPO<Tpo> : List<Tpo>, System.Collections.IEnumerable, ICollectionPO where Tpo : DataPO,new()
    {
        /// <summary>
        /// Nombre total de resultats disponible sur le serveur
        /// </summary>
        public int TotalCountResults = 0;

        /// <summary>
        /// Temps d'execution pour obtenir ce résultat
        /// </summary>
        public long ExecuteTimeElapsed = 0;

        /// <summary>
        /// Table d'origine qui as permis d'obtenir les résultats
        /// </summary>
        private System.Data.DataTable orgnTable { get; set; }


        public CollectionPO() { }


        public CollectionPO(IEnumerable<Tpo> Origine)
            : base(Origine)
        {
        }

        public CollectionPO(System.Data.DataTable table)
        {
            this.LoadFromDataTable(table);
        }


        public Type GetPOType()
        {
            return typeof(Tpo);
        }


        public System.Data.DataTable UnifiedDataTable()
        {
            // !!! Optimiser les performances
            return null;

        }

        public System.Data.DataTable GetOriginalTable()
        {
            return this.orgnTable;
        }


        //        /// <summary>
        //        /// Fusionner les objet dans un seul datatable.
        //        /// Si nécessaire: Recréation d'un nouveau System.Data.DataTable et ajouter les row PO
        //        /// </summary>
        //        public System.Data.DataTable FusionDataTable(System.Data.DataTable TableOrgn=null)
        //{
        //            try
        //            {
        //                if (TableOrgn == null) TableOrgn = this[0].GetRow().Table.Clone();//new System.Data.DataTable();
        //                if (this.Count == 0) return TableOrgn;
        //                List<System.Data.DataTable> alltables = this.Select(dt => dt.GetRow().Table).Distinct().ToList();
        //                //System.Data.DataTable retour = CONNECTOR.CopyDbTools.MergeSchemas();



        //                if (string.IsNullOrEmpty(TableOrgn.TableName)) TableOrgn.TableName = this[0].GetRow().Table.TableName;
        //                foreach (var item in this)
        //                {
        //                    System.Data.DataRow irow = item.GetRow();
        //                    System.Data.DataTable itable = irow.Table.Clone(); // on la sépare dans le cas ou il aura plusieur enregistrement
        //                    itable.Rows.Add(irow.ItemArray);
        //                    TableOrgn.Merge(itable);
        //                    //retour.Rows.Add(item);
        //                }
        //                return TableOrgn;
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception("FusionDataTable "+ex.Message);
        //            }
        //        }





        public CollectionPO<Tpo> Pagination(int pagenumber, int numberOfPage)
        {
            // !!!
            return null;
        }








        /// <summary>
        /// Savoir si le résultat contient cette donnée
        /// </summary>
        /// <param name="ChampWant"></param>
        /// <param name="ChampValue"></param>
        /// <returns></returns>
        public bool AsValue(string ChampWant, params string[] ChampValue)
        {
            foreach (DataPO item in this)
                if (ChampValue.ToList().Contains(item.GetString(ChampWant))) return true;
            return false;
        }




        /// <summary>
        /// Extraction de données dans un dictionary 
        /// </summary>
        /// <param name="keyField">Nom du champ clef (supprimera les doublon)</param>
        /// <param name="valueField">Nom du champ valeur</param>
        /// <param name="valueDynamic">Utilisera la dynamisation, permet d'obtenir plusieurs champs</param>
        /// <returns></returns>
        public Dictionary<string, string> ToDictionaryString(string keyField, string valueField, bool valueDynamic= false)
        {
            Dictionary<string, string> indexedList = new Dictionary<string, string>();
            //indexedList.Add("", "");
            foreach (DataPO dataPO in this)
            {
                string value1 = dataPO.GetString(keyField);
                string value2 = (valueDynamic)?dataPO.DynamicValues(valueField): dataPO.GetString(valueField);
                if (!indexedList.ContainsKey(value1))
                        indexedList.Add(value1, value2);
            }
            return indexedList;
        }




        public void LoadFromDataTable(System.Data.DataTable table)
        {
            DataPOTools.LoadFromDataTable(this, table);
        }



    }
}
