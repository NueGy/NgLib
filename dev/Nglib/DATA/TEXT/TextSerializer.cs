using Encaissement.DATA.FORMAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.MANIPULATE.TEXT
{
    /// <summary>
    /// Permet de sérialiser et contruire des objets lines
    /// </summary>
    public class TextSerializer
    {
        /// <summary>
        /// Structures disponibles chargé dans le TextSerializer
        /// </summary>
        public TextStructuresManager Structures = null;
        public bool Safe = false;

        public TextSerializer()
        {
            Structures = new TextStructuresManager();
        }

        public TextSerializer(TextStructuresManager Structures)
        {
            this.Structures = Structures;
        }

        public TextSerializer(params TextStructure[] structure)
        {
            Structures = new TextStructuresManager();
            Structures.SetStructure(structure);
        }










      





        #region Serial TextPart


        /// <summary>
        /// Déserialisation d'un fichier
        /// </summary>
        /// <typeparam name="Tobj"></typeparam>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<Tobj> DeSerialization<Tobj>(System.IO.FileInfo file, string StructureName = null) where Tobj : new()
        {
            string[] LinesString = System.IO.File.ReadAllLines(file.FullName);
            return this.DeSerialization<Tobj>(LinesString, StructureName);
        }






        /// <summary>
        /// Déserialisation du contenu d'un fichier
        /// </summary>
        /// <typeparam name="Tobj"></typeparam>
        /// <param name="LinesString"></param>
        /// <param name="StructureName"></param>
        /// <returns></returns>
        public List<Tobj> DeSerialization<Tobj>(string[] LinesString, string StructureName=null) where Tobj : new() //IDictionary<int, string>
        {
            List<Tobj> retour = new List<Tobj>();
            TextStructure structline = null;
            //bool castTextDatas = false;
            //if (Tobj is TextDatas) castTextDatas = true;

               TextLineAttribute attrline = TextLineAttribute.GetObjectLineAttributes(typeof(Tobj));

            if (attrline != null && string.IsNullOrWhiteSpace(StructureName))
                StructureName = attrline.StructureLineName; // si non défini en parametre mais présent dans l'attribut

            if (!string.IsNullOrWhiteSpace(StructureName)) // si défini attribut ou parametre, sinon sera en auto
            {
                structline = Structures.GetStructure(StructureName);
                if (structline == null) throw new Exception("structure " + StructureName + " non définie");
            }



            int iiline = 0;
            foreach (string linestr in LinesString)
            {
                iiline++;
                if (string.IsNullOrEmpty(linestr)) continue;
                TextStructure structlineitem = structline;
                try
                {
                    if (structlineitem == null)
                    {
                       // structlineitem = Structures.FindAuto(linestr);
                        if (structlineitem == null)  structlineitem = Structures.DefaultStructureLine;
                        if (structlineitem == null) throw new Exception("structure non définie (Ligne " + iiline + ")");
                    }
                    Tobj obj = new Tobj();
                    IDictionary<string, string> datas = TextTools.SplitText(linestr, structlineitem);

                    if (obj is TextDatas) // on lie l'objet avec la structure
                    {
                        TextDatas tdata = obj as TextDatas;
                        tdata.SetSchema(structlineitem);
                    }
                    else if (obj is IDictionary<int, string>) // <POSITION,VALEUR>
                    {
                        IDictionary<int, string> tdata = obj as IDictionary<int, string>;
                        
                    }
                    else if (obj is IDictionary<string, string>) // <FIELDNAME,VALEUR>
                    {
                        IDictionary<string, string> tdata = obj as IDictionary<string, string>;
                        MANIPULATE.COLLECTIONS.CollectionsTools.AddRange<string, string>(tdata, datas, true);
                    }
                    else
                    {
                        //reflexion !!!
                    }


                    retour.Add(obj);
                }
                catch (Exception)
                {
                    if (Safe) continue;
                    throw;
                }
            }
            return retour;
        }





        ///// <summary>
        ///// On sérialise des objets en text
        ///// </summary>
        ///// <param name="objs"></param>
        ///// <returns></returns>
        //public string[] Serialization<Tobj>(Tobj[] objs) where Tobj : TextDatas, new()
        //{
        //    List<string> retour = new List<string>();
        //    foreach (TextDatas obj in objs)
        //    {
        //        var structure = obj.GetSchema();
        //        TextTools.JoinText(obj, structure);
        //    }
        //    return retour.ToArray();
        //}

        /// <summary>
        /// On sérialise des objets en text
        /// </summary>
        /// <typeparam name="Tobj"></typeparam>
        /// <param name="objs"></param>
        /// <returns></returns>
        public string[] Serialization<Tobj>(Tobj[] objs) where Tobj : new()
        {
            List<string> retour = new List<string>();
            foreach (Tobj obj in objs)
            {
                if (obj is TextDatas) 
                {
                    TextDatas tdata = obj as TextDatas;
                    var structure = tdata.GetSchema();
                    retour.Add(TextTools.JoinText(tdata, structure));
                }
                else if (obj is IDictionary<int, string>) // <POSITION,VALEUR>
                {
                    throw new NotImplementedException();
                }
                else if (obj is IDictionary<string, string>) // <FIELDNAME,VALEUR>
                {
                    IDictionary<string, string> tdata = obj as IDictionary<string, string>;
                    throw new NotImplementedException();
                }
                else if (obj is IDictionary<string, object>) // <FIELDNAME,VALEUR>
                {
                    IDictionary<string, object> tdata = obj as IDictionary<string, object>;
                    throw new NotImplementedException();
                }
                else
                {
                    // reflexion
                }
            }
            return retour.ToArray();
        }



        #endregion




        #region ----  TEXT FACTORY ------



        /// <summary>
        /// Permet de créer une nouvelle ligne (d'un fichier) avec les valeurs par default
        /// </summary>
        /// <typeparam name="Tobj">ligne héritant de TextLine</typeparam>
        /// <param name="structureName">unqiuement si la structure n'est pas définie avec un attribut</param>
        /// <returns></returns>
        public Tobj CreateLine<Tobj>(string structureName = null) where Tobj : TextDatas, new()
        {
            if (string.IsNullOrWhiteSpace(structureName))
            {
                TextLineAttribute atrline = TextLineAttribute.GetObjectLineAttributes(typeof(Tobj));
                if (atrline != null) structureName = atrline.StructureLineName;
            }
            if (string.IsNullOrWhiteSpace(structureName)) throw new Exception("Impossible d'obtenir le nom de la structure");
            TextStructure structureline = this.Structures.GetStructure(structureName);
            if (structureline == null) throw new Exception("structure " + structureName + " introuvable");
            Tobj retour = new Tobj();
            retour.SetSchema(structureline);

            return retour;
        }





        ///// <summary>
        ///// Enrichir un objet avec des données provenant de la base
        ///// </summary>
        ///// <param name="line"></param>
        ///// <param name="row"></param>
        ///// <returns></returns>
        //public bool ReadDataRow(TexteManipulate line, System.Data.DataRow row)
        //{
        //    bool retour = false;
        //    TextStructure structureline = line.GetStructure();
        //    if (structureline == null) return false;
        //    foreach (TextStructurePart part in structureline.Parts.Values)
        //    {

        //        if (!string.IsNullOrWhiteSpace(part.DataBaseColumn))
        //        {
        //            line[part.Position] = MANIPULATE.DATASET.DataSetTools.GetRowString(row, part.DataBaseColumn);

        //            // !!! ajouter des action/manipulation lors de la lecture en base

        //            retour = true; // on confirme qu'il y as eu des lectures
        //        }
        //    }
        //    return retour;
        //}








        #endregion









        #region Serial Reflexion

        //public List<Tobj> DeSerializationReflexion<Tobj>(params string[] LinesString) where Tobj : new()
        //{
        //    System.Type objType = typeof(Tobj);
        //    List<Tobj> retour = new List<Tobj>();
        //    try
        //    {
        //        GetStructureLineByReflextion(objType);
        //        AttributePart[] attritems = AttributePart.GetObjectPartAttributes(objType);

        //        foreach (string LineString in LinesString)
        //        {
        //            MANIPULATE.TEXT.TextLine textparts = new TextLine(Structures.DefaultStructureLine);
        //            textparts.FillString(LineString);
        //            Tobj lineData = new Tobj();

        //            foreach (AttributePart item in attritems)
        //            {
        //                if (item.Position<0) continue;
        //                MANIPULATE.TEXT.TextPart part = textparts.GetPart(item.Position);
        //                if(part==null) continue;
        //                string resultStr = part.ToString(true, true);
        //                object resultData = ConvertPlus.ChangeType(resultStr, item.ObjectPropertyInfo.PropertyType);
        //                item.ObjectPropertyInfo.SetValue(lineData, resultData,null);
        //            }
        //            retour.Add(lineData);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("UnSerialize " + ex.Message, ex);
        //    }

        //    return retour;
        //}




        ///// <summary>
        ///// !!! a developper
        ///// </summary>
        ///// <typeparam name="Tobj"></typeparam>
        ///// <param name="objs"></param>
        ///// <returns></returns>
        //public string[] SerializationReflexion<Tobj>(List<Tobj> objs) where Tobj : new()
        //{
        //    System.Type objType = typeof(Tobj);
        //    //List<string> retour = new List<string>();
        //    List<DATA.MANIPULATE.TEXT.TextLine> texteLines = new List<TextLine>();
        //    try
        //    {


        //        GetStructureLineByReflextion(objType);
        //        AttributePart[] attritems = AttributePart.GetObjectPartAttributes(objType);


        //        foreach (Tobj obj in objs)
        //        {
        //            DATA.MANIPULATE.TEXT.TextLine line = new TextLine(Structures.DefaultStructureLine);

        //            foreach (AttributePart item in attritems)
        //            {
        //                if (item.Position < 0) continue;
        //                object resultData = item.ObjectPropertyInfo.GetValue(obj, null);
        //                string resultString = string.Empty;
        //                if (resultData != null) resultString = Convert.ToString(resultData);
        //                line.SetString(item.Position, resultString);
        //            }
        //            texteLines.Add(line);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("SerializationReflexion " + ex.Message, ex);
        //    }
        //    return this.SerializationTextLines(texteLines);
        //}





        public TextStructure GetStructureLineByReflextion(System.Type objType)
        {
            try
            {

                TextStructure StructureLineObj = null;

                TextPartAttribute[] attrs = TextPartAttribute.GetObjectPartAttributes(objType);
                TextLineAttribute attrline = TextLineAttribute.GetObjectLineAttributes(objType);

                if (attrline == null) return null;//throw new Exception("Attribute line Empty");
                if (string.IsNullOrWhiteSpace(attrline.StructureLineName)) return null;
                if (Structures.StructuresLines.Count == null) Structures.DefaultStructureLine = new TextStructure();

                StructureLineObj = this.Structures.GetStructure(attrline.StructureLineName);
                if (StructureLineObj == null) StructureLineObj = new TextStructure(attrline.StructureLineName);

                StructureLineObj.TextMode = attrline.TextMode;


                // enrichir si la structure existe deja
                foreach (TextPartAttribute item in attrs)
                {
                    TextStructurePart ispart = null;
                    if (!string.IsNullOrWhiteSpace(item.Name)) ispart = StructureLineObj.GetPart(item.Name); // recherche par le nom
                    else if (item.Position >= 0) ispart = StructureLineObj.GetPart(item.Position); // recherche par la position
                    if (ispart == null)
                    {
                        if (StructureLineObj.TextMode == TextModeEnum.TXT && item.Length == 0) continue; //inutile ...
                        ispart = new TextStructurePart();
                        ispart.Position = item.Position;
                        ispart.Length = item.Length;
                        ispart.Name = item.Name;

                        //if (item.StringTransformOptions != null)
                        //    foreach (var item in item.StringTransformOptions)
                        //    {

                        //    }

                        /*
                        if (!string.IsNullOrWhiteSpace(item.Regex))
                        {
                            if (ispart.data == null) ispart.data = new DATAVALUES.DataValues_data();
                            ispart.data["chaine_regex"] = item.Regex;
                        }*/

                        ispart.CompleteNumber = item.CompleteNumber;
                    }
                    StructureLineObj.SetPart(ispart);
                }


                this.Structures.SetStructure(StructureLineObj);
                return StructureLineObj;

            }
            catch (Exception ex)
            {
                throw new Exception("GetStructureLineByReflextion " + ex.Message);
            }
        }




        #endregion









    }
}
