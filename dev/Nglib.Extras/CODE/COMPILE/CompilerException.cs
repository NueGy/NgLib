using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.CODE.COMPILE
{
    /// <summary>
    /// Erreur de compilation
    /// </summary>
    public class CompilerException : System.Exception
    {
        public ICollection<Diagnostic> CompileDiagnostics { get; set; }
        public CodeModel[] codeModels { get; set; }


        public CompilerException(ICollection<Diagnostic> CompileDiagnostics, CodeModel[] codeModels) : base("COMPILER FAIL")
        {

        }

        /// <summary>
        /// Prepare
        /// </summary>
        /// <param name="result"></param>
        /// <param name="codeModels"></param>
        public string GetFailMessage()
        {

            IEnumerable<Diagnostic> failures = CompileDiagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            List<string> failCompileInfos = new List<string>();
            //failures.ToList().ForEach(fail => failCompileInfos.Add(string.Format("", diagnostic.Id, diagnostic.GetMessage())));
            foreach (Diagnostic diagnostic in failures)
            {
                string failinfo = string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                failCompileInfos.Add(failinfo);
            }
            return string.Join("\r\n", failCompileInfos.ToArray());
        }



    }
}
