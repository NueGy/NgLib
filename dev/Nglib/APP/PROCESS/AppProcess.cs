using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.PROCESS
{
    /// <summary>
    /// Permet de mieu gérer les automates
    /// </summary>
    public class AppProcess
    {
        /// <summary>
        /// Context d'execution (config)
        /// </summary>
        public AppProcessContext context { get; private set; }

        public AppProcess()
        {
            this.context = new AppProcessContext();
        }






        /// <summary>
        /// Lancement du processus (Boucle) en utilisant 
        /// METHODE DE LANCEMENT PRINCIPALE
        /// </summary>
        public async Task RunProcessAsync()
        {
            try
            {
                // initialisation du process
                if (!this.context.ProcessInitiate.HasValue)
                {
                    this.context.ProcessInitWatch.Start();
                    await this.LogAsync(string.Format("Process Init : {0} ", context.ProcessName), 0);
                    await this.InitProcessAsync();
                    this.context.ProcessInitWatch.Stop();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Process Init ERROR : {0}", ex.Message), ex);
            }

            try
            {
                this.context.ProcessRunWatch.Start();

                // methode personalisable lancée avant la méthode principale
                await this.RunBeforeInstanceAsync();

                // boucle de la méthode principale
                while (!this.context.CancelToken.IsCancellationRequested)
                {
                    context.ProcessLoopCount++;
                    await this.LogAsync(string.Format("Process Run Iteration {0} ({1})", context.ProcessLoopCount, DateTime.Now.ToShortTimeString()), 0);

                    // !!! Dev multithreading
                    bool retourok = await this.RunInstanceAsync();
                    if (retourok) break; // si positif, on arrete

                    if (this.context.ConfigLoopIterations >= context.ProcessLoopCount) break; // si nombrez d'itérations dépassées (notre si -1 sera donc illimité)
                    if (this.context.ConfigLoopSleep > 0) System.Threading.Thread.Sleep(this.context.ConfigLoopSleep); //sleep obligatoire entre les itération de boucle
                }

                //methode personalisable lancée après la méthode principale
                await this.RunAfterInstanceAsync();

                this.context.ProcessRunWatch.Stop();
            }
            catch (Exception ex)
            {
                this.context.ErrorException = ex;
                throw new Exception(string.Format("Process Run ERROR {0}", ex.Message), ex);
            }
            finally
            {
                await this.LogAsync(string.Format("Process End (in {0}ms) ", context.ProcessRunWatch.ElapsedMilliseconds), 0);
            }
            
        }

        /// <summary>
        /// Lancement du processus (Boucle) en utilisant 
        /// METHODE DE LANCEMENT PRINCIPALE
        /// </summary>
        public void RunProcess()
        {
            this.RunProcessAsync().GetAwaiter().GetResult();
        }





        /// <summary>
        /// Initialisation des données nécessaire au processus
        /// </summary>
        protected virtual async Task InitProcessAsync()
        {
            // init de base
            context.ProcessLoopCount = 0;
            this.context.ProcessInitiate = DateTime.Now;

            if (context.ProccesInitActionMethod != null)
                context.ProccesInitActionMethod(); // INIT EVENT
        }

        




        /// <summary>
        /// Lancement d'une boucle du processus
        /// </summary>
        /// <returns>Traitement concluant ou non : Si true, il arretera la bloucle</returns>
        protected virtual async Task<bool> RunInstanceAsync()
        {
            if(context.ProccesRunActionMethod != null)
                context.ProccesRunActionMethod(); // RUN EVENT
            return true;
        }


        /// <summary>
        /// se lancera avant la methode principale RunInstanceAsync
        /// </summary>
        /// <returns></returns>
        protected virtual async Task RunBeforeInstanceAsync()
        {
            
        }

        /// <summary>
        /// Se lancera après la methode principale RunInstanceAsync
        /// </summary>
        /// <returns></returns>
        protected virtual async Task RunAfterInstanceAsync()
        {
            
        }


        /// <summary>
        /// Evenement lors de la création d'un log
        /// </summary>
        public event LogEventHandler LogEvent;
        public delegate bool LogEventHandler(APP.LOG.ILog log);
        


        /// <summary>
        /// Ajouter un log (SAFE)
        /// </summary>
        /// <returns>false= Error</returns>
        public async Task<bool> LogAsync(string logtext, int LogLevel)
        {
            LOG.LogModel log = new LOG.LogModel() { LogLevel = LogLevel, LogText = logtext, DateCreate = DateTime.Now };
            return await LogAsync(log);
        }

        /// <summary>
        /// Ajouter un log (SAFE)
        /// </summary>
        public bool Log(string logtext, int LogLevel)
        {
            return LogAsync(logtext, LogLevel).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Ajouter un log (SAFE)
        /// </summary>
        /// <returns>false= Error</returns>
        public virtual async Task<bool> LogAsync(APP.LOG.ILog log)
        {
            if (log == null || string.IsNullOrEmpty(log.LogText)) return false;
            try
            {
                if (this.context.ShowLogInConsole)
                    Console.WriteLine(string.Format("LOG({0}): {1}",log.LogLevel, FORMAT.StringTools.Limit(log.LogText,64))); // =64+8= 72
                
                if (LogEvent == null) return false;
                return LogEvent(log);
            }
            catch (Exception)
            {
                return false; // SAFE
            }
        }









    }
}
