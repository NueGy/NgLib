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
    public static class StringTools
    {

        private static int lastseed = 25;
        private static Random random = new Random();


        /// <summary>
        /// Génération d'une chaine aléatoire
        /// </summary>
        public static string GenerateString(int length, string chars = "abcdefghijklmnopqrstuvwxyz123456789")
        {
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Génération d'une chaine aléatoire
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// Permet de séparer des chaines de caractères 
        /// exemple "aaaa{bbbbb}ccccc" => "{bbbbb}"
        /// </summary>
        /// <param name="chaine"></param>
        /// <param name="startSeparator"></param>
        /// <param name="endSeparator"></param>
        /// <returns></returns>
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




        /// <summary>
        /// La distance Levenshtein est définie comme le nombre minimal de caractères qu'il faut remplacer, insérer ou supprimer pour transformer la chaîne str1 en str2.
        /// </summary>
        /// <param name="S1"></param>
        /// <param name="S2"></param>
        /// <returns></returns>
        public static double LevenshteinCompare(string S1, string S2)
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
        /// la chaine contient au moin un nombre
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
        /// La chaine est totalement numérique
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
        /// Contient que des lettres et des nombres
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


        /// <summary>
        /// Permet de filtrer certain caractères uniquements
        /// </summary>
        /// <param name="original"></param>
        /// <param name="characters"></param>
        /// <returns></returns>
        public static string FilterCharacters(string original, string characters="azertyuiopqsdfghjklmwxcvbnAZERTYUIOPQSDFGHJKLMWXCVBN0123456789")
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
            if (cleanupstring) original = CleanString(original); // on nétoie le string avant de le limiter
            if (original == null) return null;
            int nb = original.Length;
            if (nb > num) nb = num;
            original = original.Substring(0, nb);
            return original;
        }


        /// <summary>
        /// Permet de découper une chaine avec gestion du outRange
        /// </summary>
        /// <param name="original"></param>
        /// <param name="Position"></param>
        /// <returns></returns>
        public static string SubstringSafe(string original, int Position)
        {
            int originalLength = original.Length;
            if (originalLength < Position) return string.Empty; // trop loin
            original = original.Substring(Position);
            return original;
        }

        /// <summary>
        /// Permet de découper une chaine avec gestion du outRange
        /// </summary>
        /// <param name="original"></param>
        /// <param name="Position"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public static string SubstringSafe(string original, int Position, int lenght)
        {
            int originalLength = original.Length;
            if (originalLength < Position) return string.Empty; // trop loin
            if (originalLength < lenght + Position) lenght = originalLength - Position; // pas assez de caractere

            original = original.Substring(Position, lenght);
            return original;
        }









        /// <summary>
        /// PadLeft/PadRight + Limit
        /// </summary>
        /// <param name="value">valeur</param>
        /// <param name="totalWith">nombre de caracteres sur le champs</param>
        /// <param name="numerique">champs numérique(gestion des zéros)</param>
        public static string Complete(string value, int totalWith, bool numerique = true)
        {
            if (value == null) value=""; // jamais null
            if (value.Length > totalWith) return Limit(value, totalWith, true);
            else
            {
                value = CleanString(value);
                if (numerique) value = value.PadLeft(totalWith, '0'); // on ajoute les zero sur la gauche
                else value = value.PadRight(totalWith, ' '); // on ajoute des blancs sur la droite
                return value;
            }
        }



        /// <summary>
        /// Permet de remplacer les accents  à=>a
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string ReplaceDiacritics(string inputString)
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
        /// Permet de supprimer les balises d'une chaine de caratères
        /// </summary>
        /// <param name="orgnStr">Chaine d'origine</param>
        /// <param name="pTagsToStrip">tag spécifique seulements</param>
        /// <returns></returns>
        public static string StripTags(string orgnStr, string[] pTagsToStrip=null)
        {
            if (pTagsToStrip==null || pTagsToStrip.Length == 0) //strip all tags
            {
                Regex rx = new Regex("<[^>]+>");
                string resultText = rx.Replace(orgnStr, "");

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
                string resultText = rx.Replace(orgnStr, "");

                return resultText;
            }
        }








        /// <summary>
        /// Supprime tous les caractères (xml,html, saut de ligne, ...)
        /// </summary>
        /// <param name="orgnStr"></param>
        /// <returns></returns>
        public static string CleanString(string orgnStr)
        {
            if (string.IsNullOrWhiteSpace(orgnStr)) return null;
            orgnStr = orgnStr.Trim().Replace("\r", " ").Replace("\n", "").Replace("\t", "");
            orgnStr = orgnStr.Replace("�", "").Replace("?", ""); // preblemen d'encodage

            Regex rx = new Regex("<[^>]+>");
            orgnStr = rx.Replace(orgnStr, "");// supprimer les balises

            // Améliorer en utilisant un regex ou  autre pour les perf
            //https://stackoverflow.com/questions/11395775/clean-the-string-is-there-any-better-way-of-doing-it
            return orgnStr;
        }



        /// <summary>
        /// Remplacer un caractere dans la chaine, gestion si vide
        /// </summary>
        public static string ReplaceChar(this string orgn, int pos, char c)
        {
            StringBuilder sb = new StringBuilder(orgn);
            if (sb.Length < pos + 1) sb.Append(' ');
            sb[pos] = c;
            return sb.ToString();
        }




      


    }
}
