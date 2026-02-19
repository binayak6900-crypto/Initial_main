# Requirements Document: mc² Programming Language

## Introduction

mc² (pronounced "em-see-squared") is a self-hosted, OS-independent programming language designed to be "as absurd as physics" while maintaining practical utility. The language embraces unconventional syntax and symbolic operators, supports both .mc² and .mc_sup2 file extensions, and features built-in memory management with arbitrary precision integers and floats (no limits).

**Critical Design Philosophy**: Nothing is built-in by default. Everything—including basic functions like print()—must be built from assembly first. The language provides the assembly foundation, and all higher-level features are constructed on top of it.

**Development Approach**:
1. Build a complete assembler (spacetime assembly) in C++
2. Implement core functions (print, memory management) in assembly
3. Test assembly thoroughly
4. **DELETE all C++ files** - no going back
5. Self-host: rewrite compiler in .mc² using the assembly foundation
6. Build all syntax features using assembly and inline assembly

The compiler produces .e² files—a universal compiled format that is NOT a native executable but is executed by the mc² runtime. This provides true OS independence: the same .e² file runs on Windows, Linux, and macOS without modification.

The build system uses build.bat (Windows) or Makefile (Unix) with command-line arguments for automation, and each compiler version is stored separately to enable reproducible self-hosting validation.

## Glossary

- **mc² Language**: The programming language being specified, with syntax inspired by physics notation
- **mc² Compiler**: The compiler executable, named "mc²" (alternatives: "mc2" or "mc_sup2"), initially bootstrapped from assembly
- **mc² Runtime**: The execution engine integrated with the compiler that loads and executes .e² files
- **Bootstrap Assembler**: Initial C++ assembler that processes spacetime assembly (DELETED after assembly is complete)
- **Self-Hosted Compiler**: The mc² compiler written in mc² itself using assembly and inline assembly, compiled to .e² format
- **Memory Manager**: Built-in arbitrary precision integer and float system (no limits on size)
- **.e² Format**: Universal compiled binary format (alternative: .e_sub2), OS-independent, executed by mc² runtime
- **Spacetime Assembly**: Advanced assembly format specific to mc², intermediate representation before .e² generation
- **Parser**: Component that reads mc² source code and builds an abstract syntax tree
- **Code Generator**: Component that transforms AST into spacetime assembly
- **Assembler**: Component that converts spacetime assembly into .e² binary format
- **Type System**: The system managing types with custom syntax (type :: << state <:: define ::> :>)
- **Namespace**: Code organization unit containing functions, global variables, and macros
- **Charset Macro**: Macro system using !charset for defining symbolic operators
- **Global Variable**: Variable prefixed with $ accessible across scopes
- **Symbolic Operator**: Custom operator defined via @assign.symboliccharset
- **Header File**: .e4² files (alternative: .e4_sup4) similar to C++ headers
- **Double Colon (::)**: Required syntax element denoting the START of a line (similar to ; but for line beginning, not end)
- **Hash Variable**: Internal variable name generated from user variable name to avoid collisions with function arguments
- **Auto Type Detection**: Automatic type inference from initialization values (e.g., :: x = 1 infers int)
- **Build System**: Automated build via build.bat (Windows) or Makefile (Unix) with -arg command-line flags
- **Indentation**: Whitespace at the beginning of lines, used as a secondary validation mechanism alongside braces
- **Strict Indentation Mode**: Compiler mode where indentation mismatches cause errors
- **Loose Indentation Mode**: Compiler mode where indentation mismatches cause warnings only

## Requirements

### Requirement 1: Language Syntax and Grammar

**User Story:** As a language designer, I want a formally specified grammar for mc², so that the language has consistent, parseable syntax.

#### Acceptance Criteria

1. THE Parser SHALL recognize type definitions using the syntax: type :: << state <:: define ::> :> set.type.setname.typename -> ::< >{<content>}
2. THE Parser SHALL recognize namespace definitions using the syntax: namespace :: <name>{<content>}
3. THE Parser SHALL recognize class definitions using the syntax: classdef :: class.for<purpose>
4. THE Parser SHALL recognize function definitions using the syntax: <type> :: <name> << (args) >> {<content>}
5. THE Parser SHALL recognize optional function arguments marked with underscores: _optional_arg_
6. THE Parser SHALL recognize variable declarations using syntax: :: <name> or :: <name> = <value>
7. THE Parser SHALL recognize import statements using both "import" and "get" keywords
8. THE Parser SHALL recognize import statements in forms: "import file.mc²" and "from file.mc² import *"
9. THE Parser SHALL support selective imports with symbols: * (all), $ (global variables), % (functions)
10. THE Parser SHALL recognize global variables prefixed with $: $(<name>) :: <type>
11. THE Parser SHALL recognize macro definitions using !charset syntax
12. THE Parser SHALL recognize symbolic operator assignments via @assign.symboliccharset
13. THE Parser SHALL recognize header includes via @include syntax for .e4² files
14. THE Parser SHALL recognize limited-area comments using syntax: -(content) where only the content inside parentheses is commented
15. THE Parser SHALL recognize multi-line comments enclosed in -{...} for better multi-line support
16. THE Parser SHALL recognize section definitions using #<section name> :: << {}
17. THE Parser SHALL support chained property access (something.something.something)
18. THE Parser SHALL accept file extensions: .mc², .mc_sup2, .e4², .e4_sup4
19. THE Parser SHALL recognize conditional statements using the syntax: if << <condition> :: True && || >> {<body>}
20. THE Parser SHALL recognize logical AND operator: &&
21. THE Parser SHALL recognize logical OR operator: ||
22. THE Parser SHALL recognize the pre-built exponentiation operator: **
23. THE Parser SHALL recognize while loops using similar conditional syntax
24. THE Parser SHALL recognize for loops with mc²-style syntax
25. THE Parser SHALL enforce that :: (double colon) is NOT optional in syntax where specified
26. THE Parser SHALL recognize indentation levels within blocks
27. THE Parser SHALL validate that indentation matches block nesting depth based on the active indentation mode
28. WHEN in strict mode and indentation does not match block structure, THE Parser SHALL produce an error
29. WHEN in loose mode and indentation does not match block structure, THE Parser SHALL produce a warning
30. WHEN in extra-loose mode, THE Parser SHALL NOT validate indentation

### Requirement 1.5: Indentation and Block Structure

**User Story:** As a language designer, I want indentation to be validated alongside brace structure, so that the language is "unreasonably logical" by requiring both.

#### Acceptance Criteria

1. THE Parser SHALL track indentation level for each line
2. THE Parser SHALL use braces {} for block structure (mandatory)
3. THE Parser SHALL use indentation as a secondary validation mechanism
4. WHEN entering a block with {, THE Parser SHALL expect increased indentation on the next line
5. WHEN exiting a block with }, THE Parser SHALL expect decreased indentation
6. THE Parser SHALL support three indentation modes: strict (default), loose, and extra-loose
7. WHEN in strict mode and indentation does not match block depth, THE Parser SHALL produce an error
8. WHEN in loose mode (--loose-indent flag) and indentation does not match block depth, THE Parser SHALL produce a warning
9. WHEN in extra-loose mode (--extra-loose-indent flag), THE Parser SHALL NOT validate indentation at all
10. THE Parser SHALL allow tabs or spaces for indentation (but not mixed within a file)
11. WHEN tabs and spaces are mixed in a file, THE Parser SHALL produce an error
12. THE Parser SHALL calculate expected indentation as: base_indent + (block_depth * indent_size)
13. THE Parser SHALL allow configurable indent_size (default: 4 spaces or 1 tab)
14. THE Parser SHALL enforce indentation on ALL statements within blocks in strict mode
15. THE Compiler SHALL accept --loose-indent command-line flag to enable loose mode
16. THE Compiler SHALL accept --extra-loose-indent command-line flag to enable extra-loose mode
17. WHEN both --loose-indent and --extra-loose-indent are specified, THE Compiler SHALL use extra-loose mode

### Requirement 2: Type System

**User Story:** As a developer, I want a type system with custom syntax and arbitrary precision numbers, so that I can work with numbers of any size without overflow.

#### Acceptance Criteria

1. THE Type_System SHALL support basic types: str, int, float, void, Null
2. THE Type_System SHALL support arbitrary precision integers (no maximum or minimum value)
3. THE Type_System SHALL support arbitrary precision floats (no precision limits)
4. THE Type_System SHALL automatically allocate memory for large numbers
5. THE Type_System SHALL support custom type definitions via type :: << state <:: define ::> :> syntax
6. THE Type_System SHALL support type inference for local variables (e.g., :: x = 0 infers int)
7. THE Type_System SHALL support automatic type casting between compatible types
8. THE Type_System SHALL validate type compatibility during compilation
9. WHEN a type mismatch occurs, THE Type_System SHALL produce a descriptive error message
10. WHEN a variable is declared with :: x = value, THE Type_System SHALL create a hash-based internal name to avoid collisions with function arguments

### Requirement 2.5: Memory Management

**User Story:** As a developer, I want built-in memory management for arbitrary precision numbers, so that I don't have to worry about overflow or manual memory allocation.

#### Acceptance Criteria

1. THE Memory_Manager SHALL be implemented in spacetime assembly
2. THE Memory_Manager SHALL automatically allocate memory for integers exceeding native word size
3. THE Memory_Manager SHALL automatically allocate memory for floats exceeding native precision
4. THE Memory_Manager SHALL perform garbage collection for unused large numbers
5. THE Memory_Manager SHALL handle arithmetic operations on arbitrary precision numbers
6. WHEN an integer operation would overflow native size, THE Memory_Manager SHALL automatically promote to arbitrary precision
7. WHEN a float operation would lose precision, THE Memory_Manager SHALL automatically promote to arbitrary precision

### Requirement 3: Control Flow and Operators

**User Story:** As a developer, I want control flow statements and logical operators, so that I can write conditional and iterative logic.

#### Acceptance Criteria

1. THE Parser SHALL recognize if statements with syntax: if << <condition> :: True && || >> {<body>}
2. THE Parser SHALL support logical AND operator (&&) with comment annotation -(and)
3. THE Parser SHALL support logical OR operator (||) with comment annotation -(or)
4. THE Parser SHALL support else and else-if clauses in conditional statements
5. THE Parser SHALL recognize while loops with similar conditional syntax
6. THE Parser SHALL recognize for loops with mc²-style iteration syntax
7. THE Compiler SHALL provide pre-built exponentiation operator (**)
8. THE Compiler SHALL support comparison operators: ==, !=, <, >, <=, >=
9. THE Compiler SHALL support arithmetic operators: +, -, *, /, %
10. WHEN evaluating conditions, THE Compiler SHALL treat non-zero values as True and zero as False

### Requirement 4: C++ Bootstrap Assembler

**User Story:** As a language implementer, I want a bootstrap assembler written in C++, so that I can process spacetime assembly and build the foundation for self-hosting.

#### Acceptance Criteria

1. THE Bootstrap_Assembler SHALL be implemented in C++ without external dependencies
2. THE Bootstrap_Assembler SHALL parse spacetime assembly syntax
3. THE Bootstrap_Assembler SHALL generate .e² binary format from assembly
4. THE Bootstrap_Assembler SHALL be used ONLY to build the initial assembly foundation
5. THE Bootstrap_Assembler SHALL process assembly for core functions (print, input, memory management)
6. WHEN the assembly foundation is complete and tested, ALL C++ files SHALL be deleted
7. THE Bootstrap_Assembler SHALL NOT be used after the assembly foundation is established
8. THE Bootstrap_Assembler SHALL accept file extensions: .mc², .mc_sup2
9. WHEN parsing fails, THE Bootstrap_Assembler SHALL report line numbers and error descriptions
10. WHEN assembly succeeds, THE Bootstrap_Assembler SHALL produce a .e² or .e_sub2 file

### Requirement 5: Self-Hosted Compiler

**User Story:** As a language designer, I want the mc² compiler written in mc² itself using assembly and inline assembly, so that the language is self-hosted and proves its completeness.

#### Acceptance Criteria

1. THE Self_Hosted_Compiler SHALL be written entirely in mc² source code
2. THE Self_Hosted_Compiler SHALL use spacetime assembly and inline assembly for core functionality
3. THE Self_Hosted_Compiler SHALL compile mc² source files to .e² binary format
4. THE Self_Hosted_Compiler SHALL be built AFTER the assembly foundation is complete and C++ files are deleted
5. THE Self_Hosted_Compiler SHALL produce identical output to the Bootstrap_Assembler for valid programs
6. WHEN the Self_Hosted_Compiler (as .e²) compiles itself, THE output SHALL be functionally equivalent to the input compiler
7. THE Self_Hosted_Compiler SHALL demonstrate all language features in its own implementation
8. THE Self_Hosted_Compiler SHALL serve as the reference implementation once bootstrapping is complete
9. WHEN executed as a .e² file, THE Self_Hosted_Compiler SHALL run via the mc² runtime without requiring external tools
10. THE Self_Hosted_Compiler SHALL NOT rely on any C++ code or external dependencies

### Requirement 5.5: .e² Binary Format and Runtime

**User Story:** As a user, I want .e² files to be executable across all operating systems, so that I can distribute programs portably or convert them to native executables.

#### Acceptance Criteria

1. THE .e² Format SHALL be a custom binary format NOT based on any existing executable format
2. THE .e² Format SHALL include a magic number "MC²\0" for file identification
3. THE .e² Format SHALL include version information for compatibility checking
4. THE .e² Format SHALL include metadata sections for symbols, types, and imports
5. THE .e² Format SHALL include a code section with spacetime assembly bytecode
6. THE .e² Format SHALL include a data section for globals and constants
7. THE .e² Format SHALL support both .e² and .e_sub2 file extensions
8. THE mc² Runtime SHALL be integrated with the compiler binary
9. WHEN a .e² file is executed via mc² runtime, THE runtime SHALL load and validate it
10. THE mc² Runtime SHALL reparse bytecode and execute it
11. THE mc² Runtime SHALL provide OS abstraction for system calls
12. THE mc² Runtime SHALL run the same .e² file on Windows, Linux, and macOS without modification
13. THE .e² Format SHALL NOT contain platform-specific machine code (only bytecode)

### Requirement 5.6: ricer.e² - Native Executable Converter

**User Story:** As a developer, I want to convert .e² files to native OS executables, so that I can distribute standalone programs without requiring mc² installation.

#### Acceptance Criteria

1. THE ricer Tool SHALL be implemented as a .e² file (ricer.e²)
2. THE ricer Tool SHALL convert .e² files to native executable formats
3. THE ricer Tool SHALL generate MZ/PE format executables for Windows (.exe)
4. THE ricer Tool SHALL generate ELF format executables for Linux
5. THE ricer Tool SHALL generate Mach-O format executables for macOS
6. THE ricer Tool SHALL support additional formats: EFI (.efi), PK, and custom formats
7. THE ricer Tool SHALL embed the mc² runtime into the generated native executable
8. THE ricer Tool SHALL accept command-line arguments: ricer.e² input.e² -o output.exe
9. THE ricer Tool SHALL support cross-compilation (generate Windows .exe on Linux, etc.)
10. WHEN a riced executable is run, IT SHALL execute without requiring mc² installation
11. THE riced executable SHALL be standalone and self-contained
12. THE ricer Tool SHALL preserve program semantics during conversion
13. THE ricer Tool SHALL be self-hosting (ricer.e² can be riced to a native executable)
14. WHEN ricer.e² is riced to native, THE resulting ricer executable SHALL produce identical output to ricer.e²

### Requirement 6: OS Independence

**User Story:** As a user, I want the mc² compiler and .e² files to run on any operating system without external dependencies, so that I can use them anywhere.

#### Acceptance Criteria

1. THE Compiler SHALL run on Windows, Linux, and macOS without modification
2. THE Compiler SHALL be implemented in C++ using only standard library features
3. THE Compiler SHALL NOT depend on external DLLs or shared libraries beyond system libraries
4. THE Compiler SHALL use only standard system calls available on all target platforms
5. THE .e² Runtime SHALL provide OS abstraction for platform-specific operations
6. WHEN compiled for a platform, THE Compiler SHALL be a single executable file
7. THE same .e² file SHALL execute on all supported platforms without recompilation

### Requirement 7: Module System and Linking

**User Story:** As a developer, I want to include mc² files like C includes .o files or DLLs, so that I can reuse compiled code efficiently.

#### Acceptance Criteria

1. WHEN a .mc² file is imported, THE Compiler SHALL link it as a compiled module
2. THE Compiler SHALL support selective imports: import *, import $, import %
3. THE Compiler SHALL support both "import" and "get" keywords for importing
4. THE Compiler SHALL support aliased imports: import file.mc² as alias
5. THE Compiler SHALL resolve namespace references across imported modules
6. THE Compiler SHALL prevent circular import dependencies
7. WHEN an imported file is not found, THE Compiler SHALL produce a clear error message

### Requirement 8: Header Files

**User Story:** As a developer, I want header files similar to C++ headers, so that I can declare interfaces separately from implementations.

#### Acceptance Criteria

1. THE Compiler SHALL recognize .e4² header files (alternative: .e4_sup4)
2. THE Compiler SHALL support @include syntax for including headers
3. THE Compiler SHALL process header files before main source compilation
4. THE Compiler SHALL allow forward declarations in header files
5. THE Compiler SHALL validate that header declarations match implementations
6. WHEN a header file is not found, THE Compiler SHALL produce a clear error message

### Requirement 9: Global Variables

**User Story:** As a developer, I want global variables with $ prefix, so that I can share state across scopes.

#### Acceptance Criteria

1. THE Compiler SHALL recognize global variable declarations: $(<name>) :: <type>
2. THE Compiler SHALL recognize global variable initialization: $(<name>) :: <type>.let <value>
3. THE Compiler SHALL make global variables accessible across all scopes in a module
4. THE Compiler SHALL make imported global variables accessible via module.$ syntax
5. WHEN a global variable is redeclared, THE Compiler SHALL produce an error

### Requirement 10: Macro System

**User Story:** As a developer, I want a macro system using !charset, so that I can define custom symbolic operators and code transformations.

#### Acceptance Criteria

1. THE Compiler SHALL recognize macro definitions: !charset :: macro.setmacro(value) << name
2. THE Compiler SHALL expand macros during preprocessing before parsing
3. THE Compiler SHALL support symbolic operator macros via @assign.symboliccharset
4. THE Compiler SHALL allow macros to reference __ANY_ARG__, __ARGS__, _Get_space_, _fragment_possible_args_, __possible_args_, _possible_arg_type_
5. WHEN a macro is undefined, THE Compiler SHALL produce an error at expansion time

### Requirement 11: Symbolic Operators

**User Story:** As a developer, I want to define custom symbolic operators, so that I can create domain-specific notation.

#### Acceptance Criteria

1. THE Compiler SHALL recognize symbolic operator definitions via @assign.symboliccharset
2. THE Compiler SHALL support operator argument patterns: _arg << symbol >> arg_
3. THE Compiler SHALL support multi-argument operators with -- separators
4. THE Compiler SHALL support relative positioning operators: << >> <+ >+ >- <-
5. THE Compiler SHALL resolve symbolic operators to their defined functions
6. WHEN an operator is used but not defined, THE Compiler SHALL produce an error

### Requirement 12: Spacetime Assembly and .e² Generation

**User Story:** As a language implementer, I want the compiler to generate spacetime assembly and convert it to .e² format, so that mc² programs can execute via the runtime.

#### Acceptance Criteria

1. THE Code_Generator SHALL produce spacetime assembly as intermediate representation
2. THE Spacetime_Assembly SHALL use syntax similar to mc² (with :: and << >> operators)
3. THE Spacetime_Assembly SHALL be simpler than traditional assembly while maintaining expressiveness
4. THE Assembler SHALL convert spacetime assembly into .e² binary format
5. THE Assembler SHALL encode instructions as bytecode in the .e² code section
6. THE Assembler SHALL embed symbol tables and type information in the .e² metadata section
7. THE Assembler SHALL embed string literals and constants in the .e² data section
8. THE Code_Generator SHALL optimize generated code for size and execution speed
9. THE Code_Generator SHALL generate correct calling conventions for the mc² runtime
10. WHEN generating .e², THE Assembler SHALL preserve program semantics
11. THE .e² Format SHALL be deterministic (same source produces same binary)

### Requirement 13: Core Assembly Functions

**User Story:** As a language implementer, I want core functions implemented in spacetime assembly, so that the language has a foundation to build upon.

#### Acceptance Criteria

1. THE print Function SHALL be implemented in spacetime assembly
2. THE input Function SHALL be implemented in spacetime assembly for reading user input
3. THE Memory_Manager SHALL be implemented in spacetime assembly
4. THE print Function SHALL support variadic arguments via _possible_args_
5. THE input Function SHALL read from standard input and return a string
6. THE input Function SHALL support optional prompt parameter
7. WHEN print is called, IT SHALL output to standard output
8. WHEN input is called, IT SHALL wait for user input and return the entered value
9. ALL core functions SHALL be tested thoroughly before C++ files are deleted
10. NO functions SHALL be built-in by default - all must be implemented in assembly first

### Requirement 13.5: Standard Library

**User Story:** As a developer, I want a standard library with common functionality built on the assembly foundation, so that I can write practical programs.

#### Acceptance Criteria

1. THE Standard_Library SHALL provide I/O functions built on print and input
2. THE Standard_Library SHALL provide string manipulation functions
3. THE Standard_Library SHALL provide mathematical functions
4. THE Standard_Library SHALL provide data structure implementations
5. THE Standard_Library SHALL be written in mc² using assembly and inline assembly
6. THE Standard_Library SHALL be compilable by the self-hosted compiler
7. WHEN importing standard library modules, THE Compiler SHALL locate them automatically

### Requirement 14: Build System

**User Story:** As a developer, I want an automated build system for mc² projects, so that I can manage multi-file projects and compiler versions easily.

#### Acceptance Criteria

1. THE Build_System SHALL be implemented via build.bat (Windows) or Makefile (Unix/Linux/macOS)
2. THE Build_System SHALL accept command-line arguments via -arg flags (no inline input)
3. THE Build_System SHALL compile multiple .mc² files in dependency order
4. THE Build_System SHALL detect file changes and recompile only what's needed
5. THE Build_System SHALL support build configuration files
6. THE Build_System SHALL link compiled modules into a single .e² executable
7. THE Build_System SHALL store each compiler version separately in a versioned directory structure
8. THE Build_System SHALL support self-hosting builds (compiling the compiler with itself)
9. WHEN a build fails, THE Build_System SHALL report which file caused the failure
10. THE Build_System SHALL automate file generation based on command-line arguments
11. THE Build_System SHALL maintain a "latest" symlink or reference to the newest compiler version

### Requirement 14.5: Compiler Version Management

**User Story:** As a language implementer, I want each compiler version stored separately, so that I can validate self-hosting by compiling each version with the previous one.

#### Acceptance Criteria

1. THE Build_System SHALL store compiler versions in separate directories using 5-part version format with optional prefix (e.g., bin/v-0-0-0-1-0/, bin/alpha-0-0-0-2-0/, bin/beta-0-0-1-0-0/)
2. THE Build_System SHALL support version prefixes: v (stable), alpha (early testing), beta (feature complete), rc (release candidate), dev (development)
3. THE Build_System SHALL maintain version metadata (version number, build date, source commit)
3. THE Build_System SHALL allow building a new compiler version using a previous version
4. THE Build_System SHALL validate that version N can compile version N+1
5. WHEN self-hosting, THE Build_System SHALL verify that the compiled compiler is functionally equivalent to the source compiler
6. THE Build_System SHALL support rollback to previous compiler versions if needed

### Requirement 15: Error Handling and Diagnostics

**User Story:** As a developer, I want clear, readable error messages, so that I can debug my code efficiently.

#### Acceptance Criteria

1. WHEN a syntax error occurs, THE Compiler SHALL report the file, line number, column, and readable error description
2. WHEN a type error occurs, THE Compiler SHALL report expected and actual types in human-readable format
3. WHEN an import fails, THE Compiler SHALL report the missing file path and search locations
4. WHEN a macro expansion fails, THE Compiler SHALL report the macro name, context, and reason for failure
5. THE Compiler SHALL use color-coded output for errors (red), warnings (yellow), and info messages (blue)
6. THE Compiler SHALL provide code snippets showing the error location with context lines
7. THE Compiler SHALL suggest possible fixes for common errors
8. WHEN multiple errors exist, THE Compiler SHALL report them in source order with clear separation

### Requirement 16: Language Extensibility and Scalability

**User Story:** As a language designer, I want mc² to be extensible and scalable, so that new features can be added and large projects can be built efficiently.

#### Acceptance Criteria

1. THE Language SHALL support adding new types without modifying the compiler core
2. THE Language SHALL support adding new operators via the macro system
3. THE Language SHALL support plugin-based code transformations
4. THE Language SHALL maintain backward compatibility for syntax changes
5. WHEN new syntax is added, THE Parser SHALL be updatable via configuration
6. THE Language SHALL scale to projects with 100,000+ lines of code
7. THE Compiler SHALL use efficient data structures for large codebases
8. THE Compiler SHALL support modular compilation for scalability
9. THE Language design SHALL prioritize extensibility through assembly and inline assembly

### Requirement 17: Scalability

**User Story:** As a developer, I want mc² to handle large codebases, so that I can build substantial projects.

#### Acceptance Criteria

1. THE Compiler SHALL compile projects with 10,000+ lines of code efficiently
2. THE Compiler SHALL use incremental compilation to minimize rebuild time
3. THE Compiler SHALL support parallel compilation of independent modules
4. THE Compiler SHALL use memory efficiently during compilation
5. WHEN compiling large projects, THE Compiler SHALL provide progress feedback

### Requirement 18: Grammar Specification and Parser

**User Story:** As a language implementer, I want a formal grammar specification, so that the parser can be implemented correctly and consistently.

#### Acceptance Criteria

1. THE Grammar SHALL be specified in a formal notation (EBNF or similar)
2. THE Grammar SHALL be unambiguous for all valid mc² programs
3. THE Parser SHALL validate input against the grammar specification
4. THE Pretty_Printer SHALL format mc² code according to the grammar
5. FOR ALL valid mc² programs, parsing then pretty-printing then parsing SHALL produce an equivalent AST

### Requirement 19: Compiler Testing and Validation

**User Story:** As a language implementer, I want comprehensive compiler tests, so that I can ensure correctness and catch regressions.

#### Acceptance Criteria

1. THE Test_Suite SHALL include tests for all language features
2. THE Test_Suite SHALL include tests for error conditions
3. THE Test_Suite SHALL include tests for edge cases (empty files, large files, deeply nested structures)
4. THE Test_Suite SHALL validate that compiled programs produce correct output
5. WHEN the compiler is modified, THE Test_Suite SHALL detect breaking changes

### Requirement 20: Documentation Structure

**User Story:** As a developer, I want comprehensive documentation of syntax, operators, and language features, so that I can learn and use mc² effectively.

#### Acceptance Criteria

1. THE Documentation SHALL be organized in a docs/ directory
2. THE Documentation SHALL include complete syntax reference for all language constructs
3. THE Documentation SHALL include operator reference with examples
4. THE Documentation SHALL include macro system documentation with __possible_args_ and _possible_arg_type_ examples
5. THE Documentation SHALL include spacetime assembly instruction reference
6. THE Documentation SHALL include .e² binary format specification
7. THE Documentation SHALL include ricer.e² usage guide
8. THE Documentation SHALL include build system reference (build.bat and Makefile)
9. THE Documentation SHALL include self-hosting guide
10. THE Documentation SHALL include examples for common patterns
11. THE Documentation SHALL NOT include temporary reports or logs
12. WHEN temporary files or reports are generated, THEY SHALL be placed in a garbage/ directory
13. THE garbage/ directory SHALL be excluded from version control (.gitignore)
14. THE Documentation SHALL be written in Markdown format
15. THE Documentation SHALL include a comprehensive index or table of contents

### Requirement 21: Command-Line Interface

**User Story:** As a developer, I want a comprehensive CLI with flags for controlling compiler behavior, so that I can customize compilation settings.

#### Acceptance Criteria

1. THE Compiler SHALL accept -h or --help flags to display usage information
2. THE Compiler SHALL accept -o or --output flags to specify output file path
3. THE Compiler SHALL accept --loose-indent flag to enable loose indentation mode
4. THE Compiler SHALL accept --extra-loose-indent flag to disable indentation validation
5. WHEN --loose-indent is specified, THE Compiler SHALL emit warnings for indentation mismatches
6. WHEN --extra-loose-indent is specified, THE Compiler SHALL NOT validate indentation
7. WHEN both --loose-indent and --extra-loose-indent are specified, THE Compiler SHALL use extra-loose mode
8. THE Compiler SHALL accept -v or --version flags to display version information
9. THE Compiler SHALL accept --verbose flag to enable detailed compilation output
10. THE Compiler SHALL display clear error messages for invalid flag combinations
