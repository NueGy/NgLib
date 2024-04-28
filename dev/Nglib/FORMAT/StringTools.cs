// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Text.RegularExpressions;

namespace Nglib.FORMAT
{
    /// <summary>
    ///     Outils pour manipulation des string
    /// </summary>
    public static class StringTools
    {
        public const string removeChars = "?&^$#@!<>’\'*�";// inclus les problèmes d'encodage
        public const string AllCharsConst = "azertyuiopqsdfghjklmwxcvbnAZERTYUIOPQSDFGHJKLMWXCVBN0123456789";

        private static int lastseed = 25;
        private static readonly Random random = new();


        /// <summary>
        ///     Génération d'une chaine aléatoire
        /// </summary>
        public static string GenerateString(int length, string chars = "abcdefghijklmnopqrstuvwxyz123456789")
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        ///     Génération d'une chaine aléatoire
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
        ///     Permet de séparer des chaines de caractères
        ///     exemple "aaaa{bbbbb}ccccc" => "{bbbbb}"
        /// </summary>
        /// <param name="chaine"></param>
        /// <param name="startSeparator"></param>
        /// <param name="endSeparator"></param>
        /// <returns></returns>
        public static string[] SplitEncapsuled(string chaine, string startSeparator, string endSeparator)
        {
            var retour = new List<string>();
            if (string.IsNullOrEmpty(chaine) || string.IsNullOrEmpty(startSeparator) ||
                string.IsNullOrEmpty(endSeparator))
                return retour.ToArray();


            var positionchaine = 0;
            for (var iteration = 0;
                 iteration < 999;
                 iteration++) //on limite à 999 éléments dynamiques pour éviter les boucles folles
            {
                var positiondynstart = chaine.IndexOf(startSeparator, positionchaine, StringComparison.Ordinal);
                if (positiondynstart < 0) break; // rien trouvé
                var positiondynstop = chaine.IndexOf(endSeparator, positiondynstart, StringComparison.Ordinal) +
                    endSeparator.Length - 1;
                positionchaine = positiondynstop; // permet de passer à la suite
                if (positiondynstop < positiondynstart) continue; // throw new Exception("erreur dans les découpes");
                var positiondyncount = positiondynstop - positiondynstart;
                if (positiondyncount < 2 || positiondyncount > 99)
                    continue; //throw new Exception("erreur dans les découpes (chaine dynamique trop grande ou trop petite)");

                var subchainedyn = chaine.Substring(positiondynstart, positiondyncount + 1);
                if (subchainedyn.IndexOf(startSeparator, startSeparator.Length, StringComparison.Ordinal) > 1)
                    continue; // throw new Exception("pas fermé corectement");

                retour.Add(subchainedyn);
            }

            return retour.ToArray();
        }



        /// <summary>
        ///     Contient que des lettres et des nombres
        /// </summary>
        public static bool IsAlphaNumeric(string input)
        {
            //Verify input
            if (string.IsNullOrEmpty(input)) return false;

            for (var i = 0; i < input.Length; i++)
                if (!char.IsLetter(input[i]) && !char.IsNumber(input[i]))
                    return false;
            return true;
        }


        /// <summary>
        ///     Permet de filtrer certain caractères uniquements
        /// </summary>
        /// <param name="original"></param>
        /// <param name="characters"></param>
        /// <returns></returns>
        public static string FilterCharacters(string original, string characters = AllCharsConst)
        {
            var retour = new StringBuilder();
            foreach (var item in original)
                if (characters.Contains(item))
                    retour.Append(item);
            return retour.ToString();
        }


        /// <summary>
        ///     Limiter la taille d'une chaine string + supprimer les retour chariot, ...
        ///     Gere meme si la taille de la chaine est plus petite que la limite
        /// </summary>
        public static string Limit(string original, int num)
        {
            if (original == null) return null;
            var nb = original.Length;
            if (nb > num) nb = num;
            original = original.Substring(0, nb);
            return original;
        }


        /// <summary>
        ///     Permet de découper une chaine avec gestion du outRange
        /// </summary>
        public static string SubstringSafe(string original, int Position)
        {
            var originalLength = original.Length;
            if (originalLength < Position) return string.Empty; // trop loin
            original = original.Substring(Position);
            return original;
        }

        /// <summary>
        ///     Permet de découper une chaine avec gestion du outRange
        /// </summary>
        public static string SubstringSafe(string original, int Position, int lenght)
        {
            var originalLength = original.Length;
            if (originalLength < Position) return string.Empty; // trop loin
            if (originalLength < lenght + Position) lenght = originalLength - Position; // pas assez de caractere
            original = original.Substring(Position, lenght);
            return original;
        }


        /// <summary>
        ///     PadLeft pour les nombres
        /// </summary>
        /// <param name="value">valeur</param>
        /// <param name="totalWith">nombre de caracteres sur le champs</param>
        public static string PadNumeric(string value, int totalWith)
        {
            if (value == null) value = ""; // jamais null
            value = value.Replace(" ", ""); // on supprime aussi les espaces (gardera les , et .)
            if (value.Length > totalWith)
                return Limit(value, totalWith);
            
            value = value.PadLeft(totalWith, '0'); // on ajoute les zero sur la gauche
            return value;
        }


        /// <summary>
        ///     Permet de remplacer les accents  à=>a
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string ReplaceDiacritics(string inputString)
        {
            var result = inputString;
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
        ///     Supprime tous les caractères (xml,html, saut de ligne, ...)
        /// </summary>
        /// <param name="orgnStr"></param>
        /// <returns></returns>
        public static string CleanString(string orgnStr)
        {
            if(orgnStr==null) return null;
            if (string.IsNullOrWhiteSpace(orgnStr)) return string.Empty;
            orgnStr = orgnStr.Trim().Replace("\r", " ").Replace("\n", "").Replace("\t", "");
            
            foreach (char c in removeChars)
                orgnStr = orgnStr.Replace(c.ToString(), string.Empty);

            // Améliorer en utilisant un regex ou  autre pour les perf
            //https://stackoverflow.com/questions/11395775/clean-the-string-is-there-any-better-way-of-doing-it
            return orgnStr.Trim();
        }


        /// <summary>
        ///     Remplacer un caractere dans la chaine, gestion si vide
        /// </summary>
        public static string ReplaceChar(this string orgn, int pos, char c)
        {
            var sb = new StringBuilder(orgn);
            if (sb.Length < pos + 1) sb.Append(' ');
            sb[pos] = c;
            return sb.ToString();
        }



        /// <summary>
        /// Ajouter une valeur à une position dans un StringBuilder
        /// </summary>
        public static void StringSetValuePosition(StringBuilder builder, int position, string value, int lenght=0)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (position < 1 || value == null) return; // ignore
            if (lenght==0) lenght = value.Length; // on prend la taille de la valeur (si non spécifié)

            position = position - 1; //Real position is 1 based
            value = Limit(value, lenght);
            value = value?.PadRight(lenght, ' ');

            if (position >= builder.Length)
            {
                builder.Append(value);
            }
            else
            {
                builder.Remove(position, value.Length);
                builder.Insert(position, value);
            }
        }





        /// <summary>
        /// Permet de découper des valeurs: Separateur csv ';';
        /// Pour la gestion des tags, vides interdit
        /// </summary>
        public static string[] SplitTag(string valuesstr, bool toUpper = true, bool neverNull=false)
        {
            if (valuesstr == null) return (neverNull) ? new string[0]:null;
            if (string.IsNullOrWhiteSpace(valuesstr)) return new string[0];
            string[] retour = valuesstr.Split(';', StringSplitOptions.None).ToArray();
            retour = retour.Select(x => CleanString(x)).ToArray();
            if(toUpper) retour = retour.Select(x => x.ToUpperInvariant()).ToArray();
            return retour;
        }






    }
}