using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.CODE.COMPILE
{
    /// <summary>
    /// Aide à la manipulation des assemblies
    /// </summary>
    public static class CompileTools
    {




        /// <summary>
        /// Permet l'execution de code compilé
        /// </summary>
        /// <param name="executeContext"></param>
        public static async Task<ExecuteResults> ExecuteCodeAsync(ICompiledCode codeModel, params object[] executeParams)
        {
            ExecuteResults retour = new ExecuteResults();
            retour.TimeExecute = new System.Diagnostics.Stopwatch();
            try
            {
                if (codeModel == null || codeModel.CompiledType == null)
                    throw new Exception("CompiledCode Not Found");
                retour.SourceCodeName = codeModel.SourceCodeName;
                retour.DateExecute = DateTime.Now;
                retour.TimeExecute.Start();
                object obj = Activator.CreateInstance(codeModel.CompiledType);
                BindingFlags bindingFlags = BindingFlags.Default | BindingFlags.InvokeMethod;
                codeModel.CompiledType.InvokeMember(codeModel.CallMasterMethod, bindingFlags, null,
                    obj, executeParams);

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("Execute " + ex.Message, ex);
            }
            finally
            {
                retour.TimeExecute.Stop();
            }
        }


    }
}
