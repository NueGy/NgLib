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
    public class CollectionPO : CollectionPO<Nglib.DATA.DATAPO.DataPO> 
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public CollectionPO() { }
        
        /// <summary>
        /// Constructeur à partir d'une DataTable
        /// </summary>
        public CollectionPO(System.Data.DataTable table) : base(table) { }
    }


    /// <summary>
    /// Liste de DataPo
    /// </summary>
    /// <typeparam name="Tpo">Type de DataPO contenu dans la collection</typeparam>
    public class CollectionPO<Tpo> : List<Tpo>, ICollectionPO where Tpo : DataPO, new()
    {
        /// <summary>
        /// Nombre total de resultats disponible sur le serveur
        /// </summary>
        public int TotalCount { get; set; } = 0;

        /// <summary>
        /// Temps d'execution pour obtenir ce résultat (en millisecondes)
        /// </summary>
        public long ExecuteTimeElapsed { get; set; } = 0;

        /// <summary>
        /// Table d'origine qui as permis d'obtenir les résultats
        /// </summary>
        private System.Data.DataTable orgnTable { get; set; }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public CollectionPO() : base()
        {
        }

        /// <summary>
        /// Constructeur à partir d'une collection existante
        /// </summary>
        public CollectionPO(IEnumerable<Tpo> origine) : base(origine)
        {
        }

        /// <summary>
        /// Constructeur à partir d'une DataTable
        /// </summary>
        public CollectionPO(System.Data.DataTable table)
        {
            this.LoadFromDataTable(table);
        }

        /// <summary>
        /// Obtient le type de DataPO contenu dans la collection
        /// </summary>
        public Type GetPOType()
        {
            return typeof(Tpo);
        }

        /// <summary>
        /// Charge la collection à partir d'une DataTable
        /// </summary>
        public void LoadFromDataTable(System.Data.DataTable table)
        {
            DataPOTools.LoadFromDataTable(this, table);
        }

        /// <summary>
        /// Convertit la collection vers un type de collection spécifique
        /// </summary>
        public TCollectionPO CastTo<TCollectionPO>() where TCollectionPO : CollectionPO<Tpo>, ICollectionPO, new()
        {
            if (this is TCollectionPO) return (TCollectionPO)this;
            TCollectionPO retour = new TCollectionPO();
            retour.AddRange(this);
            retour.TotalCount = this.TotalCount;
            retour.ExecuteTimeElapsed = this.ExecuteTimeElapsed;
            return retour;
        }

        /// <summary>
        /// Obtient la DataTable d'origine
        /// </summary>
        public System.Data.DataTable GetOriginalTable()
        {
            return this.orgnTable;
        }

        /// <summary>
        /// Savoir si le résultat contient cette donnée
        /// </summary>
        /// <param name="champWant">Nom du champ à rechercher</param>
        /// <param name="champValue">Valeur(s) recherchée(s)</param>
        public bool AsValue(string champWant, params string[] champValue)
        {
            foreach (DataPO item in this)
                if (champValue.ToList().Contains(item.GetString(champWant))) return true;
            return false;
        }

        /// <summary>
        /// Extraction de données dans un dictionary 
        /// </summary>
        /// <param name="keyField">Nom du champ clef (supprimera les doublon)</param>
        /// <param name="valueField">Nom du champ valeur</param>
        public Dictionary<string, string> ToDictionaryString(string keyField, string valueField)
        {
            Dictionary<string, string> indexedList = new Dictionary<string, string>();
            //indexedList.Add("", "");
            foreach (DataPO dataPO in this)
            {
                string value1 = dataPO.GetString(keyField);
                string value2 = dataPO.GetString(valueField);
                if (!indexedList.ContainsKey(value1))
                        indexedList.Add(value1, value2);
            }
            return indexedList;
        }

        /// <summary>
        /// Obtient la liste des DataPO
        /// </summary>
        public List<DataPO> GetPOList() => this.Cast<DataPO>().ToList();

    }
}
