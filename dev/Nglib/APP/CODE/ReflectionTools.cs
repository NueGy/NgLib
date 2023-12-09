using System;
using System.IO;


namespace Nglib.APP.CODE
{
    /// <summary>
    ///     Outils de reflexions types...
    /// </summary>
    public static class ReflectionTools
    {
        public static Type GetTypeByReflexion(string MyFullyQualifiedTypeName)
        {
            Type retour = null;
            retour = Type.GetType(MyFullyQualifiedTypeName, false, true);
            return retour;
        }


        public static T NewInstance<T>(Type type, params object[] constructorArgs)
        {
            try
            {
                object objRetour = null;
                if (constructorArgs != null && constructorArgs.Length > 0)
                    objRetour = Activator.CreateInstance(type, constructorArgs);
                objRetour = Activator.CreateInstance(type);
                return (T)objRetour;
            }
            catch (Exception ex)
            {
                throw new Exception("ObjectNewInstance " + ex.Message + $"(type:{type.Name})", ex);
            }
        }

        public static object NewInstance(Type type, params object[] constructorArgs)
        {
            try
            {
                if (constructorArgs != null && constructorArgs.Length > 0)
                    return Activator.CreateInstance(type, constructorArgs);
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new Exception("ObjectNewInstance " + ex.Message + $"(type:{type.Name})", ex);
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