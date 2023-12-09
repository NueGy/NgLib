using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.APP.LOG
{
    public class LogModel : ILog
    {
        public string LogText { get; set; }

        public int LogLevel { get; set; }

        public DateTime DateCreate { get; set; }
    }
}
