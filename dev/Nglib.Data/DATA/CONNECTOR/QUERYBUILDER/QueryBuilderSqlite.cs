using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// QueryBuilder spécifique pour SQLite
    /// </summary>
    public class QueryBuilderSqlite : QueryBuilderBase, IQueryBuilder
    {
        public override string EngineName => "sqlite";
        
        // Pas de spécificités : utilise les implémentations par défaut de QueryBuilderBase
        // (LIMIT ... OFFSET ... est la syntaxe standard)
    }
}
