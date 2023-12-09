using System;
using System.Collections.Generic;
using System.Linq;
using Nglib.APP.CODE;
using Nglib.FORMAT;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    ///     Extentions de méthodes pout l'accès faciles aux données
    /// </summary>
    public static class DataAccessorExtentions
    {
        #region ------  OBJECT  ------

        /// <summary>
        ///     Obtenir la données
        /// </summary>
        public static object GetObject(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions)
        {
            //Obtenir la données la donnée
            var adata = dataAccessor.GetData(nameValue, AccesOptions);

            //Cryptage
            if (adata != null && AccesOptions.HasFlag(DataAccessorOptionEnum.Encrypted))
            {
                var cryptoctx = dataAccessor.GetCryptoContext();
                if (cryptoctx != null)
                    adata = cryptoctx.DecryptObjectValue(dataAccessor, nameValue, adata, AccesOptions);
            }


            return adata;
        }

        /// <summary>
        ///     Obtenir la données
        /// </summary>
        public static object GetObject(this IDataAccessor dataAccessor, string nameValue)
        {
            return dataAccessor.GetData(nameValue, DataAccessorOptionEnum.None);
        }


        /// <summary>
        ///     Définir une donnée
        /// </summary>
        public static bool SetObject(this IDataAccessor dataAccessor, string nameValue, object obj,
            DataAccessorOptionEnum AccesOptions)
        {
            var adata = obj;

            // Cryptage
            if (obj != null && AccesOptions.HasFlag(DataAccessorOptionEnum.Encrypted))
            {
                var cryptoctx = dataAccessor.GetCryptoContext();
                if (cryptoctx != null)
                    adata = cryptoctx.EncryptObjectValue(dataAccessor, nameValue, adata, AccesOptions);
            }

            if (adata == null) adata = DBNull.Value; // les nul son interdit en base
            return dataAccessor.SetData(nameValue, adata, AccesOptions);
        }

        /// <summary>
        ///     Définir une donnée
        /// </summary>
        public static bool SetObject(this IDataAccessor dataAccessor, string nameValue, object obj)
        {
            return dataAccessor.SetObject(nameValue, obj, DataAccessorOptionEnum.None);
        }


        /// <summary>
        ///     Obtient les données d'un autre objet par la reflexion
        /// </summary>
        /// <param name="objetSource"></param>
        /// <param name="replaceifnotnull"></param>
        /// <param name="safe"></param>
        /// <returns></returns>
        public static bool FromObject(this IDataAccessor dataAccessor, object objetSource, bool replaceifnotnull = true,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var vals = PropertiesTools.GetValuesReflexion(objetSource);
                vals.Keys.ToList().ForEach(k => dataAccessor.SetObject(k, vals[k]));
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("FromObject " + ex.Message, ex);
            }
        }

        /// <summary>
        ///     Définie les données d'un autre objet par la reflexion
        /// </summary>
        /// <param name="objetTarget"></param>
        /// <param name="replaceifnotnull"></param>
        /// <param name="safe"></param>
        /// <returns></returns>
        public static bool ToObject(this IDataAccessor dataAccessor, object objetTarget, bool replaceifnotnull = true,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var vals = dataAccessor.ToDictionaryValues(AccesOptions);
                PropertiesTools.SetValuesReflexion(objetTarget, vals);
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("ToObject " + ex.Message, ex);
            }
        }


        //public static bool FromObject<Tobject>(this BASICS.INTERFACES.IDataAccessor dataAccessor, object objetSource, bool replaceifnotnull = true, DataAccessorOptionEnum AccesOptions = 0)
        //{
        //    return false;
        //    //throw new NotImplementedException();
        //}


        //public static bool ToObject<Tobject>(this BASICS.INTERFACES.IDataAccessor dataAccessor, object objetSource, bool replaceifnotnull = true, DataAccessorOptionEnum AccesOptions = 0)
        //{
        //    return false;
        //    //throw new NotImplementedException();
        //}

        #endregion


        #region ------  STRING  ------

        /// <summary>
        ///     Obtenir un string (SAFE et jamais null)
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static string GetString(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetString(dataAccessor, nameValue, DataAccessorOptionEnum.Safe);
        }


        /// <summary>
        ///     Obtenir un string
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static string GetString(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions)
        {
            try
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);

                if (obj == null || obj == DBNull.Value) // retourne pas null par default
                {
                    if (AccesOptions.HasFlag(DataAccessorOptionEnum.Nullable)) return null;
                    return string.Empty;
                }

                return obj.ToString();
                //DATA.MANIPULATE.TEXT.TextTreatment.Transform
                //return str;
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                return string.Empty;
            }
        }


        public static string[] GetStringArray(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            var obj = dataAccessor.GetObject(nameValue, AccesOptions);
            //!!!todo : les export de array en fonctionnent pas avec les keyvalues

            var Tbjt = obj?.GetType();

            if (obj != null && Tbjt.FullName == "System.String") return new[] { obj.ToString() };


            var retour = obj as string[];
            return retour;
        }

        #endregion


        #region ------  INT  ------

        /// <summary>
        ///     Obtenir/Convertir en INT (0 si null)
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static int GetInt(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetInt(dataAccessor, nameValue, 0);
        }


        /// <summary>
        ///     Obtenir/Convertir en INT
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static int GetInt(this IDataAccessor dataAccessor, string nameValue, int DefaultValue,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter))
                    return ConvertPlus.ToInt(obj, DefaultValue);
                return Convert.ToInt32(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        ///     Obtenir/Convertir en INT Nullable
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static int? GetInt(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions, int? defaultvalue)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToInt(obj);
                return Convert.ToInt32(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        public static int[] GetIntArray(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None, int DefaultValue = 0)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            var obj = dataAccessor.GetObject(nameValue, AccesOptions);
            var retour = obj as int[];
            if (retour == null && obj is string[])
                retour = (obj as string[])
                    .Select(item => string.IsNullOrWhiteSpace(item) ? DefaultValue : Convert.ToInt32(item)).ToArray();
            return retour;
        }

        #endregion


        #region ------  DATETIME  ------

        /// <summary>
        ///     Obtenir/Convertir en DateTime
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetDateTime(dataAccessor, nameValue, new DateTime());
        }

        /// <summary>
        ///     Obtenir/Convertir en DateTime
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IDataAccessor dataAccessor, string nameValue, DateTime DefaultValue,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToDateTime(obj);
                return Convert.ToDateTime(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        ///     Obtenir/Convertir en DateTime NULLABLE
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions, DateTime? defaultvalue)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToDateTime(obj);
                return Convert.ToDateTime(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        public static DateTime[] GetDateTimeArray(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            var obj = dataAccessor.GetObject(nameValue, AccesOptions);
            var retour = obj as DateTime[];
            if (retour == null && obj is string[])
                retour = (obj as string[])
                    .Select(item => string.IsNullOrWhiteSpace(item) ? new DateTime() : Convert.ToDateTime(item))
                    .ToArray();
            return retour;
        }

        #endregion


        #region ------  LONG  ------

        /// <summary>
        ///     Obtenir/Convertir en Long
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static long GetLong(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetLong(dataAccessor, nameValue, 0);
        }


        /// <summary>
        ///     Obtenir/Convertir en Long
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static long GetLong(this IDataAccessor dataAccessor, string nameValue, long DefaultValue,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                return Convert.ToInt64(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        ///     Obtenir/Convertir en Long Nullable
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static long? GetLong(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions, long? defaultvalue)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                return Convert.ToInt64(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        public static long[] GetLongArray(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None, long DefaultValue = 0)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            var obj = dataAccessor.GetObject(nameValue, AccesOptions);
            var retour = obj as long[];
            if (retour == null && obj is string[])
                retour = (obj as string[])
                    .Select(item => string.IsNullOrWhiteSpace(item) ? DefaultValue : Convert.ToInt64(item)).ToArray();
            return retour;
        }

        #endregion


        #region ------  DOUBLE  ------

        /// <summary>
        ///     Obtenir/Convertir en Double
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static double GetDouble(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetDouble(dataAccessor, nameValue, 0);
        }


        /// <summary>
        ///     Obtenir/Convertir en Double
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static double GetDouble(this IDataAccessor dataAccessor, string nameValue, long DefaultValue,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                return Convert.ToDouble(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }


        /// <summary>
        ///     Obtenir/Convertir en Double NULLABLE
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static double? GetDouble(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions, double? defaultvalue)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                return Convert.ToDouble(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }


        public static double[] GetDoubleArray(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None, double defaultvalue = 0)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            var obj = dataAccessor.GetObject(nameValue, AccesOptions);
            var retour = obj as double[];
            if (retour == null && obj is string[])
                retour = (obj as string[])
                    .Select(item => string.IsNullOrWhiteSpace(item) ? defaultvalue : Convert.ToDouble(item)).ToArray();
            return retour;
        }

        #endregion


        #region ------  BOOL  ------

        /// <summary>
        ///     Obtenir/Convertir en Bool
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static bool GetBoolean(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetBoolean(dataAccessor, nameValue, false);
        }

        /// <summary>
        ///     Obtenir/Convertir en Bool
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static bool GetBoolean(this IDataAccessor dataAccessor, string nameValue, bool DefaultValue,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToBoolean(obj);
                return Convert.ToBoolean(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        ///     Obtenir/Convertir en Bool NULLABLE
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static bool? GetBoolean(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions, bool? defaultvalue)
        {
            try
            {
                var obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToBoolean(obj);
                return Convert.ToBoolean(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }


        public static bool[] GetBooleanArray(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None, bool defaultvalue = false)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            var obj = dataAccessor.GetObject(nameValue, AccesOptions);
            var retour = obj as bool[];
            if (retour == null && obj is string[])
                retour = (obj as string[])
                    .Select(item => string.IsNullOrWhiteSpace(item) ? defaultvalue : Convert.ToBoolean(item)).ToArray();
            return retour;
        }

        #endregion


        #region ------  DICTIONARY  ------

        /// <summary>
        ///     Obtenir les données dans un dictionary
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


        public static Dictionary<string, string> ToDictionaryString(this IDataAccessor dataAccessor,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            var retour = new Dictionary<string, string>();
            foreach (var item in dataAccessor.ListFieldsKeys())
                retour.Add(item, dataAccessor.GetString(item, AccesOptions));
            return retour;
        }


        public static void FromDictionaryValues(this IDataAccessor dataAccessor, Dictionary<string, object> Values,
            DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                var options = DataAccessorOptionEnum.None;
                foreach (var itemkey in Values.Keys) dataAccessor.SetObject(itemkey, Values[itemkey], options);
            }
            catch (Exception)
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) return;
                throw;
            }
        }

        #endregion


        #region ------  ENUM  ------

        public static TEnum GetEnum<TEnum>(this IDataAccessor dataAccessor, string fieldname,
            DataAccessorOptionEnum AccesOptions = 0) where TEnum : struct
        {
            var defaultValue = DataAccessorTools.GetEnumDefaultValue<TEnum>();
            return GetEnum<TEnum>(dataAccessor, fieldname, defaultValue, AccesOptions).Value;
        }
        //public static TEnum GetEnum<TEnum>(this IDataAccessor dataAccessor, string fieldname, TEnum defaultValue) where TEnum : struct
        //{
        //    DataAccessorOptionEnum AccesOptions = 0
        //    return GetEnum<TEnum>(dataAccessor, fieldname, defaultValue, AccesOptions).Value;
        //}


        public static TEnum? GetEnum<TEnum>(this IDataAccessor dataAccessor, string fieldname, TEnum? defaultValue,
            DataAccessorOptionEnum AccesOptions = 0) where TEnum : struct
        {
            var foo = defaultValue;
            var obj = dataAccessor.GetObject(fieldname, AccesOptions);
            try
            {
                if (obj == null || obj == DBNull.Value || obj is DBNull)
                {
                    foo = defaultValue;
                }
                else if (obj is string)
                {
                    var objTypeIn = obj.ToString();
                    if (!string.IsNullOrWhiteSpace(objTypeIn))
                    {
                        foo = (TEnum)Enum.Parse(typeof(TEnum), objTypeIn);
                        if (!Enum.IsDefined(typeof(TEnum), foo) && !foo.ToString().Contains(","))
                            throw new InvalidOperationException(
                                string.Format("{0} is not an underlying value of the YourEnum enumeration.",
                                    objTypeIn));
                    }
                }
                else if (obj is int)
                {
                    foo = (TEnum)obj;
                }
                else if (obj is short)
                {
                    obj = Convert.ToInt32(obj); // !!! améliorer
                    foo = (TEnum)obj;
                }
                else if (obj is TEnum) // si c'est déja un enum
                {
                    foo = (TEnum)obj;
                }
                else
                {
                    throw new NotImplementedException(string.Format("Type {0} invalid, only string or int",
                        obj.GetType()));
                }
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new Exception("GetEnum " + ex.Message, ex);
            }

            return foo;
        }


        public static TEnum[] GetEnumArray<TEnum>(this IDataAccessor dataAccessor, string nameValue,
            DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            throw new NotImplementedException(); // !!!
        }

        #endregion
    }
}