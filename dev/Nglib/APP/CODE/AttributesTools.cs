using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nglib.APP.CODE
{
    /// <summary>
    ///     Outils pour la manipulation d'attributs
    /// </summary>
    public class AttributesTools
    {


        /// <summary>
        /// Obtenir un attribut depuis un objet
        /// </summary>
        public static Tattribute GetAttribute<Tattribute>(object objClass) where Tattribute : Attribute
        {
            if (objClass == null) return null;
            return GetAttribute<Tattribute>(objClass.GetType());
        }

        /// <summary>
        /// Obtenir un attribut depuis une classe
        /// </summary>
        public static Tattribute GetAttribute<Tattribute>(Type objClassType) where Tattribute : Attribute
        {
            if (objClassType == null) return null;
            var attributes = objClassType.GetCustomAttributes().ToList();
            ;
            if (attributes == null) return null;
            var typeAttributeWant = typeof(Tattribute);
            return attributes.FirstOrDefault(at => at.GetType().Equals(typeAttributeWant)) as Tattribute;
        }

        /// <summary>
        /// Obtenir un attribut depuis une classe
        /// </summary>
        public static Attribute GetAttribute(Type objClassType, Type typeAttributeWant)
        {
            if (objClassType == null) return null;
            var attributes = objClassType.GetCustomAttributes().ToList();
            ;
            if (attributes == null) return null;
            return attributes.FirstOrDefault(at => at.GetType().Equals(typeAttributeWant));
        }


        /// <summary>
        /// Liste des membres d'une classe/type avec cet attribut
        /// </summary>
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
        /// Liste des méthodes d'une classe avec cet attribut
        /// </summary>
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
        /// Liste des propriétés d'une classe avec cet attribut
        /// </summary>
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
        /// Liste des valeurs des propriétés avec cet attribut
        /// </summary>
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
        ///     Lister tous les types avec cet attribut sur toutes les assemblies dans CurrentDomain
        /// </summary>
        public static Dictionary<Type, Tattribute> GetTypesWithAttribute<Tattribute>(string typeNamePrefix = null)
            where Tattribute : Attribute
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            return assemblies.SelectMany(assembly => GetTypesWithAttribute<Tattribute>(assembly, typeNamePrefix)).ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        ///     Lister tous les types avec cet attribut sur une assembly
        /// </summary>
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