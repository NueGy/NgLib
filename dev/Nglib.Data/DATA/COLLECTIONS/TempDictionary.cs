using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nglib.DATA.COLLECTIONS
{
    /// <summary>
    /// Un dictionaire de données avec gestion de la durée de rétention 
    /// Use ConcurrentDictionary (Safe Thread et Safe nullable)
    /// </summary>
    /// <typeparam name="TKey">Type de clé</typeparam>
    /// <typeparam name="TValue">Type de valeur</typeparam>
    public class TempDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private readonly ConcurrentDictionary<TKey, TValue> keyValues = new ConcurrentDictionary<TKey, TValue>(); //données
        private readonly ConcurrentDictionary<TKey, DateTime> expireAts = new ConcurrentDictionary<TKey, DateTime>();
        private int _defaultExpiration;
        //private int _maximumStack;
        
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public TempDictionary()
        {
            _defaultExpiration = 30;
           // _maximumStack = 0; // illimité
        }
        
        /// <summary>
        /// Constructeur avec expiration par défaut
        /// </summary>
        public TempDictionary(int defaultExpiration)
        {
            _defaultExpiration = defaultExpiration;
            // _maximumStack = maximumStack;
            // new CacheDictionary();
        }



        /// <summary>
        /// Accès par clé
        /// </summary>
        public TValue this[TKey key] { get => this.Get(key); set => this.Set(key,value); }
        
        /// <summary>
        /// Nombre d'éléments
        /// </summary>
        public int Count => keyValues.Count;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">clef</param>
        /// <param name="ignoreCacheRules">activer ou non la suppression de l'objet si obsolete</param>
        /// <returns></returns>
        public TValue Get(TKey key, bool ignoreCacheRules=false)
        {
            if (!this.ContainsKey(key)) return default(TValue); // plus simple? !!!
            TValue value = keyValues[key];
            if (!ignoreCacheRules)
            {
                DateTime expireat = expireAts[key];
                if(expireat>DateTime.Now)
                {
                    this.Remove(key);
                    return default(TValue);
                }
            }
            return value;
        }

        /// <summary>
        /// Définir une valeur
        /// </summary>
        /// <param name="key">Clé du dictionnaire</param>
        /// <param name="value">Valeur à stocker</param>
        /// <param name="ExpireAt">Date d'expiration</param>
        public void Set(TKey key, TValue value, DateTime ExpireAt)
        { 
            keyValues[key] = value;
            expireAts[key] = ExpireAt;

            //if(_maximumStack>0) // On efface le plus vieu
            //{
            //    //!!!
            //}



        }

        /// <summary>
        /// Définir une valeur
        /// </summary>
        /// <param name="key">Clé du dictionnaire</param>
        /// <param name="value">Valeur à stocker</param>
        /// <param name="keepTimeSecond">Durée de conservation en secondes</param>
        public void Set(TKey key, TValue value, int? keepTimeSecond=null)
        {
            var expireat = new DateTime().AddSeconds(keepTimeSecond.HasValue ? keepTimeSecond.Value : this._defaultExpiration);
            this.Set(key, value, expireat);
        }

        /// <summary>
        /// Clés du dictionnaire
        /// </summary>
        public ICollection<TKey> Keys => keyValues.Keys;

        /// <summary>
        /// Valeurs du dictionnaire
        /// </summary>
        public ICollection<TValue> Values => keyValues.Values;


        

        /// <summary>
        /// Vide le dictionnaire
        /// </summary>
        public void Clear()
        {
            keyValues.Clear();
        }



        /// <summary>
        /// Vérifie si la clé existe
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return keyValues.ContainsKey(key);
        }

        /// <summary>
        /// Supprime une clé
        /// </summary>
        public bool Remove(TKey key)
        {
            TValue val;
            DateTime vald;
            keyValues.TryRemove(key, out val);
            return expireAts.TryRemove(key, out vald);
        }

        /// <summary>
        /// Énumérateur
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return keyValues.GetEnumerator();
        }
        


        IEnumerator IEnumerable.GetEnumerator()
        {
            return keyValues.GetEnumerator();
        }


        /// <summary>
        /// CleanUp : Supprimer les élement obsoletes
        /// </summary>
        /// <returns></returns>
        public async Task<int> CleanAsync()
        {
            try
            {
                int retour = 0;
                DateTime now = DateTime.Now;
                foreach (TKey key in this.expireAts.Keys.ToList())
                {
                    DateTime expireat = this.expireAts[key];
                    if(expireat< now)
                    {
                        this.Remove(key);
                        retour++;
                    }
                }
                return retour;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Prolongation du temps
        /// </summary>
        /// <param name="key">clef</param>
        /// <param name="expireAt">prolongation du temp</param>
        /// <returns></returns>
        public bool OverTime(TKey key, DateTime expireAt)
        {
            if (!this.expireAts.ContainsKey(key)) return false;
            this.expireAts[key] = expireAt;
            return true;
        }


    }
}
