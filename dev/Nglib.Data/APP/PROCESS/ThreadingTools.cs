using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nglib.APP.PROCESS
{
    /// <summary>
    /// Outils pour gérer le multithreading
    /// </summary>
    public static class ThreadingTools
    {

        /// <summary>
        /// Savoir si le thread est en cours d'execution
        /// </summary>
        public static bool IsRunning(this Thread thread)
        {
            if (thread == null) return false;
            List<ThreadState> runningstates = new List<ThreadState> { ThreadState.Running, ThreadState.Background, ThreadState.WaitSleepJoin };
            if (runningstates.Any(f => thread.ThreadState.HasFlag(f)))
                return true;
            else
                return false;
        }


        /// <summary>
        /// Vérifie si la tâche est en cours d'exécution
        /// </summary>
        public static bool IsRunning(this Task task)
        {
            if (task == null) return false;
            return task.Status == TaskStatus.Running || task.Status == TaskStatus.WaitingForActivation || task.Status == TaskStatus.WaitingToRun;
        }


        /// <summary>
        /// Attend le résultat d'une tâche de manière synchrone (équivalent simplifié de await)
        /// Utilise ConfigureAwait(false) pour éviter les deadlocks
        /// </summary>
        public static void Await(this Task task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Attend le résultat d'une tâche de manière synchrone et retourne la valeur (équivalent simplifié de await)
        /// Utilise ConfigureAwait(false) pour éviter les deadlocks
        /// </summary>
        public static T Await<T>(this Task<T> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

    }
}
