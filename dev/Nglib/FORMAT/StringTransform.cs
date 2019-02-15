// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nglib.DATA.ACCESSORS;
using Nglib.DATA.COLLECTIONS;
using System.Text.RegularExpressions;
using Nglib.FORMAT;

namespace Nglib.FORMAT
{

    /// <summary>
    /// Permet de dynamiser/transformer/valider une chaine de caractères selon une configuration
    /// </summary>
    public class StringTransform
    {
        public const string DefaultPrefix = "format_";
        public const string DynamicPartRegex = @"\{!(.*?)\}";
        public const string DynamicPartFormat = @"{!}";


        /// <summary>
        /// Transformer et valider une chaine selon une série d'inscrutions
        /// </summary>
        /// <param name="orgnString"></param>
        /// <param name="transformFuctions"></param>
        /// <returns></returns>
        public static string Transform(string orgnString, Dictionary<string, string> transformFuctions)
        {
            return orgnString;
            // !!!
        }



        public static string DynamiseWithAccessor(string orgnString, DATA.ACCESSORS.IDataAccessor Accessor)
        {
            Dictionary<string, object> DynValsPossibilities = new Dictionary<string, object>();
            DynValsPossibilities.Add("data", Accessor);
            return Dynamise(orgnString, DynValsPossibilities);
        }


        /// <summary>
        /// permet de remplacer des bout de chaine par des champs contenu dans les accessors
        /// </summary>
        /// <param name="orgnString"></param>
        /// <param name="Accessors"></param>
        /// <returns></returns>
        public static string Dynamise(string orgnString, Dictionary<string,object> DynValsPossibilities)
        {
            if (string.IsNullOrWhiteSpace(orgnString)) return orgnString; // innutile car vide
            if(!orgnString.Contains("{!")) return orgnString; // innutile car rien de dynamique
            try
            {
               string[] DynamicParts = Regex.Matches(orgnString, DynamicPartRegex).Cast<Match>().Select(match => match.Value).ToArray();
                if (DynamicParts == null || DynamicParts.Length == 0) return orgnString; // rien trouvé

                string finalString = orgnString;
                foreach (string dynpart in DynamicParts)
                {
                    if (dynpart.Length < 5 || !dynpart.StartsWith("{!") || !dynpart.EndsWith("}")) continue; // mauvais match
                    //retirer les début et fin de chaine puis Découpe les partie(facultatif)
                    string[] menuparts = dynpart.Substring(2, dynpart.Length-3).Split(new[] {'|'},StringSplitOptions.RemoveEmptyEntries);
                    if (menuparts.Length == 0 || menuparts[0].Length < 3) continue; // controles de bases

                    // obtient la valeur
                    string newval = DynamiseOne(menuparts, DynValsPossibilities);

                    // execute les evenetuelle fonction de transformation
                    Dictionary<string, string> transformFuctions = GetTransformFunctionOnDynamicPart(menuparts);
                    if (transformFuctions != null) newval = Transform(newval, transformFuctions);

                    // remplacement
                    finalString = finalString.Replace(dynpart, newval);
                }
                return finalString;
            }
            catch (Exception ex)
            {
                throw new Exception("Dynamise "+ex.Message,ex);
            }
        }



        private static string DynamiseOne(string[] menuparts, Dictionary<string, object> DynValsPossibilities)
        {
            string firstPart = menuparts[0].Trim().ToLower();
            string secondPart = (menuparts.Length>1)?menuparts[1]:null;
            string retour = string.Empty;



            if (string.IsNullOrWhiteSpace(firstPart)) return retour;
            else if (firstPart.EqualsList(new[] { "datenow" })) retour = DateTime.Now.ToString();
            else if (DynValsPossibilities != null && DynValsPossibilities.ContainsKey(firstPart) && !string.IsNullOrEmpty(secondPart)) // val passée en parametre
            {
                object ival = DynValsPossibilities[firstPart];
                if (ival != null && ival is DATA.ACCESSORS.IDataAccessor)
                    retour = (ival as DATA.ACCESSORS.IDataAccessor).GetString(secondPart);
                else if (ival != null)
                    retour = ival.ToString();
            }
    



            return retour;
        }

        private static Dictionary<string, string> GetTransformFunctionOnDynamicPart(string[] menuparts)
        {
            if (menuparts.Length < 3) return null;
            Dictionary<string, string> transformFuctions = new Dictionary<string, string>();
            for (int i = 2; i < (menuparts.Length-1); i++)
            {
                string functionKey = menuparts[i];
                string functionParamValue = menuparts[i];
                transformFuctions.Add(functionKey, functionParamValue);
            }
            return transformFuctions;
        }


            public static List<string> SplitDynamicParts(string orgnString)
        {
            return null;
        }




        public class StringDynamizerContext
        {
            public string OrgnString { get; set; }
            public Dictionary<string, string> DynFunctions { get; set; }
            public string FinalString { get; set; }
        }













        private static void Transform(StringDynamizerContext context)
        {
            // chaine_formatdate
            // l'odre des opérations est très importantes
            try
            {

                if (context.DynFunctions.Keys.Contains("csvposition",true)) // obtient la données dans une ligne csv
                    Transform_CsvPosition(context);

                if (context.DynFunctions.Keys.Contains("trim", true))
                    Transform_trim(context);

                if (context.DynFunctions.Keys.Contains(new List<string>() { "substring", "start", "right", "left", "limit", "size" }))
                    Transform_substring(context);

                if (context.DynFunctions.Keys.Contains("replace", true))
                    Transform_replace(context);

                if (context.DynFunctions.Keys.Contains("formatdate", true))  //transforme en date
                    Transform_formatdate(context);

                if (context.DynFunctions.Keys.Contains("atend", true))  //place a la fin
                    Transform_atend(context);

                if (context.DynFunctions.Keys.Contains("atstart", true))  //place au début
                    Transform_atstart(context);

                if (context.DynFunctions.Keys.Contains("quotation", true))  //place au début
                    Transform_quotation(context);



                // *********************************************************************************
                // VALIDATEURS

                if (context.DynFunctions.Keys.Contains("regex"))  // Validation
                    Validate_regex(context);

                if (context.DynFunctions.Keys.Contains("ismodulo"))  // Validation
                    Validate_ismodulo(context);

                if (context.DynFunctions.Keys.Contains("lengthmax"))  // Validation
                    Validate_lengthmax(context);

                if (context.DynFunctions.Keys.Contains("validtype"))  // Validation
                    Validate_validtype(context);

                if (context.DynFunctions.Keys.Contains("isempty"))  // Validation
                    Validate_isempty(context);

                if (context.DynFunctions.Keys.Contains("disallowchar"))  // Validation
                    Validate_disallowchar(context);

            }
            catch (Exception ex)
            {
                throw new Exception("Validate Error: " + ex.Message);
            }
        }




        #region ------ Transform and Validate methods -------



        private static void Transform_CsvPosition(StringDynamizerContext context)
        {
            //chaine = '"' + chaine.Replace("\"", "") + '"';
            //string[] chainedec = chaine.Split(';');
            //int position = Convert.ToInt32(fcnData["csvposition"]);
            //if (chainedec.Length < position) throw new Exception("csvposition Ligne CSV invalide, Position : " + position + "/" + chainedec.Length);
            //chaine = chainedec[position];
            //if (chaine.Length > 1 && chaine[0] == '"' && chaine[chaine.Length - 1] == '"') chaine = chaine.Substring(1, chaine.Length - 2); //suppression des guillemets
        }

        /// <summary>
        ///  Permet de supprimer les blancs sur la chaine
        /// </summary>
        /// <param name="context"></param>
        private static void Transform_trim(StringDynamizerContext context)
        {
            context.FinalString = context.FinalString.Trim();
        }

        private static void Transform_substring(StringDynamizerContext context)
        {
            //{
            //    string[] splitsubstringparam = Convert.ToString(fcnData["substring"]).Split(',');
            //    int positionsub = Convert.ToInt32(splitsubstringparam[0]);
            //    int sizesub = Convert.ToInt32(splitsubstringparam[1]);

            //    chaine = DATA.FORMAT.StringUtilities.SubstringSafe(chaine, positionsub, sizesub);

            //}


            //if (fcnData.ContainsKey("start"))
            //{
            //    chaine = chaine.Substring(Convert.ToInt32(fcnData["start"]));  //!!!!! vérifier 
            //}
            //if (fcnData.ContainsKey("right"))  //découpe à partir de la droite
            //{
            //    chaine = chaine.Substring(chaine.Length - Convert.ToInt32(fcnData["right"]));  //!!!!! vérifier 
            //}
            //if (fcnData.ContainsKey("left"))  //découpe à partir de la gauche
            //{
            //    int nb = Convert.ToInt32(fcnData["left"]);
            //    if (nb > chaine.Length) nb = chaine.Length;
            //    chaine = chaine.Substring(0, nb);
            //}



            //if (fcnData.ContainsKey("limit"))  //Limiter le nombre de caractères sans erreurs
            //{
            //    chaine = DATA.FORMAT.StringUtilities.Limit(chaine, Convert.ToInt32(fcnData["limit"]));
            //}




            //if (fcnData.ContainsKey("size"))
            //{
            //    bool size_forcer = true; if (fcnData.ContainsKey("lengthmax")) size_forcer = false; // si une taille max été défini on ne force pas.
            //    bool size_numerique = false; if (fcnData.ContainsKey("numerique")) size_numerique = true;

            //    chaine = DATA.FORMAT.StringUtilities.Complete(chaine, Convert.ToInt32(fcnData["size"]), size_numerique, size_forcer);
            //}


        }

        private static void Transform_replace(StringDynamizerContext context)
        {
            //chaine = chaine.Replace(Convert.ToString(fcnData["replace"]), Convert.ToString(fcnData["replacevalue"]));
        }

        private static void Transform_formatdate(StringDynamizerContext context)
        {
            DateTime chainedate = ConvertPlus.ToDateTime(context.FinalString);
            //context.FinalString = DateUtilities.fo(chainedate, Convert.ToString(fcnData["formatdate"]));
        }

        private static void Transform_atend(StringDynamizerContext context)
        {
            // chaine = chaine + fcnData["atend"];
        }

        private static void Transform_atstart(StringDynamizerContext context)
        {
            //chaine = fcnData["atstart"] + chaine;
        }

        private static void Transform_quotation(StringDynamizerContext context)
        {
            // met des guillemets : "toto"

            //string val = "\"";
            //string daterfg = Convert.ToString(fcnData["guillemets"]).ToLower();
            //if (daterfg != "true" || daterfg != "yes") val = daterfg;
            //chaine = val + chaine.Replace(val, "") + val; // On prend aussi soin de nétoyer la chaine

        }





        private static void Validate_regex(StringDynamizerContext context)
        {
            //System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(Convert.ToString(fcnData["regex"]));
            //if (!re.IsMatch(chaine)) throw new Exception("Validation REGEX");
        }
        private static void Validate_ismodulo(StringDynamizerContext context)
        {
            //string[] mrt = Convert.ToString(fcnData["modulo"]).Split('|');
            //if (mrt.Length < 2) throw new Exception("DEV param ismodulo invalide");
            //int nummodulo = Convert.ToInt32(mrt[0]);
            //string resmodulochaine = null, resmodulo = null;
            //if (mrt[1][0] == 'd')
            //{
            //    int lastpos = Convert.ToInt32(mrt[1][1]);
            //    resmodulochaine = chaine.Substring(chaine.Length - lastpos - 1, lastpos);
            //    resmodulo = "";// !!!calc
            //}


            //if (resmodulochaine != null && resmodulo != resmodulochaine) throw new Exception("Validation MODULO");
        }
        private static void Validate_lengthmax(StringDynamizerContext context)
        {
           // if (chaine.Length > Convert.ToInt32(fcnData["lengthmax"])) throw new Exception("Validation lenghtmax");
        }
        private static void Validate_validtype(StringDynamizerContext context)
        {
            //if (!System.Text.RegularExpressions.Regex.IsMatch(chaine, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")) throw new Exception("Validation MAIL");
        }
        private static void Validate_isempty(StringDynamizerContext context)
        {
            // if (string.IsNullOrWhiteSpace(chaine)) throw new Exception("Validation Null Empty WhiteSpace");
        }
        private static void Validate_disallowchar(StringDynamizerContext context)
        {
            // if (DATA.FORMAT.StringUtilities.ContainChars(chaine, Convert.ToString(fcnData["disallowchar"]))) throw new Exception("Validation Null Empty WhiteSpace");
        }




        #endregion



        #region --- DIVERS ---






        //public string TextCSVCut(string chaine, int Position, char separator = ';',  char? quote = null, bool safe = true)
        //{
        //    try
        //    {
        //        //chaine = '"' + chaine.Replace("\"", "") + '"';
        //        string[] chainedec = chaine.Split(separator);
        //        if (!safe && chainedec.Length < Position) throw new Exception("CSV Lengh, Position : " + Position + "/" + chainedec.Length);
        //        else if (safe && chainedec.Length < Position) return null;

        //        chaine = chainedec[Position];
        //        if (quote!=null && chaine.Length > 1 && chaine[0] == '"' && chaine[chaine.Length - 1] == '"') chaine = chaine.Substring(1, chaine.Length - 2); //suppression des guillemets
        //        return chaine;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("TextCSVCut "+ex.Message);
        //    }
        //}

        //public string TextPlainCut(string chaine, int Position, int Lenght, bool safe = true)
        //{
        //    try
        //    {
        //        if(chaine.Length < Position)
        //        {
        //            if (!safe  ) throw new Exception("TextPlain Lengh : " + Position + "/" + chaine.Length);
        //            else  return null;
        //        }
        //        if ((Position + Lenght) > chaine.Length) Lenght = chaine.Length - Position;

        //        return chaine.Substring(Position, Lenght);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("TextPlainCut " + ex.Message);
        //    }
        //}





        //public static Dictionary<string, object> fcnDataFromData(DATA.DATAVALUES.DataValues_data datav,  string prefix = "chaine_")
        //{
        //    Dictionary<string, object> fcnData = new Dictionary<string, object>();
        //    prefix = prefix.ToLower();
        //    // ---- POUR COMPATIBILITE ENCAISSEMENT DOCAPOST SEYES 2.x (renomme les noms d'actions)
        //    try
        //    {
        //        if (datav[prefix + "length"] != "") datav[prefix + "size"] = datav[prefix + "length"];// signifier que size=length
        //        if (datav[prefix + "completechamp"] != "") datav[prefix + "size"] = datav[prefix + "completechamp"];// signifier que size=completechamp
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }


        //    // ---- Pour les fonctions simples
        //    foreach (DATA.DATAVALUES.DataValues_attribut itematr in datav.getattributs(prefix))
        //        fcnData.Add(itematr.name.ToLower().Replace(prefix, ""), itematr.value);

        //    return fcnData;

        //}



        #endregion



    }
}
