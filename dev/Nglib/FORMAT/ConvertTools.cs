using System;
using System.Collections.Generic;
using System.Globalization;

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
                var ret = value.ToString().Trim();
                if (string.IsNullOrWhiteSpace(ret)) return false;
                if (string.Equals(ret, "true",  StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(ret, "false", StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(ret, "on",    StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(ret, "yes",   StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(ret, "oui",   StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(ret, "no",    StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(ret, "non",   StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(ret, "off",   StringComparison.OrdinalIgnoreCase)) return false;
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
        /// Converts object to double, handles formatted strings like "1 234,56" or "1.234,56"
        /// </summary>
        public static double ToDouble(object value, double? defaultValue = null, bool safe = false)
        {
            try
            {
                if (value == null || value == DBNull.Value) return defaultValue ?? 0d;
                if (value is double d) return d;
                if (value is bool b) return b ? 1d : 0d;
                if (value is string)
                {
                    var ret = NumberTools.NormalizeNumericString(value.ToString());
                    if (string.IsNullOrWhiteSpace(ret)) return defaultValue ?? 0d;
                    return double.Parse(ret, CultureInfo.InvariantCulture);
                }
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                if (safe) return defaultValue ?? 0d;
                throw new Exception("ToDouble " + ex.Message);
            }
        }

        /// <summary>
        /// Converts object to decimal, handles formatted strings like "1 234,56" or "1.234,56"
        /// </summary>
        public static decimal ToDecimal(object value, decimal? defaultValue = null, bool safe = false)
        {
            try
            {
                if (value == null || value == DBNull.Value) return defaultValue ?? 0m;
                if (value is decimal dec) return dec;
                if (value is bool b) return b ? 1m : 0m;
                if (value is string)
                {
                    var ret = NumberTools.NormalizeNumericString(value.ToString());
                    if (string.IsNullOrWhiteSpace(ret)) return defaultValue ?? 0m;
                    return decimal.Parse(ret, CultureInfo.InvariantCulture);
                }
                return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                if (safe) return defaultValue ?? 0m;
                throw new Exception("ToDecimal " + ex.Message);
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
                    var ret = value.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(ret) && defaultValue.HasValue) return defaultValue.Value;
                    if (string.Equals(ret, "true",  StringComparison.OrdinalIgnoreCase)) return 1;
                    if (string.Equals(ret, "false", StringComparison.OrdinalIgnoreCase)) return 0;
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
        /// Converts object to long (Int64), handles formatted strings like "1 234" or "1.234"
        /// </summary>
        public static long ToLong(object value, long? defaultValue = null, bool safe = false)
        {
            try
            {
                if (value == null || value == DBNull.Value) return defaultValue ?? 0L;
                if (value is long l) return l;
                if (value is bool b) return b ? 1L : 0L;
                if (value is string)
                {
                    var ret = NumberTools.NormalizeNumericString(value.ToString());
                    if (string.IsNullOrWhiteSpace(ret)) return defaultValue ?? 0L;
                    if (string.Equals(ret, "true",  StringComparison.OrdinalIgnoreCase)) return 1L;
                    if (string.Equals(ret, "false", StringComparison.OrdinalIgnoreCase)) return 0L;
                    return (long)double.Parse(ret, CultureInfo.InvariantCulture);
                }
                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                if (safe) return defaultValue ?? 0L;
                throw new Exception("ToLong " + ex.Message);
            }
        }

        /// <summary>
        /// Changes object type to another using enhanced Nglib.FORMAT conversion
        /// </summary>
        public static object ChangeType(object value, Type type)
            => ChangeType(value, type, CultureInfo.CurrentCulture);

        /// <summary>
        /// Changes object type to another using enhanced Nglib.FORMAT conversion, with explicit culture support
        /// </summary>
        public static object ChangeType(object value, Type type, CultureInfo culture)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (value == DBNull.Value) value = null;
            if (value == null && Nullable.GetUnderlyingType(type) != null)
                return null;
            var valueType = value?.GetType();
            if (valueType == type) return value; // Already the correct type
            if (type == typeof(string) && valueType?.IsArray == true)
                return System.Text.Json.JsonSerializer.Serialize(value); // Array → JSON string
            if (type.IsEnum)
                return value is string s ? Enum.Parse(type, s, ignoreCase: true) : Enum.ToObject(type, value);

            if (type.Equals(typeof(int)))      return ToInt(value);
            if (type.Equals(typeof(bool)))     return ToBoolean(value);
            if (type.Equals(typeof(DateTime))) return ToDateTime(value);
            if (type.Equals(typeof(double)))   return ToDouble(value);
            if (type.Equals(typeof(decimal)))  return ToDecimal(value);
            return Convert.ChangeType(value, type, culture);
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