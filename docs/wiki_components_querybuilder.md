[◀ Back to Documentation Home](../README.md)

# QueryBuilder - SQL Query Builder

**Namespace**: `Nglib.DATA.CONNECTOR.QUERYBUILDER`

Fluent API for building multi-DBMS SQL queries with automatic parameter management and SQL injection prevention.

## Supported Databases

PostgreSQL, SQL Server (MSSQL), SQLite

## Available Methods

| Category | Method | Description |
|----------|--------|-------------|
| **Query Type** | `Select(params string[] columns)` | SELECT columns (empty = *) |
| | `SelectDistinct(params string[] columns)` | SELECT DISTINCT |
| | `SelectRaw(string rawSql)` | Raw SQL expression in SELECT |
| | `Insert(Dictionary values)` | INSERT values |
| | `Update(Dictionary values)` | UPDATE values |
| | `Delete()` | DELETE command |
| **Table** | `From(string table, string alias = null)` | Set main table with optional alias |
| | `Into(string table)` | Target table for INSERT |
| **Joins** | `Join(string table, string condition, JoinType)` | Custom join |
| | `InnerJoin(string table, string condition)` | INNER JOIN |
| | `LeftJoin(string table, string condition)` | LEFT JOIN |
| | `RightJoin(string table, string condition)` | RIGHT JOIN |
| | `JoinRaw(string rawJoin, Dictionary params)` | Raw JOIN clause |
| **WHERE** | `Where(string column, string operator, object value)` | WHERE with operator |
| | `WhereEqual(string column, object value)` | WHERE column = value |
| | `WhereNotEqual(string column, object value)` | WHERE column != value |
| | `WhereGreater(string column, object value)` | WHERE column > value |
| | `WhereLess(string column, object value)` | WHERE column < value |
| | `WhereLike(string column, string pattern)` | WHERE column LIKE pattern |
| | `WhereEquals(Dictionary values)` | Multiple WHERE column = value |
| | `WhereBetween(string column, object min, max)` | WHERE BETWEEN min AND max |
| | `WhereIn(string column, IEnumerable values)` | WHERE IN (...) |
| | `WhereNotIn(string column, IEnumerable values)` | WHERE NOT IN (...) |
| | `WhereNull(string column, bool isNot = false)` | WHERE IS [NOT] NULL |
| | `WhereNotNull(string column)` | WHERE IS NOT NULL |
| | `WhereRaw(string sqlClause, Dictionary params)` | Raw WHERE clause |
| | `WhereSubquery(string column, string op, IQueryBuilder)` | WHERE with subquery |
| | `WhereExists(IQueryBuilder subquery, bool isNot)` | WHERE [NOT] EXISTS |
| **Sort & Group** | `OrderBy(params string[] columns)` | ORDER BY (use "col ASC/DESC") |
| | `GroupBy(params string[] columns)` | GROUP BY |
| | `Having(string condition)` | HAVING condition |
| **Limit** | `Limit(int count, int? offset = null)` | LIMIT with optional OFFSET |
| | `Paginate(int page, int pageSize)` | Pagination helper |
| **Build** | `Build()` | Returns (SQL, Parameters) |
| **Utility** | `Clone()` | Deep copy for reuse |
| | `Reset()` | Clear query state |
| | `ValidateQuery()` | Check query validity |

## Quick Examples

```csharp
using Nglib.DATA.CONNECTOR.QUERYBUILDER;

// Simple SELECT
var query = QueryBuilderTools.CreateQueryBuilder("postgresql")
    .From("users", "u")
    .Select("u.id", "u.name", "u.email")
    .WhereEqual("u.active", true)
    .WhereGreater("u.age", 18)
    .OrderBy("u.name ASC")
    .Limit(10);

var (sql, parameters) = query.Build();

// INSERT
var newUser = new Dictionary<string, object>
{
    ["name"] = "John Doe",
    ["email"] = "john@example.com",
    ["active"] = true
};

var insertQuery = QueryBuilderTools.CreateQueryBuilder("postgresql")
    .Into("users")
    .Insert(newUser);

// UPDATE
var updates = new Dictionary<string, object> { ["last_login"] = DateTime.Now };

var updateQuery = QueryBuilderTools.CreateQueryBuilder("postgresql")
    .From("users")
    .Update(updates)
    .WhereEqual("id", userId);

// DELETE
var deleteQuery = QueryBuilderTools.CreateQueryBuilder("postgresql")
    .From("users")
    .Delete()
    .WhereEqual("active", false)
    .WhereLess("last_login", DateTime.Now.AddYears(-1));

// WITH JOINS
var query = QueryBuilderTools.CreateQueryBuilder("postgresql")
    .From("users", "u")
    .Select("u.name", "o.total")
    .InnerJoin("orders o", "o.user_id = u.id")
    .WhereEqual("u.status", "active")
    .WhereGreater("o.total", 100);
```

## Integration

```csharp
// With IDataConnector
using var connector = new ConnectorGeneric(connectionString);
var result = await connector.QueryDataSetAsync(sql, parameters);
```
