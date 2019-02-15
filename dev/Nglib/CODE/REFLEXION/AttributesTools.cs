using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.CODE.REFLEXION
{
    /// <summary>
    /// outils pour la manipulation d'attribut
    /// </summary>
    public class AttributesTools
    {



        /// <summary>
        /// Savoir si la propriété existe
        /// </summary>
        /// <param name="type"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static System.Attribute FindAttribute(PropertyInfo property, Type typeAttributeWant)
        {
            ICollection<System.Attribute> attributes = GetDeclaredAttributes(property);
            if (attributes == null) return null;
            return attributes.FirstOrDefault(at => (at.GetType().Equals(typeAttributeWant)));
        }
        public static System.Attribute FindAttribute(ICollection<System.Attribute> attributes, Type typeAttributeWant)
        {
            if (attributes == null) return null;
            return attributes.FirstOrDefault(at => (at.GetType().Equals(typeAttributeWant)));
        }


        /// <summary>
        /// Obtient la valeur d'un element dans un attribut
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valuename"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static string FindAttributeGetString(ICollection<System.Attribute> attributes, Type typeAttributeWant, string valueNameWant)
        {
            System.Attribute attribute = FindAttribute(attributes, typeAttributeWant);
            if (attribute == null) return null;
            return ReflexionTools.GetPropertyString(attribute, valueNameWant);
        }


        public static string FindAttributeGetString(PropertyInfo property, Type typeAttributeWant, string valueNameWant)
        {
            System.Attribute attribute = FindAttribute(property, typeAttributeWant);
            if (attribute == null) return null;
            return ReflexionTools.GetPropertyString(attribute, valueNameWant);
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





        /// <summary>
        /// Obtient toute les propriété déclarées et les valeurs
        /// </summary>
        /// <returns></returns>
        public static List<System.Attribute> GetDeclaredAttributes(PropertyInfo property)
        {
            if (property == null) return null;
            List<System.Attribute> propAttributes = property.GetCustomAttributes(true).Cast<System.Attribute>().ToList();
            return propAttributes;
        }


    }
}
