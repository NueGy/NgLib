using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.APP.CODE
{
    /// <summary>
    /// Outils de reflexions types...
    /// </summary>
    public static class ReflectionTools
    {
 




        public static Type GetTypeByReflexion(string MyFullyQualifiedTypeName)
        {
            Type retour = null;
            retour = Type.GetType(MyFullyQualifiedTypeName, false, true);

            /*
            

            List<System.Reflection.Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var findAssembly = assemblies.FirstOrDefault(a => a.FullName.StartsWith("System.Data,"));
            List<Type> alltypes = findAssembly.GetTypes().ToList();

                 //.Where(x => typeof(IDomainEntity).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                 //.Select(x => x.Name).ToList();
            var sel = alltypes.Where(t => t.Name.StartsWith("System.Data.OleDb.OleDbConnection", StringComparison.OrdinalIgnoreCase));
                */
            return retour;
        }



        public static T NewInstance<T>(Type type)
        {
            return (T) NewInstance(type);
        }
        public static object NewInstance(Type type)
        {
            return  Activator.CreateInstance(type);
        }


  








     }
}
