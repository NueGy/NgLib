[◀ Back to Documentation Home](../README.md)

# DATA.COLLECTIONS

**Namespace:** `Nglib.DATA.COLLECTIONS`

## Overview

Powerful collection manipulation tools, including case-insensitive dictionaries and extension methods for safe operations on lists and dictionaries.

**Main Classes:**
- `CollectionsTools` - Static utility methods for collections
- `DictionaryData` - Case-insensitive dictionary with typed accessors
- `ListResult<T>` - List wrapper with metadata (obsolete/beta)

## CollectionsTools

Static class with extension methods for safe and advanced collection operations.

### Key Features
- **Case-insensitive operations**: Search and compare with culture-aware options
- **Safe iterations**: Error-tolerant loops with ForEachSafe()
- **Collection partitioning**: Divide lists into chunks
- **Safe accessors**: Get values without exceptions
- **Cloning**: Deep copy of collections

### Methods

#### Contains() Extensions
```csharp
// Case-insensitive string search
bool found = list.Contains("value", ignoreCase: true);
bool found = list.Contains("value", StringComparison.OrdinalIgnoreCase);

// Inverted search
bool contained = item.ContainInList(collection);
```

#### Dictionary Operations
```csharp
// Add multiple items
dictionary.AddRange(otherDictionary);

// Add or replace with case-insensitive key matching
dictionary.AddOrReplace("key", value, checkExists: true, ignoreCase: true);
```

#### Safe Iterations
```csharp
// Continue even if one item throws an exception
list.ForEachSafe(item => ProcessItem(item));
```

#### Collection Partitioning
```csharp
// Variable chunk sizes
var chunks = collection.Divide(chunkSize: 10);

// Fixed number of chunks
var parts = collection.DivideFixed(numberOf: 5);
```

#### Safe Accessors
```csharp
// Returns default if key doesn't exist
string value = dictionary.GetSafeString("key");
object obj = dictionary.GetSafeObject("key");
int number = dictionary.GetSafeValue<int>("key");
```

#### Cloning
```csharp
// Deep copy with ICloneable support
var copy = collection.Clone();
var dictCopy = dictionary.Clone();
```

#### LINQ Extensions
```csharp
// Filter out null values
var valid = collection.NotNull();
```

## DictionaryData

Case-insensitive dictionary implementing `IDataAccessor` interface for typed data access.

### Features
- **Case-insensitive keys**: Uses `StringComparer.OrdinalIgnoreCase`
- **Typed accessors**: GetString(), GetInt32(), GetBool(), etc.
- **IDataAccessor integration**: Compatible with DATA/ACCESSORS component
- **Validation**: RemoveNotAllowedKeys() with whitelist
- **Cloning**: Deep copy support

### Constructor
```csharp
// Empty dictionary
var data = new DictionaryData();

// From Dictionary<string, string>
var data = new DictionaryData(stringDictionary);

// From Dictionary<string, object>
var data = new DictionaryData(objectDictionary);
```

### Basic Operations
```csharp
var data = new DictionaryData();

// Add values
data["key"] = value;
data.SetValueMinMax("number", 50, min: 0, max: 100);

// Get values
string text = data.GetString("key");
bool exists = data.ContainsKey("key");
bool empty = data.IsEmpty();

// Remove
data.Remove("key");
```

### IDataAccessor Implementation
```csharp
// Generic typed access
object value = data.GetData("key", typeof(int));
data.SetData("key", value);

// List all keys
string[] keys = data.ListFieldsKeys();
```

### Validation
```csharp
// Keep only allowed keys
string[] allowed = new[] { "name", "age", "email" };
data.RemoveNotAllowedKeys(allowed);
```

### Cloning
```csharp
// Create a deep copy
var copy = data.Clone();
```

### Integration Example
```csharp
// Works with DataAccessorExtensions from DATA/ACCESSORS
var data = new DictionaryData();
data.SetValue("age", 25);
data.SetValue("active", true);

int age = data.GetValue<int>("age");
bool active = data.GetValue<bool>("active");
```

## ListResult&lt;T&gt; (Obsolete)

⚠️ **Status**: Marked as BETA/Obsolete

List wrapper with additional metadata. Use standard `List<T>` instead.

### Structure
```csharp
public class ListResult<T> : List<T>
{
    public ResultInfoModel ResultInfo { get; set; }
    public string ToJsonWithEnveloppe();
}
```

## Related Components
- **DATA/ACCESSORS**: Core typed data access interface
- **DATA/DATAPO**: Data object persistence (uses DictionaryData)

## Dependencies
- `System.Collections.Generic`
- `Nglib.DATA.ACCESSORS` (IDataAccessor interface)
- `Newtonsoft.Json` (ListResult serialization)

## Notes
- All case-insensitive operations use `StringComparison.OrdinalIgnoreCase`
- `ForEachSafe()` catches exceptions but doesn't log them
- `DictionaryData` is the preferred dictionary for typed data scenarios
- `ListResult<T>` is obsolete, avoid using it in new code
