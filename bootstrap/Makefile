# Makefile for Xit Bootstrap Compiler
# Uses nasm, gcc, and ld for bootstrap compilation

# Compiler and assembler settings
ASM = nasm
ASMFLAGS = -f win64
LD = gcc
LDFLAGS = 

# Directories
SRCDIR = src
BUILDDIR = build

# Source files
SOURCES = $(wildcard $(SRCDIR)/*.asm)
OBJECTS = $(SOURCES:$(SRCDIR)/%.asm=$(BUILDDIR)/%.o)

# Target executable
TARGET = $(BUILDDIR)/xit-bootstrap.exe

# Default target
all: $(TARGET)

# Create build directory
$(BUILDDIR):
	mkdir -p $(BUILDDIR)

# Compile assembly files to object files
$(BUILDDIR)/%.o: $(SRCDIR)/%.asm | $(BUILDDIR)
	$(ASM) $(ASMFLAGS) -o $@ $<

# Link object files to create executable
$(TARGET): $(OBJECTS)
	$(LD) $(LDFLAGS) -o $@ $^
	chmod +x $@

# Clean build artifacts
clean:
	rm -rf $(BUILDDIR)

# Install (copy to system path - optional)
install: $(TARGET)
	cp $(TARGET) /usr/local/bin/xit-bootstrap

# Uninstall
uninstall:
	rm -f /usr/local/bin/xit-bootstrap

# Test the bootstrap compiler
test: $(TARGET) test-lexer-property test-parser-unit test-memory-safety test-bounds-checking
	@echo "Testing bootstrap compiler..."
	@echo "Running lexer property tests..."
	./$(BUILDDIR)/test_lexer_property
	@echo "Running parser unit tests..."
	./$(BUILDDIR)/test_parser_unit
	@echo "Running memory safety property tests..."
	./$(BUILDDIR)/test_memory_safety_property
	@echo "Running bounds checking tests..."
	./$(BUILDDIR)/test_bounds_checking

# Build and run lexer property test
test-lexer-property: $(BUILDDIR)/test_lexer_property
	@echo "Running lexer property-based test..."
	./$(BUILDDIR)/test_lexer_property

# Build lexer property test
$(BUILDDIR)/test_lexer_property: tests/test_lexer_property.c $(BUILDDIR)/lexer.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Build and run parser unit tests
test-parser-unit: $(BUILDDIR)/test_parser_unit
	@echo "Running parser unit tests..."
	./$(BUILDDIR)/test_parser_unit

# Build parser unit test
$(BUILDDIR)/test_parser_unit: tests/test_parser_unit.c $(BUILDDIR)/lexer.o $(BUILDDIR)/parser.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Build and run memory safety property test
test-memory-safety: $(BUILDDIR)/test_memory_safety_property
	@echo "Running memory safety property-based test..."
	./$(BUILDDIR)/test_memory_safety_property

# Build memory safety property test
$(BUILDDIR)/test_memory_safety_property: tests/test_memory_safety_property.c $(BUILDDIR)/memory.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Build and run bounds checking test
test-bounds-checking: $(BUILDDIR)/test_bounds_checking
	@echo "Running bounds checking test..."
	./$(BUILDDIR)/test_bounds_checking

# Build bounds checking test
$(BUILDDIR)/test_bounds_checking: tests/test_bounds_checking.c $(BUILDDIR)/memory.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Build debug lexer test
$(BUILDDIR)/debug_lexer: tests/debug_lexer.c $(BUILDDIR)/lexer.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Build simple lexer test
$(BUILDDIR)/simple_lexer_test: tests/simple_lexer_test.c $(BUILDDIR)/lexer.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Run simple lexer test
simple-lexer-test: $(BUILDDIR)/simple_lexer_test
	./$(BUILDDIR)/simple_lexer_test

# Build character function test
$(BUILDDIR)/test_char_functions: tests/test_char_functions.c $(BUILDDIR)/lexer.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Build debug parser test
$(BUILDDIR)/debug_parser: tests/debug_parser.c $(BUILDDIR)/lexer.o $(BUILDDIR)/parser.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Run debug parser test
debug-parser: $(BUILDDIR)/debug_parser
	./$(BUILDDIR)/debug_parser

# Build detailed debug parser test
$(BUILDDIR)/debug_parser_detailed: tests/debug_parser_detailed.c $(BUILDDIR)/lexer.o $(BUILDDIR)/parser.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Build minimal parser test
$(BUILDDIR)/minimal_parser_test: tests/minimal_parser_test.c $(BUILDDIR)/lexer.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Build step by step parser test
$(BUILDDIR)/step_by_step_parser: tests/step_by_step_parser.c $(BUILDDIR)/lexer.o $(BUILDDIR)/parser.o | $(BUILDDIR)
	$(LD) $(LDFLAGS) -o $@ $^

# Run step by step parser test
step-by-step-parser: $(BUILDDIR)/step_by_step_parser
	./$(BUILDDIR)/step_by_step_parser

# Debug build with symbols
debug: ASMFLAGS += -g -F dwarf
debug: $(TARGET)

# Help target
help:
	@echo "Available targets:"
	@echo "  all      - Build the bootstrap compiler (default)"
	@echo "  clean    - Remove build artifacts"
	@echo "  install  - Install compiler to system path"
	@echo "  uninstall- Remove compiler from system path"
	@echo "  test     - Run compiler tests"
	@echo "  debug    - Build with debug symbols"
	@echo "  help     - Show this help message"

# Phony targets
.PHONY: all clean install uninstall test debug help

# Dependencies (assembly files don't have complex dependencies for bootstrap)
$(BUILDDIR)/main.o: $(SRCDIR)/main.asm
$(BUILDDIR)/lexer.o: $(SRCDIR)/lexer.asm
$(BUILDDIR)/parser.o: $(SRCDIR)/parser.asm $(SRCDIR)/pe_generator.asm $(SRCDIR)/runtime.asm
$(BUILDDIR)/memory.o: $(SRCDIR)/memory.asm
$(BUILDDIR)/pe_generator.o: $(SRCDIR)/pe_generator.asm
$(BUILDDIR)/runtime.o: $(SRCDIR)/runtime.asm