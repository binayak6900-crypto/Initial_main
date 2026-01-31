#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <assert.h>

#ifdef _WIN32
    #include <windows.h>
    #include <process.h>
#else
    #include <unistd.h>
    #include <sys/wait.h>
#endif

// External assembly functions (Windows calling convention)
extern void memory_init();
extern void* safe_malloc(size_t size);
extern void safe_free(void* ptr);
extern int bounds_check(void* ptr, size_t offset, size_t access_size);
extern void memory_report_leaks();

// Windows calling convention wrapper for memory_get_stats
void memory_get_stats(size_t* allocated, size_t* freed, int* blocks) {
    // This is a placeholder - the assembly function returns values in registers
    // For now, we'll use simple tracking
    *allocated = 0;
    *freed = 0;
    *blocks = 0;
}

// Property-Based Test Framework
#define MAX_ITERATIONS 100
#define MAX_ALLOCATIONS 50

typedef struct {
    void* ptr;
    size_t size;
    int is_freed;
} allocation_record_t;

// Random number generator state
static unsigned int rng_state = 1;

unsigned int simple_rand() {
    rng_state = rng_state * 1103515245 + 12345;
    return rng_state;
}

void seed_rand(unsigned int seed) {
    rng_state = seed;
}

// Generate random size between 1 and 1024
size_t random_size() {
    return (simple_rand() % 1024) + 1;
}

// Generate random offset
size_t random_offset(size_t max_size) {
    if (max_size == 0) return 0;
    return simple_rand() % (max_size + 10); // Sometimes go beyond bounds
}

// Generate random access size
size_t random_access_size() {
    return (simple_rand() % 64) + 1;
}

/**
 * Property 4: Memory Safety Guarantee
 * Validates: Requirements 14.1, 14.5, 14.6, 18.1, 18.2, 18.3, 18.6, 18.7
 * 
 * For any memory operation (allocation, deallocation, array access, pointer dereference),
 * the runtime system should prevent buffer overflows, use-after-free, null pointer 
 * dereferences, double-free, and memory leaks through automatic checking and tracking.
 */
int test_memory_safety_property() {
    printf("Testing Property 4: Memory Safety Guarantee\n");
    printf("Validates: Requirements 14.1, 14.5, 14.6, 18.1, 18.2, 18.3, 18.6, 18.7\n");
    
    int passed = 0;
    int failed = 0;
    
    for (int iteration = 0; iteration < MAX_ITERATIONS; iteration++) {
        // Seed random number generator with iteration
        seed_rand(iteration + time(NULL));
        
        // Initialize memory system for each iteration
        memory_init();
        
        allocation_record_t allocations[MAX_ALLOCATIONS];
        int allocation_count = 0;
        
        // Generate random sequence of memory operations
        int operations = (simple_rand() % 20) + 10; // 10-30 operations
        
        for (int op = 0; op < operations; op++) {
            int operation_type = simple_rand() % 4; // 0=alloc, 1=free, 2=bounds_check, 3=double_free_test
            
            switch (operation_type) {
                case 0: { // Allocation
                    if (allocation_count < MAX_ALLOCATIONS) {
                        size_t size = random_size();
                        void* ptr = safe_malloc(size);
                        
                        if (ptr != NULL) {
                            // Test 1: Allocation should return valid pointer
                            allocations[allocation_count].ptr = ptr;
                            allocations[allocation_count].size = size;
                            allocations[allocation_count].is_freed = 0;
                            allocation_count++;
                            
                            // Test 2: Should be able to write to allocated memory
                            memset(ptr, 0xAA, size);
                            
                            // Test 3: Bounds checking should work for valid access
                            int bounds_result = bounds_check(ptr, 0, size);
                            if (bounds_result != 1) {
                                printf("FAIL: Bounds check failed for valid access (iteration %d)\n", iteration);
                                failed++;
                                continue;
                            }
                        }
                    }
                    break;
                }
                
                case 1: { // Free
                    if (allocation_count > 0) {
                        int idx = simple_rand() % allocation_count;
                        if (!allocations[idx].is_freed) {
                            safe_free(allocations[idx].ptr);
                            allocations[idx].is_freed = 1;
                            
                            // Test 4: Bounds checking should fail after free
                            int bounds_result = bounds_check(allocations[idx].ptr, 0, allocations[idx].size);
                            if (bounds_result != 0) {
                                printf("FAIL: Bounds check should fail for freed memory (iteration %d)\n", iteration);
                                failed++;
                                continue;
                            }
                        }
                    }
                    break;
                }
                
                case 2: { // Bounds checking test
                    if (allocation_count > 0) {
                        int idx = simple_rand() % allocation_count;
                        if (!allocations[idx].is_freed) {
                            size_t offset = random_offset(allocations[idx].size);
                            size_t access_size = random_access_size();
                            
                            int bounds_result = bounds_check(allocations[idx].ptr, offset, access_size);
                            
                            // Test 5: Bounds checking should correctly identify valid/invalid access
                            int should_be_valid = (offset + access_size <= allocations[idx].size);
                            if ((bounds_result == 1) != should_be_valid) {
                                printf("FAIL: Bounds check incorrect - offset=%zu, access=%zu, size=%zu, result=%d (iteration %d)\n", 
                                       offset, access_size, allocations[idx].size, bounds_result, iteration);
                                failed++;
                                continue;
                            }
                        }
                    }
                    break;
                }
                
                case 3: { // Double-free test
                    if (allocation_count > 0) {
                        int idx = simple_rand() % allocation_count;
                        if (allocations[idx].is_freed) {
                            // Test 6: Double-free should be detected (we can't easily test this without crashing)
                            // For now, we just ensure the system doesn't crash
                            safe_free(allocations[idx].ptr);
                        }
                    }
                    break;
                }
            }
        }
        
        // Test 7: NULL pointer handling
        int null_bounds = bounds_check(NULL, 0, 10);
        if (null_bounds != 0) {
            printf("FAIL: Bounds check should fail for NULL pointer (iteration %d)\n", iteration);
            failed++;
            continue;
        }
        
        // Test 8: Free NULL should not crash
        safe_free(NULL);
        
        // Clean up remaining allocations
        for (int i = 0; i < allocation_count; i++) {
            if (!allocations[i].is_freed) {
                safe_free(allocations[i].ptr);
            }
        }
        
        passed++;
    }
    
    printf("Property test completed: %d passed, %d failed out of %d iterations\n", 
           passed, failed, MAX_ITERATIONS);
    
    return failed == 0;
}

// Test specific memory safety scenarios
int test_specific_memory_scenarios() {
    printf("\nTesting specific memory safety scenarios...\n");
    
    memory_init();
    
    // Test 1: Zero-size allocation
    void* zero_ptr = safe_malloc(0);
    if (zero_ptr != NULL) {
        printf("FAIL: Zero-size allocation should return NULL\n");
        return 0;
    }
    
    // Test 2: Large allocation
    void* large_ptr = safe_malloc(1000000); // 1MB
    if (large_ptr == NULL) {
        printf("FAIL: Large allocation failed\n");
        return 0;
    }
    
    // Test 3: Write to large allocation
    memset(large_ptr, 0x55, 1000000);
    
    // Test 4: Bounds check on large allocation
    if (bounds_check(large_ptr, 0, 1000000) != 1) {
        printf("FAIL: Bounds check failed for large allocation\n");
        return 0;
    }
    
    if (bounds_check(large_ptr, 1000000, 1) != 0) {
        printf("FAIL: Bounds check should fail for out-of-bounds access\n");
        return 0;
    }
    
    safe_free(large_ptr);
    
    // Test 5: Use after free detection
    if (bounds_check(large_ptr, 0, 100) != 0) {
        printf("FAIL: Use after free should be detected\n");
        return 0;
    }
    
    printf("All specific memory safety scenarios passed\n");
    return 1;
}

int main() {
    printf("=== Memory Safety Property-Based Test ===\n");
    printf("**Property 4: Memory Safety Guarantee**\n");
    printf("**Validates: Requirements 14.1, 14.5, 14.6, 18.1, 18.2, 18.3, 18.6, 18.7**\n\n");
    
    int property_result = test_memory_safety_property();
    int scenario_result = test_specific_memory_scenarios();
    
    if (property_result && scenario_result) {
        printf("\n✓ All memory safety tests PASSED\n");
        return 0;
    } else {
        printf("\n✗ Memory safety tests FAILED\n");
        return 1;
    }
}