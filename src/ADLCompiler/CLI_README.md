# ADL Compiler Command-Line Interface

## Overview

The ADL Compiler provides a zero-configuration command-line interface for compiling ADL source code to executable format. The compiler automatically handles dependency discovery, memory management, and cross-platform code generation.

## Features

- **Zero Configuration**: No config files needed - just specify the source file
- **Cross-Platform**: Compile to Android APK, Windows EXE, Linux, and macOS from any platform
- **Automatic Defaults**: Latest SDK, optimal optimization, current device targeting
- **Flexible Packaging**: Choose between installer (shared runtime) or standalone (no dependencies) modes
- **Simple Usage**: `adlc Main.adl` - that's it!

## Command-Line Options

### Compilation Modes

- `-C` - Compile to executable (default if no mode specified)
- `-E` - Preprocess only - output preprocessed source

### Output Control

- `-o <file>` - Output file path (auto-generated if not specified)

### Platform Selection

- `--platform <platform>` - Target platform (default: android)
  - `android` - Android APK
  - `windows` - Windows .exe
  - `linux` - Linux native binary
  - `macos` - macOS .app bundle
  - `all` - Generate for all platforms

### APK Packaging

- `--package-mode <mode>` - APK packaging mode (default: standalone)
  - `installer` - Installs AVM runtime, compiler, and IDE
  - `standalone` - No external dependencies

### SDK Configuration

- `--target-sdk <version>` - Target Android SDK API level (default: 35)
- `--min-sdk <version>` - Minimum Android SDK API level (default: 23)

### Optimization

- `-O0` - No optimization
- `-O1` - Basic optimization
- `-O2` - Standard optimization (default)
- `-O3` - Aggressive optimization

### Information

- `-h, --help` - Show help message
- `-v, --version` - Show version information

## Usage Examples

### Simple Compilation

```bash
# Just specify the file - compiler handles everything!
adlc Main.adl
```

### Compile to Specific Output

```bash
adlc -C -o MyApp.apk Main.adl
```

### Cross-Platform Compilation

```bash
# Compile to Windows from Android
adlc --platform windows -o MyApp.exe Main.adl

# Compile for all platforms at once
adlc --platform all Main.adl
```

### APK Packaging Modes

```bash
# Create installer APK (includes runtime)
adlc --package-mode installer -o MyApp.apk Main.adl

# Create standalone APK (no dependencies)
adlc --package-mode standalone -o MyApp.apk Main.adl
```

### SDK Configuration

```bash
# Target specific SDK version
adlc --target-sdk 33 --min-sdk 28 Main.adl
```

### Optimization Levels

```bash
# Debug build (no optimization)
adlc -O0 Main.adl

# Release build (aggressive optimization)
adlc -O3 Main.adl
```

### Preprocessing

```bash
# Output preprocessed source for debugging
adlc -E -o Main.i Main.adl
```

## Zero Configuration Defaults

When options are not specified, the compiler uses sensible defaults:

- **Platform**: Android
- **Target SDK**: 35 (Android 15.0)
- **Min SDK**: 23 (Android 6.0)
- **Package Mode**: Standalone
- **Optimization**: -O2 (standard)
- **Output Path**: Auto-generated based on platform
  - Android: `<source>.apk`
  - Windows: `<source>.exe`
  - Linux: `<source>`
  - macOS: `<source>.app`

## Implementation Details

### Architecture

The CLI is implemented in three main components:

1. **CommandLineOptions** - Data structure for parsed options
2. **CommandLineParser** - Parses and validates command-line arguments
3. **CompilerDriver** - Orchestrates the compilation process

### Compilation Pipeline

1. **Preprocessing** - Resolve includes and expand macros
2. **Lexical Analysis** - Tokenize source code
3. **Parsing** - Build Abstract Syntax Tree (AST)
4. **Semantic Analysis** - Type checking, dependency resolution, memory management
5. **Code Generation** - Generate bytecode and native code
6. **Output Generation** - Package into target format

### Error Handling

The CLI provides friendly error messages with suggestions:

- Unknown options show available options
- Missing required arguments show what's needed
- Invalid values show valid ranges
- File not found errors show helpful suggestions

## Testing

The CLI is thoroughly tested with unit tests covering:

- Flag parsing (all options)
- Default value application
- Error handling
- Output path generation
- Platform selection
- SDK version validation

Run tests with:

```bash
dotnet test --filter "FullyQualifiedName~CommandLineParserTests"
```

## Future Enhancements

The current implementation provides the CLI infrastructure. Future work will integrate:

- Full compilation pipeline (currently stub implementation)
- Incremental compilation
- Build caching
- Parallel compilation
- Progress reporting
- Detailed error messages with source context
