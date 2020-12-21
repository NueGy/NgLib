using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.COLLECTIONS
{


    /// <summary>
    /// Gestion d'une collection de Stopwatch
    /// </summary>
    [Obsolete("DevSoon")]
    public class WatchCounters
    {
        private Dictionary<string, long> elapseds = new Dictionary<string, long>();
        private Dictionary<string, System.Diagnostics.Stopwatch> timewatchs = new Dictionary<string, System.Diagnostics.Stopwatch>();
        private System.Threading.Mutex mutex = new System.Threading.Mutex();

        public WatchCounters()
        {
            this.CountersStart = DateTime.Now;
        }

        public DateTime CountersStart { get; set; }





        public long GetElapsedMilliseconds(string countername, bool CreateIfEmpty = true)
        {
            try
            {
                mutex.WaitOne();
                if (countername == null) throw new ArgumentNullException("countername");
                countername = countername.Trim().ToLower();
                if (!CreateIfEmpty && !elapseds.ContainsKey(countername)) return 0;

                if (!elapseds.ContainsKey(countername))
                {
                    timewatchs.Add(countername, new System.Diagnostics.Stopwatch());
                    elapseds.Add(countername, 0);
                }


                bool isrunning = timewatchs[countername].IsRunning;
                if (isrunning) // SI en route au moment de la demande de mesure d'intervalle, on arrete, on cummule et on relance
                {
                    timewatchs[countername].Stop();
                    long elaps = timewatchs[countername].ElapsedMilliseconds;
                    elapseds[countername] += elaps;
                    timewatchs[countername].Reset();
                    timewatchs[countername].Start();
                }

                return elapseds[countername];
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public void SetElapsedMilliseconds(string countername, long newValue, bool CreateIfEmpty = true)
        {
            try
            {
                mutex.WaitOne();
                if (countername == null) throw new ArgumentNullException("countername");
                countername = countername.Trim().ToLower();
                if (!CreateIfEmpty && !elapseds.ContainsKey(countername)) return;

                if (!elapseds.ContainsKey(countername))
                {
                    timewatchs.Add(countername, new System.Diagnostics.Stopwatch());
                    elapseds.Add(countername, 0);
                }
                elapseds[countername] = newValue;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }


        public void AddElapsedMilliseconds(string countername, long newCummulValue, bool CreateIfEmpty = true)
        {
            try
            {
                mutex.WaitOne();
                if (countername == null) throw new ArgumentNullException("countername");
                countername = countername.Trim().ToLower();
                if (!CreateIfEmpty && !elapseds.ContainsKey(countername)) return;

                if (!elapseds.ContainsKey(countername))
                {
                    timewatchs.Add(countername, new System.Diagnostics.Stopwatch());
                    elapseds.Add(countername, 0);
                }

                bool isrunning = timewatchs[countername].IsRunning;
                if (isrunning) // SI en route au moment de la demande de mesure d'intervalle, on arrete, on cummule et on relance
                {
                    timewatchs[countername].Stop();
                    long elaps = timewatchs[countername].ElapsedMilliseconds;
                    elapseds[countername] += elaps;
                    timewatchs[countername].Reset();
                    timewatchs[countername].Start();
                }


                elapseds[countername] += newCummulValue;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }








        private System.Diagnostics.Stopwatch GetWatch(string countername, bool CreateIfEmpty = true)
        {

            if (countername == null) throw new ArgumentNullException("countername");
            countername = countername.Trim().ToLower();
            if (!timewatchs.ContainsKey(countername) && CreateIfEmpty)
            {
                timewatchs.Add(countername, new System.Diagnostics.Stopwatch());
                elapseds.Add(countername, 0);
            }

            if (!timewatchs.ContainsKey(countername)) return null;
            else return timewatchs[countername];
        }





        public void Start(string countername)
        {
            try
            {
                mutex.WaitOne();
                this.GetWatch(countername, true).Start();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public void Stop(string countername)
        {
            long elaps = 0;
            try
            {
                mutex.WaitOne();
                var counter = this.GetWatch(countername, false);
                if (counter == null) return;
                counter.Stop();
                elaps = counter.ElapsedMilliseconds;
                counter.Reset();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            this.AddElapsedMilliseconds(countername, elaps);
        }



        public void Reset(string countername)
        {
            this.Stop(countername);
            this.SetElapsedMilliseconds(countername, 0, false);
        }


        public List<string> GetCounterNames()
        {
            try
            {
                return this.elapseds.Keys.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Dictionary<string, long> GetValues()
        {
            List<string> counternames = this.GetCounterNames();
            Dictionary<string, long> retour = new Dictionary<string, long>();
            foreach (string countername in counternames)
                retour.Add(countername, this.GetElapsedMilliseconds(countername));
            return retour;
        }


        public void Cumulate(WatchCounters counters)
        {
            if (counters == null) return;
            Dictionary<string, long> valstocumulates = counters.GetValues();
            foreach (var valuesitems in valstocumulates)
            {
                this.AddElapsedMilliseconds(valuesitems.Key, valuesitems.Value, true);
            }
        }



        public string GetInfosCounters()
        {
            try
            {
                Dictionary<string, long> vals = this.GetValues();
                StringBuilder retour = new StringBuilder();
                string virgule = "";
                foreach (var item in vals)
                {
                    retour.AppendFormat("{2}{0}:{1}ms", item.Key, item.Value, virgule);
                    virgule = " |";
                }
                return retour.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetInfosCounters " + ex.Message, ex);
            }

        }



        public override string ToString()
        {
            try
            {
                return GetInfosCounters();
            }
            catch (Exception)
            {
                return base.ToString();
            }

        }

    }


}
