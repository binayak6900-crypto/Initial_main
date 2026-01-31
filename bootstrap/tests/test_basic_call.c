/*
 * Test basic function call to assembly
 */

#include <stdio.h>

// Simple test function that should just return
extern void lexer_destroy();

int main() {
    printf("Testing basic assembly function call...\n");
    
    // Call the simplest function
    lexer_destroy();
    
    printf("Function call completed successfully\n");
    return 0;
}