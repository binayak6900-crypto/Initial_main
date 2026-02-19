# mc² Programming Language

> "As absurd as physics" - A self-hosted, OS-independent programming language with unconventional syntax

## Overview

mc² (pronounced "em-see-squared") is a programming language that embraces unconventional syntax inspired by physics notation while maintaining practical utility. The language features arbitrary precision numbers (no integer or float limits), built-in memory management, symbolic operators, and a powerful macro system.

**File Extensions**: `.mc²` or `.mc_sup2` (source), `.e²` or `.e_sub2` (compiled)

## Critical Design Philosophy: Assembly-First

**Nothing is built-in by default.** Everything—including basic functions like `print()` and `input()`—must be built from assembly first. The language provides the assembly foundation, and all higher-level features are constructed on top of it.

### Development Phases

1. **Phase 1-3: Bootstrap Foundation (C++ - TEMPORARY)**
   - Build C++ bootstrap assembler that processes spacetime assembly
   - Implement core functions in spacetime assembly (print, input, memory management)
   - Test assembly thoroughly with comprehensive test suite

2. **Phase 4: ⚠️ DELETE ALL C++ FILES ⚠️**
   - Once assembly foundation is complete and tested, **ALL C++ files will be DELETED**
   - No going back, no dependencies on C++
   - Only .e² binaries and assembly source remain

3. **Phase 5+: Self-Hosted Development (Pure mc² + Assembly)**
   - Rewrite compiler in .mc² using assembly and inline assembly
   - Build all syntax features using the assembly foundation
   - Prove language completeness through self-hosting

This approach ensures that mc² is truly self-contained and demonstrates that the language can build itself from the ground up.

## Key Features

### Syntax Highlights

- **`::` denotes line start** (similar to `;` but for line beginning, not end)
- **Auto type detection**: `:: x = 1` infers int and creates internal hash `__var_x_hash__`
- **Arbitrary precision**: No limits on integer or float size
- **Hash-based variables**: Internal names avoid collisions with function arguments
- **Symbolic operators**: Define custom operators via `@assign.symboliccharset`
- **Macro system**: `!charset` macros with variadic support
- **Indentation modes**: strict (default), loose (`--loose-indent`), extra-loose (`--extra-loose-indent`)

### Example Code

```mc²
:: namespace :: math {
    :: func add << x:int, y:int >> -> int {
        :: result = x + y
        :: return result
    }
    
    :: func print_sum << a:int, b:int >> -> void {
        :: sum = add << a, b >>
        :: print << "Sum:", sum >>
    }
}

:: func main << >> -> int {
    :: math.print_sum << 42, 58 >>
    :: return 0
}
```

## Compilation Model

mc² uses a unique two-stage execution model:

### 1. Runtime Execution (OS-Independent)
```bash
mc2 program.e²
```
- The `.e²` file is a universal compiled format (NOT a native executable)
- Same `.e²` file runs on Windows, Linux, and macOS without modification
- mc² runtime loads and executes the bytecode

### 2. Native Conversion (Standalone Executables)
```bash
ricer.e² program.e² -o program.exe
```
- `ricer.e²` tool converts `.e²` files to native OS executables
- Generates MZ/PE (Windows), ELF (Linux), Mach-O (macOS), EFI, and more
- Embeds mc² runtime into the native executable
- Resulting executable is standalone (no mc² installation required)

## Project Structure

```
mc²/
├── src/              # C++ bootstrap assembler (WILL BE DELETED after Phase 4)
├── mc2/              # mc² compiler source (written in mc²)
├── docs/             # Documentation
├── tests/            # Test suite
├── bin/              # Versioned compiler builds (v-.-.-.-.- format)
│   ├── v-0-0-0-1-0/ # Bootstrap compiler
│   ├── v-0-0-0-2-0/ # First self-hosted version
│   └── latest/      # Symlink to newest version
├── garbage/          # Temporary files (gitignored)
├── build.bat         # Windows build system
├── Makefile          # Unix/Linux/macOS build system
└── README.md         # This file
```

## Build System

### Windows (build.bat)
```cmd
REM Compile a source file
build.bat -compile src\main.mc² -output bin\program.e²

REM Self-hosting build
build.bat -self-host -version v1.0.0

REM Create versioned backup
build.bat -backup v-0-0-0-1-0

REM With options
build.bat -compile test.mc² -o test.e² -verbose -loose-indent
```

### Unix/Linux/macOS (Makefile)
```bash
# Compile a source file
make compile SRC=src/main.mc² OUT=bin/program.e²

# Self-hosting build
make self-host VERSION=v1.0.0

# With options
make compile SRC=test.mc² OUT=test.e² VERBOSE=1 INDENT_MODE=loose

# Clean build artifacts
make clean
```

## Command-Line Options

- `-compile <file>` - Compile a .mc² source file
- `-output, -o <file>` - Specify output file path
- `-self-host` - Build compiler using previous version
- `-backup <version>` - Create versioned backup of current compiler
- `-version <ver>` - Specify compiler version for self-hosting
- `-verbose` - Enable detailed compilation output
- `-loose-indent` - Enable loose indentation mode (warnings instead of errors)
- `-extra-loose-indent` - Disable indentation validation entirely
- `-indent-size <n>` - Set indentation size (default: 4)
- `-help, -h` - Show help message

## Indentation Modes

mc² validates indentation alongside brace structure to be "unreasonably logical":

1. **Strict Mode (default)**: Indentation mismatches produce errors
2. **Loose Mode (`--loose-indent`)**: Indentation mismatches produce warnings
3. **Extra-Loose Mode (`--extra-loose-indent`)**: No indentation validation

## Arbitrary Precision Numbers

mc² has **no limits** on integer or float size:

```mc²
:: huge = 123456789012345678901234567890
:: precise = 3.141592653589793238462643383279502884197
:: result = huge ** precise  -(exponentiation with arbitrary precision)
```

The memory manager automatically allocates space for large numbers and performs garbage collection.

## Spacetime Assembly

mc² uses a custom assembly format called "spacetime assembly" with mc²-like syntax:

```
:: func print << _possible_args_ >> -> void {
    :: syscall << io_write >> << %r0 >>
    :: return
}

:: func add << x:int, y:int >> -> int {
    :: load.const << x >> -> %r0
    :: load.const << y >> -> %r1
    :: add << %r0, %r1 >> -> %r2
    :: return << %r2 >>
}
```

## Current Status

✅ **Phase 4 COMPLETE: C++ FILES DELETED - ASSEMBLY-ONLY DEVELOPMENT** ✅

**CRITICAL MILESTONE REACHED**: All C++ source files have been deleted (Task 7). This is the **point of no return**.

### What Was Deleted
- All C++ source files (src/*.cpp, src/*.hpp)
- All C++ build artifacts (build/*.o)
- Total: ~1,780 lines of C++ code

### What Remains
- ✅ Compiled bootstrap assembler (mc2asm.exe)
- ✅ All spacetime assembly files (mc2/core/*.spacetime)
- ✅ All test files and results
- ✅ All documentation

### Assembly Foundation Status
✅ **print()** - Variadic output with hardware I/O (127 lines, fully implemented)
✅ **input()** - User input with optional prompt  
✅ **Memory Manager** - Arbitrary precision integers and floats
✅ **All tests passing** - 9/9 tests pass consistently (Checkpoint 6)

### Next Steps
- Task 8: Verify assembly foundation after deletion
- Tasks 9-17: Build self-hosted compiler in mc² using assembly
- Tasks 18+: Implement ricer.e², standard library, documentation

**Development Model**: All future code must be written in spacetime assembly or mc² (using assembly). The C++ scaffolding is gone. The real foundation—assembly—remains.

## License

[To be determined]

## Contributing

[To be determined]

---

**Remember**: This language is built from assembly up. Nothing is magic, nothing is built-in. Everything you see is constructed from the foundation.
