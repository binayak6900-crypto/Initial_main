#include <stdio.h>
#include <stdlib.h>

// Test the PE generator with C file I/O
int main() {
    printf("Creating simple PE file...\n");
    
    FILE* f = fopen("simple_pe.exe", "wb");
    if (!f) {
        printf("ERROR: Could not create file\n");
        return 1;
    }
    
    // Write minimal DOS header
    unsigned char dos_header[64] = {0};
    dos_header[0] = 'M';
    dos_header[1] = 'Z';
    dos_header[60] = 0x80;  // PE header offset
    
    fwrite(dos_header, 1, 64, f);
    
    // Pad to PE header offset
    unsigned char padding[64] = {0};
    fwrite(padding, 1, 64, f);
    
    // Write PE signature
    fwrite("PE\0\0", 1, 4, f);
    
    // Write minimal COFF header
    unsigned char coff_header[20] = {0};
    coff_header[0] = 0x64; coff_header[1] = 0x86;  // Machine (AMD64)
    coff_header[2] = 1; coff_header[3] = 0;        // Number of sections
    coff_header[16] = 240; coff_header[17] = 0;    // Optional header size
    coff_header[18] = 0x22; coff_header[19] = 0;   // Characteristics
    
    fwrite(coff_header, 1, 20, f);
    
    fclose(f);
    
    printf("SUCCESS: Simple PE file created\n");
    return 0;
}