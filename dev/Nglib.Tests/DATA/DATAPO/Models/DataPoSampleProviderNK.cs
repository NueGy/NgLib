using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    public class DataPoSampleProviderNK : DATA.DATAPO.DataPOProviderSQL<DataPoSampleNK>
    {
        public DataPoSampleProviderNK(DATA.CONNECTOR.IDataConnector connector) : base(connector)
        {

        }

     
    }
}
