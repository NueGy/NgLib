using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nglib.APP.CODE
{
    /// <summary>
    ///     Outils pour la manipulation d'attribut
    /// </summary>
    public class AttributesTools
    {
        //public static List<System.Attribute> GetCustomAttributes(PropertyInfo property, Type attributeTypeFilter = null)
        //{
        //    if (property == null) return null;
        //    List<System.Attribute> propAttributes = property.GetCustomAttributes(true).Cast<System.Attribute>().ToList();
        //    if (attributeTypeFilter != null) propAttributes = propAttributes.Where(at => (at.GetType().Equals(attributeTypeFilter))).ToList();
        //    return propAttributes;
        //}


        ///// <summary>
        /////     Permet d'obtenir toute les descriptions sur une classe
        ///// </summary>
        ///// <param name="objClassType"></param>
        ///// <returns></returns>
        //public static Dictionary<string, string> GetPropertiesDescriptions(Type objClassType)
        //{
        //    var retour = new Dictionary<string, string>();
        //    foreach (var property in objClassType.GetProperties())
        //    {
        //        var descr = property.GetCustomAttribute(typeof(DescriptionAttribute));
        //        var descrtxt = descr != null ? ((DescriptionAttribute)descr).Description : null;
        //        retour.Add(property.Name, descrtxt);
        //    }

        //    return retour;
        //}

        public static Dictionary<MethodInfo, Tattribute> FindMethodsAttribute<Tattribute>(object model)
            where Tattribute : Attribute
        {
            if (model == null) return null;
            var modeltype = model.GetType();
            var attributetype = typeof(Tattribute);
            var retour = new Dictionary<MethodInfo, Tattribute>();
            foreach (var property in modeltype.GetMethods())
            {
                var attr = property.GetCustomAttribute(attributetype) as Tattribute;
                if (attr == null) continue;
                retour.Add(property, attr);
            }

            return retour;
        }


        // Vérifier ???
        internal static string GetStringFromAttribute(PropertyInfo property, Type typeAttributeWant,
            string valueNameWant)
        {
            var attribute = property.GetCustomAttributes(typeAttributeWant).FirstOrDefault();
            if (attribute == null) return null;
            return PropertiesTools.GetStringReflexion(attribute, valueNameWant);
        }


        public static Attribute FindObjectAttribute(object objClass, Type typeAttributeWant)
        {
            if (objClass == null) return null;
            var attributes = objClass.GetType().GetCustomAttributes().ToList();
            ;
            if (attributes == null) return null;
            return attributes.FirstOrDefault(at => at.GetType().Equals(typeAttributeWant));
        }

        public static Attribute FindObjectAttribute(Type objClassType, Type typeAttributeWant)
        {
            if (objClassType == null) return null;
            var attributes = objClassType.GetCustomAttributes().ToList();
            ;
            if (attributes == null) return null;
            return attributes.FirstOrDefault(at => at.GetType().Equals(typeAttributeWant));
        }


        public static Tattribute FindObjectAttribute<Tattribute>(object objClass) where Tattribute : Attribute
        {
            if (objClass == null) return null;
            return FindObjectAttribute<Tattribute>(objClass.GetType());
        }

        public static Tattribute FindObjectAttribute<Tattribute>(Type objClassType) where Tattribute : Attribute
        {
            if (objClassType == null) return null;
            var attributes = objClassType.GetCustomAttributes().ToList();
            ;
            if (attributes == null) return null;
            var typeAttributeWant = typeof(Tattribute);
            return attributes.FirstOrDefault(at => at.GetType().Equals(typeAttributeWant)) as Tattribute;
        }


        public static Dictionary<string, Tuple<Tattribute, object>>
            FindPropertiesValueAttribute<Tattribute>(object model) where Tattribute : Attribute
        {
            if (model == null) return null;
            var retour = new Dictionary<string, Tuple<Tattribute, object>>();
            var modeltype = model.GetType();
            var attributetype = typeof(Tattribute);
            foreach (var property in modeltype.GetProperties())
            {
                var attr = property.GetCustomAttribute(attributetype) as Tattribute;
                if (attr == null) continue;
                var val = property.GetValue(model, null);
                retour.Add(property.Name, new Tuple<Tattribute, object>(attr, val));
            }

            return retour;
        }


        /// <summary>
        ///     Lister toutes les class avec cet attribut
        /// </summary>
        /// <typeparam name="Tattribute"></typeparam>
        /// <returns></returns>
        public static Dictionary<Type, Tattribute> ListClassWithAttribute<Tattribute>(string typeNamePrefix = null)
            where Tattribute : Attribute
        {
            try
            {
                var typeTattribute = typeof(Tattribute);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();


                var alltypes = assemblies.SelectMany(assembly => assembly.GetTypes().Where(type => type.IsClass))
                    .ToList();
                if (!string.IsNullOrEmpty(typeNamePrefix))
                    alltypes = alltypes.Where(a => a.FullName.StartsWith(typeNamePrefix)).ToList();
                var seltypes = alltypes.Where(type => type.IsDefined(typeTattribute, false));
                return seltypes.Distinct().ToDictionary(type => type, type => type.GetCustomAttribute<Tattribute>());
                //var seltypes = alltypes.Distinct().ToDictionary(type => type, type => type.GetCustomAttribute<Tattribute>());//alltypes.Where(type => type.is(typeTattribute, true));
                //return seltypes.Where(t=> t.Value!=null).ToDictionary(t=> t.Key, t=> t.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("ListClassWithAttribute " + ex.Message, ex);
            }
        }
    }
}