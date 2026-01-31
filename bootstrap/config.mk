# Configuration file for bootstrap compiler build
# This file contains build configuration that can be customized

# Target architecture (currently only x86-64 supported)
ARCH = x86_64

# Target platform (linux, windows, macos)
PLATFORM = linux

# Assembly format for target platform
ifeq ($(PLATFORM),linux)
    ASMFORMAT = elf64
    EXECUTABLE_EXT = 
endif

ifeq ($(PLATFORM),windows)
    ASMFORMAT = win64
    EXECUTABLE_EXT = .exe
endif

ifeq ($(PLATFORM),macos)
    ASMFORMAT = macho64
    EXECUTABLE_EXT = 
endif

# Compiler optimization level
OPTIMIZATION = -O0

# Debug settings
DEBUG_SYMBOLS = -g -F dwarf

# Memory safety features (enabled by default)
MEMORY_SAFETY = 1

# Bounds checking (enabled by default)
BOUNDS_CHECKING = 1