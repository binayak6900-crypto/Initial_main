#!/bin/bash
# Bootstrap Compiler Build Script
# Builds the xit bootstrap compiler using nasm and gcc for Windows

set -e  # Exit on any error

echo "Building xit bootstrap compiler for Windows..."

# Create build directory
mkdir -p build

# Assemble source files for Windows
echo "Assembling source files..."
nasm -f win64 -o build/main.o src/main.asm
nasm -f win64 -o build/lexer.o src/lexer.asm
nasm -f win64 -o build/parser.o src/parser.asm
nasm -f win64 -o build/memory.o src/memory.asm

# Link object files using gcc (which handles Windows linking properly)
echo "Linking object files..."
gcc -o build/xit-bootstrap.exe build/main.o build/lexer.o build/parser.o build/memory.o

echo "Bootstrap compiler built successfully: build/xit-bootstrap.exe"

echo "Build complete!"