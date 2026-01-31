/*
 * Test simple assembly function
 */

#include <stdio.h>

extern int test_simple_function();

int main() {
    printf("Calling simple assembly function...\n");
    
    int result = test_simple_function();
    
    printf("Result: %d\n", result);
    return 0;
}