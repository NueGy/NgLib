using System;
using System.IO;


namespace Nglib.APP.CODE
{
    /// <summary>
    ///     Outils de reflexions types...
    /// </summary>
    public static class ReflectionTools
    {
        /// <summary>
        ///    Obtient un type, IgnoreCase
        /// </summary>
        /// <param name="MyFullyQualifiedTypeName"></param>
        /// <returns></returns>
        public static Type GetType(string MyFullyQualifiedTypeName)
        {
            Type retour = null;
            retour = Type.GetType(MyFullyQualifiedTypeName, false,true);
            return retour;
        }

        /// <summary>
        /// Create Instance of new object
        /// </summary>
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
        /// Create Instance of new object
        /// </summary>
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


//        public static long GetSizeOfObject(object obj)
//        {
//#pragma warning disable SYSLIB0011
//            long size = 0;
//            var o = new object();
//            // voir aussi : https://social.msdn.microsoft.com/Forums/vstudio/en-US/96747ab7-7d89-4846-9e83-46f71b8ccc66/how-to-determine-size-of-the-c-object?forum=clr
//            using (Stream s = new MemoryStream())
//            {
//                var formatter = new BinaryFormatter();
//                formatter.Serialize(s, o);
//                size = s.Length;
//            }

//            return size;
//#pragma warning restore SYSLIB0011
//        }
    }
}