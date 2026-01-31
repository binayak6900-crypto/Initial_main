/*
 * Test lexer_init with debugging
 */

#include <stdio.h>
#include <string.h>

// External functions from lexer.asm
extern void lexer_init(char* source, int length);

int main() {
    char program[] = "func";  // Use array instead of pointer
    
    printf("About to call lexer_init...\n");
    printf("Program address: %p\n", program);
    printf("Program length: %d\n", (int)strlen(program));
    
    // Call lexer_init
    lexer_init(program, strlen(program));
    
    printf("lexer_init completed\n");
    return 0;
}