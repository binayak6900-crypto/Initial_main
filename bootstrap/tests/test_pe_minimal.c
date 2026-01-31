#include <stdio.h>

// Test just the basic functions
extern int pe_init(const char* filename);
extern void pe_destroy();

int main() {
    printf("Testing minimal PE functions...\n");
    
    // Test initialization
    if (!pe_init("test_minimal.exe")) {
        printf("ERROR: pe_init failed\n");
        return 1;
    }
    
    printf("SUCCESS: pe_init succeeded\n");
    
    // Test cleanup
    pe_destroy();
    printf("SUCCESS: pe_destroy succeeded\n");
    
    return 0;
}