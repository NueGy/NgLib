using System;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Specialized exception for data accessor errors with inner exception support.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_accessors"/></para>
    /// </summary>
    public class DataAccessorException : Exception
    {
        /// <summary>
        /// Initializes a new instance with message and inner exception
        /// </summary>
        /// <param name="message">Error message describing the problem</param>
        /// <param name="innerex">Inner exception that caused this error</param>
        public DataAccessorException(string message, Exception innerex) : base(message, innerex)
        {
        }

        /// <summary>
        /// Initializes a new instance with message only
        /// </summary>
        /// <param name="message">Error message describing the problem</param>
        public DataAccessorException(string message) : base(message)
        {
              
        }
    }
}