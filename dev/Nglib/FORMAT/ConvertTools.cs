using System;
using System.Collections.Generic;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Enhanced converter supporting additional conversion possibilities.
    /// Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_format"/>
    /// </summary>
    public static class ConvertTools
    {
        private static readonly Dictionary<string, Type> TypeMappings = new Dictionary<string, Type>
        {
            ["string"] = typeof(string),
            ["int"] = typeof(int),
            ["numeric"] = typeof(int),
            ["datetime"] = typeof(DateTime),
            ["date"] = typeof(DateTime),
            ["double"] = typeof(double),
            ["bool"] = typeof(bool),
            ["long"] = typeof(long),
            ["char"] = typeof(char),
            ["byte"] = typeof(byte),
            ["decimal"] = typeof(decimal)
        };

        /// <summary>
        /// Object to Bool, supports extended string values like "on", "yes", "1"
        /// </summary>
        public static bool ToBoolean(object value, bool safe = false)
        {
            try
            {
                if (value == null) return false;
                if (value is bool) return (bool)value;
                if (value is int)
                {
                    if ((int)value > 0) return true;
                    return false;
                }
                var ret = value.ToString().ToLower();
                if (string.IsNullOrWhiteSpace(ret)) return false;
                if (ret == "true") return true;
                if (ret == "false") return false;
                if (ret == "on") return true;
                if (ret == "yes") return true;
                if (ret == "1") return true;
                return false;
            }
            catch (Exception ex)
            {
                if (safe) return false;
                throw new Exception("ToBoolean " + ex.Message);
            }
        }

        /// <summary>
        /// Converts string to DateTime with support for 8-character format (yyyyMMdd)
        /// </summary>
        public static DateTime ToDateTime(object value, bool safe = false)
        {
            try
            {
                if (value is string)
                {
                    var objstr = ((string)value).Trim();
                    if (objstr.Length == 8 && !objstr.Contains(" ") && !objstr.Contains("/") &&
                        !objstr.Contains("-")) // CONVERTION depuis yyyyMMdd
                        return DateTools.ConvertDateTime8(objstr);
                }
                return Convert.ToDateTime(value);
            }
            catch (Exception ex)
            {
                if (safe) return DateTime.MinValue;
                throw new Exception("ToDateTime " + ex.Message);
            }
        }

        /// <summary>
        /// Converts object to integer with default value and boolean support
        /// </summary>
        public static int ToInt(object value, int? defaultValue = 0, bool safe = false)
        {
            try
            {
                if ((value == null || value == DBNull.Value) && defaultValue.HasValue) return defaultValue.Value;

                if (value is bool)
                    if ((bool)value) return 1;
                    else return 0;
                if (value is string)
                {
                    var ret = value.ToString().ToLower().Trim();
                    if (string.IsNullOrWhiteSpace(ret) && defaultValue.HasValue) return defaultValue.Value;
                    if (ret == "true") return 1;
                    if (ret == "false") return 0;
                }
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                if (safe) return defaultValue ?? 0;
                throw new Exception("ToInt " + ex.Message);
            }
        }

        /// <summary>
        /// Changes object type to another using enhanced Nglib.FORMAT conversion
        /// </summary>
        public static object ChangeType(object value, Type type)
        {
            if(type==null) throw new ArgumentNullException(nameof(type));
            if (value == DBNull.Value) value = null; 
            if (value == null && Nullable.GetUnderlyingType(type) != null)
                return null; //Ce champ admet des null

            if (type.Equals(typeof(int)))       return ToInt(value);
            if (type.Equals(typeof(bool)))      return ToBoolean(value);
            if (type.Equals(typeof(DateTime)))  return ToDateTime(value);
            return Convert.ChangeType(value, type); // sinon on utilise le changetype normal
        }

        /// <summary>
        /// Changes object type to another using enhanced Nglib.FORMAT conversion
        /// </summary>
        public static object ChangeType(object value, string typeName)
            => ChangeType(value, ParseType(typeName));
 
        /// <summary>
        /// Parses string type name to Type object using dictionary lookup
        /// </summary>
        public static Type ParseType(string typeName)
        {
            if(string.IsNullOrWhiteSpace(typeName)) return null;
            typeName = typeName.Trim().ToLower();
            TypeMappings.TryGetValue(typeName, out Type type);
            return type;
        }
    }
}