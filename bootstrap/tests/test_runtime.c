#include <stdio.h>

// External runtime functions
extern int runtime_init_generator();
extern int runtime_generate_entry_point();
extern int runtime_generate_init_code();
extern int runtime_generate_cleanup_code();
extern void runtime_set_main_function(int rva);
extern void* runtime_get_entry_point(int* size);
extern int runtime_get_total_runtime_size();
extern void runtime_destroy_generator();

int main() {
    printf("Testing runtime generation...\n");
    
    // Initialize runtime generator
    if (!runtime_init_generator()) {
        printf("ERROR: Failed to initialize runtime generator\n");
        return 1;
    }
    
    // Set main function RVA
    runtime_set_main_function(0x1000);
    
    // Generate runtime components
    if (!runtime_generate_entry_point()) {
        printf("ERROR: Failed to generate entry point\n");
        runtime_destroy_generator();
        return 1;
    }
    
    if (!runtime_generate_init_code()) {
        printf("ERROR: Failed to generate init code\n");
        runtime_destroy_generator();
        return 1;
    }
    
    if (!runtime_generate_cleanup_code()) {
        printf("ERROR: Failed to generate cleanup code\n");
        runtime_destroy_generator();
        return 1;
    }
    
    // Get entry point
    int size;
    void* entry_point = runtime_get_entry_point(&size);
    if (!entry_point) {
        printf("ERROR: Failed to get entry point\n");
        runtime_destroy_generator();
        return 1;
    }
    
    printf("SUCCESS: Entry point generated, size = %d bytes\n", size);
    
    // Get total runtime size
    int total_size = runtime_get_total_runtime_size();
    printf("Total runtime size: %d bytes\n", total_size);
    
    // Cleanup
    runtime_destroy_generator();
    
    printf("SUCCESS: Runtime generation test completed\n");
    return 0;
}