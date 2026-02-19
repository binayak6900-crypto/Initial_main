# Dependency Discovery Implementation

## Overview

Implemented automatic dependency discovery system for the ADL compiler that scans project directories, analyzes file dependencies, and determines correct compilation order using topological sorting.

## Implementation Details

### Core Component: DependencyAnalyzer

**Location**: `src/ADLCompiler/SemanticAnalysis/DependencyAnalyzer.cs`

**Key Features**:
1. **Automatic File Discovery**: Recursively scans project directory for all `.adl` files
2. **Symbol Indexing**: Builds index of all classes, namespaces, and functions defined in each file
3. **Reference Extraction**: Analyzes AST to find all type references (superclasses, field types, parameter types, etc.)
4. **Dependency Graph**: Builds directed graph showing which files depend on which other files
5. **Topological Sort**: Determines compilation order ensuring dependencies are compiled first
6. **Circular Dependency Detection**: Detects and reports circular dependencies with clear error messages

### Algorithm Flow

```
1. Discover all .adl files in project directory (recursive)
2. Parse each file and build AST
3. Index symbols defined in each file:
   - Classes (qualified and unqualified names)
   - Namespaces
   - Namespace functions
4. Extract references from each file:
   - Superclass references
   - Interface references
   - Field type references
   - Parameter type references
   - Variable type references
   - Expression type references
5. Build dependency graph:
   - For each reference, find the file that defines it
   - Add edge from current file to defining file
6. Topological sort:
   - DFS-based algorithm
   - Detects cycles during traversal
   - Returns files in dependency order
```

### Reference Extraction

The `ReferenceExtractor` visitor traverses the AST and collects all type references:
- Class declarations (superclass, interfaces)
- Field declarations (field types)
- Method declarations (return types, parameter types)
- Variable declarations (variable types)
- Expressions (new expressions, cast expressions, etc.)

### Circular Dependency Detection

During topological sort, the algorithm maintains a "visiting" set to detect cycles:
- If a file is encountered while already being visited, a cycle exists
- Builds readable error message showing the cycle path
- Throws `CircularDependencyException` with clear message

## Requirements Satisfied

- **5.1**: Supports optional include directives (automatic discovery eliminates need for explicit includes)
- **5.3**: Makes definitions from other files available automatically
- **5.6**: Supports relative paths (discovers files in subdirectories)
- **5.7**: Supports namespace functions across files

## Testing

### Unit Tests (13 tests)
**Location**: `tests/ADLCompiler.Tests/DependencyAnalyzerTests.cs`

Tests cover:
- Empty directory handling
- Single file projects
- Multiple files with no dependencies
- Simple dependencies (A depends on B)
- Chained dependencies (A → B → C)
- Circular dependency detection
- Namespace references
- Field type references
- Method parameter references
- Complex dependency graphs
- Subdirectory file discovery
- Dependency graph retrieval
- Symbol index retrieval

### Integration Tests (2 tests)
**Location**: `tests/ADLCompiler.Tests/DependencyAnalyzerIntegrationTests.cs`

Tests with real parser:
- Multi-file project with complex dependencies
- Circular dependency detection with real source code

All 15 tests pass successfully.

## Usage Example

```csharp
// Create analyzer for project directory
var analyzer = new DependencyAnalyzer("/path/to/project");

// Analyze dependencies (provide parser function)
var compilationOrder = analyzer.AnalyzeDependencies(file =>
{
    var source = File.ReadAllText(file);
    var lexer = new Lexer(source, file);
    var tokens = lexer.Tokenize();
    var parser = new Parser(tokens, file);
    return parser.Parse();
});

// compilationOrder now contains files in correct compilation order
foreach (var file in compilationOrder)
{
    Console.WriteLine($"Compile: {file}");
}

// Get dependency graph for visualization
var graph = analyzer.GetDependencyGraph();
foreach (var kvp in graph)
{
    Console.WriteLine($"{kvp.Key} depends on:");
    foreach (var dep in kvp.Value)
    {
        Console.WriteLine($"  - {dep}");
    }
}
```

## Error Handling

### Circular Dependencies
```
CircularDependencyException: Circular dependency detected: ClassA.adl -> ClassB.adl
```

### File Discovery Errors
```
DependencyAnalysisException: Failed to discover .adl files: [error details]
```

## Integration Points

The dependency analyzer integrates with:
1. **Lexer**: Tokenizes source files
2. **Parser**: Generates AST from tokens
3. **Symbol Table**: Uses symbol information for resolution
4. **Type Checker**: Provides compilation order for type checking
5. **Compiler**: Determines order for code generation

## Next Steps

Task 6.3 is complete. The dependency discovery system is ready for integration with:
- **Task 7**: Memory management system (will use compilation order)
- **Task 8**: Preprocessor (will use dependency graph)
- **Task 10**: Code generator (will compile files in dependency order)
