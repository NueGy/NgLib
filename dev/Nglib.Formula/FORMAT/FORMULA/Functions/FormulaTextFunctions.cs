// Copyright Nglib 2020 - MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Calculateurs de formule
    /// </summary>
    public static class FormulaTextFunctions
    {

 
        [Formula("eq", 2, "Allows comparing multiple strings")]
        public static object Eq(string[] args)
        {
            if (args.Length < 2) throw new Exception("eq() doit avoir 2 arguments minimum");
            string cmpa = args[0]?.ToString();
            if (cmpa == null) cmpa = string.Empty;
            foreach (var arg in args.Skip(1)) // tous les args doivent etre egaux
            {
                string eqarg = arg?.ToString();
                if (eqarg == null) eqarg = string.Empty;
                if (!cmpa.Equals(eqarg, StringComparison.OrdinalIgnoreCase)) return "0";
            }
            return "1"; // tous identiques
        }



        [Formula("char", 2, "Get a character from a string")]      
        public static object Char(string[] args)
            => args[0].Length <= Convert.ToInt32(args[1]) ? args[0][Convert.ToInt32(args[1])].ToString() : null;



        [Formula("len", 1, "Get the length of a string")]
        public static object Len(string[] args)
            => (args[0] != null)? args[0].Length:0;

        [Formula("islen", 2, "The string must be a certain length otherwise returns an error islen(arg,eq) islen(arg,min,max)")]
        public static object islen(string[] args)
        {
            if(args.Length==2)
                return (args[0] != null && args[0].Length == Convert.ToInt32(args[1])) ? "1" : "0";
            else
                return (args[0] != null && args[0].Length >= Convert.ToInt32(args[1]) && args[0].Length <= Convert.ToInt32(args[2])) ? "1" : "0";
        }


        [Formula("left", 2, "Get the first characters (Limit Safe)")]
        public static object Left(string[] args)
            => Nglib.FORMAT.StringTools.Limit(args[0], Convert.ToInt32(args[1])); // Left

        [Formula("right", 2, "Get the last characters")]
        public static object Right(string[] args)
            => (args[0] != null) ? args[0].Substring(args[0].Length - Convert.ToInt32(args[1])):null;

        [Formula("lower", 1, "To lowercase")]
        public static object Lower(string[] args)
            => args[0]?.ToLower();

        [Formula("upper", 1, "To uppercase")]
        public static object Upper(string[] args)
            => args[0]?.ToUpper();

        [Formula("trim", 1, "Remove spaces from the beginning and end of a string")]
        public static object Trim(string[] args)
            => (args.Length>1)? args[0]?.Trim(args[1][0]) :args[0]?.Trim();

        [Formula("ltrim", 1, "Remove spaces from the beginning of a string")]
        public static object LTrim(string[] args)
            => (args.Length > 1) ? args[0]?.TrimStart(args[1][0]) : args[0]?.TrimStart();

        [Formula("rtrim", 1, "Remove spaces from the end of a string")]
        public static object RTrim(string[] args)
            => (args.Length > 1) ? args[0]?.TrimEnd(args[1][0]) : args[0]?.TrimEnd();

        [Formula("replace", 3, "Replace one string with another replace(origin,search,new)")]
        public static object Replace(string[] args)
            => args[0].Replace(args[1], args[2]);

        [Formula("concat", 2, "Assemble multiple strings/args")]
        public static object Concat(string[] args)
            => string.Concat(args);


        [Formula("IsAlphaNumeric", 1, "If it contains only numbers and letters")]
        public static object IsAlphaNumeric(string[] args)
            => Nglib.FORMAT.StringTools.IsAlphaNumeric(args[0]);

        [Formula("OnlyAlphanumeric", 1, "Returns only alphanumeric characters from string")]
        public static object OnlyAlphanumeric(string[] args)
            => Nglib.FORMAT.StringTools.FilterCharacters(args[0]);

        [Formula("OnlyNumeric", 1, "Returns only numeric characters from string")]
        public static object OnlyNumeric(string[] args)
            => Nglib.FORMAT.StringTools.FilterCharacters(args[0], "0123456789");


        [Formula("PadNumeric", 2, "Adds zeros. Padnumeric(815,6)=000815")]
        public static object PadNumeric(string[] args)
            => Nglib.FORMAT.NumberTools.PadNumeric(args[0], Convert.ToInt32(args[1]));


        [Formula("PadLeft", 3, "Adds characters to the left")]
        public static object PadLeft(string[] args)
            => args[0]?.PadLeft(Convert.ToInt32(args[1]), args[2][0]);

        [Formula("PadRight", 3, "Adds characters to the right")]
        public static object PadRight(string[] args)
            => args[0]?.PadRight(Convert.ToInt32(args[1]), args[2][0]);

        [Formula("Contains", 2, "If it contains a string (IgnoreCase)")]
        public static object Contains(string[] args)
            => (args[0]!=null && args[1]!=null)? args[0].Contains(args[1], StringComparison.OrdinalIgnoreCase)?"1":"0":"0";

        [Formula("StartsWith", 2, "If it starts with a string (IgnoreCase)")]
        public static object StartsWith(string[] args)
            => (args[0] != null && args[1] != null) ? args[0].StartsWith(args[1], StringComparison.OrdinalIgnoreCase) ? "1" : "0" : "0";

        [Formula("EndsWith", 2, "If it ends with a string (IgnoreCase)")]
        public static object EndsWith(string[] args)
            => (args[0] != null && args[1] != null) ? args[0].EndsWith(args[1], StringComparison.OrdinalIgnoreCase) ? "1" : "0" : "0";

        [Formula("strindex", 2, "Search for a string in another")]
        public static object StrIndex(string[] args)
            => (args[0]!=null && args[1]!=null) ?args[0].IndexOf(args[1], StringComparison.OrdinalIgnoreCase):-1;

        [Formula("strlastindex", 2, "Search for a string in another")]
        public static object StrLastIndex(string[] args)
            => (args[0] != null && args[1] != null) ? args[0].LastIndexOf(args[1], StringComparison.OrdinalIgnoreCase) : -1;


        [Formula("Substring", 2, "Cut a string")]
        public static object Substring(string[] args)
            => args.Length > 2 ? Nglib.FORMAT.StringTools.SubstringSafe(args[0], Convert.ToInt32(args[1]), Convert.ToInt32(args[2]))
                : Nglib.FORMAT.StringTools.SubstringSafe(args[0], Convert.ToInt32(args[1]));


        [Formula("ReplaceDiacritics", 1, "Replace accents")]
        public static object ReplaceDiacritics(string[] args)
            => Nglib.FORMAT.StringTools.ReplaceDiacritics(args[0]);

        [Formula("CleanString", 1, "Removes all spaces, accents,...")]
        public static object CleanString(string[] args)
            => Nglib.FORMAT.StringTools.CleanString(args[0]);


        [Formula("Guid32", 0, "A random string of 32 characters")]
        public static object Guid32(string[] args)
            => Nglib.FORMAT.StringTools.RandomGuid32();


        [Formula("IsNullOrEmpty", 2, "Function isnull(value,then)")]
        public static object IsNullOrEmpty(string[] args)
            => string.IsNullOrEmpty(args[0]) ? args[1] : args[0];


        [Formula("ToString", 1, "Transforms to string, If null becomes empty")]
        public static object ToString(string[] args)
            => args[0] == null ? string.Empty : args[0].ToString();


        [Formula("Match", 2, "Checks if Match via regex match(value,regex)")]
        public static object Match(string[] args)
            => System.Text.RegularExpressions.Regex.IsMatch(args[0], args[1]);







        [Formula("StrSplit", 2, "Cuts a string into an array")]
        public static object Split(string[] args)
            => args[0]?.Split(args[1][0]);

        [Formula("StrJoin", 2, "Joins an array into a string")]
        public static object Join(string[] args)
            => (args[0]!=null)?string.Join(args[1], args[0]):null;

        [Formula("tocsv", 1, "Transforms a list of arguments into CSV. Arguments can be arrays", MethodMode = MethodModeEnum.ObjectArray)]
        public static object ToCsv(object[] args)
        {
            List<string> retour = new List<string>();
            foreach (var arg in args)
            {
                if (arg == null) retour.Add(string.Empty);
                else if (arg is string[]) retour.Add(string.Join(";", (string[])arg));
                else if (arg is object[]) retour.Add(string.Join(";", (object[])arg));
                else retour.Add(arg.ToString());
            }
            return string.Join(";", retour);
        }


    }
}
