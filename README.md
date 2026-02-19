# Android Development Ecosystem

A comprehensive Android development environment built in C# that runs entirely on Android devices (with Windows support for the VM component).

## Project Structure

```
AndroidDevEcosystem/
├── AndroidDevEcosystem.sln          # Main solution file
├── src/
│   ├── SharedUtilities/             # Shared utilities and data models
│   │   ├── Models/                  # Core data structures
│   │   │   ├── Project.cs           # Project representation
│   │   │   ├── ADLFile.cs           # ADL source file
│   │   │   ├── BuildConfiguration.cs # Build settings
│   │   │   ├── Token.cs             # Lexical token
│   │   │   └── ASTNode.cs           # Abstract Syntax Tree nodes
│   │   └── FileSystem/              # File system abstraction
│   │       ├── IFileSystemManager.cs
│   │       └── FileSystemManager.cs # Cross-platform file operations
│   ├── AndroidIDE/                  # Android IDE application
│   ├── ADLCompiler/                 # ADL language compiler
│   ├── AVM/                         # Android Virtual Machine
│   └── Shell/                       # Shell system
└── README.md
```

## Components

### 1. SharedUtilities
Core data models and utilities shared across all components:
- **Project**: Represents an ADL project with files and configuration
- **ADLFile**: Represents a single ADL source code file
- **BuildConfiguration**: Build settings and compiler flags
- **Token**: Lexical token for syntax analysis
- **ASTNode**: Abstract Syntax Tree node hierarchy
- **FileSystemManager**: Cross-platform file operations with home directory resolution

### 2. AndroidIDE
Full-featured mobile IDE with:
- Syntax highlighting
- Code suggestions
- Project management
- Build integration

### 3. ADLCompiler
Zero-dependency compiler that:
- Compiles ADL to executable format
- Supports preprocessing
- Provides friendly error messages
- Handles automatic dependency resolution

### 4. AVM (Android Virtual Machine)
Custom APK parser and execution environment for:
- Android devices
- Windows desktop systems

### 5. Shell
Integrated and standalone terminal with:
- Linux commands
- Git integration
- Package management

## Home Directory Resolution

The system uses `/storage/emulated/0/root/` as the root directory on Android devices. The `~` symbol is automatically resolved to this path.

On development machines (Windows/Linux/macOS), the root directory is `~/.android-dev-ecosystem/root/`.

## Building

```bash
# Build all projects
dotnet build

# Build specific project
dotnet build src/AndroidIDE/AndroidIDE.csproj

# Run a project
dotnet run --project src/AndroidIDE/AndroidIDE.csproj
```

## Requirements

- .NET 8.0 SDK or later
- C# 12.0 or later

## License

TBD
