using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.FORMAT
{
    public static class KeyTools
    {
        /// <summary>
        /// Prépare un string pour une clef (supprime les blanc, remplace les accent, to lower, ...)
        /// AllowCharacters = azertyuiopqsdfghjklmwxcvbn1234567890_
        /// </summary>
        public static string CleanStringForKey(string orgnStr)
        {
            if (string.IsNullOrWhiteSpace(orgnStr)) return null;
            orgnStr = StringTools.CleanString(orgnStr);
            orgnStr = orgnStr.Replace(" ", "").Replace("-", "_").Replace(".", "_").ToLowerInvariant(); // pas de blancs, pas de tiret, pas de point
            orgnStr = StringTools.ReplaceDiacritics(orgnStr); // Accent interdit;
            orgnStr = StringTools.FilterCharacters(orgnStr, "azertyuiopqsdfghjklmwxcvbn1234567890_");
            return orgnStr;
        }


        /// <summary>
        ///  Permet de générer une clef composé avec un caractère checksum en fin
        /// </summary>
        /// <param name="DateIndex"></param>
        /// <param name="tenantId"></param>
        /// <param name="itemId"></param>
        /// <param name="keyGroup"></param>
        /// <returns></returns>
        public static string WriteKeyB36(DateTime? DateIndex, int tenantId, long itemId)
        {   // 000 000 000 000
            char SpecialCommandParam = '1';  // 0: Pas de date, 1:date année 2000, 2: advancedParam
            string dateindexpart = "";
            if (DateIndex == null)
            {
                SpecialCommandParam = '0';
            }
            else if (DateIndex.Value.Year.ToString().StartsWith("20"))
            {
                SpecialCommandParam = '1';
                dateindexpart = DateIndex.Value.ToString("yy") + DateIndex.Value.DayOfYear.ToString().PadLeft(3, '0');
            }
            else
                throw new Exception("WriteFullIdB36: Date Non gérée");
            string fullWithoutSign = dateindexpart + tenantId.ToString() + itemId.ToString();
            char Checksum = CheckModulo11Digit(fullWithoutSign); //6


            string p2 = ToBase36(Convert.ToInt64(tenantId + dateindexpart)); //5WLS8
            int p2size = p2.Length;
            if (SpecialCommandParam != '0') p2size = p2size - 2; // on as le droit qu'a 9 char dans le p1, alors on économise la zone date
            string p3 = ToBase36(itemId); //J1V5
            string p1 = SpecialCommandParam + p2size.ToString();
            p1 = ToBase36(Convert.ToInt64(p1)); //F
            p1 = (string.IsNullOrEmpty(p1)) ? "0" : p1;
            return (p1 + p2 + p3 + Checksum);
        }



        /// <summary>
        /// Parser un clef
        /// </summary>
        /// <param name="fullid">val</param>
        /// <param name="IgnoreFirstChar">Ignorer les premiers charactères</param>
        /// <returns></returns>
        public static Tuple<DateTime, int, long> ParseKeyB36(string fullid, int IgnoreFirstChars = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullid)) return null;
                fullid = fullid.Trim();
                if (IgnoreFirstChars > 0)
                {
                    if (IgnoreFirstChars >= fullid.Length) return null;
                    fullid = fullid.Substring(IgnoreFirstChars);
                }

                bool EncryptedMode = fullid.StartsWith("Z");
                bool AdvancedMode = fullid.StartsWith("X");
                char Checksum = '0';
                string p1 = FromBase36(fullid.Substring(0, 1)).ToString(); //p1 à une taille de 36 maxi
                int sizep2 = Convert.ToInt32(p1[p1.Length - 1].ToString());
                if (!(p1.Length == 1 || p1.StartsWith("0"))) sizep2 = sizep2 + 2;
                string p2 = fullid.Substring(1, sizep2);
                p2 = FromBase36(p2).ToString();
                int TenantId = 0;

                string p3str = fullid.Substring((sizep2 + 1), (fullid.Length - (sizep2 + 1) - 1));
                long p3itemId = FromBase36(p3str);
                DateTime DateIndex = DateTime.MinValue;
                if (p1.Length == 1 || p1.StartsWith("0"))
                {
                    // pas de dateindex et peut-être pas de tenantid
                    TenantId = Convert.ToInt32(p2);
                    string fullWithoutSign = "" + TenantId.ToString() + p3itemId.ToString();
                    Checksum = CheckModulo11Digit(fullWithoutSign);
                }
                else if (p1.StartsWith("1"))
                {
                    string yearstr = "20" + p2.Substring(p2.Length - 5, 2);
                    DateIndex = new DateTime(Convert.ToInt32(yearstr), 1, 1);
                    DateIndex = DateIndex.AddDays(Convert.ToInt32(p2.Substring(p2.Length - 3, 3)) - 1);
                    TenantId = Convert.ToInt32(p2.Substring(0, p2.Length - 5));
                    string fullWithoutSign = p2.Substring(p2.Length - 5, 5) + TenantId.ToString() + p3itemId.ToString();
                    Checksum = CheckModulo11Digit(fullWithoutSign);
                }
                else if (p1.StartsWith("2")) // Gestion de date avec l'année entiere
                {
                    return null; // non disponible
                }

                if (Checksum != fullid[fullid.Length - 1]) return null; // Checksum invalid

                return new Tuple<DateTime, int, long>(DateIndex, TenantId, p3itemId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Parser un clef
        /// </summary>
        /// <param name="fullid"></param>
        /// <returns></returns>
        public static Tuple<DateTime, int, long> ParsekeyDigits(string fullid)
        {
            try
            {
                if (fullid == null) return null;
                fullid = fullid.Trim();
                if (string.IsNullOrEmpty(fullid) || fullid.Length != 30) return null;
                char Checksum = '0';
                string fullWithoutSign = fullid.Substring(0, 29);
                Checksum = CheckModulo11Digit(fullWithoutSign);
                if (Checksum != fullid[29]) return null; // Checksum invalid
                DateTime DateIndex = new DateTime(Convert.ToInt32(fullid.Substring(0, 4)), Convert.ToInt32(fullid.Substring(4, 2)), Convert.ToInt32(fullid.Substring(6, 2)));
                int TenantId = Convert.ToInt32(fullid.Substring(8, 9));
                long itemId = Convert.ToInt64(fullid.Substring(17, 12));
                return new Tuple<DateTime, int, long>(DateIndex, TenantId, itemId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        /// <summary>
        ///  Permet de générer une clef composé avec un caractère checksum en fin
        /// </summary>
        /// <param name="DateIndex"></param>
        /// <param name="tenantId"></param>
        /// <param name="itemId"></param>
        /// <param name="keyGroup"></param>
        /// <returns></returns>
        public static string WriteKeyDigits(DateTime DateIndex, int tenantId, long itemId) // 8+9+12+1=30
        {   // 000 000 000 000
            string fullWithoutSign = DateIndex.ToString("yyyyMMdd") + tenantId.ToString().PadLeft(9, '0') + itemId.ToString().PadLeft(12, '0');
            char Checksum = CheckModulo11Digit(fullWithoutSign);
            return (fullWithoutSign + Checksum);
        }





        public static char CheckModulo11Digit(string inputNumbers)
        {
            //https://fr.activebarcode.com/codes/checkdigit/modulo11.html
            //int sum = 0;
            //for (int i = inputNumbers.Length - 1, multiplier = 2; i >= 0; i--)
            //{
            //    sum += (int)char.GetNumericValue(inputNumbers[i]) * multiplier;
            //    if (++multiplier > 9) multiplier = 2;
            //}
            //int mod = (sum % 11);
            //if (mod == 0 || mod == 1) return '0';
            //return (11 - mod).ToString()[0];

            int Sum = 0;
            for (int i = inputNumbers.Length - 1, Multiplier = 2; i >= 0; i--)
            {
                Sum += (int)char.GetNumericValue(inputNumbers[i]) * Multiplier;

                if (++Multiplier == 8) Multiplier = 2;
            }
            string Validator = (11 - (Sum % 11)).ToString();

            //if (Validator == "11") Validator = "0";
            //else if (Validator == "10") Validator = "X";

            return Validator[0];

        }



        //private const string CharList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Encode the given number into a Base36 string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToBase36(long input)
        {
            //https://www.stum.de/2008/10/20/base36-encoderdecoder-in-c/
            if (input < 0) throw new ArgumentOutOfRangeException("input", input, "input cannot be negative");

            char[] clistarr = CharList.ToCharArray();
            var result = new Stack<char>();
            while (input != 0)
            {
                result.Push(clistarr[input % 36]);
                input /= 36;
            }
            return new string(result.ToArray()).ToUpper();
        }

        /// <summary>
        /// Decode the Base36 Encoded string into a number
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static long FromBase36(string input)
        {
            var reversed = input.ToLower().Reverse();
            long result = 0;
            int pos = 0;
            foreach (char c in reversed)
            {
                result += CharList.IndexOf(c) * (long)Math.Pow(36, pos);
                pos++;
            }
            return result;
        }
    }
}
