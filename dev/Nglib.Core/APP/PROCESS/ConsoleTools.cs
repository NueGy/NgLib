using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nglib.APP.CONSOLE
{
    /// <summary>
    /// Outils pour System.Console
    /// </summary>
    public static class ConsoleTools
    {


        private static Thread inputThread;
        private static AutoResetEvent getInput, gotInput;
        private static string input;

        static ConsoleTools()
        {
            getInput = new AutoResetEvent(false);
            gotInput = new AutoResetEvent(false);
            inputThread = new Thread(reader);
            inputThread.IsBackground = true;
            inputThread.Start();
        }

        private static void reader()
        {
            while (true)
            {
                getInput.WaitOne();
                input = Console.ReadLine();
                gotInput.Set();
            }
        }


        /// <summary>
        /// Equivalent au Console.ReadLine avec un timeout 
        /// </summary>
        public static string ReadLine(int timeOutMillisecs)
        {
            getInput.Set();
            bool success = gotInput.WaitOne(timeOutMillisecs);
            if (success)
                return input;
            else
                return null; //timelimit time out
            throw new TimeoutException("User did not provide input within the timelimit.");
        }



        /// <summary>
        /// Executer une commande DOS/CMD
        /// </summary>
        public static string ExecuteCmdCommand(string cmdexec, bool safe = false, int timeoutms=4000)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo PInfo;
                System.Diagnostics.Process P;
                PInfo = new System.Diagnostics.ProcessStartInfo("cmd", cmdexec);
                PInfo.CreateNoWindow = false; //nowindow
                PInfo.UseShellExecute = false; //use shell
                PInfo.RedirectStandardOutput = true;
                PInfo.RedirectStandardError = true;
                P = System.Diagnostics.Process.Start(PInfo);
                string output = P.StandardOutput.ReadToEnd();
                string errorput = P.StandardError.ReadToEnd();
                P.WaitForExit(timeoutms); //give it some time to finish
                P.Close();
                string retput = output + " " + errorput;
                return retput;
            }
            catch (Exception e)
            {
                if (!safe) throw new Exception("Execution commande CMD " + e.Message, e);
                return null;
            }
        }




        /// <summary>
        /// permet d'écrire un titre
        /// </summary>
        public static void ConsoleTitle(string Title)
        {
            StringBuilder retour = new StringBuilder();
            retour.AppendLine("=========================================================");
            retour.AppendLine(Title);
            retour.AppendLine("=========================================================");
            Console.Write(retour.ToString());
        }



        /// <summary>
        /// Permet d'utiliser une bar de progression dans la console
        /// </summary>
        public static void ConsoleProgressBar(int progress, int total, string stepDescription = "")
        {
            int totalChunks = 30;

            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = totalChunks + 1;
            Console.Write("]"); //end
            Console.CursorLeft = 1;

            double pctComplete = Convert.ToDouble(progress) / total;
            int numChunksComplete = Convert.ToInt16(totalChunks * pctComplete);

            //draw completed chunks
            Console.BackgroundColor = ConsoleColor.Green;
            Console.Write("".PadRight(numChunksComplete));

            //draw incomplete chunks
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write("".PadRight(totalChunks - numChunksComplete));

            //draw totals
            Console.CursorLeft = totalChunks + 5;
            Console.BackgroundColor = ConsoleColor.Black;

            string output = progress.ToString() + " of " + total.ToString();
            Console.Write(output.PadRight(15) + stepDescription); //pad the output so when changing from 3 to 4 digits we avoid text shifting
        }




        /// <summary>
        /// Permet la saisie des arguments string[] args
        /// </summary>
        /// <param name="args"></param>
        /// <param name="ArgFormat"></param>
        /// <param name="SaisieCommandTimeOut"></param>
        /// <returns></returns>
        public static string[] ConsoleReadArgs(string[] args, string ArgFormat = null, int SaisieCommandTimeOut = 20)
        {
            string[] retour = args;
            if (args == null || args.Length == 0)
            {
                Console.WriteLine(string.Format("Write main parameters ({0})", ArgFormat));
                Console.Write(string.Format("(blank=Exit / timeout:{0}s): ", SaisieCommandTimeOut));
                string cmsuser = ReadLine((SaisieCommandTimeOut * 1000)); //Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmsuser)) { Console.WriteLine("QUIT"); System.Threading.Thread.Sleep(2000); return retour; }  // QUIT
                if (string.IsNullOrWhiteSpace(cmsuser)) { Console.WriteLine("INFO"); ConsoleInfos(); Console.WriteLine("Sleep 6s ..."); System.Threading.Thread.Sleep(6000); return retour; }  // QUIT
                retour = cmsuser.Split(' ');

                Console.WriteLine();
                //DebugManual = true;
            }
            return retour;
        }


        /// <summary>
        /// Affiche des informations dans la console
        /// </summary>
        public static void ConsoleInfos()
        {
            ConsoleTitle("APP INFORMATIONS");
            Console.WriteLine(string.Format("Host: {0}", System.Net.Dns.GetHostName()));
            Console.WriteLine(string.Format("Date: {0}",DateTime.Now));

        }




        /// <summary>
        /// Permet de convertir les arguments en un dictionary
        /// séparateur '='
        /// </summary>
        public static Dictionary<string, string> ConvertArgToDictionary(string[] args, char splitchar='=')
        {
            Dictionary<string, string> retour = new Dictionary<string, string>();
            if (args == null) return retour;
            int ii = 0;
            foreach (string arg in args)
            {
                ii++;
                if (string.IsNullOrWhiteSpace(arg)) continue;
                string paramName = "";
                string paramValue = "";
                if (!arg.Contains(splitchar.ToString()) || arg.StartsWith("\"")) // sans param nommé
                {
                    paramName = "p" + ii;
                    paramValue = arg.Trim().Replace("\"", "");
                }
                else // avec param name
                {
                    paramName = arg.Substring(0, arg.IndexOf(splitchar.ToString())).Trim().ToLower();
                    paramValue = arg.Substring(arg.IndexOf(splitchar.ToString()) + 1).Trim().Replace("\"", "");
                }
                retour.Add(paramName, paramValue);
            }
            return retour;
        }



    }
}
