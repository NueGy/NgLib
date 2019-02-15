using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.CODE.COMPILE
{
    /// <summary>
    /// Outils pour la compilation dynamique
    /// </summary>
    public static class CompilerTools
    {
        //http://www.tugberkugurlu.com/archive/compiling-c-sharp-code-into-memory-and-executing-it-with-roslyn

           /// <summary>
           /// Cache des codes déja compilé
           /// </summary>
        public static List<CodeModel> CompiledCache = new List<CodeModel>();


        public static CodeModel GetCache(string SourceCodeName)
        {
            if (string.IsNullOrWhiteSpace(SourceCodeName)) return null;
            return CompiledCache.FirstOrDefault(r => SourceCodeName.Equals(r.SourceCodeName, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Compile et met en cache
        /// </summary>
        /// <param name="codeModels"></param>
        public static void CompilationWithCache(params CodeModel[] codeModels)
        {
            Compilation(codeModels);
            //met en cache ceux qui manquent
            if (codeModels != null)
                codeModels.Where(cm => GetCache(cm.SourceCodeName) == null).ToList().ForEach(cm => CompiledCache.Add(cm));
        }


        /// <summary>
        /// Compilation
        /// </summary>
        /// <param name="codeModels"></param>
        public static void Compilation(params CodeModel[] codeModels)
        {
            if (codeModels == null || codeModels.Length==0) return;
            try
            {
                string assemblyName = Path.GetRandomFileName();
                codeModels.ToList().ForEach(cm=>ValidateAndPrepareCodeModel(cm));

                // Obtient toutes les références et option
                List<MetadataReference> metadataReferences = GetAllReferences(codeModels);
                CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
                IEnumerable<SyntaxTree> syntaxTreesCodes = codeModels.Select(codes => codes.SourceCode);

                CSharpCompilation compilation = CSharpCompilation.Create(assemblyName,
                    syntaxTrees: syntaxTreesCodes,
                    references: metadataReferences,
                    options: compilationOptions);



                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);

                    if (!result.Success)
                    {
                        // Genération erreur
                        throw new CompilerException(result.Diagnostics, codeModels);
                    }
                    else
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        Assembly assembly = Assembly.Load(ms.ToArray());

                        // on retrouve les methodes
                        foreach (var item in codeModels)
                        {
                            item.Assembly = assembly;
                            item.CompiledType = assembly.GetType(item.ClassFullName);
                        }

                        
                    }
                }

            }
            catch(CompilerException)
            {
                throw; // Déja géré
            }
            catch (Exception ex)
            {
                throw new Exception("Compilation Error "+ex.Message, ex);
            }
        }


        private static List<MetadataReference> GetAllReferences(CodeModel[] codeModels)
        {
            List<MetadataReference> metadataReferences = new List<MetadataReference>();
            codeModels.Where(cm => cm.References != null).Select(cm => cm.References).ToList().ForEach(refs => metadataReferences.AddRange(refs));

            // Ajoute les références minimummetadataReferences
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
            // distinct
            metadataReferences = metadataReferences.Distinct().ToList();

            return metadataReferences;
        }




        public static void ValidateAndPrepareCodeModel(CodeModel codeModel)
        {



        }


        


        


    }
}
