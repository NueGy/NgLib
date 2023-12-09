using System.Collections.Generic;
using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValue : IValue
    {
        public KeyValue(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; protected set; }

        public object Value { get; set; }

        private bool isChangedValue { get; set; }

        /// <summary>
        ///     Plusieurs valeurs avec cette clef.
        ///     Il s'agit d'une valeur à exporter en temps que array
        /// </summary>
        public bool IsMultiples { get; set; }

        public void AcceptChanges()
        {
            isChangedValue = false;
        }

        public bool IsChanges()
        {
            return isChangedValue;
        }

        public object GetData()
        {
            return Value;
        }

        public bool SetData(object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (!AccesOptions.HasFlag(DataAccessorOptionEnum.IgnoreChange))
                isChangedValue = true;
            Value = obj;
            return true;
        }

        public KeyValuePair<string, object> ToKeyValuePair()
        {
            return new KeyValuePair<string, object>(Key, Value);
        }

        public override string ToString()
        {
            return $"{Key}={Value}";
        }
    }
}