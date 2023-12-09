using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Liste de dataPO
    /// </summary>
    public interface ICollectionPO
    {

       void LoadFromDataTable(System.Data.DataTable table);

        Type GetPOType();



        List<DataPO> GetPOList();

    }
}
