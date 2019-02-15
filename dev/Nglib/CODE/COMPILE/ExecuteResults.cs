using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Nglib.CODE.COMPILE
{
    public class ExecuteResults
    {

        public DateTime DateExecute { get; set; }

        public System.Diagnostics.Stopwatch TimeExecute { get; set; }

        public string SourceCodeName { get; set; }

    }
}
