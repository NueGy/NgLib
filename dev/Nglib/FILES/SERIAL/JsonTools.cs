using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nglib.FILES.SERIAL
{

    /// <summary>
    /// Utilitaires JSON
    /// </summary>
    public static class JsonTools
    {

        /// <summary>
        /// SerializeObject
        /// </summary>
        public static string Serialize(object value)
        {
            return JsonSerializer.Serialize(value);
        }
         

        /// <summary>
        /// Serialisation Simple sans utilisation de DLL exterieure
        /// </summary>
        [Obsolete("Use JSON.NET",false)]
        public static string SerializeDictionaryValues(Dictionary<string, object> datas)
        {
            List<string> entries = new List<string>();
            foreach (var item in datas)
            {
                if (item.Value == null) entries.Add(string.Format("\"{0}\":null", item.Key));
                else if (item.Value is bool) entries.Add(string.Format("\"{0}\":{1}", item.Key, item.Value.ToString().ToLower()));
                else if (item.Value is int || item.Value is long || item.Value is float || item.Value is double) entries.Add(string.Format("\"{0}\":{1}", item.Key, item.Value));
                else entries.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));
            }
            return "{" + string.Join(",", entries) + "}";
        }




    
        public static Dictionary<string, object> DeSerializeDictionaryValues(string json)
        {
            Dictionary<string, object> datas = new Dictionary<string, object>();
            if (string.IsNullOrWhiteSpace(json)) return datas;
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            foreach (var item in jsonElement.EnumerateObject())
            {
                if (item.Value.ValueKind == JsonValueKind.Null) datas.Add(item.Name, null);
                else if (item.Value.ValueKind == JsonValueKind.True) datas.Add(item.Name, true);
                else if (item.Value.ValueKind == JsonValueKind.False) datas.Add(item.Name, false);
                else if (item.Value.ValueKind == JsonValueKind.Number) datas.Add(item.Name, item.Value.GetDouble());
                else if (item.Value.ValueKind == JsonValueKind.String) datas.Add(item.Name, item.Value.GetString());
                else if (item.Value.ValueKind == JsonValueKind.Array) datas.Add(item.Name, item.Value.GetString());
                else if (item.Value.ValueKind == JsonValueKind.Object) datas.Add(item.Name, item.Value.GetString());
                else throw new Exception("Unknow JsonValueKind " + item.Value.ValueKind);
            }
            return datas;
        }
      


        public static string PrettyJson(string unPrettyJson)
        {
            if (string.IsNullOrWhiteSpace(unPrettyJson)) return null;
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(unPrettyJson);

            return JsonSerializer.Serialize(jsonElement, options);
        }


    }
}
