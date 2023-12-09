using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.DIAG
{
    /// <summary>
    /// Une chaine pour gérer des logs
    /// </summary>
    public class LogAtStringBuilder : IDisposable
    {
        public StringBuilder LogStack { get; set; } = new StringBuilder();
        public System.Diagnostics.Stopwatch Stopwatch { get; set;} 

        public LogAtStringBuilder() { this.Stopwatch = new System.Diagnostics.Stopwatch(); this.Stopwatch.Start(); }
        public LogAtStringBuilder(System.Diagnostics.Stopwatch StopwatchOrigin) { this.Stopwatch = StopwatchOrigin; }



        public void LogAt(string text)
        {
            string txt = $"{text}(at {this.Stopwatch.ElapsedMilliseconds}ms)";
            this.LogStack.AppendLine(txt);
        }


        public override string ToString()
        {
            return LogStack.ToString();
        }


        public void Dispose()
        {
            Stopwatch.Stop();
            //LogStack.disc
        }
    }
}
