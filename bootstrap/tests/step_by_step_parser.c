/*
 * Step by step parser test to isolate the failing function
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// Token structure (must match lexer.asm layout exactly)
typedef struct __attribute__((packed)) {
    int type;           // 4 bytes
    char* value_ptr;    // 8 bytes  
    int value_len;      // 4 bytes
    int line;           // 4 bytes
    int column;         // 4 bytes
    long padding;       // 8 bytes for alignment
} Token;

// Token type constants
#define TOKEN_IDENTIFIER    0
#define TOKEN_NUMBER        1
#define TOKEN_STRING        2
#define TOKEN_KEYWORD       3
#define TOKEN_OPERATOR      4
#define TOKEN_DELIMITER     5
#define TOKEN_EOF          6

// External functions
extern void lexer_init(char* source, int length);
extern Token* lexer_next_token();
extern void lexer_destroy();

// Individual parser functions to test
extern void parser_init(Token* tokens);
extern void parser_destroy();
extern void scope_enter();
extern void scope_exit();

int main() {
    char* program = "func main() { return 0 }";
    
    printf("Step-by-step parser test: '%s'\n", program);
    printf("===================\n");
    
    // Step 1: Tokenize
    lexer_init(program, strlen(program));
    
    Token tokens[100];
    int token_count = 0;
    Token* token;
    
    do {
        token = lexer_next_token();
        if (!token) break;
        tokens[token_count] = *token;
        token_count++;
    } while (token && token->type != TOKEN_EOF && token_count < 100);
    
    lexer_destroy();
    printf("Step 1: Tokenized %d tokens - OK\n", token_count);
    
    // Step 2: Initialize parser
    printf("Step 2: Initializing parser...\n");
    parser_init(tokens);
    printf("Step 2: Parser initialized - OK\n");
    
    // Step 3: Test scope functions
    printf("Step 3: Testing scope functions...\n");
    scope_enter();
    printf("Step 3a: scope_enter() - OK\n");
    scope_exit();
    printf("Step 3b: scope_exit() - OK\n");
    
    // Step 4: Cleanup
    printf("Step 4: Cleaning up...\n");
    parser_destroy();
    printf("Step 4: Parser destroyed - OK\n");
    
    printf("\nAll basic functions work. The issue must be in the parsing logic itself.\n");
    return 0;
}