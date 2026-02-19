# Semantic Analysis - Symbol Table

This directory contains the semantic analysis components for the ADL compiler, specifically the symbol table implementation for scope management.

## Overview

The symbol table system provides:
- **Hierarchical scope management** with parent-child relationships
- **Symbol lookup** with scope chain traversal
- **Support for nested scopes**: blocks, methods, classes, and namespaces
- **Symbol shadowing** where inner scopes can override outer scope symbols

## Requirements

Implements requirements:
- **4.2**: Java-style class definitions with access modifiers
- **4.4**: C++-style namespaces
- **4.5**: Support for both Java and C++ syntax features dynamically

## Components

### Symbol.cs
Defines the `Symbol` class representing a symbol in the symbol table:
- **Name**: Symbol identifier
- **Type**: Type reference for the symbol
- **Kind**: Variable, Parameter, Method, Field, Class, or Namespace
- **Location**: Source location for error reporting

### SymbolTable.cs
Core symbol table implementation with:
- **Parent-child relationships**: Each scope can have a parent scope
- **Symbol definition**: `Define()` adds symbols to the current scope
- **Symbol lookup**: `Lookup()` searches current scope and parent scopes
- **Local lookup**: `LookupLocal()` searches only the current scope
- **Child scope creation**: `CreateChildScope()` creates nested scopes
- **Scope types**: Global, Namespace, Class, Method, Block

### SymbolTableBuilder.cs
Builds symbol tables from AST nodes:
- Traverses the AST and creates appropriate scopes
- Defines symbols for classes, methods, fields, parameters, and variables
- Handles nested scopes for blocks, if statements, loops, etc.

## Usage Example

```csharp
// Parse source code
var lexer = new Lexer(source, "test.adl");
var tokens = lexer.Tokenize();
var parser = new Parser(tokens, "test.adl");
var ast = parser.Parse();

// Build symbol table
var builder = new SymbolTableBuilder();
var symbolTable = builder.Build(ast);

// Look up symbols
var classSymbol = symbolTable.Lookup("MyClass");
if (classSymbol != null)
{
    Console.WriteLine($"Found {classSymbol.Kind}: {classSymbol.Name}");
}
```

## Scope Hierarchy

The symbol table supports the following scope hierarchy:

```
Global Scope
  └─ Namespace Scope (C++ style)
      └─ Class Scope
          ├─ Field symbols
          └─ Method Scope
              ├─ Parameter symbols
              ├─ Local variable symbols
              └─ Block Scope (if, while, for, etc.)
                  └─ Block-local variable symbols
```

## Symbol Lookup Rules

1. **Current scope first**: Check if symbol exists in current scope
2. **Parent scope traversal**: If not found, recursively check parent scopes
3. **Shadowing**: Inner scope symbols override outer scope symbols with the same name
4. **Null result**: Returns null if symbol not found in any scope

## Testing

Comprehensive test coverage includes:
- **Unit tests** (SymbolTableTests.cs): 20 tests covering core functionality
- **Integration tests** (SymbolTableIntegrationTests.cs): 13 tests with actual parsed code

Run tests:
```bash
dotnet test --filter "FullyQualifiedName~SymbolTable"
```

## Example Code

See `examples/symbol_table_demo.adl` for a demonstration of nested scopes:
- Namespace scope
- Class scope with fields
- Method scope with parameters
- Block scopes (if statement, for loop)
- Variable shadowing examples

## Future Enhancements

The symbol table is designed to support future semantic analysis features:
- Type checking and type inference
- Method overload resolution
- Access modifier enforcement
- Namespace resolution
- Import/include dependency tracking
