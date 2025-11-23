# Nglib.Formula

**Formula parser and evaluator - Expression engine for dynamic string calculations**

Nglib.Formula is a powerful expression parser and evaluator that allows you to parse and execute dynamic formulas with parameters, functions, and operators.

  [Read Documentation](https://github.com/NueGy/NgLib/docs/wiki_components_formula.md)

## Features

- **Expression Parsing**: Parse complex mathematical and logical expressions
- **Parameter Support**: Dynamic parameter injection with `@parameter` syntax
- **Built-in Functions**: Math, string, date, and logical functions
- **Custom Functions**: Register your own custom functions
- **Operators**: Arithmetic (+, -, *, /), comparison (==, !=, <, >, <=, >=), logical (&&, ||)
- **Type Safety**: Automatic type conversion and validation
- **Performance**: Optimized parsing with segment caching
- **Extensible**: Easy to add new functions and operators

## Installation

```bash
dotnet add package Nglib.Formula
```

**Note**: Requires `Nglib` package as dependency.

## Quick Start

### Simple Evaluation
```csharp
using Nglib.FORMAT.FORMULA;

// Simple calculation
var result = FormulaTools.Evaluate("2 + 3 * 4"); // Returns 14

// With parameters
var context = new FormulaContext();
context.SetParameter("price", 100);
context.SetParameter("tax", 0.20);

var total = FormulaTools.Evaluate("@price * (1 + @tax)", context); // Returns 120
```

### Using Functions
```csharp
// Built-in functions
var result = FormulaTools.Evaluate("add(10, 20, 30)"); // Returns 60
var result = FormulaTools.Evaluate("mul(5, 3)"); // Returns 15
var result = FormulaTools.Evaluate("max(10, 25, 15)"); // Returns 25

// String functions
var result = FormulaTools.Evaluate("concat('Hello', ' ', 'World')"); // Returns "Hello World"
var result = FormulaTools.Evaluate("upper('hello')"); // Returns "HELLO"

// Conditional
var result = FormulaTools.Evaluate("if(10 > 5, 'yes', 'no')"); // Returns "yes"
```

### Complex Formulas
```csharp
var context = new FormulaContext();
context.SetParameter("quantity", 5);
context.SetParameter("unitPrice", 25.50);
context.SetParameter("discountRate", 0.10);

var formula = "mul(@quantity, @unitPrice) * (1 - @discountRate)";
var total = FormulaTools.Evaluate(formula, context); // Returns 114.75
```

### Custom Functions
```csharp
FormulaTools.RegisterFunction("double", args => {
    var value = Convert.ToDouble(args[0]);
    return value * 2;
});

var result = FormulaTools.Evaluate("double(21)"); // Returns 42
```

### Segment Parsing (Advanced)
```csharp
using Nglib.FORMAT.FORMULA.SEGMENT;

// Parse once, execute multiple times
var segments = FormulaSegmentParseTools.Parse("@price * (1 + @tax)");

// Execute with different contexts
var context1 = new FormulaContext();
context1.SetParameter("price", 100);
context1.SetParameter("tax", 0.20);
var result1 = FormulaTools.EvaluateSegments(segments, context1);

var context2 = new FormulaContext();
context2.SetParameter("price", 200);
context2.SetParameter("tax", 0.15);
var result2 = FormulaTools.EvaluateSegments(segments, context2);
```

## Built-in Functions

### Math
- `add(x, y, ...)` - Addition
- `sub(x, y)` - Subtraction
- `mul(x, y, ...)` - Multiplication
- `div(x, y)` - Division
- `mod(x, y)` - Modulo
- `abs(x)` - Absolute value
- `max(x, y, ...)` - Maximum
- `min(x, y, ...)` - Minimum
- `round(x, decimals)` - Round

### String
- `concat(str1, str2, ...)` - Concatenate strings
- `upper(str)` - Uppercase
- `lower(str)` - Lowercase
- `substring(str, start, length)` - Extract substring
- `length(str)` - String length

### Logical
- `if(condition, trueValue, falseValue)` - Conditional
- `isnull(value, default)` - Null check
- `equals(x, y)` - Equality check

### Date
- `now()` - Current date/time
- `dateadd(date, days)` - Add days to date
- `datediff(date1, date2)` - Difference in days




## 📚 Documentation

Full documentation: [https://github.com/NueGy/NgLib](https://github.com/NueGy/NgLib)  
Formula documentation   [https://github.com/NueGy/NgLib/docs/wiki_components_formula.md](https://github.com/NueGy/NgLib/docs/wiki_components_formula.md)

### 🔗 Links

- [GitHub Repository](https://github.com/NueGy/NgLib)
- [Report Issues](https://github.com/NueGy/NgLib/issues)
- [NuGet Package](https://www.nuget.org/packages/Nglib)
- [View License](https://github.com/NueGy/NgLib/blob/master/Licence.md) (MIT License)
- [Nuegy.net](https://www.nuegy.net) (Agency website)
