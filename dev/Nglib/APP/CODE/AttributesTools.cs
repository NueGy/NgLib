using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nglib.APP.CODE
{
    /// <summary>
    /// Tools for manipulating and extracting attributes from classes and members.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_appcode"/></para>
    /// </summary>
    public class AttributesTools
    {


        /// <summary>
        /// Gets an attribute from an object instance.
        /// </summary>
        /// <typeparam name="Tattribute">The attribute type to retrieve</typeparam>
        /// <param name="objClass">The object instance</param>
        /// <returns>The attribute or null if not found</returns>
        public static Tattribute GetAttribute<Tattribute>(object objClass) where Tattribute : Attribute
        {
            if (objClass == null) return null;
            return GetAttribute<Tattribute>(objClass.GetType());
        }

        /// <summary>
        /// Gets an attribute from a class type.
        /// </summary>
        /// <typeparam name="Tattribute">The attribute type to retrieve</typeparam>
        /// <param name="objClassType">The class type</param>
        /// <returns>The attribute or null if not found</returns>
        public static Tattribute GetAttribute<Tattribute>(Type objClassType) where Tattribute : Attribute
        {
            if (objClassType == null) return null;
            var attributes = objClassType.GetCustomAttributes().ToList();
            if (attributes == null) return null;
            var typeAttributeWant = typeof(Tattribute);
            return attributes.FirstOrDefault(at => at.GetType().Equals(typeAttributeWant)) as Tattribute;
        }

        /// <summary>
        /// Gets an attribute from a class type (non-generic version).
        /// </summary>
        /// <param name="objClassType">The class type</param>
        /// <param name="typeAttributeWant">The desired attribute type</param>
        /// <returns>The attribute or null if not found</returns>
        public static Attribute GetAttribute(Type objClassType, Type typeAttributeWant)
        {
            if (objClassType == null) return null;
            var attributes = objClassType.GetCustomAttributes().ToList();
            if (attributes == null) return null;
            return attributes.FirstOrDefault(at => at.GetType().Equals(typeAttributeWant));
        }


        /// <summary>
        /// Gets all members of a class/type that have a specific attribute.
        /// </summary>
        /// <typeparam name="Tattribute">The attribute type to search for</typeparam>
        /// <param name="modeltype">The type to inspect</param>
        /// <param name="memberTypes">The types of members to include (default: All)</param>
        /// <returns>Dictionary with MemberInfo as key and attribute as value</returns>
        public static IDictionary<MemberInfo, Tattribute> GetMembersWithAttribute<Tattribute>(Type modeltype, MemberTypes memberTypes = MemberTypes.All)
         where Tattribute : Attribute
        {
            if (modeltype == null) return null;
            var attributetype = typeof(Tattribute);
            var retour = new Dictionary<MemberInfo, Tattribute>();
            foreach (var property in modeltype.GetMembers().Where(m=> memberTypes.HasFlag(m.MemberType)))
            {
                var attr = property.GetCustomAttribute(attributetype) as Tattribute;
                if (attr == null) continue;
                retour.Add(property, attr);
            }
            return retour;
        }


        /// <summary>
        /// Gets all methods of a class that have a specific attribute.
        /// </summary>
        /// <typeparam name="Tattribute">The attribute type to search for</typeparam>
        /// <param name="modeltype">The type to inspect</param>
        /// <returns>Dictionary with MethodInfo as key and attribute as value</returns>
        public static Dictionary<MethodInfo, Tattribute> GetMethodsWithAttribute<Tattribute>(Type modeltype)
            where Tattribute : Attribute
            //=> GetMembersAttributes<Tattribute>(modeltype, MemberTypes.Method).ToDictionary(d=> d.Key as MethodInfo, d=> d.Value);
        {
            if (modeltype == null) return null;
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



        /// <summary>
        /// Gets all properties of a class that have a specific attribute.
        /// </summary>
        /// <typeparam name="Tattribute">The attribute type to search for</typeparam>
        /// <param name="modeltype">The type to inspect</param>
        /// <returns>Dictionary with PropertyInfo as key and attribute as value</returns>
        public static Dictionary<PropertyInfo,Tattribute> GetPropertiesWithAttribute<Tattribute>(Type modeltype) where Tattribute : Attribute
        {
            if (modeltype == null) return null;
            var retour = new Dictionary<PropertyInfo, Tattribute>();
            var attributetype = typeof(Tattribute);
            foreach (var property in modeltype.GetProperties())
            {
                var attr = property.GetCustomAttribute(attributetype) as Tattribute;
                if (attr == null) continue;
                retour.Add( property,attr);
            }
            return retour;
        }





        /// <summary>
        /// Gets property values from an object where properties have a specific attribute.
        /// </summary>
        /// <typeparam name="Tattribute">The attribute type to search for</typeparam>
        /// <param name="model">The object instance</param>
        /// <returns>Dictionary with attribute as key and property value as value</returns>
        public static Dictionary<Tattribute, object> GetValuesWithAttribute<Tattribute>(object model) where Tattribute : Attribute
        {
            if (model == null) return null;
            var retour = new Dictionary<Tattribute, object>();
            var modeltype = model.GetType();
            var attributetype = typeof(Tattribute);
            foreach (var property in modeltype.GetProperties())
            {
                var attr = property.GetCustomAttribute(attributetype) as Tattribute;
                if (attr == null) continue;
                var val = property.GetValue(model, null);
                retour.Add(attr, val);
            }
            return retour;
        }









   


        /// <summary>
        /// Lists all types with a specific attribute from all assemblies in the current domain.
        /// </summary>
        /// <typeparam name="Tattribute">The attribute type to search for</typeparam>
        /// <param name="typeNamePrefix">Optional prefix to filter type names</param>
        /// <returns>Dictionary with Type as key and attribute as value</returns>
        public static Dictionary<Type, Tattribute> GetTypesWithAttribute<Tattribute>(string typeNamePrefix = null)
            where Tattribute : Attribute
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            return assemblies.SelectMany(assembly => GetTypesWithAttribute<Tattribute>(assembly, typeNamePrefix)).ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Lists all types with a specific attribute from a single assembly.
        /// </summary>
        /// <typeparam name="Tattribute">The attribute type to search for</typeparam>
        /// <param name="assembly">The assembly to inspect</param>
        /// <param name="typeNamePrefix">Optional prefix to filter type names</param>
        /// <returns>Dictionary with Type as key and attribute as value</returns>
        public static Dictionary<Type, Tattribute> GetTypesWithAttribute<Tattribute>(Assembly assembly, string typeNamePrefix = null)
             where Tattribute : Attribute
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            var typeTattribute = typeof(Tattribute);
            var alltypes = assembly.GetTypes().Where(type => type.IsClass).ToList();
            if (!string.IsNullOrEmpty(typeNamePrefix))
                alltypes = alltypes.Where(a => a.FullName.StartsWith(typeNamePrefix)).ToList();
            var seltypes = alltypes.Where(type => type.IsDefined(typeTattribute, false));
            return seltypes.Distinct().ToDictionary(type => type, type => type.GetCustomAttribute<Tattribute>());
        }








        // Vérifier ??? to delete
        [Obsolete("Use PropertiesTools.GetString instead")]
        internal static string GetStringFromAttribute(PropertyInfo property, Type typeAttributeWant,
            string valueNameWant)
        {
            var attribute = property.GetCustomAttributes(typeAttributeWant).FirstOrDefault();
            if (attribute == null) return null;
            return PropertiesTools.GetString(attribute, valueNameWant);
        }








    }
}