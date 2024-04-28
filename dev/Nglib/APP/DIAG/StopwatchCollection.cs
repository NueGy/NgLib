using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nglib.APP.DIAG
{
    /// <summary>
    /// Permet de gérer une collection de mesures
    /// </summary>
    [Obsolete("BETA")]
    public class StopwatchCollection 
    {
        public Dictionary<string, Stopwatch> Watchs = new Dictionary<string, Stopwatch>();
        private object threadLoc = new object(); 


        public void Start(string counterName)
        {
            Stopwatch watch = this.GetOrCreate(counterName);
            watch.Start();
        }

        public void Stop(string counterName)
        {
            Stopwatch watch = this.GetOrCreate(counterName,false);
            if(watch!=null) watch.Stop();
        }

        public void Restart(string counterName)
        {
            Stopwatch watch = this.GetOrCreate(counterName);
            watch.Restart();
        }

        public long GetElapsedMilliseconds(string counterName)
        {
            Stopwatch watch = this.GetOrCreate(counterName,false);
            if (watch != null) return watch.ElapsedMilliseconds;
            else return 0;
        }

        public TimeSpan GetElapsed(string counterName)
        {
            Stopwatch watch = this.GetOrCreate(counterName, false);
            if (watch != null) return watch.Elapsed;
            else return TimeSpan.MinValue;
        }

        public Stopwatch GetOrCreate(string counterName, bool CreateIfNotExist=true)
        {
            if (string.IsNullOrWhiteSpace(counterName)) return null;
            counterName = counterName.Trim().ToUpper();
            lock (threadLoc)
            {
                if (!this.Watchs.ContainsKey(counterName))
                {
                    if (!CreateIfNotExist) return null;
                    this.Watchs.Add(counterName, new Stopwatch());
                }
                return this.Watchs[counterName];
            }
        }


        public void StopAll()
        {
            lock (threadLoc)
            {
                this.Watchs.Values.ForEach(w => w.Stop());
            }
        }



        public Dictionary<string,long> GetElapsedMilliseconds()
        {
            lock (threadLoc)
            {
                return this.Watchs.ToDictionary(w => w.Key, w=> w.Value.ElapsedMilliseconds);
            }
        }

        public Dictionary<string, TimeSpan> GetElapsed()
        {
            lock (threadLoc)
            {
                return this.Watchs.ToDictionary(w => w.Key, w => w.Value.Elapsed);
            }
        }

    }
}
