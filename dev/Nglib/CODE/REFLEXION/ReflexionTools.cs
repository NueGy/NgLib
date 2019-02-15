using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.CODE.REFLEXION
{
    /// <summary>
    /// Outils de reflexions types...
    /// </summary>
    public static class ReflexionTools
    {

        public static object GetPropertyValue(object src, string propName)
        {
            if (src == null || string.IsNullOrWhiteSpace(propName)) return null;
            PropertyInfo prop = src.GetType().GetProperty(propName);
            if(prop==null)prop = src.GetType().GetProperties().FirstOrDefault(p => propName.Equals(p.Name, StringComparison.OrdinalIgnoreCase)); // onréessaye en mode case nosensitive
            if (prop == null) return null;
            return prop.GetValue(src, null);
        }

        public static string GetPropertyString(object src, string propName)
        {
            object obj = GetPropertyValue(src, propName);
            if (obj == null) return null;
            return obj.ToString();
        }





        public static Type GetTypeByReflexion(string MyFullyQualifiedTypeName)
        {
            Type retour = null;
            retour = Type.GetType(MyFullyQualifiedTypeName, false, true);

            return retour;
        }



        public static T NewInstance<T>(Type type)
        {
            return (T)Activator.CreateInstance(type);
        }



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
        /// Chargement des proprietés d'un objet depuis les valeurs d'un dictionary
        /// </summary>
        public static T ObjectFromDictionary<T>(IDictionary<string, object> source) where T : class, new()
        {
            T someObject = new T();
            Type someObjectType = someObject.GetType();

            foreach (KeyValuePair<string, object> item in source)
            {
                someObjectType.GetProperty(item.Key).SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        /// <summary>
        /// Chargement des proprietés d'un objet depuis les valeurs d'un dictionary
        /// </summary>
        public static bool ObjectFromDictionary<T>(IDictionary<string, object> source, object objDest) where T : class
        {
            T someObject = (T)objDest;
            Type someObjectType = someObject.GetType();
            bool retour = false;

            foreach (KeyValuePair<string, object> item in source)
            {
                PropertyInfo proinfo = someObjectType.GetProperty(item.Key);
                if (proinfo != null)
                {
                    proinfo.SetValue(someObject, item.Value, null);
                    retour = true;
                }
            }
            return retour;
        }

        /// <summary>
        /// Création d'un dictionary à partir des propriétés d'un objet
        /// </summary>
        public static Dictionary<string, object> ObjectToDictionary(object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
}
