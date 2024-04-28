using System.Data;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    ///     I DATAPO
    /// </summary>
    public interface IDataPO
    {
        DataRow GetRow(bool RefreshFlow = true);
        void SetRow(DataRow row);
        DataTable InitSchema();
    }
}