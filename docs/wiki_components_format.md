[◀ Back to Documentation Home](../README.md)

# FORMAT

**Namespace:** `Nglib.FORMAT`

## Overview

Utility classes for string manipulation, date/time operations, type conversion, number handling, and key sanitization. Foundational formatting tools used across the library.

**Main Classes:**
- `StringTools` - String manipulation and sanitization
- `DateTools` - Date/time conversion and formatting
- `ConvertTools` - Enhanced type conversion
- `KeyTools` - Key sanitization and base36 encoding
- `NumberTools` - Number validation and operations
- `ObjectBaseTools` - Base conversion utilities (obsolete/beta)

## StringTools

Static class for string manipulation, cleaning, and formatting operations.

### Key Features
- **Random generation**: Thread-safe random strings and GUIDs
- **Validation**: Alphanumeric checking
- **Sanitization**: Diacritics removal, character filtering, cleaning
- **Safe operations**: Substring, limit with out-of-range handling
- **Parsing**: Tag splitting, encapsulated string extraction

### Methods

#### Random Generation
```csharp
// Random alphanumeric string
string code = StringTools.RandomString(8); // e.g., "a3k9p2m1"
string code = StringTools.RandomString(10, "0123456789");

// Time-based GUID (32 chars, no hyphens)
string guid = StringTools.RandomGuid32(); // e.g., "1f23a4b5..."
```

#### Validation
```csharp
// Check if contains only letters and numbers
bool valid = StringTools.IsAlphaNumeric("abc123"); // true
bool invalid = StringTools.IsAlphaNumeric("abc-123"); // false
```

#### Character Filtering
```csharp
// Keep only alphanumeric characters
string clean = StringTools.FilterCharacters("Hello-World!"); // "HelloWorld"
string custom = StringTools.FilterCharacters("a1-b2+c3", "abc123"); // "abc"
```

#### String Limiting
```csharp
// Safe substring operations
string limited = StringTools.Limit("LongString", 4); // "Long"
string sub1 = StringTools.SubstringSafe("Hello", 2); // "llo"
string sub2 = StringTools.SubstringSafe("Hi", 10); // "" (no error)
string sub3 = StringTools.SubstringSafe("Hello", 1, 3); // "ell"
```

#### Diacritics Removal
```csharp
// Replace accented characters
string clean = StringTools.ReplaceDiacritics("café"); // "cafe"
string clean2 = StringTools.ReplaceDiacritics("naïve"); // "naive"
```

#### String Sanitization
```csharp
// Remove problematic characters (XML, line breaks, special chars)
string clean = StringTools.CleanString("Hello\r\nWorld!<>"); // "Hello World"
```

#### Character Replacement
```csharp
// Replace character at position (extension method)
string result = "Hello".ReplaceChar(1, 'a'); // "Hallo"
```

#### Tag Splitting
```csharp
// Split CSV-style tags (semicolon separated)
string[] tags = StringTools.SplitTag("TAG1;tag2;TAG3", toUpper: true);
// Result: ["TAG1", "TAG2", "TAG3"]

string[] tags2 = StringTools.SplitTag("a;;b", neverNull: true);
// Result: ["A", "B"] (empty values removed)
```

#### Encapsulated String Extraction
```csharp
// Extract strings between delimiters
string input = "text{value1}more{value2}end";
string[] parts = StringTools.SplitEncapsuled(input, "{", "}");
// Result: ["{value1}", "{value2}"]
```

## DateTools

Static class for date/time manipulation and conversion.

### Methods

#### Time Difference Display
```csharp
// Human-readable time difference
string diff = DateTools.ToStringDateDelay(date1, date2);
// Possible outputs: "Now", "5 Sec", "10 Min", "2 Hr", "3 Days"
```

#### Conditional Date/Time Display
```csharp
// Show time if today, otherwise show date
string display = DateTools.ToStringDateOrTime(DateTime.Now); // "14:30:25"
string display2 = DateTools.ToStringDateOrTime(yesterday); // "07/10/2025"
```

#### Business Days Calculation
```csharp
// Add days excluding weekends/holidays
List<DayOfWeek> weekends = new() { DayOfWeek.Saturday, DayOfWeek.Sunday };
List<DateTime> holidays = new() { new DateTime(2025, 12, 25) };

DateTime result = DateTools.AddDaysWithExcludes(
    startDate: DateTime.Now,
    addDays: 5,
    excludesDates: holidays,
    excludesDays: weekends
);
```

#### Unix Timestamp Conversion
```csharp
// DateTime to Unix timestamp
long timestamp = DateTools.DateTimeToTimeStamp(DateTime.Now);
long current = DateTools.Time(); // Current UTC timestamp

// Unix timestamp to DateTime
DateTime dt = DateTools.TimeStampToDateTime(1697020800);
DateTime dtLocal = DateTools.TimeStampToDateTime(1697020800, useLocalTime: true);
```

#### 8-Character Date Format
```csharp
// Convert "20251008" format
DateTime date = DateTools.ConvertDateTime8("20251008");
```

#### Safe Parsing
```csharp
// TryParse with nullable return
DateTime? date = DateTools.TryParse("2025-10-08"); // DateTime
DateTime? invalid = DateTools.TryParse("invalid"); // null
```

## ConvertTools

Enhanced type converter with extended string parsing.

### Methods

#### Boolean Conversion
```csharp
// Extended boolean parsing
bool val1 = ConvertTools.ToBoolean("true"); // true
bool val2 = ConvertTools.ToBoolean("yes"); // true
bool val3 = ConvertTools.ToBoolean("on"); // true
bool val4 = ConvertTools.ToBoolean("1"); // true
bool val5 = ConvertTools.ToBoolean(0); // false

// Safe mode (no exceptions)
bool safe = ConvertTools.ToBoolean("invalid", safe: true); // false
```

#### DateTime Conversion
```csharp
// Standard and 8-char format support
DateTime dt1 = ConvertTools.ToDateTime("2025-10-08");
DateTime dt2 = ConvertTools.ToDateTime("20251008"); // 8-char format

// Safe mode
DateTime safe = ConvertTools.ToDateTime("invalid", safe: true); // DateTime.MinValue
```

#### Integer Conversion
```csharp
// With default value and boolean support
int val1 = ConvertTools.ToInt("123"); // 123
int val2 = ConvertTools.ToInt(null, defaultValue: 10); // 10
int val3 = ConvertTools.ToInt(true); // 1
int val4 = ConvertTools.ToInt("true"); // 1

// Safe mode
int safe = ConvertTools.ToInt("abc", defaultValue: 0, safe: true); // 0
```

#### Generic Type Conversion
```csharp
// Enhanced ChangeType with Nglib formatting
object result = ConvertTools.ChangeType("123", typeof(int)); // 123
object result2 = ConvertTools.ChangeType("true", "bool"); // true
object result3 = ConvertTools.ChangeType("20251008", "datetime"); // DateTime

// Type name parsing
Type type = ConvertTools.ParseType("int"); // typeof(int)
Type type2 = ConvertTools.ParseType("numeric"); // typeof(int)
Type type3 = ConvertTools.ParseType("date"); // typeof(DateTime)
```

### Supported Type Names
- `string`, `int`, `numeric`, `double`, `decimal`, `long`
- `bool`, `char`, `byte`
- `datetime`, `date`

## KeyTools

Static class for key sanitization and base36 encoding.

### Key Sanitization
```csharp
// Sanitize keys (alphanumeric + . _ -)
string key1 = KeyTools.SanitizeKey("Hello World!", upperStr: true); // "HELLOWORLD"
string key2 = KeyTools.SanitizeKey("café-123", upperStr: false); // "cafe123"
string key3 = KeyTools.SanitizeKey("test.key_01"); // "TEST.KEY_01"

// Validate key format
bool valid = KeyTools.IsValidKey("test_key.01"); // true
bool invalid = KeyTools.IsValidKey("test key!"); // false
```

### Base36 Key Generation (Beta/Obsolete)
```csharp
// Generate base36 key with date, tenant, item ID
string key = KeyTools.WriteKeyB36(
    DateIndex: DateTime.Now,
    tenantId: 123,
    itemId: 456789,
    prefix: "PRD"
);
// Result: "PRD-..." (base36 encoded with checksum)
```

## NumberTools

Static class for number validation and calculations.

### Methods

#### Validation
```csharp
// Check if string is numeric
bool valid1 = NumberTools.IsNumeric("123"); // true
bool valid2 = NumberTools.IsNumeric("12.5", allowDecimal: true); // true
bool invalid = NumberTools.IsNumeric("abc"); // false

// Check if contains any numbers
bool hasNum = NumberTools.HasNumeric("test123"); // true
bool noNum = NumberTools.HasNumeric("test"); // false
```

#### Amount Rounding
```csharp
// Round to 2 decimals
double amount = NumberTools.RoundAmount(123.456); // 123.46
```

#### Percentage Calculation
```csharp
// Calculate percentage (int/long/double overloads)
int percent1 = NumberTools.CalcPercent(50, 200); // 25
int percent2 = NumberTools.CalcPercent(75L, 300L); // 25
int percent3 = NumberTools.CalcPercent(1.5, 6.0); // 25
```

## ObjectBaseTools (Obsolete)

⚠️ **Status**: Marked as BETA/Obsolete

Base conversion utilities for hex and custom bases.

```csharp
// Bytes to hex string
byte[] bytes = { 0x1A, 0x2B, 0x3C };
string hex = ObjectBaseTools.HexStringFromBytes(bytes); // "1a2b3c"

// Base conversion
string result = ObjectBaseTools.BaseConvert("FF", fromBase: 16, toBase: 10); // "255"
```

## Related Components
- **DATA/ACCESSORS**: Uses ConvertTools for type conversion
- **DATA/DATAPO**: Uses DateTools for timestamp handling

## Dependencies
- `System.Text` (StringBuilder)
- `System.Globalization` (CultureInfo)
- `Nglib.SECURITY.CRYPTO` (KeyTools base36 checksums)

## Notes
- **Thread-safe**: RandomString uses ThreadLocal<Random>
- **Performance**: StringTools methods use StringBuilder and Span<T> where applicable
- **Culture**: ConvertTools respects culture for number/date parsing
- **Null-safe**: Most methods handle null inputs gracefully
- **No regex**: Intentionally avoids regex for performance (commented out)
