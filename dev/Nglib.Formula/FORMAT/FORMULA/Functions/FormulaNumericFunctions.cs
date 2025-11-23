// Copyright Nglib 2020 - MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Calculateurs de formule
    /// </summary>
    public static class FormulaNumericFunctions
    {



        
       
      


        [Formula("lt", 2, "If arg1 is less than arg2")]
        public static object Lt(string[] args)
            => Convert.ToDouble(args[0]) < Convert.ToDouble(args[1]) ? "1" : "0";

        [Formula("gt", 2, "If arg1 is greater than arg2")]
        public static object Gt(string[] args)
            => Convert.ToDouble(args[0]) > Convert.ToDouble(args[1]) ? "1" : "0";


        [Formula("gteq", 2, "If arg1 is greater than or equal to arg2")]
        public static object GtEq(string[] args)
            => Convert.ToDouble(args[0]) >= Convert.ToDouble(args[1]) ? "1" : "0";


        [Formula("lteq", 2, "If arg1 is less than or equal to arg2")]
        public static object LtEq(string[] args)
            => Convert.ToDouble(args[0]) <= Convert.ToDouble(args[1]) ? "1" : "0";





        [Formula("IsNumeric", 1, "If the string is numeric")]
        public static object IsNumeric(string[] args)
            => double.TryParse(args[0], out double d) ? "1" : "0";

        [Formula("ToDouble", 1, "Force to Double/Float format if string")]
        public static object ToDouble(string[] args)
            => Convert.ToDouble(args[0]);

        [Formula("ToInt", 1, "Force to Int64/long format if string")]
        public static object ToInt(string[] args)
            => Convert.ToInt64(args[0]);

        [Formula("add", 2, "Add two numbers")]
        public static object Add(string[] args)
            => (Convert.ToDouble(args[0]) + Convert.ToDouble(args[1])).ToString();

        [Formula("sub", 2, "Subtract two numbers")]
        public static object Sub(string[] args)
            => (Convert.ToDouble(args[0]) - Convert.ToDouble(args[1])).ToString();



        [Formula("mul", 2, "Multiply two numbers")]
        public static object Mul(string[] args)
            => (Convert.ToDouble(args[0]) * Convert.ToDouble(args[1])).ToString();

        [Formula("div", 2, "Divide two numbers")]
        public static object Div(string[] args)
            => (Convert.ToDouble(args[0]) / Convert.ToDouble(args[1])).ToString();

        [Formula("mod", 2, "Modulo of two numbers")]
        public static object Mod(string[] args)
            => (Convert.ToDouble(args[0]) % Convert.ToDouble(args[1])).ToString();

        [Formula("abs", 1, "Absolute value")]
        public static object Abs(string[] args)
            => Math.Abs(Convert.ToDouble(args[0])).ToString();

        [Formula("round", 2, "Round a number")]
        public static object Round(string[] args)
            => Math.Round(Convert.ToDouble(args[0]), Convert.ToInt32(args[1])).ToString();

        [Formula("floor", 1, "Round a number down to the nearest integer")]
        public static object Floor(string[] args)
            => Math.Floor(Convert.ToDouble(args[0])).ToString();

        [Formula("ceil", 1, "Round a number up to the nearest integer")]
        public static object Ceil(string[] args)
            => Math.Ceiling(Convert.ToDouble(args[0])).ToString();


        [Formula("min", 2, "Return the minimum of all args")]
        public static object Min(string[] args)
        {
            double min = double.MaxValue;
            foreach (var item in args)
            {
                double num = Convert.ToDouble(item);
                if (num < min) min = num;
            }
            return min.ToString();
        }

        [Formula("max", 2, "Return the maximum of all args")]
        public static object Max(string[] args)
        {
            double max = double.MinValue;
            foreach (var item in args)
            {
                double num = Convert.ToDouble(item);
                if (num > max) max = num;
            }
            return max.ToString();
        }

        [Formula("sum", 2, "Return the sum of all args")]
        public static object Sum(string[] args)
        {
            double sum = 0;
            foreach (var item in args)
            {
                double num = Convert.ToDouble(item);
                sum += num;
            }
            return sum.ToString();
        }

        [Formula("avg", 2, "Return the average of all args")]
        public static object Avg(string[] args)
        {
            double sum = 0;
            foreach (var item in args)
            {
                double num = Convert.ToDouble(item);
                sum += num;
            }
            return (sum / args.Length).ToString();
        }

        [Formula("Between", 3, "The value arg1 must be between arg2 and arg3")]
        public static object Between(string[] args)
        {
            if(args[0]==null) return "0";
            if (args[1] == null || args[2] == null) throw new Exception("Arg[1] Or Arg[2] is null");
 
            double val = Convert.ToDouble(args[0]);
            double min = Convert.ToDouble(args[1]);
            double max = Convert.ToDouble(args[2]);
            return (val >= min && val <= max) ? "1" : "0";
        }


        [Formula("amtf", 1, "Format as string amount. amtf('1586,45'[,isCentimes][,devise])=1 586.45")]
        public static object amtf(string[] args)
        {
            double num = Convert.ToDouble(args[0]);
            return num.ToString("0.00");
        }

        [Formula("amt", 1, "Convert a string to double amount amt('1586,45')=1 586.45")]
        public static object amt(string[] args)
        {
            string val = Convert.ToString(args[0]);
            val = val.Replace(" ", "").Replace(".", ",");
            double num = Convert.ToDouble(val);
            return Convert.ToDouble(num.ToString("0.00"));
        }


        [Formula("amtct", 1, "Convert a string to numeric cent amount int string. amtct('1 586.45')=158645")]
        public static object amtct(string[] args)
        {
            string val = Convert.ToString(args[0]);
            val = val.Replace(" ", "").Replace(".", ",");
            // todo!!! supprimer les devises
            // Todo!!! gérer des centimes sur 1 caractères. padleft
            val = val.Replace(",", "");
            long num = Convert.ToInt64(args[0]);
            return num;
        }





        [Formula("KeyMod", 1, "Get the control key (2chars) of a string, modulo 100")]
        public static object KeyMod(string[] args)
        {
            try
            {
                string valeur = args[0];
                if (string.IsNullOrWhiteSpace(valeur)) return "";
                valeur = valeur.Trim();
                int Modulo = 100;
                if(args.Length>1) Modulo = Convert.ToInt32(args[1]);
                int somme = 0;
                for (int i = 1; i <= valeur.Length; i++)
                {
                    char car = valeur[valeur.Length - i];
                    int valueuradd = (Convert.ToInt32(car.ToString()) * i);
                    somme += valueuradd;
                }

                int mod = somme % Modulo;
                return mod.ToString().PadLeft(2,'0');
            }
            catch (Exception e)
            {
                throw new Exception($"KeyMod Calcul de la clef ({args[0]})" + e.Message);
            }
        }



    }
}
