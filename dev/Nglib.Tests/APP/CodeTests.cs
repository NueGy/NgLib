//using System;
//using System.Collections.Generic;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Nglib.MANIPULATE.CODE
//{
//    [TestClass]
//    public class CodeTests
//    {
//        [TestMethod]
//        public void TestCompil()
//        {
//            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
//            string codescr = "string resy = null;  int src = 8; src++; for (int y = 0; y < 1000; y++) resy=Convert.ToString(src * y * 8);";

//            watch.Reset();
//            watch.Start();
//            List<Nglib.MANIPULATE.CODE.CodeModel> codes = new List<CodeModel>();
//            for (int i = 0; i < 10; i++) codes.Add(Nglib.MANIPULATE.CODE.CodeBuilder.CreateScript(codescr+ "//pa"+i.ToString()));
//            Nglib.MANIPULATE.CODE.CodeModel code = codes[0];
//            watch.Stop();
//            Console.WriteLine(string.Format("elapsed {0}ms", watch.ElapsedMilliseconds));

//            watch.Reset();
//            watch.Start();
//            Nglib.MANIPULATE.CODE.CompilerTools.CompilationWithCache(codes.ToArray());
//            watch.Stop();
//            Console.WriteLine(string.Format("elapsed {0}ms", watch.ElapsedMilliseconds));


//            watch.Reset();
//            watch.Start();
//            for (int i = 0; i < 1000; i++)
//            {
//                Nglib.CODE.COMPILE.CompileTools.ExecuteAsync(code, "testpass").GetAwaiter().GetResult();
//            }
//            watch.Stop();
//            Console.WriteLine(string.Format("elapsed {0}ms", watch.ElapsedMilliseconds));



//            watch.Reset();
//            watch.Start();
//            for (int i = 0; i < 1000; i++)
//            {
//                string resy = null;  int src = 8; src++; for (int y = 0; y < 1000; y++) resy=Convert.ToString(src * y * 8);
//            }
//            watch.Stop();
//            Console.WriteLine(string.Format("elapsed {0}ms", watch.ElapsedMilliseconds));
//        }
//    }
//}
