using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.MODELS.BASICS
{

    /// <summary>
    /// Moteur
    /// </summary>
    public interface IEngine : IDisposable
    {

        void Open();
        void Close();

    }
}
