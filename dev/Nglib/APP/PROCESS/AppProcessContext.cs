using Nglib.DATA.PARAMVALUES;
using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.APP.PROCESS
{
    /// <summary>
    /// Variable et configuration du process
    /// </summary>
    public class AppProcessContext
    {
        /// <summary>
        /// Le context ne peus être initialisé que par cette DLL
        /// </summary>
        internal AppProcessContext()
        {
            this.ProcessRunId = FORMAT.StringTools.GenerateGuid32();
            if (CancelToken == null) this.CancelToken = new System.Threading.CancellationToken();
        }


        /// <summary>
        /// Numéro unique de lancement du processus
        /// </summary>
        public string ProcessRunId { get; set; }

        /// <summary>
        /// nom du processus
        /// </summary>
        public string ProcessName { get; set; }


        /// <summary>
        /// Nombre de boucle effectuées
        /// </summary>
        public int ProcessLoopCount = 0;

        /// <summary>
        /// Nombre de threads à lancer la methode principale (SOON !!!)
        /// </summary>
        public int ConfigThreadCount = 1;

        /// <summary>
        /// Date d'initialisation du processus
        /// </summary>
        public DateTime? ProcessInitiate = null;

        /// <summary>
        /// Durée d'exécution sur l'ensemble de processus (before, loop, after)(Hors Init)
        /// </summary>
        public System.Diagnostics.Stopwatch ProcessRunWatch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// Durée d'exécution de l'initialisation
        /// </summary>
        public System.Diagnostics.Stopwatch ProcessInitWatch = new System.Diagnostics.Stopwatch();

        ///// <summary>
        ///// Niveau d'affichage des traces
        ///// </summary>
        //public System.Diagnostics.TraceLevel ProcessTraceLevel = System.Diagnostics.TraceLevel.Info;

        /// <summary>
        /// Configuration personalisable pour ce processus
        /// </summary>
        public ParamValues FluxConfig { get; set; }

        /// <summary>
        /// Pour forcer l'arret du processus
        /// </summary>
        public System.Threading.CancellationToken CancelToken { get; set; }

        /// <summary>
        /// Methode lancé par defaul si RunInstance() non hérité
        /// </summary>
        public Action ProccesRunActionMethod { get; set; }

        /// <summary>
        /// Methode lancé par defaul si Init() non hérité
        /// </summary>
        public Action ProccesInitActionMethod { get; set; }


        /// <summary>
        /// Error
        /// </summary>
        public Exception ErrorException { get; set; }

        /// <summary>
        /// Mutex lible d'utilisation
        /// </summary>
        public System.Threading.Mutex Mutex { get; set; }

        /// <summary>
        /// Données Libres (Cast?)
        /// </summary>
        public Dictionary<string, object> DatasDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Données Libre  (Cast?)
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Nombre d'itérations de lancement de la boucle principale
        /// note -1 = Illimité
        /// </summary>
        public int ConfigLoopIterations = -1;



        /// <summary>
        /// Temps de pause entre chaque itérations, en millisecondes
        /// </summary>
        public int ConfigLoopSleep = 100;


        /// <summary>
        /// Cacher les logs sur la console
        /// </summary>
        public bool ShowLogInConsole { get; set; }


        /// <summary>
        /// Obtenir le status boucle
        /// </summary>
        /// <returns></returns>
        public string GetLoopStateText()
        {
            return string.Format("");
           // return string.Format("Iteration {0}/{1}", this.ProcessIterationCount, this.);
        }




        /// <summary>
        /// Chargement des paramètres avec datavalue/xml
        /// </summary>
        /// <param name="FluxConfig"></param>
        public void LoadConfig(ParamValues FluxConfig)
        {
            this.FluxConfig = FluxConfig;

            // COnfig Loop
            ParamValuesNode FluxConfigProcessLoop = FluxConfig.Get("/param/process/loop", false);
            if(FluxConfigProcessLoop!=null)
            {
                if(FluxConfigProcessLoop.GetObject("iteration", DataAccessorOptionEnum.None)!=null)
                    this.ConfigLoopIterations = FluxConfigProcessLoop.GetInt("iterations");
                if (FluxConfigProcessLoop.GetObject("sleep", DataAccessorOptionEnum.None) != null)
                    this.ConfigLoopSleep = FluxConfigProcessLoop.GetInt("sleep");
            }

        }





    }
}
