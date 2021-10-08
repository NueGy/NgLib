using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.APP.CODE
{
    /// <summary>
    /// outils pour la manipulation d'attribut
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


        /// <summary>
        /// Permet d'obtenir toute les descriptions sur une classe
        /// </summary>
        /// <param name="objClassType"></param>
        /// <returns></returns>
        public static Dictionary<string,string> GetPropertiesDescriptions(Type objClassType)
        {
            Dictionary<string, string> retour = new Dictionary<string, string>();
            foreach (PropertyInfo property in objClassType.GetProperties())
            {
                var descr = property.GetCustomAttribute(typeof(System.ComponentModel.DescriptionAttribute));
                string descrtxt = (descr != null) ? ((System.ComponentModel.DescriptionAttribute)descr).Description : null;
                retour.Add(property.Name, descrtxt);
            }

            return retour;  
        }



        // Vérifier ???
        internal static string GetStringFromAttribute(PropertyInfo property, Type typeAttributeWant, string valueNameWant)
        {
            System.Attribute attribute = property.GetCustomAttributes(typeAttributeWant).FirstOrDefault();
            if (attribute == null) return null;
            return PropertiesTools.GetStringReflexion(attribute, valueNameWant);
        }



        public static System.Attribute FindObjectAttribute(object objClass, Type typeAttributeWant)
        {
            if (objClass == null) return null;
            var attributes = objClass.GetType().GetCustomAttributes().Cast<System.Attribute>().ToList(); ;
            if (attributes == null) return null;
            return attributes.FirstOrDefault(at => (at.GetType().Equals(typeAttributeWant)));
        }
        public static System.Attribute FindObjectAttribute(Type objClassType, Type typeAttributeWant)
        {
            if (objClassType == null) return null;
            var attributes = objClassType.GetCustomAttributes().Cast<System.Attribute>().ToList(); ;
            if (attributes == null) return null;
            return attributes.FirstOrDefault(at => (at.GetType().Equals(typeAttributeWant)));
        }



        public static Tattribute FindObjectAttribute<Tattribute>(object objClass) where Tattribute : System.Attribute
        {
            if (objClass == null) return null;
            return FindObjectAttribute<Tattribute>(objClass.GetType());
        }

        public static Tattribute FindObjectAttribute<Tattribute>(Type objClassType) where Tattribute : System.Attribute
        {
            if (objClassType == null) return null;
            var attributes = objClassType.GetCustomAttributes().Cast<System.Attribute>().ToList(); ;
            if (attributes == null) return null;
            Type typeAttributeWant = typeof(Tattribute);
            return attributes.FirstOrDefault(at => (at.GetType().Equals(typeAttributeWant))) as Tattribute;
        }


        public static Dictionary<string, Tuple<Tattribute, object>> FindPropertiesValueAttribute<Tattribute>(object model) where Tattribute : System.Attribute
        {
            if (model == null) return null;
            Dictionary<string, Tuple<Tattribute, object>> retour = new Dictionary<string, Tuple<Tattribute, object>>();
            Type modeltype = model.GetType();
            Type attributetype = typeof(Tattribute);
            foreach (PropertyInfo property in modeltype.GetProperties())
            {
                Tattribute attr = property.GetCustomAttribute(attributetype) as Tattribute;
                if (attr == null) continue;
                object val = property.GetValue(model, null);
                retour.Add(property.Name, new Tuple<Tattribute, object>(attr, val) );
            }

            return retour;
        }


    }
}
