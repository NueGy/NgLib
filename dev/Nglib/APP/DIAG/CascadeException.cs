using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.DIAG
{
    /// <summary>
    /// Generic exception for application errors with method context tracing.
    /// Preserves the original exception by avoiding CascadeException nesting.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_appdiag"/></para>
    /// </summary>
    public class CascadeException : Exception
    {
        /// <summary>
        /// Creates a new CascadeException with method context.
        /// </summary>
        /// <param name="methodName">The method name where the exception occurred</param>
        /// <param name="innerException">The original exception</param>
        public CascadeException(string methodName,  Exception innerException)
            : base(RecomposeExceptionMessage(methodName, innerException), GetOriginalException(innerException))
        {

        }

        /// <summary>
        /// Composes the exception message with method context.
        /// </summary>
        private static string RecomposeExceptionMessage(string methodName,  Exception ex)
        {
            if(ex == null)
                throw new ArgumentNullException(nameof(ex));
            
            string msg = $"{methodName}:{ex.Message}";
            return msg;
        }

        /// <summary>
        /// Extracts the original exception, unwrapping CascadeException if necessary.
        /// </summary>
        private static Exception GetOriginalException( Exception innerEx)
        {
            if(innerEx == null) return null;
            if(!(innerEx is CascadeException)) return innerEx;
            // innerEx is CascadeException, so we search for the real original exception
            return innerEx.InnerException;
        }

    }
}