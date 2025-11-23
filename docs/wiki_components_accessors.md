[◀ Back to Documentation Home](../README.md)

# DATA.ACCESSORS

**Namespace:** `Nglib.DATA.ACCESSORS`

## Overview

Standardized interface for type-safe data access with automatic type conversion from various sources (DataRow, Dictionary, Configuration, etc.).

**Namespace:** `Nglib.DATA.ACCESSORS`

---

## IDataAccessor

Core interface providing uniform data access with automatic type conversion.

### Key Methods
- `GetData(string, DataAccessorOptionEnum)` - Low-level raw data retrieval
- `SetData(string, object, DataAccessorOptionEnum)` - Low-level raw data assignment
- `ListFieldsKeys()` - Lists available field keys
- `GetCryptoContext()` - Optional crypto context

**⚠️ Note:** Use extension methods (`GetValue<T>`, `SetValue`) instead of direct interface methods.

---

## DataAccessorExtensions

Extension methods providing type-safe access with automatic conversion to base types.

### Core Methods

**Generic Access:**
```csharp
T GetValue<T>(string name, T defaultValue, DataAccessorOptionEnum options)
T GetValue<T>(string name)
bool TryGetValue<T>(string name, out T value, DataAccessorOptionEnum options)
bool SetValue(string name, object value, DataAccessorOptionEnum options)
```

**Typed Shortcuts:**
```csharp
string GetString(string name, string defaultValue, DataAccessorOptionEnum options)
int GetInt(string name, int defaultValue, DataAccessorOptionEnum options)
long GetLong(string name, long defaultValue, DataAccessorOptionEnum options)
double GetDouble(string name, double defaultValue, DataAccessorOptionEnum options)
bool GetBoolean(string name, bool defaultValue, DataAccessorOptionEnum options)
DateTime GetDateTime(string name, DateTime defaultValue, DataAccessorOptionEnum options)
TEnum GetEnum<TEnum>(string name, TEnum defaultValue, DataAccessorOptionEnum options)
```

**Array Support:**
```csharp
string[] GetStringArray(string name, DataAccessorOptionEnum options)
int[] GetIntArray(string name, DataAccessorOptionEnum options)
// ... and other array types
```

### Examples

```csharp
// Basic usage
var name = accessor.GetValue<string>("username");
var age = accessor.GetValue<int>("age", 18, DataAccessorOptionEnum.Safe);

// TryGetValue pattern (no exceptions)
if (accessor.TryGetValue<int>("count", out var count))
    Console.WriteLine($"Count: {count}");

// Required fields
var email = accessor.GetValue<string>("email", null, 
    DataAccessorOptionEnum.Required); // Throws if null/empty

// Set values
accessor.SetValue("username", "John");
accessor.SetValue("age", 30, DataAccessorOptionEnum.NotReplace); // Don't overwrite if exists
```

---

## DataAccessorOptionEnum

Flags controlling data accessor behavior.

| Flag | Description | Use Case |
|------|-------------|----------|
| **None** | Default behavior | Standard usage |
| **Safe** | No exceptions, returns default | Optional data reading |
| **Required** | Rejects null, DBNull, empty string, numeric zeros | Mandatory fields |
| **NotReplace** | Don't overwrite if key exists | Preserve existing values |
| **NotCreateColumn** | Throws if key doesn't exist | Strict schema enforcement |
| **IgnoreChange** | No change notification | Silent updates |
| **CurrentCulture** | Use CurrentCulture instead of InvariantCulture | Localized parsing |
| **AdvancedConverter** | Use ConvertTools.ChangeType | Complex conversions |
| ~~**Encrypted**~~ | ~~Crypto support~~ | ~~Sensitive data~~ (deprecated) |
| ~~**UseCache**~~ | ~~Cache reading~~ | ~~Performance~~ (deprecated) |

### Common Combinations

```csharp
// Required field with strict schema
var value = data.GetValue<string>("username", null, 
    DataAccessorOptionEnum.Required | DataAccessorOptionEnum.NotCreateColumn);

// Safe reading with cache
var timeout = data.GetValue<int>("timeout", 30, 
    DataAccessorOptionEnum.Safe | DataAccessorOptionEnum.UseCache);

// Conditional update
data.SetValue("defaultValue", "new", DataAccessorOptionEnum.NotReplace);

// Required validation with safe fallback
var email = data.GetValue<string>("email", "default@mail.com", 
    DataAccessorOptionEnum.Required | DataAccessorOptionEnum.Safe);
```

---

## DataAccessorTools

Static utility methods for data accessor manipulation.

### Methods
- `GetEnumDefaultValue<TEnum>()` - Gets default enum value
- `ConvertoArrayString(object)` - Converts array to string[]
- `CopyTo(IDataAccessor source, IDataAccessor destination)` - Copies all data

### Examples

```csharp
// Copy all data between accessors
sourceAccessor.CopyTo(destinationAccessor);

// Get default enum value
var defaultStatus = DataAccessorTools.GetEnumDefaultValue<Status>();
```

---

## DataAccessorException

Specialized exception for data accessor errors.

```csharp
try
{
    var value = accessor.GetValue<int>("invalid", DataAccessorOptionEnum.Required);
}
catch (DataAccessorException ex)
{
    Console.WriteLine($"Accessor error: {ex.Message}");
}
```

---

## IDataAccessorCryptoContext

Interface for encryption/decryption operations on data.

### Properties
- `OptionForEncrypt` - Encryption configuration
- `OptionForDecrypt` - Decryption configuration

### Methods
- `GetIV(IDataAccessor)` - Gets initialization vector
- `EncryptObjectValue(...)` - Encrypts before storage
- `DecryptObjectValue(...)` - Decrypts after retrieval

---

## Common Patterns

**Safe Configuration Reading:**
```csharp
var timeout = config.GetValue<int>("Timeout", 30, DataAccessorOptionEnum.Safe);
var apiKey = config.GetValue<string>("ApiKey", null, 
    DataAccessorOptionEnum.Required | DataAccessorOptionEnum.Safe);
```

**Strict Database Access:**
```csharp
var userId = row.GetValue<int>("UserId", 0, 
    DataAccessorOptionEnum.Required | DataAccessorOptionEnum.NotCreateColumn);
```

**Dictionary Conversion:**
```csharp
// To dictionary
var dict = accessor.ToDictionaryValues();

// From dictionary
accessor.FromDictionaryValues(new Dictionary<string, object>
{
    { "Name", "John" },
    { "Age", 30 }
});
```

**Object Mapping (Reflection):**
```csharp
// From object properties
accessor.FromReflectionProperties(user);

// To object properties
accessor.ToReflectionProperties(user);
```

**TryGetValue Pattern:**
```csharp
if (accessor.TryGetValue<DateTime>("LastLogin", out var lastLogin))
{
    Console.WriteLine($"Last login: {lastLogin}");
}
else
{
    Console.WriteLine("Never logged in");
}
```

---

## Notes
- Always use **extension methods** (`GetValue<T>`, `SetValue`) instead of direct `GetData`/`SetData`
- **Required** flag validates null, DBNull, empty strings, and numeric zeros
- **Safe** mode returns `defaultValue` instead of throwing exceptions
- **InvariantCulture** is used by default for data portability (use `CurrentCulture` flag for UI)
- Extension methods support **arrays** for all basic types
- **Thread-safe** for read operations (depends on underlying implementation)

---

## Implementations
- **DictionaryData** - Dictionary-based (case-insensitive)
- **DataPO** - ADO.NET DataRow wrapper
- **ConfigurationAccessor** - ASP.NET Core IConfiguration wrapper
- **ParamValues** - Hierarchical key-value structures
