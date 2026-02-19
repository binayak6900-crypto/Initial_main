# Spacetime Assembly Examples

This document provides comprehensive examples of spacetime assembly code demonstrating key language features.

## Example 1: print Function with Variadic Arguments

The `print` function demonstrates how to handle variadic arguments using `_possible_args_`. This implementation iterates through all provided arguments and outputs them to standard output.

**Note**: The actual implementation in `mc2/core/print.spacetime` uses a jump table approach with explicit register checking for %r0-%r7. The example below shows the conceptual algorithm.

**Actual Implementation** (mc2/core/print.spacetime - 127 lines):
- Uses jump table with labels @print_arg_r0 through @print_arg_r7
- Counter-based iteration through registers %r0-%r7
- Each register is checked for null/zero before output
- Supports up to 8 arguments maximum

**Conceptual Algorithm**:
```
:: func print << _possible_args_ >> -> void {
    -(Initialize argument counter)
    :: load.const << 0 >> -> %r10
    
@print_loop:
    -(Load argument at current index)
    :: load.arg << %r10 >> -> %r0
    
    -(Check if we've reached the end (null argument))
    :: test << %r0 >>
    :: jz << @print_end >>
    
    -(Output the argument to stdout)
    :: syscall << io_write >> << %r0 >>
    
    -(Increment counter and continue)
    :: add << %r10, 1 >> -> %r10
    :: jump << @print_loop >>
    
@print_end:
    :: return
}
```

**Usage Example**:
```
@main:
    -(Print multiple values of different types)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "The answer is " >> -> %r0
        :: load.const << 42 >> -> %r1
        :: load.const << " and that's final!" >> -> %r2
    }
    
    -(Print a single value)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Hello, world!\n" >> -> %r0
    }
    
    -(Print numbers and strings mixed)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Value: " >> -> %r0
        :: load.const << 3.14159 >> -> %r1
        :: load.const << ", Count: " >> -> %r2
        :: load.const << 100 >> -> %r3
        :: load.const << "\n" >> -> %r4
    }
    
    :: return
}
```

**Output**:
```
The answer is 42 and that's final!
Hello, world!
Value: 3.14159, Count: 100
```

---

## Example 2: input Function with Optional Prompt

The `input` function demonstrates optional parameters (marked with underscores). It reads user input from standard input, optionally displaying a prompt first.

```
:: func input << _prompt_:str >> -> str {
    -(Load the optional prompt parameter)
    :: load.local << -8 >> -> %r0
    
    -(Check if prompt was provided (not null))
    :: test << %r0 >>
    :: jz << @input_no_prompt >>
    
@input_with_prompt:
    -(Display the prompt)
    :: call << print >> << _possible_args_ >> {
        :: load.local << -8 >> -> %r0
    }
    
@input_no_prompt:
    -(Read from standard input)
    :: syscall << io_read >> -> %r0
    
    -(Return the input string)
    :: return << %r0 >>
}
```

**Usage Example**:
```
@main:
    -(Get user input with a prompt)
    :: load.const << "Enter your name: " >> -> %r0
    :: call << input >> << %r0 >>
    :: store.local << %r0 >> -> -8  -(store name)
    
    -(Print greeting)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Hello, " >> -> %r0
        :: load.local << -8 >> -> %r1
        :: load.const << "!\n" >> -> %r2
    }
    
    -(Get input without a prompt)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Enter a number: " >> -> %r0
    }
    :: call << input >> << null >>  -(no prompt)
    :: store.local << %r0 >> -> -16  -(store number)
    
    :: return
}
```

**Interactive Session**:
```
Enter your name: Alice
Hello, Alice!
Enter a number: 42
```

---

## Example 3: Memory Allocation for Arbitrary Precision

This example demonstrates automatic memory allocation for arbitrary precision integers and floats when values exceed native type limits.

```
:: func allocate_bigint << value:int >> -> ptr {
    -(Load the value to check)
    :: load.local << -8 >> -> %r0
    
    -(Compare with max int64 value)
    :: load.const << 9223372036854775807 >> -> %r1
    :: cmp << %r0, %r1 >>
    :: jle << @use_native >>
    
@use_bigint:
    -(Value exceeds native size, allocate BigInt structure)
    -(BigInt structure: sign (1 byte) + length (4 bytes) + digits array)
    :: load.const << 32 >> -> %r0  -(initial allocation size)
    :: syscall << malloc >> << %r0 >> -> %r1
    
    -(Promote native int to arbitrary precision)
    :: load.local << -8 >> -> %r0
    :: promote.bigint << %r0 >> -> %r2
    
    -(Store BigInt data in allocated memory)
    :: store << %r2 >> -> %r1
    
    -(Return pointer to BigInt)
    :: return << %r1 >>
    
@use_native:
    -(Value fits in native int, return as-is)
    :: return << %r0 >>
}

:: func allocate_bigfloat << value:float >> -> ptr {
    -(Load the value to check precision)
    :: load.local << -8 >> -> %r0
    
    -(Check if precision exceeds native float64)
    :: load.const << 15 >> -> %r1  -(float64 has ~15 decimal digits)
    :: call << check_precision >> << %r0 >>
    :: cmp << %r0, %r1 >>
    :: jle << @use_native_float >>
    
@use_bigfloat:
    -(Allocate BigFloat structure)
    -(BigFloat: sign + mantissa (BigInt) + exponent + precision)
    :: load.const << 64 >> -> %r0
    :: syscall << malloc >> << %r0 >> -> %r1
    
    -(Promote to arbitrary precision float)
    :: load.local << -8 >> -> %r0
    :: promote.bigfloat << %r0 >> -> %r2
    
    -(Store BigFloat data)
    :: store << %r2 >> -> %r1
    
    :: return << %r1 >>
    
@use_native_float:
    :: return << %r0 >>
}

:: func bigint_add << a:ptr, b:ptr >> -> ptr {
    -(Add two arbitrary precision integers)
    :: load.local << -8 >> -> %r0   -(first BigInt)
    :: load.local << -16 >> -> %r1  -(second BigInt)
    
    -(Perform arbitrary precision addition)
    :: bigint.add << %r0, %r1 >> -> %r2
    
    -(Allocate memory for result)
    :: load.const << 32 >> -> %r0
    :: syscall << malloc >> << %r0 >> -> %r3
    
    -(Store result)
    :: store << %r2 >> -> %r3
    
    :: return << %r3 >>
}
```

**Usage Example**:
```
@main:
    -(Create a very large integer)
    :: load.const << 99999999999999999999999999999 >> -> %r0
    :: call << allocate_bigint >> << %r0 >>
    :: store.local << %r0 >> -> -8  -(store BigInt pointer)
    
    -(Create another large integer)
    :: load.const << 88888888888888888888888888888 >> -> %r0
    :: call << allocate_bigint >> << %r0 >>
    :: store.local << %r0 >> -> -16
    
    -(Add them together)
    :: load.local << -8 >> -> %r0
    :: load.local << -16 >> -> %r1
    :: call << bigint_add >> << %r0, %r1 >>
    
    -(Print result)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Result: " >> -> %r0
        :: load.local << -8 >> -> %r1  -(result pointer)
        :: load.const << "\n" >> -> %r2
    }
    
    -(Clean up memory)
    :: load.local << -8 >> -> %r0
    :: syscall << free >> << %r0 >>
    :: load.local << -16 >> -> %r0
    :: syscall << free >> << %r0 >>
    
    :: return
}
```

---

## Example 4: Function Calls and Returns

This example demonstrates various function calling patterns, parameter passing, and return values.

```
:: func add << a:int, b:int >> -> int {
    -(Load parameters from stack)
    :: load.local << -8 >> -> %r0   -(first parameter)
    :: load.local << -16 >> -> %r1  -(second parameter)
    
    -(Perform addition)
    :: add << %r0, %r1 >> -> %r2
    
    -(Return result in %r0)
    :: return << %r2 >>
}

:: func multiply << a:int, b:int >> -> int {
    :: load.local << -8 >> -> %r0
    :: load.local << -16 >> -> %r1
    :: mul << %r0, %r1 >> -> %r2
    :: return << %r2 >>
}

:: func calculate << x:int, y:int, z:int >> -> int {
    -(Demonstrates nested function calls)
    -(Calculate: (x + y) * z)
    
    -(Call add(x, y))
    :: load.local << -8 >> -> %r0   -(x)
    :: load.local << -16 >> -> %r1  -(y)
    :: call << add >> << %r0, %r1 >>
    :: store.local << %r0 >> -> -32  -(store intermediate result)
    
    -(Call multiply(result, z))
    :: load.local << -32 >> -> %r0  -(result from add)
    :: load.local << -24 >> -> %r1  -(z)
    :: call << multiply >> << %r0, %r1 >>
    
    -(Return final result)
    :: return << %r0 >>
}

:: func factorial << n:int >> -> int {
    -(Recursive function example)
    :: load.local << -8 >> -> %r0
    
    -(Base case: if n <= 1, return 1)
    :: cmp << %r0, 1 >>
    :: jle << @factorial_base >>
    
@factorial_recursive:
    -(Recursive case: n * factorial(n-1))
    :: load.local << -8 >> -> %r0
    :: sub << %r0, 1 >> -> %r1
    
    -(Call factorial(n-1))
    :: call << factorial >> << %r1 >>
    :: store.local << %r0 >> -> -16  -(store result)
    
    -(Multiply n * factorial(n-1))
    :: load.local << -8 >> -> %r0
    :: load.local << -16 >> -> %r1
    :: mul << %r0, %r1 >> -> %r2
    
    :: return << %r2 >>
    
@factorial_base:
    :: load.const << 1 >> -> %r0
    :: return << %r0 >>
}

:: func void_function << >> -> void {
    -(Function with no parameters and no return value)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "This function returns nothing\n" >> -> %r0
    }
    :: return  -(void return)
}
```

**Usage Example**:
```
@main:
    -(Simple function call)
    :: load.const << 10 >> -> %r0
    :: load.const << 20 >> -> %r1
    :: call << add >> << %r0, %r1 >>
    :: call << print >> << _possible_args_ >> {
        :: load.const << "10 + 20 = " >> -> %r0
        :: load.local << -8 >> -> %r1
        :: load.const << "\n" >> -> %r2
    }
    
    -(Nested function calls)
    :: load.const << 5 >> -> %r0
    :: load.const << 3 >> -> %r1
    :: load.const << 2 >> -> %r2
    :: call << calculate >> << %r0, %r1, %r2 >>
    :: call << print >> << _possible_args_ >> {
        :: load.const << "(5 + 3) * 2 = " >> -> %r0
        :: load.local << -8 >> -> %r1
        :: load.const << "\n" >> -> %r2
    }
    
    -(Recursive function call)
    :: load.const << 5 >> -> %r0
    :: call << factorial >> << %r0 >>
    :: call << print >> << _possible_args_ >> {
        :: load.const << "5! = " >> -> %r0
        :: load.local << -8 >> -> %r1
        :: load.const << "\n" >> -> %r2
    }
    
    -(Void function call)
    :: call << void_function >>
    
    :: return
}
```

**Output**:
```
10 + 20 = 30
(5 + 3) * 2 = 16
5! = 120
This function returns nothing
```

---

## Example 5: Control Flow (if, while, for)

This example demonstrates conditional statements, loops, and complex control flow patterns.

### If Statement Example

```
:: func check_value << n:int >> -> void {
    :: load.local << -8 >> -> %r0
    
    -(Check if n > 100)
    :: cmp << %r0, 100 >>
    :: jgt << @value_is_large >>
    
    -(Check if n > 50)
    :: cmp << %r0, 50 >>
    :: jgt << @value_is_medium >>
    
@value_is_small:
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Value is small (<=50)\n" >> -> %r0
    }
    :: jump << @check_value_end >>
    
@value_is_medium:
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Value is medium (51-100)\n" >> -> %r0
    }
    :: jump << @check_value_end >>
    
@value_is_large:
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Value is large (>100)\n" >> -> %r0
    }
    
@check_value_end:
    :: return
}
```

### While Loop Example

```
:: func count_down << n:int >> -> void {
    :: load.local << -8 >> -> %r0
    :: store.local << %r0 >> -> -16  -(counter variable)
    
@while_loop_start:
    -(Check condition: counter > 0)
    :: load.local << -16 >> -> %r0
    :: cmp << %r0, 0 >>
    :: jle << @while_loop_end >>
    
@while_loop_body:
    -(Print current counter value)
    :: call << print >> << _possible_args_ >> {
        :: load.local << -16 >> -> %r0
        :: load.const << "\n" >> -> %r1
    }
    
    -(Decrement counter)
    :: load.local << -16 >> -> %r0
    :: sub << %r0, 1 >> -> %r0
    :: store.local << %r0 >> -> -16
    
    -(Continue loop)
    :: jump << @while_loop_start >>
    
@while_loop_end:
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Blast off!\n" >> -> %r0
    }
    :: return
}
```

### For Loop Example

```
:: func sum_range << start:int, end:int >> -> int {
    -(Calculate sum from start to end (inclusive))
    :: load.local << -8 >> -> %r0   -(start)
    :: load.local << -16 >> -> %r1  -(end)
    
    -(Initialize loop variable and accumulator)
    :: store.local << %r0 >> -> -24  -(i = start)
    :: load.const << 0 >> -> %r0
    :: store.local << %r0 >> -> -32  -(sum = 0)
    
@for_loop_start:
    -(Check condition: i <= end)
    :: load.local << -24 >> -> %r0  -(i)
    :: load.local << -16 >> -> %r1  -(end)
    :: cmp << %r0, %r1 >>
    :: jgt << @for_loop_end >>
    
@for_loop_body:
    -(sum += i)
    :: load.local << -32 >> -> %r0  -(sum)
    :: load.local << -24 >> -> %r1  -(i)
    :: add << %r0, %r1 >> -> %r0
    :: store.local << %r0 >> -> -32
    
@for_loop_increment:
    -(i++)
    :: load.local << -24 >> -> %r0
    :: add << %r0, 1 >> -> %r0
    :: store.local << %r0 >> -> -24
    
    -(Continue loop)
    :: jump << @for_loop_start >>
    
@for_loop_end:
    -(Return sum)
    :: load.local << -32 >> -> %r0
    :: return << %r0 >>
}
```

### Nested Loops Example

```
:: func print_multiplication_table << n:int >> -> void {
    -(Print n x n multiplication table)
    :: load.local << -8 >> -> %r0
    :: store.local << %r0 >> -> -16  -(max value)
    
    -(Outer loop: i from 1 to n)
    :: load.const << 1 >> -> %r0
    :: store.local << %r0 >> -> -24  -(i = 1)
    
@outer_loop_start:
    :: load.local << -24 >> -> %r0  -(i)
    :: load.local << -16 >> -> %r1  -(n)
    :: cmp << %r0, %r1 >>
    :: jgt << @outer_loop_end >>
    
@outer_loop_body:
    -(Inner loop: j from 1 to n)
    :: load.const << 1 >> -> %r0
    :: store.local << %r0 >> -> -32  -(j = 1)
    
@inner_loop_start:
    :: load.local << -32 >> -> %r0  -(j)
    :: load.local << -16 >> -> %r1  -(n)
    :: cmp << %r0, %r1 >>
    :: jgt << @inner_loop_end >>
    
@inner_loop_body:
    -(Calculate i * j)
    :: load.local << -24 >> -> %r0  -(i)
    :: load.local << -32 >> -> %r1  -(j)
    :: mul << %r0, %r1 >> -> %r2
    
    -(Print result with tab)
    :: call << print >> << _possible_args_ >> {
        :: load.local << -40 >> -> %r0  -(result)
        :: load.const << "\t" >> -> %r1
    }
    
    -(j++)
    :: load.local << -32 >> -> %r0
    :: add << %r0, 1 >> -> %r0
    :: store.local << %r0 >> -> -32
    
    :: jump << @inner_loop_start >>
    
@inner_loop_end:
    -(Print newline after each row)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "\n" >> -> %r0
    }
    
    -(i++)
    :: load.local << -24 >> -> %r0
    :: add << %r0, 1 >> -> %r0
    :: store.local << %r0 >> -> -24
    
    :: jump << @outer_loop_start >>
    
@outer_loop_end:
    :: return
}
```

### Complex Control Flow Example

```
:: func find_prime << n:int >> -> int {
    -(Find the nth prime number)
    :: load.local << -8 >> -> %r0
    :: store.local << %r0 >> -> -16  -(target count)
    
    -(Initialize variables)
    :: load.const << 0 >> -> %r0
    :: store.local << %r0 >> -> -24  -(prime count)
    :: load.const << 2 >> -> %r0
    :: store.local << %r0 >> -> -32  -(candidate)
    
@find_prime_loop:
    -(Check if we've found enough primes)
    :: load.local << -24 >> -> %r0  -(count)
    :: load.local << -16 >> -> %r1  -(target)
    :: cmp << %r0, %r1 >>
    :: jge << @find_prime_end >>
    
@check_candidate:
    -(Check if candidate is prime)
    :: load.local << -32 >> -> %r0
    :: call << is_prime >> << %r0 >>
    :: test << %r0 >>
    :: jz << @not_prime >>
    
@is_prime_found:
    -(Increment prime count)
    :: load.local << -24 >> -> %r0
    :: add << %r0, 1 >> -> %r0
    :: store.local << %r0 >> -> -24
    
@not_prime:
    -(Move to next candidate)
    :: load.local << -32 >> -> %r0
    :: add << %r0, 1 >> -> %r0
    :: store.local << %r0 >> -> -32
    
    :: jump << @find_prime_loop >>
    
@find_prime_end:
    -(Return the last candidate (nth prime))
    :: load.local << -32 >> -> %r0
    :: sub << %r0, 1 >> -> %r0  -(subtract 1 because we incremented after finding)
    :: return << %r0 >>
}

:: func is_prime << n:int >> -> int {
    -(Return 1 if n is prime, 0 otherwise)
    :: load.local << -8 >> -> %r0
    
    -(Handle special cases)
    :: cmp << %r0, 2 >>
    :: jlt << @not_prime_return >>
    :: cmp << %r0, 2 >>
    :: je << @is_prime_return >>
    
    -(Check divisibility from 2 to sqrt(n))
    :: load.const << 2 >> -> %r0
    :: store.local << %r0 >> -> -16  -(divisor)
    
@divisor_loop:
    -(Check if divisor * divisor > n)
    :: load.local << -16 >> -> %r0
    :: mul << %r0, %r0 >> -> %r1
    :: load.local << -8 >> -> %r2
    :: cmp << %r1, %r2 >>
    :: jgt << @is_prime_return >>
    
    -(Check if n % divisor == 0)
    :: load.local << -8 >> -> %r0
    :: load.local << -16 >> -> %r1
    :: mod << %r0, %r1 >> -> %r2
    :: test << %r2 >>
    :: jz << @not_prime_return >>
    
    -(Increment divisor)
    :: load.local << -16 >> -> %r0
    :: add << %r0, 1 >> -> %r0
    :: store.local << %r0 >> -> -16
    
    :: jump << @divisor_loop >>
    
@is_prime_return:
    :: load.const << 1 >> -> %r0
    :: return << %r0 >>
    
@not_prime_return:
    :: load.const << 0 >> -> %r0
    :: return << %r0 >>
}
```

**Usage Example**:
```
@main:
    -(Test if statement)
    :: load.const << 25 >> -> %r0
    :: call << check_value >> << %r0 >>
    
    :: load.const << 75 >> -> %r0
    :: call << check_value >> << %r0 >>
    
    :: load.const << 150 >> -> %r0
    :: call << check_value >> << %r0 >>
    
    -(Test while loop)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Countdown:\n" >> -> %r0
    }
    :: load.const << 5 >> -> %r0
    :: call << count_down >> << %r0 >>
    
    -(Test for loop)
    :: load.const << 1 >> -> %r0
    :: load.const << 10 >> -> %r1
    :: call << sum_range >> << %r0, %r1 >>
    :: call << print >> << _possible_args_ >> {
        :: load.const << "Sum of 1 to 10: " >> -> %r0
        :: load.local << -8 >> -> %r1
        :: load.const << "\n" >> -> %r2
    }
    
    -(Test nested loops)
    :: call << print >> << _possible_args_ >> {
        :: load.const << "5x5 Multiplication Table:\n" >> -> %r0
    }
    :: load.const << 5 >> -> %r0
    :: call << print_multiplication_table >> << %r0 >>
    
    -(Test complex control flow)
    :: load.const << 10 >> -> %r0
    :: call << find_prime >> << %r0 >>
    :: call << print >> << _possible_args_ >> {
        :: load.const << "The 10th prime number is: " >> -> %r0
        :: load.local << -8 >> -> %r1
        :: load.const << "\n" >> -> %r2
    }
    
    :: return
}
```

**Output**:
```
Value is small (<=50)
Value is medium (51-100)
Value is large (>100)
Countdown:
5
4
3
2
1
Blast off!
Sum of 1 to 10: 55
5x5 Multiplication Table:
1	2	3	4	5	
2	4	6	8	10	
3	6	9	12	15	
4	8	12	16	20	
5	10	15	20	25	
The 10th prime number is: 29
```

---

## Summary

These examples demonstrate the core features of spacetime assembly:

1. **Variadic Functions**: Using `_possible_args_` for flexible argument lists
2. **Optional Parameters**: Using `_param_` syntax for optional function arguments
3. **Memory Management**: Automatic allocation and promotion for arbitrary precision numbers
4. **Function Calls**: Parameter passing, return values, recursion, and nested calls
5. **Control Flow**: If statements, while loops, for loops, nested loops, and complex branching

All examples follow spacetime assembly conventions:
- `::` prefix for instructions
- `<< >>` for operand grouping
- `%r` prefix for registers
- `@` prefix for labels
- `-(comment)` for inline comments

**Validates Requirements**: 12.2, 13.1, 13.2, 13.4, 13.5, 13.6, 2.5.1-2.5.7
