# Implementation Plan: mc² Programming Language

## Overview

This implementation plan covers building the mc² programming language compiler and toolchain with a critical assembly-first approach. The workflow is:

1. **C++ Bootstrap Assembler**: Build minimal assembler in C++ to process spacetime assembly (.spacetime → .e²)
2. **Core Assembly Functions**: Implement print, input, and memory management in spacetime assembly
3. **Comprehensive Testing**: Test all assembly functions thoroughly
4. **DELETE C++ FILES**: Remove all C++ code - no going back, no dependencies
5. **Self-Hosted Compiler**: Rewrite compiler in mc² using assembly and inline assembly
6. **Build All Features**: Implement all syntax features using the assembly foundation

**Critical Philosophy**: Nothing is built-in. Everything from print() to memory management must be implemented in assembly first. The language provides assembly, and all features are built on top of it.

**IMPORTANT**: The first compiler (C++ bootstrap assembler) serves as the base converter (.spacetime → .e²) until we build a working ricer. The ricer.e² tool will then convert .e² files to native OS executables. Until ricer is complete, all programs run as .e² files with the compiler runtime.

Key features:
- `::` denotes line start (not line end like `;`)
- Auto type detection: `:: x = 1` infers int, creates hash `__var_x_hash__`
- Arbitrary precision: no integer or float limits
- Hash-based variables: avoid collisions with function args
- Indentation modes: strict (default), loose (--loose-indent), extra-loose (--extra-loose-indent)
- Scalable: handles 100,000+ line projects
- File extensions: `.spacetime` (assembly), `.e²` or `.e_sub2` (binary)

## Tasks

- [x] 1. Project Setup and Build System
  - Create project directory structure (src/, mc2/, docs/, tests/, bin/, garbage/)
  - Set up build.bat for Windows with -arg command-line support
  - Set up Makefile for Unix/Linux/macOS with command-line arguments
  - Configure .gitignore to exclude garbage/ and build artifacts
  - Create initial README.md with project overview emphasizing assembly-first approach
  - Document that C++ files will be DELETED after Phase 4
  - Document 5-part version format (v-.-.-.-.-) with versions saved in bin/
  - Document binary naming: <name>-<version>-<optional-prefix>-<optional-TEST>
  - Update a.mc² with additional syntax examples for project structure and build commands
  - _Requirements: 14.1, 14.2, 14.10, 20.12, 20.13_

- [x] 2. Define Spacetime Assembly Format
  - [x] 2.1 Document spacetime assembly syntax based on a.mc²
    - Define instruction format: `:: <opcode> << <operands> >> -> <destination>`
    - Define register naming: %r0, %r1, %r2, etc.
    - Define label syntax: @label_name
    - Define constant syntax: immediate values and $global_name
    - Document all opcodes (load, store, add, sub, mul, div, call, return, syscall, etc.)
    - Document _possible_args_ for variadic functions
    - _Requirements: 12.1, 12.2, 12.3_
  
  - [x] 2.2 Create spacetime assembly examples
    - Write example: print function with variadic arguments
    - Write example: input function with optional prompt
    - Write example: memory allocation for arbitrary precision
    - Write example: function calls and returns
    - Write example: control flow (if, while, for)
    - _Requirements: 12.2_
  
  - [x] 2.3 Update a.mc² with spacetime assembly syntax examples
    - Add assembly instruction format examples
    - Add register usage examples
    - Add label and jump examples

- [ ] 3. Implement C++ Bootstrap Assembler (TEMPORARY - WILL BE DELETED)
  - [x] 3.1 Create minimal assembler in C++
    - Parse spacetime assembly syntax (.spacetime files)
    - Generate .e² binary format
    - Support basic opcodes (load, store, add, mul, call, return, syscall)
    - Support register allocation
    - Support labels and jumps
    - Validate input extension (.spacetime only)
    - Validate output extension (.e² or .e_sub2 only)
    - NOTE: This serves as the base converter until ricer.e² is built
    - NOTE: Give compiler version after name additional prefix after version also avilable like TEST after prefix beta-.-.-.-.-. alpha-.-.-.-.-. 
    - _Requirements: 4.1, 4.2, 4.3_
  
  - [x] 3.2 Implement .e² binary format writer
    - Write magic number "MC²\0"
    - Write version and header information
    - Write code section with bytecode
    - Write data section with constants
    - _Requirements: 4.9, 4.10, 5.5.1-5.5.7_
  
  - [x] 3.3 Implement basic runtime executor
    - Load .e² files
    - Execute bytecode instructions
    - Handle system calls (io_write, io_read, malloc, free)
    - _Requirements: 4.1, 5.5.8-5.5.12_
  
  - [ ]* 3.4 Write unit tests for assembler
    - Test instruction parsing
    - Test .e² generation
    - Test bytecode execution
    - _Requirements: 4.1-4.10_
  
  - [x] 3.5 Update a.mc² with C++ bootstrap assembler examples
    - Add .e² binary format examples
    - Add proper and dynamic args
    - Add bytecode instruction examples

- [x] 4. Checkpoint - Bootstrap Assembler Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Implement Core Functions in Spacetime Assembly
  - [x] 5.1 Implement print function in assembly
    - Ensure it can do hardware I/O functions 
    - Support variadic arguments via _possible_args_
    - Handle multiple types (int, float, str)
    - Output to standard output via syscall
    - Example: print << "Value:", 42, "is big" >>
    - _Requirements: 13.1, 13.4_
  
  - [x] 5.2 Implement input function in assembly
    - Accept optional prompt parameter
    - Read from standard input via syscall
    - Return string value
    - Example: :: name = input << "Enter name: " >>
    - _Requirements: 13.2, 13.5, 13.6_
  
  - [x] 5.3 Implement memory management in assembly
    - Implement malloc for arbitrary precision numbers
    - Implement free for memory deallocation
    - Implement BigInt structure and operations
    - Implement BigFloat structure and operations
    - Implement automatic promotion from native types
    - Implement garbage collection
    - _Requirements: 2.5.1-2.5.7, 2.2, 2.3, 2.4_
  
  - [ ]* 5.4 Write comprehensive tests for core functions
    - Test print with various argument types
    - Test input with and without prompt
    - Test memory allocation and deallocation
    - Test arbitrary precision arithmetic
    - Test automatic type promotion
    - Test garbage collection
    - _Requirements: 13.9_
  
  - [x] 5.5 Update a.mc² with core assembly function examples
    - Add print function usage examples
    - Add input function usage examples
    - Add memory management examples
    - Add arbitrary precision number examples

- [x] 6. CRITICAL CHECKPOINT - Test Assembly Thoroughly
  - Run ALL assembly tests multiple times
  - Verify print works correctly with all types
  - Verify input reads correctly
  - Verify memory management handles large numbers
  - Verify no memory leaks
  - Verify all syscalls work on target platforms (Windows, Linux, macOS)
  - Document any issues found
  - **DO NOT PROCEED until all tests pass consistently**
  - Update a.mc² with validated assembly patterns and best practices
  - Archive pwd in a 7z shell
  - _Requirements: 13.9_

- [x] 7. ⚠️ DELETE ALL C++ FILES ⚠️
  - [x] 7.1 Verify assembly foundation is complete
    - Confirm all core functions work
    - Confirm all tests pass
    - Confirm .e² files execute correctly
    - _Requirements: 4.6, 4.7_
  
  - [x] 7.2 Delete C++ source files
    - Remove src/ directory with all C++ code
    - Remove C++ build artifacts
    - Keep only .e² binaries and assembly source
    - Update build system to use .e² assembler
    - _Requirements: 4.6, 4.7, 5.10_
  
  - [x] 7.3 Document the deletion
    - Record what was deleted
    - Record current state of assembly foundation
    - Update README.md to reflect assembly-only development
    - _Requirements: 4.6_
  
  - [x] 7.4 Update a.mc² to mark C++ deletion milestone
    - Add note about assembly-only development phase
    - Document transition to self-hosted compiler

- [x] 8. Checkpoint - C++ Deleted, Assembly-Only Development Begins
  - Ensure assembly foundation still works after C++ deletion
  - Verify .e² assembler can be used for further development
  - Update a.mc² with post-deletion development notes
  - Ask the user if questions arise.

- [ ] 9. Implement Lexer in mc² (using assembly)
  - [x] 9.1 Create Token types and structures in mc²
    - Define token types using mc² type system
    - Implement Token structure with type, lexeme, line, column
    - Use assembly for low-level string operations
    - _Requirements: 5.1, 5.2_
  
  - [x] 9.2 Implement tokenization in mc²
    - Recognize :: as line start marker
    - Recognize << >> for parameters
    - Recognize all operators and keywords
    - Create hash-based variable names internally
    - Use inline assembly for performance-critical operations
    - _Requirements: 5.1, 5.2, 1.1-1.25_
  
  - [ ] 9.3 Implement comment stripping
    - Handle -(content) comments
    - Handle -{...} multi-line comments
    - _Requirements: 1.14, 1.15_
  
  - [ ]* 9.4 Write property test for lexer
    - **Property 6: Comment Elimination**
    - **Validates: Requirements 1.14, 1.15**
  
  - [ ] 9.5 Update a.mc² with lexer examples
    - Add tokenization examples
    - Add comment syntax examples
    - Add hash-based variable naming examples

- [ ] 10. Implement Parser in mc² (using assembly)
  - [ ] 10.1 Create AST node structures in mc²
    - Define all AST node types
    - Use mc² type system for node definitions
    - Implement tree construction using assembly
    - _Requirements: 5.1, 5.2_
  
  - [ ] 10.2 Implement recursive descent parser
    - Parse type definitions
    - Parse namespaces and functions
    - Parse expressions with operator precedence
    - Parse control flow statements
    - Use inline assembly for performance
    - _Requirements: 5.1, 5.2, 1.1-1.25_
  
  - [ ] 10.3 Implement indentation validation
    - Track block depth
    - Validate indentation in strict/loose/extra-loose modes
    - Generate errors or warnings based on mode
    - _Requirements: 1.26-1.30, 1.5.1-1.5.17_
  
  - [ ]* 10.4 Write property test for parse-print round trip
    - **Property 1: Parse-Print Round Trip**
    - **Validates: Requirements 18.5**
  
  - [ ] 10.5 Update a.mc² with parser examples
    - Add AST structure examples
    - Add parsing examples for all syntax elements
    - Add indentation validation examples

- [ ] 11. Implement Type System in mc² (using assembly)
  - [ ] 11.1 Create type structures
    - Define built-in types (int, float, str, void, Null)
    - Support arbitrary precision int and float
    - Implement custom type definitions
    - _Requirements: 2.1-2.10, 5.1, 5.2_
  
  - [ ] 11.2 Implement type inference
    - Infer types from initialization values
    - Create hash-based variable names
    - Example: :: x = 1 creates __var_x_hash__ with type int
    - _Requirements: 2.6, 2.10_
  
  - [ ] 11.3 Implement auto type casting
    - Cast between compatible types
    - Automatic int -> float promotion
    - _Requirements: 2.7_
  
  - [ ]* 11.4 Write property test for type inference
    - **Property 10: Type Inference Correctness**
    - **Validates: Requirements 2.6**
  
  - [ ] 11.5 Update a.mc² with type system examples
    - Add type inference examples
    - Add arbitrary precision type examples
    - Add custom type definition examples
    - Add auto casting examples

- [ ] 12. Implement Code Generator in mc² (using assembly)
  - [ ] 12.1 Generate spacetime assembly from AST
    - Convert AST nodes to assembly instructions
    - Allocate virtual registers
    - Generate labels for control flow
    - Use inline assembly for code generation
    - _Requirements: 5.1, 5.2, 12.1-12.3_
  
  - [ ] 12.2 Implement expression code generation
    - Generate code for arithmetic operations
    - Handle arbitrary precision numbers
    - Generate function calls
    - _Requirements: 12.1-12.3_
  
  - [ ]* 12.3 Write property test for semantic preservation
    - **Property 14: Semantic Preservation**
    - **Validates: Requirements 12.7**
  
  - [ ] 12.4 Update a.mc² with code generation examples
    - Add spacetime assembly generation examples
    - Add register allocation examples
    - Add expression code generation examples

- [ ] 13. Implement Assembler in mc² (using assembly)
  - [ ] 13.1 Implement .e² binary writer
    - Write magic number, version, headers
    - Encode instructions as bytecode
    - Write metadata and symbol tables
    - Use inline assembly for binary operations
    - _Requirements: 5.1, 5.2, 12.4-12.7_
  
  - [ ]* 13.2 Write property test for .e² determinism
    - **Property: Same source produces same binary**
    - **Validates: Requirements 12.11**
  
  - [ ] 13.3 Update a.mc² with assembler examples
    - Add .e² binary format examples
    - Add bytecode encoding examples
    - Add metadata structure examples

- [ ] 14. Implement Preprocessor in mc² (using assembly)
  - [ ] 14.1 Implement macro expansion
    - Parse !charset macros
    - Expand __possible_args_ and _possible_arg_type_
    - Support variadic macros
    - _Requirements: 5.1, 5.2, 10.1-10.4_
  
  - [ ] 14.2 Implement @include processing
    - Resolve header file paths
    - Insert header content
    - _Requirements: 8.2, 8.3_
  
  - [ ]* 14.3 Write property test for macro expansion
    - **Property 28: Macro Expansion**
    - **Validates: Requirements 10.2**
  
  - [ ] 14.4 Update a.mc² with preprocessor examples
    - Add !charset macro examples
    - Add @include examples
    - Add __possible_args_ usage examples
    - Add _possible_arg_type_ examples

- [ ] 15. Implement CLI in mc² (using assembly)
  - [ ] 15.1 Parse command-line arguments
    - Support -h, --help, -o, --output, -v, --version
    - Support --loose-indent, --extra-loose-indent
    - Support --verbose, --indent-size
    - _Requirements: 21.1-21.10_
  
  - [ ]* 15.2 Write unit tests for CLI
    - Test all flag combinations
    - _Requirements: 21.1-21.10_
  
  - [ ] 15.3 Update a.mc² with CLI examples
    - Add command-line argument parsing examples
    - Add flag handling examples

- [ ] 16. Wire All Components Together
  - [ ] 16.1 Create main compiler entry point
    - Integrate lexer, parser, type checker, code generator, assembler
    - Handle errors and diagnostics
    - Produce .e² output files
    - _Requirements: 5.1, 5.2, 5.6, 5.7_
  
  - [ ] 16.2 Implement error reporting
    - Format errors with file, line, column
    - Include code snippets
    - Use color-coded output
    - _Requirements: 15.1-15.8_
  
  - [ ] 16.3 Update a.mc² with compiler integration examples
    - Add full compilation pipeline examples
    - Add error reporting examples
    - Add diagnostic output examples

- [ ] 17. Checkpoint - Self-Hosted Compiler Complete
  - Ensure all tests pass, ask the user if questions arise.
  - Update a.mc² with self-hosted compiler milestone notes

- [ ] 18. Implement ricer.e² Tool in mc²
  - [ ] 18.1 Implement .e² to native converter
    - Load .e² files
    - Generate MZ/PE for Windows
    - Generate ELF for Linux
    - Generate Mach-O for macOS
    - NOTE: Until ricer is complete, the C++ bootstrap assembler serves as base converter
    - After ricer is built, it becomes the primary way to create standalone executables
    - _Requirements: 5.6.1-5.6.6_
  
  - [ ] 18.2 Embed runtime in native executables
    - Include runtime code
    - Make standalone executables
    - _Requirements: 5.6.7, 5.6.10, 5.6.11_
  
  - [ ]* 18.3 Write integration test for ricer
    - Test .e² to native conversion
    - _Requirements: 5.6.12_
  
  - [ ] 18.4 Update a.mc² with ricer.e² examples
    - Add .e² to native conversion examples
    - Add platform-specific executable format examples
    - Add runtime embedding examples

- [ ] 19. Self-Hosting Validation
  - [ ] 19.1 Compile compiler with itself (Stage 1)
    - Use compiler.e² to compile mc² compiler source
    - Produce compiler-v2.e²
    - Store in bin/v-0-0-0-2-0/
    - _Requirements: 5.5, 14.5.3_
  
  - [ ] 19.2 Compile compiler with itself (Stage 2)
    - Use compiler-v2.e² to compile mc² compiler source
    - Produce compiler-v3.e²
    - Store in bin/v-0-0-0-3-0/
    - _Requirements: 5.5, 14.5.3_
  
  - [ ] 19.3 Validate equivalence
    - Compare compiler-v2.e² and compiler-v3.e²
    - Verify functional equivalence
    - _Requirements: 5.4, 5.5_
  
  - [ ] 19.4 Update a.mc² with self-hosting validation examples
    - Add multi-stage compilation examples
    - Add compiler version comparison examples
    - Add equivalence validation examples

- [ ] 20. Write Standard Library in mc²
  - [ ] 20.1 Implement I/O functions
    - Build on print and input from assembly
    - Add file I/O functions
    - _Requirements: 13.5.1_
  
  - [ ] 20.2 Implement string functions
    - String manipulation, search, comparison
    - _Requirements: 13.5.2_
  
  - [ ] 20.3 Implement math functions
    - Basic math, trigonometry
    - Use arbitrary precision numbers
    - _Requirements: 13.5.3_
  
  - [ ] 20.4 Implement data structures
    - Arrays, lists, maps
    - _Requirements: 13.5.4_
  
  - [ ]* 20.5 Write unit tests for standard library
    - Test all functions
    - _Requirements: 13.5.1-13.5.4_
  
  - [ ] 20.6 Update a.mc² with standard library examples
    - Add I/O function examples
    - Add string function examples
    - Add math function examples
    - Add data structure examples

- [ ] 21. Write Comprehensive Documentation
  - [ ] 21.1 Write syntax reference (docs/syntax-reference.md)
    - Document all language constructs
    - Document :: as line start
    - Document hash-based variables
    - Document arbitrary precision numbers
    - Include examples
    - _Requirements: 20.2_
  
  - [ ] 21.2 Write operator reference (docs/operator-reference.md)
    - Document all operators
    - Document symbolic operator definitions
    - _Requirements: 20.3_
  
  - [ ] 21.3 Write macro system documentation (docs/macro-system.md)
    - Document !charset macros
    - Document __possible_args_ and _possible_arg_type_
    - Include variadic function examples
    - _Requirements: 20.4_
  
  - [ ] 21.4 Write spacetime assembly reference (docs/spacetime-assembly.md)
    - Document instruction format
    - Document all opcodes
    - Include examples
    - _Requirements: 20.5_
  
  - [ ] 21.5 Write .e² format specification (docs/e2-format.md)
    - Document binary format structure
    - _Requirements: 20.6_
  
  - [ ] 21.6 Write ricer usage guide (docs/ricer-guide.md)
    - Document ricer CLI
    - Document supported formats
    - _Requirements: 20.7_
  
  - [ ] 21.7 Write build system reference (docs/build-system.md)
    - Document build.bat and Makefile
    - Document version management
    - _Requirements: 20.8_
  
  - [ ] 21.8 Write self-hosting guide (docs/self-hosting.md)
    - Document bootstrap process
    - Document C++ deletion step
    - Document assembly-first approach
    - _Requirements: 20.9_
  
  - [ ] 21.9 Write example programs (docs/examples/)
    - Hello world
    - Functions and control flow
    - Macros and symbolic operators
    - Variadic functions
    - Arbitrary precision arithmetic
    - _Requirements: 20.10_
  
  - [ ] 21.10 Create documentation index (docs/README.md)
    - Create table of contents
    - Link all documentation
    - _Requirements: 20.11, 20.14, 20.15_
  
  - [ ] 21.11 Update a.mc² with comprehensive language reference
    - Add complete syntax reference
    - Add all documented features
    - Add cross-references to documentation

- [ ] 22. Final Checkpoint - Complete System
  - Ensure all tests pass
  - Verify self-hosting works
  - Verify documentation is complete
  - Update a.mc² with final complete language specification
  - Ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties
- Unit tests validate specific examples and edge cases
- **CRITICAL**: C++ files MUST be deleted after Phase 4 (Task 7)
- All features after C++ deletion must be built using assembly and inline assembly
