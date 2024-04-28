using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.CODE
{
    /// <summary>
    /// Aide à la manipulation des assemblies
    /// </summary>
    [Obsolete("DevSoon")]
    public static class CodeCompileTools
    {


        /// <summary>
        /// Permet l'execution de code compilé
        /// </summary>
        /// <param name="executeContext"></param>
        public static async Task<CompileExecuteResult> ExecuteCodeAsync(ICompiledCode codeModel, params object[] executeParams)
        {
            CompileExecuteResult retour = new CompileExecuteResult();
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


        public class CompileExecuteResult
        {
            public DateTime DateExecute { get; set; }

            public System.Diagnostics.Stopwatch TimeExecute { get; set; }

            public string SourceCodeName { get; set; }

        }
    }
}
