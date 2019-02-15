using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    public class DataPoSampleProvider : DATA.DATAPO.DataPOProviderSQL<DataPoSample>
    {
        public DataPoSampleProvider(DATA.CONNECTOR.IDataConnector connector) : base(connector)
        {

        }

     
    }
}
