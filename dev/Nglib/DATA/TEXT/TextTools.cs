using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.MANIPULATE.TEXT
{
    public class TextTools
    {


        /// <summary>
        /// Transforme un dictionary de positions en une chaine
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static string JoinText(IDictionary<int, string> datas, TEXT.TextStructure schema)
        {
            try
            {
                StringBuilder retour = new StringBuilder();
                int partnumber = 0;
                int lastPositionPlusSize = 0;
                foreach (var ipart in schema.GetPartsOrdered())
                {
                    TextStructurePart partschema = ipart.Value;


                    // la donnée
                    string orgnData = null;
                    if (datas.ContainsKey(ipart.Key)) orgnData = datas[ipart.Key];
                    if (string.IsNullOrWhiteSpace(orgnData) && partschema != null && partschema.CompleteNumber) orgnData = "0"; // si complete number, ce sera forcément des 0

                    retour.Append(JoinTextItem(schema, partschema, orgnData, partnumber, lastPositionPlusSize));
                    partnumber++;
                }
                return retour.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("JoinText {0}", ex.Message), ex);
            }
        }



        /// <summary>
        /// Transforme un dictionary de positions en une chaine
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static string JoinText(IDictionary<string, string> datas, TEXT.TextStructure schema)
        {
            try
            {
                StringBuilder retour = new StringBuilder();

                int lastPositionPlusSize = 0;
                int partnumber = 0;
                foreach (var ipart in schema.GetPartsOrdered())
                {
                    TextStructurePart partschema = ipart.Value;


                    // obtenir la donnée
                    string orgnData = null;
                    if (datas.ContainsKey(partschema.NodeName)) orgnData = datas[partschema.NodeName];
                    if (string.IsNullOrWhiteSpace(orgnData) && partschema != null && partschema.CompleteNumber) orgnData = "0"; // si complete number, ce sera forcément des 0


                    retour.Append(JoinTextItem(schema, partschema, orgnData, partnumber, lastPositionPlusSize));
                    partnumber++;
                }
                return retour.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("JoinText {0}", ex.Message), ex);
            }
        }


        private static string JoinTextItem(TEXT.TextStructure schema,TextStructurePart partschema, string orgnData, int partNumber,int lastPositionPlusSize)
        {
            try
            {
                // Valider / Transformer / dynamiser la donnée
                // !!!
               



                if (schema.TextMode == TextModeEnum.CSV) // Génération simple d'un CSV
                {
                    if (partNumber == 0) return orgnData; // on met pas de séparateur pour le premier champ
                    else return (schema.TextModeCSVSeparator.ToString() + orgnData);
                }
                else if (schema.TextMode == TextModeEnum.TXT)
                {
                    // on ajoute des blancs ou des zero pour avoir la bonne taille
                    orgnData = FORMAT.StringUtilities.Complete(orgnData, partschema.Length, partschema.CompleteNumber, true);
                    lastPositionPlusSize = partschema.Position + partschema.Length;

                    //Ajouter des blancs si il y as un trou (!!! es vraiement nécessaire ?)
                    if (lastPositionPlusSize < (partschema.Position))
                        return (new string(schema.TextModeCSVSeparator, partschema.Position - lastPositionPlusSize));
                    return (orgnData);
                   
                }
                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }




        /// <summary>
        /// Permet de découper un string en tableau de position
        /// </summary>
        /// <param name="text"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static Dictionary<string, string> SplitText(string inputString, TEXT.TextStructure schema)
        {
            if (string.IsNullOrWhiteSpace(inputString)) throw new Exception("SplitText inputString empty");
            if (schema.TextMode == TextModeEnum.CSV)
            {
                if (!inputString.Contains(schema.TextModeCSVSeparator)) throw new Exception("SplitText inputString ne contient pas de séparateur");
                return SplitTextCSV(inputString, schema);
            }
            else if (schema.TextMode == TextModeEnum.TXT)
            {
                if (schema.Count == 0) throw new Exception("SplitText La structure n'a pas été définie");
                return SplitTextTXT(inputString, schema);
            }
            return null; // arrivera pas
        }




        private static Dictionary<string, string> SplitTextCSV(string inputString, TEXT.TextStructure schema)
        {
            try
            {
                string[] inputStringT = inputString.Split(schema.TextModeCSVSeparator);
                Dictionary<string, string> retour = new Dictionary<string, string>();
                foreach (var ipart in schema.GetPartsOrdered())
                {
                    TextStructurePart partschema = ipart.Value;
                    if (inputStringT.Length < partschema.Position)
                    {
                        retour.Add(partschema.NodeName, null);
                        continue;
                    }
                    retour.Add(partschema.NodeName, inputStringT[ipart.Key]);
                }
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SplitText CSV {0}", ex.Message), ex);
            }
        }

        private static Dictionary<string, string> SplitTextTXT(string inputString, TEXT.TextStructure schema)
        {
            try
            {
                int sizestring = inputString.Length;
                int lastPositionPlusSize = 0;
                Dictionary<string, string> retour = new Dictionary<string, string>();
                foreach (var ipart in schema.GetPartsOrdered())
                {
                    TextStructurePart partschema = ipart.Value;

                    int itemPosition = partschema.Position; // Attention position logique et non réel  1=0
                    int itemSize = partschema.Length;
                    int itemPositionEnd = itemPosition + itemSize;
                    if (itemPositionEnd > sizestring) itemSize = sizestring - itemPositionEnd;

                    if (sizestring < itemPosition || itemSize < 1)  // hors limite (la taille de la chaine est plus petite que ce qui a été défini)
                    {
                        retour.Add(partschema.NodeName, null);
                        continue;
                    }

                    // on découpe la partie qui nous interresse
                    string datastr = inputString.Substring(itemPosition, itemSize);

                    if (!schema.ReadDisallowTrim && datastr != null)
                        datastr = datastr.Trim();
                    retour.Add(partschema.NodeName, datastr);
                    lastPositionPlusSize = itemPositionEnd;
                }
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SplitText TXT {0}", ex.Message), ex);
            }
        }



        public static void ValidateText(IDictionary<string, string> datas, TEXT.TextStructure schema)
        {

        }





        //public static void ValidateTextLines(params TextLine[] lines)
        //{
        //    int iline = 0;

        //    foreach (TextLine line in lines)
        //    {
        //        iline++;
        //        try
        //        {
        //            foreach (var part in line.GetStructure().Parts.Values)
        //            {
        //                string val = line[part.Name];
        //                line[part.Name] = part.ValidateValue(val);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception("Line " + iline.ToString() + " " + ex.Message, ex);
        //        }
        //    }

        //}




        public static bool IsEqualSubstringValue(int position, int size, string fullChaine, string wantChaine)
        {
            try
            {
                string strtocompare = null;

                if (fullChaine.Length < position) return false;
                int realsize = resizeChaineLength(fullChaine.Length, position, size);
                strtocompare = fullChaine.Substring(position, realsize);


                if (strtocompare == null || wantChaine == null) return false;

                if (strtocompare.Equals(wantChaine)) return true;
                return false;

            }
            catch (Exception)
            {
                throw;
            }
        }


        public static int resizeChaineLength(int fullChaineLenght, int position, int size)
        {
            if (fullChaineLenght < position) return -1;
            if (size < 1) return size;

            // Attention position logique et non réel  1=0
            int itemPositionEnd = position + size;
            if (itemPositionEnd > fullChaineLenght) return (fullChaineLenght - itemPositionEnd);
            return size;
        }








        public static string[] ExtractLines(System.IO.FileInfo filetxt, bool FirstLineHeader = false, bool CSVDefineStructure = false)
        {
            string[] lines = System.IO.File.ReadAllLines(filetxt.FullName);
            return lines; // DefineLines(FirstLineHeader, CSVDefineStructure, lines);
        }

        //public static string[] DefineLines(bool FirstLineHeader, bool CSVDefineStructure, params string[] lines)
        //{

        //    if (CSVDefineStructure && Structures.StructuresLines.Count > 0 && Structures.StructuresLines.Count == 0 && lines.Length > 0 && Structures.DefaultStructureLine != null)
        //    {
        //        if (Structures.DefaultStructureLine.TextMode == TextModeEnum.CSV)
        //            Structures.DefaultStructureLine.SetStructureFromCSV(lines[0], FirstLineHeader, true);
        //    }

        //    if (FirstLineHeader)
        //    {
        //        List<string> linesList = new List<string>(lines);
        //        linesList.RemoveAt(0);
        //        lines = linesList.ToArray();
        //    }
        //    return lines;
        //}

    }
}
