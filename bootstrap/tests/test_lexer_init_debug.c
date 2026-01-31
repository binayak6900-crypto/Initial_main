/*
 * Test debug version of lexer_init
 */

#include <stdio.h>
#include <string.h>

extern int test_lexer_init_debug(char* source, int length);

int main() {
    char program[] = "func";
    
    printf("Testing debug lexer_init...\n");
    
    int result = test_lexer_init_debug(program, strlen(program));
    
    printf("Result: %d\n", result);
    return 0;
}