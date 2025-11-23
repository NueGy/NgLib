using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;
using Nglib.APP.CODE;
using Nglib.FORMAT;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Extension methods for easy typed data access and automatic type conversion.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_accessors"/></para>
    /// </summary>
    public static class DataAccessorExtensions
    {


        #region ------  PRINCIPAUX  ------


        /// <summary>
        /// Gets generic typed value with automatic conversion and custom default value.
        /// Optimized for performance with type caching and minimal reflection.
        /// </summary>
        /// <typeparam name="T">Target type for conversion</typeparam>
        /// <param name="accessor">Data accessor instance</param>
        /// <param name="name">Field name to retrieve</param>
        /// <param name="defaultValue">Value to return if null or conversion fails</param>
        /// <param name="option">Access options for behavior control</param>
        /// <returns>Converted value of type T or specified default value</returns>
        public static T GetValue<T>(this IDataAccessor accessor, string name, T defaultValue, DataAccessorOptionEnum option = 0)
        {
            try
            {
                // Fast path: null check
                if (option.HasFlag(DataAccessorOptionEnum.Safe) && accessor == null)
                    return defaultValue;

                // Retrieve raw data
                var obj = accessor.GetData(name, option);

                // Fast path: null or DBNull
                if (obj == null || obj is DBNull)
                {
                    // Flag Required - validation stricte
                    if (option.HasFlag(DataAccessorOptionEnum.Required))
                        throw new DataAccessorException("Required field is null or DBNull");
                    
                    return defaultValue;
                }

                // Décryptage si nécessaire
                if(option.HasFlag(DataAccessorOptionEnum.Encrypted))
                    obj = DataAccessorTools.ApplyDecryption(accessor, name, obj, option);

                // Validation Required après décryptage (pour valeurs vides)
                if (option.HasFlag(DataAccessorOptionEnum.Required))
                    DataAccessorTools.ValidateRequiredValue(obj);

                // Fast path: direct type match
                if (obj is T directValue)
                    return directValue;

                // Type analysis
                var targetType = typeof(T);
                var objType = obj.GetType();
                var culture = DataAccessorTools.GetCulture(option);

                // Tentative de conversion rapide avec les fast paths
                if (DataAccessorTools.TryFastPathConversion<T>(obj, targetType, objType, culture, option, out var result))
                    return result;

                // Gestion des types nullables
                if (DataAccessorTools.TryNullableConversion<T>(obj, ref targetType, objType, out var nullableResult))
                    return nullableResult;

                // Assignable types (inheritance/interface)
                if (targetType.IsAssignableFrom(objType))
                    return (T)obj;

                // Conversions complexes (TimeSpan, Array→List, JSON)
                if (DataAccessorTools.TryComplexConversion<T>(obj, targetType, objType, culture, option, out var complexResult))
                    return complexResult;

                // Conversion standard finale
                return DataAccessorTools.PerformFinalConversion<T>(obj, targetType, culture, option);
            }
            catch (Exception ex)
            {
                if (!option.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException($"GetValue<{typeof(T).Name}>({name}) " + ex.Message, ex);
                return defaultValue;
            }
        }



        /// <summary>
        /// Gets generic typed value with automatic conversion and behavior control
        /// </summary>
        /// <typeparam name="T">Target type for conversion</typeparam>
        /// <param name="accessor">Data accessor instance</param>
        /// <param name="name">Field name to retrieve</param>
        /// <param name="AccesOptions">Access options for behavior control</param>
        /// <returns>Converted value of type T or default(T) if conversion fails/not found</returns>
        public static T GetValue<T>(this IDataAccessor accessor, string name, DataAccessorOptionEnum AccesOptions)
        {
            return GetValue<T>(accessor, name, default(T), AccesOptions);
        }

        /// <summary>
        /// Gets generic typed value with automatic conversion using safe mode
        /// </summary>
        /// <typeparam name="T">Target type for conversion</typeparam>
        /// <param name="accessor">Data accessor instance</param>
        /// <param name="name">Field name to retrieve</param>
        /// <returns>Converted value of type T or default(T) if conversion fails/not found</returns>
        public static T GetValue<T>(this IDataAccessor accessor, string name)
        {
            return GetValue<T>(accessor, name, default(T), DataAccessorOptionEnum.Safe);
        }

        /// <summary>
        /// IMPROVEMENT #11: Modern and safe TryGetValue pattern.
        /// Attempts to retrieve a typed value without throwing exceptions.
        /// </summary>
        /// <typeparam name="T">Target type for conversion</typeparam>
        /// <param name="accessor">Data accessor instance</param>
        /// <param name="name">Field name to retrieve</param>
        /// <param name="value">Output parameter receiving the converted value</param>
        /// <param name="AccesOptions">Access options (Safe flag automatically added)</param>
        /// <returns>True if value was successfully retrieved and converted, false otherwise</returns>
        public static bool TryGetValue<T>(this IDataAccessor accessor, string name, out T value, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            try
            {
                // Force Safe mode
                AccesOptions |= DataAccessorOptionEnum.Safe;
                
                value = GetValue<T>(accessor, name, default(T), AccesOptions);
                
                // Check if we got a real value (not just the default)
                var obj = accessor?.GetData(name, AccesOptions);
                if (obj == null || obj is DBNull)
                {
                    value = default(T);
                    return false;
                }
                
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }


        /// <summary>
        /// Updates a value with conversion and optional encryption support (SetData overload).
        /// Handles flags: NotReplace, NotCreateColumn, Encrypted, Required.
        /// </summary>
        public static bool SetValue(this IDataAccessor accessor, string name, object value, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                // Fast path: null check
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && accessor == null)
                    return false;

                // Vérifier si la clé existe (pour NotReplace et NotCreateColumn)
                bool keyExists = false;
                try
                {
                    var existingKeys = accessor.ListFieldsKeys();
                    keyExists = existingKeys.Any(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
                }
                catch
                {
                    // Si ListFieldsKeys échoue, on suppose que la clé n'existe pas
                    keyExists = false;
                }

                // Flag NotReplace: Ne pas remplacer si la clé existe déjà
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.NotReplace) && keyExists)
                {
                    if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                        return false; // Retourne false silencieusement
                    return true; // La valeur existe déjà, on considère que c'est OK
                }

                // Flag NotCreateColumn: Ne pas créer si la clé n'existe pas
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.NotCreateColumn) && !keyExists)
                {
                    if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                        return false; // Retourne false silencieusement
                    throw new DataAccessorException($"SetValue({name}): Column does not exist and NotCreateColumn flag is set");
                }

                object obj = value;

                // Flag Required: Valider que la valeur n'est pas vide
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Required))
                    DataAccessorTools.ValidateRequiredValue(obj);

                // Cryptage
                if (obj != null && AccesOptions.HasFlag(DataAccessorOptionEnum.Encrypted))
                {
                    var cryptoctx = accessor.GetCryptoContext();
                    if (cryptoctx != null)
                        obj = cryptoctx.EncryptObjectValue(accessor, name, obj, AccesOptions);
                }

                if (obj == null) obj = DBNull.Value; // les nul sont interdit en base
                return accessor.SetData(name, obj, AccesOptions);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException($"Accessor.SetValue({name}) " + ex.Message, ex);
                return false;
            }
        }



        #endregion



        #region ------  ACCESSEURS SPECIALISES  ------

        /// <summary>
        /// Gets object data with optional decryption support
        /// </summary>
        [Obsolete("Use GetValue<>")]
        public static object GetObject(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions)
            => GetValue<object>(dataAccessor, nameValue, AccesOptions);
 

        /// <summary>
        /// Gets object data with default options (no encryption, no special handling)
        /// </summary>
        public static object GetObject(this IDataAccessor dataAccessor, string nameValue)
        {
            //Obtenir la données la donnée directement : Todo faire un accessor.GetData ?
            var adata = dataAccessor.GetData(nameValue, DataAccessorOptionEnum.None);
            return adata;
        }



        /// <summary>
        /// Sets object data with default options (no encryption, no special handling)
        /// </summary>
        [Obsolete("use SetValue")]
        public static bool SetObject(this IDataAccessor dataAccessor, string nameValue, object obj)
        {
            return dataAccessor.SetValue(nameValue, obj, DataAccessorOptionEnum.None);
        }






        /// <summary>
        /// Gets string value with safe mode (never null, returns empty string on error) DIRECT/SAFE/NotNull
        /// </summary>
        public static string GetString(this IDataAccessor dataAccessor, string nameValue)
        {
            try
            {
                var obj = dataAccessor.GetData(nameValue, DataAccessorOptionEnum.Safe);
                if (obj == null || obj == DBNull.Value) // retourne pas null par default
                    return string.Empty;
                return obj.ToString();
            }
            catch
            {
                return string.Empty;//safe
            }
        }





        /// <summary>
        /// Gets integer value with automatic type conversion - DIRECT/SAFE/NotNull=0
        /// </summary>
        public static int GetInt(this IDataAccessor dataAccessor, string nameValue)
        {
            try
            {
                var obj = dataAccessor.GetData(nameValue, DataAccessorOptionEnum.None);
                if (obj == DBNull.Value || obj == null) return 0;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return 0;
                return Convert.ToInt32(obj);
            }
            catch
            {
                return 0; // SAFE
            }
        }

        /// <summary>
        /// Equivalent to GetValue-long
        /// </summary>
        public static long GetLong(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetValue<long>(dataAccessor, nameValue, 0);
        }

        /// <summary>
        /// Obtenir/Convertir en Double
        /// </summary>
        public static double GetDouble(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetValue<double>(dataAccessor, nameValue, 0);
        }


        /// <summary>
        /// Obtenir/Convertir en DateTime
        /// </summary>
        public static DateTime GetDateTime(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetValue<DateTime>(dataAccessor, nameValue, new DateTime(), DataAccessorOptionEnum.None);
        }





        /// <summary>
        /// Obtenir/Convertir en Bool (Avec Mode : AdvancedConverter)
        /// </summary>
        public static bool GetBoolean(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetValue<bool>(dataAccessor, nameValue, false, DataAccessorOptionEnum.AdvancedConverter);
        }



        #endregion


        #region ------  DICTIONARY  ------

        /// <summary>
        /// Obtenir les données dans un dictionary
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionaryValues(this IDataAccessor dataAccessor,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            var retour = new Dictionary<string, object>();
            foreach (var item in dataAccessor.ListFieldsKeys())
                retour.Add(item, dataAccessor.GetObject(item, AccesOptions));
            return retour;
        }


        /// <summary>
        /// Converts all accessor values to a string-typed dictionary
        /// </summary>
        /// <param name="dataAccessor">Data accessor instance</param>
        /// <param name="AccesOptions">Access options for behavior control</param>
        /// <returns>Dictionary with string keys and string values</returns>
        public static Dictionary<string, string> ToDictionaryString(this IDataAccessor dataAccessor,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            var retour = new Dictionary<string, string>();
            foreach (var item in dataAccessor.ListFieldsKeys())
                retour.Add(item, dataAccessor.GetValue<string>(item, AccesOptions));
            return retour;
        }


        /// <summary>
        /// Populates accessor from dictionary values
        /// </summary>
        /// <param name="dataAccessor">Data accessor instance</param>
        /// <param name="Values">Dictionary with values to import</param>
        /// <param name="AccesOptions">Access options for behavior control</param>
        public static void FromDictionaryValues(this IDataAccessor dataAccessor, Dictionary<string, object> Values,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var options = DataAccessorOptionEnum.None;
                foreach (var itemkey in Values.Keys) dataAccessor.SetValue(itemkey, Values[itemkey], options);
            }
            catch (Exception)
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) return;
                throw;
            }
        }

        #endregion


        #region ------  ENUM  ------

        /// <summary>
        /// Gets enum value with default enum value if null or conversion fails
        /// </summary>
        /// <typeparam name="TEnum">Enum type to convert to</typeparam>
        /// <param name="dataAccessor">Data accessor instance</param>
        /// <param name="fieldname">Field name to retrieve</param>
        /// <param name="AccesOptions">Access options for behavior control</param>
        /// <returns>Enum value or default enum value</returns>
        public static TEnum GetEnum<TEnum>(this IDataAccessor dataAccessor, string fieldname,
            DataAccessorOptionEnum AccesOptions = 0) where TEnum : struct
        {
            var defaultValue = DataAccessorTools.GetEnumDefaultValue<TEnum>();
            return GetValue<TEnum>(dataAccessor, fieldname, defaultValue, AccesOptions);
        }
        //public static TEnum GetEnum<TEnum>(this IDataAccessor dataAccessor, string fieldname, TEnum defaultValue) where TEnum : struct
        //{
        //    DataAccessorOptionEnum AccesOptions = 0
        //    return GetEnum<TEnum>(dataAccessor, fieldname, defaultValue, AccesOptions).Value;
        //}
         


        #endregion



        #region ------  REFLEXION  ------

        /// <summary>
        /// Populates this accessor from another object using reflection
        /// </summary>
        /// <param name="dataAccessor">Data accessor instance to populate</param>
        /// <param name="objetSource">Source object to copy properties from</param>
        /// <param name="replaceifnotnull">Whether to replace existing non-null values</param>
        /// <param name="AccesOptions">Access options for behavior control</param>
        /// <returns>True if operation succeeded</returns>
        public static bool FromReflectionProperties(this IDataAccessor dataAccessor, object objetSource, bool replaceifnotnull = true,
            DataAccessorOptionEnum option = 0)
        {
            try
            {
                var vals = PropertiesTools.GetValues(objetSource);
                vals.Keys.ToList().ForEach(k => dataAccessor.SetObject(k, vals[k]));
                return (vals.Count>0)?true:false;
            }
            catch (Exception ex)
            {
                if (!option.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new Exception($"FromReflectionProperties " + ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Populates target object properties from this accessor using reflection
        /// </summary>
        /// <param name="dataAccessor">Data accessor instance to read from</param>
        /// <param name="objetTarget">Target object to populate</param>
        /// <param name="replaceifnotnull">Whether to replace existing non-null values</param>
        /// <param name="AccesOptions">Access options for behavior control</param>
        /// <returns>True if operation succeeded</returns>
        public static bool ToReflectionProperties(this IDataAccessor dataAccessor, object objetTarget, bool replaceifnotnull = true, DataAccessorOptionEnum option = 0)
        {
            try
            {
                var vals = dataAccessor.ToDictionaryValues(option);
                PropertiesTools.SetValues(objetTarget, vals);
                return false;
            }
            catch (Exception ex)
            {
                if (!option.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new Exception($"FromReflectionProperties " + ex.Message, ex);
                return false;
            }
        }


        #endregion


        #region ------  FUNCTIONS  ------

        /// <summary>
        /// Vérifier si une valeur est null ou vide
        /// </summary>
        /// <param name="dataAccessor">data</param>
        /// <param name="nameValue">Name field</param>
        /// <param name="Advanced">Verify: IsNullOrWhiteSpace + 0</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool IsNullOrEmpty(this IDataAccessor dataAccessor, string nameValue, bool Advanced=true)
        {
            try
            {
                if(dataAccessor==null) return true;
                var adata = dataAccessor.GetData(nameValue, DataAccessorOptionEnum.Safe);
                if (adata == null || adata == DBNull.Value) return true;
                var strdata = adata.ToString();
                if(string.IsNullOrEmpty(strdata)) return true;
                if (Advanced)
                {
                    if (strdata == "0") return true;
                    if (string.IsNullOrWhiteSpace(strdata)) return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"IsEmpty "+ex.Message);
            }
        }
         

        #endregion

        #region ------ DEPRECATED METHODS (WILL BE REMOVED IN NEXT VERSION) ------

        // ===========================================================================================
        // NOTICE: Ces méthodes sont dépréciées et seront supprimées dans la prochaine version.
        // Elles sont marquées avec [Obsolete(error: true)] pour forcer les erreurs de compilation
        // et guider les développeurs vers la nouvelle API générique GetValue<T>.
        // 
        // MIGRATION:
        // - Remplacez GetDateTime(name, defaultValue, options) par GetValue<DateTime>(name, defaultValue, options)
        // - Remplacez GetDateTimeNullable(name) par GetValue<DateTime?>(name)
        // - Remplacez GetDateTimeArray(name) par GetValue<DateTime[]>(name)
        // - Idem pour Long, Double, Boolean, etc.
        // ===========================================================================================

        // ------ DateTime Methods ------

        [Obsolete("Use GetValue<DateTime>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static DateTime GetDateTime(this IDataAccessor dataAccessor, string nameValue, DateTime defaultValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<DateTime>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<DateTime?>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static DateTime? GetDateTimeNullable(this IDataAccessor dataAccessor, string nameValue, DateTime? defaultValue = null, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<DateTime?>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<DateTime[]>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static DateTime[] GetDateTimeArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.Safe)
        {
            return GetValue<DateTime[]>(dataAccessor, nameValue, null, accesOptions);
        }

        // ------ Long Methods ------

        [Obsolete("Use GetValue<long>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static long GetLong(this IDataAccessor dataAccessor, string nameValue, long defaultValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<long>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<long?>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static long? GetLongNullable(this IDataAccessor dataAccessor, string nameValue, long? defaultValue = null, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<long?>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<long[]>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static long[] GetLongArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.Safe, long defaultValue = 0)
        {
            return GetValue<long[]>(dataAccessor, nameValue, null, accesOptions);
        }

        // ------ Double Methods ------

        [Obsolete("Use GetValue<double>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static double GetDouble(this IDataAccessor dataAccessor, string nameValue, double defaultValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<double>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<double?>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static double? GetDoubleNullable(this IDataAccessor dataAccessor, string nameValue, double? defaultValue = null, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<double?>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<double[]>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static double[] GetDoubleArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.Safe, double defaultValue = 0)
        {
            return GetValue<double[]>(dataAccessor, nameValue, null, accesOptions);
        }

        // ------ Boolean Methods ------

        [Obsolete("Use GetValue<bool>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static bool GetBoolean(this IDataAccessor dataAccessor, string nameValue, bool defaultValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<bool>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<bool?>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static bool? GetBooleanNullable(this IDataAccessor dataAccessor, string nameValue, bool? defaultValue = null, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<bool?>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<bool[]>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static bool[] GetBooleanArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.Safe, bool defaultValue = false)
        {
            return GetValue<bool[]>(dataAccessor, nameValue, null, accesOptions);
        }

        // ------ String Additional Methods ------

        [Obsolete("Use GetValue<string>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static string GetString(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum accesOptions)
        {
            return GetValue<string>(dataAccessor, nameValue, string.Empty, accesOptions);
        }

        [Obsolete("Use GetValue<string[]>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static string[] GetStringArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.Safe)
        {
            return GetValue<string[]>(dataAccessor, nameValue, null, accesOptions);
        }

        // ------ Int Additional Methods ------

        [Obsolete("Use GetValue<int>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static int GetInt(this IDataAccessor dataAccessor, string nameValue, int defaultValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<int>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<int?>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static int? GetIntNullable(this IDataAccessor dataAccessor, string nameValue, int? defaultValue = null, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None)
        {
            return GetValue<int?>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        [Obsolete("Use GetValue<int[]>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static int[] GetIntArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.Safe, int defaultValue = 0)
        {
            return GetValue<int[]>(dataAccessor, nameValue, null, accesOptions);
        }

        // ------ Enum Methods ------

        [Obsolete("Use GetValue<TEnum>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static TEnum GetEnum<TEnum>(this IDataAccessor dataAccessor, string nameValue) where TEnum : struct
        {
            return GetEnum<TEnum>(dataAccessor, nameValue, DataAccessorOptionEnum.None);
        }

        [Obsolete("Use GetValue<TEnum>(name, defaultValue, options) instead. This method will be removed in next version.", error: true)]
        public static TEnum GetEnum<TEnum>(this IDataAccessor dataAccessor, string nameValue, TEnum defaultValue, DataAccessorOptionEnum accesOptions = DataAccessorOptionEnum.None) where TEnum : struct
        {
            return GetValue<TEnum>(dataAccessor, nameValue, defaultValue, accesOptions);
        }

        #endregion

    }
}