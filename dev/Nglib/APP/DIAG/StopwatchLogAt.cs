using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.DIAG
{
    /// <summary>
    /// Stopwatch-based tracing utility for performance monitoring and logging.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_appdiag"/></para>
    /// </summary>
    public class StopwatchLogAt
    {
        /// <summary>
        /// The main stopwatch timer.
        /// </summary>
        public System.Diagnostics.Stopwatch Stopwatch { get; private set; } = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// The collected trace logs.
        /// </summary>
        public List<string> Traces { get; private set; } = new List<string>();

        /// <summary>
        /// Free value to identify the object (compartment/category).
        /// </summary>
        public string Compartment { get; set; }

        /// <summary>
        /// If true, time is displayed before the text.
        /// </summary>
        public bool TimeBeforeText { get; set; }

        /// <summary>
        /// Minimum verbosity level to accept traces.
        /// </summary>
        public System.Diagnostics.TraceLevel VerbosityMin = System.Diagnostics.TraceLevel.Info;

        /// <summary>
        /// Contains at least one error (System.Diagnostics.TraceLevel.Error).
        /// </summary>
        public bool AnyError { get; set; }

        /// <summary>
        /// Thread synchronization lock.
        /// </summary>
        private object ThreadLock { get; set; } = new object();

        /// <summary>
        /// Start date when Start() was first called.
        /// </summary>
        public DateTime? FirstStart { get; set; }

        /// <summary>
        /// Starts the stopwatch.
        /// </summary>
        /// <param name="msg">Optional message to log</param>
        public void Start(string msg = null)
        {
            this.Stopwatch.Start();
            this.AddTrace(msg);
            if (!FirstStart.HasValue)
                FirstStart = DateTime.Now;
        }

        /// <summary>
        /// Stops the stopwatch.
        /// </summary>
        /// <param name="msg">Optional message to log</param>
        public void Stop(string msg = null)
        {
            this.Stopwatch.Stop();
            this.AddTrace(msg);
        }

        /// <summary>
        /// Gets the elapsed time in milliseconds.
        /// </summary>
        public long ElapsedMilliseconds => this.Stopwatch.ElapsedMilliseconds;

        /// <summary>
        /// Adds a trace log entry with timestamp.
        /// </summary>
        /// <param name="msg">The message to log</param>
        /// <param name="level">The trace level (default: Info)</param>
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
            if (level == System.Diagnostics.TraceLevel.Warning) msg += "[WARNING]";

            lock (ThreadLock)
            {
                if (level== System.Diagnostics.TraceLevel.Error) this.AnyError = true;
                this.Traces.Add(msg);
            }
        }

        /// <summary>
        /// Creates and starts a new StopwatchLogAt instance.
        /// </summary>
        /// <returns>A started StopwatchLogAt instance</returns>
        public static StopwatchLogAt StartNew()
        {
            StopwatchLogAt retour = new StopwatchLogAt();
            retour.Start();
            return retour;
        }

        /// <summary>
        /// Returns all traces as a formatted string.
        /// </summary>
        /// <returns>String representation of all traces</returns>
        public override string ToString()
        {
            StringBuilder retour = new StringBuilder();
            this.Traces.ForEach(t => retour.AppendLine(t));
            return retour.ToString();
        }


    }
}
