/*
 * Minimal lexer test to debug segfault
 */

#include <stdio.h>
#include <string.h>

// External functions from lexer.asm
extern void lexer_init(char* source, int length);
extern void lexer_destroy();

int main() {
    char* program = "func";
    
    printf("Testing lexer_init with: '%s'\n", program);
    printf("Length: %d\n", (int)strlen(program));
    
    // Just test initialization
    lexer_init(program, strlen(program));
    printf("lexer_init completed successfully\n");
    
    // Test cleanup
    lexer_destroy();
    printf("lexer_destroy completed successfully\n");
    
    return 0;
}