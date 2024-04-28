using System;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    ///     Error
    /// </summary>
    public class DataAccessorException : Exception
    {
        public DataAccessorException(string message, Exception innerex) : base(message, innerex)
        {
        }

        public DataAccessorException(string message) : base(message)
        {
              
        }
    }
}