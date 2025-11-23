# Nglib.Data

**Dataset, DataPO, DataConnector - SQL database components**

Nglib.Data provides powerful data access components for working with relational databases, including DataSet helpers, DataPO (Plain Old Data) pattern, and flexible database connectors.

## Features

- **DataPO Pattern**: Object-relational mapping with change tracking
- **Data Connectors**: Generic database connectors (SQL Server, PostgreSQL, SQLite, MySQL)
- **Query Builder**: Fluent SQL query construction
- **DataSet Tools**: Advanced DataSet and DataTable manipulation
- **Schema Management**: Automatic table schema detection and management
- **Transaction Support**: Built-in transaction management
- **Async Operations**: Full async/await support for all database operations

## Installation

```bash
dotnet add package Nglib.Data
```

**Note**: Requires `Nglib` package as dependency.

## Quick Start

### DataPO Pattern
```csharp
using Nglib.DATA.DATAPO;

public class User : DataPO<User>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Create and save
var user = new User { Name = "John", Email = "john@example.com" };
await user.SaveAsync(connector);

// Query
var users = await User.QueryAsync(connector, "SELECT * FROM Users WHERE Name LIKE @name", 
    new { name = "%John%" });
```

### Data Connector
```csharp
using Nglib.DATA.CONNECTOR;

var connector = new ConnectorGeneric("PostgreSQL", connectionString);

// Query with parameters
var result = await connector.QueryAsync("SELECT * FROM Users WHERE Age > @age", 
    new { age = 18 });

// Transaction
using (connector.BeginTransaction())
{
    await connector.ExecuteAsync("INSERT INTO Users (Name) VALUES (@name)", 
        new { name = "Alice" });
    connector.CommitTransaction();
}
```

### Query Builder
```csharp
using Nglib.DATA.QUERYBUILDER;

var query = QueryBuilderTools.CreateQueryBuilder("PostgreSQL")
    .Select("Id", "Name", "Email")
    .From("Users")
    .Where("Age > @age")
    .OrderBy("Name")
    .BuildQuery();

var results = await connector.QueryDataSetAsync(query, new { age = 18 });
```

## Supported Databases

- Microsoft SQL Server
- PostgreSQL
- SQLite
- MySQL/MariaDB

## 📚 Documentation

Full documentation: [https://github.com/NueGy/NgLib](https://github.com/NueGy/NgLib)


### 🔗 Links

- [GitHub Repository](https://github.com/NueGy/NgLib)
- [Report Issues](https://github.com/NueGy/NgLib/issues)
- [NuGet Package](https://www.nuget.org/packages/Nglib)
- [View License](https://github.com/NueGy/NgLib/blob/master/Licence.md) (MIT License)
- [Nuegy.net](https://www.nuegy.net) (Agency website)
