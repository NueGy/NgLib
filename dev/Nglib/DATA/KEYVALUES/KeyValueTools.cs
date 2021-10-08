using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.KEYVALUES
{
    public static class KeyValueTools
    {






        public static object ConvertArrayToListRecursive(object obj) // !!! améliorer
        {
            if (obj == null || obj == DBNull.Value) return obj;
            Type objtype = obj.GetType();
            if (objtype.IsArray)
            {
                if (obj is object[])
                    return (obj as object[]).Select(subobj => ConvertArrayToListRecursive(subobj)).ToList();
                else if (obj is int[])
                    return (obj as int[]).ToList();
                else if (obj is int[])
                    return (obj as bool[]).ToList();
                else if (obj is int[])
                    return (obj as string[]).ToList();
                else if (obj is int[])
                    return (obj as double[]).ToList();
                else if (obj is int[])
                    return (obj as DateTime[]).ToList();
            }
            else if (obj is Dictionary<string, object>)
            {
                Dictionary<string, object> dic = obj as Dictionary<string, object>;
                return dic.ToDictionary(k => k.Key, v => ConvertArrayToListRecursive(v.Value));
            }

            return obj;
        }




        private static Type KeyValuesSerializerJsonType { get; set; }

        public static IKeyValuesSerializer SerializerFactory(bool isxml)
        {
            IKeyValuesSerializer retour = null;
            if (isxml)
            {
                retour = new KEYVALUES.KeyValuesSerializerXML();
            }
            else
            {
                retour = new KEYVALUES.KeyValuesSerializerJson();
                //if (DataValuesTools.DatavaluesSerializerJsonType == null) // la fonction ne peus pas être incluse dans le code car nécessite d'autre DLL (attendre .netcore 3 pour simplifier cette étape)
                //    DataValuesTools.DatavaluesSerializerJsonType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("Nglib.DATA.DATAVALUES.DatavaluesSerializerJson, Nglib.Extras");

                ////if (DataValuesTools.DatavaluesSerializerJsonType == null) // on prend un json dérivé
                ////    DataValuesTools.DatavaluesSerializerJsonType = typeof(DATAVALUES.DatavamueSerializerJsonLite);

                //if (DataValuesTools.DatavaluesSerializerJsonType == null)
                //    throw new Exception("DataValueSerializer For JSON not found. Please import Nglib.Extras.Dll AND Newtonsoft.Json");


                //retour = CODE.REFLEXION.ReflexionTools.NewInstance<IKeyValuesSerializer>(DataValuesTools.DatavaluesSerializerJsonType);
                //if (retour == null) throw new Exception("DatavaluesSerializerJsonType NewInstance error");
            }
            return retour;
        }






        public static string Serialize(KEYVALUES.KeyValues val,string type="sjon")
        {
            bool isxml = type.Equals("xml");
            IKeyValuesSerializer serialer = SerializerFactory(isxml);
            return serialer.Serialize(val);
        }
        public static KEYVALUES.KeyValues Deserialize(string val)
        {
            if (val == null) return null;
            if (string.IsNullOrWhiteSpace(val)) return new KeyValues();
            bool isxml = val.TrimStart().StartsWith("<");
            IKeyValuesSerializer serialer = SerializerFactory(isxml);
            return serialer.DeSerialize(val);
        }


    }
}
