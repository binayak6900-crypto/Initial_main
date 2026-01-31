/*
 * Test to verify token structure layout matches assembly
 */

#include <stdio.h>
#include <stddef.h>

// Try different structure layouts to match assembly
typedef struct {
    int type;           // 4 bytes at offset 0
    int padding1;       // 4 bytes padding to align pointer
    char* value_ptr;    // 8 bytes at offset 8
    int value_len;      // 4 bytes at offset 16
    int line;           // 4 bytes at offset 20
    int column;         // 4 bytes at offset 24
    int padding2;       // 4 bytes padding to make 32 bytes total
} Token;

int main() {
    printf("Token structure analysis:\n");
    printf("Size: %zu bytes\n", sizeof(Token));
    printf("Offsets:\n");
    printf("  type: %zu\n", offsetof(Token, type));
    printf("  value_ptr: %zu\n", offsetof(Token, value_ptr));
    printf("  value_len: %zu\n", offsetof(Token, value_len));
    printf("  line: %zu\n", offsetof(Token, line));
    printf("  column: %zu\n", offsetof(Token, column));
    
    return 0;
}