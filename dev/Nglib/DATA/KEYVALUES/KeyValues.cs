using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.KEYVALUES
{
    [Obsolete("BETA")]
    public class KeyValues : IkeyValues
    {
        private List<KeyValue> datas;

        public KeyValues()
        {
            datas = new List<KeyValue>();
        }

        //public KeyValues(List<KeyValue> dataslist) 
        //{ 
        //    this.datas = dataslist;
        //    if (this.datas == null) throw new ArgumentException("dataslist");
        //}

        public KeyValues(KeyValues keyValues)
        {
            datas = keyValues.datas;
        }

        private bool isChangedValueList { get; set; } // la list a été modifié (add ou del)

        public object this[string firstkeyname]
        {
            get => this.GetObject(firstkeyname);
            set => this.SetObject(firstkeyname, value);
        }


        protected internal IDataAccessorCryptoContext CryptoContext { get; set; }


        public KeyValue this[int index]
        {
            get => datas[index];
            set => datas[index] = value;
        }


        // ---- IdataAccessor Elements

        /// <summary>
        ///     obtenir le context de cryptage de l'objet
        /// </summary>
        public virtual IDataAccessorCryptoContext GetCryptoContext()
        {
            return CryptoContext;
        }

        public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            if (string.IsNullOrWhiteSpace(nameValue)) return null;
            nameValue = nameValue.Trim().Trim('/'); // clean


            if (nameValue.Contains("/")) // il faut descendre dans les sous keyvalues
            {
                var nameValueRootPart = nameValue.Split('/')[0];
                var inval = datas.FirstOrDefault(v =>
                    nameValueRootPart.Equals(v.Key, StringComparison.OrdinalIgnoreCase));
                if (inval != null && inval.GetData() is KeyValues)
                {
                    var nameValueSubpart = nameValue.Substring(nameValueRootPart.Length);
                    var subkv = inval.GetData() as KeyValues;
                    return subkv.GetData(nameValueSubpart, AccesOptions);
                }

                return null;
            } // recherche simple

            KeyValue val = null;
            val = datas.FirstOrDefault(v => nameValue.Equals(v.Key, StringComparison.OrdinalIgnoreCase));
            if (val == null) return null;
            return val.GetData();
        }

        public string[] ListFieldsKeys()
        {
            return datas.Select(b => b.Key).ToArray();
        }

        public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (string.IsNullOrWhiteSpace(nameValue)) return false;
            var val = datas.FirstOrDefault(v => nameValue.Equals(v.Key, StringComparison.OrdinalIgnoreCase));
            if (val == null)
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.NotCreateColumn)) return false;
                val = new KeyValue(nameValue, obj);
                datas.Add(val);
            }

            return val.SetData(obj, AccesOptions);
        }


        public List<KeyValue> GetDatas(string key, bool StartWith = false)
        {
            if (string.IsNullOrWhiteSpace(key)) return datas;
            if (StartWith) return datas.Where(d => d.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase)).ToList();
            var datasr = datas.Where(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).ToList();
            return datasr;
        }


        /// <summary>
        ///     définir le context de cryptage de l'objet
        /// </summary>
        public void SetCryptoOptions(IDataAccessorCryptoContext dataPOCryptoContext)
        {
            CryptoContext = dataPOCryptoContext;
        }


        public bool DelData(string nameValue)
        {
            var datafind = datas.Where(v => nameValue.Equals(v.Key, StringComparison.OrdinalIgnoreCase));
            datafind.ToList().ForEach(d => datas.Remove(d));
            return datafind.Count() > 0 ? true : false;
        }


        public bool AcceptChanges()
        {
            datas.ForEach(data => data.AcceptChanges());
            isChangedValueList = false;
            return true;
        }

        public bool IsChanges()
        {
            if (isChangedValueList) return true;
            return datas.Any(data => data.IsChanges());
        }


        //!!!todo revoir pour Dictionary<string,Keyvalue> GroupValuesByKeys
        public Dictionary<string, object> GroupKeys()
        {
            var retour = new Dictionary<string, object>();
            var listKeys = ListFieldsKeys().Distinct().ToList();
            if (listKeys.Count > 1 && listKeys.Contains(""))
                throw new Exception("Impossible de mélanger array/Object key(empty)");
            listKeys.ForEach(key =>
            {
                var keysvaluesfromKey = datas.Where(d => key == d.Key).ToList();
                var allobjs = keysvaluesfromKey.Select(d => d.Value).ToArray();
                if (allobjs.Length == 1 && !keysvaluesfromKey[0].IsMultiples)
                    retour.Add(key, allobjs[0]); // si une seul valeur on stoke que la premiere valeur
                else
                    retour.Add(key, allobjs); // sinon ce sera un array
            });
            return retour;
        }


        /// <summary>
        ///     Retourne un dictionary d'objets (qui peut en contenir d'autre)
        /// </summary>
        /// <param name="KeepFirstKeyOnly"></param>
        /// <returns></returns>
        public Dictionary<string, object> ToFullDictionary()
        {
            var retour = new Dictionary<string, object>();
            //if (KeepFirstKeyOnly) // c'est simple on prend juste le premier
            //{
            //    this.datas.ForEach(data =>
            //    {
            //        if (!retour.ContainsKey(data.Key))
            //        {
            //            object[] ret= TransformToObject(data.Value);
            //            if(ret==null ) retour.Add(data.Key,null );
            //            else if( ret.Length == 1) retour.Add(data.Key, ret[0]);
            //            else retour.Add(data.Key, ret);
            //        }
            //    });
            //    return retour;
            //}

            var GroupKeysdic = GroupKeys();
            GroupKeysdic.Keys.ToList().ForEach(k =>
            {
                var ret = TransformToObject(GroupKeysdic[k]);
                if (ret == null) retour.Add(k, null);
                else retour.Add(k, ret);
            });
            return retour;
        }

        private static object TransformToObject(object obj)
        {
            if (obj == null || obj == DBNull.Value) return null;

            if (obj is KeyValue)
            {
                var objc = obj as KeyValue;
                var ojb2 = TransformToObject(objc.Value);
                if (objc.IsMultiples) return new[] { ojb2 }; // Il s'agit d'une valeur multiple, on force un array
                return ojb2;
            }

            if (obj is KeyValue[]) return (obj as KeyValue[]).Select(kv => kv.Value).ToArray();

            if (obj is KeyValues)
            {
                object nobj = (obj as KeyValues).ToFullDictionary();
                return nobj;
            }

            if (obj is object[] && (obj as object[]).Any(objin => objin is KeyValues))
            {
                var keyValuesT = (obj as object[]).Select(objin => objin as KeyValues).Where(kv => kv != null)
                    .ToArray();
                return keyValuesT.Select(kv => kv.ToFullDictionary()).ToArray();
            }

            return obj;
        }


        public List<T> GetSubKeyValues<T>(string key, bool StartWith = false) where T : KeyValues, new()
        {
            var datas = GetDatas(key);
            if (datas == null) return null;
            List<T> retour = null;
            retour = datas.Where(d => d.Value != null && d.Value is KeyValues).Select(d =>
            {
                var nd = new T();
                nd.AddRange(d.Value as KeyValues);
                return nd;
            }).ToList();
            return retour;
        }

        public List<object> GetObjects(string key)
        {
            var vdatas = GetDatas(key);
            var retour = vdatas.SelectMany(d =>
            {
                var vr = TransformToObject(d);
                if (vr is object[]) return vr as object[];
                return new[] { vr };
            }).ToList();
            return retour;
        }

        public List<string> GetStrings(string key)
        {
            var vdatas = GetObjects(key);
            return vdatas.Select(d => Convert.ToString(d)).ToList();
        }


        #region List Elements

        public int Count => datas.Count;

        public bool IsReadOnly => false;

        public void ForEach(Action<KeyValue> action)
        {
            datas.ForEach(action);
        }


        public void Add(KeyValue item)
        {
            datas.Add(item);
            isChangedValueList = true;
        }

        public void Add(string key, object value)
        {
            var item = new KeyValue(key, value);
            Add(item);
            isChangedValueList = true;
        }

        public List<KeyValue> GetList()
        {
            return datas.ToList();
        }

        public void AddRange(IEnumerable<KeyValue> items)
        {
            datas.AddRange(items.Where(item => item != null));
            isChangedValueList = true;
        }

        public void Clear()
        {
            datas.Clear();
            isChangedValueList = true;
        }

        public bool Contains(KeyValue item)
        {
            return datas.Contains(item);
        }

        public void CopyTo(KeyValue[] array, int arrayIndex)
        {
            datas.CopyTo(array, arrayIndex);
        }


        public IEnumerator<KeyValue> GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        public int IndexOf(KeyValue item)
        {
            return datas.IndexOf(item);
        }

        public void Insert(int index, KeyValue item)
        {
            datas.Insert(index, item);
            isChangedValueList = true;
        }


        public bool Remove(KeyValue item)
        {
            var isok = datas.Remove(item);
            if (isok) isChangedValueList = true;
            return isok;
        }

        public void RemoveAt(int index)
        {
            datas.RemoveAt(index);
            isChangedValueList = true;
        }


        public void SetInnerDatas(List<KeyValue> datasforreplace)
        {
            datas = datasforreplace;
        }

        public List<KeyValue> GetInnerDatas()
        {
            return datas;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        #endregion
    }
}