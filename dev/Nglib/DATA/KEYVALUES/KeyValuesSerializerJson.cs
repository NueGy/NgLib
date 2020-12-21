using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValuesSerializerJson : IKeyValuesSerializer
    {
        public KeyValues DeSerialize(string fluxstring)
        {
            if (string.IsNullOrWhiteSpace(fluxstring)) return new KeyValues();
            System.Text.Json.JsonDocument jsondoc = System.Text.Json.JsonDocument.Parse(fluxstring);
            if (jsondoc.RootElement.ValueKind != JsonValueKind.Object) throw new Exception("Invalid Json : Object Root Type Required");
            KeyValues keyValues = new KeyValues();
            var vals = jsondoc.RootElement.EnumerateObject().ToList().SelectMany(elem => DeSerializeOne(elem)).ToList();
            keyValues.AddRange(vals);
            return keyValues;
        }




        public static KeyValue[] DeSerializeOne(System.Text.Json.JsonProperty elem)
        {

            string realname = elem.Name;
            if (realname.Equals("Lines", StringComparison.OrdinalIgnoreCase))
                ; // debug
            if (elem.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                KeyValue[] vals = elem.Value.EnumerateObject().ToList().SelectMany(v => DeSerializeOne(v)).ToArray();
                KeyValues valv = new KeyValues(); valv.AddRange(vals);
                return new KeyValue[] { new KeyValue(realname, valv) };
            }
            else if (elem.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var arrayvals = elem.Value.EnumerateArray().ToList();
                KeyValue[] vals = arrayvals.Select(v => new KeyValue(realname, ReadValue(v)) { IsMultiples=true } ).ToArray();
                return vals;
            }
            else
            {
                object val = ReadValue(elem.Value);
                return new KeyValue[] { new KeyValue(realname, val) };
            }

        }


        public static object ReadValue(System.Text.Json.JsonElement elem)
        {
            if (elem.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                KeyValue[] vals = elem.EnumerateObject().ToList().SelectMany(v => DeSerializeOne(v)).ToArray();
                KeyValues valv = new KeyValues(); valv.AddRange(vals);
                return valv;
            }
            else if (elem.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                // déja géré par DeSerializeOne
                //KeyValue[] vals = elem.Value.EnumerateArray().ToList().Select(v => DeSerializeOne(v)).ToArray();
                //KeyValues valv = new KeyValues(); valv.AddRange(vals);
                //retour = new KeyValue(realname, valv);
                return null;
            }
            else if (elem.ValueKind == System.Text.Json.JsonValueKind.Null || elem.ValueKind == System.Text.Json.JsonValueKind.Undefined)
            {
                return null;
            }
            else if (elem.ValueKind == System.Text.Json.JsonValueKind.False)
            {
                return false;
            }
            else if (elem.ValueKind == System.Text.Json.JsonValueKind.True)
            {
                return true;
            }
            else if (elem.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                int i = 0;
                if (elem.TryGetInt32(out i)) return i;

                long li = 0;
                if (elem.TryGetInt64(out li)) return li;

                double di = 0;
                if (elem.TryGetDouble(out di)) return di;

                return elem.GetRawText(); // !!!
            }
            else if (elem.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                return elem.GetString();
            }
            else
            {
                return elem.GetRawText();
            }
        }






        public static bool JsonWriterIgnoreNull = true;
        public static bool JsonWriterIndented = false;

        public string Serialize(KeyValues values)
        {
            if (values == null) return null;
            var options = new JsonWriterOptions { Indented = JsonWriterIndented };
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {

                    var dvalues = values.GroupKeys();
                    writer.WriteStartObject();
                    foreach (var item in dvalues)
                    {
                        writer.WritePropertyName(item.Key);
                        WriteValue(writer, item.Value);
                    }
                    writer.WriteEndObject();


                }
                string json = Encoding.UTF8.GetString(stream.ToArray());
                return json;
            }
        }





        public static void WriteValue(Utf8JsonWriter writer, object obj)
        {
            if (obj == null || obj == DBNull.Value) writer.WriteNullValue();
            else if(obj is KeyValues)
            {
                KeyValues subKeyValues = obj as KeyValues;
                var dvalues = subKeyValues.GroupKeys();
                writer.WriteStartObject();
                foreach (var item in dvalues)
                {
                    writer.WritePropertyName(item.Key);
                    WriteValue(writer, item.Value);
                }
                writer.WriteEndObject();
            }
            else if(obj.GetType().IsArray && obj is object[]) // utiliser Ienumerable ... :!!!!
            {
                object[] objs = obj as object[];
                writer.WriteStartArray();
                objs.ToList().ForEach(objin => WriteValue(writer, objin));
                writer.WriteEndArray();
            }
            else if (obj is bool) writer.WriteBooleanValue(((bool)obj));
            else if (obj is int) writer.WriteNumberValue(((int)obj));
            else if (obj is long) writer.WriteNumberValue(((long)obj));
            else if (obj is double) writer.WriteNumberValue(((double)obj));
            else
            {
                string val = Convert.ToString(obj);
                writer.WriteStringValue(val);
            }
        }



    }
}
