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
        public const string DefaultConnectionStr = "DefaultConnection";

        /// <summary>
        /// Obtient le connecteur principal (DefaultConnection)
        /// </summary>
        /// <returns></returns>
        public IDataConnector GetDefaultConnector()
        {
            IDataConnector master = this.FirstOrDefault(c => DefaultConnectionStr.Equals(c.ConnectorName, StringComparison.OrdinalIgnoreCase));
            if(master==null) master = this.FirstOrDefault(c => !c.ReadOnly);
            return master;
        }



    }
}
