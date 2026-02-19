# Version Management Guide

## Overview

The mc² build system automatically manages compiler versions in separate directories with a 5-part versioning scheme.

## Version Format

**Format:** `<prefix>-X-Y-Z-W-V`

**Supported Prefixes:**
- `v` - Stable releases
- `alpha` - Early testing versions
- `beta` - Feature complete, testing phase
- `rc` - Release candidates
- `dev` - Development builds

**Examples:**
- `v-0-0-0-1-0` - Bootstrap compiler (stable)
- `alpha-0-0-0-2-0` - First alpha self-hosted version
- `beta-0-0-1-0-0` - Beta release
- `rc-1-0-0-0-0` - Release candidate
- `v-1-0-0-0-0` - First stable release

## Building New Versions

### Windows (build.bat)

**With version specified:**
```cmd
build.bat -self-host -version v-0-0-0-2-0
```

**Interactive mode (prompts for version):**
```cmd
build.bat -self-host
```

The build system will prompt you for:
1. Version prefix (v/alpha/beta/rc/dev)
2. Major version
3. Minor version
4. Patch version
5. Build version
6. Revision

### Creating Versioned Backups

You can create a versioned backup of the current compiler without rebuilding:

**With version specified:**
```cmd
build.bat -backup v-0-0-0-1-0
```

**Interactive mode (prompts for version):**
```cmd
build.bat -backup
```

This is useful for:
- Preserving the bootstrap compiler before modifications
- Creating snapshots of working compilers
- Archiving specific compiler builds

### Unix/Linux/macOS (Makefile)

**With version specified:**
```bash
make self-host VERSION=v-0-0-0-2-0
```

**Interactive mode (prompts for version):**
```bash
make self-host
```

The build system will prompt you for the same version components.

## Version Directory Structure

Each version is stored in its own directory:

```
bin/
├── v-0-0-0-1-0/              # Bootstrap compiler
│   ├── mc2-v-0-0-0-1-0.exe   # Windows binary
│   ├── mc2-v-0-0-0-1-0       # Unix binary
│   └── VERSION.txt           # Build metadata
├── alpha-0-0-0-2-0/          # First alpha version
│   ├── mc2-alpha-0-0-0-2-0.exe
│   └── VERSION.txt
└── v-1-0-0-0-0/              # First stable release
    ├── mc2-v-1-0-0-0-0.exe
    ├── ricer-v-1-0-0-0-0.exe # Ricer tool
    └── VERSION.txt
```

## Version Metadata

Each version directory contains a `VERSION.txt` file with:
- Version number
- Build date and time
- Build system (Windows/Unix/Linux/macOS)
- Backup source (if created via `-backup` action)

Example (self-hosted build):
```
Version: v-0-0-0-1-0
Build Date: Sun 02/15/2026 21:25:41.54
Build System: Windows
```

Example (backup):
```
Version: v-0-0-0-1-0
Build Date: Mon 02/16/2026 10:30:15.22
Build System: Windows
Backup of: mc2asm.exe
```

## Overwrite Protection

If you try to build a version that already exists, the build system will:
1. Warn you that the version exists
2. Prompt you to confirm overwrite (y/n)
3. Cancel the build if you choose 'n'

## Version Detection

When building a new version, the build system:
1. Detects the last version in the bin/ directory
2. Displays it for reference
3. Shows the current version being built

This helps you track version progression.

## Using a Specific Version

**Windows:**
```cmd
bin\v-0-0-0-1-0\mc2-v-0-0-0-1-0.exe input.spacetime -o output.e²
```

**Unix/Linux/macOS:**
```bash
bin/v-0-0-0-1-0/mc2-v-0-0-0-1-0 input.spacetime -o output.e²
```

## Version Progression

Recommended version progression:
1. `v-0-0-0-1-0` - Bootstrap compiler (C++)
2. `alpha-0-0-0-2-0` - First self-hosted version
3. `alpha-0-0-0-3-0` - Compiled by alpha-0-0-0-2-0
4. `beta-0-0-1-0-0` - Feature complete
5. `rc-1-0-0-0-0` - Release candidate
6. `v-1-0-0-0-0` - First stable release

## Self-Hosting Validation

Each version should be able to compile the next version:
- Bootstrap (C++) compiles alpha-0-0-0-2-0
- alpha-0-0-0-2-0 compiles alpha-0-0-0-3-0
- alpha-0-0-0-3-0 compiles beta-0-0-1-0-0
- And so on...

This ensures each version is functionally correct.
