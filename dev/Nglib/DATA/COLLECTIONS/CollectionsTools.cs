using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nglib.DATA.COLLECTIONS
{
    /// <summary>
    /// Static utility tools for manipulating List and Dictionary collections with advanced features.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_collections"/></para>
    /// </summary>
    public static class CollectionsTools
    {



        /// <summary>
        /// Checks if the dictionary contains the specified key.
        /// </summary>
        public static bool ContainsKey<TValue>(this IDictionary<string, TValue> dic, string keySearch, bool insensitive)
        {
            return dic.Keys.Contains(keySearch, insensitive);
        }

        /// <summary>
        /// Checks if the collection contains at least one of the specified keys.
        /// </summary>
        public static bool Contains(this ICollection<string> keys, ICollection<string> keysSearch,
            bool insensitive = true)
        {
            if (keys == null || keys.Count == 0 || keysSearch == null || keysSearch.Count == 0) return false;
            var compare = insensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var itemkey in keys)
            {
                if (string.IsNullOrEmpty(itemkey)) continue;
                if (keysSearch.Count(kal => itemkey.Equals(kal, compare)) > 0)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Checks if the collection contains the specified key with optional case-insensitive mode.
        /// </summary>
        public static bool Contains(this ICollection<string> keys, string keySearch, bool insensitive)
        {
            if (keys == null || keys.Count == 0) return false;
            if (string.IsNullOrEmpty(keySearch)) return false;
            var compare = insensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (keys.Any(kal => keySearch.Equals(kal, compare)))
                return true;
            return false;
        }


        /// <summary>
        /// Checks if the string is in the list (inverted Contains).
        /// </summary>
        public static bool ContainInList(this string str, ICollection<string> keysSearch, bool insensitive = true)
        {
            return keysSearch.Contains(str, insensitive);
        }

        /// <summary>
        /// Checks if the string is in the list (InsensitiveMode = true, inverted Contains).
        /// </summary>
        public static bool ContainInList(this string str, params string[] keysSearch)
        {
            return keysSearch.Contains(str, true);
        }

        /// <summary>
        /// Merges two dictionaries. If a key already exists, it is replaced.
        /// </summary>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dicOrigin,
            IDictionary<TKey, TValue> dicToAdd, bool allowOverride = true)
        {
            dicToAdd.ForEachSafe(x =>
            {
                if (!dicOrigin.ContainsKey(x.Key)) dicOrigin.Add(x.Key, x.Value);
                else if (allowOverride) dicOrigin[x.Key] = x.Value;
            });
        }


        /// <summary>
        /// Adds or replaces a value in the dictionary.
        /// </summary>
        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (!dic.ContainsKey(key)) dic.Add(key, value);
            else dic[key] = value;
        }


        /// <summary>
        /// Adds or replaces a value with case-insensitive handling.
        /// </summary>
        public static void AddOrReplace<TValue>(this IDictionary<string, TValue> dic, string keyString, TValue value, bool insensitive)
        {
            if (!dic.ContainsKey(keyString, insensitive)) dic.Add(keyString, value);
            else
            {
                string realkey = dic.Keys.FirstOrDefault(k => keyString.Equals(k, insensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                if (realkey != null) dic[realkey] = value;
            }
        }


        /// <summary>
        /// LINQ extension: Returns only non-null elements.
        /// </summary>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
        {
            return source.Where(x => x != null);
        }



        /// <summary>
        /// Like LINQ ForEach but with safe error handling.
        /// </summary>
        /// <returns>false: ok, true: anyError</returns>
        public static bool ForEachSafe<T>(this IEnumerable<T> source, Action<T> action)
        {
            bool anyError = false;
            foreach (var item in source)
            {
                try
                {
                    action(item);
                }
                catch (Exception)
                {
                    anyError = true;
                }
            }
            return anyError;
        }



        /// <summary>
        /// Divides the collection into multiple arrays with a maximum number of items per array.
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
        /// Divides the collection into a fixed number of arrays with balanced distribution.
        /// </summary>
        public static List<T[]> DivideFixed<T>(this ICollection<T> collection, int countOfPart)
        {
            if (collection == null || countOfPart <= 0) return new List<T[]>();
            if (countOfPart == 1) return new List<T[]> { collection.ToArray() };
            
            var chunks = new List<T[]>();
            var totalItems = collection.Count;
            var itemsPerChunk = totalItems / countOfPart;
            var remainder = totalItems % countOfPart;
            
            var currentIndex = 0;
            var collectionArray = collection.ToArray();
            
            for (var i = 0; i < countOfPart; i++)
            {
                var chunkSize = itemsPerChunk + (i < remainder ? 1 : 0);
                if (chunkSize > 0 && currentIndex < totalItems)
                {
                    var chunk = new T[chunkSize];
                    Array.Copy(collectionArray, currentIndex, chunk, 0, chunkSize);
                    chunks.Add(chunk);
                    currentIndex += chunkSize;
                }
                else
                {
                    chunks.Add(new T[0]);
                }
            }
            
            return chunks;
        }

        /// <summary>
        /// ForEach extension usable on dictionaries (simple, like LINQ).
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        /// <summary>
        /// Moves the first item to the last position.
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
        /// Clones a list using ICloneable if available.
        /// </summary>
        public static List<TValue> Clone<TValue>(this IList<TValue> list)
        {
            if (list == null) return new List<TValue>();
            return list.Select(x => (x is ICloneable)? ((TValue)((ICloneable)x).Clone()) : x).ToList();
        }


        /// <summary>
        /// Gets a value (case-insensitive), safe mode returns null if not found.
        /// </summary>
        public static string GetSafeString(this IDictionary<string, string> dic, string key)
        {
            if (string.IsNullOrEmpty(key) || dic == null) return null;
            var val = dic.FirstOrDefault(d => key.Equals(d.Key, StringComparison.OrdinalIgnoreCase));
            return val.Value;
        }

        /// <summary>
        /// Gets a value (case-insensitive), safe mode returns empty string if not found.
        /// </summary>
        public static string GetSafeString(this IDictionary<string, object> dic, string key)
        {
            var obj = GetSafeObject(dic, key);
            if (obj == null || obj == DBNull.Value) return "";
            return Convert.ToString(obj);
        }

        /// <summary>
        /// Gets an object value (case-insensitive), safe mode returns null if not found.
        /// </summary>
        public static object GetSafeObject(this IDictionary<string, object> dic, string key)
        {
            if (string.IsNullOrEmpty(key) || dic == null) return null;
            
            var val = dic.Where(d => key.Equals(d.Key, StringComparison.OrdinalIgnoreCase)).Select(d=>d.Value).FirstOrDefault();
            //if (val.) return null;
            return val;
        }

        /// <summary>
        /// Gets a value, safe mode returns default(TValue) if not found.
        /// </summary>
        public static TValue GetSafeValue<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            if (key ==null|| dic == null) return default(TValue);
            if(dic.ContainsKey(key)) return dic[key];
            else return default(TValue);
        }

        


    }
}