using Nglib.MANIPULATE.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.MANIPULATE.TEXT
{
    /// <summary>
    /// Représente la structure d'une ligne de donnée
    /// </summary>
    public class TextStructure : System.Attribute
    {
        /// <summary>
        /// Pour connaitre ou sont stocké les champs lors de l'import d'unfichier
        /// </summary>
        public const string DefaultPartsNodes = "/param/parts/";

        /// <summary>
        /// Taille maximum d'une ligne acceptable
        /// </summary>
        public int sizemaxchaine = 999999;

        /// <summary>
        /// Mode de l'import 
        /// </summary>
        public TextModeEnum TextMode = TextModeEnum.CSV;

        /// <summary>
        /// Caractère de séparation dans le cas du mode CSV
        /// </summary>
        public char TextModeCSVSeparator = ';';

        /// <summary>
        /// Toute les Données de la structure
        /// </summary>
        public DATAVALUES.DataValues OriginalStructureModel = null;


        /// <summary>
        /// Les champs dans la structure
        /// </summary>
        public List<TextStructurePart> Parts = new List<TextStructurePart> ();

        /// <summary>
        /// Le nom de la structure
        /// </summary>
        public string StructureLineName = null;

        /// <summary>
        /// désactive les trim au moment de la lecture
        /// </summary>
        public bool ReadDisallowTrim { get; set; }






        public TextStructure()
        {
            this.StructureLineName = "default";
        }

        public TextStructure(string structureName)
        {
            this.StructureLineName = structureName;
        }


        public TextStructure(DATAVALUES.DataValues StructureModel, string structureName = "default")
        {
            this.StructureLineName = structureName;
            this.SetStructure(StructureModel);
        }






        public IDictionary<int, TextStructurePart> GetPartsOrdered()
        {
            if (_PartsOrderedCache != null) return _PartsOrderedCache;

            IDictionary<int, TextStructurePart> retour = new SortedDictionary<int, TextStructurePart>();
            this.Parts.Where(p=>p.Position>-1).ToList().ForEach(p => { if(!retour.ContainsKey(p.Position))retour.Add(p.Position, p);  });
            return retour;
        }
        private IDictionary<int, TextStructurePart> _PartsOrderedCache = null;






        #region importer la structure
        public void SetStructure(Dictionary<int, string> PositionAndName)
        {
            if (PositionAndName == null) return;
            foreach (int Position in PositionAndName.Keys)
            {
                TextStructurePart ipart = this.GetPart(Position);
                if (ipart == null)
                {
                    string namem = PositionAndName[Position];
                    ipart = new TextStructurePart();
                    ipart.Position = Position;
                    ipart.Name = namem;
                    this.Parts.Add(ipart);
                }
            }
        }

        public void SetStructure(DATAVALUES.DataValues datastruct)
        {
            if (datastruct == null) return;
            this.OriginalStructureModel = datastruct;
            // INFOS
            string txtmodestr = datastruct.GetString("TextMode");
            if (txtmodestr.Equals("CSV", StringComparison.InvariantCultureIgnoreCase)) { this.TextMode = TextModeEnum.CSV; this.TextModeCSVSeparator = ';'; }
            else if (txtmodestr.Equals("TXT", StringComparison.InvariantCultureIgnoreCase)){ this.TextMode = TextModeEnum.TXT; this.TextModeCSVSeparator = ' '; }

            string TextModeCSVSeparatorstr = datastruct.GetString("TextModeCSVSeparator");
            if (TextModeCSVSeparatorstr.Length > 0) this.TextModeCSVSeparator = TextModeCSVSeparatorstr[0];

            // PARTIES
            List<DATAVALUES.DataValuesNode> partsdv = datastruct.GetDatas(DefaultPartsNodes);
            foreach (DATAVALUES.DataValuesNode item in partsdv)
            {
                string namem = item.NodeName;
                TextStructurePart ipart = this.GetPart(namem);
                if (ipart == null)
                {
                    if (string.IsNullOrWhiteSpace(item["position"])) continue;
                    ipart = new TextStructurePart();
                    ipart.Fusion(item);
                    this.Parts.Add(ipart);
                }
                else
                {
                    ipart.Fusion(item, true);
                }

            }
        }



        public void SetStructure(System.Data.DataTable tableStruct)
        {
            //if (TypeFile != TypeFileEnum.CSV) throw new Exception("(DEV) SetStructure a partir de DataTable est disponible que pour le CSV");
            int position = 0;
            foreach (System.Data.DataColumn col in tableStruct.Columns)
            {
                string namem = col.ColumnName;
                TextStructurePart ipart = this.GetPart(namem);
                if (ipart == null)
                {
                    ipart = new TextStructurePart();
                    ipart.Position = position;
                    ipart.Name = namem;
                    ipart.Length = col.MaxLength;
                    this.Parts.Add(ipart);
                    position++;
                }
            }
        }


        public void SetStructureFromCSV(string inputString, bool Header = true, bool safe = true)
        {
            int iposition = 0;
            string[] inputStringT = inputString.Split(this.TextModeCSVSeparator);
            foreach (string partstring in inputStringT)
            {
                TextStructurePart ipart = new TextStructurePart();
                ipart.Position = iposition;
                
                if (Header) ipart.Name = inputStringT[ipart.Position].Trim();
                this.SetPart(ipart);
                iposition++;
            }
        }

        #endregion




        #region Gestion des parts

        public int Count
        {
            get { return Parts.Count; }
        }



        public TextStructurePart GetPart(int position)
        {
            //if (this.Parts.ContainsKey(position))
            //    return this.Parts[position];
            return this.Parts.FirstOrDefault(p => p.Position == position);
        }

        public TextStructurePart GetPart(string namePart)
        {
            return this.Parts.FirstOrDefault(p => p.NodeName.Equals(namePart,  StringComparison.OrdinalIgnoreCase));
        }

        public string GetName(int position)
        {
            TextStructurePart ipart = this.GetPart(position);
            if (ipart == null) return null;
            else return ipart.Name;
        }

        public int GetPosition(string namePart)
        {
            try
            {
                TextStructurePart ipart = this.GetPart(namePart);
                if (ipart == null) return -1;
                else return ipart.Position;
            }
            catch (Exception e)
            {
                return -2;
            }
        }

        public bool DeletePart(int Position)
        {
            TextStructurePart ipart = this.GetPart(Position);
            if (ipart == null) return false;
            return this.Parts.Remove(ipart);
        }

        public bool SetPart(TextStructurePart part)
        {
            if (part.Position < 0) return false;
            DeletePart(part.Position);
            this.Parts.Add( part);
            return true;
        }

        public TextStructurePart SetPart(int Position, int Lenght = 0, string Name = null)
        {
            TextStructurePart retour = new TextStructurePart();
            retour.Position = Position;
            retour.Name = Name;
            retour.Length = Lenght;
            if (this.SetPart(retour)) return retour;
            else return null;
        }



        //public List<TextStructurePart> GetFindAutoParts()
        //{
        //    List<TextStructurePart> retour = new List<TextStructurePart>();
        //    foreach (TextStructurePart item in this.Parts.Values)
        //    {
        //        if (item.data != null && item.data.AttributeIsTrue("findauto")) retour.Add(item);
        //    }
        //    return retour;
        //}



        #endregion







    }


}
