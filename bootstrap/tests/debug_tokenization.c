/*
 * Debug tokenization step by step
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stddef.h>

// External functions from lexer.asm
extern int test_is_identifier_start(char c);

int main() {
    char c = 'f';
    
    printf("Testing character '%c'\n", c);
    int result = test_is_identifier_start(c);
    printf("is_identifier_start('%c') = %d\n", c, result);
    
    if (result == 1) {
        printf("Character should be treated as identifier start\n");
    } else {
        printf("Character should NOT be treated as identifier start\n");
    }
    
    return 0;
}