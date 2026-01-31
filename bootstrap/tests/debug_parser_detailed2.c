/*
 * Detailed parser debug test
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

int main() {
    char* program = "func main() { }";
    
    printf("Testing program: '%s'\n", program);
    printf("===================\n");
    
    lexer_init(program, strlen(program));
    
    Token* token;
    int i = 0;
    do {
        token = lexer_next_token();
        if (token) {
            printf("Token %d: type=%d, line=%d, column=%d, len=%d", 
                   i, token->type, token->line, token->column, token->value_len);
            
            if (token->value_ptr && token->value_len > 0) {
                printf(", value='");
                for (int j = 0; j < token->value_len && j < 20; j++) {
                    printf("%c", token->value_ptr[j]);
                }
                printf("'");
            }
            printf("\n");
            
            i++;
        }
    } while (token && token->type != 6 && i < 10);  // TOKEN_EOF = 6
    
    lexer_destroy();
    
    return 0;
}