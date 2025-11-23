using System.Collections.Generic;
using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.KEYVALUES
{
    public interface IkeyValues : IList<KeyValue>, IDataAccessor
    {
        List<KeyValue> GetDatas(string key, bool StartWith = false);
    }
}