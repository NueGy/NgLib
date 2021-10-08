﻿using Nglib.FORMAT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.ACCESSORS
{


    /// <summary>
    /// Extentions de méthodes pout l'accès faciles aux données
    /// </summary>
    public static class DataAccessorExtentions
    {



        #region ------  OBJECT  ------



        /// <summary>
        /// Obtenir la données
        /// </summary>
        public static object GetObject(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            //Obtenir la données la donnée
            object adata = dataAccessor.GetData(nameValue, AccesOptions);

            //Cryptage
            if (adata!=null && AccesOptions.HasFlag(DataAccessorOptionEnum.Encrypted))
            {
                var cryptoctx = dataAccessor.GetCryptoContext();
                if (cryptoctx != null)
                    adata = cryptoctx.DecryptObjectValue(dataAccessor, nameValue, adata, AccesOptions);
            }
               
            

            return adata;
        }
        /// <summary>
        /// Obtenir la données
        /// </summary>
        public static object GetObject(this IDataAccessor dataAccessor, string nameValue)
        {
            return dataAccessor.GetData(nameValue, DataAccessorOptionEnum.None);
        }


        /// <summary>
        /// Définir une donnée
        /// </summary>
        public static bool SetObject(this IDataAccessor dataAccessor, string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            object adata = obj; 

            // Cryptage
            if (obj!=null && AccesOptions.HasFlag(DataAccessorOptionEnum.Encrypted))
            {
                var cryptoctx = dataAccessor.GetCryptoContext();
                if (cryptoctx != null)
                    adata = cryptoctx.EncryptObjectValue(dataAccessor, nameValue, adata, AccesOptions);
            }
            if (adata == null) adata = DBNull.Value; // les nul son interdit en base
            return dataAccessor.SetData(nameValue, adata, AccesOptions);
        }

        /// <summary>
        /// Définir une donnée
        /// </summary>
        public static bool SetObject(this IDataAccessor dataAccessor, string nameValue, object obj)
        {
            return dataAccessor.SetObject(nameValue, obj, DataAccessorOptionEnum.None);
        }


        /// <summary>
        /// Obtient les données d'un autre objet par la reflexion
        /// </summary>
        /// <param name="objetSource"></param>
        /// <param name="replaceifnotnull"></param>
        /// <param name="safe"></param>
        /// <returns></returns>
        public static bool FromObject(this IDataAccessor dataAccessor, object objetSource, bool replaceifnotnull = true, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                Dictionary<string, object> vals = Nglib.APP.CODE.PropertiesTools.GetValuesReflexion(objetSource);
                vals.Keys.ToList().ForEach(k => dataAccessor.SetObject(k, vals[k]));
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("FromObject "+ex.Message,ex);
            }
        }

        /// <summary>
        /// Définie les données d'un autre objet par la reflexion
        /// </summary>
        /// <param name="objetTarget"></param>
        /// <param name="replaceifnotnull"></param>
        /// <param name="safe"></param>
        /// <returns></returns>
        public static bool ToObject(this IDataAccessor dataAccessor, object objetTarget, bool replaceifnotnull = true, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                Dictionary<string, object> vals = dataAccessor.ToDictionaryValues(AccesOptions);
                Nglib.APP.CODE.PropertiesTools.SetValuesReflexion(objetTarget, vals);
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
        /// Obtenir un string (SAFE et jamais null)
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static string GetString(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetString(dataAccessor, nameValue, DataAccessorOptionEnum.Safe);
        }


        /// <summary>
        /// Obtenir un string
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static string GetString(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            try
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);

                if ((obj == null || obj == DBNull.Value)) // retourne pas null par default
                {
                    if (AccesOptions.HasFlag(DataAccessorOptionEnum.Nullable)) return null;
                    else return string.Empty;
                }
                return obj.ToString();
                //DATA.MANIPULATE.TEXT.TextTreatment.Transform
                //return str;
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                return string.Empty;
            }
        }


        public static string[] GetStringArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            object obj = dataAccessor.GetObject(nameValue, AccesOptions);
            string[] retour = obj as string[];
            return retour;
        }




        public static string DynamicValues(this IDataAccessor dataAccessor, string formatingValues, Dictionary<string, string> transformFuctions = null, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            try
            {
                // dynamisation de la chaine de caratère
                string retour = FORMAT.StringTransform.DynamiseWithAccessor(formatingValues, dataAccessor);
                // Transformation de la chaine finale selon une série d'instruction de transformation
                if (transformFuctions != null)
                    retour = FORMAT.StringTransform.Transform(retour, transformFuctions);
                return retour;
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("FormatingMultiValues " + ex.Message, ex);
                return string.Empty;
            }
        }








        #endregion





        #region ------  INT  ------


        /// <summary>
        /// Obtenir/Convertir en INT (0 si null)
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static int GetInt(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetInt(dataAccessor, nameValue, 0);
        }


        /// <summary>
        /// Obtenir/Convertir en INT
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static int GetInt(this IDataAccessor dataAccessor, string nameValue, int DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                else if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToInt(obj, DefaultValue);
                else return Convert.ToInt32(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Obtenir/Convertir en INT Nullable
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static int? GetInt(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions, int? defaultvalue)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                else if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToInt(obj);
                else return Convert.ToInt32(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        public static int[] GetIntArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions= DataAccessorOptionEnum.None, int DefaultValue=0) 
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            object obj = dataAccessor.GetObject(nameValue, AccesOptions);
            int[] retour = obj as int[];
            if (retour == null && obj is string[])
                retour = (obj as string[]).Select(item => (string.IsNullOrWhiteSpace(item)) ? DefaultValue : Convert.ToInt32(item)).ToArray();
            return retour;
        }



        #endregion





        #region ------  DATETIME  ------

        /// <summary>
        /// Obtenir/Convertir en DateTime
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetDateTime(dataAccessor, nameValue, new DateTime());
        }

        /// <summary>
        /// Obtenir/Convertir en DateTime
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IDataAccessor dataAccessor, string nameValue, DateTime DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                else if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToDateTime(obj);
                else return Convert.ToDateTime(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Obtenir/Convertir en DateTime NULLABLE
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions, DateTime? defaultvalue)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                else if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToDateTime(obj);
                else return Convert.ToDateTime(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        public static DateTime[] GetDateTimeArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            object obj = dataAccessor.GetObject(nameValue, AccesOptions);
            DateTime[] retour = obj as DateTime[];
            if (retour == null && obj is string[])
                retour = (obj as string[]).Select(item => (string.IsNullOrWhiteSpace(item)) ? new DateTime() : Convert.ToDateTime(item)).ToArray();
            return retour;
        }

        #endregion





        #region ------  LONG  ------


        /// <summary>
        /// Obtenir/Convertir en Long
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static long GetLong(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetLong(dataAccessor, nameValue, 0);
        }


        /// <summary>
        /// Obtenir/Convertir en Long
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static long GetLong(this IDataAccessor dataAccessor, string nameValue, long DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                else return Convert.ToInt64(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Obtenir/Convertir en Long Nullable
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static long? GetLong(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions, long? defaultvalue)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                else return Convert.ToInt64(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        public static long[] GetLongArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None, long DefaultValue=0)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            object obj = dataAccessor.GetObject(nameValue, AccesOptions);
            long[] retour = obj as long[];
            if (retour == null && obj is string[])
                retour = (obj as string[]).Select(item => (string.IsNullOrWhiteSpace(item)) ? DefaultValue : Convert.ToInt64(item)).ToArray();
            return retour;
        }

        #endregion





        #region ------  DOUBLE  ------


        /// <summary>
        /// Obtenir/Convertir en Double
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static double GetDouble(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetDouble(dataAccessor, nameValue, 0);
        }


        /// <summary>
        /// Obtenir/Convertir en Double
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static double GetDouble(this IDataAccessor dataAccessor, string nameValue, long DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                else return Convert.ToDouble(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }


        /// <summary>
        /// Obtenir/Convertir en Double NULLABLE
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static double? GetDouble(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions, double? defaultvalue)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                else return Convert.ToDouble(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }


        public static double[] GetDoubleArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None, double defaultvalue=0)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            object obj = dataAccessor.GetObject(nameValue, AccesOptions);
            double[] retour = obj as double[];
            if (retour == null && obj is string[])
                retour = (obj as string[]).Select(item => (string.IsNullOrWhiteSpace(item)) ? defaultvalue : Convert.ToDouble(item)).ToArray();
            return retour;
        }

        #endregion





        #region ------  BOOL  ------


        /// <summary>
        /// Obtenir/Convertir en Bool
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static bool GetBoolean(this IDataAccessor dataAccessor, string nameValue)
        {
            return GetBoolean(dataAccessor, nameValue, false);
        }

        /// <summary>
        /// Obtenir/Convertir en Bool 
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static bool GetBoolean(this IDataAccessor dataAccessor, string nameValue, bool DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return DefaultValue;
                if (obj is string && string.IsNullOrWhiteSpace((string)obj)) return DefaultValue;
                else if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToBoolean(obj);
                else return Convert.ToBoolean(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Obtenir/Convertir en Bool NULLABLE
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static bool? GetBoolean(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions, bool? defaultvalue)
        {
            try
            {
                object obj = dataAccessor.GetObject(nameValue, AccesOptions);
                if (obj == DBNull.Value || obj == null) return null;
                else if (AccesOptions.HasFlag(DataAccessorOptionEnum.AdvancedConverter)) return ConvertPlus.ToBoolean(obj);
                else return Convert.ToBoolean(obj);
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("GetString " + ex.Message, ex);
                throw;
            }
        }


        public static bool[] GetBooleanArray(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None, bool defaultvalue=false)
        {
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe) && dataAccessor == null) return null;
            object obj = dataAccessor.GetObject(nameValue, AccesOptions);
            bool[] retour = obj as bool[];
            if (retour == null && obj is string[])
                retour = (obj as string[]).Select(item => (string.IsNullOrWhiteSpace(item)) ? defaultvalue : Convert.ToBoolean(item)).ToArray();
            return retour;
        }


        #endregion







        #region ------  DICTIONARY  ------


        /// <summary>
        /// Obtenir les données dans un dictionary
        /// </summary>
        /// <param name="dataAccessor"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionaryValues(this IDataAccessor dataAccessor, DataAccessorOptionEnum AccesOptions = 0)
        {
            Dictionary<string, object> retour = new Dictionary<string, object>();
            foreach (string item in dataAccessor.ListFieldsKeys())
                retour.Add(item, dataAccessor.GetObject(item, AccesOptions));
            return retour;
        }


        public static Dictionary<string, string> ToDictionaryString(this IDataAccessor dataAccessor, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            Dictionary<string, string> retour = new Dictionary<string, string>();
            foreach (string item in dataAccessor.ListFieldsKeys())
                retour.Add(item, dataAccessor.GetString(item, AccesOptions));
            return retour;
        }


        public static void FromDictionaryValues(this IDataAccessor dataAccessor, Dictionary<string, object> Values, DataAccessorOptionEnum AccesOptions = 0)
        {
            try
            {
                DataAccessorOptionEnum options = DataAccessorOptionEnum.None;
                foreach (string itemkey in Values.Keys)
                {
                    dataAccessor.SetObject(itemkey, Values[itemkey], options);
                }
            }
            catch (Exception)
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) return;
                else throw;
            }

        }



        #endregion





        #region ------  ENUM  ------



        public static TEnum GetEnum<TEnum>(this IDataAccessor dataAccessor, string fieldname, DataAccessorOptionEnum AccesOptions = 0) where TEnum : struct
        {
            TEnum defaultValue = DataAccessorTools.GetEnumDefaultValue<TEnum>();
            return GetEnum<TEnum>(dataAccessor, fieldname, defaultValue, AccesOptions).Value;
        }




        public static TEnum? GetEnum<TEnum>(this IDataAccessor dataAccessor, string fieldname, TEnum? defaultValue, DataAccessorOptionEnum AccesOptions = 0) where TEnum : struct
        {
            TEnum? foo = defaultValue;
            Object obj = dataAccessor.GetObject(fieldname, AccesOptions);
            try
            {
                if (obj == null || obj == DBNull.Value || obj is System.DBNull)
                    foo = defaultValue;
                else if (obj is string)
                {
                    string objTypeIn = obj.ToString();
                    if (!string.IsNullOrWhiteSpace(objTypeIn))
                    {
                        foo = (TEnum)Enum.Parse(typeof(TEnum), objTypeIn);
                        if (!Enum.IsDefined(typeof(TEnum), foo) && !foo.ToString().Contains(","))
                            throw new InvalidOperationException(string.Format("{0} is not an underlying value of the YourEnum enumeration.", objTypeIn));
                    }

                }
                else if (obj is int || obj is short)
                {
                    foo = (TEnum)obj;
                }
                else
                {
                    throw new NotImplementedException(string.Format("Type {0} non géré", obj.GetType().ToString()));
                }

            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe))
                    throw new Exception("GetEnum " + ex.Message, ex);
            }
            return foo;
        }


        public static TEnum[] GetEnumArray<TEnum>(this IDataAccessor dataAccessor, string nameValue, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            throw new NotImplementedException(); // !!!
        }

        #endregion




    }
    

    
}
