using System;

namespace Nglib.APP.LOG
{
    /// <summary>
    /// Interface for log entries with text, level, and timestamp.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLibComponents/wiki/wiki_components_appdiag"/></para>
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// The log message text.
        /// </summary>
        string LogText { get; }

        /// <summary>
        /// The log severity level.
        /// </summary>
        int LogLevel { get; }

        /// <summary>
        /// The log creation timestamp.
        /// </summary>
        DateTime DateCreate { get; }
    }
}