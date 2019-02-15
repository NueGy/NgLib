// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Amélioration du convertisseur en prenant en compte plus de possibilitées
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
        /// Object to Bool, prend en compte des
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ToBoolean(object obj)
        {
            try
            {
                if (obj == null) return false;

                if (obj is bool) return (bool)obj;
                else if (obj is int)
                {
                    if ((int)obj > 0) return true;
                    else return false;
                }

                string ret = obj.ToString().ToLower();
                if (string.IsNullOrWhiteSpace(ret)) return false;
                else if (ret == "true") return true;
                else if (ret == "false") return false;
                else if (ret == "on") return true;
                else if (ret == "yes") return true;
                else if (ret == "1") return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw new Exception("ToBoolean " + ex.Message);
            }
        }







        /// <summary>
        /// permet de convertir un string en une date (support les format yyyyMMdd ou ddMMyyyy)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(object obj)
        {
            if (obj is string)
            {
                string objstr = (string)obj;
                if (objstr.Length == 8 && !objstr.Contains(" ") && !objstr.Contains("/") && !objstr.Contains("-")) // CONVERTION depuis yyyyMMdd ou ddMMyyyy
                {
                    int selFormdate = Convert.ToInt32(objstr.Substring(6, 2));
                    DateTime retour = new DateTime();
                    if (selFormdate > 12) // Si le mois supérieur à 12 donc c'est une année
                        retour = new DateTime(Convert.ToInt32(objstr.Substring(0, 4)), Convert.ToInt32(objstr.Substring(4, 2)), Convert.ToInt32(objstr.Substring(6, 2)));
                    else
                        retour = new DateTime(Convert.ToInt32(objstr.Substring(4, 4)), Convert.ToInt32(objstr.Substring(2, 2)), Convert.ToInt32(objstr.Substring(0, 2)));
                    if (retour.Year < 2000 && retour.Year > 2099) throw new Exception("date year invalide");
                    else return retour;
                }
                else return Convert.ToDateTime(obj);
            }
            else return Convert.ToDateTime(obj);
        }


        public static int ToInt(object obj, int? defaultValue = 0)
        {
            if ((obj == null || obj == DBNull.Value) && defaultValue.HasValue) return defaultValue.Value;

            if (obj is bool)
                if ((bool)obj == true) return 1; else return 0;
            if (obj is string)
            {
                string ret = obj.ToString().ToLower();
                if (string.IsNullOrWhiteSpace(ret) && defaultValue.HasValue) return defaultValue.Value;
                else if (ret == "true") return 1;
                else if (ret == "false") return 0;
            }

            return Convert.ToInt32(obj);

        }




        /// <summary>
        /// Changer le type d'un objet en un autre. En utilisant la fonction  Nglib.FORMAT.ConvertPlus
        /// </summary>
        public static object ChangeType(object valeur, System.Type type)
        {
            if (type.Equals(typeof(int)))
                return ConvertPlus.ToInt(valeur);
            else if (type.Equals(typeof(bool))) return ConvertPlus.ToBoolean(valeur);
            else if (type.Equals(typeof(DateTime))) return ConvertPlus.ToDateTime(valeur);
            else return Convert.ChangeType(valeur, type); // sinon on utilise le changetype normal

        }




        /// <summary>
        /// Changer le type d'un objet en un autre. En utilisant la fonction  Nglib.FORMAT.ConvertPlus
        /// </summary>
        public static object ChangeType(object valeur, string typename)
        {
            if (string.IsNullOrWhiteSpace(typename)) return valeur;
            System.Type type = null;
            if (typename == "string") type = typeof(string);
            else if ((typename == "int" || typename == "numeric")) type = typeof(int);
            else if (typename == "datetime") type = typeof(DateTime);
            else if (typename == "double" && !(valeur is double)) type = typeof(double);
            else if (typename == "bool" && !(valeur is bool)) type = typeof(bool);
            else if (typename == "long" && !(valeur is long)) type = typeof(long);
            if (type == null) throw new Exception("typename factory not found");
            return ChangeType(valeur, type);
        }



    }
}
