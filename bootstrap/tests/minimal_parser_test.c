/*
 * Minimal parser test - just structure validation
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

// Simple C-based parser for comparison
int simple_parse_function(Token* tokens, int* index) {
    printf("  Parsing function starting at token %d\n", *index);
    
    // Expect 'func'
    if (tokens[*index].type != TOKEN_KEYWORD) {
        printf("    ERROR: Expected keyword, got type %d\n", tokens[*index].type);
        return 0;
    }
    
    // Check if it's 'func'
    if (tokens[*index].value_len != 4 || 
        strncmp(tokens[*index].value_ptr, "func", 4) != 0) {
        printf("    ERROR: Expected 'func', got '");
        for (int i = 0; i < tokens[*index].value_len; i++) {
            printf("%c", tokens[*index].value_ptr[i]);
        }
        printf("'\n");
        return 0;
    }
    (*index)++;
    
    // Expect identifier
    if (tokens[*index].type != TOKEN_IDENTIFIER) {
        printf("    ERROR: Expected identifier, got type %d\n", tokens[*index].type);
        return 0;
    }
    printf("    Function name: '");
    for (int i = 0; i < tokens[*index].value_len; i++) {
        printf("%c", tokens[*index].value_ptr[i]);
    }
    printf("'\n");
    (*index)++;
    
    // Expect '('
    if (tokens[*index].type != TOKEN_DELIMITER || 
        tokens[*index].value_ptr[0] != '(') {
        printf("    ERROR: Expected '(', got type %d\n", tokens[*index].type);
        return 0;
    }
    (*index)++;
    
    // Expect ')'
    if (tokens[*index].type != TOKEN_DELIMITER || 
        tokens[*index].value_ptr[0] != ')') {
        printf("    ERROR: Expected ')', got type %d\n", tokens[*index].type);
        return 0;
    }
    (*index)++;
    
    // Expect '{'
    if (tokens[*index].type != TOKEN_DELIMITER || 
        tokens[*index].value_ptr[0] != '{') {
        printf("    ERROR: Expected '{', got type %d\n", tokens[*index].type);
        return 0;
    }
    (*index)++;
    
    // Parse statements until '}'
    while (*index < 100 && tokens[*index].type != TOKEN_EOF) {
        if (tokens[*index].type == TOKEN_DELIMITER && 
            tokens[*index].value_ptr[0] == '}') {
            (*index)++;
            printf("    Function parsed successfully\n");
            return 1;
        }
        
        // Skip statement (simplified)
        printf("    Skipping statement token: type %d\n", tokens[*index].type);
        (*index)++;
    }
    
    printf("    ERROR: Missing '}'\n");
    return 0;
}

int simple_parse_program(Token* tokens) {
    int index = 0;
    
    while (index < 100 && tokens[index].type != TOKEN_EOF) {
        if (!simple_parse_function(tokens, &index)) {
            return 0;
        }
    }
    
    return 1;
}

int main() {
    char* program = "func main() { return 0 }";
    
    printf("Testing with simple C parser: '%s'\n", program);
    printf("===================\n");
    
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
    
    printf("Tokenized %d tokens\n", token_count);
    
    int result = simple_parse_program(tokens);
    printf("Simple parser result: %d (1=success, 0=failure)\n", result);
    
    return result ? 0 : 1;
}