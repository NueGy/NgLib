//Copyright Nglib 2020 - MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;


namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Enables string formatting using dynamic calculation formulas
    /// Documentation: <see href="https://github.com/NueGy/NgLibComponents/wiki/wiki_components_formula"/>
    /// </summary>
    public static class FormulaTools
    {



        /// <summary>
        /// Loads additional custom functions
        /// </summary>
        /// <param name="classType">Type containing formula functions</param>
        /// <returns>Number of functions loaded</returns>
        public static int LoadFunctions(Type classType)
        {
            FormulaExecuteTools.InitDefaultsFunctions(); // also loads defaults
            return FormulaExecuteTools.LoadFunctions(classType);
        }



        /// <summary>
        /// Calculates a formula expression
        /// </summary>
        /// <param name="formuleStr">Formula string</param>
        /// <param name="globalParameters">Environment parameters for calculations @XX</param>
        /// <returns>Calculated string result</returns>
        public static string Eval(string formuleStr, Dictionary<string, object> globalParameters = null)
        {
            try
            {
                var formule = ParseContext(formuleStr);
                if (formule == null) return null;
                formule.Parameters = globalParameters;
                bool ok = CalculateAsync(formule).GetAwaiter().GetResult();
                if (!ok) return null;
                return formule.ResultValue?.ToString();
            }
            catch (FormulaException fex)
            {
                throw new FormulaException(fex.Command, $"{fex.Message}({fex.Command})");
            }
        }

        /// <summary>
        /// Calculates formulas within a composed string containing text and formulas. Formulas are enclosed in braces {=formula}
        /// </summary>
        /// <param name="composedStr">Composed string with {=formula} patterns</param>
        /// <param name="globalParameters">Global parameters dictionary</param>
        /// <param name="safe">If true, replaces failed formulas with {} instead of throwing</param>
        /// <returns>String with calculated formulas</returns>
        public static string EvalComposedString(string composedStr, Dictionary<string, object> globalParameters = null, bool safe = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(composedStr)) return composedStr;
                if (!composedStr.Contains("{=")) return composedStr; // No formulas present
                Dictionary<int, string> formuls = FormulaParseTools.ExtractComposedString(composedStr);
                // calculate formulas
                string retour = composedStr.ToString();
                foreach (var item in formuls)
                {
                    string formuleStr = item.Value;
                    string result = null;
                    try { result = Eval(formuleStr, globalParameters); }
                    catch (Exception) { if (safe) result = "{}"; else throw; }
                    retour = retour.Replace("{=" + formuleStr + "}", result);
                }
                return retour;
            }
            catch (FormulaException fex)
            {
                throw new FormulaException(fex.Command, $"{fex.Message}({fex.Command})");
            }
        }









        /// <summary>
        /// Calculates a formula asynchronously
        /// </summary>
        /// <param name="formule">Formula context to calculate</param>
        /// <returns>True if calculation succeeded</returns>
        public static async Task<bool> CalculateAsync(FormulaContext formule)
        {
            if (formule?.RootSegment == null || string.IsNullOrWhiteSpace(formule.RootSegment.Text)) 
                return false;
            
            FormulaExecuteTools.InitDefaultsFunctions();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // Calculate root segment (recursive on all sub-segments)
                object result = FormulaExecuteTools.CalculateSegment(formule, formule.RootSegment);
                
                // Store result in Value for compatibility
                formule.ResultValue = result;
                
                return result != null;
            }
            catch (FormulaException exf)
            {
                throw; // already handled
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.Message == "Exception has been thrown by the target of an invocation.")
                    throw new FormulaException("MAIN", $"{ex.InnerException.Message}");
                else
                    throw new FormulaException("MAIN", $"{ex.Message}");
            }
            finally
            {
                formule.ElapsedTime = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();
            }
        }



        /// <summary>
        /// Gets the formula value as string. Equivalent to: formule?.Value?.ToString() with additional checks
        /// </summary>
        /// <param name="formule">Formula context</param>
        /// <param name="nullable">If true, returns null for empty values; otherwise returns empty string</param>
        /// <returns>Formula result as string</returns>
        public static string GetValueString(this FormulaContext formule, bool nullable = false)
        {
            if (formule?.ResultValue == null || formule.ResultValue == DBNull.Value)
            {
                if (nullable) return null;
                else return string.Empty;
            }
            string retour = formule.ResultValue.ToString();
            //if(formule.Command=="STRING" && )
            return retour;
        }






        /// <summary>
        /// Parser et découper une formule sans l'exécuter.
        /// Obtien un context
        /// </summary>
        public static FormulaContext ParseContext(string formuleStr)
        {
            if (string.IsNullOrWhiteSpace(formuleStr)) return null;
            FormulaContext context = new FormulaContext();
            context.RootSegment = FormulaParseTools.Parse(formuleStr);
            return context;
        }





        /// <summary>
        /// Grouper les formules par groupe
        /// </summary>
        public static Dictionary<string, List<FormulaAttribute>> GroupByGroupe(this List<FormulaAttribute> formulas)
        {
            var list = new Dictionary<string, List<FormulaAttribute>>();
            foreach (var item in formulas)
            {
                if (item.Groupe == null) item.Groupe = ""; // si null
                if (!list.ContainsKey(item.Groupe)) list.Add(item.Groupe, new List<FormulaAttribute>());
                list[item.Groupe].Add(item);
            }
            return list;
        }




        /// <summary>
        /// Permet de valider un context de formule avant son exécution
        /// </summary>
        public static Nglib.APP.DIAG.ValidateModel IsValid(FormulaContext formule, bool recursive = true)
        {
            if (formule == null) return Nglib.APP.DIAG.ValidateModel.Invalid("NullOrEmpty");
            if (formule.RootSegment==null || string.IsNullOrWhiteSpace(formule.RootSegment.Text)) return Nglib.APP.DIAG.ValidateModel.Invalid("NullOrEmpty");

            ////Validations des sous formules
            //if (recursive && formule.SubFormules.Count > 0)
            //    foreach (var subf in formule.SubFormules)
            //    {
            //        var ok = IsValid(subf);
            //        if (!ok.IsValid) return ok;
            //    }
            return Nglib.APP.DIAG.ValidateModel.Success;
        }





    }
}




