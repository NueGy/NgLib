using System;
using System.Collections.Generic;
using System.Linq;

namespace Nglib.DATA.KEYVALUES
{
    public static class KeyValueTools
    {
        private static Type KeyValuesSerializerJsonType { get; set; }


        public static string ConcatKey(string key1, string key2)
        {
            if (key2.StartsWith(key1)) return key2;
            return $"{key1?.TrimEnd('/')}/{key2}";
        }


        public static object ConvertArrayToListRecursive(object obj) // !!! améliorer
        {
            if (obj == null || obj == DBNull.Value) return obj;
            var objtype = obj.GetType();
            if (objtype.IsArray)
            {
                if (obj is object[])
                    return (obj as object[]).Select(subobj => ConvertArrayToListRecursive(subobj)).ToList();
                if (obj is int[])
                    return (obj as int[]).ToList();
                if (obj is int[])
                    return (obj as bool[]).ToList();
                if (obj is int[])
                    return (obj as string[]).ToList();
                if (obj is int[])
                    return (obj as double[]).ToList();
                if (obj is int[])
                    return (obj as DateTime[]).ToList();
            }
            else if (obj is Dictionary<string, object>)
            {
                var dic = obj as Dictionary<string, object>;
                return dic.ToDictionary(k => k.Key, v => ConvertArrayToListRecursive(v.Value));
            }

            return obj;
        }

        public static IKeyValuesSerializer SerializerFactory(bool isxml)
        {
            IKeyValuesSerializer retour = null;
            if (isxml)
                throw new Exception("KeyValues SerializerFactory XML not supported");
                //retour = new KeyValuesSerializerXML();
            else
                retour = new KeyValuesSerializerJson();
            //if (DataValuesTools.DatavaluesSerializerJsonType == null) // la fonction ne peus pas être incluse dans le code car nécessite d'autre DLL (attendre .netcore 3 pour simplifier cette étape)
            //    DataValuesTools.DatavaluesSerializerJsonType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("Nglib.DATA.DATAVALUES.DatavaluesSerializerJson, Nglib.Extras");
            ////if (DataValuesTools.DatavaluesSerializerJsonType == null) // on prend un json dérivé
            ////    DataValuesTools.DatavaluesSerializerJsonType = typeof(DATAVALUES.DatavamueSerializerJsonLite);
            //if (DataValuesTools.DatavaluesSerializerJsonType == null)
            //    throw new Exception("DataValueSerializer For JSON not found. Please import Nglib.Extras.Dll AND Newtonsoft.Json");
            //retour = CODE.REFLEXION.ReflexionTools.NewInstance<IKeyValuesSerializer>(DataValuesTools.DatavaluesSerializerJsonType);
            //if (retour == null) throw new Exception("DatavaluesSerializerJsonType NewInstance error");
            return retour;
        }


        public static string Serialize(KeyValues val, string type = "sjon")
        {
            var isxml = type.Equals("xml");
            var serialer = SerializerFactory(isxml);
            return serialer.Serialize(val);
        }

        public static KeyValues Deserialize(string val)
        {
            if (val == null) return null;
            if (string.IsNullOrWhiteSpace(val)) return new KeyValues();
            var isxml = val.TrimStart().StartsWith("<");
            var serialer = SerializerFactory(isxml);
            return serialer.DeSerialize(val);
        }
    }
}