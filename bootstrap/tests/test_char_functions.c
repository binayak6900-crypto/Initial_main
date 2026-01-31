/*
 * Test character classification functions
 */

#include <stdio.h>

// External functions from lexer.asm
extern int test_is_identifier_start(char c);

int main() {
    char test_chars[] = {'f', 'A', 'z', '1', '+', '_', '\0'};
    
    for (int i = 0; test_chars[i] != '\0'; i++) {
        char c = test_chars[i];
        int result = test_is_identifier_start(c);
        printf("is_identifier_start('%c') = %d\n", c, result);
    }
    
    return 0;
}