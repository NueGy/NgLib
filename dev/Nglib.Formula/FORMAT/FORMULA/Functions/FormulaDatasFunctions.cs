using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Calculateurs de formule pour des données
    /// </summary>
    public static class FormulaDatasFunctions
    {

/*

        [Formula("Data", 1, "Retrieve data passed as a GlobalParameter", UseExample = "Data('@MonObjet')",  MethodMode = MethodModeEnum.Context)]
        public static object Data(FormulaContext ctx)
        {
            string dataName = null;
            object retour = null;
            string[] args = ctx.SubFormules?.Select(fom => fom.GetValueString()).ToArray(); // convertir les arguments en string
            try
            {
                dataName = args[0]?.Trim();
                if(string.IsNullOrEmpty(dataName)) throw new Exception("Data() doit avoir un argument");
                if(!dataName.StartsWith("@")) throw new Exception("le parametre doit commencer par @");
                dataName = dataName.TrimStart('@').ToUpper().Trim();
                // Recherche de la donnée
                if (ctx.MasterFormula?.Parameters == null) throw new Exception("Pas de paramètres dans le contexte");
                var objs = ctx.MasterFormula.Parameters.Where(d => d.Key.Equals(dataName, StringComparison.OrdinalIgnoreCase));
                if (objs == null || objs.Count() == 0) throw new Exception($"Parameter not found({dataName})");
                var obj = objs.Select(d => d.Value).FirstOrDefault();
                if (obj == null) return null;

                // Si c'est une sous données
                if (args.Length > 1)
                {
                    string datafilter = args[1];
                    if (obj is System.Data.DataRow)
                    {
                        var dr = obj as System.Data.DataRow;
                        retour = Nglib.DATA.COLLECTIONS.DataSetTools.GetRowObject(dr, datafilter);
                    }
                    else if (obj is Nglib.DATA.ACCESSORS.IDataAccessor)
                    {
                        var acc = obj as Nglib.DATA.ACCESSORS.IDataAccessor;
                        retour = acc.GetData(datafilter, Nglib.DATA.ACCESSORS.DataAccessorOptionEnum.Required);
                    }
                    else if (obj is string[])
                    {
                        var acc = obj as string[];
                        retour = acc[Convert.ToInt32(datafilter)];
                    }
                }
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception($"FormulaData({dataName}) " + ex.Message);
            }
        }

*/






    }
}
