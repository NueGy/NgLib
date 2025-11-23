Nglib - General C# Library
==========================

A comprehensive .NET library providing reusable components for modern C# applications. Nglib offers a collection of battle-tested utilities, extensions, and frameworks designed to accelerate development and improve code quality.

_Open-source (MIT) - General and Common .NET Helpers, Extensions, Data Access, HTTP Client, Formula Engine, and more._

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](Licence.md)
[![.NET](https://img.shields.io/badge/.NET-6.0%2B-512BD4)](https://dotnet.microsoft.com/)

[📚 Full Documentation](docs/Home.md)

## 📦 NuGet Packages

| Package | NuGet | Description |
| ------- | ----- | ----------- |
| [**Nglib**](https://www.nuget.org/packages/Nglib/) | [![Version](https://img.shields.io/nuget/v/nglib.svg)](https://www.nuget.org/packages/Nglib/) [![Downloads](https://img.shields.io/nuget/dt/Nglib.svg)](https://www.nuget.org/packages/Nglib/) | Core abstractions, basic tools, zero dependencies |
| [**Nglib.Data**](https://www.nuget.org/packages/Nglib.Data/) | [![Version](https://img.shields.io/nuget/v/nglib.Data.svg)](https://www.nuget.org/packages/Nglib.Data/) [![Downloads](https://img.shields.io/nuget/dt/Nglib.Data.svg)](https://www.nuget.org/packages/Nglib.Data/) | Data access components, DataPO ORM, providers |
| [**Nglib.Formula**](https://www.nuget.org/packages/Nglib.Formula/) | [![Version](https://img.shields.io/nuget/v/nglib.Formula.svg)](https://www.nuget.org/packages/Nglib.Formula/) [![Downloads](https://img.shields.io/nuget/dt/Nglib.Formula.svg)](https://www.nuget.org/packages/Nglib.Formula/) | Dynamic string format, formula evaluation |

## 🚀 Quick Start

```bash
# Install core package (zero dependencies)
dotnet add package Nglib
```


## 📚 Core Components

### 🔤 [FORMAT](docs/wiki_components_format.md)
String manipulation, date/time utilities, and conversion tools.

**Features**:
- String sanitization and validation (`CleanString`, `SanitizeKey`, `IsAlphaNumeric`)
- Random generation (`RandomString`, `RandomGuid32`)
- Date formatting (Unix timestamps, 8-char format `20251008`, business days)
- Safe substring operations (`SubstringSafe`, `Limit`)
- Diacritics removal (`ReplaceDiacritics`)
- Advanced parsing (extended `ToBoolean`, `ToDateTime`, `ToInt`)

```csharp
// String utilities
string random = StringTools.RandomString(12, true, true, false); // alphanumeric
string cleaned = StringTools.CleanString("Hello  World!"); // normalize spaces

// Date utilities
int timestamp = DateTools.DateTimeToTimeStamp(DateTime.Now);
string date8 = DateTools.ConvertDateTime8(DateTime.Now); // "20251008"

// Key sanitization
string key = KeyTools.SanitizeKey("My-Key_123"); // "mykey123"
```

[📖 View FORMAT Documentation](docs/wiki_components_format.md)

---

### 🧮 [FORMULA](docs/wiki_components_formula.md)
Dynamic formula evaluation engine with custom functions and template strings.

**Features**:
- Expression evaluation (`2 + 3 * 4`, `@param1 + @param2`)
- Template strings (`"Hello {=@name}!"`)
- Built-in functions (math, string, date, conditionals)
- Custom functions via `[Formula]` attribute
- AST parsing and execution

```csharp
using Nglib.Formula;

// Simple evaluation
var result = FormulaTools.Eval("2 + 3 * 4"); // 14

// With parameters
var context = new FormulaContext();
context.Parameters.Add("price", 100);
context.Parameters.Add("quantity", 5);
var total = FormulaTools.Eval("@price * @quantity", context); // 500

// Template strings
string template = "Total: {=@price * @quantity} EUR";
string result = FormulaTools.EvalComposedString(template, context); 
// "Total: 500 EUR"

// Custom functions
[Formula("DOUBLE", "Doubles the input value")]
public static object MyDouble(object value) => Convert.ToDouble(value) * 2;

FormulaTools.Eval("DOUBLE(21)"); // 42
```

[📖 View FORMULA Documentation](docs/wiki_components_formula.md)

---

### 🗄️ [DATA/DATAPO](docs/wiki_components_datapo.md)
Hybrid ADO.NET/NoSQL ORM with Active Record pattern and flexible data access.

**Features**:
- Hybrid storage (relational fields + NoSQL flows in text columns)
- Path-based access (`"field"`, `"/flow/path"`, `"prop:linkedfield"`)
- Change tracking and validation
- Encryption support
- Provider abstraction (SQL Server, Oracle, PostgreSQL, SQLite, Excel)

```csharp
using Nglib.Data.DATAPO;

// Create and save
var user = new DataPO();
user.SetData("Name", "John");
user.SetData("Email", "john@example.com");
user.SetData("/preferences/theme", "dark"); // NoSQL flow
user.AcceptChanges(); // Save to database

// Query and modify
var user = DataPO.LoadById(123);
string name = user.GetData<string>("Name");
bool isChanged = user.IsChanged("Email");

// Encryption
user.SetData("SSN", "123-45-6789", encrypted: true);
```

[📖 View DATAPO Documentation](docs/wiki_components_datapo.md)

---

### 📡 [NET/HTTPCLIENT](docs/wiki_components_httpclient.md)
HTTP client wrapper with authentication, attribute-based modeling, and OAuth2 support.

**Features**:
- Attribute-based API modeling (`[Endpoint]`, `[EndpointParameter]`)
- Multiple authentication methods (Bearer, Basic, OAuth2, JWT)
- Automatic token management and renewal
- Request/response serialization
- Validation and error handling

```csharp
using Nglib.NET.HTTPCLIENT;

// Define API endpoint
[Endpoint("GET", "/api/users/{userId}")]
public class GetUserRequest {
    [Required]
    [EndpointParameter(HttpParameterTypeEnum.Path)]
    public int UserId { get; set; }
    
    [EndpointParameter("include_details", HttpParameterTypeEnum.Query)]
    public bool IncludeDetails { get; set; }
}

// Use with OAuth2
var config = new HttpClientConfigModel {
    AuthType = TokenAuthTypeEnum.OAuth2Client,
    ClientId = "app-id",
    ClientSecret = "secret",
    TokenEndpointUrl = "https://auth.example.com/token"
};

var handler = new HttpClientTokenHandler { Config = config };
var client = new HttpClient(handler);

var request = new GetUserRequest { UserId = 123, IncludeDetails = true };
var httpRequest = EndpointTools.CreateRequestFromModel(request);
var response = await client.SendAsync(httpRequest);
response.Validate(); // Detailed error messages
var user = await response.ReadAsync<User>();
```

[📖 View HTTPCLIENT Documentation](docs/wiki_components_httpclient.md)

---

### 🔍 [DATA/ACCESSORS](docs/wiki_components_accessors.md)
Unified data access pattern for any object type with type-safe operations.

**Features**:
- Universal `GetData/SetData` interface
- Path-based navigation (`"user.address.city"`)
- Type conversion with culture support
- Array/collection handling
- Extensions for 80+ types

```csharp
using Nglib.DATA.ACCESSORS;

var user = new { Name = "John", Age = 30, Address = new { City = "Paris" } };

// Type-safe access
string name = user.GetValue<string>("Name");
int age = user.GetValue<int>("Age");

// Path navigation
string city = user.GetValue<string>("Address.City");

// Dictionary access
var dict = new Dictionary<string, object>();
dict.SetValue("key1", 42);
int value = dict.GetValue<int>("key1");
```

[📖 View ACCESSORS Documentation](docs/wiki_components_accessors.md)

---

### 📦 [DATA/COLLECTIONS](docs/wiki_components_collections.md)
Collection utilities and extensions for safe manipulation.

**Features**:
- Safe operations (`ContainsSafe`, `ForEachSafe`)
- Collection transformations (`Divide`, `Clone`, `AddRange`)
- `DictionaryData` (case-insensitive Dictionary + IDataAccessor)
- Null-safe accessors

```csharp
using Nglib.DATA.COLLECTIONS;

// Safe operations
var list = new List<int> { 1, 2, 3, 4, 5 };
bool exists = list.ContainsSafe(3); // true
list.ForEachSafe(x => Console.WriteLine(x)); // null-safe

// Divide collection
var chunks = list.Divide(2); // [[1,2], [3,4], [5]]

// Case-insensitive dictionary
var dict = new DictionaryData();
dict["MyKey"] = "value";
string value = dict.GetValue<string>("mykey"); // case-insensitive
```

[📖 View COLLECTIONS Documentation](docs/wiki_components_collections.md)

---

### 🛠️ [APP/CODE](docs/wiki_components_appcode.md)
Reflection utilities, property manipulation, and attribute discovery.

**Features**:
- Type resolution and instance creation
- Property/field access with conversion
- Attribute inspection and method discovery
- Performance-optimized reflection

```csharp
using Nglib.APP.CODE;

// Type resolution
Type type = ReflectionTools.ResolveType("System.String");
object instance = ReflectionTools.CreateInstance(type);

// Property manipulation
var user = new User { Name = "John" };
PropertiesTools.SetValue(user, "Name", "Jane");
string name = PropertiesTools.GetValue<string>(user, "Name");

// Find methods with attribute
var methods = AttributesTools.GetMethodsWithAttribute<MyAttribute>(typeof(MyClass));
```

[📖 View APP/CODE Documentation](docs/wiki_components_appcode.md)

---

### ✅ [APP/DIAG](docs/wiki_components_appdiag.md)
Validation, diagnostics, and error handling utilities.

**Features**:
- `ValidateModel` (Result/Either pattern)
- `StopwatchLogAt` (performance monitoring)
- `CascadeException` (contextual errors)

```csharp
using Nglib.APP.DIAG;

// Result pattern
var result = ValidateModel.Success;
if (string.IsNullOrEmpty(input))
    result = ValidateModel.Invalid("Input is required");

result.EnsureIsValid(); // Throws if invalid

// Performance monitoring
var sw = new StopwatchLogAt("Operation");
// ... do work ...
sw.LogAt("Step 1");
// ... more work ...
sw.LogAt("Step 2");
Console.WriteLine(sw.ToString()); // Shows timing for each step

// Combine validations
var result = ValidateModel.Combine(
    ValidateEmail(email),
    ValidateAge(age),
    ValidatePassword(password)
);
```

[📖 View APP/DIAG Documentation](docs/wiki_components_appdiag.md)

---

## 📖 Complete Documentation

**Component Guides**:
- [FORMAT - String & Date Utilities](docs/wiki_components_format.md)
- [FORMULA - Expression Engine](docs/wiki_components_formula.md)
- [DATA/DATAPO - ORM Framework](docs/wiki_components_datapo.md)
- [DATA/CONNECTOR - Database Connector](docs/wiki_components_connector.md)
- [DATA/QUERYBUILDER - SQL Query Builder](docs/wiki_components_querybuilder.md)
- [NET/HTTPCLIENT - HTTP Wrapper](docs/wiki_components_httpclient.md)
- [DATA/ACCESSORS - Data Access Pattern](docs/wiki_components_accessors.md)
- [DATA/COLLECTIONS - Collection Tools](docs/wiki_components_collections.md)
- [APP/CODE - Reflection Utils](docs/wiki_components_appcode.md)
- [APP/DIAG - Validation & Diagnostics](docs/wiki_components_appdiag.md)
- [SECURITY - Crypto & Identity](docs/wiki_components_security.md)

**General Docs**:
- [📚 Documentation Index](docs/Home.md)
- [📦 NuGet Package](https://www.nuget.org/packages/Nglib)
- [📄 License (MIT)](Licence.md)

## 🤝 Contributing & License

Contributions are welcome! Feel free to open issues or submit pull requests.

This project is licensed under the MIT License - see the [LICENSE](Licence.md) file for details.



