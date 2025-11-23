[◀ Back to Main README](../README.md)C# .Net library tools, Dev components, OpenSource



# NgLib Documentation Index### The project is based on two mains libraries.



> **NgLib** - A comprehensive .NET library providing reusable components for modern C# applications.  - **Nglib.dll**  Generic Tools,Abstractions, small library for a publication with Blazor ([Nuget](https://www.nuget.org/packages/Nglib))

> Open-source (MIT) - General and Common .NET Helpers, Extensions, Data Access, HTTP Client, Formula Engine, and more.

- **Nglib.Core.dll**  Tools, library for backends applications ([Nuget](https://www.nuget.org/packages/Nglib.core))

---



## 📦 NuGet Packages

# Documentation COMPONENTS 

| Package | Description |Documentation for Api/Class/Namespace (Only main components)

| ------- | ----------- |

| **[Nglib](https://www.nuget.org/packages/Nglib/)** | Core abstractions, basic tools, zero dependencies |### APP

| **[Nglib.Data](https://www.nuget.org/packages/Nglib.Data/)** | Data access components, DataPO ORM, providers |* **CODE** Tools for reflection, Compilation

| **[Nglib.Formula](https://www.nuget.org/packages/Nglib.Formula/)** | Dynamic formula evaluation engine |* **DIAG/PROCESS**   _Application exe/console Tools_

 

---

### DATA

## 📚 Components Documentation* [ACCESSORS](wiki_components_accessors.md) Interface for object manipulation GetString(), GetInt(), GetDateTime(), GetEnum(), ...

* [CONNECTOR](wiki_components_connector.md) Extension of ADO.NET/DbConnection functionality to launch SQL queries with minimal code

### 🛠️ Application (APP)* [DATAPO](wiki_components_datapo.md) Manipulate database objects (ADO.NET DataTable/DataRow). Providers for common persistence operations

* [KEYVALUES](wiki_components_keyvalues.md) Serializable data dictionary in Json format 

- **[APP.CODE](wiki_components_appcode.md)** - Reflection utilities, property manipulation, and attribute discovery* [COLLECTIONS](wiki_components_collections.md) Extensions and tools to manipulate collections

- **[APP.DIAG](wiki_components_appdiag.md)** - Diagnostics, validation, and error handling utilities

### FORMAT

### 🗄️ Data Access (DATA)* **NumberTools,DateTools,ObjectbaseTools,StringTools,...** Additional manipulation tools

* [KeyTools](wiki_components_format.md) Generate/Parse Nglib Custom ID

- **[DATA.ACCESSORS](wiki_components_accessors.md)** - Unified data access interface with type-safe operations

- **[DATA.COLLECTIONS](wiki_components_collections.md)** - Collection utilities and safe manipulation extensions###  SECURITY

- **[DATA.CONNECTOR](wiki_components_connector.md)** - Multi-database connector (SQL Server, PostgreSQL, MySQL, SQLite, Oracle)* [CRYPTO](wiki_components_security.md) Hash/AES & Cryptography Tools

- **[DATA.DATAMODEL](wiki_components_datamodel.md)** - Automatic HTML form generation from API data models

- **[DATA.DATAPO](wiki_components_datapo.md)** - Hybrid ADO.NET/NoSQL ORM with Active Record pattern

- **[DATA.QUERYBUILDER](wiki_components_querybuilder.md)** - Fluent SQL query builder (multi-DBMS)



### 🔤 Formatting & Parsing (FORMAT)

## In french

- **[FORMAT](wiki_components_format.md)** - String manipulation, date/time utilities, and type conversion

- **[FORMULA](wiki_components_formula.md)** - Dynamic formula evaluation engine with template stringsIl s’agit d’une bibliothèque open-source d'outils génériques, des extensions, des utilitaires ADO.net,  etc ...



### 📡 Network (NET)Vous trouverez plus d'informations dans le wiki/code.

- **[NET.HTTPCLIENT](wiki_components_httpclient.md)** - HTTP client wrapper with OAuth2 and attribute-based modeling

### 🔐 Security (SECURITY)

- **[SECURITY](wiki_components_security.md)** - Cryptography tools (AES256, JWT) and identity management

---

## 🚀 Quick Start

```bash
# Install core package (zero dependencies)
dotnet add package Nglib

# For data access features
dotnet add package Nglib.Data

# For formula engine
dotnet add package Nglib.Formula
```

## 📖 Usage Examples

### String & Date Utilities
```csharp
using Nglib.FORMAT;

// String manipulation
string random = StringTools.RandomString(12);
string cleaned = StringTools.CleanString("Hello  World!");

// Date utilities
int timestamp = DateTools.DateTimeToTimeStamp(DateTime.Now);
string date8 = DateTools.ConvertDateTime8(DateTime.Now); // "20251009"
```

### Formula Engine
```csharp
using Nglib.FORMULA;

var context = new FormulaContext();
context.Parameters.Add("price", 100);
context.Parameters.Add("quantity", 5);

string result = FormulaTools.Eval("@price * @quantity", context); // "500"
string template = "Total: {=@price * @quantity} EUR";
string output = FormulaTools.EvalComposedString(template, context); // "Total: 500 EUR"
```

### Data Access
```csharp
using Nglib.DATA.ACCESSORS;

// Unified data access
var data = new DictionaryData();
data.SetValue("name", "John");
data.SetValue("age", 30);

string name = data.GetValue<string>("name");
int age = data.GetValue<int>("age");
```

### HTTP Client with OAuth2
```csharp
using Nglib.NET.HTTPCLIENT;

var config = new HttpClientConfigModel {
    AuthType = TokenAuthTypeEnum.OAuth2Client,
    ClientId = "your-client-id",
    ClientSecret = "your-secret",
    TokenEndpointUrl = "https://auth.example.com/token"
};

var handler = new HttpClientTokenHandler { Config = config };
var client = new HttpClient(handler);
var response = await client.GetAsync("https://api.example.com/data");
```

---

## 🔗 Useful Links

- [📦 NuGet Package - Nglib](https://www.nuget.org/packages/Nglib)
- [📦 NuGet Package - Nglib.Data](https://www.nuget.org/packages/Nglib.Data)
- [📦 NuGet Package - Nglib.Formula](https://www.nuget.org/packages/Nglib.Formula)
- [📄 License (MIT)](../Licence.md)
- [🏠 Main Repository](https://github.com/NueGy/NgLibComponents)

---

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](../Licence.md) file for details.
