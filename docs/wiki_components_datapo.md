[◀ Back to Documentation Home](../README.md)

# DATA.DATAPO

**Namespace:** `Nglib.DATA.DATAPO`

## Overview

Hybrid ORM combining ADO.NET (DataRow/DataTable) with NoSQL capabilities (XML/JSON flows). Architecture inspired by the **Active Record** pattern with advanced data persistence features.

**Main Classes:**
- `DataPO` - Base persistent object class
- `IDataPO` - Core interface for persistent objects
- `IDataPOFlow` - Interface for NoSQL data flows
- `DataPOProvider*` - Database providers (SQL, CRUD operations)
- `CollectionPO` - Collection of DataPO objects

## Architecture

### Hybrid Model
DataPO combines two storage approaches:
1. **Structured data**: ADO.NET DataRow (columns/tables)
2. **Semi-structured data**: NoSQL flows (XML/JSON in text columns)

### Path-Based Access
Supports complex data paths:
- `"field"` - Direct DataRow column
- `"/flowname/path"` - NoSQL flow data
- `"property:field"` - Linked DataPO objects

## DataPO Class

Base class for persistent objects with hybrid storage capabilities.

### Core Features
- **DataRow wrapper**: ADO.NET integration
- **NoSQL flows**: XML/JSON in database columns
- **IDataAccessor**: Typed data access via extensions
- **Change tracking**: AcceptChanges/IsChanges
- **Encryption**: Built-in crypto support
- **Schema management**: Dynamic column creation

### Constructor
```csharp
// Empty constructor
var obj = new DataPO();

// From existing DataRow
var obj = new DataPO(dataRow);
```

### DataRow Operations
```csharp
// Initialize schema (override in derived class)
public override DataTable InitSchema()
{
    var table = new DataTable("MyTable");
    table.Columns.Add("Id", typeof(int));
    table.Columns.Add("Name", typeof(string));
    table.PrimaryKey = new[] { table.Columns["Id"] };
    return table;
}

// Get DataRow (auto-initializes schema)
DataRow row = obj.GetRow(syncFlows: true);

// Set DataRow
obj.SetRow(existingRow);

// Check if in database
bool exists = obj.IsInDataBase();
```

### Data Access
```csharp
// Indexer access
obj["name"] = "John";
string name = (string)obj["name"];

// IDataAccessor integration (via extensions from DATA/ACCESSORS)
obj.SetValue("age", 25);
int age = obj.GetValue<int>("age");

// Complex paths
obj.SetData("/config/setting", value, DataAccessorOptionEnum.None);
object val = obj.GetData("/config/setting", DataAccessorOptionEnum.Safe);
```

### NoSQL Flows
Embed semi-structured data in database text columns:

```csharp
// Define and get a flow (auto-created if doesn't exist)
var flow = obj.GetOrDefineFlow<MyFlow>("ConfigData", 
    FlowTypeEnum.Json, 
    FullEncrypted: false);

// Access flow data
flow["setting1"] = value;

// Get existing flow
IDataPOFlow flow = obj.GetDataPOFlow("ConfigData");

// Flows are synchronized on GetRow(syncFlows: true)
```

### Change Tracking
```csharp
// Check if object has changes
bool hasChanges = obj.IsChanges();

// Accept all changes (DataRow + flows)
bool accepted = obj.AcceptChanges();

// Get modified fields
string[] keys = obj.ListFieldsKeys();
```

### Encryption
```csharp
// Set encryption context
obj.SetCryptoOptions(cryptoContext);

// Get context
var context = obj.GetCryptoContext();

// Get unique IV for object
string iv = obj.GetCryptoIV();
```

## IDataPOFlow Interface

Interface for NoSQL data stored in database text fields.

### Methods
```csharp
public interface IDataPOFlow
{
    string GetFieldName();                    // Database column name
    FlowTypeEnum GetFieldType();              // XML/JSON type
    bool IsFieldEncrypted();                  // Full encryption flag
    
    void DefineField(string name, FlowTypeEnum type, bool encrypted);
    string SerializeField();                  // To database format
    void DeSerializeField(string data);       // From database format
    
    bool IsChanges();                         // Check modifications
    bool AcceptChanges();                     // Mark as saved
}
```

### Flow Types
- `FlowTypeEnum.Xml` - XML format
- `FlowTypeEnum.Json` - JSON format

## Advanced Features

### Dynamic Column Creation
DataPO automatically creates missing columns:
```csharp
// Column auto-created if doesn't exist
obj["newField"] = value;

// Prevent auto-creation
obj.SetData("field", value, DataAccessorOptionEnum.NotCreateColumn);
```

### Linked Objects
Access data from related DataPO properties:
```csharp
// Assuming obj has a "User" property (another DataPO)
string userName = obj.GetData("User:Name", options);

// Read-only (write not supported for linked objects)
```

### Change Tracking Without Modification
```csharp
// Set value without marking as changed
obj.SetData("field", value, DataAccessorOptionEnum.IgnoreChange);
```

### Schema Initialization
Override `InitSchema()` in derived classes:
```csharp
public class UserPO : DataPO
{
    public override DataTable InitSchema()
    {
        var table = new DataTable("Users");
        table.Columns.Add("UserId", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("ConfigJson", typeof(string)); // For NoSQL flow
        
        table.PrimaryKey = new[] { table.Columns["UserId"] };
        return table;
    }
}
```

## DataPO Providers (Nglib.Data)

Additional classes for database operations:

### DataPOProvider*
- **DataPOProviderSQL**: SQL Server operations
- **DataPOProviderCRUD**: CRUD helpers
- **DataPOProviderExtends**: Extension methods
- **DataPOProviderTools**: Utility methods
- **DataPOSchemaTools**: Schema management

### CollectionPO
Collection management for DataPO objects:
```csharp
public interface ICollectionPO
{
    // Collection operations
    void Add(IDataPO item);
    void Remove(IDataPO item);
    IEnumerable<IDataPO> GetItems();
}
```

## Integration Example

Complete example with flows:
```csharp
public class ProductPO : DataPO
{
    public override DataTable InitSchema()
    {
        var table = new DataTable("Products");
        table.Columns.Add("ProductId", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Price", typeof(decimal));
        table.Columns.Add("MetaData", typeof(string)); // JSON flow
        return table;
    }
}

// Usage
var product = new ProductPO();
product.SetValue("ProductId", 1);
product.SetValue("Name", "Widget");
product.SetValue("Price", 29.99m);

// NoSQL metadata
var meta = product.GetOrDefineFlow<JsonFlow>("MetaData", FlowTypeEnum.Json);
meta["color"] = "blue";
meta["weight"] = 1.5;

// Save to database (via provider)
DataRow row = product.GetRow(); // Syncs flows
// ... insert/update in database
```

## Related Components
- **DATA/ACCESSORS**: Typed data access (GetValue<T>, SetValue)
- **DATA/COLLECTIONS**: DictionaryData, CollectionTools

## Dependencies
- `System.Data` (ADO.NET)
- `Nglib.DATA.ACCESSORS` (IDataAccessor interface)
- `Nglib.SECURITY.CRYPTO` (encryption support)

## Notes
- **Hybrid storage**: Use structured columns for queryable data, flows for flexible/nested data
- **Auto-initialization**: Schema is created automatically on first access
- **Change tracking**: Both DataRow and flows track modifications independently
- **Path syntax**: Use "/" prefix for flows, ":" for linked objects
- **Performance**: Flow synchronization happens on `GetRow(syncFlows: true)`
- **Thread-safety**: Not thread-safe by default, wrap in locks if needed
