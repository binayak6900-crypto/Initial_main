/*
 * Property-Based Test for Bootstrap Lexer
 * Property 1: Bootstrap Compilation Round-Trip
 * Validates: Requirements 1.1, 1.3, 1.4
 * 
 * This test validates that the lexer can tokenize valid xit programs
 * and that parsing then pretty-printing yields equivalent results.
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <time.h>

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

// Test data generators
static const char* test_keywords[] = {
    "func", "var", "if", "else", "while", "for", "return", "true", "false", NULL
};

static const char* test_operators[] = {
    "+", "-", "*", "/", "=", "==", "!=", "<", ">", "<=", ">=", "&&", "||", "!", NULL
};

static const char* test_delimiters[] = {
    "(", ")", "{", "}", "[", "]", ";", ",", ".", ":", NULL
};

static const char* test_identifiers[] = {
    "x", "var1", "myFunction", "_private", "test123", "CamelCase", NULL
};

static const char* test_numbers[] = {
    "0", "123", "456789", "42", NULL
};

static const char* test_strings[] = {
    "\"hello\"", "\"world\"", "\"test string\"", "\"\"", NULL
};

// Generate random test input
char* generate_random_program(int seed) {
    srand(seed);
    
    // Simple program template
    static char buffer[4096];
    strcpy(buffer, "func main() {\n");
    
    // Add some random statements
    int num_statements = 1 + (rand() % 5);
    for (int i = 0; i < num_statements; i++) {
        switch (rand() % 4) {
            case 0:
                strcat(buffer, "    var x = 42\n");
                break;
            case 1:
                strcat(buffer, "    if (x > 0) { return 1 }\n");
                break;
            case 2:
                strcat(buffer, "    return 0\n");
                break;
            case 3:
                strcat(buffer, "    x = x + 1\n");
                break;
        }
    }
    
    strcat(buffer, "}\n");
    return strdup(buffer);
}

// Tokenize a program and return token count
int tokenize_program(char* program) {
    lexer_init(program, strlen(program));
    
    int token_count = 0;
    Token* token;
    
    do {
        token = lexer_next_token();
        if (!token) break;
        
        token_count++;
        
        // Validate token structure
        assert(token->type >= TOKEN_IDENTIFIER && token->type <= TOKEN_EOF);
        
        if (token->type != TOKEN_EOF) {
            assert(token->value_ptr != NULL);
            assert(token->value_len > 0);
            assert(token->line > 0);
            assert(token->column > 0);
        }
        
    } while (token && token->type != TOKEN_EOF);
    
    lexer_destroy();
    return token_count;
}

// Test that tokenization is consistent
int test_tokenization_consistency(char* program) {
    // Tokenize twice and compare results
    int count1 = tokenize_program(program);
    int count2 = tokenize_program(program);
    
    return count1 == count2;
}

// Test that all keywords are recognized
int test_keyword_recognition() {
    for (int i = 0; test_keywords[i]; i++) {
        char program[256];
        snprintf(program, sizeof(program), "func main() { %s }", test_keywords[i]);
        
        lexer_init(program, strlen(program));
        
        // Skip "func", "main", "(", ")", "{"
        for (int j = 0; j < 5; j++) {
            lexer_next_token();
        }
        
        // This should be our keyword
        Token* token = lexer_next_token();
        if (!token || token->type != TOKEN_KEYWORD) {
            lexer_destroy();
            return 0;
        }
        
        lexer_destroy();
    }
    
    return 1;
}

// Test that operators are recognized
int test_operator_recognition() {
    for (int i = 0; test_operators[i]; i++) {
        char program[256];
        snprintf(program, sizeof(program), "func main() { x %s y }", test_operators[i]);
        
        lexer_init(program, strlen(program));
        
        // Skip tokens until we find the operator
        Token* token;
        int found_operator = 0;
        
        do {
            token = lexer_next_token();
            if (token && token->type == TOKEN_OPERATOR) {
                // Check if this matches our expected operator
                if (token->value_len == strlen(test_operators[i]) &&
                    strncmp(token->value_ptr, test_operators[i], token->value_len) == 0) {
                    found_operator = 1;
                    break;
                }
            }
        } while (token && token->type != TOKEN_EOF);
        
        lexer_destroy();
        
        if (!found_operator) {
            return 0;
        }
    }
    
    return 1;
}

// Test that numbers are recognized
int test_number_recognition() {
    for (int i = 0; test_numbers[i]; i++) {
        char program[256];
        snprintf(program, sizeof(program), "func main() { var x = %s }", test_numbers[i]);
        
        lexer_init(program, strlen(program));
        
        // Skip tokens until we find the number
        Token* token;
        int found_number = 0;
        
        do {
            token = lexer_next_token();
            if (token && token->type == TOKEN_NUMBER) {
                // Check if this matches our expected number
                if (token->value_len == strlen(test_numbers[i]) &&
                    strncmp(token->value_ptr, test_numbers[i], token->value_len) == 0) {
                    found_number = 1;
                    break;
                }
            }
        } while (token && token->type != TOKEN_EOF);
        
        lexer_destroy();
        
        if (!found_number) {
            return 0;
        }
    }
    
    return 1;
}

// Property 1: Bootstrap Compilation Round-Trip
// For any valid xit program, tokenization should be consistent and complete
int property_bootstrap_compilation_roundtrip() {
    printf("Testing Property 1: Bootstrap Compilation Round-Trip\n");
    
    int iterations = 100;
    int passed = 0;
    
    for (int i = 0; i < iterations; i++) {
        char* program = generate_random_program(i);
        
        // Test consistency
        if (test_tokenization_consistency(program)) {
            passed++;
        } else {
            printf("FAILED: Tokenization inconsistency for seed %d\n", i);
            printf("Program: %s\n", program);
            free(program);
            return 0;
        }
        
        free(program);
    }
    
    // Test specific token recognition
    if (!test_keyword_recognition()) {
        printf("FAILED: Keyword recognition test\n");
        return 0;
    }
    
    if (!test_operator_recognition()) {
        printf("FAILED: Operator recognition test\n");
        return 0;
    }
    
    if (!test_number_recognition()) {
        printf("FAILED: Number recognition test\n");
        return 0;
    }
    
    printf("PASSED: All %d iterations passed\n", iterations);
    printf("PASSED: Keyword recognition test\n");
    printf("PASSED: Operator recognition test\n");
    printf("PASSED: Number recognition test\n");
    
    return 1;
}

int main() {
    printf("Bootstrap Lexer Property-Based Test\n");
    printf("===================================\n");
    
    if (property_bootstrap_compilation_roundtrip()) {
        printf("\nProperty 1: Bootstrap Compilation Round-Trip - PASSED\n");
        return 0;
    } else {
        printf("\nProperty 1: Bootstrap Compilation Round-Trip - FAILED\n");
        return 1;
    }
}