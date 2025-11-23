using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nglib.SECURITY.CRYPTO;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Utility class for key manipulation and sanitization.
    /// Documentation: <see href="https://github.com/NueGy/NgLibComponents/wiki/wiki_components_format"/>
    /// </summary>
    public static class KeyTools
    {
        //private const string CharList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";
        internal static readonly HashSet<char> KeysCharsConst = new(StringTools.AlphaNumCharsConst + "._");

        /// <summary>
        /// Sanitizes a key string allowing only alphanumeric chars, dots, underscores and dashes (a-z A-Z 0-9 ._-)
        /// Combines diacritic replacement and character filtering in a single pass for optimal performance
        /// </summary>
        /// <param name="orgnStr">Original string to sanitize</param>
        /// <param name="upperStr">Convert to uppercase if true</param>
        /// <returns>Sanitized key string or null if input is null</returns>
        public static string SanitizeKey(string orgnStr, bool upperStr = true)
        {
            if (orgnStr == null) return null;
            if (string.IsNullOrWhiteSpace(orgnStr)) return string.Empty; 
    
            var sb = new StringBuilder(orgnStr.Length);
            foreach (char c in orgnStr)
            {
                char processedChar = StringTools.DiacriticsMap.TryGetValue(c, out char replacement) ? replacement : c;
                if (upperStr)
                    processedChar = char.ToUpperInvariant(processedChar);
                if (KeyTools.KeysCharsConst.Contains(processedChar))
                    sb.Append(processedChar);
            }
            return sb.ToString();
        }


        /// <summary>
        /// Validates that a key respects the allowed characters.
        /// Allowed characters: alphanumeric + period + underscore (a-z A-Z 0-9 . _)
        /// </summary>
        /// <param name="key">Key to validate</param>
        /// <returns>True if the key is valid, False otherwise</returns>
        public static bool IsValidKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            
            // Check each character against the allowed character set
            return key.All(c => KeysCharsConst.Contains(c));
        }



        /// <summary>
        /// Generates a base36 key composed of 3 values with a checksum
        /// </summary>
        /// <param name="DateIndex">Date (optional)</param>
        /// <param name="tenantId">Tenant/Group ID</param>
        /// <param name="itemId">Unique identifier</param>
        /// <param name="prefix">Adds a prefix xx- to the key</param>
        /// <returns>Base36 key string</returns>
        [Obsolete("BETA")]
        public static string WriteKeyB36(DateTime? DateIndex, int tenantId, long itemId, string prefix = null)
        {
            try
            {
                if (itemId == 0) return null;
                var SpecialCommandParam = '1'; // 0: No date, 1:year 2000 date, 2: advancedParam
                var dateindexpart = "";
                if (DateIndex == null)
                {
                    SpecialCommandParam = '0';
                }
                else if (DateIndex.Value.Year.ToString().StartsWith("20"))
                {
                    SpecialCommandParam = '1';
                    dateindexpart = DateIndex.Value.ToString("yy") +
                                    DateIndex.Value.DayOfYear.ToString().PadLeft(3, '0');
                }
                else
                {
                    throw new Exception("Date not managed");
                }

                var fullWithoutSign = dateindexpart + tenantId + itemId;
                var Checksum = CheckModulo11Digit(fullWithoutSign); //6


                var p2 = ToBase36(Convert.ToInt64(tenantId + dateindexpart)); //5WLS8
                var p2size = p2.Length;
                if (SpecialCommandParam != '0')
                    p2size = p2size - 2; // we only have rights to 9 chars in p1, so we save the date zone
                var p3 = ToBase36(itemId); //J1V5
                var p1 = SpecialCommandParam + p2size.ToString();
                p1 = ToBase36(Convert.ToInt64(p1)); //F
                p1 = string.IsNullOrEmpty(p1) ? "0" : p1;

                if (prefix != null) prefix = SanitizeKey(prefix);
                prefix = prefix != null ? prefix + "-" : "";
                return prefix + p1 + p2 + p3 + Checksum;
            }
            catch (Exception ex)
            {
                throw new Exception("WriteKeyB36 " + ex.Message);
            }
        }


        /// <summary>
        /// Parser un clef
        /// </summary>
        /// <param name="fullid">val</param>
        /// <returns></returns>
        [Obsolete("BETA")]
        public static KeyB36 ParseKeyB36(string fullid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullid)) return new KeyB36();
                fullid = fullid.Trim();
                if (fullid.Length < 4) return new KeyB36(); // taille impossible
                var retourk = new KeyB36();
                if (fullid.Contains("-")) // Gestion du prefix
                {
                    var idt = fullid.Split('-');
                    retourk.Prefix = idt[0];
                    fullid = idt[1];
                    if (fullid.Length < 4) return new KeyB36(); // taille impossible
                }

                var Checksum = '0';
                var p1 = FromBase36(fullid.Substring(0, 1)).ToString(); //p1 à une taille de 36 maxi
                var sizep2 = Convert.ToInt32(p1[p1.Length - 1].ToString());
                if (!(p1.Length == 1 || p1.StartsWith("0"))) sizep2 = sizep2 + 2;
                var p2 = fullid.Substring(1, sizep2);
                p2 = FromBase36(p2).ToString();


                var p3str = fullid.Substring(sizep2 + 1, fullid.Length - (sizep2 + 1) - 1);
                retourk.ItemId = FromBase36(p3str);
                retourk.DateIndex = DateTime.MinValue;
                if (p1.Length == 1 || p1.StartsWith("0"))
                {
                    // pas de dateindex et peut-être pas de tenantid
                    retourk.TenantId = Convert.ToInt32(p2);
                    var fullWithoutSign = "" + retourk.TenantId + retourk.ItemId;
                    Checksum = CheckModulo11Digit(fullWithoutSign);
                }
                else if (p1.StartsWith("1"))
                {
                    // date en 20xx (mode pour économiser des chars)
                    var yearstr = "20" + p2.Substring(p2.Length - 5, 2);
                    retourk.DateIndex = new DateTime(Convert.ToInt32(yearstr), 1, 1);
                    retourk.DateIndex = retourk.DateIndex.AddDays(Convert.ToInt32(p2.Substring(p2.Length - 3, 3)) - 1);
                    retourk.TenantId = Convert.ToInt32(p2.Substring(0, p2.Length - 5));
                    var fullWithoutSign = p2.Substring(p2.Length - 5, 5) + retourk.TenantId + retourk.ItemId;
                    Checksum = CheckModulo11Digit(fullWithoutSign);
                }
                else if (p1.StartsWith("2")) // Gestion de date avec l'année entiere
                {
                    //!!!
                    return new KeyB36(); // non disponible
                }

                if (Checksum != fullid[fullid.Length - 1]) retourk.IsValid = false; // Checksum invalid
                else retourk.IsValid = true;

                return retourk;
            }
            catch (Exception ex)
            {
                return new KeyB36();
            }
        }


        public static long ParseKeyB36AndValidate(string fullid, int tenantidRequired)
        {
            if (string.IsNullOrEmpty(fullid)) return 0;
            var keyB36 = ParseKeyB36(fullid);
            if (!keyB36.IsValid) throw new Exception("ParseKeyB36AndValidate Invalid");
            if (tenantidRequired == 0 || keyB36.TenantId != tenantidRequired)
                throw new Exception("ParseKeyB36AndValidate Tenant");
            return keyB36.ItemId;
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

            var Sum = 0;
            for (int i = inputNumbers.Length - 1, Multiplier = 2; i >= 0; i--)
            {
                Sum += (int)char.GetNumericValue(inputNumbers[i]) * Multiplier;

                if (++Multiplier == 8) Multiplier = 2;
            }

            var Validator = (11 - Sum % 11).ToString();

            //if (Validator == "11") Validator = "0";
            //else if (Validator == "10") Validator = "X";

            return Validator[0];
        }

        /// <summary>
        /// Encode the given number into a Base36 string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToBase36(long input)
        {
            //https://www.stum.de/2008/10/20/base36-encoderdecoder-in-c/
            if (input < 0) throw new ArgumentOutOfRangeException("input", input, "input cannot be negative");

            var clistarr = CharList.ToCharArray();
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
            var pos = 0;
            foreach (var c in reversed)
            {
                result += CharList.IndexOf(c) * (long)Math.Pow(36, pos);
                pos++;
            }

            return result;
        }


        [Obsolete("BETA")]
        public static string StringSignRemove(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (!id.Contains(".S")) return id;
            id = id.Remove(id.LastIndexOf(".S"));
            return id;
        }



        public struct KeyB36
        {
            public string Prefix { get; set; }
            public DateTime DateIndex { get; set; }
            public int TenantId { get; set; }
            public long ItemId { get; set; }
            public bool IsValid { get; set; }
            public bool AdvancedMode { get; set; }


            public Tuple<DateTime, int, long> ToTuple()
            {
                return new Tuple<DateTime, int, long>(DateIndex, TenantId, ItemId);
            }

            public Dictionary<string, object> ToDictionary(string ItemIdName)
            {
                if (!IsValid) throw new Exception("Invalid Key");
                var retour = new Dictionary<string, object>();
                if (TenantId > 0) retour.Add("TenantId", TenantId);
                if (DateIndex.Year > 0) retour.Add("DateIndex", DateIndex);
                if (!string.IsNullOrEmpty(ItemIdName)) retour.Add(ItemIdName, ItemId);
                return retour;
            }

            public override string ToString()
            {
                //if(IsValid) ret
                return ItemId.ToString();
            }
        }
    }
}