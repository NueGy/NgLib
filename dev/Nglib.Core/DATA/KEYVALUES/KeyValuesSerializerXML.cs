using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValuesSerializerXML : IKeyValuesSerializer
    {
        public KeyValues DeSerialize(string fluxstring)
        {
            //content = ExchangeTools.CleanUpXml(fluxstring);
            var retour = new KeyValues();

            using (var reader = new StringReader(fluxstring))
            {
                var xmldoc = XDocument.Load(reader);
                xmldoc.Root.Descendants().Where(n => n.Parent == xmldoc.Root).ToList()
                    .ForEach(n => retour.Add(DeSerializeOne(n)));
                // retour.UnSerial(xmldoc.Root);
            }

            return retour;
        }

        public string Serialize(KeyValues values)
        {
            var root = new XElement("root");
            values.ForEach(v => root.Add(SerializeOne(v)));
            var xdoc = new XDocument();
            xdoc.Add(root);
            var xmlcontent = xdoc.ToString();
            return xmlcontent;
        }


        public static XElement SerializeOne(KeyValue sub)
        {
            XElement element = null;
            if (sub.Value == null) return new XElement(sub.Key, null);
            var ValueType = sub.Value.GetType();

            if (sub.Value is KeyValues)
            {
                var sitm = sub.Value as KeyValues;
                var subsElems = sitm.ToList().Select(itm => SerializeOne(itm));
                element = new XElement(sub.Key, null);
                element.Add(subsElems);
            }
            else if (sub.Value is KeyValue[])
            {
                var sitm = sub.Value as KeyValue[];
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
            var nodename = node.Name.ToString().ToLower();
            KeyValue retour = null;
            if (node.HasElements)
            {
                var arrayvalues = new KeyValues();
                node.Descendants().ToList().ForEach(n =>
                {
                    var res = DeSerializeOne(n);
                    if (res != null) arrayvalues.Add(res);
                });
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