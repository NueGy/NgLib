using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Nglib.DATA.CONNECTOR
{
    /// <summary>
    /// Liste de connecteurs SQL
    /// </summary>
    public class ConnectorCollection : List<IDataConnector>
    {
        /// <summary>
        /// Obtient le connecteur principal (DefaultConnection)
        /// </summary>
        /// <returns></returns>
        public IDataConnector GetDefaultConnector()
        {
            IDataConnector master = this.FirstOrDefault(c => !c.ReadOnly); 
            if(master==null) master = this.FirstOrDefault();
            return master;
        }



    }
}
