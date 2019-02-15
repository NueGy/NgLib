using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.CONNECTOR
{
    /// <summary>
    /// Exception Spécifique aux Connecteurs SQL (contient la requette SQL)
    /// </summary>
    public class ConnectorException : Exception
    {
        public QueryContext queryContext { get; set; }

        public ConnectorException(QueryContext queryContext, string message) : base(message) { this.queryContext = queryContext; }
        public ConnectorException(QueryContext queryContext, string message, Exception ex) : base(message, ex)  { this.queryContext = queryContext; }


        public override string ToString()
        {
            string msg = string.Format("[{0}]{1} (SQL:{2})", queryContext.QueryTry, this.Message, queryContext.sqlQuery);
            return msg;
        }


    }
}
