#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// External assembly functions
extern void memory_init();
extern void* safe_malloc(size_t size);
extern void safe_free(void* ptr);
extern int bounds_check(void* ptr, size_t offset, size_t access_size);
extern int stack_overflow_check();
extern int null_pointer_check(void* ptr);
extern void* safe_array_access(void* array_ptr, size_t index, size_t element_size);

// Test bounds checking functionality
int test_bounds_checking() {
    printf("Testing bounds checking for array operations...\n");
    
    memory_init();
    
    // Test 1: Basic bounds checking
    void* ptr = safe_malloc(100);
    if (!ptr) {
        printf("FAIL: Could not allocate memory\n");
        return 0;
    }
    
    // Valid access
    if (bounds_check(ptr, 0, 50) != 1) {
        printf("FAIL: Valid bounds check failed\n");
        return 0;
    }
    
    // Invalid access (out of bounds)
    if (bounds_check(ptr, 50, 60) != 0) {
        printf("FAIL: Invalid bounds check should have failed\n");
        return 0;
    }
    
    // Test 2: Null pointer checking
    if (null_pointer_check(NULL) != 0) {
        printf("FAIL: Null pointer check should detect null\n");
        return 0;
    }
    
    if (null_pointer_check(ptr) != 1) {
        printf("FAIL: Valid pointer check failed\n");
        return 0;
    }
    
    // Test 3: Stack overflow checking
    if (stack_overflow_check() != 1) {
        printf("FAIL: Stack overflow check failed for normal stack\n");
        return 0;
    }
    
    // Test 4: Safe array access
    int* array = (int*)safe_malloc(10 * sizeof(int));
    if (!array) {
        printf("FAIL: Could not allocate array\n");
        return 0;
    }
    
    // Initialize array
    for (int i = 0; i < 10; i++) {
        array[i] = i * 2;
    }
    
    // Valid array access
    void* element = safe_array_access(array, 5, sizeof(int));
    if (!element) {
        printf("FAIL: Valid array access failed\n");
        return 0;
    }
    
    // Check if we got the right element
    if (*(int*)element != 10) {
        printf("FAIL: Array access returned wrong value: expected 10, got %d\n", *(int*)element);
        return 0;
    }
    
    // Invalid array access (out of bounds)
    element = safe_array_access(array, 15, sizeof(int));
    if (element != NULL) {
        printf("FAIL: Out of bounds array access should have failed\n");
        return 0;
    }
    
    // Test 5: Array access with null pointer
    element = safe_array_access(NULL, 0, sizeof(int));
    if (element != NULL) {
        printf("FAIL: Array access with null pointer should have failed\n");
        return 0;
    }
    
    // Clean up
    safe_free(ptr);
    safe_free(array);
    
    printf("All bounds checking tests passed\n");
    return 1;
}

// Recursive function to test stack overflow detection
int recursive_function(int depth) {
    char buffer[1024];  // Use some stack space
    memset(buffer, 0, sizeof(buffer));
    
    // Check stack on each recursion
    if (stack_overflow_check() == 0) {
        printf("Stack overflow detected at depth %d\n", depth);
        return depth;
    }
    
    if (depth > 100) {  // Limit recursion to prevent actual crash
        return depth;
    }
    
    return recursive_function(depth + 1);
}

int test_stack_overflow_detection() {
    printf("Testing stack overflow detection...\n");
    
    // Test normal stack usage
    if (stack_overflow_check() != 1) {
        printf("FAIL: Normal stack should be safe\n");
        return 0;
    }
    
    // Test with recursive calls (this should eventually detect overflow or hit our limit)
    int max_depth = recursive_function(0);
    printf("Maximum safe recursion depth: %d\n", max_depth);
    
    // The test passes if we don't crash and get some reasonable depth
    if (max_depth > 10) {
        printf("Stack overflow detection test passed\n");
        return 1;
    } else {
        printf("FAIL: Stack overflow detection may be too aggressive\n");
        return 0;
    }
}

int main() {
    printf("=== Bounds Checking and Array Operations Test ===\n");
    
    int bounds_result = test_bounds_checking();
    int stack_result = test_stack_overflow_detection();
    
    if (bounds_result && stack_result) {
        printf("\n✓ All bounds checking and array operation tests PASSED\n");
        return 0;
    } else {
        printf("\n✗ Some bounds checking tests FAILED\n");
        return 1;
    }
}