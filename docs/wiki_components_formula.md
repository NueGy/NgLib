[◀ Back to Documentation Home](../README.md)
# FORMULA

**Namespace:** `Nglib.FORMULA`  
**Package:** `Nglib.Formula`

Dynamic formula evaluation engine for calculating expressions and embedding formulas within strings. Supports parameters, custom functions, operators, and composed string templating.

## Quick Start

### Simple Evaluation
```csharp
// Basic arithmetic
string result = FormulaTools.Eval("5 + 3"); // "8"
string result2 = FormulaTools.Eval("if(gt(mul(4, 5), 10), upper('yes'), 'no')"); // "YES"

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


## Formula list
List of built-in functions, operators, and syntax supported by the formula engine.

### Operators

| Operator | Description | Example |
|---|---|---|
| `+` | Addition or string concatenation | `5 + 3` → `8`, `'a' + 'b'` → `ab` |
| `-` | Subtraction | `10 - 4` → `6` |
| `*` | Multiplication | `6 * 7` → `42` |
| `/` | Division | `20 / 4` → `5` |
| `==` or `=` | Equality | `10 == 10` → `1` |
| `!=` or `<>` | Inequality | `5 != 3` → `1` |
| `>` | Greater than | `5 > 3` → `1` |
| `>=` | Greater or equal | `5 >= 5` → `1` |
| `<` | Less than | `3 < 5` → `1` |
| `<=` | Less or equal | `3 <= 5` → `1` |

### Arithmetic Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `add(a, b)` | 2 | Addition | `add(3, 4)` → `7` |
| `sub(a, b)` | 2 | Subtraction | `sub(10, 3)` → `7` |
| `mul(a, b)` | 2 | Multiplication | `mul(3, 4)` → `12` |
| `div(a, b)` | 2 | Division | `div(10, 2)` → `5` |
| `mod(a, b)` | 2 | Modulo | `mod(10, 3)` → `1` |
| `abs(n)` | 1 | Absolute value | `abs(-5)` → `5` |
| `round(n, d)` | 2 | Round to d decimals | `round(3.456, 2)` → `3.46` |
| `floor(n)` | 1 | Round down | `floor(3.9)` → `3` |
| `ceil(n)` | 1 | Round up | `ceil(3.1)` → `4` |
| `min(a, b, ...)` | 2+ | Minimum value | `min(5, 2, 8)` → `2` |
| `max(a, b, ...)` | 2+ | Maximum value | `max(5, 2, 8)` → `8` |
| `sum(a, b, ...)` | 2+ | Sum of all args | `sum(1, 2, 3)` → `6` |
| `avg(a, b, ...)` | 2+ | Average | `avg(2, 4, 6)` → `4` |
| `Between(v, min, max)` | 3 | Value between min and max (inclusive) | `Between(15, 10, 20)` → `1` |

### Numeric Conversion Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `IsNumeric(s)` | 1 | Returns `1` if string is numeric | `IsNumeric('123')` → `1`, `IsNumeric('abc')` → `0` |
| `ToDouble(s)` | 1 | Parse string to double | `ToDouble('1 234,56')` → `1234.56` |
| `ToDecimal(s)` | 1 | Parse string to decimal | `ToDecimal('1.234,56')` → `1234.56` |
| `ToInt(s)` | 1 | Parse string to integer (Int64) | `ToInt('1 234')` → `1234` |

> Handles localized formats: `"1 234,56"`, `"1.234,56"`, `"1,234.56"`

### Amount Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `amt(s)` | 1 | Parse monetary string to double | `amt('1 586,45')` → `1586.45` |
| `amtf(s)` | 1 | Format double as amount string (2 decimals) | `amtf('1586.4')` → `1586.40` |
| `amtct(s)` | 1 | Convert amount to integer cents | `amtct('1 586.45')` → `158645` |
| `KeyMod(s[, mod])` | 1-2 | Control key (modulo, default 100) | `KeyMod('12345')` → `09` |

### Logic Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `if(cond, true, false)` | 3 | Conditional | `if(gt(5,3), 'Yes', 'No')` → `Yes` |
| `and(a, b, ...)` | 2+ | All values are true | `and(1, 1)` → `1` |
| `or(a, b, ...)` | 1+ | At least one value is true | `or(0, 1)` → `1` |
| `not(a)` | 1 | Inverts boolean | `not(0)` → `1` |
| `isnull(a, b, ...)` | 2+ | Returns first non-null value | `isnull(null, 'default')` → `default` |
| `nullif(a, b)` | 2 | Returns null if a == b | `nullif('x', 'x')` → `null` |
| `error(msg)` | 1 | Throws an exception | `error('msg')` |

### Comparison Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `eq(a, b, ...)` | 2+ | All equal (case-insensitive) | `eq('abc', 'ABC')` → `1` |
| `equal(a, b)` | 2 | Strictly equal (type + value) | `equal('1', '1')` → `1` |
| `gt(a, b)` | 2 | Greater than | `gt(5, 3)` → `1` |
| `gteq(a, b)` | 2 | Greater or equal | `gteq(5, 5)` → `1` |
| `lt(a, b)` | 2 | Less than | `lt(3, 5)` → `1` |
| `lteq(a, b)` | 2 | Less or equal | `lteq(3, 5)` → `1` |

### String Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `concat(a, b, ...)` | 2+ | Concatenation | `concat('a','b','c')` → `abc` |
| `upper(s)` | 1 | Uppercase | `upper('hello')` → `HELLO` |
| `lower(s)` | 1 | Lowercase | `lower('HELLO')` → `hello` |
| `trim(s[, c])` | 1-2 | Trim whitespace or character | `trim('  hi  ')` → `hi` |
| `ltrim(s[, c])` | 1-2 | Trim left | `ltrim('  hi')` → `hi` |
| `rtrim(s[, c])` | 1-2 | Trim right | `rtrim('hi  ')` → `hi` |
| `left(s, n)` | 2 | First n characters | `left('Hello', 3)` → `Hel` |
| `right(s, n)` | 2 | Last n characters | `right('Hello', 3)` → `llo` |
| `len(s)` | 1 | String length | `len('Hello')` → `5` |
| `Substring(s, start[, len])` | 2-3 | Substring (safe) | `Substring('Hello', 1, 3)` → `ell` |
| `replace(s, old, new)` | 3 | Replace substring | `replace('ab','b','c')` → `ac` |
| `Contains(s, search)` | 2 | Contains (case-insensitive) | `Contains('Hello','ell')` → `1` |
| `StartsWith(s, prefix)` | 2 | Starts with (case-insensitive) | `StartsWith('Hello','he')` → `1` |
| `EndsWith(s, suffix)` | 2 | Ends with (case-insensitive) | `EndsWith('Hello','lo')` → `1` |
| `strindex(s, search)` | 2 | Index of substring | `strindex('Hello','ll')` → `2` |
| `strlastindex(s, search)` | 2 | Last index of substring | `strlastindex('abab','b')` → `3` |
| `char(s, i)` | 2 | Character at position | `char('Hello', 0)` → `H` |
| `ReplaceDiacritics(s)` | 1 | Remove accents | `ReplaceDiacritics('café')` → `cafe` |

### String Validation & Filtering

| Function | Args | Description | Example |
|---|---|---|---|
| `IsAlphaNumeric(s)` | 1 | Returns `1` if only letters and digits | `IsAlphaNumeric('abc1')` → `1` |
| `OnlyAlphanumeric(s)` | 1 | Keeps only letters and digits | `OnlyAlphanumeric('ab-1!')` → `ab1` |
| `OnlyNumeric(s)` | 1 | Keeps only digit characters | `OnlyNumeric('Year: 2024')` → `2024` |
| `islen(s, n)` or `islen(s, min, max)` | 2-3 | Checks string length | `islen('abc', 3)` → `1` |

### Padding Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `PadNumeric(s, n)` | 2 | Left-pad with zeros | `PadNumeric('815', 6)` → `000815` |
| `PadLeft(s, n, c)` | 3 | Left-pad with character | `PadLeft('5', 3, '0')` → `005` |
| `PadRight(s, n, c)` | 3 | Right-pad with character | `PadRight('5', 3, '0')` → `500` |

### Date Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `now()` | 0 | Current date/time | `now()` → `22/05/2026 14:30:00` |
| `day(d)` | 1 | Day of date | `day(now())` → `22` |
| `month(d)` | 1 | Month of date | `month(now())` → `5` |
| `year(d)` | 1 | Year of date | `year(now())` → `2026` |
| `todate(d, fmt)` | 2 | Reformat a date | `todate(now(), 'yyyy-MM-dd')` |
| `dateabs(d)` | 1 | Date without time | `dateabs(now())` → `22/05/2026` |
| `datediff(d1, d2)` | 2 | Difference in days | `datediff(d1, d2)` → `5` |
| `addday(d, n)` | 2 | Add days | `addday(now(), 7)` |
| `addmonth(d, n)` | 2 | Add months | `addmonth(now(), 1)` |
| `addyear(d, n)` | 2 | Add years | `addyear(now(), 1)` |
| `addhour(d, n)` | 2 | Add hours | `addhour(now(), 2)` |
| `addminute(d, n)` | 2 | Add minutes | `addminute(now(), 30)` |
| `addsecond(d, n)` | 2 | Add seconds | `addsecond(now(), 10)` |
| `PartOfDate(d, part)` | 2 | Get a date part | `PartOfDate(now(), 'FirstDayOfMonth')` |
| `ToTimestamp(d)` | 1 | DateTime to Unix timestamp | `ToTimestamp(now())` → `1716379200` |
| `FromTimestamp(t)` | 1 | Unix timestamp to DateTime | `FromTimestamp('1716379200')` |

### Encoding Functions

| Function | Args | Description | Example |
|---|---|---|---|
| `ToBase64(s)` | 1 | Encode to Base64 | `ToBase64('hello')` → `aGVsbG8=` |
| `FromBase64(s)` | 1 | Decode from Base64 | `FromBase64('aGVsbG8=')` → `hello` |


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



## Developer Guide

Main static class for formula evaluation.

**Main Classes:**
- `FormulaTools` - Main entry point for formula evaluation
- `FormulaContext` - Context object carrying formula state and results
- `FormulaSegmentModel` - Parsed formula segment (AST node)
- `FormulaParseTools` - Formula parsing utilities
- `FormulaExecuteTools` - Formula execution engine
- `FormulaAttribute` - Attribute for custom function registration
- `FormulaException` - Specialized exception for formula errors


### FormulaContext

Context object holding formula state and execution details.

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

### Custom Functions

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

// Load functions (Registers custom functions:)
FormulaTools.LoadFunctions(typeof(CustomFunctions));

// Use them
string result = FormulaTools.Eval("myFunc('test')");
string result2 = FormulaTools.Eval("calculate(5, 3)"); // "25"
```



## Divers
### Related Components
- **FORMAT**: StringTools, DateTools, ConvertTools used internally
- **APP/DIAG**: ValidateModel for formula validation results

### Dependencies
- `Nglib.FORMAT` (StringTools, ConvertTools)
- `Nglib.APP.DIAG` (ValidateModel for validation)
- `System.Reflection` (custom function loading)

### Notes
- **Thread-safe**: Formula evaluation is stateless (parameters passed explicitly)
- **Case-insensitive**: Function names are case-insensitive
- **Extensible**: Easy to add custom functions via attributes
- **Safe mode**: `EvalComposedString(safe: true)` prevents template rendering failures
- **No eval security**: Do not evaluate user-supplied formulas without validation (code injection risk)
