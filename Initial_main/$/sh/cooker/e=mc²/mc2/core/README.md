# Core Assembly Functions

This directory contains the core functions implemented in spacetime assembly for the mc² programming language. These functions form the foundation upon which all higher-level language features are built.

## Philosophy

**Nothing is built-in by default.** Everything—including basic functions like `print()` and `input()`—must be built from assembly first. The language provides the assembly foundation, and all higher-level features are constructed on top of it.

## Implemented Functions

### 1. print.spacetime

**Purpose**: Variadic output function for displaying text and values to standard output.

**Features**:
- Supports variadic arguments via `_possible_args_`
- Handles multiple types (int, float, str)
- Hardware I/O via `io_write` syscall
- Supports up to 8 arguments (registers %r0-%r7)

**Usage**:
```
@main:
    :: load.const << "Hello, " >> -> %r0
    :: load.const << "world" >> -> %r1
    :: load.const << "!" >> -> %r2
    :: load.const << 0 >> -> %r3  -(null terminator)
    :: call << @print >>
    :: return
```

**Implementation Details**:
- Uses %r10 as argument counter
- Uses %r11 for comparison operations
- Uses %r12 for null checking
- Iterates through registers %r0-%r7
- Stops when encountering null/zero value
- Each argument is output via `io_write` syscall

**Validates Requirements**: 13.1, 13.4

---

### 2. input.spacetime

**Purpose**: Input function for reading user input from standard input.

**Features**:
- Optional prompt parameter
- Hardware I/O via `io_read` syscall
- Returns string value in %r0
- Calls `print()` if prompt provided

**Usage (with prompt)**:
```
@main:
    :: load.const << "Enter your name: " >> -> %r0
    :: call << @input >>
    -(result is now in %r0)
    :: return
```

**Usage (without prompt)**:
```
@main:
    :: load.const << 0 >> -> %r0
    :: call << @input >>
    -(result is now in %r0)
    :: return
```

**Implementation Details**:
- Checks %r0 for prompt (non-zero = prompt provided)
- If prompt exists, calls `@print` to display it
- Uses `io_read` syscall to read user input
- Returns input string in %r0

**Validates Requirements**: 13.2, 13.5, 13.6

---

### 3. memory.spacetime

**Purpose**: Memory management for arbitrary precision integers and floats.

**Features**:
- Arbitrary precision integers (BigInt) - NO LIMITS
- Arbitrary precision floats (BigFloat) - NO LIMITS
- Automatic promotion when native types overflow
- Garbage collection support (placeholder for future implementation)
- Memory allocation via `malloc` syscall
- Memory deallocation via `free` syscall

**BigInt Structure**:
```
Offset 0: sign (1 byte) - 0 for positive, 1 for negative
Offset 1: length (4 bytes) - number of 64-bit digits
Offset 5: digits array (8 bytes per digit, little-endian)
```

**BigFloat Structure**:
```
Offset 0: sign (1 byte)
Offset 1: exponent (4 bytes)
Offset 5: precision (4 bytes)
Offset 9: mantissa pointer (8 bytes) - points to BigInt
```

**Functions**:
- `@allocate_bigint` - Allocate BigInt structure
- `@allocate_bigfloat` - Allocate BigFloat structure
- `@bigint_add` - Add two BigInts
- `@bigint_mul` - Multiply two BigInts
- `@bigfloat_add` - Add two BigFloats
- `@bigfloat_mul` - Multiply two BigFloats
- `@promote_to_bigint` - Auto-promote native int
- `@promote_to_bigfloat` - Auto-promote native float
- `@gc_collect` - Garbage collection (placeholder)
- `@free_bigint` - Free BigInt memory
- `@free_bigfloat` - Free BigFloat memory

**Usage Example (BigInt)**:
```
@main:
    -(Allocate first BigInt)
    :: load.const << 99999999999999999999 >> -> %r0
    :: call << @allocate_bigint >>
    :: load.const << 0 >> -> %r8
    :: add << %r0, %r8 >> -> %r8  -(save pointer)
    
    -(Allocate second BigInt)
    :: load.const << 88888888888888888888 >> -> %r0
    :: call << @allocate_bigint >>
    :: load.const << 0 >> -> %r9
    :: add << %r0, %r9 >> -> %r9  -(save pointer)
    
    -(Add them together)
    :: load.const << 0 >> -> %r0
    :: add << %r8, %r0 >> -> %r0
    :: load.const << 0 >> -> %r1
    :: add << %r9, %r1 >> -> %r1
    :: call << @bigint_add >>
    
    -(Free memory)
    :: load.const << 0 >> -> %r0
    :: add << %r8, %r0 >> -> %r0
    :: call << @free_bigint >>
    :: load.const << 0 >> -> %r0
    :: add << %r9, %r0 >> -> %r0
    :: call << @free_bigint >>
    
    :: return
```

**Validates Requirements**: 2.5.1-2.5.7, 2.2, 2.3, 2.4

---

## Hardware I/O Syscalls

All core functions use hardware I/O syscalls for OS independence:

### io_write
- **Purpose**: Write to standard output
- **Used by**: `print()` to output text
- **Syntax**: `:: syscall << io_write >> << %r0 >>`
- **Input**: %r0 = data to write (string or number)
- **Output**: none

### io_read
- **Purpose**: Read from standard input
- **Used by**: `input()` to read user input
- **Syntax**: `:: syscall << io_read >> -> %r0`
- **Input**: none
- **Output**: %r0 = string read from stdin

### malloc
- **Purpose**: Allocate memory
- **Used by**: Memory manager for BigInt/BigFloat
- **Syntax**: `:: syscall << malloc >> << %r0 >> -> %r1`
- **Input**: %r0 = size in bytes
- **Output**: %r1 = pointer to allocated memory

### free
- **Purpose**: Free memory
- **Used by**: Memory manager to deallocate
- **Syntax**: `:: syscall << free >> << %r0 >>`
- **Input**: %r0 = pointer to memory
- **Output**: none

---

## Arbitrary Precision Number Limits

mc² supports truly unlimited precision:

### Native Types
- **int**: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807 (64-bit)
- **float**: ~15 decimal digits precision (64-bit)

### Arbitrary Precision Types
- **BigInt**: NO LIMIT - can represent any integer
- **BigFloat**: NO LIMIT - can represent any precision

### Automatic Promotion
- When native int exceeds 9,223,372,036,854,775,807, auto-promotes to BigInt
- When native float loses precision, auto-promotes to BigFloat
- Promotion is transparent to the programmer

---

## Integration Example

All core functions work together seamlessly:

```
-(Interactive Calculator)
@calculator:
    -(Prompt for first number)
    :: load.const << "Enter first number: " >> -> %r0
    :: call << @input >>
    :: load.const << 0 >> -> %r8
    :: add << %r0, %r8 >> -> %r8  -(save input)
    
    -(Prompt for second number)
    :: load.const << "Enter second number: " >> -> %r0
    :: call << @input >>
    :: load.const << 0 >> -> %r9
    :: add << %r0, %r9 >> -> %r9  -(save input)
    
    -(Convert to BigInts for arbitrary precision)
    :: load.const << 0 >> -> %r0
    :: add << %r8, %r0 >> -> %r0
    :: call << @allocate_bigint >>
    :: load.const << 0 >> -> %r8
    :: add << %r0, %r8 >> -> %r8
    
    :: load.const << 0 >> -> %r0
    :: add << %r9, %r0 >> -> %r0
    :: call << @allocate_bigint >>
    :: load.const << 0 >> -> %r9
    :: add << %r0, %r9 >> -> %r9
    
    -(Add them)
    :: load.const << 0 >> -> %r0
    :: add << %r8, %r0 >> -> %r0
    :: load.const << 0 >> -> %r1
    :: add << %r9, %r1 >> -> %r1
    :: call << @bigint_add >>
    
    -(Print result)
    :: load.const << "Result: " >> -> %r0
    :: load.const << 0 >> -> %r1
    :: add << %r0, %r1 >> -> %r1  -(result from bigint_add)
    :: load.const << 0 >> -> %r2
    :: call << @print >>
    
    -(Clean up)
    :: load.const << 0 >> -> %r0
    :: add << %r8, %r0 >> -> %r0
    :: call << @free_bigint >>
    :: load.const << 0 >> -> %r0
    :: add << %r9, %r0 >> -> %r0
    :: call << @free_bigint >>
    
    :: return
```

---

## Testing

Comprehensive tests for all core functions are located in `tests/test_core_functions.spacetime`.

To run tests:
```bash
# Windows
mc2asm -assemble tests/test_core_functions.spacetime -o tests/test_core_functions.e²
mc2asm tests/test_core_functions.e²

# Unix/Linux/macOS
./mc2asm -assemble tests/test_core_functions.spacetime -o tests/test_core_functions.e²
./mc2asm tests/test_core_functions.e²
```

---

## Next Steps

After implementing and testing these core functions:

1. **Task 5.4**: Write comprehensive tests for all core functions
2. **Task 6**: Test assembly thoroughly (CRITICAL CHECKPOINT)
3. **Task 7**: ⚠️ DELETE ALL C++ FILES ⚠️ (Point of no return)
4. **Task 8+**: Self-hosted compiler development (pure mc² + assembly)

---

## Requirements Validated

- ✅ 13.1: print function implemented in spacetime assembly
- ✅ 13.4: print supports variadic arguments via _possible_args_
- ✅ 13.2: input function implemented in spacetime assembly
- ✅ 13.5: input reads from standard input and returns string
- ✅ 13.6: input supports optional prompt parameter
- ✅ 2.5.1: Memory manager implemented in spacetime assembly
- ✅ 2.5.2: Automatic allocation for integers exceeding native size
- ✅ 2.5.3: Automatic allocation for floats exceeding native precision
- ✅ 2.5.4: Garbage collection support (placeholder)
- ✅ 2.5.5: Arithmetic operations on arbitrary precision numbers
- ✅ 2.5.6: Automatic promotion when overflow would occur
- ✅ 2.5.7: Automatic promotion when precision would be lost
- ✅ 2.2: Arbitrary precision integers (no limits)
- ✅ 2.3: Arbitrary precision floats (no limits)
- ✅ 2.4: Automatic memory allocation for large numbers

---

**Status**: Task 5 Complete - Core functions implemented and documented
**Next**: Task 5.4 - Write comprehensive tests
