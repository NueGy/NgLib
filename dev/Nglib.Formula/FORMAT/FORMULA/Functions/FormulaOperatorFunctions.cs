// Copyright Nglib 2020 - MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Calculateurs de formule
    /// </summary>
    public static class FormulaOperatorFunctions
    {

        /// <summary>
        /// Formula calculators
        /// </summary>
        [Formula("equal", 2, "Object Equal Must be exactly identical (type, ...)")]
        public static object EqualObjects(string[] args)
        {
            if (args.Length < 2) throw new Exception("egal() must have 2 arguments");
            foreach (var arg in args.Skip(1)) // all args must be equal
                if (args[0] != arg)
                    return "0";
            return "1";
        }

        [Formula("or", 1, "Takes the first argument with a positive value (non-empty, non-null, !='0')")]
        public static object Or(string[] args)
        {
            foreach (var arg in args)
                if (Nglib.FORMAT.ConvertTools.ToBoolean(arg))
                    return "1";
            return "0";
        }

        [Formula("and", 2, "If all values are =1")]
        public static object And(string[] args)
        {
            foreach (var arg in args)
                if (!Nglib.FORMAT.ConvertTools.ToBoolean(arg))
                    return "0";
            return "1";
        }

        [Formula("not", 1, "Inverts the value")]
        public static object Not(string[] args)
            => Nglib.FORMAT.ConvertTools.ToBoolean(args[0]) ? "0" : "1";


        [Formula("if", 3, "If function (value=1, trueValue, falseValue)")]
        public static object If(string[] args)
            => Nglib.FORMAT.ConvertTools.ToBoolean(args[0]) ? args[1] : args[2];



        [Formula("isnull", 2, "Returns the first non-null argument Isnull(arg1,arg2,arg3,...)")]
        public static object IsNull(string[] args)
        {
            foreach (var arg in args)
                if (arg != null)
                    return arg;
            return null;
        }

        [Formula("nullif", 2, "Returns null if arg1 is equal to arg2")]
        public static object NullIf(string[] args)
            => (args[0] != null) ? args[0].ToString().Equals(args[1]) ? null : args[0] : null;





        [Formula("error", 1, "Triggers an error")]
        public static object Error(string[] args)
            => throw new Exception(args[0]);

        [Formula("Ok", 0, "Ensures that the value is not null, empty, or ='0', otherwise error")]
        public static object Ok(string[] args)
            => Okn(args) != null ? args[0] : null;

        [Formula("Okn", 0, "Ensures that the value is not null, empty, or ='0' (and will always return empty), otherwise error")]
        public static object Okn(string[] args)
        {
            if (args.Length == 0) throw new Exception("Ok():NOARGS");
            int ii = 0;
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg?.ToString())) throw new Exception($"Ok({ii}):ARGNULL");
                if (arg.ToString().Trim().Trim('0') == "") throw new Exception($"Ok({ii}):ARGNULL0");
            }
            return "";
        }

 
        // DEPRECATED: MethodMode.Context n'est plus supporté
        [Obsolete("Not implemented in new segment system", true)]
        [Formula("Safe", 1, "Will not cause an error, Returns NULL safe(xxxx) (SOON ...)", MethodMode = MethodModeEnum.Context)]
        public static object Safe(FormulaContext ctx)
        {
            throw new NotSupportedException("safe() function is not yet implemented in new segment system");
        }


        // DEPRECATED: MethodMode.Context n'est plus supporté
        [Obsolete("Not implemented in new segment system", true)]
        [Formula("Lock", 1, "Multithread protection lock(xxxx) (SOON ...)", MethodMode = MethodModeEnum.Context)]
        public static object Lock(FormulaContext ctx)
        {
            throw new NotSupportedException("lock() function is not yet implemented in new segment system");
        }



    }
}
