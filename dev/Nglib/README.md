# NgLib - General C# Library

**Nglib** is a C# .NET library containing useful components and helpers for modern applications.

Open-source (MIT) - General and Common .NET Helpers, Extensions, Data Access, Dev Components, and more.

## 📦 Packages

| Package | Description |
| ------- | ----------- |
| **[Nglib](https://www.nuget.org/packages/Nglib/)** [![Nglib](https://img.shields.io/nuget/v/Nglib.svg)](https://www.nuget.org/packages/Nglib/) | Core abstractions, basic tools, zero dependencies |
| **[Nglib.Data](https://www.nuget.org/packages/Nglib.Data/)** [![Nglib.Data](https://img.shields.io/nuget/v/Nglib.Data.svg)](https://www.nuget.org/packages/Nglib.Data/) | Data access components, DataPO ORM, providers |
| **[Nglib.Formula](https://www.nuget.org/packages/Nglib.Formula/)** [![Nglib.Formula](https://img.shields.io/nuget/v/Nglib.Formula.svg)](https://www.nuget.org/packages/Nglib.Formula/) | Dynamic formula evaluation engine |

## 🚀 Installation

```bash
dotnet add package Nglib
```

## ⚡ Features

- **Data Accessors** - Flexible data access patterns with `IDataAccessor` interface
- **Collections Tools** - Extensions for Dictionary, List and collection manipulations
- **String Tools** - Advanced string manipulation and formatting utilities
- **Date Tools** - Date/time parsing, formatting and calculations
- **Number Tools** - Number formatting, conversion and validation
- **Key Tools** - Unique key generation and manipulation
- **Reflection Tools** - Simplified reflection operations
- **Validation Tools** - Model validation with `ValidateModel` pattern
- **HTTP Client Tools** - HTTP client helpers with token management
- **Formula Engine** - Expression parser and evaluator (separate package)

## 📖 Quick Examples

### Data Accessor
```csharp
using Nglib.DATA.ACCESSORS;

var data = new DictionaryData();
data.SetValue("name", "John");
data.SetValue("age", 30);

string name = data.GetValue<string>("name");
int age = data.GetValue<int>("age");
```

### String Tools
```csharp
using Nglib.FORMAT;

string random = StringTools.RandomString(12);
string cleaned = StringTools.CleanString("Hello  World!");
string key = KeyTools.SanitizeKey("My-Key_123"); // "mykey123"
```

### Validation
```csharp
using Nglib.APP.DIAG;

var result = ValidateModel.Success;
if (value < 0)
    result.SetInvalid("Value must be positive");

result.EnsureIsValid(); // Throws if invalid
```

## 📚 Documentation

Full documentation: [https://github.com/NueGy/NgLib](https://github.com/NueGy/NgLib)


### 🔗 Links

- [GitHub Repository](https://github.com/NueGy/NgLib)
- [Report Issues](https://github.com/NueGy/NgLib/issues)
- [NuGet Package](https://www.nuget.org/packages/Nglib)
- [View License](https://github.com/NueGy/NgLib/blob/master/Licence.md) (MIT License)
- [Nuegy.net](https://www.nuegy.net) (Agency website)
