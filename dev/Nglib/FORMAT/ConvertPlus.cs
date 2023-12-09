// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;

namespace Nglib.FORMAT
{
    /// <summary>
    ///     Amélioration du convertisseur en prenant en compte plus de possibilitées
    /// </summary>
    public static class ConvertPlus
    {
        /*
        public static Tobj ChangeType<Tobj>(object obj, Tobj DefaultValue)
        {
            Type type = typeof(Tobj);
            if (type.Equals(typeof(int))) return (Tobj)ConvertPlus.ToInt(obj);
            else if (type.Equals(typeof(bool))) return ConvertPlus.ToBoolean(obj);
            else if (type.Equals(typeof(DateTime))) return ConvertPlus.ToDateTime(obj);
            else return Convert.ChangeType(obj, type);

        }
        */


        /// <summary>
        ///     Object to Bool, prend en compte des
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
        ///     permet de convertir un string en une date (support les format yyyyMMdd ou ddMMyyyy)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(object obj)
        {
            if (obj is string)
            {
                var objstr = (string)obj;
                if (objstr.Length == 8 && !objstr.Contains(" ") && !objstr.Contains("/") &&
                    !objstr.Contains("-")) // CONVERTION depuis yyyyMMdd ou ddMMyyyy
                {
                    var selFormdate = Convert.ToInt32(objstr.Substring(6, 2));
                    var retour = new DateTime();
                    if (selFormdate > 12) // Si le mois supérieur à 12 donc c'est une année
                        retour = new DateTime(Convert.ToInt32(objstr.Substring(0, 4)),
                            Convert.ToInt32(objstr.Substring(4, 2)), Convert.ToInt32(objstr.Substring(6, 2)));
                    else
                        retour = new DateTime(Convert.ToInt32(objstr.Substring(4, 4)),
                            Convert.ToInt32(objstr.Substring(2, 2)), Convert.ToInt32(objstr.Substring(0, 2)));
                    if (retour.Year < 2000 && retour.Year > 2099) throw new Exception("date year invalide");
                    return retour;
                }

                return Convert.ToDateTime(obj);
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
                var ret = obj.ToString().ToLower();
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
            if (type.Equals(typeof(int)))
                return ToInt(valeur);
            if (type.Equals(typeof(bool))) return ToBoolean(valeur);
            if (type.Equals(typeof(DateTime))) return ToDateTime(valeur);
            return Convert.ChangeType(valeur, type); // sinon on utilise le changetype normal
        }


        /// <summary>
        ///     Changer le type d'un objet en un autre. En utilisant la fonction  Nglib.FORMAT.ConvertPlus
        /// </summary>
        public static object ChangeType(object valeur, string typename)
        {
            if (string.IsNullOrWhiteSpace(typename)) return valeur;
            Type type = null;
            if (typename == "string") type = typeof(string);
            else if (typename == "int" || typename == "numeric") type = typeof(int);
            else if (typename == "datetime") type = typeof(DateTime);
            else if (typename == "double" && !(valeur is double)) type = typeof(double);
            else if (typename == "bool" && !(valeur is bool)) type = typeof(bool);
            else if (typename == "long" && !(valeur is long)) type = typeof(long);
            if (type == null) throw new Exception("typename factory not found");
            return ChangeType(valeur, type);
        }
    }
}