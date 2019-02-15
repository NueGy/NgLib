using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Nglib.APP.CONSOLE
{
    public static class AppCore
    {
        /// <summary>
        /// Paramètres étendus à toute l'application
        /// </summary>
        public static ConcurrentDictionary<string, string> GlobalParameters = new ConcurrentDictionary<string, string>();



        /// <summary>
        /// Logs Static en mémoire (STACK)
        /// </summary>
        public static ConcurrentQueue<LOG.ILog> GlobalLogs = new ConcurrentQueue<LOG.ILog>();




        //public static Microsoft.Extensions.Configuration.IConfiguration Configuration {get; set;}





        /// <summary>
        /// Permet d'effacer des logs anciens de la stack et lancer le garbage collector
        /// </summary>
        public static async System.Threading.Tasks.Task CleanAppAsync()
        {
            try
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                throw new Exception("Nglib AppCore CleanApp "+ex.Message);
            }
        }

    }
}
