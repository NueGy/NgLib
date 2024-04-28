using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.DIAG
{
    /// <summary>
    /// Permet de gérer des traces basées sur un stopWatch
    /// </summary>
    [Obsolete("BETA")]
    public class StopwatchLogAt
    {
        public System.Diagnostics.Stopwatch Stopwatch { get; set; } = new System.Diagnostics.Stopwatch();
        public List<string> Traces = new List<string>();
        public string Compartment { get; set; }
        public bool TimeBeforeText { get; set; }
        public System.Diagnostics.TraceLevel VerbosityMin = System.Diagnostics.TraceLevel.Info;

        private object ThreadLock { get; set; } = new object();

        public DateTime? FirstStart { get; set; }

        /// <summary>
        /// Start Stopwatch
        /// </summary>
        public void Start(string msg = null) { this.Stopwatch.Start(); this.AddTrace(msg); if (!FirstStart.HasValue) FirstStart = DateTime.Now; }

        /// <summary>
        /// Stop Stopwatch
        /// </summary>
        public void Stop(string msg=null) { this.Stopwatch.Stop(); this.AddTrace(msg); }

        /// <summary>
        /// Get Stopwatch.ElapsedMilliseconds
        /// </summary>
        public long ElapsedMilliseconds => this.Stopwatch.ElapsedMilliseconds;

        public void AddTrace(string msg, System.Diagnostics.TraceLevel level= System.Diagnostics.TraceLevel.Info)
        {
            if (string.IsNullOrEmpty(msg)) return;
            if (this.VerbosityMin == System.Diagnostics.TraceLevel.Off) return;
            if (level > this.VerbosityMin) return;

            msg = msg.Replace("\r", "").Replace("\n", "\t");
            if (!string.IsNullOrEmpty(this.Compartment)) msg = $"[{this.Compartment}]" + msg;
            if (!TimeBeforeText && Stopwatch.ElapsedMilliseconds<60000) msg += $" At {Stopwatch.ElapsedMilliseconds} ms";
            else if (!TimeBeforeText) msg += $" At {Stopwatch.Elapsed.ToString()}";
            else msg = $"[{Stopwatch.Elapsed.ToString()}]"+ msg;
            if (level == System.Diagnostics.TraceLevel.Error) msg += "[ERROR]";

            lock (ThreadLock)
            {
                this.Traces.Add(msg);
            }
        }


        public static StopwatchLogAt StartNew()
        {
            StopwatchLogAt retour = new StopwatchLogAt();
            retour.Start();
            return retour;
        }

        public override string ToString()
        {
            StringBuilder retour = new StringBuilder();
            this.Traces.ForEach(t => retour.AppendLine(t));
            return retour.ToString();
        }


    }
}
