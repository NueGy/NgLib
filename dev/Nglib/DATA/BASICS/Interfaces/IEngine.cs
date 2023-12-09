using System;

namespace Nglib.DATA.BASICS
{
    /// <summary>
    ///     Moteur
    /// </summary>
    public interface IEngine : IDisposable
    {
        void Open();
        void Close();
    }
}