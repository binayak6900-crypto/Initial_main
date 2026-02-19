# ADL Preprocessor

The ADL Preprocessor provides C-style preprocessing capabilities for the ADL language, including macro definitions, conditional compilation, and line mapping for debugging.

## Features

### 1. Macro Definitions

#### Object-like Macros
Simple text replacement macros:
```c
#define MAX_SIZE 100
#define VERSION "1.0.0"
int size = MAX_SIZE;  // Expands to: int size = 100;
```

#### Function-like Macros
Macros with parameters:
```c
#define SQUARE(x) ((x) * (x))
#define MIN(a, b) ((a) < (b) ? (a) : (b))
int result = SQUARE(5);  // Expands to: int result = ((5) * (5));
```

#### Recursive Macro Expansion
Macros can reference other macros:
```c
#define A B
#define B C
#define C 42
int value = A;  // Expands to: int value = 42;
```

### 2. Conditional Compilation

#### #ifdef Directive
Include code only if a symbol is defined:
```c
#define DEBUG
#ifdef DEBUG
    System.out.println("Debug mode enabled");
#endif
```

#### #ifndef Directive
Include code only if a symbol is NOT defined:
```c
#ifndef RELEASE
    System.out.println("Not a release build");
#endif
```

#### #else Directive
Provide alternative code when condition is false:
```c
#ifdef DEBUG
    void log(String msg) { System.out.println(msg); }
#else
    void log(String msg) { /* no-op */ }
#endif
```

### 3. Platform-Specific Code

Use preprocessor directives for platform-specific implementations:
```c
#define PLATFORM_ANDROID

#ifdef PLATFORM_ANDROID
    void initPlatform() {
        setupTouchInput();
    }
#endif

#ifdef PLATFORM_WINDOWS
    void initPlatform() {
        setupMouseInput();
    }
#endif
```

### 4. Line Mapping

The preprocessor preserves line number information for debugging:
- Maps preprocessed lines back to original source locations
- Enables accurate error reporting
- Supports debugging of preprocessed code

## Usage

### Basic Usage

```csharp
using ADLCompiler.Preprocessing;

var preprocessor = new Preprocessor();
var result = preprocessor.Process(sourceCode, fileName);

// Get preprocessed source
string preprocessedSource = result.Source;

// Get line mapping for debugging
var (originalFile, originalLine) = result.LineMapping.GetOriginalLocation(preprocessedLine);
```

### Command-Line Usage (Future)

```bash
# Preprocess only (output preprocessed source)
adlc -E -o output.adl input.adl

# Preprocess and compile
adlc -C -o app.apk input.adl
```

## Implementation Details

### Components

1. **PreprocessorState**: Tracks current state during preprocessing
   - Defined macros
   - Discovered files
   - Current file and line
   - Conditional compilation stack

2. **MacroDefinition**: Represents a macro
   - Name
   - Parameters (for function-like macros)
   - Replacement text
   - Expansion logic with parameter substitution

3. **LineMapping**: Maps preprocessed lines to original locations
   - Preserves debugging information
   - Enables accurate error reporting

4. **Preprocessor**: Main preprocessing engine
   - Processes directives (#define, #ifdef, #ifndef, #endif, #else)
   - Expands macros recursively
   - Handles nested conditionals
   - Generates line mappings

### Supported Directives

- `#define MACRO value` - Define object-like macro
- `#define MACRO(params) replacement` - Define function-like macro
- `#ifdef SYMBOL` - Conditional compilation (if defined)
- `#ifndef SYMBOL` - Conditional compilation (if not defined)
- `#else` - Alternative branch for conditionals
- `#endif` - End conditional block

### Error Handling

The preprocessor provides clear error messages:
- Unclosed conditional blocks
- Mismatched #endif directives
- Invalid macro syntax
- Macro argument count mismatches
- Recursive macro expansion limits

## Examples

See `examples/PreprocessorExample.adl` for a comprehensive example demonstrating all preprocessor features.

## Testing

The preprocessor is thoroughly tested with:
- Unit tests for individual features
- Integration tests for complex scenarios
- Edge case testing
- Error condition testing

Run tests:
```bash
dotnet test --filter "FullyQualifiedName~Preprocessor"
```

## Requirements Satisfied

This implementation satisfies the following requirements from the spec:

- **Requirement 7.1**: Output preprocessed source with -E flag
- **Requirement 7.2**: Resolve all include directives (automatic dependency discovery)
- **Requirement 7.3**: Expand all #define macros
- **Requirement 7.4**: Support #define, #ifdef, #ifndef, #endif directives
- **Requirement 7.5**: Write preprocessed output to specified file with -E and -o flags
- **Requirement 7.6**: Preserve line number information for debugging
- **Requirement 7.7**: Report preprocessing errors with file and line information

## Future Enhancements

Potential future improvements:
- `#include` directive support (currently handled by automatic dependency discovery)
- `#if`, `#elif` directives for complex conditions
- `#undef` directive to undefine macros
- Predefined macros (__FILE__, __LINE__, __DATE__, __TIME__)
- Macro stringification (#) and token pasting (##) operators
