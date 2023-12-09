using System;
using System.Collections;
using System.Collections.Generic;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValuesPartition<Tval> : IList<Tval> where Tval : KeyValues, new()
    {
        public KeyValuesPartition(KeyValues originalArray)
        {
            this.originalArray = originalArray;
            subKey = "";
        }

        public KeyValuesPartition(KeyValues originalArray, string subKey)
        {
            this.originalArray = originalArray;
            this.subKey = subKey?.TrimEnd('/');
        }

        public KeyValues originalArray { get; }
        public string subKey { get; }

        public int Count => originalArray.GetDatas(subKey).Count;

        public bool IsReadOnly => originalArray.IsReadOnly;

        public Tval this[int index]
        {
            get => throw new NotImplementedException("Method this[]get Not allowed in KeyValuesPartition");
            set => throw new NotImplementedException("Method this[]set Not allowed in KeyValuesPartition");
        }

        public int IndexOf(Tval item)
        {
            throw new NotImplementedException("Method IndexOf Not allowed in KeyValuesPartition");
        }

        public void Insert(int index, Tval item)
        {
            throw new NotImplementedException("Method Insert Not allowed in KeyValuesPartition");
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException("Method RemoveAt Not allowed in KeyValuesPartition");
        }

        public void Add(Tval item)
        {
            if (item == null) return;
            var keyValue = new KeyValue(subKey, item);
            originalArray.Add(keyValue);
        }

        public void Clear()
        {
            GetPartitionList().ForEach(kv => Remove(kv));
        }

        public bool Contains(Tval item)
        {
            return GetOriginalKeyvalueOfData(item) != null;
        }

        public void CopyTo(Tval[] array, int arrayIndex)
        {
            GetPartitionList().CopyTo(array, arrayIndex);
        }

        public bool Remove(Tval item)
        {
            var indexof = GetOriginalKeyvalueOfData(item);
            if (indexof == null) return false;
            return originalArray.Remove(indexof);
        }


        public IEnumerator<Tval> GetEnumerator()
        {
            return GetPartitionList().GetEnumerator();
            //throw new NotImplementedException();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            //return this.originalArray.GetDatas(this.subKey).GetEnumerator();
            return GetPartitionList().GetEnumerator();
            //throw new NotImplementedException();
        }


        public List<KeyValue> GetDatas(string key, bool StartWith = false)
        {
            return originalArray.GetDatas(KeyValueTools.ConcatKey(subKey, key), StartWith);
        }


        public KeyValue GetOriginalKeyvalueOfData(KeyValues item)
        {
            if (item == null) return null;
            foreach (var de in originalArray)
            {
                if (de == null || de.Value == null) continue;
                var values = de.Value as KeyValues;
                if (values == null) continue;
                if (item.GetInnerDatas() == values.GetInnerDatas()) return de;
            }

            return null;
        }


        public List<Tval> GetPartitionList()
        {
            var kvret = originalArray.GetDatas(subKey);
            var retour = new List<Tval>();
            foreach (var de in kvret)
            {
                if (de == null || de.Value == null) continue; // ne peus pas être null
                if (de.Value is Tval)
                {
                    retour.Add(de.Value as Tval); // La c'est simple
                }
                else if (de.Value is KeyValues)
                {
                    // Il surcaster le nouvel objet
                    var tvl = de.Value as KeyValues;
                    var nval = new Tval();
                    nval.SetInnerDatas(tvl
                        .GetInnerDatas()); // il est important de bien utiliser les memes objet sans nouvelle liste
                    retour.Add(nval);
                }
            }

            return retour;
        }
    }
}