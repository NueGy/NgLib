using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValue : IValue
    {

        public string Key { get; protected set; }

        public object Value { get; set; }

        private bool isChangedValue { get; set; }

        /// <summary>
        /// Plusieurs valeurs avec cette clef.
        /// Il s'agit d'une valeur à exporter en temps que array
        /// </summary>
        public bool IsMultiples { get; set; }

        public KeyValue(string key, object value) 
        {
            this.Key = key;
            this.Value = value;
        }

        public void AcceptChanges()
        {
            this.isChangedValue = false;
        }
        public bool IsChanges()
        {
            return this.isChangedValue;
        }

        public object GetData()
        {
            return Value;
        }

        public bool SetData(object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (!AccesOptions.HasFlag(DataAccessorOptionEnum.IgnoreChange))
                this.isChangedValue = true;
            this.Value = obj;
            return true;
        }
        public KeyValuePair<string,object> ToKeyValuePair()
        {
            return new KeyValuePair<string, object>(this.Key,this.Value);
        }

    }
}
