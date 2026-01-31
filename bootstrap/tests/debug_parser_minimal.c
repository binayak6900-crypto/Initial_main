/*
 * Minimal parser debug test
 */

#include <stdio.h>
#include <string.h>

// Token structure (must match lexer.asm layout exactly)
typedef struct {
    int type;           // 4 bytes at offset 0
    int padding1;       // 4 bytes padding to align pointer
    char* value_ptr;    // 8 bytes at offset 8
    int value_len;      // 4 bytes at offset 16
    int line;           // 4 bytes at offset 20
    int column;         // 4 bytes at offset 24
    int padding2;       // 4 bytes padding to make 32 bytes total
} Token;

// External functions
extern void lexer_init(char* source, int length);
extern Token* lexer_next_token();
extern void lexer_destroy();
extern void parser_init(Token* tokens);
extern int parser_parse_program();
extern void parser_destroy();

int main() {
    char* program = "func main() { }";  // Simplest possible valid program
    
    printf("Testing minimal program: '%s'\n", program);
    
    // Tokenize
    lexer_init(program, strlen(program));
    
    // Collect all tokens
    Token tokens[20];  // Static array to avoid malloc issues
    int token_count = 0;
    
    Token* token;
    do {
        token = lexer_next_token();
        if (token && token_count < 19) {
            tokens[token_count] = *token;
            printf("Token %d: type=%d, len=%d\n", token_count, token->type, token->value_len);
            token_count++;
        }
    } while (token && token->type != 6 && token_count < 19);  // TOKEN_EOF = 6
    
    lexer_destroy();
    
    printf("Collected %d tokens\n", token_count);
    
    // Test parser
    printf("Initializing parser...\n");
    parser_init(tokens);
    
    printf("Calling parser_parse_program...\n");
    int result = parser_parse_program();
    
    printf("Parser result: %d\n", result);
    
    parser_destroy();
    
    return 0;
}