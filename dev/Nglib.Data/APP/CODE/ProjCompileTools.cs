using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml; 

namespace Nglib.APP.CODE
{
    /// <summary>
    /// Outils divers pour la compilation des projets .NET (Utilisation via powershell)
    /// </summary>
    public static class ProjCompileTools
    {

        public static void Test()
        {
            Console.WriteLine("CompileTools.Test"); 
        }



        /// <summary>
        /// parser un fichier csproj pour en extraire les infos de version
        /// </summary>
        public static Dictionary<string, string> ParseCsproj(string csprojPath) 
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(csprojPath);

                Dictionary<string, string> retour = new Dictionary<string, string>();
                string version = "";
                version = doc.SelectSingleNode("//Version")?.InnerText;
                if (string.IsNullOrEmpty(version)) version = doc.SelectSingleNode("//AssemblyVersion")?.InnerText;
                retour.Add("version", version);
                retour.Add("assemblyname", doc.SelectSingleNode("//AssemblyName").InnerText);
                retour.Add("targetframework", doc.SelectSingleNode("//TargetFramework").InnerText);



                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("ParseCsproj " + ex.Message);
            }
        }

        public static string ParseVersionCsproj(string csprojPath) => ParseCsproj(csprojPath)["version"];


        /// <summary>
        /// Compilation d'un projet .NET
        /// </summary>
        public static void CompileProject(string projectPath, string outputPath, string configuration = "Release")
        {
            try
            {
                if (string.IsNullOrEmpty(projectPath)) throw new ArgumentNullException(nameof(projectPath));
                if (string.IsNullOrEmpty(outputPath)) throw new ArgumentNullException(nameof(outputPath));
                if (string.IsNullOrEmpty(configuration)) throw new ArgumentNullException(nameof(configuration));

                string cmd = $"dotnet publish \"{projectPath}\" -c {configuration} -o \"{outputPath}\"";
                Console.WriteLine(cmd);
                var proc = System.Diagnostics.Process.Start("cmd.exe", $"/c {cmd}");
                proc.WaitForExit();
                if (proc.ExitCode != 0) throw new Exception("Erreur de compilation");
            }
            catch (Exception ex)
            {
                throw new Exception("CompileProject " + ex.Message, ex);
            }
        }








    }
}
