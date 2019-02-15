using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// I DATAPO
    /// </summary>
    public interface IDataPO
    {
        System.Data.DataRow GetRow(bool RefreshFlow = true);
        void SetRow(System.Data.DataRow row);
        System.Data.DataTable InitSchema();
    }
}
