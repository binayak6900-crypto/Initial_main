; Bootstrap Memory Management
; Safe memory allocation and deallocation for bootstrap compiler

section .data
    ; Memory header magic number for corruption detection
    MEMORY_MAGIC        equ 0xDEADBEEF
    MEMORY_FREED        equ 0xFEEDFACE

section .bss
    ; Memory tracking structures
    allocated_blocks    resq 1024   ; Track allocated memory blocks (pointers)
    block_sizes         resq 1024   ; Track sizes of allocated blocks
    block_count         resd 1      ; Number of allocated blocks
    total_allocated     resq 1      ; Total bytes allocated
    total_freed         resq 1      ; Total bytes freed
    stack_base          resq 1      ; Base of the stack
    stack_limit         resq 1      ; Stack overflow limit

section .text
    global safe_malloc
    global safe_free
    global bounds_check
    global memory_init
    global memory_report_leaks
    global stack_overflow_check
    global null_pointer_check
    global safe_array_access
    
    ; Windows API imports
    extern GetProcessHeap
    extern HeapAlloc
    extern HeapFree

; Memory header structure (24 bytes total):
; Offset 0:  Magic number (8 bytes)
; Offset 8:  Size (8 bytes) 
; Offset 16: Allocated flag (8 bytes)

; Initialize memory management system
memory_init:
    push rbp
    mov rbp, rsp
    
    ; Initialize counters
    mov dword [rel block_count], 0
    mov qword [rel total_allocated], 0
    mov qword [rel total_freed], 0
    
    mov rax, 1          ; Success
    
    pop rbp
    ret

; Safe memory allocation with header
; Parameters: rcx = size (Windows calling convention)
; Returns: rax = allocated memory pointer (or NULL on failure)
safe_malloc:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    push r13
    sub rsp, 32         ; Shadow space for Windows API calls
    
    ; Store requested size
    mov r12, rcx
    
    ; Check for zero size allocation
    test r12, r12
    jz .alloc_failed
    
    ; Add space for memory header (24 bytes) and align
    mov rcx, r12
    add rcx, 24
    add rcx, 7
    and rcx, ~7
    
    ; Allocate using Windows heap
    call GetProcessHeap
    mov rcx, rax        ; heap handle
    mov rdx, 0          ; flags
    mov r8, r12
    add r8, 24          ; size including header
    call HeapAlloc
    
    test rax, rax
    jz .alloc_failed
    
    mov r13, rax        ; Save allocated address
    
    ; Set up memory header
    mov qword [r13], MEMORY_MAGIC       ; Magic number
    mov qword [r13 + 8], r12            ; Original size
    mov qword [r13 + 16], 1             ; Allocated flag
    
    ; Track this allocation (simplified)
    mov eax, [rel block_count]
    cmp eax, 1024
    jae .alloc_failed   ; Too many allocations
    
    ; Add to tracking arrays
    mov rbx, rax
    shl rbx, 3          ; multiply by 8
    lea rdx, [rel allocated_blocks]
    mov [rdx + rbx], r13
    lea rdx, [rel block_sizes]
    mov [rdx + rbx], r12
    inc dword [rel block_count]
    
    ; Update statistics
    add [rel total_allocated], r12
    
    ; Return pointer after header
    mov rax, r13
    add rax, 24
    jmp .done
    
.alloc_failed:
    xor rax, rax
    
.done:
    add rsp, 32
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret

; Safe memory deallocation with double-free protection
; Parameters: rcx = memory pointer (Windows calling convention)
safe_free:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    push r13
    sub rsp, 32         ; Shadow space
    
    ; Store pointer
    mov r12, rcx
    
    ; Check if pointer is valid
    test r12, r12
    jz .done
    
    ; Get header address (subtract 24 bytes)
    sub r12, 24
    
    ; Check magic number
    mov rax, [r12]
    cmp rax, MEMORY_MAGIC
    jne .corruption_detected
    
    ; Check if already freed
    mov rax, [r12 + 16]
    cmp rax, 0
    je .double_free_detected
    
    ; Get size for statistics
    mov r13, [r12 + 8]
    
    ; Mark as freed
    mov qword [r12 + 16], 0
    mov qword [r12], MEMORY_FREED
    
    ; Free the memory using Windows heap
    call GetProcessHeap
    mov rcx, rax        ; heap handle
    mov rdx, 0          ; flags
    mov r8, r12         ; pointer to free
    call HeapFree
    
    ; Update statistics
    add [rel total_freed], r13
    
    ; Remove from tracking (simplified - just decrement count)
    dec dword [rel block_count]
    jmp .done
    
.double_free_detected:
    ; Just continue - error handling simplified
    jmp .done
    
.corruption_detected:
    ; Just continue - error handling simplified
    
.done:
    add rsp, 32
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret

; Report memory leaks
memory_report_leaks:
    push rbp
    mov rbp, rsp
    
    ; Simplified - just return
    
    pop rbp
    ret

; Bounds checking for array access
; Parameters: rcx = pointer, rdx = offset, r8 = access_size (Windows calling convention)
; Returns: rax = 1 if valid, 0 if invalid
bounds_check:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Check if pointer is valid
    test rcx, rcx
    jz .invalid
    
    ; Get header address (subtract 24 bytes)
    mov rbx, rcx
    sub rbx, 24
    
    ; Check magic number
    mov rax, [rbx]
    cmp rax, MEMORY_MAGIC
    jne .invalid
    
    ; Check if still allocated
    mov rax, [rbx + 16]
    test rax, rax
    jz .invalid
    
    ; Get allocated size
    mov rax, [rbx + 8]
    
    ; Check if offset + access_size <= allocated_size
    mov rbx, rdx
    add rbx, r8
    cmp rbx, rax
    ja .invalid
    
    ; Valid access
    mov rax, 1
    jmp .done
    
.invalid:
    xor rax, rax
    
.done:
    pop rbx
    pop rbp
    ret

; Stack overflow detection and protection
; Parameters: none
; Returns: rax = 1 if stack is safe, 0 if overflow detected
stack_overflow_check:
    push rbp
    mov rbp, rsp
    
    ; Get current stack pointer
    mov rax, rsp
    
    ; Check if we have stack base initialized
    mov rbx, [rel stack_base]
    test rbx, rbx
    jz .init_stack_base
    
    ; Check if current stack pointer is within safe limits
    mov rcx, [rel stack_limit]
    cmp rax, rcx
    jb .overflow_detected
    
    ; Stack is safe
    mov rax, 1
    jmp .done
    
.init_stack_base:
    ; Initialize stack base and limit (first call)
    mov [rel stack_base], rsp
    mov rbx, rsp
    sub rbx, 65536      ; 64KB stack limit
    mov [rel stack_limit], rbx
    mov rax, 1
    jmp .done
    
.overflow_detected:
    xor rax, rax
    
.done:
    pop rbp
    ret

; Null pointer dereference prevention
; Parameters: rcx = pointer to check
; Returns: rax = 1 if valid, 0 if null
null_pointer_check:
    push rbp
    mov rbp, rsp
    
    ; Check if pointer is null
    test rcx, rcx
    jz .null_detected
    
    ; Pointer is valid
    mov rax, 1
    jmp .done
    
.null_detected:
    xor rax, rax
    
.done:
    pop rbp
    ret

; Safe array access with comprehensive checking
; Parameters: rcx = array_pointer, rdx = index, r8 = element_size
; Returns: rax = element_address if valid, 0 if invalid
safe_array_access:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    push r13
    sub rsp, 32         ; Shadow space
    
    ; Store parameters
    mov r12, rcx        ; array pointer
    mov r13, rdx        ; index
    
    ; First check for null pointer
    mov rcx, r12
    call null_pointer_check
    test rax, rax
    jz .access_failed
    
    ; Check stack overflow
    call stack_overflow_check
    test rax, rax
    jz .access_failed
    
    ; Calculate offset (index * element_size)
    mov rax, r13
    mul r8              ; rax = index * element_size
    mov rbx, rax        ; offset
    
    ; Perform bounds checking
    mov rcx, r12        ; array pointer
    mov rdx, rbx        ; offset
    ; r8 already contains element_size
    call bounds_check
    test rax, rax
    jz .access_failed
    
    ; Calculate and return element address
    mov rax, r12
    add rax, rbx
    jmp .done
    
.access_failed:
    xor rax, rax
    
.done:
    add rsp, 32
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret