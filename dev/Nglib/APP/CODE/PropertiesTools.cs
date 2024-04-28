using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nglib.DATA.KEYVALUES;
using Nglib.FORMAT;

namespace Nglib.APP.CODE
{
    /// <summary>
    ///     Manipulation de valeurs dans un objet
    /// </summary>
    public static class PropertiesTools
    {
        /// <summary>
        ///     Obtient toutes les propriétés(Accesseurs) déclarées
        /// </summary>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(Type potype, bool OnlyPublic = true)
        {
            if(potype == null) return null;
            if(OnlyPublic)
                return potype.GetProperties();
            else 
                return potype.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }


        /// <summary>
        ///     Obtient la property(Accesseurs) d'un objet
        /// </summary>
        public static PropertyInfo GetProperty(object objSrc, string propertyName)
        {
            if (objSrc == null || string.IsNullOrWhiteSpace(propertyName)) return null;
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var prop = objSrc.GetType().GetProperty(propertyName, bindFlags);
            if (prop == null)
                prop = objSrc.GetType().GetProperties(bindFlags)
                    .FirstOrDefault(p =>
                        propertyName.Equals(p.Name,
                            StringComparison.OrdinalIgnoreCase)); // onréessaye en mode case nosensitive
            return prop;
        }

        /// <summary>
        ///     Obtient la property(Variable) d'un objet
        /// </summary>
        public static FieldInfo GetField(object objSrc, string memberName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            if (objSrc == null || string.IsNullOrWhiteSpace(memberName)) return null;
            var prop = objSrc.GetType().GetField(memberName, bindFlags);
            if (prop == null)
                prop = objSrc.GetType().GetFields(bindFlags)
                    .FirstOrDefault(p =>memberName.Equals(p.Name,StringComparison.OrdinalIgnoreCase)); // onréessaye en mode case nosensitive
            return prop;
        }   





        /// <summary>
        ///     Obtenir la données dans un objet, Meme si private
        /// </summary>
        public static object GetValue(object objSrc, string propertyName, bool safe = false)
        {
            if (objSrc == null)
            {
                if (safe) return null;
                else throw new ArgumentNullException("objSrc");
            }
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                if (safe) return null;
                else throw new ArgumentNullException("propertyName");
            }
            var prop = GetProperty(objSrc, propertyName);
            if (prop != null) return prop.GetValue(objSrc, null);

            var prfl = GetField(objSrc, propertyName);
            if (prfl != null) return prfl.GetValue(objSrc);

            if (safe) return null;
            else throw new Exception($"PropertiesTools.GetValue: {propertyName} not found in {objSrc.GetType().Name}");
        }


        /// <summary>
        ///     Obtenir les données d'un objet dans un dictionary
        /// </summary>
        public static Dictionary<string, object> GetValues(object objScr, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return objScr.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(objScr, null)
            );
        }



        /// <summary>
        ///     Obtenir la données et la convertir en string (SAFE)
        /// </summary>
        public static string GetString(object objSrc, string propertyName)
        {
            var obj = GetValue(objSrc, propertyName, true);
            if (obj == null) return null;
            return obj.ToString();
        }






        /// <summary>
        ///     Permet de mettre à jours une veleur dans un objet
        ///     Réalisera une conversion de la valeur si nécessaire
        /// </summary>
        public static void SetValue(object objDest, PropertyInfo propertyInfo, object value, object[] index = null)
        {
            if (objDest == null) throw new ArgumentNullException("objSrc");
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            try
            {
                object realvalue = ConvertPlus.ChangeType(value, propertyInfo.PropertyType);
                propertyInfo.SetValue(objDest, realvalue, index);
            }
            catch (Exception ex)
            {
                throw new Exception($"SetValueReflexion({propertyInfo.Name}) {ex.Message}", ex);
            }
        }





        /// <summary>
        ///     Permet de mettre à jours une valeur dans un objet
        ///     Réalisera un conversion de la valeur si nécessaire
        /// </summary>
        public static void SetValue(object objDest, string propertyName, object value, object[] index = null)
        {
            if (objDest == null) throw new ArgumentNullException("objDest");
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException("propertyName");
            var prop = GetProperty(objDest, propertyName);
            if (prop == null) throw new Exception($"Property {propertyName} not found in {objDest.GetType().Name}");
            SetValue(objDest, prop, value, index);
        }


        /// <summary>
        ///     Mettre à jours un objet à partir d'un dictionary
        /// </summary>
        public static void SetValues(object objDest, IDictionary<string, object> values)
        {
            if (objDest == null) throw new ArgumentNullException("objDest");
            if (values == null) return;
            try
            {
                var someObjectType = objDest.GetType();
                var properties =
                    someObjectType.GetProperties().Where(p => p.CanWrite).ToArray(); // !!! Filtrer les types impossibles

                foreach (var proinfo in properties)
                {
                    var itemval = values.FirstOrDefault(d => proinfo.Name.Equals(d.Key, StringComparison.OrdinalIgnoreCase));
                    if (itemval.Key == null) continue;
                    object realvalue = ConvertPlus.ChangeType(itemval.Value, proinfo.PropertyType);
                    proinfo.SetValue(objDest, realvalue, null); //!!! améliorer : Gérer les cast automatiquement
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"SetValuesReflexion {ex.Message}", ex);
            }
        }



    }
}