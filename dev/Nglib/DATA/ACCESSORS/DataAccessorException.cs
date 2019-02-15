using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Error
    /// </summary>
    public class DataAccessorException : System.Exception
    {
        public DataAccessorException(string message, System.Exception innerex) : base(message, innerex) { }
        public DataAccessorException(string message) : base(message) { }
    }
}
