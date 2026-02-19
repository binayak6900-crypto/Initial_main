# mc² Compiler Source (Self-Hosted)

This directory contains the mc² compiler written in mc² itself, using assembly and inline assembly.

## Purpose

After the C++ bootstrap phase is complete and deleted, this directory will contain:
- Lexer (tokenization)
- Parser (AST generation)
- Type checker (inference, validation)
- Code generator (spacetime assembly)
- Assembler (.e² binary generation)
- Preprocessor (macros, includes)
- Standard library

## Implementation Approach

All components are written in mc² using:
- **Spacetime assembly** for low-level operations
- **Inline assembly** for performance-critical code
- **mc² syntax** for high-level logic

## Files (To Be Implemented)

- `lexer.mc²` - Tokenization
- `parser.mc²` - AST generation
- `typechecker.mc²` - Type inference and validation
- `codegen.mc²` - Spacetime assembly generation
- `assembler.mc²` - .e² binary generation
- `preprocessor.mc²` - Macro expansion and includes
- `main.mc²` - Compiler entry point
- `stdlib/` - Standard library modules

## Build

The mc² compiler is compiled using the previous compiler version, demonstrating self-hosting capability.
