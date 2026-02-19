# mc² Lexer

This directory contains the lexer (tokenizer) implementation for the mc² programming language, written in spacetime assembly.

## Overview

The lexer is responsible for converting source code text into a stream of tokens that can be parsed by the parser. It recognizes all mc² syntax elements including:

- Keywords (type, namespace, function, if, while, import, etc.)
- Identifiers (variable and function names)
- Operators (::, <<, >>, &&, ||, **, +, -, *, /, etc.)
- Literals (integers, floats, strings)
- Symbols ($, %, !, @, #, _, -)
- Delimiters ({, }, (, ), ,)
- Comments (-(content) and -{...})

## Files

### token.spacetime

Defines the Token data structure and token type enumeration.

**Token Types**:
- `TOKEN_TYPE_KEYWORD` (0) - Language keywords
- `TOKEN_TYPE_IDENTIFIER` (1) - Variable/function names
- `TOKEN_TYPE_OPERATOR` (2) - Operators
- `TOKEN_TYPE_LITERAL` (3) - Literal values
- `TOKEN_TYPE_SYMBOL` (4) - Special symbols
- `TOKEN_TYPE_DELIMITER` (5) - Delimiters
- `TOKEN_TYPE_COMMENT` (6) - Comments (stripped during parsing)

**Token Structure** (36 bytes):
```
Offset 0:  type (int, 4 bytes)
Offset 4:  lexeme_ptr (pointer, 8 bytes)
Offset 12: lexeme_len (int, 4 bytes)
Offset 16: line (int, 4 bytes)
Offset 20: column (int, 4 bytes)
Offset 24: file_ptr (pointer, 8 bytes)
Offset 32: file_len (int, 4 bytes)
```

**Functions**:
- `token_create(type, lexeme, line, column, file) -> ptr` - Create a new token
- `token_get_type(token) -> int` - Get token type
- `token_get_lexeme(token) -> str` - Get token lexeme
- `token_get_line(token) -> int` - Get token line number
- `token_get_column(token) -> int` - Get token column number
- `token_get_file(token) -> str` - Get token source file
- `token_free(token)` - Free token memory
- `token_type_name(type) -> str` - Get human-readable type name
- `token_print(token)` - Print token for debugging
- `token_init()` - Initialize token system

**String Utilities**:
- `string_ptr(str) -> ptr` - Get pointer to string data
- `string_len(str) -> int` - Get string length
- `string_from_ptr(ptr, len) -> str` - Create string from pointer and length

## Usage Example

```spacetime
-(Initialize token system)
:: call << token_init >>

-(Create a token)
:: load.global << $TOKEN_TYPE_KEYWORD >> -> %r0
:: load.const << "function" >> -> %r1
:: load.const << 1 >> -> %r2  -(line)
:: load.const << 0 >> -> %r3  -(column)
:: load.const << "test.mc²" >> -> %r4
:: call << token_create >> << %r0, %r1, %r2, %r3, %r4 >> -> %r5

-(Print the token)
:: call << token_print >> << %r5 >>

-(Free the token)
:: call << token_free >> << %r5 >>
```

## Implementation Notes

1. **Memory Management**: Tokens are allocated on the heap and must be freed with `token_free()` to prevent memory leaks.

2. **String Handling**: Token lexemes and filenames are stored as pointers with lengths. The string utility functions handle low-level string operations.

3. **Assembly-First Approach**: All token operations are implemented in spacetime assembly, following the mc² philosophy of building everything from assembly up.

4. **Hash-Based Variables**: When the lexer creates tokens for variables, it will generate hash-based internal names (e.g., `__var_x_a3f2b1__`) to avoid collisions with function arguments.

5. **Comment Handling**: Comments are tokenized but will be stripped during parsing (Property 6: Comment Elimination).

## Requirements Validated

- **Requirement 5.1**: Self-hosted compiler written in mc² using assembly
- **Requirement 5.2**: Uses spacetime assembly for core functionality
- **Design Token Structure**: Implements Token with type, lexeme, line, column, file

### lexer.spacetime

Implements the lexer state machine and core lexer functions.

**Lexer Structure** (36 bytes):
```
Offset 0:  source_ptr (pointer, 8 bytes)
Offset 8:  source_len (int, 4 bytes)
Offset 12: position (int, 4 bytes)
Offset 16: line (int, 4 bytes)
Offset 20: column (int, 4 bytes)
Offset 24: file_ptr (pointer, 8 bytes)
Offset 32: file_len (int, 4 bytes)
```

**Functions**:
- `lexer_create(source, filename) -> ptr` - Create a new lexer
- `lexer_at_end(lexer) -> int` - Check if at end of source
- `lexer_peek(lexer) -> int` - Peek current character
- `lexer_advance(lexer) -> int` - Advance to next character
- `lexer_skip_whitespace(lexer)` - Skip whitespace characters
- `is_alpha(char) -> int` - Check if alphabetic
- `is_digit(char) -> int` - Check if digit
- `is_alnum(char) -> int` - Check if alphanumeric
- `is_keyword(str) -> int` - Check if string is a keyword
- `hash_variable_name(name) -> int` - Generate hash for variable
- `create_hash_variable(name) -> str` - Create hash-based variable name
- `lexer_init()` - Initialize lexer system

**Keywords Recognized**:
type, namespace, classdef, function, if, else, while, for, import, from, get, return, True, False, Null, int, float, str, void, ptr

### tokenize.spacetime

Implements token recognition for all mc² syntax elements.

**Functions**:
- `lexer_tokenize_identifier(lexer) -> ptr` - Tokenize identifier/keyword
- `lexer_tokenize_number(lexer) -> ptr` - Tokenize number literal
- `lexer_tokenize_string(lexer) -> ptr` - Tokenize string literal
- `lexer_tokenize_operator(lexer) -> ptr` - Tokenize operator/symbol
- `lexer_tokenize_delimiter(lexer) -> ptr` - Tokenize delimiter
- `lexer_next_token(lexer) -> ptr` - Get next token (main function)

**Operators Recognized**:
- `::` - Line start marker (double colon)
- `<<` `>>` - Parameter brackets
- `&&` - Logical AND
- `||` - Logical OR
- `**` - Exponentiation
- `==` `!=` - Equality/inequality
- `<` `>` `<=` `>=` - Comparison
- `+` `-` `*` `/` `%` - Arithmetic
- `=` - Assignment
- `@` `#` `$` `!` - Special symbols

**Delimiters Recognized**:
`(` `)` `{` `}` `,`

### utils.spacetime

Utility functions for string operations.

**Functions**:
- `string_equals(str1, str2) -> int` - Compare strings for equality
- `int_to_hex(value) -> str` - Convert integer to hexadecimal string
- `string_concat(str1, str2) -> str` - Concatenate two strings

## Testing

Run tokenization tests:
```bash
# Compile and run test
mc2 tests/test_tokenize.spacetime -o tests/test_tokenize.e²
mc2 tests/test_tokenize.e²
```

## Next Steps

1. ~~Implement tokenization logic (task 9.2)~~ ✓ Complete
2. Implement comment stripping (task 9.3)
3. Write property tests for lexer (task 9.4)
4. Update a.mc² with lexer examples (task 9.5)

## See Also

- [Spacetime Assembly Reference](../../docs/spacetime-assembly.md)
- [mc² Syntax Reference](../../docs/syntax-reference.md)
- [Design Document](.kiro/specs/mc2-language/design.md)
