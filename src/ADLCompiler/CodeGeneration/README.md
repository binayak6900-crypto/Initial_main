# ADL Compiler Code Generation

This directory contains the output format design and data structures for the ADL compiler's code generation phase.

## Overview

The ADL compiler generates a **hybrid bytecode + native code format** that combines:
- **Dalvik-compatible bytecode** for Java-style code (with garbage collection)
- **Native ARM/x86 machine code** for C++-style code (with automatic reference counting)
- **Metadata** for debugging, runtime type information, and memory management

## Files

### OUTPUT_FORMAT_DESIGN.md
Complete specification of the ADL Binary Bundle (.adlb) format, including:
- File structure and layout
- Bytecode instruction set (Dalvik-compatible with ADL extensions)
- Native code format for multiple architectures
- Memory management instructions
- Metadata format for debugging and runtime
- Resource section format
- Execution model

### OutputFormat.cs
Core data structures:
- `ADLBinaryBundle` - Complete output bundle
- `ADLBHeader` - File header with magic number and section offsets
- `Architecture` - Architecture flags (ARM, ARM64, x86, x86_64)
- `AccessFlags` - Access modifiers for classes, methods, and fields

### BytecodeFormat.cs
Bytecode section data structures:
- `BytecodeSection` - Container for all bytecode classes
- `BytecodeClass` - Class definition with fields and methods
- `BytecodeMethod` - Method with instructions and exception handlers
- `BytecodeInstruction` - Individual bytecode instruction
- `Opcode` - Dalvik-compatible opcodes with ADL extensions (0xF0-0xFF)
- `TypeDescriptor` - Utilities for Java type descriptors

**Key Features**:
- Dalvik-compatible opcodes (0x00-0xEF)
- ADL extension opcodes:
  - `0xF0` INVOKE_BUILTIN - Call built-in Android/NDK API
  - `0xF1` INVOKE_NATIVE - Call native C++ function
  - `0xF2` REF_COUNT_INC - Increment reference count
  - `0xF3` REF_COUNT_DEC - Decrement reference count
  - `0xF4` AUTO_CLEANUP - Automatic cleanup at scope boundary
  - `0xF5` NULL_CHECK - Automatic null safety check
  - `0xF6` RAYLIB_CALL - Call raylib function

### NativeCodeFormat.cs
Native code section data structures:
- `NativeCodeSection` - Container for all native functions
- `NativeFunction` - Function with machine code and relocations
- `CallingConvention` - Platform-specific calling conventions
- `Relocation` - Relocation entries for linking
- `ARMCodeGen` - Helper for generating ARM machine code
- `x86_64CodeGen` - Helper for generating x86-64 machine code

**Supported Architectures**:
- ARM (32-bit) with AAPCS calling convention
- ARM64 (64-bit) with AAPCS64 calling convention
- x86 (32-bit) with cdecl calling convention
- x86-64 (64-bit) with System V ABI calling convention

### MemoryManagementFormat.cs
Memory management section data structures:
- `MemoryManagementSection` - Container for all memory management scopes
- `MemoryManagementScope` - Scope with allocations and cleanup operations
- `Allocation` - Memory allocation record
- `RefCountOperation` - Reference counting operation (increment/decrement)
- `Deallocation` - Cleanup operation
- `MemoryManagementHelper` - Utilities for generating memory management code

**Automatic Memory Management**:
- Reference counting for C++ objects
- Scope-based cleanup insertion
- Automatic null safety checks
- Conditional deallocation (only when ref count reaches zero)

### MetadataFormat.cs
Metadata section data structures:
- `MetadataSection` - Container for all metadata
- `DebugInfo` - Source file mappings, line numbers, local variables
- `TypeInformation` - Class, method, and field descriptors
- `APIBindings` - Built-in API, NDK, and raylib function references

**Debug Information**:
- Source file mappings with SHA-256 hashes
- Line number tables for bytecode and native code
- Local variable tables with scope information

**API Bindings**:
- Built-in Android API references with SDK version requirements
- NDK function pointers with library and symbol names
- raylib function pointers with auto-management flags

### ResourceFormat.cs
Resource section data structures:
- `ResourceSection` - Container for all resources
- `StringPool` - Efficient string storage with deduplication
- `ConstantPool` - Compile-time constants (integers, floats, strings, etc.)
- `StaticData` - Static data segment for global variables

**Features**:
- String deduplication (same string stored once)
- Constant deduplication (same constant stored once)
- Symbol-based static data access

## Usage Example

```csharp
using ADLCompiler.CodeGeneration;

// Create a new binary bundle
var bundle = new ADLBinaryBundle();

// Set up header
bundle.Header = ADLBHeader.Create(
    targetSDK: 33,
    architectureFlags: (uint)(Architecture.ARM64 | Architecture.x86_64)
);

// Add a bytecode class
var myClass = new BytecodeClass
{
    ClassName = "com.example.MyClass",
    SuperClassName = "java.lang.Object",
    AccessFlags = AccessFlags.Public
};

// Add a method
var method = new BytecodeMethod
{
    Name = "myMethod",
    Signature = TypeDescriptor.MethodSignature(
        new[] { TypeDescriptor.Int, TypeDescriptor.Int },
        TypeDescriptor.Int
    ),
    AccessFlags = AccessFlags.Public,
    MaxStack = 4,
    MaxLocals = 3
};

// Add bytecode instructions
method.Instructions.Add(new BytecodeInstruction(
    Opcode.CONST,
    new byte[] { 0, 42 },  // Load constant 42
    sourceLine: 10
));

method.Instructions.Add(new BytecodeInstruction(
    Opcode.RETURN,
    new byte[] { 0 },
    sourceLine: 11
));

myClass.Methods.Add(method);
bundle.Bytecode.Classes.Add(myClass);

// Add strings to resource pool
uint stringID = bundle.Resources.Strings.AddString("Hello, World!");

// Add constants
uint intConstID = bundle.Resources.Constants.AddInteger(42);
uint floatConstID = bundle.Resources.Constants.AddFloat(3.14f);

// Add memory management for native code
var scope = MemoryManagementHelper.CreateMethodScope("MyClass", "nativeMethod");
MemoryManagementHelper.AddAllocation(
    scope,
    typeName: "Image",
    allocationLocation: 0,
    scopeEnd: 100
);
bundle.MemoryManagement.Scopes.Add(scope);

// Add debug information
var lineTable = new LineNumberTable(methodID: 0);
lineTable.Entries.Add(new LineNumberEntry(
    instructionOffset: 0,
    sourceLine: 10,
    sourceColumn: 5,
    fileID: 0
));
bundle.Metadata.DebugInfo.LineNumbers.Add(lineTable);

// Bundle is now ready to be serialized and written to .adlb file
```

## Next Steps

The following components need to be implemented to complete the code generation phase:

1. **Binary Serialization** - Write ADLBinaryBundle to .adlb file format
2. **Bytecode Generator** - Generate bytecode from AST for Java-style code
3. **Native Code Generator** - Generate machine code from AST for C++-style code
4. **Memory Management Integration** - Insert automatic memory management code
5. **Metadata Generator** - Generate debug info and type information
6. **Linker** - Resolve relocations and link native code

## References

- **Dalvik Bytecode**: https://source.android.com/docs/core/runtime/dalvik-bytecode
- **ARM AAPCS**: ARM Architecture Procedure Call Standard
- **x86-64 System V ABI**: System V Application Binary Interface
- **Android APK Format**: https://developer.android.com/guide/components/fundamentals
