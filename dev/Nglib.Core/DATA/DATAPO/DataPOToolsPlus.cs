using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.DATAPO
{
    public  static class DataPOToolsPlus
    {


        /// <summary>
        /// Extraction de données dans un dictionary 
        /// </summary>
        /// <param name="keyField">Nom du champ clef (supprimera les doublon)</param>
        /// <param name="valueField">Nom du champ valeur</param>
        /// <param name="valueDynamic">Utilisera la dynamisation, permet d'obtenir plusieurs champs</param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDynamicDictionaryString(this DATAPO.ICollectionPO item, string keyField, string valueField)
        {
            Dictionary<string, string> indexedList = new Dictionary<string, string>();
            //indexedList.Add("", "");
            foreach (DataPO dataPO in item.GetPOList())
            {
                string value1 = dataPO.GetString(keyField);
                string value2 = dataPO.DynamicValues(valueField);
                if (!indexedList.ContainsKey(value1))
                    indexedList.Add(value1, value2);
            }
            return indexedList;
        }

    }
}
