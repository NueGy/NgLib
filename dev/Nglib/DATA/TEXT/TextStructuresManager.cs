using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.MANIPULATE.TEXT
{
    /// <summary>
    /// Gestion des structures
    /// Import /export dans un fichier de paramétrage
    /// </summary>
    public class TextStructuresManager
    {

        /// <summary>
        /// Structures de fichiers disponibles
        /// </summary>
        public Dictionary<string, TextStructure> StructuresLines = new Dictionary<string, TextStructure>();


        /// <summary>
        /// Structure par default
        /// </summary>
        public TextStructure DefaultStructureLine
        {
            get
            {
                if (StructuresLines == null || StructuresLines.Count == 0) return null;
                if (StructuresLines.ContainsKey("default")) return this.StructuresLines["default"];
                return null;
            }
            set
            {
                DefaultStructureLine.StructureLineName = "default";
                this.SetStructure(DefaultStructureLine);
            }
        }


        /// <summary>
        /// Ajouter de nouvelles structures
        /// </summary>
        /// <param name="DataStructs"></param>
        public void SetStructure(params TextStructure[] DataStructs)
        {
            foreach (TextStructure DataStruct in DataStructs)
            {
                string name = DataStruct.StructureLineName;
                if (string.IsNullOrWhiteSpace(name)) name = "default"; //name = "NA" + (StructuresLines.Count+1).ToString();
                if (this.StructuresLines.ContainsKey(name)) this.StructuresLines.Remove(name);
                DataStruct.StructureLineName = name;
                this.StructuresLines.Add(name, DataStruct);
            }
        }



        /// <summary>
        /// Ajouter une nouvelle structure depuis le xml 
        /// </summary>
        /// <param name="DataStruct"></param>
        /// <param name="structureName"></param>
        public void SetStructureXml(DATAVALUES.DataValues DataStruct, string structureName = "default")
        {
            this.SetStructure(new TextStructure(DataStruct, structureName));
        }



        /// <summary>
        /// Obtenir une structure
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TextStructure GetStructure(string name)
        {
            if (this.StructuresLines == null || this.StructuresLines.Count == 0) return null;
            foreach (string itemkey in this.StructuresLines.Keys)
                if (itemkey.Equals(name, StringComparison.InvariantCultureIgnoreCase)) return this.StructuresLines[itemkey];
            return null;
        }




        public void ExportToFile(System.IO.FileInfo fileparam)
        {
            throw new NotImplementedException();
        }

        //public int ImportFromFile(string fileparampath)
        //{
        //    System.IO.FileInfo fileparam = new System.IO.FileInfo(fileparampath);
        //    return ImportFromFile(fileparam);
        //}

        //public int ImportFromFile(System.IO.FileInfo fileparam)
        //{
        //    try
        //    {
        //        if (!fileparam.Exists) throw new Exception("le fichier n'existe pas");
        //        if (!fileparam.Extension.Equals(".xml")) throw new Exception("Seul les fichiers .xml sont acceptés");
        //        DATAVALUES.DataValues filedata = new DATAVALUES.DataValues();
        //        filedata.DatavalueManager().fromXML(fileparam.FullName);
        //        List<TextStructure> structuresfinds = ExtractStructuresMultiples(filedata);

        //        foreach (TextStructure itemstructure in structuresfinds)
        //        {
        //            this.SetStructure(itemstructure);
        //        }

        //        return structuresfinds.Count;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Impossible d'importer les structures du fichier (" + fileparam .Name+ ") " + ex.Message);
        //    }
        //}




        public int Count
        {
            get { return this.StructuresLines.Count; }
        }





        //public TextStructure FindAuto(string chaine)
        //{
        //    try
        //    {
        //        foreach (TextStructure item in this.StructuresLines.Values)
        //        {
        //            if (item.TextMode != TextModeEnum.TXT) continue;
        //            List<TextStructurePart> findautoparts = item.GetFindAutoParts();
        //            if(findautoparts.Count == 0)continue;
        //            bool isgoodstructure = true; // bon jusqu'a preuve du contraire

        //            foreach (TextStructurePart itempart in findautoparts)
        //            {
        //                if (!TextTools.IsEqualSubstringValue(itempart.Position, itempart.Length, chaine, itempart.DefaultValue)) isgoodstructure = false;
        //            }

        //            if (isgoodstructure) return item;

        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("FindAuto "+ex.Message);
        //    }
        //}


















        public static List<TextStructure> ExtractStructuresMultiples(DATAVALUES.DataValues DvParamFile)
        {
            List<TextStructure> retour = new List<TextStructure>();
            try
            {
                List<string> structuresName = DvParamFile.ListFieldsName("/param/structures/").ToList(); // on essaye dabord si c'est encapsulé
                foreach (string structureName in structuresName)
                {
                    DATAVALUES.DataValues structuredv = null;

                    // extraire la stucture
                    //if (encapsuleNode) structuredv = DvParamFile.CloneDatas("/param/structures/" + structureName + "/", false);
                    //else structuredv = DvParamFile.CloneDatas("/param/" + structureName + "/", false);
                    //!!!m!!!

                    structuredv = ValidateCleanParamPart(structuredv);


                    TextStructure StructureLine = new TextStructure(structuredv, structureName);
                    retour.Add(StructureLine);
                }

                return retour;
            }
            catch (Exception)
            {

                throw;
            }
        }




        public static DATAVALUES.DataValues ValidateCleanParamPart(DATAVALUES.DataValues fileparamStructure)
        {
            // ne peus contenir qu'une seul structure
            // ne peus pas contenie deux fois la meme position

            List<DATAVALUES.DataValuesNode> parts = fileparamStructure.GetDatas("/param/parts");
            List<string> positions = new List<string>();
            foreach (var item in parts)
            {
                string pos = item["position"];
                if (string.IsNullOrWhiteSpace(pos)) continue;
                if (positions.Contains(pos)) throw new Exception("Position " + pos + " en doublon");
                positions.Add(pos);
            }
            return fileparamStructure;
        }



















    }
}
