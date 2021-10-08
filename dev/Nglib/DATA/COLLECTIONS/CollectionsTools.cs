using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Nglib.DATA.COLLECTIONS
{
    /// <summary>
    /// Outils pour manipuler les List et Dictionary
    /// </summary>
    public static class CollectionsTools
    {


        ///// <summary>
        ///// Permet de rendre le dictionary insensible à la casse
        ///// </summary>
        ///// <param name="dic"></param>
        //public static void MakeInsensitive(this Dictionary<string, dynamic> dic)
        //{
        //    if(dic.Comparer== StringComparison.OrdinalIgnoreCase || )
        //    Dictionary retour = Dictionary()
        //}

        ///// <summary>
        ///// Permet de rendre le dictionary insensible à la casse
        ///// </summary>
        ///// <param name = "dic" ></ param >
        ////public static void MakeLowerKey(this Dictionary<string, dynamic> dic)
        ////{
        ////    if (dic.Comparer == StringComparison.OrdinalIgnoreCase || )
        ////        Dictionary retour = Dictionary()
        ////}
     

        public static bool ContainsKey(this IDictionary<string,dynamic> dic, string keySearch, bool Insensitive)
        {
            return CollectionsTools.Contains(dic.Keys, keySearch, Insensitive);
        }

        public static bool ContainsKey(this IDictionary<string, dynamic> dic, ICollection<string> keysSearch, bool Insensitive = true)
        {
            return CollectionsTools.Contains(dic.Keys, keysSearch, Insensitive);
        }







        /// <summary>
        /// Contient au moins l'une des clefs
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="keysSearch"></param>
        /// <param name="Insensitive"></param>
        /// <returns></returns>
        public static bool Contains(this ICollection<string> keys, ICollection<string> keysSearch, bool Insensitive=true)
        {
            if (keys == null || keys.Count == 0 || keysSearch == null || keysSearch.Count == 0) return false;
            StringComparison compare = (Insensitive)?StringComparison.OrdinalIgnoreCase: StringComparison.Ordinal;
            foreach (string itemkey in keys)
            {
                if (string.IsNullOrEmpty(itemkey)) continue;
                if (keysSearch.Count(kal => itemkey.Equals(kal, compare)) > 0)
                    return true;
            }
            return true;
        }


        /// <summary>
        /// Recehch
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="keySearch"></param>
        /// <param name="Insensitive"></param>
        /// <returns></returns>
        public static bool Contains(this ICollection<string> keys, string keySearch, bool Insensitive)
        {
            if (string.IsNullOrEmpty(keySearch)) return false;
            StringComparison compare = (Insensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (keys.Count(kal => keySearch.Equals(kal, compare)) > 0)
                return true;
            return false;
        }




        public static bool EqualsList(this string str, ICollection<string> keysSearch, bool Insensitive = true)
        {
            return keysSearch.Contains(str, Insensitive);
        }

        public static bool EqualsList(this string str, string[] keysSearch, bool Insensitive = true)
        {
            return keysSearch.Contains(str, Insensitive);
        }


        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd, bool AllowOverride = true)
        {
            dicToAdd.ForEach(x => { if (!dic.ContainsKey(x.Key)) dic.Add(x.Key, x.Value); else if (AllowOverride) dic[x.Key] = x.Value; });
        }



        /// <summary>
        /// Permet d'ajouter ou remplacer une valeur
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (!dic.ContainsKey(key)) dic.Add(key, value); else dic[key] = value;
        }

        public static bool ContainsKeys<TKey, TValue>(this Dictionary<TKey, TValue> dic, IEnumerable<TKey> keys)
        {
            bool result = false;
            keys.ForEachOrBreak((x) => { result = dic.ContainsKey(x); return result; });
            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static void ForEachOrBreak<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            foreach (var item in source)
            {
                bool result = func(item);
                if (result) break;
            }
        }


        /// <summary>
        /// Diviser avec un nombre d'elements fixe par liste
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="sizeOfPart"></param>
        /// <returns></returns>
        public static List<List<T>> DivideFixed<T>(this IList<T> collection, int sizeOfPart)
        {
            var chunks = new List<List<T>>();
            var chunkCount = collection.Count() / sizeOfPart;

            if (collection.Count % sizeOfPart > 0)
                chunkCount++;

            for (var i = 0; i < chunkCount; i++)
                chunks.Add(collection.Skip(i * sizeOfPart).Take(sizeOfPart).ToList());

            return chunks;
        }

        /// <summary>
        /// Diviser en un nombre de part, avec autant d'élements dans chaque part
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="countOfPart"></param>
        /// <returns></returns>
        public static List<List<T>> DivideEqual<T>(this IList<T> collection, int countOfPart)
        {
            var chunks = new List<List<T>>();
 
            return chunks;
        }


        /// <summary>
        /// Masculer le premier objet en dernier
        /// </summary>
        /// <param name="list"></param>
        public static void MoveFirstToLast(this IList list)
        {
            var count = list.Count;

            if (count < 2) return;

            var item = list[0];
            list.RemoveAt(0);
            list.Insert(count - 1, item);
        }



        public static Dictionary<TKey,TValue> Clone<TKey,TValue>(this Dictionary<TKey, TValue> dic)
        {
            if (dic == null) return new Dictionary<TKey, TValue>();
            return dic.ToDictionary(k => k.Key, v => v.Value); // clonner aussi les valeurs ...
        }


        public static string GetSafeString(this IDictionary<string, object> dic, string key)
        {
            object obj = GetSafeObject(dic, key);
            if (obj == null || obj == DBNull.Value) return "";
            return Convert.ToString(obj);
        }
        public static object GetSafeObject(this IDictionary<string, object> dic, string key)
        {
            if (string.IsNullOrEmpty(key) || dic==null) return null;
            var val = dic.FirstOrDefault(d=> key.Equals(d.Key, StringComparison.OrdinalIgnoreCase));
            //if (val.) return null;
            return val.Value;
        }



    }
}
