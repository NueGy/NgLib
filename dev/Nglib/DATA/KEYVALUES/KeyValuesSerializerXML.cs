using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValuesSerializerXML : IKeyValuesSerializer
    {
        public KeyValues DeSerialize(string fluxstring)
        {
            //content = ExchangeTools.CleanUpXml(fluxstring);
            KeyValues retour = new KeyValues();

            using (var reader = new System.IO.StringReader(fluxstring))
            {
                XDocument xmldoc = XDocument.Load(reader);
                xmldoc.Root.Descendants().Where(n => n.Parent == xmldoc.Root).ToList().ForEach(n => retour.Add(DeSerializeOne(n)));
               // retour.UnSerial(xmldoc.Root);
            }
            return retour;
        }

        public string Serialize(KeyValues values)
        {
            XElement root = new XElement("root");
            values.ForEach(v => root.Add(SerializeOne(v)));
            XDocument xdoc = new XDocument();
            xdoc.Add(root);
            string xmlcontent = xdoc.ToString();
            return xmlcontent;
        }


        public static XElement SerializeOne(Nglib.DATA.KEYVALUES.KeyValue sub)
        {
            XElement element = null;
            if (sub.Value == null) return new XElement(sub.Key, null);
            Type ValueType = sub.Value.GetType();

            if (sub.Value is Nglib.DATA.KEYVALUES.KeyValues)
            {
                Nglib.DATA.KEYVALUES.KeyValues sitm = sub.Value as Nglib.DATA.KEYVALUES.KeyValues;
                var subsElems = sitm.ToList().Select(itm => SerializeOne(itm));
                element = new XElement(sub.Key, null);
                element.Add(subsElems);
            }
            else if (sub.Value is Nglib.DATA.KEYVALUES.KeyValue[])
            {
                KeyValue[] sitm = sub.Value as KeyValue[];
                var subsElems = sitm.ToList().Select(itm => SerializeOne(itm));
                element = new XElement(sub.Key, null);
                element.Add(subsElems);
            }
            else
            {
                element = new XElement(sub.Key, sub.Value);
            }
            return element;
        }





        public static KeyValue DeSerializeOne(XElement node)
        {
            string nodename = node.Name.ToString().ToLower();
            KeyValue retour = null;
            if (node.HasElements)
            {
                KeyValues arrayvalues = new KeyValues();
                node.Descendants().ToList().ForEach(n => { var res = DeSerializeOne(n); if (res != null) arrayvalues.Add(res); });
                retour = new KeyValue(nodename, arrayvalues);
            }
            else
            {
                retour = new KeyValue(nodename, node.Value);
            }
            return retour;
        }




    }
}
