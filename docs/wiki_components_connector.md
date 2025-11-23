[◀ Back to Documentation Home](../README.md)

# DATA.CONNECTOR

**Namespace:** `Nglib.DATA.CONNECTOR`

## Overview

Multi-engine database connector component for interacting with different databases (SQL Server, MySQL, PostgreSQL, SQLite, Oracle).

## Key Features

### Thread-Safety
- **SemaphoreSlim**: High-performance synchronization for open/close and queries
- **Interlocked**: Thread-safe counter for `RequestCount`
- Performance: 30-50% faster than Mutex for intra-process synchronization

### Resource Management
- **IDisposable**: Complete pattern with automatic resource cleanup
- Releases: connections, transactions, semaphores
- Anti-usage protection after dispose via `ObjectDisposedException`

### Observability
- **QueryBegin**: Event triggered before query execution
- **QueryCompleted**: Event triggered after execution (success or error)
- **RequestCount**: Thread-safe property returning total number of queries

### Reflection Cache
- Connector factory cache for optimal performance
- Uses `ConcurrentDictionary` for thread-safe access

## Basic Usage

```csharp
using var connector = new ConnectorGeneric(EnumConnectorMode.SQLITE);
connector.ConnectionString = "Data Source=mydb.db";
connector.QueryBegin += (ctx) => Console.WriteLine($"Query: {ctx.Query}");
connector.QueryCompleted += (ctx) => Console.WriteLine($"Duration: {ctx.ExecutionTime}ms");

// Execute query
var result = await connector.QueryScalarAsync<int>("SELECT COUNT(*) FROM Users");

// Statistics
Console.WriteLine($"Total requests: {connector.RequestCount}");
```

## Transactions

```csharp
using var connector = new ConnectorGeneric(EnumConnectorMode.SQLITE);
connector.ConnectionString = "Data Source=mydb.db";

connector.BeginTransaction();
try
{
    await connector.QueryScalarAsync("INSERT INTO Users (Name) VALUES (@name)", 
        new Dictionary<string, object> { ["name"] = "John" });
    connector.CommitTransaction();
}
catch
{
    connector.RollbackTransaction();
    throw;
}
```

## Best Practices

1. **Always use `using`** to automatically dispose
2. **Events**: Use `QueryBegin`/`QueryCompleted` for monitoring
3. **Thread-safety**: Connector is thread-safe, can be shared between threads
4. **Transactions**: Always rollback on error

## Available Extensions
- `IDataConnectorExtends`: Extension methods for simplified queries
- `DataPOProvider`: Provider for business objects (Data Persistent Objects)