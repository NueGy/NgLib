[◀ Back to Documentation Home](../README.md)

# QueryBuilder - SQL Query Builder

**Namespace**: `Nglib.DATA.CONNECTOR.QUERYBUILDER`

## Description

QueryBuilder is a Fluent API for building multi-DBMS SQL queries. It allows building SELECT, INSERT, UPDATE and DELETE queries in a typed and secure manner, with support for subqueries, joins and advanced clauses.

**Main Features:**
- Fluent SQL query construction with continuous validation
- Multi-DBMS support (PostgreSQL, SQL Server, SQLite)
- Automatic and prefixed parameter management to prevent SQL injection
- Advanced clauses: subqueries, EXISTS/NOT EXISTS, BETWEEN, IN/NOT IN
- Safe cloning for query reuse
- Strict operator validation (`=`, `<>`, `LIKE`, `IN`, …)
- Paginated limitation via `Limit(int count, int? offset = null)`

## Main Classes

- **IQueryBuilder**: Main interface defining construction methods
- **BaseQueryBuilder**: Base abstract implementation with common logic
- **PostgresqlQueryBuilder**: PostgreSQL specialized implementation

## Complete Example

```csharp
using Nglib.DATA.CONNECTOR.QUERYBUILDER;

// === 1. ADVANCED SELECT QUERY ===
// Search for active users with their recent orders
var subQuery = new PostgresqlQueryBuilder()
    .From("orders")
    .Select("COUNT(*)")
    .Where("user_id", "=", "users.id")
    .Where("created_at", ">=", DateTime.Now.AddMonths(-6));

var usersQuery = new PostgresqlQueryBuilder()
    .From("users")
    .Select("id", "name", "email", "created_at")
    .Where("active", "=", true)
    .Where("age", ">=", 18)
    .WhereIn("role", new[] { "customer", "premium" })
    .WhereSubquery("recent_orders", ">", subQuery)  // Verifies operator is allowed before construction
    .OrderBy("name ASC")
    .Limit(50);

var (sql, parameters) = usersQuery.Build();
// Execution
using var connector = new ConnectorGeneric(connectionString);
var userData = await connector.QueryDataSetAsync(sql, parameters);

// === 2. CLONING AND REUSE ===
// Reuse base query for different criteria
var premiumUsers = usersQuery.Clone()
    .Where("subscription", "=", "premium");

var recentUsers = usersQuery.Clone()
    .Where("created_at", ">=", DateTime.Now.AddDays(-30))
    .Reset() // Reset previous for new criteria
    .OrderBy("created_at DESC");

// === 3. INSERT ===
var newUser = new Dictionary<string, object>
{
    ["name"] = "John Doe",
    ["email"] = "john@example.com",
    ["role"] = "customer",
    ["active"] = true,
    ["created_at"] = DateTime.Now
};

var insertQuery = new PostgresqlQueryBuilder()
    .Into("users")
    .Insert(newUser);

await connector.ExecuteNonQueryAsync(insertQuery.Build());

// === 4. UPDATE ===
var updates = new Dictionary<string, object>
{
    ["last_login"] = DateTime.Now,
    ["login_count"] = 1  // Will be incremented via custom clause
};

var updateQuery = new PostgresqlQueryBuilder()
    .From("users")
    .Update(updates)
    .WhereClause("login_count = login_count + 1", null)  // Custom SQL clause
    .Where("id", "=", userId);

// === 5. DELETE WITH EXISTS ===
// Delete inactive users without orders
var hasOrdersQuery = new PostgresqlQueryBuilder()
    .From("orders")
    .Select("1")
    .Where("user_id", "=", "users.id");

var deleteQuery = new PostgresqlQueryBuilder()
    .From("users")
    .Delete()
    .Where("active", "=", false)
    .Where("last_login", "<", DateTime.Now.AddYears(-2))
    .WhereExists(hasOrdersQuery, isNotCondition: true);  // NOT EXISTS

// === 6. WHERE CLAUSE ONLY RETRIEVAL ===
// To reuse conditions in other contexts
var conditionsQuery = new PostgresqlQueryBuilder()
    .Where("status", "=", "active")
    .Where("price", "BETWEEN", new[] { 10, 100 });

var (whereClause, whereParams) = conditionsQuery.BuildWhereClause();
// Result: "WHERE status = @status AND price BETWEEN @price_min AND @price_max"

// === 7. AUTOMATIC VALIDATION ===
try 
{
    var invalidQuery = new PostgresqlQueryBuilder()
        .From("users")
        .OrderBy(null);  // Throws ArgumentNullException
}
catch (ArgumentNullException ex)
{
    // Validation error handling
}
```

## Simple Usage

```csharp
// Basic SELECT
var query = new PostgresqlQueryBuilder()
    .From("products")
    .Select("id", "name", "price")
    .Where("category_id", "=", 5)
    .OrderBy("price DESC")
    .Limit(10);

var (sql, parameters) = query.Build();
```

## Advanced Features

- **Subqueries**: `WhereSubquery()` and `WhereExists()` isolate and prefix secondary parameters
- **Validation**: `Where()` rejects any non-whitelisted syntax and handles `NULL` via `IS [NOT] NULL`
- **Pagination**: `Limit(count, offset)` replaces the old pair of overloads
- **BuildWhereClause()**: Extraction of WHERE clause only (SQL + parameter dictionary)
- **Multi-DBMS support**: PostgreSQL, SQL Server, SQLite

## Integration

```csharp
// With IDataConnector
using var connector = new ConnectorGeneric(connectionString);
var result = await connector.QueryDataSetAsync(queryBuilder.Build());

// Conversion to QueryContext
var queryContext = queryBuilder.BuildQuery();
```