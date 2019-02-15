using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.APP.LOG
{
    public interface ILog
    {

        string LogText { get; }

        int LogLevel { get; }

        DateTime DateCreate { get; }

    }
}
