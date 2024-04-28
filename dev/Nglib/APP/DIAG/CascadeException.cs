using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.DIAG
{
    /// <summary>
    /// Exception Générique pour les erreurs de l'application
    /// </summary>
    public class CascadeException : Exception
    {

 

        public CascadeException(string methodName,  Exception innerException)
            : base(RecomposeExceptionMessage(methodName, innerException), GetOriginalException(innerException))
        {

        }

        private static string RecomposeExceptionMessage(string methodName,  Exception ex)
        {
            if(methodName==null && ex == null)
            {

            }
            string msg = $"{methodName}:{ex.Message}";
            return msg;
        }


        private static Exception GetOriginalException( Exception innerEx)
        {
            if(innerEx==null)return null;
            if(!(innerEx is CascadeException)) return innerEx;
            // innerEx is CascadeException alors on cherche la vrai exception d'origine
            return innerEx.InnerException;
        }

    }
}
