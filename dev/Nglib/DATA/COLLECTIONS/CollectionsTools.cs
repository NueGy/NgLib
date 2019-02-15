using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

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





        /// <summary>
        /// Fusionner un dictionary
        /// Adds new keys only
        /// </summary>
        /// <param name="dic">origine</param>
        /// <param name="dicToAdd">rangetomerge</param>
        /// <param name="AllowOverride">remplace ou ignore les memes clef</param>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd, bool AllowOverride=true)
        {
            dicToAdd.ForEach(x => { if (!dic.ContainsKey(x.Key)) dic.Add(x.Key, x.Value); else if(AllowOverride) dic[x.Key] = x.Value; });
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


    }
}
