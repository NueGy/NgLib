// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Outils pour manipulation des string
    /// </summary>
    public static class StringUtilities
    {


        #region Extraction / Génération

        public static string[] ExtractWords(string input)
        {
            return Regex.Split(input, @"\W+");
        }



        /// <summary>
        /// Permet de découper une chaine
        /// </summary>
        public static DATA.DATAVALUES.DataValues SplitString(string chaine, DATA.DATAVALUES.DataValues paramfordecoupe, string prefixdecoup = "decoupe_")
        {
            DATA.DATAVALUES.DataValues retour = new DATA.DATAVALUES.DataValues();

            foreach (DATA.DATAVALUES.DataValuesNode itemd in paramfordecoupe.GetList())
            {
                DATA.DATAVALUES.DataValuesNode nouveau = itemd.Clone();
                nouveau.Value = chaine;
                //nouveau.actionDynamisation();
                //nouveau.DynamiseLocalValue(false, true, null, prefixdecoup);
                retour.Add(nouveau);
            }
            return retour;
        }



        public static bool ContainChars(string chaine, string chars)
        {
            foreach (char charitem in chars) if (chaine.Contains(charitem)) return true;
            return false;
        }




        /// <summary>
        /// transforme une chaine pour etre compatible au chemin de fichier
        /// </summary>
        /// <param name="chaine"></param>
        /// <returns></returns>
        public static string FormatNameFile(string chaine)
        {
            chaine = chaine.Replace("/", "");
            chaine = chaine.Replace(@"\", "");
            chaine = chaine.Replace(":", "");
            chaine = chaine.Replace(" ", "");
            //!!!
            return chaine;
        }




        public static List<string> ExtractNumber(string chaine)
        {
            List<string> retour = new List<string>();
            string cumul = "";
            foreach (var c in chaine)
            {
                if (char.IsNumber(c))
                {
                    cumul += c;
                }
                else if (cumul != "")
                {
                    try { retour.Add(cumul); }
                    catch (Exception) { }
                    cumul = "";
                }
            }
            return retour;
        }



        /// <summary>
        /// Returns the initials of each word in a string. Words must be separated with spaces.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="capitalizeInitials">True to capitalize each initial in the output string.</param>
        /// <param name="preserveSpaces">True to preserver the spaces between initials in the output string.</param>
        /// <param name="includePeriod">True to include a '.' after each intiali</param>
        public static string GetInitials(string input, bool capitalizeInitials, bool preserveSpaces, bool includePeriod)
        {
            //Verify input
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string[] words = input.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    if (capitalizeInitials)
                        words[i] = char.ToUpper(words[i][0]).ToString(); //only keep the first letter
                    else
                        words[i] = words[i][0].ToString(); //only keep the first letter

                    if (includePeriod)
                        words[i] += ".";
                }
            }

            if (preserveSpaces)
                return string.Join(" ", words);
            else
                return string.Join("", words);
        }

        /// <summary>
        /// Génération d'une chaine aléatoire
        /// </summary>
        public static string GenerateString(int length, string chars = "abcdefghijklmnopqrstuvwxyz123456789")
        {
            try
            {
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static int lastseed = 25;
        private static Random random = new Random();


        public static string GenerateGuid32()
        {
            try
            {
                var tempGuid = Guid.NewGuid();
                var bytes = tempGuid.ToByteArray();
                var time = DateTime.Now;

                bytes[3] = (byte)time.Year;
                bytes[2] = (byte)time.Month;
                bytes[1] = (byte)time.Day;
                bytes[0] = (byte)time.Hour;
                bytes[5] = (byte)time.Minute;
                bytes[4] = (byte)time.Second;

                var CurrentGuid = new Guid(bytes);

                return CurrentGuid.ToString("N");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string[] SplitEncapsuled(string chaine, string startSeparator, string endSeparator)
        {
            List<string> retour = new List<string>();
            if (string.IsNullOrEmpty(chaine) || string.IsNullOrEmpty(startSeparator) || string.IsNullOrEmpty(endSeparator)) 
                return retour.ToArray();

            
            int positionchaine = 0;
            for (int iteration = 0; iteration < 999; iteration++) //on limite à 999 éléments dynamiques pour éviter les boucles folles
            {
                int positiondynstart = chaine.IndexOf(startSeparator, positionchaine, StringComparison.Ordinal);
                if (positiondynstart < 0) break; // rien trouvé
                int positiondynstop = chaine.IndexOf(endSeparator, positiondynstart, StringComparison.Ordinal) + endSeparator.Length - 1;
                positionchaine = positiondynstop; // permet de passer à la suite
                if (positiondynstop < positiondynstart) continue; // throw new Exception("erreur dans les découpes");
                int positiondyncount = positiondynstop - positiondynstart;
                if (positiondyncount < 2 || positiondyncount > 99) continue;  //throw new Exception("erreur dans les découpes (chaine dynamique trop grande ou trop petite)");

                string subchainedyn = chaine.Substring(positiondynstart, positiondyncount + 1);
                if (subchainedyn.IndexOf(startSeparator, startSeparator.Length, StringComparison.Ordinal) > 1) continue;// throw new Exception("pas fermé corectement");

                retour.Add(subchainedyn);
            }
            return retour.ToArray();
        }


        #endregion




        #region Validation


        public static bool IsValidForXML(string value)
        {
            int result = 0;
            bool isValid = true;

            //la chaine est vide
            if (string.IsNullOrWhiteSpace(value)) isValid = false;

            //la chaine fait moins de 3 caractéres
            if (value.Length < 3) isValid = false;

            //la chaine fait plus de 200 caractéres
            if (value.Length > 128) isValid = false;

            //le premier caractére est un nombre
            if (int.TryParse(value.Substring(1, 1), out result)) isValid = false;

            // la chaine contient au moins un espace
            if (value.Contains(" ")) isValid = false;

            //la chaine contient des caractéres spéciaux
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^ \\w]");
            if (regex.Match(value).Success) isValid = false;

            return isValid;
        }


        public static double Levenshtein(string S1, string S2)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(S1)) S1 = "";
                if (string.IsNullOrWhiteSpace(S2)) S2 = "";

                string SC1 = S1.ToUpper();
                string SC2 = S2.ToUpper();

                int n = SC1.Length;
                int m = SC2.Length;

                if (n + m == 0) return 100;
                else if (n == 0) return 0;
                else if (m == 0) return 0;


                int[,] d = new int[n + 1, m + 1];
                int cost = 0;

                for (int i = 0; i <= n; i++) d[i, 0] = i;
                for (int j = 0; j <= m; j++) d[0, j] = j;

                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        if (SC1[i - 1] == SC2[j - 1]) cost = 0;
                        else cost = 1;

                        d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                    }
                }

                return System.Math.Round((1.0 - ((double)d[n, m] / (double)System.Math.Max(n, m))) * 100.0, 2);
            }
            catch (Exception)
            {
                return -1;
            }

        }


        /// <summary>
        /// Returns whether a string is composed of only numeric characters.
        /// </summary>
        public static bool IsNumeric(string input)
        {
            //Verify input
            if (string.IsNullOrEmpty(input))
                return false;

            for (int i = 0; i < input.Length; i++)
            {
                if (!char.IsNumber(input[i]))
                    return false; //single non-numeric integer makes function false
            }
            return true;
        }


        /// <summary>
        /// Returns whether a string contains any numberic characters.
        /// </summary>
        public static bool HasNumeric(string input)
        {
            //Verify input
            if (string.IsNullOrEmpty(input))
                return false;

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsNumber(input[i]))
                    return true; //single numeric integer makes function true
            }
            return false;
        }



        /// <summary>
        /// Returns whether a string is composed of only letter and number characters.
        /// </summary>
        public static bool IsAlphaNumeric(string input)
        {
            //Verify input
            if (string.IsNullOrEmpty(input)) return false;

            for (int i = 0; i < input.Length; i++)
            {
                if (!char.IsLetter(input[i]) && !char.IsNumber(input[i]))
                    return false;
            }
            return true;
        }


        #endregion




        #region Manipulation de la chaine



        public static string AllowCharacters(string original, string characters="azertyuiopqsdfghjklmwxcvbnAZERTYUIOPQSDFGHJKLMWXCVBN0123456789")
        {
            StringBuilder retour = new StringBuilder();
            foreach (char item in original)
            {
                if (characters.Contains(item)) retour.Append(item);
            }
            return retour.ToString();
        }



        /// <summary>
        /// Limiter la taille d'une chaine string + supprimer les retour chariot, ...
        /// Gere meme si la taille de la chaine est plus petite que la limite
        /// </summary>
        public static string Limit(string original, int num, bool cleanupstring=true)
        {
            if (original == null) return null;
            int nb = original.Length;
            if (nb > num) nb = num;
            original = original.Substring(0, nb);
            if (cleanupstring) original = original.Replace("\r", "").Replace("\n", " ").Replace("\t", " ");
            return original;
        }



        public static string SubstringSafe(string original, int Position)
        {
            int originalLength = original.Length;
            if (originalLength < Position) return string.Empty; // trop loin
            original = original.Substring(Position);
            return original;
        }

        public static string SubstringSafe(string original, int Position, int lenght)
        {
            int originalLength = original.Length;
            if (originalLength < Position) return string.Empty; // trop loin
            if (originalLength < lenght + Position) lenght = originalLength - Position; // pas assez de caractere

            original = original.Substring(Position, lenght);
            return original;
        }



        /// <summary>
        /// Capitalizes the first character in a string.
        /// </summary>
        public static string Capitalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (input.Length == 1) return input.ToUpper();

            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        /// <summary>
        /// Returns a string with each word's first character capitalized. Words are separated according to the sepecified string sequence.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="separator">The string sequence that separates words.</param>
        public static string GetTitle(string input, string separator)
        {
            //Verify input
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string[] words = input.Split(separator.ToCharArray());

            for (int i = 0; i < words.Length; i++)
            {
                //Capitalize each word
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]).ToString() + words[i].Substring(1);
            }

            return string.Join(separator, words);
        }



        /// <summary>
        /// Permet de gerer des lignes de textes bruts multichamps
        /// </summary>
        /// <param name="value">valeur</param>
        /// <param name="nbspace">nombre de caracteres sur le champs</param>
        /// <param name="numerique">champs numérique(gestion des zéros)</param>
        /// <param name="forcer">true : ne leve pas d'exeption si valeur tronqué</param>
        /// <param name="name">nom du champs, facultatif</param>
        public static string Complete(string value, int nbspace, bool numerique, bool forcer = false, string name = "")
        {
            if (value == null) value = "";
            value = value.Replace('\n', ' ');
            value = value.Replace('\r', ' ');
            int taille = value.Length;
            if (value == null) value = "";
            if (value.Length > nbspace)
            {
                if (!forcer) throw new Exception(" Valeur tronqué ... (" + name + " " + taille + "/" + nbspace + " : " + value + ")");
                else { value = value.Substring(0, nbspace); taille = nbspace; }

            }
            if (!numerique || (value == "" && numerique)) for (int i = 0; i < (nbspace - taille); i++) value += " ";
            else if (value != "" && numerique) for (int i = 0; i < (nbspace - taille); i++) value = "0" + value;
            return value;



            // !!! optimiser : string.padleft ...
        }




        public static string removeDiacritics(string inputString)
        {
            string result = inputString;
            result = result.Replace('à', 'a');
            result = result.Replace('á', 'a');
            result = result.Replace('ä', 'a');
            result = result.Replace('â', 'a');
            result = result.Replace('ã', 'a');
            result = result.Replace('å', 'a');
            result = result.Replace('é', 'e');
            result = result.Replace('è', 'e');
            result = result.Replace('ê', 'e');
            result = result.Replace('ë', 'e');
            result = result.Replace('ì', 'i');
            result = result.Replace('í', 'i');
            result = result.Replace('ï', 'i');
            result = result.Replace('î', 'i');
            result = result.Replace('ò', 'o');
            result = result.Replace('ó', 'o');
            result = result.Replace('ô', 'o');
            result = result.Replace('ö', 'o');
            result = result.Replace('û', 'u');
            result = result.Replace('ü', 'u');
            result = result.Replace('ù', 'u');
            result = result.Replace('ú', 'u');
            result = result.Replace('ý', 'y');
            result = result.Replace('ÿ', 'y');
            result = result.Replace('ç', 'c');
            result = result.Replace('ñ', 'n');
            return result;
        }




            /// <summary>
        /// Returns a string with characters in reverse order.
        /// </summary>
        public static string Reverse(string input)
        {
            //Validate input
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            char[] outputChars = input.ToCharArray();

            //Reverse
            Array.Reverse(outputChars);

            //build a string from the processed characters and return it
            return new string(outputChars);
        }

        /// <summary>
        /// Permet de supprimer les balises d'une chaine de caratères
        /// </summary>
        /// <param name="pTaggedText"></param>
        /// <returns></returns>
        public static string StripTags(string pTaggedText)
        {
            return StripTags(pTaggedText, new string[] { });
        }
        /// <summary>
        /// Permet de supprimer les balises d'une chaine de caratères
        /// </summary>
        /// <param name="pTaggedText"></param>
        /// <returns></returns>
        public static string StripTags(string pTaggedText, string[] pTagsToStrip)
        {
            if (pTagsToStrip.Length == 0) //strip all tags
            {
                Regex rx = new Regex("<[^>]+>");
                string resultText = rx.Replace(pTaggedText, "");

                return resultText;
            }
            else //strip only specified tags
            {
                string tagsToStrip = "";
                for (int s = 0; s < pTagsToStrip.Length; s++)
                {
                    if (s > 0) { tagsToStrip += "|"; }
                    tagsToStrip += pTagsToStrip[s];
                }
                Regex rx = new Regex("</?(?i:" + tagsToStrip + ")([^>]*>");
                string resultText = rx.Replace(pTaggedText, "");

                return resultText;
            }
        }






        #endregion




        #region Tableaux de chaines


        /// <summary>
        /// Returns a string with a given seperator inserted after every character.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="separator">The separator to insert.</param>
        public static string InsertSeparator(string input, string separator)
        {
            //Validate string
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            List<char> outputChars = new List<char>(input.ToCharArray());
            char[] separatorChars = separator.ToCharArray();

            int i = 1;
            while (i < outputChars.Count)
            {
                if (i != outputChars.Count) //don't add separator to the end of string
                    outputChars.InsertRange(i, separatorChars);

                i += 1 + separator.Length; //go up the interval amount plus separator
            }

            return new string(outputChars.ToArray());
        }


        /// <summary>
        /// Returns a string with a given seperator inserted after a specified interval of characters.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="separator">The separator to insert.</param>
        /// <param name="interval">The number of characters between separators.</param>
        public static string InsertSeparator(string input, string separator, int interval=1)
        {
            //Validate string
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            List<char> outputChars = new List<char>(input.ToCharArray());
            char[] separatorChars = separator.ToCharArray();

            int i = interval;
            while (i < outputChars.Count)
            {
                if (i != outputChars.Count) //don't add separator to the end of string
                    outputChars.InsertRange(i, separatorChars);

                i += interval + separator.Length; //go up the interval amount plus separator
            }

            return new string(outputChars.ToArray());
        }








        #endregion





        /// <summary>
        /// Permet de nétoyer un string (supprime les blanc, remplace les accent, to lower, ...)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string CleanStringForId(this string item)
        {
            return null;
        }





        /// <summary>
        /// Remplacer un caractere dans la chaine, gestion si vide
        /// </summary>
        /// <param name="orgn"></param>
        /// <param name="pos"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ReplaceChar(this string orgn, int pos, char c)
        {
            StringBuilder sb = new StringBuilder(orgn);
            if (sb.Length < pos + 1) sb.Append(' ');
            sb[pos] = c;
            return sb.ToString();
        }




      


    }
}
