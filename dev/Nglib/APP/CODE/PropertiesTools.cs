using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.APP.CODE
{
    /// <summary>
    /// Manipulation de valeurs dans un objets
    /// </summary>
    public static class PropertiesTools
    {


        /// <summary>
        /// Obtient toute les propriété déclarées et les valeurs
        /// </summary>
        /// <returns></returns>
        public static List<PropertyInfo> GetProperties(Type potype)
        {
            PropertyInfo[] props = potype.GetProperties();
            List<PropertyInfo> retour = new List<PropertyInfo>();
            if (props != null)
                foreach (PropertyInfo prp in props)
                {
                    // !!! trier uniquement les vraies données
                    retour.Add(prp);

                }
            return retour;
        }


        /// <summary>
        /// Obtient la property d'un objet
        /// </summary>
        /// <param name="objSrc"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(object objSrc, string propertyName)
        {
            if (objSrc == null || string.IsNullOrWhiteSpace(propertyName)) return null;
            PropertyInfo prop = objSrc.GetType().GetProperty(propertyName);
            if (prop == null) prop = objSrc.GetType().GetProperties().FirstOrDefault(p => propertyName.Equals(p.Name, StringComparison.OrdinalIgnoreCase)); // onréessaye en mode case nosensitive
            return prop;
        }


        /// <summary>
        /// Obtenir la données dans un objet
        /// </summary>
        /// <param name="objSrc"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetValueReflexion(object objSrc, string propertyName)
        {
            if (objSrc == null) throw new ArgumentNullException("objSrc");
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
            PropertyInfo prop = GetProperty(objSrc, propertyName);
            if (prop == null) throw new Exception($"Property {propertyName} not found in {objSrc.GetType().Name}");
            return prop.GetValue(objSrc, null);
        }


        /// <summary>
        /// Obtenir la données dans un objet SAFE
        /// </summary>
        /// <param name="objSrc"></param>
        /// <param name="propertyName"></param>
        /// <param name="safeException"></param>
        /// <returns></returns>
        public static object GetValueReflexion(object objSrc, string propertyName, bool safeException)
        {
            try
            {
                if (objSrc == null) { if (safeException) return null; else throw new ArgumentNullException("objSrc"); }
                return GetValueReflexion(objSrc, propertyName);
            }
            catch (Exception ex)
            {
                if (safeException) return null;
                else throw new Exception($"GetValueReflexion " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtenir la données et la convertir en string
        /// </summary>
        /// <param name="objSrc"></param>
        /// <param name="propertyName"></param>
        /// <param name="safeException"></param>
        /// <returns></returns>
        public static string GetStringReflexion(object objSrc, string propertyName, bool safeException = false)
        {
            object obj = GetValueReflexion(objSrc, propertyName, safeException);
            if (obj == null) return null;
            return obj.ToString();
        }



        /// <summary>
        /// Permet de mettre à jours une veleur dans un objet
        /// Réalisera un convertion de la valeur si nécessaire
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        public static void SetValueReflexion(object objDest, PropertyInfo propertyInfo, object value, object[] index = null)
        {
            if (objDest == null) throw new ArgumentNullException("objSrc");
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            try
            {
                object realvalue;
                if (propertyInfo.PropertyType == typeof(string))
                    realvalue = Convert.ToString(value);
                else if (propertyInfo.PropertyType == typeof(bool))
                    realvalue = FORMAT.ConvertPlus.ToBoolean(value);
                else if (propertyInfo.PropertyType == typeof(char))
                    realvalue = Convert.ToChar(value);
                else if (propertyInfo.PropertyType == typeof(byte))
                    realvalue = Convert.ToByte(value);
                else if (propertyInfo.PropertyType == typeof(decimal))
                    realvalue = Convert.ToDecimal(value);
                else if (propertyInfo.PropertyType == typeof(int))
                    realvalue = Convert.ToInt32(value);
                else if (propertyInfo.PropertyType == typeof(long))
                    realvalue = Convert.ToInt64(value);
                else if (propertyInfo.PropertyType == typeof(DateTime))
                    realvalue = FORMAT.ConvertPlus.ToDateTime(value);
                else if (propertyInfo.PropertyType == typeof(DateTime?))
                    realvalue = FORMAT.ConvertPlus.ToDateTime(value); // !!!
                else
                    realvalue = value;

                propertyInfo.SetValue(objDest, realvalue, index);
            }
            catch (Exception ex)
            {
                throw new Exception($"SetValueAndConvert({propertyInfo.Name}) {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Permet de mettre à jours une veleur dans un objet
        /// Réalisera un convertion de la valeur si nécessaire
        /// </summary>
        /// <param name="objDest"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        public static void SetValueReflexion(object objDest, string propertyName, object value, object[] index = null)
        {
            if (objDest == null) throw new ArgumentNullException("objDest");
            PropertyInfo prop = GetProperty(objDest, propertyName);
            if (prop == null) throw new Exception($"Property {propertyName} not found in {objDest.GetType().Name}");
            SetValueReflexion(objDest, prop, value, index);
        }






        /// <summary>
        /// Mettre à jours un objet à partir d'un dictionary
        /// </summary>
        /// <param name="objDest"></param>
        /// <param name="values"></param>
        public static void SetValuesReflexion(object objDest, IDictionary<string, object> values)
        {
            if (objDest == null) throw new ArgumentNullException("objDest");
            Type someObjectType = objDest.GetType();
            foreach (KeyValuePair<string, object> item in values)
            {
                PropertyInfo proinfo = someObjectType.GetProperty(item.Key);
                if (proinfo != null)
                    proinfo.SetValue(objDest, item.Value, null);
            }
        }


        /// <summary>
        /// Obtenir les données d'un objet dans un dictionary
        /// </summary>
        public static Dictionary<string, object> GetValuesReflexion(object objScr, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return objScr.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(objScr, null)
            );

        }




















    }
}
