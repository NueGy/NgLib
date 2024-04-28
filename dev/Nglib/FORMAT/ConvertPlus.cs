// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.KEYVALUES;
using System;
using System.Reflection;

namespace Nglib.FORMAT
{
    /// <summary>
    ///     Amélioration du convertisseur en prenant en compte plus de possibilités
    /// </summary>
    public static class ConvertPlus
    {


        /// <summary>
        ///     Object to Bool, prend en compte des valeur string comme "on", "yes", "1"
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ToBoolean(object obj, bool safe = false)
        {
            try
            {
                if (obj == null) return false;
                if (obj is bool) return (bool)obj;
                if (obj is int)
                {
                    if ((int)obj > 0) return true;
                    return false;
                }
                var ret = obj.ToString().ToLower();
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
        ///     permet de convertir un string en une date (support les format date8 yyyyMMdd)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(object obj)
        {
            if (obj is string)
            {
                var objstr = ((string)obj).Trim();
                if (objstr.Length == 8 && !objstr.Contains(" ") && !objstr.Contains("/") &&
                    !objstr.Contains("-")) // CONVERTION depuis yyyyMMdd
                    return DateTools.ConvertDateTime8(objstr);
            }
            return Convert.ToDateTime(obj);
        }


        public static int ToInt(object obj, int? defaultValue = 0)
        {
            if ((obj == null || obj == DBNull.Value) && defaultValue.HasValue) return defaultValue.Value;

            if (obj is bool)
                if ((bool)obj) return 1;
                else return 0;
            if (obj is string)
            {
                var ret = obj.ToString().ToLower().Trim();
                if (string.IsNullOrWhiteSpace(ret) && defaultValue.HasValue) return defaultValue.Value;
                if (ret == "true") return 1;
                if (ret == "false") return 0;
            }
            return Convert.ToInt32(obj);
        }


        /// <summary>
        ///     Changer le type d'un objet en un autre. En utilisant la fonction  Nglib.FORMAT.ConvertPlus
        /// </summary>
        public static object ChangeType(object valeur, Type type)
        {
            if(type==null) throw new ArgumentNullException("ChangeType.Type");
            if (valeur == DBNull.Value) valeur = null; 
            if (valeur == null && Nullable.GetUnderlyingType(type) != null)
                return null; //Ce champ admet des null

            if (type.Equals(typeof(int)))       return ToInt(valeur);
            if (type.Equals(typeof(bool)))      return ToBoolean(valeur);
            if (type.Equals(typeof(DateTime)))  return ToDateTime(valeur);
            return Convert.ChangeType(valeur, type); // sinon on utilise le changetype normal
        }


        /// <summary>
        ///     Changer le type d'un objet en un autre. En utilisant la fonction  Nglib.FORMAT.ConvertPlus
        /// </summary>
        public static object ChangeType(object valeur, string typename)
            => ChangeType(valeur, ParseType(typename));
 

        public static Type ParseType(string typename)
        {
            if(string.IsNullOrWhiteSpace(typename)) return null;
            typename = typename.Trim().ToLower();
            Type type = null;
            if (typename == "string") type = typeof(string);
            else if (typename == "int" || typename == "numeric") type = typeof(int);
            else if (typename == "datetime") type = typeof(DateTime);
            else if (typename == "date") type = typeof(DateTime);
            else if (typename == "double" ) type = typeof(double);
            else if (typename == "bool") type = typeof(bool);
            else if (typename == "long") type = typeof(long);
            else if (typename == "char") type = typeof(char);
            else if (typename == "byte") type = typeof(byte);
            else if (typename == "decimal") type = typeof(decimal);
 
            return type;
        }


    }
}