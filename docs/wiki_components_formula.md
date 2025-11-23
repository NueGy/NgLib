[◀ Back to Documentation Home](../README.md)

# FORMULA

**Namespace:** `Nglib.FORMULA`  
**Package:** `Nglib.Formula`

## Overview

Dynamic formula evaluation engine for calculating expressions and embedding formulas within strings. Supports parameters, custom functions, operators, and composed string templating.

**Main Classes:**
- `FormulaTools` - Main entry point for formula evaluation
- `FormulaContext` - Context object carrying formula state and results
- `FormulaSegmentModel` - Parsed formula segment (AST node)
- `FormulaParseTools` - Formula parsing utilities
- `FormulaExecuteTools` - Formula execution engine
- `FormulaAttribute` - Attribute for custom function registration
- `FormulaException` - Specialized exception for formula errors

**Project:** `Nglib.Formula` (separate assembly)

## Quick Start

### Simple Evaluation
```csharp
// Basic arithmetic
string result = FormulaTools.Eval("5 + 3"); // "8"
string result2 = FormulaTools.Eval("mul(4, 5)"); // "20"

// With parameters
var parameters = new Dictionary<string, object>
{
    ["price"] = 100,
    ["quantity"] = 5
};
string total = FormulaTools.Eval("mul(@price, @quantity)", parameters); // "500"
```

### Composed String (Template)
```csharp
// Embed formulas in text with {=formula}
var parameters = new Dictionary<string, object>
{
    ["user"] = "John",
    ["age"] = 25
};

string template = "Hello {=@user}, in 5 years you'll be {=add(@age, 5)} years old";
string result = FormulaTools.EvalComposedString(template, parameters);
// Result: "Hello John, in 5 years you'll be 30 years old"
```

## FormulaTools

Main static class for formula evaluation.

### Methods

#### Eval()
Evaluates a single formula expression:
```csharp
string result = FormulaTools.Eval(
    formuleStr: "add(10, 20)",
    globalParameters: null
);
```

#### EvalComposedString()
Evaluates formulas embedded in text:
```csharp
string result = FormulaTools.EvalComposedString(
    composedStr: "Price: {=mul(@price, 1.2)} (including tax)",
    globalParameters: parameters,
    safe: false // If true, failed formulas become "{}" instead of throwing
);
```

#### LoadFunctions()
Registers custom functions:
```csharp
// Define custom functions in a class
public static class MyFunctions
{
    [Formula("double", MethodMode = MethodModeEnum.ObjectArray)]
    public static object Double(object[] args)
    {
        return Convert.ToDouble(args[0]) * 2;
    }
}

// Load them
int count = FormulaTools.LoadFunctions(typeof(MyFunctions));
```

#### CalculateAsync()
Asynchronous formula calculation:
```csharp
var context = FormulaTools.ParseContext("mul(5, 10)");
bool success = await FormulaTools.CalculateAsync(context);
string result = context.GetValueString(); // "50"
```

## Formula Syntax

### Literals
```csharp
// Numbers
"123"       // Integer
"3.14"      // Decimal
"-42"       // Negative

// Strings
"'hello'"   // Single quotes
"\"world\"" // Double quotes
```

### Parameters
Access external values with `@` prefix:
```csharp
var parameters = new Dictionary<string, object>
{
    ["name"] = "Alice",
    ["score"] = 95
};

FormulaTools.Eval("@name", parameters);  // "Alice"
FormulaTools.Eval("@score", parameters); // "95"
```

### Functions
Call built-in or custom functions:
```csharp
"add(5, 3)"              // 8
"mul(4, 2.5)"            // 10
"concat('Hello', ' ', 'World')" // "Hello World"
"if(gt(@score, 90), 'A', 'B')"  // Conditional
```

### Operators
Standard arithmetic and comparison:
```csharp
"5 + 3"    // Addition
"10 - 4"   // Subtraction
"6 * 7"    // Multiplication
"20 / 4"   // Division
"10 == 10" // Equality
"5 >= 3"   // Greater or equal
```

### Nested Expressions
```csharp
"add(mul(2, 3), 4)"  // (2*3) + 4 = 10
"if(gt(@age, 18), 'Adult', 'Minor')"
```

## FormulaContext

Context object holding formula state and execution details.

### Properties
```csharp
public class FormulaContext
{
    // Parsed formula tree
    public FormulaSegmentModel RootSegment { get; set; }
    
    // Input parameters
    public Dictionary<string, object> Parameters { get; set; }
    
    // Calculation result
    public object ResultValue { get; set; }
    
    // Execution time in milliseconds
    public long ElapsedTime { get; set; }
    
    // Calculated values for each segment
    public Dictionary<FormulaSegmentModel, object> CalcValues { get; }
}
```

### Usage
```csharp
var context = new FormulaContext
{
    RootSegment = parsedFormula,
    Parameters = parameters
};

await FormulaTools.CalculateAsync(context);

Console.WriteLine($"Result: {context.ResultValue}");
Console.WriteLine($"Time: {context.ElapsedTime}ms");
```

## FormulaSegmentModel

Represents a parsed formula segment (AST node).

### Segment Types (FormulaSegmentTypeEnum)
- `ValNumber` - Numeric literal (123, 3.14)
- `ValString` - String literal ('hello', "world")
- `ValParameter` - Parameter reference (@price, @user)
- `Function` - Function call (add(), mul())
- `Operator` - Operator (+, -, *, /, ==, >=)
- `FormulaContent` - Raw formula content

### Structure
```csharp
public class FormulaSegmentModel
{
    public string Text { get; set; }                // Raw text
    public FormulaSegmentTypeEnum Type { get; set; } // Segment type
    public List<FormulaSegmentModel> Children { get; } // Sub-segments
}
```

## Built-in Functions

### Arithmetic
- `add(a, b)` - Addition
- `sub(a, b)` - Subtraction
- `mul(a, b)` - Multiplication
- `div(a, b)` - Division
- `mod(a, b)` - Modulo

### Math
- `ceil(n)` - Ceiling
- `floor(n)` - Floor
- `round(n)` - Rounding
- `abs(n)` - Absolute value

### Comparison
- `eq(a, b)` - Equal (==)
- `neq(a, b)` - Not equal (!=)
- `gt(a, b)` - Greater than (>)
- `gte(a, b)` - Greater or equal (>=)
- `lt(a, b)` - Less than (<)
- `lte(a, b)` - Less or equal (<=)

### Logic
- `if(condition, trueValue, falseValue)` - Conditional
- `and(a, b)` - Logical AND
- `or(a, b)` - Logical OR
- `not(a)` - Logical NOT

### String
- `concat(str1, str2, ...)` - Concatenation
- `substr(str, start, length)` - Substring
- `upper(str)` - Uppercase
- `lower(str)` - Lowercase
- `trim(str)` - Trim whitespace

## Custom Functions

Register your own functions using the `[Formula]` attribute:

```csharp
public static class CustomFunctions
{
    // String array mode (default)
    [Formula("myFunc")]
    public static string MyFunction(string[] args)
    {
        return $"Result: {args[0]}";
    }
    
    // Object array mode
    [Formula("calculate", MethodMode = MethodModeEnum.ObjectArray)]
    public static object Calculate(object[] args)
    {
        int a = Convert.ToInt32(args[0]);
        int b = Convert.ToInt32(args[1]);
        return a * b + 10;
    }
    
    // Context mode (full access)
    [Formula("advanced", MethodMode = MethodModeEnum.Context)]
    public static object Advanced(FormulaContext context)
    {
        // Access context.Parameters, context.RootSegment, etc.
        return context.Parameters["specialValue"];
    }
}

// Load functions
FormulaTools.LoadFunctions(typeof(CustomFunctions));

// Use them
string result = FormulaTools.Eval("myFunc('test')");
string result2 = FormulaTools.Eval("calculate(5, 3)"); // "25"
```

## FormulaAttribute

Attribute for marking custom formula functions.

### Properties
```csharp
[AttributeUsage(AttributeTargets.Method)]
public class FormulaAttribute : Attribute
{
    public string FunctionName { get; set; }
    public MethodModeEnum MethodMode { get; set; } = MethodModeEnum.StringArray;
    public FormulaDataType ReturnType { get; set; }
}
```

### Usage
```csharp
[Formula("functionName", MethodMode = MethodModeEnum.ObjectArray)]
public static object MyCustomFunction(object[] args) { }
```

## FormulaException

Specialized exception for formula errors.

```csharp
try
{
    string result = FormulaTools.Eval("invalid_function()");
}
catch (FormulaException ex)
{
    Console.WriteLine($"Command: {ex.Command}");
    Console.WriteLine($"Message: {ex.Message}");
}
```

## Advanced Examples

### Temperature Converter
```csharp
var parameters = new Dictionary<string, object>
{
    ["celsius"] = 25
};

string formula = "add(mul(@celsius, 1.8), 32)";
string fahrenheit = FormulaTools.Eval(formula, parameters); // "77"
```

### Invoice Calculator
```csharp
var invoice = new Dictionary<string, object>
{
    ["subtotal"] = 100,
    ["tax_rate"] = 0.20,
    ["discount"] = 10
};

string template = @"
Subtotal: ${=@subtotal}
Tax: ${=mul(@subtotal, @tax_rate)}
Discount: -${=@discount}
Total: ${=sub(add(@subtotal, mul(@subtotal, @tax_rate)), @discount)}
";

string result = FormulaTools.EvalComposedString(template, invoice);
// Subtotal: $100
// Tax: $20
// Discount: -$10
// Total: $110
```

### Conditional Logic
```csharp
var user = new Dictionary<string, object>
{
    ["age"] = 20,
    ["premium"] = true
};

string message = FormulaTools.EvalComposedString(
    "Status: {=if(and(gt(@age, 18), @premium), 'Premium Adult', 'Standard')}",
    user
);
// Status: Premium Adult
```

## Performance

- **Parsing**: Formulas are parsed into AST on first evaluation
- **Caching**: Consider caching `FormulaContext` objects for repeated evaluations
- **Async**: Use `CalculateAsync()` for long-running formulas
- **ElapsedTime**: Check `context.ElapsedTime` for performance monitoring

## Related Components
- **FORMAT**: StringTools, DateTools, ConvertTools used internally
- **APP/DIAG**: ValidateModel for formula validation results

## Dependencies
- `Nglib.FORMAT` (StringTools, ConvertTools)
- `Nglib.APP.DIAG` (ValidateModel for validation)
- `System.Reflection` (custom function loading)

## Notes
- **Thread-safe**: Formula evaluation is stateless (parameters passed explicitly)
- **Case-insensitive**: Function names are case-insensitive
- **Extensible**: Easy to add custom functions via attributes
- **Safe mode**: `EvalComposedString(safe: true)` prevents template rendering failures
- **No eval security**: Do not evaluate user-supplied formulas without validation (code injection risk)
