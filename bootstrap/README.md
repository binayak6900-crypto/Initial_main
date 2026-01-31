# Xit Bootstrap Compiler

This directory contains the bootstrap implementation of the xit programming language compiler, written entirely in x86-64 assembly language.

## Directory Structure

```
bootstrap/
├── src/                    # Assembly source files
│   ├── main.asm           # Main entry point
│   ├── lexer.asm          # Lexical analyzer
│   ├── parser.asm         # Parser with direct code generation
│   └── memory.asm         # Memory management system
├── build/                 # Build artifacts (created during build)
├── Makefile              # Build configuration
├── build.sh              # Build script
├── clean.sh              # Clean script
└── README.md             # This file
```

## Prerequisites

The bootstrap compiler requires the following tools:

- **nasm** - The Netwide Assembler for x86-64 assembly
- **gcc** - GNU Compiler Collection (for linking on Windows)
- **make** - GNU Make (optional, can use build.sh instead)

### Installing Prerequisites on Windows:
- Install MSYS2 or MinGW-w64 to get gcc and make
- Install NASM from https://www.nasm.us/
- Or use a package manager like Chocolatey:
```bash
choco install nasm mingw make
```

### Installing Prerequisites on Ubuntu/Debian:
```bash
sudo apt-get install nasm gcc make
```

## Building

### Using Make (recommended):
```bash
make all
```

### Using build script:
```bash
chmod +x build.sh
./build.sh
```

### Manual build:
```bash
mkdir -p build
nasm -f win64 -o build/main.o src/main.asm
nasm -f win64 -o build/lexer.o src/lexer.asm
nasm -f win64 -o build/parser.o src/parser.asm
nasm -f win64 -o build/memory.o src/memory.asm
gcc -o build/xit-bootstrap.exe build/main.o build/lexer.o build/parser.o build/memory.o
```

## Usage

Once built, the bootstrap compiler can be used as follows:

```bash
./build/xit-bootstrap.exe <source_file.xit> <output_executable>
```

Example:
```bash
./build/xit-bootstrap.exe tests/simple.xit output.exe
```

## Development Notes

- This is the initial bootstrap implementation with minimal functionality
- The lexer, parser, and code generator are implemented directly in assembly
- Memory management includes safety features to prevent common vulnerabilities
- Direct binary generation eliminates the need for intermediate representations
- The compiler generates native x86-64 machine code

## Build Targets

- `make all` - Build the bootstrap compiler (default)
- `make clean` - Remove build artifacts
- `make debug` - Build with debug symbols
- `make test` - Run compiler tests (when implemented)
- `make install` - Install to system path
- `make help` - Show available targets

## Next Steps

1. Implement core lexer functionality in `src/lexer.asm`
2. Implement direct code generation parser in `src/parser.asm`
3. Complete memory safety system in `src/memory.asm`
4. Add file I/O and system interface functions
5. Test with minimal xit programs

This bootstrap compiler will be used to compile the self-hosting version of xit written in xit itself.