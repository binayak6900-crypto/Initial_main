/*
 * Detailed debug test for parser
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

const char* token_type_names[] = {
    "IDENTIFIER", "NUMBER", "STRING", "KEYWORD", "OPERATOR", "DELIMITER", "EOF"
};

// External functions
extern void lexer_init(char* source, int length);
extern Token* lexer_next_token();
extern void lexer_destroy();

// Let's test the individual parser functions
extern void parser_init(Token* tokens);
extern void parser_destroy();

// Test just the symbol table functions
extern void symbol_table_init();
extern int symbol_table_add(char* name, int symbol_type, int data_type);
extern int symbol_table_lookup(char* name);

int main() {
    printf("Testing parser components individually...\n");
    
    // Test 1: Symbol table
    printf("\n1. Testing symbol table:\n");
    symbol_table_init();
    
    char* test_name = "test_var";
    int result = symbol_table_add(test_name, 0, 0);  // SYMBOL_VARIABLE, TYPE_INT
    printf("   Added symbol '%s': result = %d\n", test_name, result);
    
    int lookup_result = symbol_table_lookup(test_name);
    printf("   Lookup symbol '%s': result = %d\n", test_name, lookup_result);
    
    // Test 2: Simple tokenization and parser init
    printf("\n2. Testing parser initialization:\n");
    char* program = "func main() { return 0 }";
    
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
    
    printf("   Tokenized %d tokens\n", token_count);
    
    // Initialize parser
    parser_init(tokens);
    printf("   Parser initialized\n");
    
    // Test 3: Check if we can access tokens through parser
    printf("\n3. Testing token access:\n");
    printf("   First token should be 'func' (type 3)\n");
    printf("   Token 0: type=%d, value_len=%d\n", tokens[0].type, tokens[0].value_len);
    
    if (tokens[0].value_ptr && tokens[0].value_len > 0) {
        printf("   Token 0 value: '");
        for (int i = 0; i < tokens[0].value_len; i++) {
            printf("%c", tokens[0].value_ptr[i]);
        }
        printf("'\n");
    }
    
    parser_destroy();
    
    printf("\nBasic component tests completed.\n");
    return 0;
}