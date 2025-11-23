using System;
using System.IO;


namespace Nglib.APP.CODE
{
    /// <summary>
    /// Tools for type reflection and instance creation.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_appcode"/></para>
    /// </summary>
    public static class ReflectionTools
    {
        /// <summary>
        /// Gets a type by its fully qualified name (case-insensitive).
        /// </summary>
        /// <param name="MyFullyQualifiedTypeName">The fully qualified type name</param>
        /// <returns>The Type object or null if not found</returns>
        public static Type GetType(string MyFullyQualifiedTypeName)
        {
            Type retour = null;
            retour = Type.GetType(MyFullyQualifiedTypeName, false,true);
            return retour;
        }

        /// <summary>
        /// Creates a new instance of the specified type with optional constructor arguments.
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <param name="exactType">The exact type to instantiate (if different from T)</param>
        /// <param name="constructorArgs">Constructor arguments</param>
        /// <returns>A new instance of type T</returns>
        public static T CreateInstance<T>(Type exactType=null, params object[] constructorArgs)
        {
            try
            {
                if(exactType==null) exactType = typeof(T);
                if (constructorArgs != null && constructorArgs.Length > 0)
                    return (T)Activator.CreateInstance(exactType, constructorArgs);
                return (T)Activator.CreateInstance(exactType);
            }
            catch (Exception ex)
            {
                throw new Exception($"CreateInstance {ex.Message} (type:{typeof(T).Name})", ex);
            }
        }

        /// <summary>
        /// Creates a new instance of the specified type with optional constructor arguments.
        /// </summary>
        /// <param name="type">The type to instantiate</param>
        /// <param name="constructorArgs">Constructor arguments</param>
        /// <returns>A new instance of the specified type</returns>
        public static object CreateInstance(Type type, params object[] constructorArgs)
        {
            try
            {
                if (constructorArgs != null && constructorArgs.Length > 0)
                    return Activator.CreateInstance(type, constructorArgs);
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new Exception($"CreateInstance {ex.Message} (type:{type.Name})", ex);
            }
        }

        /// <summary>
        /// Gets the version of the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to get the version from</param>
        /// <param name="fullVersion">If true, returns the full version (e.g., "1.0.0.0"), otherwise returns the major and minor version (e.g., "1.0")</param>
        public static string GetVersion(System.Reflection.Assembly assembly, bool fullVersion = false)
        {
            if (assembly == null) return null;
            string version = assembly.GetName().Version.ToString();
            string[] parts = version.Split('.');
            if (!fullVersion && parts.Length > 2)
            {
                return $"{parts[0]}.{parts[1]}";
            }
            return version;
        }
    }
}