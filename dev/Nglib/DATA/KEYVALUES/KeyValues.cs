using Nglib.DATA.ACCESSORS;
using Nglib.SECURITY.CRYPTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValues : IList<KeyValue>, Nglib.DATA.ACCESSORS.IDataAccessor
    {
        private List<KeyValue> datas;
        private bool isChangedValueList { get; set; } // la list a été modifié (add ou del)


        public KeyValue this[int index] { get => this.datas[index]; set => this.datas[index]=value; }
        public object this[string firstkeyname] { get => this.GetObject(firstkeyname); set => this.SetObject(firstkeyname,value); }



        protected internal DATA.ACCESSORS.IDataAccessorCryptoContext CryptoContext { get; set; }

        public KeyValues() { datas = new List<KeyValue>(); }

        //public KeyValues(List<KeyValue> dataslist) 
        //{ 
        //    this.datas = dataslist;
        //    if (this.datas == null) throw new ArgumentException("dataslist");
        //}

        public KeyValues(KeyValues keyValues)
        {
            this.datas = keyValues.datas;
 
        }


        // ---- IdataAccessor Elements

        /// <summary>
        /// obtenir le context de cryptage de l'objet
        /// </summary>
        public virtual IDataAccessorCryptoContext GetCryptoContext()
        {
            return this.CryptoContext;
        }



        /// <summary>
        /// définir le context de cryptage de l'objet
        /// </summary>
        public void SetCryptoOptions(IDataAccessorCryptoContext dataPOCryptoContext)
        {
            this.CryptoContext = dataPOCryptoContext;
        }

        public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            if (string.IsNullOrWhiteSpace(nameValue)) return null;
            KeyValue val = this.datas.FirstOrDefault(v => nameValue.Equals(v.Key, StringComparison.OrdinalIgnoreCase));
            if (val==null) return null;
            return val.GetData();
        }

        public string[] ListFieldsKeys()
        {
            return this.datas.Select(b => b.Key).ToArray();
        }
        public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (string.IsNullOrWhiteSpace(nameValue)) return false;
            KeyValue val = this.datas.FirstOrDefault(v => nameValue.Equals(v.Key, StringComparison.OrdinalIgnoreCase));
            if (val == null)
            { 
                if(AccesOptions.HasFlag(DataAccessorOptionEnum.NotCreateColumn)) return false;
                val = new KeyValue(nameValue, obj);
                this.datas.Add(val);
            } 
            return val.SetData(obj, AccesOptions);
        }


        public bool DelData(string nameValue)
        {
            var datafind = this.datas.Where(v => nameValue.Equals(v.Key, StringComparison.OrdinalIgnoreCase));
            datafind.ToList().ForEach(d => this.datas.Remove(d));
            return datafind.Count() > 0 ? true : false;
        }


        public bool AcceptChanges()
        {
            this.datas.ForEach(data => data.AcceptChanges());
            this.isChangedValueList = false;
            return true;
        }
        public bool IsChanges()
        {
            if (this.isChangedValueList) return true;
           return this.datas.Any(data => data.IsChanges());
        }


        public Dictionary<string, object> GroupKeys()
        {
            Dictionary<string, object> retour = new Dictionary<string, object>();
            var listKeys = this.ListFieldsKeys().Distinct().ToList();
            listKeys.ForEach(key =>
            {
                var keysvaluesfromKey = this.datas.Where(d => key.Equals(d.Key)).ToList();
                object[] allobjs = keysvaluesfromKey.Select(d => d.Value).ToArray();
                if (allobjs.Length == 1 && !keysvaluesfromKey[0].IsMultiples)
                    retour.Add(key, allobjs[0]);       // si une seul valeur on stoke que la premiere valeur
                else
                    retour.Add(key, allobjs); // sinon ce sera un array
            });
            return retour;
        }



        /// <summary>
        /// Retourne un dictionary d'objets (qui peut en contenir d'autre)
        /// </summary>
        /// <param name="KeepFirstKeyOnly"></param>
        /// <returns></returns>
        public Dictionary<string, object> ToFullDictionary()
        {
            Dictionary<string, object> retour = new Dictionary<string, object>();
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

            var GroupKeysdic = this.GroupKeys();
            GroupKeysdic.Keys.ToList().ForEach(k =>
            {
                object ret = TransformToObject(GroupKeysdic[k]);
                if (ret == null) retour.Add(k, null);
                else retour.Add(k, ret);
            });
            return retour;
        }

        private static object TransformToObject(object obj)
        {
            if (obj == null || obj== DBNull.Value)
                return null;
            else if (obj is KeyValue)
            {
                KeyValue objc = (obj as KeyValue);
                object ojb2 = TransformToObject(objc.Value);
                if (objc.IsMultiples) return new object[] { ojb2 };  // Il s'agit d'une valeur multiple, on force un array
                else return ojb2;
            }
            else if (obj is KeyValue[])
            {
                return (obj as KeyValue[]).Select(kv => kv.Value).ToArray();
            }
            else if (obj is KeyValues)
            {
                object nobj = (obj as KeyValues).ToFullDictionary();
                return nobj ;
            }
            else if (obj is object[] && (obj as object[]).Any(objin => objin is KeyValues))
            {
                KeyValues[] keyValuesT = (obj as object[]).Select(objin => objin as KeyValues).Where(kv=> kv !=null).ToArray();
                return keyValuesT.Select(kv=>kv.ToFullDictionary()).ToArray();
            }
            else
            {
                return  obj ;
            }
        }



        public List<KeyValue> GetDatas(string key, bool StartWith=false)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if(StartWith) return this.datas.Where(d => d.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase)).ToList();
            return this.datas.Where(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<object> GetObjects(string key)
        {
            List<KeyValue> vdatas = GetDatas(key);
            List<object> retour = vdatas.SelectMany(d => { object vr = TransformToObject(d); if (vr is object[]) return vr as object[]; else return new object[] { vr };  }).ToList();
            return retour;
        }

        public List<string> GetStrings(string key)
        {
            List<object> vdatas = GetObjects(key);
            return vdatas.Select(d => Convert.ToString(d)).ToList();
        }


        #region List Elements


        public int Count => this.datas.Count;

        public bool IsReadOnly => false;

        public void ForEach(Action<KeyValue> action) { this.datas.ForEach(action); }


        public void Add(KeyValue item)
        {
            this.datas.Add(item);
            this.isChangedValueList = true;
        }
        public void Add(string key ,object value)
        {
            KeyValue item = new KeyValue(key, value);
            this.Add(item);
            this.isChangedValueList = true;
        }


        public void AddRange(IEnumerable<KeyValue> items)
        {
            this.datas.AddRange(items.Where(item=> item!=null));
            this.isChangedValueList = true;
        }

        public void Clear()
        {
            this.datas.Clear();
            this.isChangedValueList = true;
        }

        public bool Contains(KeyValue item)
        {
            return this.datas.Contains(item);
        }

        public void CopyTo(KeyValue[] array, int arrayIndex)
        {
            this.datas.CopyTo(array, arrayIndex);
        }

 

        public IEnumerator<KeyValue> GetEnumerator()
        {
            return this.datas.GetEnumerator();
        }

        public int IndexOf(KeyValue item)
        {
            return this.datas.IndexOf(item);
        }

        public void Insert(int index, KeyValue item)
        {
            this.datas.Insert(index,item);
            this.isChangedValueList = true;
        }



        public bool Remove(KeyValue item)
        {
            bool isok = this.datas.Remove(item);
            if(isok) this.isChangedValueList = true;
            return isok;
        }

        public void RemoveAt(int index)
        {
            this.datas.RemoveAt(index);
            this.isChangedValueList = true;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.datas.GetEnumerator();
        }


        #endregion

    }
}
