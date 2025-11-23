[◀ Back to Documentation Home](../README.md)

# APP.DIAG

**Namespace:** `Nglib.APP.DIAG`

## Overview

Diagnostic and validation utilities for error handling, performance monitoring, and result patterns.

---

## ValidateModel

Validation result model implementing the Result/Either pattern for error handling without exceptions.

### Features
- Result pattern with boolean status and error messages
- Fluent API for chaining operations
- Multiple error notes support
- Fail-fast validation with `EnsureIsValid()`
- Combine multiple validation results

### Examples

```csharp
// Factory methods
var success = ValidateModel.Success;
var fail = ValidateModel.Fail;
var invalid = ValidateModel.Invalid("Error message");
var fromEx = ValidateModel.Invalid(exception);

// Fluent API
var result = new ValidateModel()
    .SetInvalid("Something went wrong")
    .AddNote("Additional info");

// Fail-fast pattern
result.EnsureIsValid(); // Throws if invalid

// Combine validations
var combined = ValidateModel.Combine(
    ValidateEmail(user.Email),
    ValidateAge(user.Age),
    ValidatePassword(user.Password)
);
if (!combined.IsValid)
{
    Console.WriteLine(combined.Message);
    foreach (var note in combined.Notes)
        Console.WriteLine($"  - {note}");
}
```

**Use Cases:**
- Input validation without exceptions
- Business rule validation
- Multi-step validation workflows
- API result patterns

---

## StopwatchLogAt

Stopwatch-based tracing utility for performance monitoring with timestamped logs.

### Features
- Automatic timestamp on each trace
- Thread-safe log collection
- Verbosity level filtering
- Error detection tracking
- Compartment/category support

### Examples

```csharp
// Quick start
var watch = StopwatchLogAt.StartNew();
watch.AddTrace("Starting operation");

// Do work...
DoSomeWork();
watch.AddTrace("Work completed");

watch.Stop("Finished");
Console.WriteLine(watch.ToString());

// Advanced usage
var watch = new StopwatchLogAt 
{ 
    Compartment = "API",
    VerbosityMin = TraceLevel.Info,
    TimeBeforeText = true
};
watch.Start("Processing request");
watch.AddTrace("Connecting to database");
watch.AddTrace("Query failed", TraceLevel.Error);
watch.Stop();

if (watch.AnyError)
    Console.WriteLine("Errors detected!");

Console.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}ms");
```

**Use Cases:**
- Performance profiling
- Operation tracing
- Debugging complex workflows
- Audit logging

---

## CascadeException

Exception wrapper that preserves method context while avoiding nested CascadeException chains.

### Features
- Automatic method context in error messages
- Unwraps nested CascadeException to preserve original exception
- Simple pattern for error propagation

### Examples

```csharp
// Basic usage
public void MyMethod()
{
    try
    {
        DatabaseOperation();
    }
    catch (Exception ex)
    {
        throw new CascadeException(nameof(MyMethod), ex);
    }
}

// Result: "MyMethod:Original error message"
// InnerException: Original exception (not CascadeException)

// Call stack tracing
public void Level1()
{
    try { Level2(); }
    catch (Exception ex)
    {
        throw new CascadeException(nameof(Level1), ex);
    }
}

public void Level2()
{
    try { Level3(); }
    catch (Exception ex)
    {
        throw new CascadeException(nameof(Level2), ex);
    }
}

// Result: "Level1:Level2:Level3:Database connection failed"
```

**Use Cases:**
- Method call tracing
- Error context preservation
- Debug-friendly stack traces

---

## ILog

Simple interface for log entries with text, level, and timestamp.

### Properties
- `LogText` - The log message
- `LogLevel` - Severity level (int)
- `DateCreate` - Creation timestamp

### Example

```csharp
public class AppLog : ILog
{
    public string LogText { get; set; }
    public int LogLevel { get; set; }
    public DateTime DateCreate { get; set; } = DateTime.Now;
}
```

---

## Common Patterns

**Validation Pipeline:**
```csharp
public ValidateModel ValidateUser(User user)
{
    var validations = new[]
    {
        ValidateEmail(user.Email),
        ValidateAge(user.Age),
        ValidatePassword(user.Password)
    };
    return ValidateModel.Combine(validations);
}
```

**Performance Monitoring:**
```csharp
public async Task<Result> ProcessData()
{
    var watch = StopwatchLogAt.StartNew();
    watch.AddTrace("Starting data processing");
    
    var result = await LongOperation();
    watch.AddTrace($"Processed {result.Count} items");
    
    watch.Stop();
    _logger.Log(watch.ToString());
    return result;
}
```

**Error Context Tracing:**
```csharp
public void SaveData(Data data)
{
    try
    {
        var validation = ValidateData(data);
        validation.EnsureIsValid();
        _repository.Save(data);
    }
    catch (Exception ex)
    {
        throw new CascadeException(nameof(SaveData), ex);
    }
}
```

---

## Notes
- `ValidateModel` is **sealed** for optimization and to prevent inheritance issues
- `StopwatchLogAt` is **thread-safe** with internal locking
- Use `ValidateModel` instead of throwing exceptions for expected validation failures
- `CascadeException` automatically unwraps nested instances to preserve original exceptions
