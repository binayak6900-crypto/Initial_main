# mc² Build System for Unix/Linux/macOS
# Supports command-line arguments
#
# ⚠️ NOTE: C++ FILES DELETED - ASSEMBLY-ONLY DEVELOPMENT ⚠️
# The C++ bootstrap assembler has been deleted (Task 7).
# This build system now uses the compiled mc2asm executable for all operations.
# All future development is assembly-only using spacetime assembly.
# The C++ compilation targets below are preserved for reference but will not work.

.PHONY: help compile self-host clean assembler test-asm

# Default values
SRC ?=
OUT ?=
VERSION ?=
VERBOSE ?= 0
INDENT_MODE ?= strict
INDENT_SIZE ?= 4

# Compiler settings
CXX := g++
CXXFLAGS := -std=c++17 -Wall -Wextra -O2
ifeq ($(VERBOSE),1)
	CXXFLAGS += -DVERBOSE
endif

# Directories
SRC_DIR := src
BUILD_DIR := build
BIN_DIR := bin

# Assembler source files
ASM_SOURCES := $(SRC_DIR)/assembler.cpp $(SRC_DIR)/runtime.cpp $(SRC_DIR)/main.cpp
ASM_OBJECTS := $(BUILD_DIR)/assembler.o $(BUILD_DIR)/runtime.o $(BUILD_DIR)/main.o
ASM_TARGET := $(BIN_DIR)/mc2asm

help:
	@echo "mc² Build System - Unix/Linux/macOS"
	@echo ""
	@echo "Usage:"
	@echo "  make assembler                              Build the bootstrap assembler"
	@echo "  make test-asm                               Test the assembler"
	@echo "  make compile SRC=<input> OUT=<output>       Compile a .mc² source file"
	@echo "  make self-host VERSION=<version>            Build compiler using previous version"
	@echo "  make clean                                  Remove build artifacts"
	@echo "  make help                                   Show this help message"
	@echo ""
	@echo "Targets:"
	@echo "  assembler        Build the C++ bootstrap assembler"
	@echo "  test-asm         Test the assembler with test_simple.asm"
	@echo "  compile          Compile a .mc² source file"
	@echo "  self-host        Build compiler using previous version"
	@echo "  clean            Remove build artifacts"
	@echo "  help             Show this help message"
	@echo ""
	@echo "Options:"
	@echo "  SRC=<file>       Specify input source file"
	@echo "  OUT=<file>       Specify output file path"
	@echo "  VERSION=<ver>    Specify compiler version for self-hosting"
	@echo "  VERBOSE=1        Enable detailed compilation output"
	@echo "  INDENT_MODE=<m>  Set indentation mode (strict/loose/extra-loose)"
	@echo "  INDENT_SIZE=<n>  Set indentation size (default: 4)"
	@echo ""
	@echo "Examples:"
	@echo "  make compile SRC=src/main.mc² OUT=bin/program.e²"
	@echo "  make self-host VERSION=v-1-0-0-0-0"
	@echo "  make compile SRC=test.mc² OUT=test.e² VERBOSE=1 INDENT_MODE=loose"
	@echo ""

compile:
	@echo "[mc² Build System] Compiling $(SRC)..."
	@if [ -z "$(SRC)" ]; then \
		echo "Error: Input file not specified. Use SRC=<file>"; \
		exit 1; \
	fi
	@if [ -z "$(OUT)" ]; then \
		echo "Error: Output file not specified. Use OUT=<file>"; \
		exit 1; \
	fi
	@echo "Compilation target: $(SRC) -> $(OUT)"
	@echo "Indentation mode: $(INDENT_MODE)"
	@echo "Indent size: $(INDENT_SIZE)"
	@if [ "$(VERBOSE)" = "1" ]; then echo "[Verbose mode enabled]"; fi
	@echo ""
	@echo "Note: Compiler not yet implemented. This is a placeholder."

self-host:
	@echo "[mc² Build System] Self-hosting build..."
	@if [ -z "$(VERSION)" ]; then \
		echo ""; \
		echo "Version not specified. Please provide version information."; \
		echo ""; \
		echo "Format: prefix-X-Y-Z-W-V"; \
		echo "Supported prefixes: v, alpha, beta, rc, dev"; \
		echo "Examples: v-1-0-0-0-0, alpha-0-0-0-2-0, beta-0-0-1-0-0"; \
		echo ""; \
		printf "Enter version prefix (v/alpha/beta/rc/dev): "; \
		read VERSION_PREFIX; \
		printf "Enter major version: "; \
		read VERSION_MAJOR; \
		printf "Enter minor version: "; \
		read VERSION_MINOR; \
		printf "Enter patch version: "; \
		read VERSION_PATCH; \
		printf "Enter build version: "; \
		read VERSION_BUILD; \
		printf "Enter revision: "; \
		read VERSION_REVISION; \
		VERSION="$$VERSION_PREFIX-$$VERSION_MAJOR-$$VERSION_MINOR-$$VERSION_PATCH-$$VERSION_BUILD-$$VERSION_REVISION"; \
		echo ""; \
		echo "Building version: $$VERSION"; \
		echo ""; \
		$(MAKE) self-host VERSION=$$VERSION; \
		exit 0; \
	fi
	@if [ -d "bin/$(VERSION)" ]; then \
		echo ""; \
		echo "Warning: Version $(VERSION) already exists!"; \
		printf "Overwrite existing version? (y/n): "; \
		read OVERWRITE; \
		if [ "$$OVERWRITE" != "y" ] && [ "$$OVERWRITE" != "Y" ]; then \
			echo "Build cancelled."; \
			exit 1; \
		fi; \
	fi
	@echo "Building compiler version: $(VERSION)"
	@echo "Target directory: bin/$(VERSION)/"
	@mkdir -p bin/$(VERSION)
	@echo ""
	@echo "Detecting existing versions..."
	@LAST_VERSION=$$(ls -1 bin 2>/dev/null | grep -E '^[a-z]+-[0-9]' | tail -1); \
	if [ -n "$$LAST_VERSION" ]; then \
		echo "Last version found: $$LAST_VERSION"; \
		echo "Current version: $(VERSION)"; \
	fi
	@echo ""
	@echo "Copying assembler to versioned directory..."
	@BINARY_NAME="mc2-$(VERSION)"; \
	if [ -f $(ASM_TARGET) ]; then \
		cp $(ASM_TARGET) bin/$(VERSION)/$$BINARY_NAME; \
		echo "Created: bin/$(VERSION)/$$BINARY_NAME"; \
	elif [ -f bin/v-0-0-0-1-0/mc2asm ]; then \
		cp bin/v-0-0-0-1-0/mc2asm bin/$(VERSION)/$$BINARY_NAME; \
		echo "Created: bin/$(VERSION)/$$BINARY_NAME"; \
	else \
		echo "Warning: Assembler not found. Build assembler first."; \
	fi
	@echo "Creating version metadata..."
	@echo "Version: $(VERSION)" > bin/$(VERSION)/VERSION.txt
	@echo "Build Date: $$(date)" >> bin/$(VERSION)/VERSION.txt
	@echo "Build System: Unix/Linux/macOS" >> bin/$(VERSION)/VERSION.txt
	@echo ""
	@echo "Compiler version $(VERSION) saved to bin/$(VERSION)/"
	@echo ""
	@echo "To use this version:"
	@echo "  bin/$(VERSION)/mc2-$(VERSION) input.spacetime -o output.e²"

assembler: $(ASM_TARGET)
	@echo "[mc² Build System] Bootstrap assembler built successfully: $(ASM_TARGET)"

$(ASM_TARGET): $(ASM_OBJECTS)
	@mkdir -p $(BIN_DIR)
	@echo "[Linking] $@"
	@$(CXX) $(CXXFLAGS) -o $@ $^

$(BUILD_DIR)/%.o: $(SRC_DIR)/%.cpp
	@mkdir -p $(BUILD_DIR)
	@echo "[Compiling] $<"
	@$(CXX) $(CXXFLAGS) -c -o $@ $<

test-asm: assembler
	@echo "[mc² Build System] Testing assembler..."
	@$(ASM_TARGET) tests/test_simple.spacetime tests/test_simple.e²
	@echo "[Test] Assembler test complete"

clean:
	@echo "[mc² Build System] Cleaning build artifacts..."
	@rm -rf bin/ build/ garbage/*.e² garbage/*.o
	@find . -name "*.e²" -type f -delete 2>/dev/null || true
	@find . -name "*.e_sub2" -type f -delete 2>/dev/null || true
	@find . -name "*.o" -type f -delete 2>/dev/null || true
	@echo "Clean complete."
