[◀ Back to Documentation Home](../README.md)

# APP.CODE

**Namespace:** `Nglib.APP.CODE`

## Overview

Reflection-based utilities for runtime type manipulation, property access, and attribute inspection.

---

## ReflectionTools

Tools for type resolution and dynamic instance creation.

### Features
- Type resolution by fully qualified name (case-insensitive)
- Generic and non-generic instance creation with constructor parameters

### Examples

```csharp
// Get type by name
Type myType = ReflectionTools.GetType("System.String");

// Create instance
var instance = ReflectionTools.CreateInstance<MyClass>();
var instance = ReflectionTools.CreateInstance<MyClass>(null, "param1", 42);
```

---

## PropertiesTools

Tools for manipulating object properties and fields using reflection.

### Features
- Read/write properties and fields (public and private)
- Automatic type conversion on assignment
- Case-insensitive property lookup
- Dictionary-based bulk operations
- Safe mode to avoid exceptions

### Examples

```csharp
// Get/Set values
object value = PropertiesTools.GetValue(myObject, "MyProperty");
PropertiesTools.SetValue(myObject, "Age", "30"); // Auto-converts string to int

// Safe mode (no exception)
object value = PropertiesTools.GetValue(myObject, "UnknownProperty", safe: true);

// Dictionary operations
var data = PropertiesTools.GetValues(myObject);
PropertiesTools.SetValues(myObject, new Dictionary<string, object> 
{ 
    { "Name", "John" }, 
    { "Age", 30 } 
});
```

---

## AttributesTools

Tools for inspecting and extracting attributes from types and members.

### Features
- Extract attributes from types, objects, and members
- Find properties/methods with specific attributes
- Discover types with attributes across assemblies
- Filter by member type and name prefix

### Examples

```csharp
// Get attribute from type
var attr = AttributesTools.GetAttribute<SerializableAttribute>(typeof(MyClass));

// Find properties with attribute
var props = AttributesTools.GetPropertiesWithAttribute<RequiredAttribute>(typeof(User));
foreach (var (prop, attr) in props)
{
    Console.WriteLine($"{prop.Name} is required");
}

// Discover all types with attribute
var types = AttributesTools.GetTypesWithAttribute<PluginAttribute>();

// Get values with attribute
var values = AttributesTools.GetValuesWithAttribute<RequiredAttribute>(myObject);
```

---

## Common Use Cases

**Dynamic Mapping:**
```csharp
PropertiesTools.SetValues(target, dataDictionary);
```

**Validation:**
```csharp
var requiredProps = AttributesTools.GetPropertiesWithAttribute<RequiredAttribute>(typeof(User));
```

**Plugin Discovery:**
```csharp
var plugins = AttributesTools.GetTypesWithAttribute<PluginAttribute>()
    .Select(t => ReflectionTools.CreateInstance<IPlugin>(t.Key));
```

---

## Notes
- All tools are **thread-safe** (stateless static methods)
- Use `safe` mode to avoid exceptions when property might not exist
- Cache `PropertyInfo` for performance in repeated operations
- Avoid excessive reflection in performance-critical paths
