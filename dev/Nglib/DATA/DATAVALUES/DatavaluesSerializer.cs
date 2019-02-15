using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.DATAVALUES
{
    [Obsolete("A revoir")]
    public class DatavaluesSerializer
    {
        public DataValues datavalue;
        public string defaultxmlfile;


        public DatavaluesSerializer(DataValues datavalue)
        {
            this.datavalue = datavalue;
        }





        #region XML To

        /// <summary>
        /// Enregistre la liste dans un fichier config xml
        /// </summary>
        /// <param name="fichierXML"></param>
        public void toXML(string fichierXML)
        {
            try
            {
                // vérif et convert
                DATAVALUES.DataValues dvforxml = datavalue.Clone();
                foreach (DATAVALUES.DataValuesNode itemd in dvforxml.GetList())
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
        public string tofluxXML()
        {
            string fluxxml = ""; //chaine xml final
            if (datavalue.Count() == 0) return "<?xml version=\"1.0\" ?><param><empty>True</empty></param>";


            DATAVALUES.DataValues dvforxml = null; // datavalue buffer
            try
            {
                try
                {
                    // vérif et convert
                    dvforxml = datavalue.Clone();
                    try { dvforxml.Sort(); } catch { }
                    foreach (DATAVALUES.DataValuesNode itemd in dvforxml.GetList())
                    {
                        if (itemd.Name.Length > 0 && itemd.Name[0] != '/') itemd.Name = "/noparam/" + itemd.Name;
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


            //if (this.datavalue.isEncrypted) fluxxml = cryptageFlux(fluxxml);
            return fluxxml;
        }


        private XmlTextWriter savewriteburn(XmlTextWriter XmlTextWriter, DATAVALUES.DataValues databurn)
        {
            string[] exnode = new string[1];
            bool start = true;
            bool exburn = false;
            int nodeouvert = 0;
            int iielement = 1;
            // ex :  /root/general/param/ex/data/
            // ex : /root/societe/liste/data/
            int ii = 0;
            foreach (DATAVALUES.DataValuesNode datatab in databurn.GetList())
            {
                string namet = datatab.Name;
                if (namet[0] == '/') namet = namet.Substring(1);
                if (namet[namet.Length - 1] == '/') namet = namet.Substring(0, namet.Length - 1);
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
                    if (item != exnode[ii])
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



        private XmlTextWriter savewritenode(XmlTextWriter XmlTextWriter, DATAVALUES.DataValuesNode datatab, string item)
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


        #endregion




        #region XML FROM


        /// <summary>
        /// Datavalue fromc onfig flux string xml
        /// </summary>
        /// <param name="fichierXML"></param>
        public void fromFluxXML(string configflux)
        {
            try
            {
                //if (this.datavalue.isEncrypted) configflux = DecryptageFlux(configflux);
                // else if (SeyesLib3.DATA.CHAINE.Tools.limitchaine(configflux, 18) == "{!SeyesDVEncrypted") throw new Exception("Fluxxml Encrypté");
                datavalue.Clear();
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(configflux);
                string lastpath = "/";
                foreach (XmlNode xChild in xDoc.ChildNodes)
                {
                    if (xChild.NodeType == XmlNodeType.Element) lastpath = "/" + xChild.Name.ToLower() + "/";
                    if (xChild.HasChildNodes) getdataXMLChild(xChild, lastpath, null);
                }
                defaultxmlfile = "";
                datavalue.AcceptChanges(); // on retire les marqueurs de changements car c'est logique
            }
            catch (Exception)
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
        public void fromXML(string fichierXML)
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
                    if (xChild.HasChildNodes) getdataXMLChild(xChild, lastpath, null);
                }
                defaultxmlfile = fichierXML;

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        private void getdataXMLChild(XmlNode xdot, string pathac, XmlAttributeCollection sattributs)
        {
            string lastpath = pathac;
            DATAVALUES.DataValuesNode bufliste = new DATAVALUES.DataValuesNode();
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
                    bufliste = GenerateNewNodeFromXML(lastpath, xChild); // Ajout d'une nouvelle clef/valeur
                    if (bufliste != null) datavalue.Add(bufliste);


                }
                else if (xChild.HasChildNodes) getdataXMLChild(xChild, lastpath, sauvattributs);
            }
        }




        private DATAVALUES.DataValuesNode GenerateNewNodeFromXML(string lastpath, XmlNode xChild)
        {
            DATAVALUES.DataValuesNode bufliste = new DATAVALUES.DataValuesNode();
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


        #endregion



        #region FROM JSON


        //public void FromFluxJson(string strflow)
        //{
        //    try
        //    {
        //        Newtonsoft.Json.Linq.JObject jobject = Newtonsoft.Json.Linq.JObject.Parse(strflow);
        //        foreach (var joitem in jobject)
        //            FromJsonSub(joitem.Value);

        //        datavalue.AcceptChange(); // on retire les marqueurs de changements car c'est logique
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("FromFluxJson " + ex.Message, ex);
        //    }
        //}


        //private void FromJsonSub(JToken jitem)
        //{
        //    if (!jitem.HasValues)
        //    {
        //        string fullname = jitem.Path;
        //        fullname = fullname.Replace(".", "/").Replace("[0]", "");
        //        fullname = "/param/" + fullname;
        //        DATAVALUES.DataValues_data dataelement = new DATAVALUES.DataValues_data();
        //        dataelement.name = fullname;
        //        dataelement.value = jitem.ToString();// jitem.ToObject();
        //        datavalue.AddData(dataelement);
        //    }
        //    else
        //    {
        //        foreach (var jsubitem in jitem.Children())
        //        {
        //            FromJsonSub(jsubitem);
        //        }
        //    }
        //}


        #endregion



        #region TO JSON


        //public string tofluxJson() 
        //{
        //    Newtonsoft.Json.Linq.JObject jobject = new Newtonsoft.Json.Linq.JObject();

        //    tofluxJson_Recursif(jobject, this.datavalue.GetList());

        //    Newtonsoft.Json.Formatting outjsonformating = Newtonsoft.Json.Formatting.Indented;
        //    string jsontstr = jobject.ToString();
        //    //jsontstr = jsontstr.Replace("\r\n", "");//.Replace("\"", "");
        //    //jsontstr = System.Text.RegularExpressions.Regex.Unescape(jsontstr);
        //    return jsontstr;
        //}


        //private void tofluxJson_Recursif(Newtonsoft.Json.Linq.JObject jobject, List<DATA.DATAVALUES.DataValues_data> itemdvs)  // !!! améliorer pour rendre multiniveau
        //{

        //    foreach (var itemdv in itemdvs)
        //    {
        //        object jivalue = itemdv.value;
        //        Newtonsoft.Json.Linq.JValue itemj = new Newtonsoft.Json.Linq.JValue(jivalue);
        //        jobject.Add(itemdv.NameMinimal, itemj);
        //    }

        //}



        #endregion




    }
}
