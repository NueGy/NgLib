using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using Nglib.DATA.ACCESSORS;
using System.Text.Json;

namespace Nglib.DATA.PARAMVALUES
{

    public class ParamValuesSerializerJson :IParamValuesSerializer
    {
        

        public ParamValuesSerializerJson()
        {

        }




        public ParamValues DeSerialize(string fluxstring, ParamValues retour = null)
        {
            try
            {
                if (retour == null) retour = new ParamValues();
                System.Text.Json.JsonDocument jsondoc = System.Text.Json.JsonDocument.Parse(fluxstring);
                if (jsondoc.RootElement.ValueKind != JsonValueKind.Object) throw new Exception("Invalid Json : Object Root Type Required");
                //Newtonsoft.Json.Linq.JObject jobject = Newtonsoft.Json.Linq.JObject.Parse(fluxstring);
                foreach (var joitem in jsondoc.RootElement.EnumerateObject().ToList())
                    FromJsonSub(retour, joitem,"/param/");

                retour.AcceptChanges(); // on retire les marqueurs de changements
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("FromFluxJson " + ex.Message, ex);
            }
        }


        private void FromJsonSub(ParamValues datavalue, System.Text.Json.JsonProperty elem, string lastpath)
        {
            if (elem.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                // Array invalids
            }
            else if (elem.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                string newlastpath = lastpath + elem.Name + "/";
                foreach (var jsubitem in elem.Value.EnumerateObject().ToList())
                    FromJsonSub(datavalue,jsubitem, newlastpath);
                
            }
            else
            {
                string fullname = lastpath+elem.Name;
                ParamValuesNode dataelement = new ParamValuesNode();
                dataelement.Name = fullname;
                dataelement.Value = Nglib.DATA.KEYVALUES.KeyValuesSerializerJson.ReadValue(elem.Value);
                datavalue.Add(dataelement);
            }


        }



        public static bool ConfigIndented = false;

        public string Serialize(ParamValues datavalue)
        {
            try
            {
                ParamValuesNodeHierarchical firstnode = ParamValuesTools.GetHierarchicalNodes(datavalue.GetList());
                //Newtonsoft.Json.Linq.JToken jobject = Serialize_Recursif(firstnode,true);

                //Newtonsoft.Json.Formatting outjsonformating = Newtonsoft.Json.Formatting.None;
                //string jsontstr = jobject.ToString(outjsonformating);
                ////jsontstr = jsontstr.Replace("\r\n", "");//.Replace("\"", "");
                ////jsontstr = System.Text.RegularExpressions.Regex.Unescape(jsontstr);
                //return jsontstr;

                var options = new JsonWriterOptions() { Indented = ConfigIndented };
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, options))
                    {
                        // on écrit pas le premier noeud param 
                        writer.WriteStartObject();
                        foreach (var subnode in firstnode.ChildrenNodes)
                            Serialize_Recursif(writer, subnode);
                        writer.WriteEndObject();
                    }
                    string json = Encoding.UTF8.GetString(stream.ToArray());
                    return json;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("SerializeJson "+ex.Message,ex);
            }
        }


        private void Serialize_Recursif(Utf8JsonWriter writer, ParamValuesNodeHierarchical node)
        {
            if (node == null) return;
            if (node.ValueNode != null) // Valeur FINALE
            {   //Ajoute les valeurs
                object value = node.ValueNode.Value;
                string name = node.NodeName;
                writer.WritePropertyName(name);
                Nglib.DATA.KEYVALUES.KeyValuesSerializerJson.WriteValue(writer, value);
            }
            else if (node.ChildrenNodes != null && node.ChildrenNodes.Count > 0)
            {   // Ajoute un nouvel objet
                writer.WritePropertyName(node.NodeName);
                writer.WriteStartObject();
                foreach (var subnode in node.ChildrenNodes)
                    Serialize_Recursif(writer, subnode);
                writer.WriteEndObject();
            }

        }




    }
}
