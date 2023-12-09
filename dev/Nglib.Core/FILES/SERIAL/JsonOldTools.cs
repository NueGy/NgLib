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
    public static class JsonOldTools
    {

        private static System.Reflection.MethodInfo TypeSerializeObject = null;
        private static System.Reflection.MethodInfo TypeDeserializeObject = null;


        public static bool UseNewtonsoft = false;
        public static bool UseSystemTextJson = true;
        public static bool UseDataContractJson = true;





        /// <summary>
        /// Permet de lancer la sérialisation d'un objet Meme si absence de DLL System.Text.Json
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("Obsolete depuis le passage sur .net5")]
        public static string Serialize(object value)
        {
            try
            {
                if (value == null) return null;
                Type valueType = value.GetType();

                if (TypeSerializeObject == null && UseSystemTextJson)
                {
                    //Newtonsoft.Json.JsonConvert, Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed
                    Type tjsonconvert = Nglib.APP.CODE.ReflectionTools.GetTypeByReflexion("System.Text.Json.JsonSerializer, System.Text.Json");
                    if (tjsonconvert != null)
                        TypeSerializeObject = tjsonconvert.GetMethods().FirstOrDefault(m => m.ToString().Equals("System.String Serialize(System.Object, System.Type, System.Text.Json.JsonSerializerOptions)"));
                }
                if (TypeSerializeObject == null && UseNewtonsoft)
                {
                    //Newtonsoft.Json.JsonConvert, Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed
                    Type tjsonconvert = Nglib.APP.CODE.ReflectionTools.GetTypeByReflexion("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
                    if (tjsonconvert != null)
                        TypeSerializeObject = tjsonconvert.GetMethods().FirstOrDefault(m => m.ToString().Equals("System.String SerializeObject(System.Object)"));
                }
                if (TypeSerializeObject == null && UseDataContractJson)
                {
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(valueType);
                    using (var stream = new System.IO.MemoryStream())
                    {
                        serializer.WriteObject(stream, value);
                        return Encoding.Default.GetString(stream.ToArray());
                    }
                }

                if (TypeSerializeObject == null) return null;
                return Convert.ToString(TypeSerializeObject.Invoke(null, new object[] { value, valueType, null }));


            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Obsolete("Obsolete depuis le passage sur .net5")]
        public static Tobj Deserialize<Tobj>(string json) where Tobj : class
        {
            try
            {
                if (TypeDeserializeObject == null && UseSystemTextJson)
                {
                    //{System.Object Deserialize(System.String, System.Type, System.Text.Json.JsonSerializerOptions)}
                    Type tjsonconvert = Nglib.APP.CODE.ReflectionTools.GetTypeByReflexion("System.Text.Json.JsonSerializer, System.Text.Json");
                    if (tjsonconvert != null)
                        TypeDeserializeObject = tjsonconvert.GetMethods().FirstOrDefault(m => m.ToString().Equals("System.Object Deserialize(System.String, System.Type, System.Text.Json.JsonSerializerOptions)"));
                }
                if (TypeDeserializeObject == null && UseNewtonsoft)
                {
                    //Newtonsoft.Json.JsonConvert, Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed
                    Type tjsonconvert = Nglib.APP.CODE.ReflectionTools.GetTypeByReflexion("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
                    if (tjsonconvert != null)
                        TypeDeserializeObject = tjsonconvert.GetMethods().FirstOrDefault(m => m.ToString().Equals("System.Object DeserializeObject(System.String, System.Type)"));
                }
                if (TypeDeserializeObject == null && UseDataContractJson)
                {
                    using (var stream = new System.IO.MemoryStream(Encoding.Default.GetBytes(json)))
                    {
                        var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Tobj));
                        return serializer.ReadObject(stream) as Tobj;
                    }
                }

                if (TypeDeserializeObject == null) return default(Tobj);
                Type tretour = typeof(Tobj);
                object val = TypeDeserializeObject.Invoke(null, new object[] { json, tretour,null });
                return (Tobj)val;

            }
            catch (Exception)
            {

                throw;
            }
        }







 


        ///// <summary>
        ///// Serializes an object to JSON (DataContractJsonSerializer)
        ///// </summary>
        //public static string Serialize<TType>(TType instance) where TType : class
        //    {
        //        var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(TType));
        //        using (var stream = new System.IO.MemoryStream())
        //        {
        //            serializer.WriteObject(stream, instance);
        //            return Encoding.Default.GetString(stream.ToArray());
        //        }
        //    }

        ///// <summary>
        ///// DeSerializes an object from JSON (DataContractJsonSerializer)
        ///// </summary>
        //public static TType DeSerialize<TType>(string json) where TType : class
        //{
        //    using (var stream = new System.IO.MemoryStream(Encoding.Default.GetBytes(json)))
        //    {
        //        var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(TType));
        //        return serializer.ReadObject(stream) as TType;
        //    }
        //}



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








        [Obsolete("Use JSON.NET",false)]
        public static Dictionary<string, object> DeSerializeDictionaryValues(string json)
        {
            int end;
            return DeSerializeDictionaryValues(json, 0, out end);
        }
        private static Dictionary<string, object> DeSerializeDictionaryValues(string json, int start, out int end)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            bool escbegin = false;
            bool escend = false;
            bool inquotes = false;
            string key = null;
            int cend;
            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> child = null;
            List<object> arraylist = null;
            Regex regex = new Regex(@"\\u([0-9a-z]{4})", RegexOptions.IgnoreCase);
            int autoKey = 0;
            for (int i = start; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '\\') escbegin = !escbegin;
                if (!escbegin)
                {
                    if (c == '"')
                    {
                        inquotes = !inquotes;
                        if (!inquotes && arraylist != null)
                        {
                            arraylist.Add(DecodeString(regex, sb.ToString()));
                            sb.Length = 0;
                        }
                        continue;
                    }
                    if (!inquotes)
                    {
                        switch (c)
                        {
                            case '{':
                                if (i != start)
                                {
                                    child = DeSerializeDictionaryValues(json, i, out cend);
                                    if (arraylist != null) arraylist.Add(child);
                                    else
                                    {
                                        dict.Add(key, child);
                                        key = null;
                                    }
                                    i = cend;
                                }
                                continue;
                            case '}':
                                end = i;
                                if (key != null)
                                {
                                    if (arraylist != null) dict.Add(key, arraylist);
                                    else dict.Add(key, DecodeString(regex, sb.ToString()));
                                }
                                return dict;
                            case '[':
                                arraylist = new List<object>();
                                continue;
                            case ']':
                                if (key == null)
                                {
                                    key = "array" + autoKey.ToString();
                                    autoKey++;
                                }
                                if (arraylist != null && sb.Length > 0)
                                {
                                    arraylist.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                                dict.Add(key, arraylist);
                                arraylist = null;
                                key = null;
                                continue;
                            case ',':
                                if (arraylist == null && key != null)
                                {
                                    dict.Add(key, DecodeString(regex, sb.ToString()));
                                    key = null;
                                    sb.Length = 0;
                                }
                                if (arraylist != null && sb.Length > 0)
                                {
                                    arraylist.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                                continue;
                            case ':':
                                key = DecodeString(regex, sb.ToString());
                                sb.Length = 0;
                                continue;
                        }
                    }
                }
                sb.Append(c);
                if (escend) escbegin = false;
                if (escbegin) escend = true;
                else escend = false;
            }
            end = json.Length - 1;
            return dict; //theoretically shouldn't ever get here
        }
        private static string DecodeString(Regex regex, string str)
        {
            return Regex.Unescape(regex.Replace(str, match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber))));
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
