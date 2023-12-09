using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Nglib.DATA.COLLECTIONS;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValuesSerializerJson : IKeyValuesSerializer
    {
        public static bool JsonWriterIgnoreNull = true;
        public static bool JsonWriterIndented = false;

        public KeyValues DeSerialize(string fluxstring)
        {
            return DeSerialize<KeyValues>(fluxstring);
        }

        public string Serialize(KeyValues values)
        {
            if (values == null) return null;
            var options = new JsonWriterOptions { Indented = JsonWriterIndented };
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    var dvalues = values.GroupKeys();

                    //Mode array (On commence direct par un array)
                    if (dvalues.Count == 1 && string.IsNullOrEmpty(dvalues.FirstOrDefault().Key))
                    {
                        WriteValue(writer, dvalues.FirstOrDefault().Value, true);
                    }
                    else if (dvalues.Count > 0) //Mode normal Object
                    {
                        writer.WriteStartObject();
                        foreach (var item in dvalues)
                        {
                            writer.WritePropertyName(item.Key);
                            WriteValue(writer, item.Value);
                        }

                        writer.WriteEndObject();
                    }
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());
                return json;
            }
        }


        public T DeSerialize<T>(string fluxstring) where T : new()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fluxstring)) return new T();
                fluxstring = fluxstring.Trim();
                if (new T() as KeyValues == null) return default; // Vérification du cast possible
                var jsondoc = JsonDocument.Parse(fluxstring);


                var aditem = new T();
                var keyValues = aditem as KeyValues;
                List<KeyValue> vals = null;
                if (jsondoc.RootElement.ValueKind == JsonValueKind.Object) // mode normal
                {
                    vals = jsondoc.RootElement.EnumerateObject().ToList().SelectMany(elem => DeSerializeSub(elem))
                        .ToList();
                }
                else if (jsondoc.RootElement.ValueKind == JsonValueKind.Array) // mode array direct
                {
                    var arrayvals = jsondoc.RootElement.EnumerateArray().ToList();
                    var valsret = arrayvals.Select(v => new KeyValue("", ReadValue(v)) { IsMultiples = true })
                        .ToArray();
                    vals = valsret.ToList();
                }
                else
                {
                    throw new Exception("Invalid Json : Object Root Type Required");
                }


                keyValues.AddRange(vals);
                return aditem;
            }
            catch (Exception ex)
            {
                throw new Exception("DeSerialize " + ex.Message, ex);
            }
        }


        public ListResult<KeyValues> DeSerializeList(string fluxstring)
        {
            return DeSerializeList<KeyValues>(fluxstring);
        }


        public ListResult<T> DeSerializeList<T>(string fluxstring) where T : new()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fluxstring)) return null;
                if (new T() as KeyValues == null) return null; // Vérification du cast possible
                var retour = new ListResult<T>();
                var jsondoc = JsonDocument.Parse(fluxstring);
                if (jsondoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    // une liste simple
                    var liste = jsondoc.RootElement.EnumerateArray();
                    liste.ToList().ForEach(noderoot =>
                    {
                        if (noderoot.ValueKind == JsonValueKind.Object)
                        {
                            var aditem = new T();
                            var keyValues = aditem as KeyValues;
                            keyValues.AddRange(noderoot.EnumerateObject().ToList()
                                .SelectMany(elem => DeSerializeSub(elem)).ToList());
                            retour.Add(aditem);
                        }
                    });
                }
                else if (jsondoc.RootElement.ValueKind == JsonValueKind.Object
                         && jsondoc.RootElement.EnumerateObject().Select(os => os.Name).Any(osname =>
                             osname.Equals("data", StringComparison.OrdinalIgnoreCase)))
                {
                    // Objet liste complet
                    var liste = jsondoc.RootElement.EnumerateObject()
                        .FirstOrDefault(osn => osn.Name.Equals("data", StringComparison.OrdinalIgnoreCase)).Value
                        .EnumerateArray();

                    liste.ToList().ForEach(noderoot =>
                    {
                        if (noderoot.ValueKind == JsonValueKind.Object)
                        {
                            var aditem = new T();
                            var keyValues = aditem as KeyValues;
                            keyValues.AddRange(noderoot.EnumerateObject().ToList()
                                .SelectMany(elem => DeSerializeSub(elem)).ToList());
                            retour.Add(aditem);
                        }
                    });
                }
                else if (jsondoc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // Ce n'est pas une liste
                    var aditem = new T();
                    var keyValues = aditem as KeyValues;
                    keyValues.AddRange(jsondoc.RootElement.EnumerateObject().ToList()
                        .SelectMany(elem => DeSerializeSub(elem)).ToList());
                    retour.Add(aditem);
                }
                else
                {
                    throw new Exception("Invalid Json : Object/Array Root Type Required");
                }


                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("KeyValues DeSerializeList " + ex.Message, ex);
            }
        }


        public static T DeSerializeItem<T>(string fluxstring) where T : KeyValues, new()
        {
            var serializerJson = new KeyValuesSerializerJson();
            return serializerJson.DeSerialize<T>(fluxstring);
        }

        public static KeyValue[] DeSerializeSub(JsonProperty elem)
        {
            var realname = elem.Name;
            if (realname.Equals("Lines", StringComparison.OrdinalIgnoreCase))
                ; // debug
            if (elem.Value.ValueKind == JsonValueKind.Object)
            {
                var vals = elem.Value.EnumerateObject().ToList().SelectMany(v => DeSerializeSub(v)).ToArray();
                var valv = new KeyValues();
                valv.AddRange(vals);
                return new KeyValue[] { new(realname, valv) };
            }

            if (elem.Value.ValueKind == JsonValueKind.Array)
            {
                var arrayvals = elem.Value.EnumerateArray().ToList();
                var vals = arrayvals.Select(v => new KeyValue(realname, ReadValue(v)) { IsMultiples = true }).ToArray();
                return vals;
            }

            var val = ReadValue(elem.Value);
            return new KeyValue[] { new(realname, val) };
        }


        public static object ReadValue(JsonElement elem)
        {
            if (elem.ValueKind == JsonValueKind.Object)
            {
                var vals = elem.EnumerateObject().ToList().SelectMany(v => DeSerializeSub(v)).ToArray();
                var valv = new KeyValues();
                valv.AddRange(vals);
                return valv;
            }

            if (elem.ValueKind == JsonValueKind.Array)
                // déja géré par DeSerializeOne
                //KeyValue[] vals = elem.Value.EnumerateArray().ToList().Select(v => DeSerializeOne(v)).ToArray();
                //KeyValues valv = new KeyValues(); valv.AddRange(vals);
                //retour = new KeyValue(realname, valv);
                return null;

            if (elem.ValueKind == JsonValueKind.Null || elem.ValueKind == JsonValueKind.Undefined) return null;

            if (elem.ValueKind == JsonValueKind.False) return false;

            if (elem.ValueKind == JsonValueKind.True) return true;

            if (elem.ValueKind == JsonValueKind.Number)
            {
                var i = 0;
                if (elem.TryGetInt32(out i)) return i;

                long li = 0;
                if (elem.TryGetInt64(out li)) return li;

                double di = 0;
                if (elem.TryGetDouble(out di)) return di;

                return elem.GetRawText(); // !!!
            }

            if (elem.ValueKind == JsonValueKind.String)
                return elem.GetString();
            return elem.GetRawText();
        }

        public string SerializeList(ListResult<KeyValues> values)
        {
            return SerializeList<KeyValues>(values);
        }

        public string SerializeList<T>(ListResult<T> values) where T : KeyValues, new()
        {
            if (values == null || values.data == null) return null;
            var options = new JsonWriterOptions { Indented = JsonWriterIndented };
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("data");
                    writer.WriteStartArray();
                    foreach (var itemValues in values.data)
                    {
                        var dvalues = itemValues.GroupKeys();
                        writer.WriteStartObject();
                        foreach (var item in dvalues)
                        {
                            writer.WritePropertyName(item.Key);
                            WriteValue(writer, item.Value);
                        }

                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());
                return json;
            }
        }


        public static void WriteValue(Utf8JsonWriter writer, object obj, bool forcemultiple = false)
        {
            if (obj == null || obj == DBNull.Value)
            {
                writer.WriteNullValue();
            }
            else if (obj.GetType().IsArray && obj is object[]) // utiliser Ienumerable ... :!!!!
            {
                var objs = obj as object[];
                writer.WriteStartArray();
                objs.ToList().ForEach(objin => WriteValue(writer, objin));
                writer.WriteEndArray();
            }
            else if (forcemultiple)
            {
                writer.WriteStartArray();
                WriteValue(writer, obj);
                writer.WriteEndArray();
            }
            else if (obj is KeyValues)
            {
                var subKeyValues = obj as KeyValues;
                var dvalues = subKeyValues.GroupKeys();
                writer.WriteStartObject();
                foreach (var item in dvalues)
                {
                    writer.WritePropertyName(item.Key);
                    WriteValue(writer, item.Value);
                }

                writer.WriteEndObject();
            }
            else if (obj is bool)
            {
                writer.WriteBooleanValue((bool)obj);
            }
            else if (obj is int)
            {
                writer.WriteNumberValue((int)obj);
            }
            else if (obj is long)
            {
                writer.WriteNumberValue((long)obj);
            }
            else if (obj is double)
            {
                writer.WriteNumberValue((double)obj);
            }
            else if (obj is DateTime)
            {
                writer.WriteStringValue((DateTime)obj);
            }
            else if (obj is DateTime? && obj != null)
            {
                writer.WriteStringValue(((DateTime?)obj).Value);
            }
            else
            {
                var val = Convert.ToString(obj);
                writer.WriteStringValue(val);
            }
        }
    }
}