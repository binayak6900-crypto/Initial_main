# Design Document: mc² Programming Language

## Overview

mc² is a self-hosted, OS-independent programming language with unconventional syntax inspired by physics notation. The language embraces symbolic operators, custom type syntax, arbitrary precision numbers (no integer or float limits), built-in memory management, and a powerful macro system.

**Critical Design Philosophy**: Nothing is built-in by default. Everything—including basic functions like print() and input()—must be built from assembly first. The language provides the assembly foundation, and all higher-level features are constructed on top of it using assembly and inline assembly.

**Development Workflow**:
1. **Phase 1**: Build C++ bootstrap assembler that processes spacetime assembly
2. **Phase 2**: Implement core functions in assembly (print, input, memory management)
3. **Phase 3**: Test assembly thoroughly with comprehensive test suite
4. **Phase 4**: **DELETE all C++ files** - no going back, no dependencies
5. **Phase 5**: Self-host - rewrite compiler in .mc² using assembly and inline assembly
6. **Phase 6**: Build all syntax features using the assembly foundation

The compiler produces .e² files—a universal compiled format executed by the mc² runtime. Programs can be executed in two ways:
1. **Runtime Execution**: mc² runtime loads and executes .e² files directly (OS-independent)
2. **Native Conversion**: ricer.e² tool converts .e² files to native OS executables (MZ/PE for Windows, ELF for Linux, Mach-O for macOS, etc.)

Key design principles:
- Unconventional but consistent syntax ("as absurd as physics")
- `::` denotes line start (similar to `;` but for line beginning)
- Auto type detection with hash-based variable naming to avoid collisions
- Arbitrary precision integers and floats (no limits)
- Self-hosting capability (compiler written in mc², using assembly)
- OS independence (no external dependencies, custom binary format)
- Extensibility and scalability through macros, symbolic operators, and assembly
- Module system similar to C's object file linking
- Build automation via build.bat (Windows) or Makefile (Unix) with -arg command-line arguments

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│              PHASE 1-4: Bootstrap (C++ - TEMPORARY)          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  C++ Bootstrap Assembler (DELETED after Phase 4)            │
│  ┌────────────────────────────────────────────┐             │
│  │  Spacetime Assembly Parser                 │             │
│  │  .e² Binary Generator                      │             │
│  │  Runtime Executor                          │             │
│  └────────────────────────────────────────────┘             │
│                          │                                    │
│                          ▼                                    │
│  Core Assembly Functions (.mc² with inline assembly)        │
│  ┌────────────────────────────────────────────┐             │
│  │  print() - variadic output                 │             │
│  │  input() - user input with optional prompt │             │
│  │  Memory Manager - arbitrary precision      │             │
│  └────────────────────────────────────────────┘             │
│                          │                                    │
│                          ▼                                    │
│  Comprehensive Testing & Validation                         │
│  ┌────────────────────────────────────────────┐             │
│  │  Test all assembly functions               │             │
│  │  Verify memory management                  │             │
│  │  Validate I/O operations                   │             │
│  └────────────────────────────────────────────┘             │
│                          │                                    │
│                          ▼                                    │
│  ⚠️  DELETE ALL C++ FILES - NO GOING BACK ⚠️                │
│                                                               │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│         PHASE 5+: Self-Hosted (Pure mc² + Assembly)          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  mc² Compiler (Written in mc² with inline assembly)         │
│  ┌────────────────────────────────────────────┐             │
│  │  Preprocessor (macros, includes, operators)│             │
│  │  Lexer (tokenization)                      │             │
│  │  Parser (AST generation)                   │             │
│  │  Type Checker (inference, validation)      │             │
│  │  Code Generator (spacetime assembly)       │             │
│  │  Assembler (.e² binary generation)         │             │
│  │  Runtime (bytecode execution)              │             │
│  └────────────────────────────────────────────┘             │
│                          │                                    │
│                          ▼                                    │
│  Output: .e² or .e_sub2 (Universal Compiled Format)         │
│                                                               │
│  ┌────────────────────────────────────────────────────┐     │
│  │           mc² Runtime / Executor                   │     │
│  │  - Loads .e² files                                 │     │
│  │  - Executes bytecode                               │     │
│  │  - OS-independent execution                        │     │
│  │  - Arbitrary precision number support              │     │
│  └────────────────────────────────────────────────────┘     │
│                                                               │
│  ┌────────────────────────────────────────────────────┐     │
│  │           ricer.e² - Native Converter              │     │
│  │  - Converts .e² to native executables              │     │
│  │  - Generates MZ/PE (Windows .exe)                  │     │
│  │  - Generates ELF (Linux binaries)                  │     │
│  │  - Generates Mach-O (macOS binaries)               │     │
│  │  - Embeds mc² runtime in native executable         │     │
│  └────────────────────────────────────────────────────┘     │
│                                                               │
└─────────────────────────────────────────────────────────────┘

Key Features:
  - :: denotes line start (not line end like ;)
  - Auto type detection: :: x = 1 (infers int, creates hash)
  - Arbitrary precision: no integer or float limits
  - Hash-based variables: avoid collisions with function args
  - Nothing built-in: everything from assembly up
  - Scalable: handles 100,000+ line projects

Execution Modes:
  1. Runtime Mode: mc2 program.e² (portable, requires mc² installed)
  2. Native Mode: ricer.e² program.e² -o program.exe (standalone)

Build System:
  - build.bat (Windows) or Makefile (Unix)
  - Automated file generation via -arg flags
  - Each compiler version stored separately
  - Supports self-hosting through ricer
```

### Component Breakdown

1. **Preprocessor**
   - Expands macros (!charset system)
   - Processes @include directives for .e4² headers
   - Handles conditional compilation (if needed)
   - Resolves symbolic operator definitions

2. **Lexer (Tokenizer)**
   - Converts source text into tokens
   - Recognizes special syntax: ::, <<, >>, $, %, !, @, #
   - Handles comments: -(content) and -{...}
   - Tracks line/column for error reporting

3. **Parser**
   - Builds Abstract Syntax Tree (AST) from tokens
   - Enforces grammar rules
   - Handles operator precedence
   - Validates syntax structure

4. **Type Checker**
   - Validates type compatibility
   - Performs type inference for local variables
   - Checks function signatures
   - Validates namespace and import resolution

5. **Optimizer**
   - Constant folding
   - Dead code elimination
   - Inline expansion for small functions
   - Symbolic operator resolution

6. **Code Generator**
   - Produces spacetime assembly (advanced assembly format)
   - Handles calling conventions
   - Manages stack frames
   - Generates symbol tables for linking

7. **Assembler**
   - Converts spacetime assembly to .e² binary format
   - Encodes instructions in custom bytecode
   - Embeds metadata (symbols, types, debug info)
   - Produces OS-independent binary

8. **Runtime / Executor**
   - Integrated with compiler binary
   - Loads .e² files when executed
   - Reparses and executes bytecode
   - Provides OS abstraction layer
   - Handles system calls uniformly across platforms

9. **ricer.e² - Native Converter**
   - Standalone tool (itself a .e² file)
   - Converts .e² files to native OS executables
   - Generates platform-specific executable formats:
     - **Windows**: MZ/PE format (.exe)
     - **Linux**: ELF format (no extension)
     - **macOS**: Mach-O format (no extension)
     - **UEFI**: EFI format (.efi)
     - **Other**: PK and custom formats
   - Embeds mc² runtime into the native executable
   - Resulting executable is standalone (no mc² installation required)
   - Self-hosting: ricer itself is compiled to .e², then riced to native

### Spacetime Assembly

Spacetime assembly is the advanced assembly format used as an intermediate representation in mc². It is based on the syntax from a.mc² and uses mc²'s distinctive operators.

**Design Principles**:
- Uses :: for instruction prefixes (similar to statement starters in mc²)
- Uses << >> for operand grouping
- Supports symbolic operators defined in source
- More readable than traditional assembly
- Direct mapping to .e² bytecode

**Example Spacetime Assembly**:
```
:: func crazy << x:int, _y_:int >> -> int {
    :: load.const << x >> -> %r0
    :: load.const << _y_ >> -> %r1
    :: add << %r0, %r1 >> -> %r2
    :: call << print >> << _possible_args_ >> 
    {
        :: load.const << 42 >> -> %r3
        :: mul << %r2, %r3 >> -> %r4
        :: if << %r4 > 100 >> -> void {
            :: call << print >> << %r4, "is big!" >> 
        } else {
            :: call << print >> << %r4, "is small..." >> 
        }
        :: return << %r4 >>
    }
}

:: func print << _possible_args_ >> -> void {
    :: syscall << io_write >> << %r0 >>
    :: return
}

:: func input << prompt:str >> -> str {
    :: call << print >> << prompt >>
    :: syscall << io_read >> -> %r0
    :: return << %r0 >>
}
```

**Instruction Format**:
- `:: <opcode> << <operands> >> -> <destination>`
- Registers: %r0, %r1, %r2, etc.
- Labels: @label_name
- Constants: immediate values or $global_name
- Types: embedded in operands when needed

### .e² Binary Format

The .e² (or .e_sub2) format is a custom binary format that is:
- **NOT a native executable**: Does not contain platform-specific machine code
- **OS-independent**: Same .e² file runs on Windows, Linux, macOS
- **Self-contained**: Includes all necessary metadata and bytecode
- **Executable via mc²**: When double-clicked, OS launches mc² compiler/runtime to execute it

**File Structure**:
```
.e² File Format:
┌─────────────────────────────────────┐
│ Magic Number: "MC²\0" (4 bytes)    │
├─────────────────────────────────────┤
│ Version: uint32                     │
├─────────────────────────────────────┤
│ Header Size: uint32                 │
├─────────────────────────────────────┤
│ Metadata Section:                   │
│  - Symbol Table                     │
│  - Type Information                 │
│  - Import Dependencies              │
│  - Debug Information (optional)     │
├─────────────────────────────────────┤
│ Code Section:                       │
│  - Spacetime Assembly Bytecode      │
│  - Instruction Stream               │
│  - Constant Pool                    │
├─────────────────────────────────────┤
│ Data Section:                       │
│  - Global Variables                 │
│  - String Literals                  │
│  - Static Data                      │
└─────────────────────────────────────┘
```

**Execution Model**:
1. User double-clicks .e² file
2. OS file association launches: `mc2 <file>.e²`
3. mc² runtime loads and validates the .e² file
4. Runtime reparses bytecode into executable form
5. Runtime executes the program with OS abstraction
6. Program runs as if it were a native executable

## Components and Interfaces

### 1. Preprocessor Module

```
Interface: Preprocessor
  - expand_macros(source: String) -> String
  - process_includes(source: String, search_paths: List<Path>) -> String
  - resolve_symbolic_operators(source: String) -> String
  - get_macro_definitions() -> Map<String, Macro>

Macro Parameters:
  - __ANY_ARG__: Matches any single argument
  - __ARGS__: Matches all arguments as a list
  - _Get_space_: Inserts whitespace
  - _fragment_possible_args_: Matches argument fragments
  - __possible_args_: Variadic argument list with custom binding rules (e.g., :< br.set(,) :)
  - _possible_arg_type_: Type-level argument placeholder for generic/template-like behavior
```

The preprocessor handles three main tasks:
- **Macro Expansion**: Processes !charset macros and expands them inline, supporting variadic arguments via __possible_args_ with custom separators
- **Include Processing**: Resolves @include directives and inserts header content
- **Symbolic Operator Resolution**: Maps symbolic operators to their function implementations
- **Type-Level Generics**: Supports _possible_arg_type_ for compile-time type parameterization

### 2. Lexer Module

```
Interface: Lexer
  - tokenize(source: String) -> Result<List<Token>, LexError>
  - peek_token() -> Option<Token>
  - next_token() -> Option<Token>
  - get_position() -> Position

Token Types:
  - Keyword (type, namespace, function, if, while, import, from, get, etc.)
  - Identifier (variable/function names)
  - Operator (::, <<, >>, &&, ||, **, +, -, *, /, etc.)
  - Literal (int, float, string)
  - Symbol ($, %, !, @, #, _, -)
  - Delimiter ({, }, (, ), ,)
  - Comment (ignored after lexing)

Key Behavior:
  - :: is recognized as line start marker (required at beginning of statements)
  - Variable names are hashed internally to avoid collisions with function arguments
  - Example: :: x = 1 creates internal hash like __var_x_a3f2b1__ to prevent collision
```

The lexer must handle mc²'s unique syntax:
- Double colons (::) as a required line start element (not line end like ;)
- Angle brackets (<< >>) for parameter lists and conditions
- Underscores for optional parameters (_optional_arg_)
- Special prefixes ($, %, !, @, #)

### 2.5. Memory Management Module

```
Interface: MemoryManager (implemented in spacetime assembly)
  - alloc_bigint(value: int) -> BigInt
  - alloc_bigfloat(value: float) -> BigFloat
  - add_bigint(a: BigInt, b: BigInt) -> BigInt
  - mul_bigfloat(a: BigFloat, b: BigFloat) -> BigFloat
  - gc_collect() -> void
  - promote_to_bigint(value: int) -> BigInt
  - promote_to_bigfloat(value: float) -> BigFloat

BigInt Structure:
  - sign: bool
  - digits: List<uint64>  // Little-endian digit array
  - length: int

BigFloat Structure:
  - sign: bool
  - mantissa: BigInt
  - exponent: int
  - precision: int

Operations:
  - Automatic promotion when native types overflow
  - Garbage collection for unused large numbers
  - Arithmetic operations on arbitrary precision numbers
```

The memory manager provides:
- **No integer limits**: Integers can grow to any size
- **No float limits**: Floats maintain arbitrary precision
- **Automatic promotion**: Native types automatically promote when needed
- **Built-in GC**: Garbage collection for large number objects
- **Assembly implementation**: Core functionality in spacetime assembly

### 3. Parser Module

```
Interface: Parser
  - parse(tokens: List<Token>) -> Result<AST, ParseError>
  - parse_type_definition() -> TypeDef
  - parse_namespace() -> Namespace
  - parse_function() -> Function
  - parse_statement() -> Statement
  - parse_expression() -> Expression

AST Node Types:
  - Program (root node)
  - TypeDefinition
  - Namespace
  - ClassDefinition
  - FunctionDefinition
  - VariableDeclaration
  - ImportStatement
  - IfStatement
  - WhileLoop
  - ForLoop
  - Expression (binary, unary, call, member access)
  - Literal
```

The parser builds a tree structure representing the program's syntax. It enforces grammar rules and produces detailed error messages with line/column information.

### 4. Type System Module

```
Interface: TypeSystem
  - check_types(ast: AST) -> Result<TypedAST, TypeError>
  - infer_type(expression: Expression) -> Type
  - validate_assignment(lhs: Type, rhs: Type) -> Result<(), TypeError>
  - resolve_type_name(name: String) -> Option<Type>
  - auto_cast(value: Value, target_type: Type) -> Result<Value, TypeError>

Built-in Types:
  - int (arbitrary precision integer)
  - float (arbitrary precision float)
  - str (string)
  - void (no return value)
  - Null (null/none type)

Custom Types:
  - User-defined via type :: << state <:: define ::> :> syntax

Type Inference:
  - :: x = 1 infers int
  - :: y = 3.14 infers float
  - :: z = "hello" infers str
  - Creates hash-based internal name: __var_x_a3f2b1__

Auto Casting:
  - int -> float (automatic)
  - float -> int (requires explicit cast)
  - Compatible custom types (based on type definition)
```

The type system performs static type checking and inference. Local variables can have their types inferred (e.g., `:: x = 0` infers int and creates hash __var_x_hash__), while global variables and function parameters require explicit types.

### 4.5. Core Assembly Functions Module

```
Interface: CoreFunctions (implemented in spacetime assembly)
  - print(_possible_args_) -> void
  - input(prompt: str = "") -> str
  - malloc(size: int) -> ptr
  - free(ptr: ptr) -> void

print Function:
  - Variadic arguments via _possible_args_
  - Outputs to standard output
  - Handles multiple types (int, float, str)
  - Example: print << "Value:", 42, "is big" >>

input Function:
  - Reads from standard input
  - Optional prompt parameter
  - Returns string value
  - Example: :: name = input << "Enter name: " >>

Memory Functions:
  - malloc: Allocate memory for arbitrary precision numbers
  - free: Deallocate memory (called by GC)
  - Used by MemoryManager for BigInt/BigFloat allocation

Critical Requirement:
  - ALL functions implemented in spacetime assembly
  - NO built-in functions except what's in assembly
  - Must be tested thoroughly before C++ deletion
```

### 5. Code Generator Module

```
Interface: CodeGenerator
  - generate(typed_ast: TypedAST, target: Target) -> Result<String, CodeGenError>
  - generate_function(func: Function) -> String
  - generate_expression(expr: Expression) -> String
  - allocate_registers() -> RegisterAllocation

Targets:
  - X86_64 (native x86-64 assembly)
  - SpacetimeAssembly (custom mc²-style assembly)
```

The code generator produces assembly code from the typed AST. It supports two output formats:
- **Native Assembly**: Standard x86-64 assembly for direct execution
- **Spacetime Assembly**: Custom assembly format with mc²-like syntax (::, <<, >>)

### 6. Module Linker

```
Interface: Linker
  - link_modules(modules: List<CompiledModule>) -> Result<Executable, LinkError>
  - resolve_imports(module: Module) -> Result<(), LinkError>
  - check_circular_dependencies(modules: List<Module>) -> Result<(), LinkError>
```

The linker combines compiled modules into a single executable. It resolves imports, handles namespace references, and ensures no circular dependencies exist.

### 7. Command-Line Interface

```
Interface: CLI
  - parse_arguments(args: List<String>) -> Result<CompilerConfig, CLIError>
  - display_help() -> void
  - display_version() -> void
```

The CLI module handles command-line argument parsing and configuration:

**Supported Flags**:
- `-h, --help`: Display usage information
- `-o, --output <path>`: Specify output file path
- `-v, --version`: Display compiler version
- `--verbose`: Enable detailed compilation output
- `--loose-indent`: Enable loose indentation mode (warnings instead of errors)
- `--extra-loose-indent`: Disable indentation validation entirely
- `--indent-size <n>`: Set expected indentation size (default: 4 spaces or 1 tab)

**Indentation Modes**:
1. **Strict Mode (default)**: Indentation mismatches produce errors
2. **Loose Mode (--loose-indent)**: Indentation mismatches produce warnings
3. **Extra-Loose Mode (--extra-loose-indent)**: No indentation validation

**Flag Priority**:
- If both `--loose-indent` and `--extra-loose-indent` are specified, extra-loose mode takes precedence
- Invalid flag combinations produce clear error messages

### 8. Build System

The build system is implemented via:
- **build.bat** (Windows): Batch script for building the compiler
- **Makefile** (Unix/Linux/macOS): Make-based build system

**Build System Features**:
- Automated file generation via command-line arguments (no inline input)
- Compiler version management (each version stored separately)
- Dependency tracking and incremental compilation
- Cross-platform support

**Build Commands**:
```bash
# Windows
build.bat -compile src/main.mc² -output bin/program.e²
build.bat -version v-1-0-0-0-0 -self-host

# Unix/Linux/macOS
make compile SRC=src/main.mc² OUT=bin/program.e²
make self-host VERSION=v-1-0-0-0-0
```

**Compiler Version Storage** (format: v-.-.-.-.- or prefix-.-.-.-.-.- saved in bin/):
```
bin/
├── v-0-0-0-1-0/       (C++ bootstrap compiler)
├── alpha-0-0-0-2-0/   (First alpha self-hosted version)
├── alpha-0-0-0-3-0/   (Compiled by alpha-0-0-0-2-0)
├── beta-0-0-1-0-0/    (Beta release)
├── rc-1-0-0-0-0/      (Release candidate)
├── v-1-0-0-0-0/       (First stable release)
└── latest/            (Symlink to newest version)
```

**Supported version prefixes**: `v`, `alpha`, `beta`, `rc`, `dev`

**Architecture**:
- Single main compiler (initially assembler, evolves to full compiler)
- Compiler takes `.mc²` or `.spacetime` → produces `.e²` files
- `.e²` files run in presence of compiler (integrated runtime)
- `ricer.e²` converts `.e²` → native OS executables (standalone)

Each compiler version can compile the next, ensuring reproducibility and validation of self-hosting.

## Data Models

### Token

```
Token {
  type: TokenType
  lexeme: String
  line: int
  column: int
  file: String
}
```

### AST Nodes

```
Program {
  imports: List<ImportStatement>
  type_definitions: List<TypeDef>
  namespaces: List<Namespace>
  functions: List<Function>
  global_variables: List<GlobalVar>
}

TypeDef {
  name: String
  state: String
  definition: String
  body: String
}

Namespace {
  name: String
  sections: List<Section>
  functions: List<Function>
  global_variables: List<GlobalVar>
  macros: List<Macro>
}

Function {
  return_type: Type
  name: String
  parameters: List<Parameter>
  body: List<Statement>
}

Parameter {
  name: String
  type: Type
  optional: bool
}

GlobalVar {
  name: String  // without $ prefix
  type: Type
  initial_value: Option<Expression>
}

ImportStatement {
  keyword: String  // "import" or "get"
  file: String
  selector: ImportSelector  // *, $, %, or specific names
  alias: Option<String>
}

IfStatement {
  condition: Expression
  then_body: List<Statement>
  else_body: Option<List<Statement>>
}

Expression {
  // Binary operations, function calls, member access, literals, etc.
}
```

### Type Representation

```
Type {
  kind: TypeKind
  name: String
  custom_definition: Option<String>
}

TypeKind = Int | Float | Str | Void | Null | Custom
```

### Macro Definition

```
Macro {
  name: String
  value: String
  parameters: List<String>  // __ARGS__, __ANY_ARG__, _Get_space_, _fragment_possible_args_, __possible_args_, _possible_arg_type_
  binding_rules: Option<String>  // e.g., ":< br.set(,) :" for __possible_args_
}

SymbolicOperator {
  symbol: String  // e.g., "**"
  function_ref: FunctionReference
  argument_pattern: String  // e.g., "_arg << ** >> arg_"
}
```

### Compiled Module

```
CompiledModule {
  name: String
  exports: Exports
  assembly_code: String
  symbol_table: Map<String, Symbol>
}

Exports {
  functions: List<String>
  global_variables: List<String>
  types: List<String>
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Parse-Print Round Trip
*For any* valid mc² program, parsing it to an AST, pretty-printing the AST back to source code, and parsing again should produce an equivalent AST.
**Validates: Requirements 18.5**

### Property 2: Syntax Element Recognition
*For any* valid mc² syntax element (type definition, namespace, function, variable, import, macro, etc.), the parser should correctly identify it and create the appropriate AST node type.
**Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.6, 1.10, 1.11, 1.12, 1.13, 1.16, 1.19, 1.23, 1.24**

### Property 3: Optional Parameter Recognition
*For any* function definition with parameters marked with underscores (_optional_arg_), the parser should mark those parameters as optional in the AST.
**Validates: Requirements 1.5**

### Property 4: Import Syntax Equivalence
*For any* module import, using "import" or "get" keywords should produce equivalent AST nodes with the same semantic meaning.
**Validates: Requirements 1.7, 7.3**

### Property 5: Import Selector Preservation
*For any* import statement with a selector (*, $, %), the AST should correctly capture which selector was used.
**Validates: Requirements 1.9, 7.2**

### Property 6: Comment Elimination
*For any* mc² source code containing comments (-(content) or -{...}), the parsed AST should not contain any comment nodes.
**Validates: Requirements 1.14, 1.15**

### Property 7: Chained Property Access Parsing
*For any* expression with chained property access (a.b.c.d), the parser should create a nested member access AST structure.
**Validates: Requirements 1.17**

### Property 8: Operator Recognition
*For any* expression containing operators (&&, ||, **, ==, !=, <, >, <=, >=, +, -, *, /, %), the parser should correctly identify the operator and create the appropriate binary expression node.
**Validates: Requirements 1.20, 1.21, 1.22, 3.8, 3.9**

### Property 9: Double Colon Enforcement
*For any* syntax element that requires :: (double colon), omitting it should result in a parse error.
**Validates: Requirements 1.25**

### Property 10: Type Inference Correctness
*For any* local variable declaration with initialization (:: x = value), the inferred type should match the type of the initialization value.
**Validates: Requirements 2.3**

### Property 11: Type Compatibility Validation
*For any* assignment or function call, if the types are incompatible, the type checker should produce an error.
**Validates: Requirements 2.4**

### Property 12: Type Error Messages
*For any* type mismatch error, the error message should contain both the expected type and the actual type.
**Validates: Requirements 2.5, 15.2**

### Property 13: Truthiness Semantics
*For any* conditional expression with a numeric value, zero should evaluate to False and non-zero should evaluate to True.
**Validates: Requirements 3.10**

### Property 14: Semantic Preservation
*For any* valid mc² program, compiling it and executing the result should produce the same behavior as interpreting the source directly.
**Validates: Requirements 12.7**

### Property 15: Calling Convention Correctness
*For any* function call in generated assembly, the calling convention should match the target platform's ABI specification.
**Validates: Requirements 12.6**

### Property 16: Spacetime Assembly Syntax
*For any* code generated in spacetime assembly format, the output should use mc² syntax elements (::, <<, >>).
**Validates: Requirements 12.3**

### Property 17: Bootstrap-Self-Hosted Equivalence
*For any* valid mc² program, compiling it with the bootstrap compiler and with the self-hosted compiler should produce functionally equivalent output.
**Validates: Requirements 5.4**

### Property 18: Module Import Resolution
*For any* import statement referencing an existing module, the compiler should successfully resolve and link the module.
**Validates: Requirements 7.1**

### Property 19: Import Aliasing
*For any* aliased import (import file.mc² as alias), references to the alias should resolve to the imported module's exports.
**Validates: Requirements 7.4**

### Property 20: Namespace Cross-Module Access
*For any* namespace defined in an imported module, it should be accessible from the importing module using qualified names.
**Validates: Requirements 7.5**

### Property 21: Circular Import Detection
*For any* set of modules with circular import dependencies, the compiler should detect the cycle and produce an error.
**Validates: Requirements 7.6**

### Property 22: Header Processing Order
*For any* source file with @include directives, declarations in the header should be available in the source file.
**Validates: Requirements 8.3**

### Property 23: Forward Declaration Support
*For any* type forward-declared in a header file, it should be usable in the source file before its full definition.
**Validates: Requirements 8.4**

### Property 24: Declaration-Implementation Matching
*For any* function or type declared in a header, if the implementation doesn't match the declaration, the compiler should produce an error.
**Validates: Requirements 8.5**

### Property 25: Global Variable Scope
*For any* global variable $(<name>) declared in a module, it should be accessible from all functions within that module.
**Validates: Requirements 9.3**

### Property 26: Imported Global Access
*For any* global variable imported from another module, it should be accessible using module.$<name> syntax.
**Validates: Requirements 9.4**

### Property 27: Global Redeclaration Prevention
*For any* global variable, attempting to declare it twice in the same module should produce an error.
**Validates: Requirements 9.5**

### Property 28: Macro Expansion
*For any* macro definition and usage, the macro should be expanded during preprocessing before parsing begins.
**Validates: Requirements 10.2**

### Property 29: Macro Parameter Support
*For any* macro using special parameters (__ANY_ARG__, __ARGS__, _Get_space_, _fragment_possible_args_, __possible_args_, _possible_arg_type_), the parameters should be correctly substituted during expansion.
**Validates: Requirements 10.4**

### Property 30: Symbolic Operator Resolution
*For any* symbolic operator usage, the compiler should resolve it to the function defined via @assign.symboliccharset.
**Validates: Requirements 11.5**

### Property 31: Operator Argument Pattern Matching
*For any* symbolic operator with an argument pattern (_arg << symbol >> arg_), the compiler should correctly parse and apply the pattern.
**Validates: Requirements 11.2**

### Property 32: Multi-Argument Operator Support
*For any* symbolic operator with multiple arguments separated by --, the compiler should correctly parse all arguments.
**Validates: Requirements 11.3**

### Property 33: Standard Library Auto-Resolution
*For any* import of a standard library module, the compiler should locate it automatically without requiring a full path.
**Validates: Requirements 13.6**

### Property 34: Dependency Order Compilation
*For any* set of modules with dependencies, the build system should compile them in an order that satisfies all dependencies.
**Validates: Requirements 14.1**

### Property 35: Incremental Compilation
*For any* project where only some files have changed, the build system should only recompile the changed files and their dependents.
**Validates: Requirements 14.2**

### Property 36: Module Linking
*For any* set of compiled modules, the build system should link them into a single executable.
**Validates: Requirements 14.4**

### Property 37: Build Error File Identification
*For any* build failure, the error message should identify which file caused the failure.
**Validates: Requirements 14.5**

### Property 38: Syntax Error Reporting
*For any* syntax error, the error message should include the file path, line number, column number, and a readable description.
**Validates: Requirements 15.1**

### Property 39: Import Error Path Reporting
*For any* failed import, the error message should include the missing file path and the search locations that were checked.
**Validates: Requirements 15.3**

### Property 40: Macro Error Context Reporting
*For any* macro expansion failure, the error message should include the macro name, usage context, and reason for failure.
**Validates: Requirements 15.4**

### Property 41: Color-Coded Error Output
*For any* compiler message, errors should use red color codes, warnings should use yellow, and info messages should use blue.
**Validates: Requirements 15.5**

### Property 42: Error Code Snippets
*For any* compilation error, the error message should include a code snippet showing the error location with surrounding context lines.
**Validates: Requirements 15.6**

### Property 43: Error Suggestion Provision
*For any* common error pattern, the error message should include a suggestion for how to fix it.
**Validates: Requirements 15.7**

### Property 44: Multiple Error Ordering
*For any* source file with multiple errors, the compiler should report them in source order (by line number) with clear separation between errors.
**Validates: Requirements 15.8**

### Property 45: Pretty Printer Validity
*For any* AST, the pretty printer should produce syntactically valid mc² source code that can be parsed without errors.
**Validates: Requirements 18.4**

### Property 46: Parallel Compilation
*For any* set of independent modules (no dependencies between them), the build system should be able to compile them in parallel.
**Validates: Requirements 17.3**

### Property 47: Compilation Progress Reporting
*For any* large project compilation, the compiler should output progress updates indicating which files are being compiled.
**Validates: Requirements 17.5**

### Property 48: Indentation Validation in Strict Mode
*For any* mc² source file with blocks compiled in strict mode (default), if indentation does not match block nesting depth, the parser should produce an error.
**Validates: Requirements 1.26, 1.27, 1.28, 1.5.7**

### Property 49: Indentation Warning in Loose Mode
*For any* mc² source file with blocks compiled with --loose-indent flag, if indentation does not match block nesting depth, the parser should produce a warning (not an error).
**Validates: Requirements 1.5.8, 21.5**

### Property 50: No Indentation Validation in Extra-Loose Mode
*For any* mc² source file compiled with --extra-loose-indent flag, the parser should NOT validate indentation at all.
**Validates: Requirements 1.5.9, 21.6**

### Property 51: Mixed Indentation Detection
*For any* mc² source file, if tabs and spaces are mixed for indentation, the parser should produce an error regardless of indentation mode.
**Validates: Requirements 1.5.11**


## Error Handling

### Error Categories

The mc² compiler will produce errors in the following categories:

1. **Lexical Errors**: Invalid characters, malformed tokens, unclosed strings/comments
2. **Syntax Errors**: Grammar violations, missing required elements (::), unbalanced delimiters
3. **Semantic Errors**: Type mismatches, undefined variables, circular dependencies
4. **Import Errors**: Missing files, circular imports, invalid import selectors
5. **Macro Errors**: Undefined macros, invalid macro parameters, expansion failures
6. **Linking Errors**: Unresolved symbols, duplicate definitions, ABI mismatches

### Error Reporting Format

Each error message will follow this structure:

```
[ERROR] <file>:<line>:<column>: <description>
  |
<line-1> | <context code>
<line>   | <code with error>
         | <caret pointing to error>
<line+1> | <context code>
  |
  = help: <suggestion for fixing>
```

Example:

```
[ERROR] main.mc²:15:8: Type mismatch in assignment
  |
14 | :: x :: int
15 | x = "hello"
   |     ^^^^^^^ expected 'int', found 'str'
16 | 
  |
  = help: Convert the string to an integer using str.to_int()
```

### Error Recovery

The compiler will attempt to recover from errors to report multiple issues in a single compilation:

- **Syntax Errors**: Skip to the next statement boundary (;, }, or ::)
- **Type Errors**: Continue type checking with an "error" type placeholder
- **Import Errors**: Mark module as unavailable but continue checking other modules

### Error Codes

Each error type will have a unique code (e.g., E001, E002) for documentation and tooling:

- **E001-E099**: Lexical errors
- **E100-E199**: Syntax errors
- **E200-E299**: Type errors
- **E300-E399**: Import/module errors
- **E400-E499**: Macro errors
- **E500-E599**: Code generation errors
- **E600-E699**: Linking errors

## Testing Strategy

### Dual Testing Approach

The mc² compiler will be validated using both unit tests and property-based tests:

**Unit Tests**: Verify specific examples, edge cases, and error conditions
- Specific syntax examples (valid and invalid)
- Edge cases (empty files, deeply nested structures, large files)
- Error message formatting
- Integration between compiler phases

**Property-Based Tests**: Verify universal properties across all inputs
- Parse-print round trips with randomly generated ASTs
- Type checking with randomly generated expressions
- Semantic preservation with randomly generated programs
- Error reporting consistency with randomly generated invalid code

### Property-Based Testing Configuration

**Library Selection**:
- **Python Bootstrap**: Use Hypothesis for property-based testing
- **mc² Self-Hosted**: Implement a property testing library in mc² itself

**Test Configuration**:
- Minimum 100 iterations per property test
- Each test tagged with: **Feature: mc2-language, Property N: [property text]**
- Shrinking enabled to find minimal failing examples

**Example Property Test**:

```python
# Feature: mc2-language, Property 1: Parse-Print Round Trip
@given(valid_mc2_ast())
@settings(max_examples=100)
def test_parse_print_round_trip(ast):
    source1 = pretty_print(ast)
    ast2 = parse(source1)
    source2 = pretty_print(ast2)
    ast3 = parse(source2)
    assert ast_equal(ast2, ast3)
```

### Test Organization

```
tests/
├── unit/
│   ├── lexer/
│   │   ├── test_tokenization.py
│   │   ├── test_comments.py
│   │   └── test_special_syntax.py
│   ├── parser/
│   │   ├── test_types.py
│   │   ├── test_functions.py
│   │   ├── test_namespaces.py
│   │   └── test_expressions.py
│   ├── typechecker/
│   │   ├── test_inference.py
│   │   ├── test_validation.py
│   │   └── test_errors.py
│   ├── codegen/
│   │   ├── test_x86_64.py
│   │   ├── test_spacetime.py
│   │   └── test_optimization.py
│   └── integration/
│       ├── test_end_to_end.py
│       ├── test_self_hosting.py
│       └── test_stdlib.py
├── property/
│   ├── test_parse_print.py
│   ├── test_type_checking.py
│   ├── test_semantic_preservation.py
│   ├── test_error_reporting.py
│   └── generators.py  # Random AST/code generators
└── fixtures/
    ├── valid/  # Valid mc² programs
    ├── invalid/  # Invalid programs for error testing
    └── stdlib/  # Standard library test programs
```

### Self-Hosting Validation

The ultimate test of the mc² compiler is self-hosting:

1. **Bootstrap Phase**: Compile the self-hosted compiler using the bootstrap compiler
2. **Stage 1**: Use the bootstrapped compiler to compile itself (produces Stage 1 compiler)
3. **Stage 2**: Use Stage 1 compiler to compile itself (produces Stage 2 compiler)
4. **Validation**: Stage 1 and Stage 2 compilers should be binary identical (or functionally equivalent)

This three-stage bootstrap validates that:
- The compiler can compile itself
- The compiled compiler produces consistent output
- The language is complete enough to implement its own compiler

### Continuous Integration

All tests will run on:
- Linux (Ubuntu latest)
- macOS (latest)
- Windows (latest)

Each commit will:
1. Run all unit tests
2. Run all property tests
3. Compile the self-hosted compiler (if applicable)
4. Run the self-hosting validation
5. Check for memory leaks (valgrind on Linux)
6. Measure compilation performance

### Test Coverage Goals

- **Line Coverage**: Minimum 85% of compiler code
- **Branch Coverage**: Minimum 80% of conditional branches
- **Property Coverage**: All 47 correctness properties implemented as tests
- **Feature Coverage**: All language features tested with at least one unit test

### Performance Benchmarks

Track performance metrics over time:
- Compilation speed (lines per second)
- Memory usage during compilation
- Generated code size
- Generated code execution speed

Benchmarks will use:
- Small programs (< 100 lines)
- Medium programs (100-1000 lines)
- Large programs (> 1000 lines)
- Self-hosting compilation (compiling the compiler itself)

## Project Structure

```
mc2-language/
├── src/                    # C++ source code for bootstrap compiler
│   ├── lexer/
│   ├── parser/
│   ├── typechecker/
│   ├── codegen/
│   ├── assembler/
│   ├── runtime/
│   └── main.cpp
├── mc2/                    # mc² source code for self-hosted compiler
│   ├── compiler/
│   ├── ricer/
│   └── stdlib/
├── docs/                   # Comprehensive documentation
│   ├── syntax-reference.md
│   ├── operator-reference.md
│   ├── macro-system.md
│   ├── spacetime-assembly.md
│   ├── e2-format.md
│   ├── ricer-guide.md
│   ├── build-system.md
│   ├── self-hosting.md
│   └── examples/
├── tests/                  # Test suite
│   ├── unit/
│   ├── property/
│   └── fixtures/
├── bin/                    # Versioned compiler binaries (v-.-.-.-.- format)
│   ├── v-0-0-0-1-0/
│   ├── v-0-0-0-2-0/
│   └── latest/
├── garbage/                # Temporary files, logs, reports (gitignored)
├── build.bat               # Windows build script
├── Makefile                # Unix build script
├── .gitignore              # Excludes garbage/ and build artifacts
└── README.md               # Project overview
```

**Documentation Requirements**:
- All syntax and operators must be documented in docs/
- Temporary reports and logs go in garbage/ (not committed)
- Examples should demonstrate real-world usage patterns
- Documentation should be comprehensive and searchable
