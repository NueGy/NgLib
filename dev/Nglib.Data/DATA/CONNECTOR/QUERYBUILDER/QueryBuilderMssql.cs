using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// QueryBuilder spécifique pour Microsoft SQL Server
    /// </summary>
    public class QueryBuilderMssql : QueryBuilderBase, IQueryBuilder
    {
        public override string EngineName => "mssql";

        /// <summary>
        /// Construit la clause TOP pour SQL Server (si LIMIT sans OFFSET)
        /// </summary>
        protected override void BuildTopClause(StringBuilder sql)
        {
            // TOP si LIMIT sans OFFSET
            if (context.LimitCount.HasValue && !context.OffsetCount.HasValue)
                sql.Append($"TOP {context.LimitCount.Value} ");
        }

        /// <summary>
        /// Construit la clause de pagination pour SQL Server (OFFSET...FETCH)
        /// </summary>
        protected override void BuildPaginationClause(StringBuilder sql)
        {
            // OFFSET...FETCH pour pagination
            if (context.OffsetCount.HasValue)
            {
                if (!context.OrderByClauses.Any())
                    sql.Append(" ORDER BY (SELECT NULL)"); // SQL Server exige ORDER BY pour OFFSET

                sql.Append($" OFFSET {context.OffsetCount.Value} ROWS");

                if (context.LimitCount.HasValue)
                    sql.Append($" FETCH NEXT {context.LimitCount.Value} ROWS ONLY");
            }
        }
    }
}
