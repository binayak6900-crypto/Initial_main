# ADL Compiler Output Format Design

## Overview

The ADL compiler generates a **hybrid bytecode + native code format** that combines:
- **Dalvik-compatible bytecode** for Java-style code (with garbage collection)
- **Native ARM/x86 machine code** for C++-style code (with automatic reference counting)
- **Metadata** for debugging, runtime type information, and memory management

This hybrid approach allows the AVM to execute both high-level Java code and performance-critical C++ code efficiently within the same application.

## File Format Structure

The output is packaged as an **ADL Binary Bundle (.adlb)** with the following structure:

```
ADL Binary Bundle (.adlb)
├── Header
│   ├── Magic Number (0x41444C42 = "ADLB")
│   ├── Version (uint32)
│   ├── Target SDK (uint32)
│   ├── Architecture Flags (uint32)
│   └── Section Offsets
├── Bytecode Section
│   ├── Class Count
│   └── Classes[]
│       ├── Class Name
│       ├── Super Class
│       ├── Interfaces[]
│       ├── Fields[]
│       └── Methods[]
│           ├── Method Name
│           ├── Signature
│           ├── Access Flags
│           ├── Max Stack
│           ├── Max Locals
│           └── Instructions[]
├── Native Code Section
│   ├── Function Count
│   └── Functions[]
│       ├── Function Name
│       ├── Signature
│       ├── Architecture (ARM/ARM64/x86/x86_64)
│       ├── Code Size
│       ├── Machine Code[]
│       └── Relocations[]
├── Memory Management Section
│   ├── Scope Count
│   └── Scopes[]
│       ├── Scope ID
│       ├── Allocations[]
│       │   ├── Pointer ID
│       │   ├── Type
│       │   └── Allocation Location
│       ├── Reference Count Operations[]
│       │   ├── Operation Type (INC/DEC)
│       │   ├── Pointer ID
│       │   └── Location
│       └── Deallocations[]
│           ├── Pointer ID
│           └── Location
├── Metadata Section
│   ├── Debug Info
│   │   ├── Source File Mappings
│   │   ├── Line Number Tables
│   │   └── Variable Names
│   ├── Type Information
│   │   ├── Class Descriptors
│   │   ├── Method Descriptors
│   │   └── Field Descriptors
│   └── API Bindings
│       ├── Built-in API References
│       └── NDK Function Pointers
└── Resource Section
    ├── String Pool
    ├── Constant Pool
    └── Static Data
```

## 1. Header Format

```csharp
public struct ADLBHeader
{
    public uint MagicNumber;        // 0x41444C42 ("ADLB")
    public uint Version;            // Format version (1)
    public uint TargetSDK;          // Android SDK API level
    public uint ArchitectureFlags;  // Bit flags: ARM=1, ARM64=2, x86=4, x86_64=8
    public uint BytecodeSectionOffset;
    public uint NativeCodeSectionOffset;
    public uint MemoryManagementSectionOffset;
    public uint MetadataSectionOffset;
    public uint ResourceSectionOffset;
    public uint TotalSize;
}
```

## 2. Bytecode Section Format

### 2.1 Bytecode Instruction Set

The bytecode is **Dalvik-compatible** with extensions for ADL-specific features:

#### Standard Dalvik Instructions (Opcodes 0x00-0xEF)
- **Move**: `move`, `move-wide`, `move-object`
- **Return**: `return`, `return-void`, `return-wide`, `return-object`
- **Const**: `const`, `const-wide`, `const-string`, `const-class`
- **Arithmetic**: `add-int`, `sub-int`, `mul-int`, `div-int`, `rem-int`
- **Comparison**: `cmp-long`, `if-eq`, `if-ne`, `if-lt`, `if-ge`, `if-gt`, `if-le`
- **Array**: `aget`, `aput`, `array-length`, `new-array`
- **Instance**: `iget`, `iput`, `new-instance`, `instance-of`, `check-cast`
- **Static**: `sget`, `sput`
- **Invoke**: `invoke-virtual`, `invoke-super`, `invoke-direct`, `invoke-static`, `invoke-interface`

#### ADL Extension Instructions (Opcodes 0xF0-0xFF)
- **0xF0**: `invoke-builtin` - Call built-in Android/NDK API
- **0xF1**: `invoke-native` - Call native C++ function
- **0xF2**: `ref-count-inc` - Increment reference count
- **0xF3**: `ref-count-dec` - Decrement reference count
- **0xF4**: `auto-cleanup` - Automatic cleanup at scope boundary
- **0xF5**: `null-check` - Automatic null safety check
- **0xF6**: `raylib-call` - Call raylib function
- **0xF7-0xFF**: Reserved for future extensions

### 2.2 Bytecode Class Format

```csharp
public struct BytecodeClass
{
    public string ClassName;           // Fully qualified name (e.g., "com.example.MainActivity")
    public string SuperClassName;      // Parent class name
    public string[] Interfaces;        // Implemented interfaces
    public AccessFlags AccessFlags;    // public, private, protected, final, abstract
    public BytecodeField[] Fields;     // Class fields
    public BytecodeMethod[] Methods;   // Class methods
}

public struct BytecodeField
{
    public string Name;
    public string TypeDescriptor;      // Java type descriptor (e.g., "I" for int, "Ljava/lang/String;" for String)
    public AccessFlags AccessFlags;
    public object InitialValue;        // For static fields
}

public struct BytecodeMethod
{
    public string Name;
    public string Signature;           // Method signature (e.g., "(II)I" for int method(int, int))
    public AccessFlags AccessFlags;
    public ushort MaxStack;            // Maximum stack depth
    public ushort MaxLocals;           // Number of local variables
    public BytecodeInstruction[] Instructions;
    public ExceptionHandler[] ExceptionHandlers;
}

public struct BytecodeInstruction
{
    public byte Opcode;                // Instruction opcode
    public byte[] Operands;            // Instruction operands (variable length)
    public uint SourceLine;            // Source line number for debugging
}
```

### 2.3 Type Descriptors

Following Dalvik conventions:
- **Primitives**: `V` (void), `Z` (boolean), `B` (byte), `S` (short), `C` (char), `I` (int), `J` (long), `F` (float), `D` (double)
- **Objects**: `Lpackage/ClassName;` (e.g., `Ljava/lang/String;`)
- **Arrays**: `[` prefix (e.g., `[I` for int[], `[Ljava/lang/String;` for String[])

### 2.4 Method Signatures

Format: `(ParameterTypes)ReturnType`

Examples:
- `()V` - void method()
- `(I)I` - int method(int)
- `(Ljava/lang/String;I)Z` - boolean method(String, int)
- `([I[I)[[I` - int[][] method(int[], int[])

## 3. Native Code Section Format

### 3.1 Native Function Format

```csharp
public struct NativeFunction
{
    public string FunctionName;        // Mangled C++ name
    public string Signature;           // Function signature
    public Architecture Architecture;  // ARM, ARM64, x86, x86_64
    public uint CodeSize;              // Size of machine code in bytes
    public byte[] MachineCode;         // Raw machine code
    public Relocation[] Relocations;   // Relocation entries for linking
    public uint StackFrameSize;        // Stack frame size
    public CallingConvention Convention; // Calling convention
}

public enum Architecture
{
    ARM = 1,
    ARM64 = 2,
    x86 = 4,
    x86_64 = 8
}

public enum CallingConvention
{
    ARM_AAPCS,      // ARM Architecture Procedure Call Standard
    ARM64_AAPCS64,  // ARM64 calling convention
    x86_cdecl,      // x86 C declaration
    x86_64_SysV     // x86-64 System V ABI
}

public struct Relocation
{
    public uint Offset;                // Offset in machine code
    public RelocationType Type;        // Type of relocation
    public string TargetSymbol;        // Symbol to relocate to
    public int Addend;                 // Addend for relocation
}

public enum RelocationType
{
    Absolute,       // Absolute address
    Relative,       // PC-relative address
    GOT,            // Global Offset Table
    PLT             // Procedure Linkage Table
}
```

### 3.2 Native Code Generation

For C++ code, the compiler generates native machine code:

**ARM Assembly Example**:
```asm
; void process(int* data, size_t length)
process:
    push    {r4, r5, lr}        ; Save registers
    mov     r4, r0              ; r4 = data pointer
    mov     r5, r1              ; r5 = length
    mov     r2, #0              ; r2 = i = 0
.loop:
    cmp     r2, r5              ; Compare i with length
    bge     .end                ; If i >= length, exit
    ldr     r3, [r4, r2, lsl #2] ; Load data[i]
    lsl     r3, r3, #1          ; data[i] *= 2
    str     r3, [r4, r2, lsl #2] ; Store data[i]
    add     r2, r2, #1          ; i++
    b       .loop               ; Continue loop
.end:
    pop     {r4, r5, pc}        ; Restore and return
```

**x86-64 Assembly Example**:
```asm
; void process(int* data, size_t length)
process:
    push    rbp
    mov     rbp, rsp
    mov     rax, 0              ; i = 0
.loop:
    cmp     rax, rsi            ; Compare i with length
    jge     .end                ; If i >= length, exit
    mov     ecx, [rdi + rax*4]  ; Load data[i]
    shl     ecx, 1              ; data[i] *= 2
    mov     [rdi + rax*4], ecx  ; Store data[i]
    inc     rax                 ; i++
    jmp     .loop               ; Continue loop
.end:
    pop     rbp
    ret
```

## 4. Memory Management Section Format

### 4.1 Memory Management Instructions

```csharp
public struct MemoryManagementScope
{
    public string ScopeID;             // Unique scope identifier
    public Allocation[] Allocations;   // Memory allocations in this scope
    public RefCountOperation[] RefCountOps; // Reference counting operations
    public Deallocation[] Deallocations; // Cleanup operations
}

public struct Allocation
{
    public uint PointerID;             // Unique pointer identifier
    public string TypeName;            // Type being allocated
    public uint AllocationLocation;    // Instruction offset where allocation occurs
    public uint ScopeEnd;              // Instruction offset where scope ends
}

public struct RefCountOperation
{
    public RefCountOpType Type;        // INCREMENT or DECREMENT
    public uint PointerID;             // Pointer being ref-counted
    public uint Location;              // Instruction offset
}

public enum RefCountOpType
{
    INCREMENT,
    DECREMENT
}

public struct Deallocation
{
    public uint PointerID;             // Pointer to deallocate
    public uint Location;              // Instruction offset
    public bool Conditional;           // Only deallocate if ref count is zero
}
```

### 4.2 Automatic Memory Management Flow

1. **Allocation**: When `new` is encountered, compiler:
   - Generates allocation code
   - Assigns unique PointerID
   - Initializes reference count to 1
   - Records allocation in Memory Management Section

2. **Assignment**: When pointer is assigned:
   - Increment reference count of target
   - Decrement reference count of previous value
   - If previous value's ref count reaches 0, deallocate

3. **Scope Exit**: When leaving scope:
   - Decrement reference counts of all pointers in scope
   - Deallocate pointers with ref count = 0
   - Call destructors if needed

4. **Null Safety**: Before dereferencing:
   - Insert automatic null check
   - Throw NullPointerException if null

## 5. Metadata Section Format

### 5.1 Debug Information

```csharp
public struct DebugInfo
{
    public SourceFileMapping[] SourceFiles;
    public LineNumberTable[] LineNumbers;
    public LocalVariableTable[] LocalVariables;
}

public struct SourceFileMapping
{
    public uint FileID;
    public string FilePath;
    public string SourceHash;          // SHA-256 hash for verification
}

public struct LineNumberTable
{
    public uint MethodID;
    public LineNumberEntry[] Entries;
}

public struct LineNumberEntry
{
    public uint InstructionOffset;     // Bytecode or native code offset
    public uint SourceLine;            // Line number in source file
    public uint SourceColumn;          // Column number in source file
    public uint FileID;                // Source file ID
}

public struct LocalVariableTable
{
    public uint MethodID;
    public LocalVariableEntry[] Variables;
}

public struct LocalVariableEntry
{
    public string Name;
    public string TypeDescriptor;
    public uint StartOffset;           // Instruction offset where variable becomes valid
    public uint EndOffset;             // Instruction offset where variable goes out of scope
    public uint RegisterOrSlot;        // Register number or stack slot
}
```

### 5.2 Type Information

```csharp
public struct TypeInformation
{
    public ClassDescriptor[] Classes;
    public MethodDescriptor[] Methods;
    public FieldDescriptor[] Fields;
}

public struct ClassDescriptor
{
    public uint ClassID;
    public string ClassName;
    public uint SuperClassID;
    public uint[] InterfaceIDs;
    public uint[] FieldIDs;
    public uint[] MethodIDs;
    public ClassFlags Flags;
}

public struct MethodDescriptor
{
    public uint MethodID;
    public string MethodName;
    public string Signature;
    public uint ClassID;
    public MethodFlags Flags;
    public bool IsNative;              // True if native C++ code
    public uint CodeOffset;            // Offset in bytecode or native section
}

public struct FieldDescriptor
{
    public uint FieldID;
    public string FieldName;
    public string TypeDescriptor;
    public uint ClassID;
    public FieldFlags Flags;
}
```

### 5.3 API Bindings

```csharp
public struct APIBindings
{
    public BuiltinAPIReference[] BuiltinAPIs;
    public NDKFunctionPointer[] NDKFunctions;
    public RaylibFunctionPointer[] RaylibFunctions;
}

public struct BuiltinAPIReference
{
    public uint APIID;
    public string ClassName;
    public string MethodName;
    public string Signature;
    public uint MinSDK;                // Minimum SDK version required
    public bool IsDeprecated;
}

public struct NDKFunctionPointer
{
    public uint FunctionID;
    public string FunctionName;
    public string Library;             // e.g., "libGLESv2.so", "libOpenSLES.so"
    public string Symbol;              // Native symbol name
}

public struct RaylibFunctionPointer
{
    public uint FunctionID;
    public string FunctionName;
    public string Signature;
    public bool IsAutoManaged;         // True if resources are auto-managed
}
```

## 6. Resource Section Format

```csharp
public struct ResourceSection
{
    public StringPool Strings;
    public ConstantPool Constants;
    public StaticData StaticData;
}

public struct StringPool
{
    public uint StringCount;
    public StringEntry[] Strings;
}

public struct StringEntry
{
    public uint StringID;
    public uint Length;
    public byte[] UTF8Data;
}

public struct ConstantPool
{
    public uint ConstantCount;
    public Constant[] Constants;
}

public struct Constant
{
    public ConstantType Type;
    public byte[] Data;
}

public enum ConstantType
{
    Integer,
    Long,
    Float,
    Double,
    String,
    Class,
    MethodHandle,
    MethodType
}

public struct StaticData
{
    public uint DataSize;
    public byte[] Data;                // Static data segment
}
```

## 7. Execution Model

### 7.1 Hybrid Execution

The AVM executes the hybrid format as follows:

1. **Load Bundle**: Parse .adlb file and load all sections into memory
2. **Initialize Runtime**: Set up class loader, memory manager, API bindings
3. **Find Entry Point**: Locate main method from manifest
4. **Execute**:
   - **Bytecode methods**: Interpret using bytecode interpreter
   - **Native methods**: Execute directly as machine code
   - **Built-in APIs**: Call AVM's Android API implementations
   - **NDK functions**: Call through function pointers
   - **Memory management**: Execute ref-count operations automatically

### 7.2 Method Invocation

**Bytecode → Bytecode**:
```
invoke-virtual v0, v1, v2, Lcom/example/MyClass;->method(II)I
```
- Push arguments onto stack
- Look up method in class
- Create new stack frame
- Execute method bytecode
- Return result

**Bytecode → Native**:
```
invoke-native v0, v1, v2, _ZN7MyClass6methodEii  ; Mangled C++ name
```
- Marshal arguments from bytecode stack to native calling convention
- Call native function
- Marshal return value back to bytecode stack

**Bytecode → Built-in API**:
```
invoke-builtin v0, v1, 0x1234  ; API ID 0x1234 = Activity.setContentView
```
- Look up API implementation by ID
- Call AVM's implementation directly
- No marshaling needed (AVM handles both sides)

**Native → Bytecode**:
```asm
; Call Java method from C++
mov     r0, #123           ; Argument
bl      _jni_call_method   ; JNI-style call
```
- Use JNI-style interface
- Marshal arguments to bytecode format
- Invoke bytecode interpreter
- Marshal return value back

## 8. Optimization Opportunities

### 8.1 Just-In-Time (JIT) Compilation

Future enhancement: Compile hot bytecode paths to native code at runtime
- Profile execution to identify hot methods
- Compile bytecode → native code
- Replace bytecode with native code pointer
- Significant performance improvement for compute-intensive code

### 8.2 Ahead-Of-Time (AOT) Compilation

Future enhancement: Pre-compile all bytecode to native code
- Compile during installation
- Eliminate interpreter overhead
- Faster startup time
- Larger binary size

### 8.3 Link-Time Optimization

- Inline small methods across bytecode/native boundary
- Dead code elimination
- Constant propagation
- Function specialization

## 9. Compatibility

### 9.1 Dalvik Compatibility

The bytecode section is **compatible with Dalvik** for standard Java code:
- Can be executed by Dalvik VM (without ADL extensions)
- Can use standard Android tools (dex2oat, dexdump)
- Allows gradual migration from standard Android development

### 9.2 Cross-Platform Support

The format supports multiple architectures:
- **Android**: ARM, ARM64
- **Windows**: x86, x86_64
- **Linux**: x86, x86_64, ARM, ARM64
- **macOS**: x86_64, ARM64 (Apple Silicon)

Each platform's AVM loads the appropriate native code section for its architecture.

## 10. Security Considerations

### 10.1 Code Signing

- Bundle includes SHA-256 hash of all sections
- Signature verification before execution
- Prevents tampering with bytecode or native code

### 10.2 Sandboxing

- Native code executes in sandboxed environment
- Memory access restricted to allocated regions
- System calls filtered through AVM security layer

### 10.3 Permission Enforcement

- API calls check permissions before execution
- Built-in APIs enforce Android permission model
- NDK functions restricted based on permissions

## Summary

The ADL hybrid bytecode + native format provides:

✅ **Flexibility**: Support both Java and C++ code in one bundle
✅ **Performance**: Native code for performance-critical sections
✅ **Compatibility**: Dalvik-compatible bytecode for standard Java code
✅ **Automatic Memory Management**: Built-in reference counting and cleanup
✅ **Rich Metadata**: Complete debugging and type information
✅ **Cross-Platform**: Support for multiple architectures
✅ **Security**: Sandboxing and permission enforcement
✅ **Extensibility**: Reserved opcodes for future features

This format enables the ADL compiler to generate efficient, portable, and secure applications that leverage the best of both Java and C++ worlds.
