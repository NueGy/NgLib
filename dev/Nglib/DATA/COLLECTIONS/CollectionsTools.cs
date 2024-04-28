using Nglib.DATA.KEYVALUES;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nglib.DATA.COLLECTIONS
{
    /// <summary>
    ///     Outils pour manipuler  List et Dictionary
    /// </summary>
    public static class CollectionsTools
    {



        /// <summary>
        ///     Contient la clef
        /// </summary>
        public static bool ContainsKey<TValue>(this IDictionary<string, TValue> dic, string keySearch, bool Insensitive)
        {
            return dic.Keys.Contains(keySearch, Insensitive);
        }

        /// <summary>
        ///     Contient au moins l'une des clefs
        /// </summary>
        public static bool Contains(this ICollection<string> keys, ICollection<string> keysSearch,
            bool Insensitive = true)
        {
            if (keys == null || keys.Count == 0 || keysSearch == null || keysSearch.Count == 0) return false;
            var compare = Insensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var itemkey in keys)
            {
                if (string.IsNullOrEmpty(itemkey)) continue;
                if (keysSearch.Count(kal => itemkey.Equals(kal, compare)) > 0)
                    return true;
            }

            return true;
        }


        /// <summary>
        ///     Si la liste contient la clef, avec mode Insensitive
        /// </summary>
        public static bool Contains(this ICollection<string> keys, string keySearch, bool Insensitive)
        {
            if (string.IsNullOrEmpty(keySearch)) return false;
            var compare = Insensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (keys.Any(kal => keySearch.Equals(kal, compare)))
                return true;
            return false;
        }


        /// <summary>
        /// Si le string est dans la liste
        /// Comme string[].Contains() mais en inversé
        /// </summary>
        public static bool EqualsList(this string str, ICollection<string> keysSearch, bool Insensitive = true)
        {
            return keysSearch.Contains(str, Insensitive);
        }

        /// <summary>
        /// Si le string est dans la liste (InsensitiveMode = true)
        /// Comme string[].Contains() mais en inversé
        /// </summary>
        public static bool EqualsList(this string str, params string[] keysSearch)
        {
            return keysSearch.Contains(str, true);
        }

        /// <summary>
        /// Fusionner deux dictionaries, si la clef existe déjà, elle est remplacée
        /// </summary>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dicOrigin,
            IDictionary<TKey, TValue> dicToAdd, bool AllowOverride = true)
        {
            dicToAdd.ForEachSafe(x =>
            {
                if (!dicOrigin.ContainsKey(x.Key)) dicOrigin.Add(x.Key, x.Value);
                else if (AllowOverride) dicOrigin[x.Key] = x.Value;
            });
        }


        /// <summary>
        ///     Permet d'ajouter ou remplacer une valeur
        /// </summary>
        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (!dic.ContainsKey(key)) dic.Add(key, value);
            else dic[key] = value;
        }


        /// <summary>
        ///     Permet d'ajouter ou remplacer une valeur avec gestion de la casse (Insensitive)
        /// </summary>
        public static void AddOrReplace<TValue>(this IDictionary<string, TValue> dic, string keyString, TValue value, bool Insensitive)
        {
            if (!dic.ContainsKey(keyString,Insensitive)) dic.Add(keyString, value);
            string realkey = dic.Keys.FirstOrDefault(k => keyString.Equals(k, StringComparison.OrdinalIgnoreCase));
            dic[realkey] = value;
        }


        /// <summary>
        /// Linq, Retourne que les éléments non null
        /// </summary>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
        {
            return source.Where(x => x != null);
        }



        /// <summary>
        ///  Comme Linq ForEach
        /// </summary>
        /// <returns>false:ok true:anyError</returns>
        public static bool ForEachSafe<T>(this IEnumerable<T> source, Action<T> action)
        {
            bool anyError = false;
            foreach (var item in source)
            {
                try
                {
                    action(item);
                }
                catch (Exception ex)
                {
                    anyError = true;
                }
            }
            return anyError;
        }



        /// <summary>
        ///     Diviser en autant de collections que nécessaire (avec autant d'éléments dans chaque liste)
        /// </summary>
        public static List<T[]> Divide<T>(this ICollection<T> collection, int maxCountItemByList)
        {
            var chunks = new List<T[]>();
            var chunkCount = collection.Count() / maxCountItemByList;

            if (collection.Count % maxCountItemByList > 0)
                chunkCount++;

            for (var i = 0; i < chunkCount; i++)
                chunks.Add(collection.Skip(i * maxCountItemByList).Take(maxCountItemByList).ToArray());

            return chunks;
        }

        /// <summary>
        ///     Diviser en un nombre de listes fixe, avec autant éléments dans chaque listes
        /// </summary>
        [Obsolete("Soon",true)]
        public static List<T[]> DivideFixed<T>(this ICollection<T> collection, int countOfPart)
        {
            var chunks = new List<T[]>();
            var chunkSize = collection.Count() / countOfPart;

            //for (var i = 0; i < chunkCount; i++)
            //    chunks.Add(collection.Skip(i * sizeOfPart).Take(sizeOfPart).ToList());

            return chunks;
        }

        /// <summary>
        /// ForEach Utilisable sur les dictionnaires (simple, comme linq)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        /// <summary>
        ///     Basculer le premier objet en dernier
        /// </summary>
        public static void MoveFirstToLast(this IList list)
        {
            var count = list.Count;
            if (count < 2) return;
            var item = list[0];
            list.RemoveAt(0);
            list.Insert(count - 1, item);
        }



        /// <summary>
        /// Permet de clonner une liste, en utilisant ICloneable si possible
        /// </summary>
        public static List<TValue> Clone<TValue>(this IList<TValue> list)
        {
            if (list == null) return new List<TValue>();
            return list.Select(x => (x is ICloneable)? ((TValue)((ICloneable)x).Clone()) : x).ToList();
        }


        /// <summary>
        /// Obtenir une valeur  (No CaseSensitive), safe retourne null si non trouvé
        /// </summary>
        public static string GetString(this IDictionary<string, string> dic, string key)
        {
            if (string.IsNullOrEmpty(key) || dic == null) return null;
            var val = dic.FirstOrDefault(d => key.Equals(d.Key, StringComparison.OrdinalIgnoreCase));
            return val.Value;
        }

        /// <summary>
        /// Obtenir une valeur  (No CaseSensitive), safe retourne null si non trouvé
        /// </summary>
        public static string GetString(this IDictionary<string, object> dic, string key)
        {
            var obj = GetObject(dic, key);
            if (obj == null || obj == DBNull.Value) return "";
            return Convert.ToString(obj);
        }

        /// <summary>
        /// Obtenir une valeur (No CaseSensitive), safe retourne null si non trouvé
        /// </summary>
        public static object GetObject(this IDictionary<string, object> dic, string key)
        {
            if (string.IsNullOrEmpty(key) || dic == null) return null;
            var val = dic.FirstOrDefault(d => key.Equals(d.Key, StringComparison.OrdinalIgnoreCase));
            //if (val.) return null;
            return val.Value;
        }

        /// <summary>
        /// Obtenir une valeur, safe retourne null si non trouvé
        /// </summary>
        public static TValue GetSafe<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            if (key ==null|| dic == null) return default(TValue);
            if(dic.ContainsKey(key)) return dic[key];
            else return default(TValue);
        }

        


    }
}