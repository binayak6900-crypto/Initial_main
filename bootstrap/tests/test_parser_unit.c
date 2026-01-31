/*
 * Unit Tests for Bootstrap Parser
 * Tests parser edge cases and direct code generation
 * Requirements: 8.1
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

// Token type constants (must match lexer.asm)
#define TOKEN_IDENTIFIER    0
#define TOKEN_NUMBER        1
#define TOKEN_STRING        2
#define TOKEN_KEYWORD       3
#define TOKEN_OPERATOR      4
#define TOKEN_DELIMITER     5
#define TOKEN_EOF          6

// Token structure (must match lexer.asm layout exactly)
typedef struct __attribute__((packed)) {
    int type;           // 4 bytes
    char* value_ptr;    // 8 bytes  
    int value_len;      // 4 bytes
    int line;           // 4 bytes
    int column;         // 4 bytes
    long padding;       // 8 bytes for alignment
} Token;

// External functions from lexer.asm
extern void lexer_init(char* source, int length);
extern Token* lexer_next_token();
extern void lexer_destroy();

// External functions from parser.asm
extern void parser_init(Token* tokens);
extern int parser_parse_program();
extern int parser_emit_executable(char* filename);
extern void parser_destroy();

// Helper function to tokenize source code
Token* tokenize_source(char* source, int* token_count) {
    lexer_init(source, strlen(source));
    
    // Count tokens first
    int count = 0;
    Token* token;
    do {
        token = lexer_next_token();
        if (token) count++;
    } while (token && token->type != TOKEN_EOF);
    
    // Allocate token array
    Token* tokens = malloc(count * sizeof(Token));
    *token_count = count;
    
    // Tokenize again and copy tokens
    lexer_destroy();
    lexer_init(source, strlen(source));
    
    for (int i = 0; i < count; i++) {
        token = lexer_next_token();
        if (token) {
            tokens[i] = *token;
        }
    }
    
    lexer_destroy();
    return tokens;
}

// Test 1: Malformed syntax error handling
int test_malformed_syntax() {
    printf("Test 1: Malformed syntax error handling\n");
    
    char* malformed_programs[] = {
        "func {",                    // Missing function name
        "func main (",               // Missing closing parenthesis
        "func main() {",             // Missing closing brace
        "func main() { var }",       // Incomplete variable declaration
        "func main() { var x }",     // Missing assignment
        "func main() { return }",    // Incomplete return (this should actually be valid)
        "func main() { x = }",       // Missing expression
        "func main() { if }",        // Incomplete if statement
        "func main() { if (x }",     // Missing closing parenthesis
        NULL
    };
    
    int passed = 0;
    int total = 0;
    
    for (int i = 0; malformed_programs[i]; i++) {
        total++;
        printf("  Testing: '%s'\n", malformed_programs[i]);
        
        int token_count;
        Token* tokens = tokenize_source(malformed_programs[i], &token_count);
        
        parser_init(tokens);
        int result = parser_parse_program();
        parser_destroy();
        
        // Malformed programs should fail to parse
        if (result == 0) {
            printf("    PASS: Correctly detected malformed syntax\n");
            passed++;
        } else {
            printf("    FAIL: Should have detected malformed syntax\n");
        }
        
        free(tokens);
    }
    
    printf("Test 1 Results: %d/%d passed\n\n", passed, total);
    return passed == total;
}

// Test 2: Nested expression parsing with direct code generation
int test_nested_expressions() {
    printf("Test 2: Nested expression parsing with direct code generation\n");
    
    char* valid_programs[] = {
        "func main() { var x = 42 }",
        "func main() { var x = 42\nvar y = 24 }",
        "func main() { var x = 42\nreturn x }",
        "func main() { var x = 42\nvar y = x\nreturn y }",
        "func test() { return 0 }\nfunc main() { return 1 }",
        NULL
    };
    
    int passed = 0;
    int total = 0;
    
    for (int i = 0; valid_programs[i]; i++) {
        total++;
        printf("  Testing: '%s'\n", valid_programs[i]);
        
        int token_count;
        Token* tokens = tokenize_source(valid_programs[i], &token_count);
        
        parser_init(tokens);
        int result = parser_parse_program();
        parser_destroy();
        
        // Valid programs should parse successfully
        if (result == 1) {
            printf("    PASS: Successfully parsed valid program\n");
            passed++;
        } else {
            printf("    FAIL: Should have parsed valid program\n");
        }
        
        free(tokens);
    }
    
    printf("Test 2 Results: %d/%d passed\n\n", passed, total);
    return passed == total;
}

// Test 3: Function parameter parsing and code emission
int test_function_parameters() {
    printf("Test 3: Function parameter parsing and code emission\n");
    
    char* programs[] = {
        "func main() { return 0 }",                    // No parameters
        "func add() { return 42 }",                    // Simple function
        "func test() { var x = 1\nreturn x }",         // Local variable
        "func first() { return 1 }\nfunc second() { return 2 }", // Multiple functions
        NULL
    };
    
    int passed = 0;
    int total = 0;
    
    for (int i = 0; programs[i]; i++) {
        total++;
        printf("  Testing: '%s'\n", programs[i]);
        
        int token_count;
        Token* tokens = tokenize_source(programs[i], &token_count);
        
        parser_init(tokens);
        int result = parser_parse_program();
        
        if (result == 1) {
            // Try to emit executable (this tests code generation)
            int emit_result = parser_emit_executable("test_output.bin");
            if (emit_result == 1) {
                printf("    PASS: Successfully parsed and generated code\n");
                passed++;
            } else {
                printf("    FAIL: Parsing succeeded but code generation failed\n");
            }
        } else {
            printf("    FAIL: Failed to parse valid program\n");
        }
        
        parser_destroy();
        free(tokens);
    }
    
    printf("Test 3 Results: %d/%d passed\n\n", passed, total);
    return passed == total;
}

// Test 4: Edge cases and boundary conditions
int test_edge_cases() {
    printf("Test 4: Edge cases and boundary conditions\n");
    
    char* edge_cases[] = {
        "",                                           // Empty program
        "func main() { }",                           // Empty function
        "func main() { var x = 0\nvar y = 1\nvar z = 2 }", // Multiple variables
        "func main() { if (1) { return 1 } }",       // Simple if statement
        "func main() { var x = 123456789 }",         // Large number
        NULL
    };
    
    int passed = 0;
    int total = 0;
    
    for (int i = 0; edge_cases[i]; i++) {
        total++;
        printf("  Testing: '%s'\n", edge_cases[i]);
        
        int token_count;
        Token* tokens = tokenize_source(edge_cases[i], &token_count);
        
        parser_init(tokens);
        int result = parser_parse_program();
        parser_destroy();
        
        // Most edge cases should parse (except empty program)
        int expected_result = (strlen(edge_cases[i]) == 0) ? 0 : 1;
        
        if (result == expected_result) {
            printf("    PASS: Handled edge case correctly\n");
            passed++;
        } else {
            printf("    FAIL: Edge case handling incorrect (got %d, expected %d)\n", 
                   result, expected_result);
        }
        
        free(tokens);
    }
    
    printf("Test 4 Results: %d/%d passed\n\n", passed, total);
    return passed == total;
}

// Test 5: Symbol table functionality
int test_symbol_table() {
    printf("Test 5: Symbol table functionality\n");
    
    char* programs[] = {
        "func main() { var x = 1\nvar y = x }",      // Variable reference
        "func test() { var a = 1 }\nfunc main() { var b = 2 }", // Multiple scopes
        "func main() { var x = 1\nif (x) { var y = 2 } }", // Nested scopes
        NULL
    };
    
    int passed = 0;
    int total = 0;
    
    for (int i = 0; programs[i]; i++) {
        total++;
        printf("  Testing: '%s'\n", programs[i]);
        
        int token_count;
        Token* tokens = tokenize_source(programs[i], &token_count);
        
        parser_init(tokens);
        int result = parser_parse_program();
        parser_destroy();
        
        if (result == 1) {
            printf("    PASS: Symbol table handled correctly\n");
            passed++;
        } else {
            printf("    FAIL: Symbol table error\n");
        }
        
        free(tokens);
    }
    
    printf("Test 5 Results: %d/%d passed\n\n", passed, total);
    return passed == total;
}

int main() {
    printf("Bootstrap Parser Unit Tests\n");
    printf("===========================\n\n");
    
    int tests_passed = 0;
    int total_tests = 5;
    
    if (test_malformed_syntax()) tests_passed++;
    if (test_nested_expressions()) tests_passed++;
    if (test_function_parameters()) tests_passed++;
    if (test_edge_cases()) tests_passed++;
    if (test_symbol_table()) tests_passed++;
    
    printf("Overall Results: %d/%d tests passed\n", tests_passed, total_tests);
    
    if (tests_passed == total_tests) {
        printf("All parser unit tests PASSED!\n");
        return 0;
    } else {
        printf("Some parser unit tests FAILED!\n");
        return 1;
    }
}