using System.Collections.Generic;

namespace Nglib.DATA.DATAMODEL
{
    public interface IDataModel
    {
        List<ModelValue> FormValues { get; set; }
    }
}