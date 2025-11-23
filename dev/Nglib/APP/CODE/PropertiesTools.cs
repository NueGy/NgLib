using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nglib.FORMAT;

namespace Nglib.APP.CODE
{
    /// <summary>
    /// Tools for manipulating object properties and fields using reflection.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_appcode"/></para>
    /// </summary>
    public static class PropertiesTools
    {
        /// <summary>
        /// Gets all declared properties (accessors) of a type.
        /// </summary>
        /// <param name="potype">The type to inspect</param>
        /// <param name="OnlyPublic">If true, returns only public properties</param>
        /// <returns>Array of PropertyInfo</returns>
        public static PropertyInfo[] GetProperties(Type potype, bool OnlyPublic = true)
        {
            if(potype == null) return null;
            if(OnlyPublic)
                return potype.GetProperties();
            else 
                return potype.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }


        /// <summary>
        /// Gets the property (accessor) of an object by name (case-insensitive).
        /// </summary>
        /// <param name="objSrc">The source object</param>
        /// <param name="propertyName">The property name</param>
        /// <returns>PropertyInfo or null if not found</returns>
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
        /// Gets the field (variable) of an object by name (case-insensitive).
        /// </summary>
        /// <param name="objSrc">The source object</param>
        /// <param name="memberName">The field name</param>
        /// <returns>FieldInfo or null if not found</returns>
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
        /// Gets the value from an object property or field, even if private.
        /// </summary>
        /// <param name="objSrc">The source object</param>
        /// <param name="propertyName">The property or field name</param>
        /// <param name="safe">If true, returns null instead of throwing exceptions</param>
        /// <returns>The property/field value</returns>
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
        /// Gets all properties and their values from an object as a dictionary.
        /// </summary>
        /// <param name="objScr">The source object</param>
        /// <param name="bindingAttr">Binding flags to filter properties</param>
        /// <returns>Dictionary with property names as keys and values as objects</returns>
        public static Dictionary<string, object> GetValues(object objScr, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return objScr.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(objScr, null)
            );
        }



        /// <summary>
        /// Gets the value from an object and converts it to string (SAFE mode).
        /// </summary>
        /// <param name="objSrc">The source object</param>
        /// <param name="propertyName">The property name</param>
        /// <returns>String representation of the value or null</returns>
        public static string GetString(object objSrc, string propertyName)
        {
            var obj = GetValue(objSrc, propertyName, true);
            if (obj == null) return null;
            return obj.ToString();
        }






        /// <summary>
        /// Updates a property value in an object with automatic type conversion.
        /// </summary>
        /// <param name="objDest">The destination object</param>
        /// <param name="propertyInfo">The PropertyInfo to update</param>
        /// <param name="value">The new value</param>
        /// <param name="index">Optional index parameters</param>
        public static void SetValue(object objDest, PropertyInfo propertyInfo, object value, object[] index = null)
        {
            if (objDest == null) throw new ArgumentNullException("objSrc");
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            try
            {
                object realvalue = ConvertTools.ChangeType(value, propertyInfo.PropertyType);
                propertyInfo.SetValue(objDest, realvalue, index);
            }
            catch (Exception ex)
            {
                throw new Exception($"SetValueReflexion({propertyInfo.Name}) {ex.Message}", ex);
            }
        }





        /// <summary>
        /// Updates a property value in an object by name with automatic type conversion.
        /// </summary>
        /// <param name="objDest">The destination object</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="value">The new value</param>
        /// <param name="index">Optional index parameters</param>
        public static void SetValue(object objDest, string propertyName, object value, object[] index = null)
        {
            if (objDest == null) throw new ArgumentNullException("objDest");
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException("propertyName");
            var prop = GetProperty(objDest, propertyName);
            if (prop == null) throw new Exception($"Property {propertyName} not found in {objDest.GetType().Name}");
            SetValue(objDest, prop, value, index);
        }


        /// <summary>
        /// Updates an object from a dictionary with automatic type conversion.
        /// </summary>
        /// <param name="objDest">The destination object to update</param>
        /// <param name="values">Dictionary containing property names and values</param>
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
                    object realvalue = ConvertTools.ChangeType(itemval.Value, proinfo.PropertyType);
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