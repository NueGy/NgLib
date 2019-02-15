using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nglib.CODE.COMPILE
{
    /// <summary>
    /// Permet de concevoir et parser un codesource c# - compilation dynamique
    /// </summary>
    public class CodeBuilder
    {
        /// <summary>
        /// Methode principale à lancer automatiquement
        /// </summary>
        public const string MasterMethodForExecute = "Execute"; // public void Execute(ExecuteContext executeContext)




        /// <summary>
        /// using à déclarer
        /// </summary>
        public List<string> ClassUsings = new List<string>();


        /// <summary>
        /// CodeSource principal encapsuler dans la methode Execute
        /// </summary>
        public string ScriptCode { get; set; }

        /// <summary>
        /// Autre morceau de codes à ajouter (en dehors de la méthode execute)
        /// </summary>
        public string OtherCode { get; set; }


        /// <summary>
        /// Nom de la classe 
        /// </summary>
        public string ClassName { get; set; }


        /// <summary>
        /// namespace à utiliser
        /// </summary>
        public string NameSpace { get; set; }

        public CodeBuilder()
        {
            ClassUsings.Add("System");
            NameSpace = "CodeBuilderNs";
        }











        public string BuildText()
        {
            if (string.IsNullOrWhiteSpace(this.ClassName))
                this.ClassName = string.Format("ClBuild{0}",FORMAT.StringUtilities.GenerateGuid32());


            StringBuilder codetext = new StringBuilder();
            // Ajoute les using
            ClassUsings.ForEach(txt => codetext.AppendLine(string.Format("using {0};", txt)));
            codetext.AppendLine();


            //Construction du namespace et de la classe
            codetext.AppendLine(string.Format("namespace {0}", NameSpace));
            codetext.AppendLine("{");
            codetext.AppendLine(string.Format("public class {0}", ClassName));
            codetext.AppendLine("{");

            // methode principal Execute
            codetext.AppendLine();
            codetext.AppendLine(string.Format("public void {0}(object context)", MasterMethodForExecute));
            codetext.AppendLine("{");
            codetext.AppendLine(CleanCode(this.ScriptCode));
            codetext.AppendLine("}");
            codetext.AppendLine();


            // OtherCodes autres morceaux de codes directement dans la classe
            if (!string.IsNullOrEmpty(OtherCode))
            {
                codetext.AppendLine(OtherCode);
                codetext.AppendLine();
            }

            // fermeture du namespace et de la classe
            codetext.AppendLine("}");
            codetext.AppendLine("}");

            return codetext.ToString();
        }


        public CodeModel Build()
        {
            CodeModel retour = new CodeModel();
            string TextSourceCode = BuildText();
            retour.SourceCode = ParseCode(TextSourceCode);
            retour.SourceCodeName = this.ClassName;
            retour.ClassFullName = string.Format("{0}.{1}", this.NameSpace, this.ClassName);
            return retour;
        }





        public static SyntaxTree ParseCode(string CodeSourcesText)
        {
            SyntaxTree sourceCode = CSharpSyntaxTree.ParseText(CodeSourcesText);
            return sourceCode;
        }

        public static string CleanCode(string codeOrgn)
        {
            return codeOrgn;
        }




        public static CodeModel CreateScript(string script)
        {
            CodeBuilder codeBuilder = new CodeBuilder();
            codeBuilder.ScriptCode = script;
            return codeBuilder.Build();
        }



        

    }
}
