using Nglib.DATA.ACCESSORS;
using Nglib.FORMAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Nglib.DATA.COLLECTIONS
{
    /// <summary>
    /// Dictionary with string keys and object values, featuring typed accessors and utility methods.
    /// Supports case-insensitive operations for dynamic data manipulation.
    /// Includes validation of allowed characters in keys.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_collections"/></para>
    /// </summary>
    public class DictionaryData : Dictionary<string, object>, IDataAccessor //, IXmlSerializable
    {

        /// <summary>
        /// Initializes a new empty instance of DictionaryData with case-insensitive keys.
        /// </summary>
        public DictionaryData() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance from a string/string dictionary.
        /// </summary>
        /// <param name="dic">Source dictionary to convert</param>
        public DictionaryData(IDictionary<string, string> dic) : base(dic.ToDictionary(d => d.Key, d => (object)d.Value), StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance from a string/object dictionary.
        /// </summary>
        /// <param name="dic">Source dictionary</param>
        public DictionaryData(IDictionary<string, object> dic) : base(dic, StringComparer.OrdinalIgnoreCase)
        {
        }

 

        /// <summary>
        /// Gets a value as a string.
        /// </summary>
        /// <param name="fieldName">Field name to retrieve</param>
        /// <returns>Value converted to string</returns>
        public string GetString(string fieldName) => DataAccessorExtensions.GetString(this, fieldName);


        /// <summary>
        /// Updates two related fields xxxMin and xxxMax simultaneously.
        /// Useful for defining value ranges (dates, amounts, etc.).
        /// </summary>
        /// <param name="fieldName">Base field name (without Min/Max suffix)</param>
        /// <param name="valMin">Minimum value to set</param>
        /// <param name="valMax">Maximum value to set</param>
        /// <returns>True if both values were set successfully</returns>
        public bool SetValueMinMax(string fieldName, object valMin, object valMax)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) return false;
            bool retour = true;
            retour = this.SetValue(fieldName + "Min", valMin);
            retour = this.SetValue(fieldName + "Max", valMax);
            return retour;
        }



        /// <summary>
        /// Checks if a key exists in the dictionary (case-insensitive).
        /// Since the dictionary uses StringComparer.OrdinalIgnoreCase, this is automatically case-insensitive.
        /// </summary>
        /// <param name="name">Key name to search for</param>
        /// <returns>True if the key exists, False otherwise</returns>
        public new bool ContainsKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return base.ContainsKey(name.Trim());
        }

        /// <summary>
        /// Removes a key from the dictionary (case-insensitive).
        /// Since the dictionary uses StringComparer.OrdinalIgnoreCase, this is automatically case-insensitive.
        /// </summary>
        /// <param name="name">Key name to remove</param>
        /// <returns>True if the key was found and removed, False otherwise</returns>
        public new bool Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return base.Remove(name.Trim());
        }


        /// <summary>
        /// Checks if the value is null or if the string is empty.
        /// </summary>
        /// <param name="name">Key name to check</param>
        /// <returns>True if empty or not found</returns>
        public bool IsEmpty(string name) 
            => string.IsNullOrEmpty(name) || !this.Any(p => p.Key.Equals(name, StringComparison.OrdinalIgnoreCase)) || string.IsNullOrEmpty(this.FirstOrDefault(p => p.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value?.ToString());

        /// <summary>
        /// Retrieves a value from the dictionary in case-insensitive mode.
        /// Implementation of IDataAccessor interface.
        /// </summary>
        /// <param name="nameValue">Field name to retrieve</param>
        /// <param name="AccesOptions">Data access options</param>
        /// <returns>Value found or null if not found</returns>
        public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            //=> Nglib.DATA.COLLECTIONS.CollectionsTools.GetValue<object>(this, nameValue);
            if (string.IsNullOrEmpty(nameValue)) return null;
            var val = this.Where(d => nameValue.Equals(d.Key, StringComparison.OrdinalIgnoreCase)).Select(d => d.Value).FirstOrDefault();
            return val;
        }

        /// <summary>
        /// Sets a value in the dictionary.
        /// Implementation of IDataAccessor interface.
        /// </summary>
        /// <param name="nameValue">Field name to set</param>
        /// <param name="obj">Value to associate with the field</param>
        /// <param name="AccesOptions">Data access options</param>
        /// <returns>True if the value was set successfully</returns>
        public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (string.IsNullOrEmpty(nameValue)) return false;
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.NotReplace) && this.ContainsKey(nameValue))
                return false; // Don't replace if the key already exists
            
            
            Nglib.DATA.COLLECTIONS.CollectionsTools.AddOrReplace(this, nameValue, obj, true);
            return true;
        }
         
        /// <summary>
        /// Returns the list of all dictionary keys.
        /// </summary>
        /// <returns>Array containing all keys</returns>
        public string[] ListFieldsKeys() => Keys.ToArray();

        /// <summary>
        /// Returns the cryptography context (not implemented).
        /// </summary>
        /// <returns>Null - no cryptography context</returns>
        public IDataAccessorCryptoContext GetCryptoContext() => null;





        /// <summary>
        /// Removes all non-standard keys from the dictionary.
        /// Only alphanumeric characters + '-', '_', '.' are allowed.
        /// </summary>
        /// <returns>True if any keys were removed, False otherwise</returns>
        public bool RemoveNotAllowedKeys()
        {
            var keysToRemove = new List<string>();
            foreach (var key in this.Keys)
            {
                if (!KeyTools.IsValidKey(key))
                    keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
                this.Remove(key);
            return keysToRemove.Count > 0;
        }




        /// <summary>
        /// Clones a DictionaryData with all its values.
        /// </summary>
        /// <returns>New instance with the same data</returns>
        public DictionaryData Clone()
        {
            var clone = new DictionaryData();
            foreach (var kvp in this)
            {
                clone[kvp.Key] = kvp.Value;
            }
            return clone;
        }


    }
}


