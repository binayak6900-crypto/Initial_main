/*
 * Debug test for lexer to understand token structure
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

// External functions from lexer.asm
extern void lexer_init(char* source, int length);
extern Token* lexer_next_token();
extern void lexer_destroy();

int main() {
    char* program = "func";
    
    printf("Program: '%s'\n", program);
    printf("Token structure size: %zu bytes\n", sizeof(Token));
    printf("Offsets: type=%zu, value_ptr=%zu, value_len=%zu, line=%zu, column=%zu\n",
           offsetof(Token, type), offsetof(Token, value_ptr), offsetof(Token, value_len),
           offsetof(Token, line), offsetof(Token, column));
    
    lexer_init(program, strlen(program));
    
    Token* token = lexer_next_token();
    if (token) {
        printf("Raw token data:\n");
        printf("  Address: %p\n", token);
        printf("  type: %d (at offset 0)\n", token->type);
        printf("  value_ptr: %p (at offset 8)\n", token->value_ptr);
        printf("  value_len: %d (at offset 16)\n", token->value_len);
        printf("  line: %d (at offset 20)\n", token->line);
        printf("  column: %d (at offset 24)\n", token->column);
        
        // Print raw bytes
        unsigned char* bytes = (unsigned char*)token;
        printf("Raw bytes: ");
        for (int i = 0; i < 32; i++) {
            printf("%02x ", bytes[i]);
        }
        printf("\n");
    }
    
    lexer_destroy();
    return 0;
}