# Memory Management Strategy Design

## Overview

This document describes the automatic memory management strategy implemented for the ADL compiler. The system provides automatic memory management for C++ objects using reference counting and scope-based cleanup, eliminating the need for manual `delete` or `free` calls.

## Requirements

Based on Requirements 4.2 and 4.4:
- Support C++-style code with automatic memory management
- No manual `delete` or `free` calls needed
- Compiler inserts cleanup code at appropriate scope boundaries
- Prevent memory leaks, dangling pointers, and double-free errors

## Design Components

### 1. Reference Counting System

The memory management system uses reference counting to track object lifetimes:

- **Allocation Tracking**: Every `new` expression is tracked as an `Allocation`
- **Reference Count Initialization**: When an object is allocated, its reference count is initialized to 1
- **Reference Count Operations**: The system generates three types of operations:
  - `Initialize`: Set ref count to 1 after allocation
  - `Decrement`: Decrease ref count when leaving scope
  - `CheckAndDelete`: Delete object if ref count reaches zero

**Data Structures**:
```csharp
public class Allocation
{
    public string VariableName { get; set; }
    public string TypeName { get; set; }
    public SourceLocation AllocationLocation { get; set; }
    public SourceLocation AssignmentLocation { get; set; }
    public SourceLocation? ScopeEnd { get; set; }
    public bool IsPointer { get; set; }
    public bool IsArray { get; set; }
    public AllocationKind Kind { get; set; }
}

public class RefCountOperation
{
    public RefCountOpType Type { get; set; }
    public string VariableName { get; set; }
    public SourceLocation Location { get; set; }
}
```

### 2. Scope-Based Cleanup Insertion

The system analyzes code structure to determine scope boundaries and inserts cleanup code:

- **Scope Tracking**: Maintains a stack of scopes (Global, Class, Method, Block, Loop, Conditional)
- **Scope Hierarchy**: Each scope tracks its parent and children
- **Allocation Ownership**: Allocations are associated with the scope where they occur
- **Automatic Cleanup**: At scope exit, all allocations in that scope are automatically cleaned up

**Data Structures**:
```csharp
public class MemoryScope
{
    public string Id { get; set; }
    public MemoryScope? Parent { get; set; }
    public List<MemoryScope> Children { get; set; }
    public List<Allocation> Allocations { get; set; }
    public SourceLocation StartLocation { get; set; }
    public SourceLocation? EndLocation { get; set; }
    public ScopeKind Kind { get; set; }
}
```

### 3. Smart Pointer Conversion

The system can convert raw pointers to smart pointers for enhanced safety:

- **Conversion Analysis**: Determines appropriate smart pointer type based on usage patterns
- **Smart Pointer Types**:
  - `Unique`: Single ownership (like `unique_ptr`)
  - `Shared`: Shared ownership with ref counting (like `shared_ptr`)
  - `Weak`: Weak reference (like `weak_ptr`)
- **Default Strategy**: Currently uses `Shared` pointers for all allocations

**Data Structures**:
```csharp
public class SmartPointerConversion
{
    public string OriginalPointerName { get; set; }
    public string SmartPointerName { get; set; }
    public SmartPointerType Type { get; set; }
    public SourceLocation Location { get; set; }
}
```

## Analysis Process

The `MemoryManagementAnalyzer` performs the following steps:

### 1. AST Traversal

The analyzer walks the Abstract Syntax Tree (AST) and:
- Enters/exits scopes as it encounters blocks, methods, and classes
- Identifies pointer allocations (`new` expressions)
- Tracks variable declarations with pointer types
- Analyzes assignments to pointers

### 2. Allocation Detection

For each variable declaration or assignment:
```csharp
// Detected pattern:
Image* img = new Image(1920, 1080);

// Creates allocation:
Allocation {
    VariableName = "img",
    TypeName = "Image",
    IsPointer = true,
    Kind = AllocationKind.New
}
```

### 3. Scope Management

The analyzer maintains a scope stack:
```
Global Scope
  └─ Class Scope (TestClass)
      └─ Method Scope (process)
          └─ Block Scope
              └─ Allocations: [img]
```

### 4. Instruction Generation

After analysis, the system generates:

**Reference Counting Operations**:
```csharp
// For: Image* img = new Image(1920, 1080);

RefCountOperation { Type = Initialize, Variable = "img", Location = allocation }
RefCountOperation { Type = Decrement, Variable = "img", Location = scope_end }
RefCountOperation { Type = CheckAndDelete, Variable = "img", Location = scope_end }
```

**Deallocation Operations**:
```csharp
Deallocation {
    VariableName = "img",
    Kind = DeallocationKind.Delete,
    Location = scope_end
}
```

## Code Generation Example

### Input Code:
```cpp
void processImage() {
    Image* img = new Image(1920, 1080);
    img->apply(filter);
    img->save("output.png");
}
```

### Generated Code (Conceptual):
```cpp
void processImage() {
    Image* img = new Image(1920, 1080);
    __ref_count_init(img);  // Compiler inserted
    
    img->apply(filter);
    img->save("output.png");
    
    __ref_count_dec(img);   // Compiler inserted
    if (__ref_count_is_zero(img)) {
        __safe_delete(img);  // Compiler inserted
    }
}
```

## Memory Safety Guarantees

The system provides the following guarantees:

1. **No Memory Leaks**: All allocations are tracked and cleaned up at scope exit
2. **No Dangling Pointers**: Reference counting ensures objects aren't deleted while still referenced
3. **No Double-Free**: Objects are only deleted when reference count reaches zero
4. **Automatic Null Checking**: The system can insert null checks before pointer dereferences

## Array Handling

The system distinguishes between single object and array allocations:

```cpp
// Single object
Image* img = new Image(1920, 1080);
// Generates: delete img;

// Array
int* arr = new int[100];
// Generates: delete[] arr;
```

## Nested Scopes

The system correctly handles nested scopes:

```cpp
void process() {
    Image* img1 = new Image(1920, 1080);  // Cleaned up at method end
    {
        Image* img2 = new Image(800, 600);  // Cleaned up at block end
    }
    // img2 already cleaned up here
}
// img1 cleaned up here
```

## Manual Strategy

For advanced users who want manual memory management:

```csharp
var analyzer = new MemoryManagementAnalyzer(MemoryManagementStrategy.Manual);
```

This disables automatic memory management insertion.

## Future Enhancements

Potential improvements for future versions:

1. **Escape Analysis**: Detect when objects don't escape their scope and use stack allocation
2. **Ownership Analysis**: Determine unique vs shared ownership automatically
3. **Cycle Detection**: Detect and handle reference cycles
4. **Move Semantics**: Support C++11 move semantics for efficient transfers
5. **Custom Deleters**: Support custom deletion functions for special resources

## Testing

The memory management system is thoroughly tested with:

- Simple pointer allocations
- Array allocations
- Multiple allocations in same scope
- Nested scopes
- Field allocations
- Smart pointer conversions
- Manual strategy mode

All tests verify:
- Correct allocation tracking
- Proper reference counting operations
- Appropriate deallocation generation
- Correct scope handling

## Integration

The memory management system integrates with:

1. **Parser**: Uses AST nodes to identify allocations
2. **Semantic Analyzer**: Works alongside type checking and symbol resolution
3. **Code Generator**: Consumes memory management instructions to emit cleanup code
4. **Type System**: Uses type information to determine pointer types

## Performance Considerations

- **Compile-Time Overhead**: Analysis is performed once during compilation
- **Runtime Overhead**: Reference counting adds minimal overhead (increment/decrement operations)
- **Memory Overhead**: Each managed object has a reference count (typically 4-8 bytes)
- **Optimization Opportunities**: Future optimizations can eliminate ref counting for provably safe cases

## Conclusion

The automatic memory management system provides C++ developers with memory safety without sacrificing control or performance. By using reference counting and scope-based cleanup, the system eliminates common memory errors while maintaining the familiar C++ syntax and semantics.
