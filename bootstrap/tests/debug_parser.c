/*
 * Debug test for parser to understand what's failing
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
extern void parser_init(Token* tokens);
extern int parser_parse_program();
extern void parser_destroy();

int main() {
    char* program = "func main() { return 0 }";
    
    printf("Testing program: '%s'\n", program);
    printf("===================\n");
    
    // First, let's see what tokens we get
    lexer_init(program, strlen(program));
    
    printf("Tokens:\n");
    Token* token;
    int token_count = 0;
    Token tokens[100];  // Static array for simplicity
    
    do {
        token = lexer_next_token();
        if (!token) break;
        
        tokens[token_count] = *token;
        
        printf("Token %d: type=%s (%d), line=%d, column=%d, len=%d", 
               token_count, token_type_names[token->type], token->type, 
               token->line, token->column, token->value_len);
        
        if (token->type != TOKEN_EOF && token->value_ptr && token->value_len > 0) {
            printf(", value='");
            for (int j = 0; j < token->value_len; j++) {
                printf("%c", token->value_ptr[j]);
            }
            printf("'");
        }
        printf("\n");
        
        token_count++;
        
    } while (token && token->type != TOKEN_EOF && token_count < 100);
    
    lexer_destroy();
    
    printf("\nNow testing parser with %d tokens...\n", token_count);
    
    parser_init(tokens);
    int result = parser_parse_program();
    parser_destroy();
    
    printf("Parser result: %d (1=success, 0=failure)\n", result);
    
    return 0;
}