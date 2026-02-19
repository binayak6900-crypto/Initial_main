# Error Reporting System

This directory contains the friendly error reporting system for the ADL compiler.

## Overview

The error reporting system provides clear, actionable error messages with:
- **Friendly language**: Uses simple, non-technical terms
- **Source context**: Shows the exact line of code with the error
- **Visual indicators**: Caret (^) pointing to the error location
- **Helpful suggestions**: Provides guidance on how to fix common errors
- **Multiple errors**: Collects all errors in a single compilation pass

## Components

### CompilerDiagnostic

Represents a single diagnostic (error, warning, or info) with:
- Severity level (Error, Warning, Info)
- Source location (file, line, column)
- Message and friendly message
- Source code snippet
- Suggestions for fixing the error
- Quick fixes (automated code replacements)

### DiagnosticReporter

Collects and formats diagnostics with:
- Error collection (doesn't stop at first error)
- Source file caching for snippet extraction
- Typo detection using Levenshtein distance
- Type conversion suggestions
- Formatted output for IDE parsing

## Error Message Format

```
<file>:<line>:<column>: <severity>: <message>
  <source line>
  <caret indicator>
  
Suggestion: <helpful suggestion>
```

## Examples

### Syntax Error
```
Main.adl:15:23: error: unknown variable 'result'
    int total = result + 5;
                ^~~~~~
  
Suggestion: Did you mean 'results'? Or did you forget to declare 'result'?
```

### Type Error
```
Main.adl:20:15: error: cannot convert String to int
    int count = "hello";
                ^~~~~~~
  
Suggestion: Use Integer.parseInt("hello") to convert String to int
```

### Missing Class
```
Main.adl:10:5: error: class 'Utils' not found
    Utils.log("message");
    ^~~~~
  
Suggestion: Make sure Utils.adl exists in your project directory
```

### Duplicate Symbol
```
Main.adl:25:10: error: 'myVar' is already defined
    int myVar = 10;
        ^~~~~
  
Suggestion: 'myVar' was first defined at Main.adl:15:10
```

## Usage

### Basic Usage

```csharp
var reporter = new DiagnosticReporter();

// Report errors
reporter.ReportSyntaxError(location, "unexpected token");
reporter.ReportTypeError(location, "int", "String");
reporter.ReportUndefinedSymbol(location, "result", availableSymbols);
reporter.ReportMissingClass(location, "Utils");
reporter.ReportDuplicateSymbol(location, "myVar", originalLocation);

// Check for errors
if (reporter.HasErrors)
{
    Console.WriteLine(reporter.FormatAll());
    return 1;
}
```

### Custom Diagnostics

```csharp
var diagnostic = CompilerDiagnostic.Error(location, "message", "friendly message");
diagnostic.Suggestions.Add("Try this fix");
diagnostic.Suggestions.Add("Or try this alternative");
reporter.Report(diagnostic);
```

### Multiple Errors

The reporter collects all errors during compilation:

```csharp
// Report multiple errors
reporter.ReportTypeError(loc1, "int", "String");
reporter.ReportUndefinedSymbol(loc2, "foo");
reporter.ReportSyntaxError(loc3, "missing semicolon");

// Format all at once, sorted by location
var output = reporter.FormatAll();
// Output:
// file.adl:10:5: error: ...
// file.adl:15:10: error: ...
// file.adl:20:15: error: ...
// 3 error(s), 0 warning(s)
```

## Features

### Typo Detection

The reporter uses Levenshtein distance to find similar symbol names:

```csharp
var availableSymbols = new List<string> { "userName", "userEmail", "userId" };
reporter.ReportUndefinedSymbol(location, "userNam", availableSymbols);
// Suggestion: Did you mean 'userName'?
```

### Type Conversion Suggestions

Common type conversions are suggested automatically:

```csharp
reporter.ReportTypeError(location, "int", "String");
// Suggestion: Use Integer.parseInt(value) to convert String to int

reporter.ReportTypeError(location, "String", "int");
// Suggestion: Use Integer.toString(value) or String.valueOf(value) to convert int to String
```

### Source Context

The reporter automatically extracts and displays the source line:

```csharp
// Source file contains:
//   int total = result + 5;

reporter.ReportUndefinedSymbol(location, "result");
// Output shows:
//   int total = result + 5;
//               ^~~~~~
```

## Integration with Compiler

The error reporting system integrates with all compiler phases:

1. **Lexer/Parser**: Report syntax errors
2. **Semantic Analysis**: Report type errors, undefined symbols
3. **Dependency Analysis**: Report missing classes/files
4. **Code Generation**: Report unsupported features

Example integration:

```csharp
public class TypeChecker
{
    private readonly DiagnosticReporter _reporter;
    
    public TypeChecker(DiagnosticReporter reporter)
    {
        _reporter = reporter;
    }
    
    public void CheckAssignment(ASTNode node, Type expectedType, Type actualType)
    {
        if (!AreTypesCompatible(expectedType, actualType))
        {
            _reporter.ReportTypeError(
                node.Location,
                expectedType.Name,
                actualType.Name
            );
        }
    }
}
```

## Requirements Satisfied

This implementation satisfies the following requirements:

- **8.1**: Reports file path, line number, and error description for syntax errors
- **8.2**: Reports conflicting types and location for type errors
- **8.3**: Reports symbol name and location for undefined symbols
- **8.4**: Collects multiple errors in a single compilation pass
- **8.5**: Formats errors for easy parsing by the IDE (file:line:column: format)

## Testing

The error reporting system has comprehensive tests:

- **Unit Tests**: Test individual components (CompilerDiagnostic, DiagnosticReporter)
- **Integration Tests**: Test with real source files and multiple errors
- **Typo Detection Tests**: Verify Levenshtein distance algorithm
- **Type Conversion Tests**: Verify suggestion generation

Run tests:
```bash
dotnet test --filter "FullyQualifiedName~ErrorReporting"
```
