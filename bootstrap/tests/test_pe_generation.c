#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// External assembly functions
extern int pe_init(const char* filename);
extern void pe_set_code(const void* code_buffer, int code_size);
extern int pe_generate_executable();
extern void pe_destroy();

int main() {
    printf("Testing PE generation...\n");
    
    // Simple x86-64 machine code that returns 42
    // mov rax, 42; ret
    unsigned char test_code[] = {
        0x48, 0xC7, 0xC0, 0x2A, 0x00, 0x00, 0x00,  // mov rax, 42
        0xC3                                          // ret
    };
    
    // Initialize PE generator
    if (!pe_init("test_pe_output.exe")) {
        printf("ERROR: Failed to initialize PE generator\n");
        return 1;
    }
    
    // Set machine code
    pe_set_code(test_code, sizeof(test_code));
    
    // Generate executable
    if (!pe_generate_executable()) {
        printf("ERROR: Failed to generate PE executable\n");
        pe_destroy();
        return 1;
    }
    
    // Cleanup
    pe_destroy();
    
    printf("SUCCESS: PE executable generated successfully\n");
    return 0;
}