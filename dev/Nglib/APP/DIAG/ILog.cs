using System;

namespace Nglib.APP.LOG
{
    public interface ILog
    {
        string LogText { get; }

        int LogLevel { get; }

        DateTime DateCreate { get; }
    }
}