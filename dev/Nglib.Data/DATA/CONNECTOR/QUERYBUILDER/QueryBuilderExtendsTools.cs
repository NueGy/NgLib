using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    public static class QueryBuilderExtendsTools
    {
        public static IQueryBuilder SelectAll(this IQueryBuilder builder) => builder.Select();

        public static IQueryBuilder InnerJoin(this IQueryBuilder builder, string table, string condition) => builder.Join(table, condition, JoinTypeEnum.Inner);

        public static IQueryBuilder LeftJoin(this IQueryBuilder builder, string table, string condition) => builder.Join(table, condition, JoinTypeEnum.Left);

        public static IQueryBuilder RightJoin(this IQueryBuilder builder, string table, string condition) => builder.Join(table, condition, JoinTypeEnum.Right);

        public static IQueryBuilder WhereEqual(this IQueryBuilder builder, string column, object value) => builder.Where(column, "=", value);

        public static IQueryBuilder WhereNotEqual(this IQueryBuilder builder, string column, object value) => builder.Where(column, "!=", value);

        public static IQueryBuilder WhereGreater(this IQueryBuilder builder, string column, object value) => builder.Where(column, ">", value);

        public static IQueryBuilder WhereLess(this IQueryBuilder builder, string column, object value) => builder.Where(column, "<", value);

        public static IQueryBuilder WhereLike(this IQueryBuilder builder, string column, string pattern) => builder.Where(column, "LIKE", pattern);

        public static IQueryBuilder WhereNotIn(this IQueryBuilder builder, string column, IEnumerable<object> values) => builder.WhereIn(column, values, true);

        public static IQueryBuilder WhereNotNull(this IQueryBuilder builder, string column) => builder.WhereNull(column, true);

        public static IQueryBuilder WhereBetweenDate(this IQueryBuilder builder, string column, DateTime? dateMin, DateTime? dateMax) => builder.WhereBetween(column, dateMin, dateMax);

        public static IQueryBuilder Paginate(this IQueryBuilder builder, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var offset = (page - 1) * pageSize;
            return builder.Limit(pageSize, offset);
        }
    }
}