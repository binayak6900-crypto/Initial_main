# Spacetime Assembly Reference

## Overview

Spacetime assembly is the advanced assembly format used as an intermediate representation in mc². It uses mc²'s distinctive syntax with `::`, `<<`, and `>>` operators, making it more readable than traditional assembly while maintaining direct mapping to .e² bytecode.

**File Extension**: `.spacetime`

**Design Principles**:
- Uses `::` for instruction prefixes (similar to statement starters in mc²)
- Uses `<< >>` for operand grouping
- Supports symbolic operators defined in source
- More readable than traditional assembly
- Direct mapping to .e² bytecode
- Saved as `.spacetime` files during compilation

## Instruction Format

The general instruction format is:

```
:: <opcode> << <operands> >> -> <destination>
```

**Components**:
- `::` - Required instruction prefix (marks the start of an instruction)
- `<opcode>` - The operation to perform (see Opcodes section)
- `<< <operands> >>` - Zero or more operands enclosed in angle brackets
- `-> <destination>` - Optional destination register or label

**Examples**:
```
:: load.const << 42 >> -> %r0
:: add << %r0, %r1 >> -> %r2
:: call << print >> << "Hello" >>
:: return << %r0 >>
```

## Register Naming

Spacetime assembly uses virtual registers with the `%r` prefix:

- `%r0`, `%r1`, `%r2`, ..., `%r15` - General-purpose registers
- `%r0` - Typically used for return values
- `%rsp` - Stack pointer (special register)
- `%rbp` - Base pointer (special register)

**Register Usage Conventions**:
- `%r0` - Return value, first argument
- `%r1-%r3` - Additional arguments
- `%r4-%r7` - Caller-saved temporary registers
- `%r8-%r15` - Callee-saved registers

**Example**:
```
:: load.const << 10 >> -> %r0
:: load.const << 20 >> -> %r1
:: add << %r0, %r1 >> -> %r2
```

## Label Syntax

Labels are used for control flow and function definitions. They use the `@` prefix:

```
@label_name
```

**Label Usage**:
- Function entry points: `@function_name`
- Loop labels: `@loop_start`, `@loop_end`
- Conditional branches: `@if_true`, `@if_false`, `@if_end`

**Example**:
```
@main:
    :: load.const << 0 >> -> %r0
    :: jump << @loop_start >>

@loop_start:
    :: add << %r0, 1 >> -> %r0
    :: cmp << %r0, 10 >>
    :: jlt << @loop_start >>
    :: return << %r0 >>
```

## Constant Syntax

Constants can be specified in several ways:

### Immediate Values
- **Integers**: `42`, `-17`, `0`
- **Floats**: `3.14`, `-0.5`, `1.0e10`
- **Strings**: `"Hello, world!"`, `"Line 1\nLine 2"`
- **Booleans**: `true`, `false`
- **Null**: `null`

### Global Variables
Global variables are referenced with the `$` prefix:

```
$global_name
```

**Example**:
```
:: load.global << $counter >> -> %r0
:: add << %r0, 1 >> -> %r0
:: store.global << %r0 >> -> $counter
```

### Type Annotations
Constants can include type annotations when needed:

```
:: load.const << 42:int >> -> %r0
:: load.const << 3.14:float >> -> %r1
:: load.const << "text":str >> -> %r2
```

## Opcodes

### Memory Operations

#### load.const
Load a constant value into a register.

**Syntax**: `:: load.const << <value> >> -> <register>`

**Example**:
```
:: load.const << 42 >> -> %r0
:: load.const << "Hello" >> -> %r1
:: load.const << 3.14 >> -> %r2
```

#### load.global
Load a global variable into a register.

**Syntax**: `:: load.global << $<name> >> -> <register>`

**Example**:
```
:: load.global << $counter >> -> %r0
```

#### store.global
Store a register value into a global variable.

**Syntax**: `:: store.global << <register> >> -> $<name>`

**Example**:
```
:: store.global << %r0 >> -> $counter
```

#### load.local
Load a local variable into a register.

**Syntax**: `:: load.local << <offset> >> -> <register>`

**Example**:
```
:: load.local << -8 >> -> %r0  -(load from stack offset -8)
```

#### store.local
Store a register value into a local variable.

**Syntax**: `:: store.local << <register> >> -> <offset>`

**Example**:
```
:: store.local << %r0 >> -> -8  -(store to stack offset -8)
```

### Arithmetic Operations

#### add
Add two values.

**Syntax**: `:: add << <src1>, <src2> >> -> <dest>`

**Example**:
```
:: add << %r0, %r1 >> -> %r2
:: add << %r0, 1 >> -> %r0  -(increment)
```

#### sub
Subtract two values.

**Syntax**: `:: sub << <src1>, <src2> >> -> <dest>`

**Example**:
```
:: sub << %r0, %r1 >> -> %r2
:: sub << %r0, 1 >> -> %r0  -(decrement)
```

#### mul
Multiply two values.

**Syntax**: `:: mul << <src1>, <src2> >> -> <dest>`

**Example**:
```
:: mul << %r0, %r1 >> -> %r2
:: mul << %r0, 2 >> -> %r0  -(double)
```

#### div
Divide two values (integer or float division based on operand types).

**Syntax**: `:: div << <dividend>, <divisor> >> -> <dest>`

**Example**:
```
:: div << %r0, %r1 >> -> %r2
:: div << %r0, 2 >> -> %r0  -(halve)
```

#### mod
Modulo operation (remainder after division).

**Syntax**: `:: mod << <dividend>, <divisor> >> -> <dest>`

**Example**:
```
:: mod << %r0, %r1 >> -> %r2
:: mod << %r0, 10 >> -> %r0  -(get last digit)
```

#### neg
Negate a value (unary minus).

**Syntax**: `:: neg << <src> >> -> <dest>`

**Example**:
```
:: neg << %r0 >> -> %r1
```

### Comparison Operations

#### cmp
Compare two values and set flags.

**Syntax**: `:: cmp << <src1>, <src2> >>`

**Example**:
```
:: cmp << %r0, %r1 >>
:: jlt << @less_than >>
```

#### test
Test a value (typically for zero/non-zero).

**Syntax**: `:: test << <src> >>`

**Example**:
```
:: test << %r0 >>
:: jz << @is_zero >>
```

### Control Flow Operations

#### jump
Unconditional jump to a label.

**Syntax**: `:: jump << @<label> >>`

**Example**:
```
:: jump << @loop_start >>
```

#### jz (jump if zero)
Jump if the last comparison resulted in zero/equal.

**Syntax**: `:: jz << @<label> >>`

**Example**:
```
:: test << %r0 >>
:: jz << @is_zero >>
```

#### jnz (jump if not zero)
Jump if the last comparison resulted in non-zero/not equal.

**Syntax**: `:: jnz << @<label> >>`

**Example**:
```
:: test << %r0 >>
:: jnz << @not_zero >>
```

#### jlt (jump if less than)
Jump if the last comparison showed less than.

**Syntax**: `:: jlt << @<label> >>`

**Example**:
```
:: cmp << %r0, 10 >>
:: jlt << @less_than_ten >>
```

#### jle (jump if less than or equal)
Jump if the last comparison showed less than or equal.

**Syntax**: `:: jle << @<label> >>`

#### jgt (jump if greater than)
Jump if the last comparison showed greater than.

**Syntax**: `:: jgt << @<label> >>`

#### jge (jump if greater than or equal)
Jump if the last comparison showed greater than or equal.

**Syntax**: `:: jge << @<label> >>`

### Function Operations

#### call
Call a function.

**Syntax**: `:: call << <function> >> << <args> >>`

**Example**:
```
:: call << print >> << "Hello, world!" >>
:: call << add_numbers >> << %r0, %r1 >>
```

**With Variadic Arguments**:
```
:: call << print >> << _possible_args_ >> {
    :: load.const << "Value:" >> -> %r0
    :: load.const << 42 >> -> %r1
    :: load.const << "is the answer" >> -> %r2
}
```

#### return
Return from a function.

**Syntax**: `:: return` or `:: return << <value> >>`

**Example**:
```
:: return  -(void return)
:: return << %r0 >>  -(return value in %r0)
```

### System Call Operations

#### syscall
Invoke a system call.

**Syntax**: `:: syscall << <syscall_name> >> << <args> >> -> <dest>`

**System Calls**:
- `io_write` - Write to standard output
- `io_read` - Read from standard input
- `malloc` - Allocate memory
- `free` - Free memory
- `exit` - Exit program

**Example**:
```
:: syscall << io_write >> << "Hello\n" >>
:: syscall << io_read >> -> %r0
:: syscall << malloc >> << 1024 >> -> %r0
:: syscall << free >> << %r0 >>
:: syscall << exit >> << 0 >>
```

### Type Conversion Operations

#### cast
Convert between types.

**Syntax**: `:: cast << <src>, <target_type> >> -> <dest>`

**Example**:
```
:: cast << %r0, float >> -> %r1  -(int to float)
:: cast << %r0, int >> -> %r1    -(float to int)
:: cast << %r0, str >> -> %r1    -(any to string)
```

### Arbitrary Precision Operations

#### promote.bigint
Promote a native integer to arbitrary precision.

**Syntax**: `:: promote.bigint << <src> >> -> <dest>`

**Example**:
```
:: load.const << 999999999999999999 >> -> %r0
:: promote.bigint << %r0 >> -> %r1  -(promote to arbitrary precision)
```

#### promote.bigfloat
Promote a native float to arbitrary precision.

**Syntax**: `:: promote.bigfloat << <src> >> -> <dest>`

**Example**:
```
:: load.const << 3.141592653589793238 >> -> %r0
:: promote.bigfloat << %r0 >> -> %r1  -(promote to arbitrary precision)
```

#### bigint.add
Add two arbitrary precision integers.

**Syntax**: `:: bigint.add << <src1>, <src2> >> -> <dest>`

**Example**:
```
:: bigint.add << %r0, %r1 >> -> %r2
```

#### bigfloat.mul
Multiply two arbitrary precision floats.

**Syntax**: `:: bigfloat.mul << <src1>, <src2> >> -> <dest>`

**Example**:
```
:: bigfloat.mul << %r0, %r1 >> -> %r2
```

## Variadic Functions with _possible_args_

Spacetime assembly supports variadic functions using the `_possible_args_` placeholder. This allows functions to accept a variable number of arguments.

**Syntax**:
```
:: func <name> << _possible_args_ >> -> <return_type> {
    -(function body)
}
```

**Calling with _possible_args_**:
```
:: call << <function> >> << _possible_args_ >> {
    :: load.const << <arg1> >> -> %r0
    :: load.const << <arg2> >> -> %r1
    :: load.const << <arg3> >> -> %r2
}
```

**Example - print function**:
```
:: func print << _possible_args_ >> -> void {
    -(iterate through arguments and output each)
    :: syscall << io_write >> << %r0 >>
    :: return
}

-(calling print with multiple arguments)
:: call << print >> << _possible_args_ >> {
    :: load.const << "Value:" >> -> %r0
    :: load.const << 42 >> -> %r1
    :: load.const << "is the answer" >> -> %r2
}
```

**Custom Argument Separators**:
The `_possible_args_` system supports custom binding rules for argument separators:

```
__possible_args_ :< br.set(,) :>  -(comma-separated arguments)
__possible_args_ :< br.set(;) :>  -(semicolon-separated arguments)
```

## Complete Examples

### Example 1: Simple Function with Arithmetic

```
@add_numbers:
    :: load.local << -8 >> -> %r0   -(load first parameter)
    :: load.local << -16 >> -> %r1  -(load second parameter)
    :: add << %r0, %r1 >> -> %r2
    :: return << %r2 >>

@main:
    :: load.const << 10 >> -> %r0
    :: load.const << 20 >> -> %r1
    :: call << add_numbers >> << %r0, %r1 >>
    :: return << %r0 >>
```

### Example 2: Control Flow with Conditionals

```
@check_value:
    :: load.local << -8 >> -> %r0
    :: cmp << %r0, 100 >>
    :: jgt << @value_is_big >>
    
@value_is_small:
    :: call << print >> << "Value is small" >>
    :: jump << @check_value_end >>
    
@value_is_big:
    :: call << print >> << "Value is big" >>
    
@check_value_end:
    :: return
```

### Example 3: Loop Example

```
@count_to_ten:
    :: load.const << 0 >> -> %r0
    
@loop_start:
    :: call << print >> << %r0 >>
    :: add << %r0, 1 >> -> %r0
    :: cmp << %r0, 10 >>
    :: jle << @loop_start >>
    
    :: return
```

### Example 4: print Function with Variadic Arguments

```
:: func print << _possible_args_ >> -> void {
    -(iterate through all arguments)
    :: load.const << 0 >> -> %r10  -(counter)
    
@print_loop:
    :: load.arg << %r10 >> -> %r0  -(load argument at index)
    :: test << %r0 >>
    :: jz << @print_end >>  -(null means no more args)
    
    :: syscall << io_write >> << %r0 >>
    :: add << %r10, 1 >> -> %r10
    :: jump << @print_loop >>
    
@print_end:
    :: return
}

-(usage)
@main:
    :: call << print >> << _possible_args_ >> {
        :: load.const << "The answer is " >> -> %r0
        :: load.const << 42 >> -> %r1
        :: load.const << " and that's final!" >> -> %r2
    }
    :: return
```

### Example 5: input Function with Optional Prompt

```
:: func input << _prompt_:str >> -> str {
    -(check if prompt was provided)
    :: load.local << -8 >> -> %r0
    :: test << %r0 >>
    :: jz << @input_no_prompt >>
    
@input_with_prompt:
    :: call << print >> << %r0 >>
    
@input_no_prompt:
    :: syscall << io_read >> -> %r0
    :: return << %r0 >>
}

-(usage with prompt)
@main:
    :: load.const << "Enter your name: " >> -> %r0
    :: call << input >> << %r0 >>
    :: store.local << %r0 >> -> -8  -(store result)
    
    -(usage without prompt)
    :: call << input >> << null >>
    :: return
```

### Example 6: Memory Management with Arbitrary Precision

```
:: func allocate_bigint << value:int >> -> ptr {
    -(check if value needs arbitrary precision)
    :: load.local << -8 >> -> %r0
    :: cmp << %r0, 9223372036854775807 >>  -(max int64)
    :: jle << @use_native >>
    
@use_bigint:
    :: syscall << malloc >> << 32 >> -> %r1  -(allocate BigInt structure)
    :: promote.bigint << %r0 >> -> %r2
    :: store << %r2 >> -> %r1
    :: return << %r1 >>
    
@use_native:
    :: return << %r0 >>  -(return native value)
}
```

### Example 7: Function with Global Variables

```
$counter :: int

@increment_counter:
    :: load.global << $counter >> -> %r0
    :: add << %r0, 1 >> -> %r0
    :: store.global << %r0 >> -> $counter
    :: return << %r0 >>

@main:
    :: load.const << 0 >> -> %r0
    :: store.global << %r0 >> -> $counter
    
    :: call << increment_counter >>
    :: call << increment_counter >>
    :: call << increment_counter >>
    
    :: load.global << $counter >> -> %r0
    :: call << print >> << "Counter:", %r0 >>
    :: return
```

## Instruction Reference Summary

### Memory Operations
- `load.const` - Load constant value
- `load.global` - Load global variable
- `store.global` - Store to global variable
- `load.local` - Load local variable
- `store.local` - Store to local variable
- `load.arg` - Load function argument by index

### Arithmetic Operations
- `add` - Addition
- `sub` - Subtraction
- `mul` - Multiplication
- `div` - Division
- `mod` - Modulo
- `neg` - Negation

### Comparison Operations
- `cmp` - Compare two values
- `test` - Test value for zero

### Control Flow Operations
- `jump` - Unconditional jump
- `jz` - Jump if zero
- `jnz` - Jump if not zero
- `jlt` - Jump if less than
- `jle` - Jump if less than or equal
- `jgt` - Jump if greater than
- `jge` - Jump if greater than or equal

### Function Operations
- `call` - Call function
- `return` - Return from function

### System Call Operations
- `syscall` - Invoke system call
  - `io_write` - Write to stdout
  - `io_read` - Read from stdin
  - `malloc` - Allocate memory
  - `free` - Free memory
  - `exit` - Exit program

### Type Conversion Operations
- `cast` - Convert between types

### Arbitrary Precision Operations
- `promote.bigint` - Promote to arbitrary precision integer
- `promote.bigfloat` - Promote to arbitrary precision float
- `bigint.add`, `bigint.sub`, `bigint.mul`, `bigint.div` - BigInt arithmetic
- `bigfloat.add`, `bigfloat.sub`, `bigfloat.mul`, `bigfloat.div` - BigFloat arithmetic

## Mapping to .e² Bytecode

Each spacetime assembly instruction maps to one or more bytecode instructions in the .e² format:

| Spacetime Assembly | .e² Bytecode | Opcode |
|-------------------|--------------|--------|
| `load.const` | `LOAD_CONST` | 0x01 |
| `load.global` | `LOAD_GLOBAL` | 0x02 |
| `store.global` | `STORE_GLOBAL` | 0x03 |
| `add` | `ADD` | 0x10 |
| `sub` | `SUB` | 0x11 |
| `mul` | `MUL` | 0x12 |
| `div` | `DIV` | 0x13 |
| `mod` | `MOD` | 0x14 |
| `cmp` | `CMP` | 0x20 |
| `jump` | `JMP` | 0x30 |
| `jz` | `JZ` | 0x31 |
| `jnz` | `JNZ` | 0x32 |
| `jlt` | `JLT` | 0x33 |
| `jle` | `JLE` | 0x34 |
| `jgt` | `JGT` | 0x35 |
| `jge` | `JGE` | 0x36 |
| `call` | `CALL` | 0x40 |
| `return` | `RET` | 0x41 |
| `syscall` | `SYSCALL` | 0x50 |
| `cast` | `CAST` | 0x60 |
| `promote.bigint` | `PROMOTE_BIGINT` | 0x70 |
| `promote.bigfloat` | `PROMOTE_BIGFLOAT` | 0x71 |

## Best Practices

1. **Use Descriptive Labels**: Name labels clearly to indicate their purpose
   - Good: `@loop_start`, `@error_handler`, `@validate_input`
   - Bad: `@l1`, `@x`, `@temp`

2. **Comment Your Code**: Use `-(comment)` to explain complex logic
   ```
   :: add << %r0, %r1 >> -> %r2  -(calculate total)
   ```

3. **Follow Calling Conventions**: Use registers consistently
   - `%r0` for return values and first argument
   - `%r1-%r3` for additional arguments
   - `%r4-%r7` for temporary values

4. **Handle Arbitrary Precision**: Check for overflow and promote when needed
   ```
   :: cmp << %r0, 9223372036854775807 >>
   :: jgt << @use_bigint >>
   ```

5. **Use _possible_args_ for Variadic Functions**: Implement flexible functions
   ```
   :: func print << _possible_args_ >> -> void
   ```

6. **Validate Inputs**: Check for null, zero, or invalid values before processing
   ```
   :: test << %r0 >>
   :: jz << @handle_null >>
   ```

7. **Clean Up Resources**: Free allocated memory when done
   ```
   :: syscall << free >> << %r0 >>
   ```

## See Also

- [mc² Syntax Reference](syntax-reference.md) - High-level language syntax
- [.e² Format Specification](e2-format.md) - Binary format details
- [Build System Reference](build-system.md) - Compilation and linking
- [Self-Hosting Guide](self-hosting.md) - Bootstrap process

---

**Validates Requirements**: 12.1, 12.2, 12.3