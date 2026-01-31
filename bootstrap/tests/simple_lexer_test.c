/*
 * Simple lexer test to debug tokenization
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stddef.h>

// Token structure (must match lexer.asm layout exactly)
typedef struct __attribute__((packed)) {
    int type;           // 4 bytes
    char* value_ptr;    // 8 bytes  
    int value_len;      // 4 bytes
    int line;           // 4 bytes
    int column;         // 4 bytes
    long padding;       // 8 bytes for alignment
} Token;

// Token type constants (must match lexer.asm)
#define TOKEN_IDENTIFIER    0
#define TOKEN_NUMBER        1
#define TOKEN_STRING        2
#define TOKEN_KEYWORD       3
#define TOKEN_OPERATOR      4
#define TOKEN_DELIMITER     5
#define TOKEN_EOF          6

// External functions from lexer.asm
extern void lexer_init(char* source, int length);
extern Token* lexer_next_token();
extern void lexer_destroy();

const char* token_type_names[] = {
    "IDENTIFIER", "NUMBER", "STRING", "KEYWORD", "OPERATOR", "DELIMITER", "EOF"
};

int main() {
    char* program = "func";
    
    printf("Testing simple program: '%s'\n", program);
    
    lexer_init(program, strlen(program));
    
    Token* token;
    int token_count = 0;
    
    do {
        token = lexer_next_token();
        if (!token) break;
        
        printf("Token %d: type=%s (%d), line=%d, column=%d, len=%d", 
               token_count, token_type_names[token->type], token->type, 
               token->line, token->column, token->value_len);
        
        if (token->type != TOKEN_EOF && token->value_ptr && token->value_len > 0) {
            printf(", value='");
            for (int i = 0; i < token->value_len; i++) {
                printf("%c", token->value_ptr[i]);
            }
            printf("'");
        }
        printf("\n");
        
        token_count++;
        
    } while (token && token->type != TOKEN_EOF && token_count < 10);
    
    lexer_destroy();
    return 0;
}