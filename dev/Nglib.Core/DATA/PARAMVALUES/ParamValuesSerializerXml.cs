using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.PARAMVALUES
{

    public class ParamValuesSerializerXml : IParamValuesSerializer
    {



        public ParamValuesSerializerXml()
        {
            //System.Text.j
        }



        
        /// <summary>
        /// Enregistre la liste dans un fichier config xml
        /// </summary>
        /// <param name="fichierXML"></param>
        public void SerializeToFile(ParamValues datavalue, string fichierXML)
        {
            try
            {
                // vérif et convert
                ParamValues dvforxml = datavalue.Clone();
                foreach (ParamValuesNode itemd in dvforxml.GetList())
                {
                    if (itemd.Name.Length > 0 && itemd.Name[0] != '/') itemd.Name = "/noparam/" + itemd.Name;
                }

                XmlTextWriter XmlTextWriter = new XmlTextWriter(fichierXML, System.Text.Encoding.UTF8);
                XmlTextWriter.Formatting = Formatting.Indented;

                XmlTextWriter.WriteStartDocument();

                XmlTextWriter = savewriteburn(XmlTextWriter, dvforxml);

                XmlTextWriter.Flush(); //vide le buffer
                XmlTextWriter.Close(); // ferme le document
            }
            finally
            {

            }
        }


        /// <summary>
        /// Enregistre la liste dans un fichier config xml
        /// </summary>
        /// <param name="fichierXML"></param>
        public string Serialize(ParamValues datavalue)
        {
            string fluxxml = ""; //chaine xml final
            if (datavalue.Count() == 0) return "<?xml version=\"1.0\" ?><param><empty>True</empty></param>";


            ParamValues dvforxml = null; // datavalue buffer
            try
            {
                try
                {
                    // vérif et convert
                    dvforxml = datavalue.Clone();
                    try { dvforxml.Sort(); } catch { }
                    foreach (ParamValuesNode itemd in dvforxml.GetList())
                    {
                        if (itemd.Name.Length > 0 && itemd.Name[0] != '/') itemd.Name = "/param/" + itemd.Name;
                    }
                }
                catch (Exception ev)
                {
                    throw new Exception("Verif : " + ev.Message);
                }

                StringBuilder builder = new StringBuilder();
                using (StringWriter stringWriter = new StringWriter(builder))
                {
                    XmlTextWriter writer = new XmlTextWriter(stringWriter);

                    // This produces UTF16 XML
                    writer.Indentation = 1;
                    writer.IndentChar = '\t';
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartDocument();
                    writer = savewriteburn(writer, dvforxml);

                    writer.WriteEndDocument();
                    writer.Close();
                }


                fluxxml = builder.ToString();
            }
            catch (Exception e)
            {
                throw new Exception("Datavalue toFluxXml : " + e, e);
            }
            finally
            {
            }


            return fluxxml;
        }


        private XmlTextWriter savewriteburn(XmlTextWriter XmlTextWriter, ParamValues databurn)
        {
            string[] exnode = new string[1];
            bool start = true;
            bool exburn = false;
            int nodeouvert = 0;
            int iielement = 1;
            int ii = 0;
            foreach (ParamValuesNode datatab in databurn.GetList())
            {
                string namet = datatab.Name;
                namet = namet.Trim('/');
                int namti = namet.Split('/').Length;

                // on cree la premiere donnee
                if (start)
                {
                    exnode = namet.Split('/');
                    ii = 0;
                    foreach (string item in namet.Split('/'))
                    {
                        //if (ii != namet.Split('/').Length - 1)
                        if (ii == namti - 1) // -1 pour taille -> index du tableau
                        {
                            XmlTextWriter = savewritenode(XmlTextWriter, datatab, item);
                            //if (datatab.type == "Data.Burn") { exburn = true; }// bricolage du bug quil ferme pas tout ...


                            //XmlTextWriter.WriteEndElement();
                            nodeouvert++;
                        }
                        else { XmlTextWriter.WriteStartElement(item); nodeouvert++; }
                        ii++;
                    }


                    start = false;
                    continue;
                }



                // on ferme les node en trops
                int comptage = 0;
                ii = 0;
                foreach (string item in namet.Split('/'))
                {
                    if (exnode.Length <= ii || item != exnode[ii])
                    {
                        comptage = exnode.Length - ii; // -1?
                        break;
                    }
                    ii++;
                }
                int comptage2 = comptage;
                for (ii = 0; comptage > 0; comptage--)
                {
                    XmlTextWriter.WriteEndElement();
                    nodeouvert--;
                }




                // on creer les nouveau node et la donnée

                ii = 0;
                foreach (string item in namet.Split('/'))
                {
                    if ((ii > nodeouvert - 1) || (exburn && ii >= nodeouvert - 1)) //&& ii != namti
                        if (ii == namti - 1)// -1 pour taille -> index du tableau
                        {
                            if (exburn) XmlTextWriter.WriteEndElement(); // bricolage ...
                            exburn = false;
                            XmlTextWriter = savewritenode(XmlTextWriter, datatab, item);
                            //if (datatab.type == "Data.Burn") { exburn = true; } // bricolage du bug quil ferme pas tout ...

                            //XmlTextWriter.WriteEndElement();
                            nodeouvert++;
                            exnode = namet.Split('/');
                        }
                        else
                        {
                            XmlTextWriter.WriteStartElement(item);
                            nodeouvert++;
                        }

                    ii++;
                }




                // si c'est la derniere donnée on ferme tout
                if (databurn.Count() == iielement)
                    while (nodeouvert > 0)
                    {
                        XmlTextWriter.WriteEndElement();
                        nodeouvert = nodeouvert - 1;
                    }
                iielement++;
            }

            return XmlTextWriter;

        }



        private XmlTextWriter savewritenode(XmlTextWriter XmlTextWriter, ParamValuesNode datatab, string item)
        {
            if (datatab.Value == null) datatab.Value = ""; //Attention aux noeuds null...
            XmlTextWriter.WriteStartElement(item);
            foreach (var attribut in datatab.Attributs)
            {
                XmlTextWriter.WriteAttributeString(attribut.Key, Convert.ToString(attribut.Value));
            }
            XmlTextWriter.WriteValue(datatab.GetString(null));
            return XmlTextWriter;
        }
        


    

        /// <summary>
        /// Datavalue fromc onfig flux string xml
        /// </summary>
        /// <param name="fichierXML"></param>
        public ParamValues DeSerialize(string fluxstring, ParamValues retour = null)
        {
            try
            {
                if(retour==null) retour = new ParamValues();
                retour.Clear();
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(fluxstring);
                string lastpath = "/";
                foreach (XmlNode xChild in xDoc.ChildNodes)
                {
                    if (xChild.NodeType == XmlNodeType.Element) lastpath = "/" + xChild.Name.ToLower() + "/";
                    if (xChild.HasChildNodes) getdataXMLChild(retour, xChild, lastpath, null);
                }
                retour.AcceptChanges(); // on retire les marqueurs de changements

                return retour;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Datavalue from fichier config xml
        /// </summary>
        /// <param name="fichierXML"></param>
        public void fromXML(ParamValues datavalue,string fichierXML)
        {
            try
            {
                datavalue.Clear();
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(fichierXML);
                string lastpath = "/";
                foreach (XmlNode xChild in xDoc.ChildNodes)
                {
                    if (xChild.NodeType == XmlNodeType.Element) lastpath = "/" + xChild.Name.ToLower() + "/";
                    if (xChild.HasChildNodes) getdataXMLChild(datavalue,xChild, lastpath, null);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        private void getdataXMLChild(ParamValues datavalue, XmlNode xdot, string pathac, XmlAttributeCollection sattributs)
        {
            string lastpath = pathac;
            ParamValuesNode bufliste = new ParamValuesNode();
            XmlAttributeCollection sauvattributs = null;
            foreach (XmlNode xChild in xdot.ChildNodes)
            {
                //System.Console.WriteLine(xChild.Name);
                if (xChild.NodeType == XmlNodeType.Element)
                {
                    lastpath = pathac + xChild.Name.ToLower() + "/";
                    if (xChild.Attributes.Count > 0) sauvattributs = xChild.Attributes;
                    else sauvattributs = null;
                }

                if (xChild.NodeType == XmlNodeType.Text || (xChild.NodeType == XmlNodeType.Element && !xChild.HasChildNodes && xChild.Value == null))
                {
                    bufliste = GenerateNewNodeFromXML(datavalue,lastpath, xChild); // Ajout d'une nouvelle clef/valeur
                    if (bufliste != null) datavalue.Add(bufliste);


                }
                else if (xChild.HasChildNodes) getdataXMLChild(datavalue,xChild, lastpath, sauvattributs);
            }
        }




        private ParamValuesNode GenerateNewNodeFromXML(ParamValues datavalue,string lastpath, XmlNode xChild)
        {
            ParamValuesNode bufliste = new ParamValuesNode();
            try
            {

                bufliste.datavalues_parent = datavalue;
                bufliste.Name = lastpath.ToLower();
                if (bufliste.Name.Substring(bufliste.Name.Length - 1) == "/") bufliste.Name = bufliste.Name.Substring(0, bufliste.Name.Length - 1);
                if (bufliste.Name.Length > 9 && bufliste.Name.Substring(0, 9) == "/noparam/") bufliste.Name.Replace("/noparam/", "");

                // GESTION DES ATTRIBUTS
                // on tente d'obtenir les attributs (Il sont stockées sur le noeud parent)
                XmlAttributeCollection sattributs = null;
                XmlNode xParentElement = null;
                if (xChild.NodeType == XmlNodeType.Element) xParentElement = xChild; // il n'avait déja pas d'enfant.
                else xParentElement = xChild.ParentNode;

                if (xParentElement != null && xParentElement.NodeType == XmlNodeType.Element)
                {
                    sattributs = xParentElement.Attributes;
                }
                if (sattributs != null && sattributs.Count > 0)
                {
                    foreach (XmlAttribute item in sattributs)
                    {
                        string nameatr = item.LocalName.ToLower();
                        bufliste[nameatr] = item.Value;
                    }
                }


                // dyna
                //if (bufliste.value != null && bufliste.value is string && bufliste.value.ToString().Contains("{!"));

                // Affectation de la données
                bufliste.SetObject(null,xChild.Value);

            }
            catch (Exception)
            {
                bufliste["datatype_error"] = "true";
            }
            return bufliste;
        }






        


    }
}
