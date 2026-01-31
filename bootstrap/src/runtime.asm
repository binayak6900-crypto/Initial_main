; Runtime Initialization and Entry Point Generation
; Provides program entry point and runtime setup for generated executables

section .data
    ; Runtime constants
    STACK_SIZE          equ 65536       ; 64KB stack
    HEAP_SIZE           equ 1048576     ; 1MB heap
    
    ; Entry point template (x86-64 machine code)
    entry_point_template:
        ; Standard program entry point
        ; push rbp
        db 0x55
        ; mov rbp, rsp
        db 0x48, 0x89, 0xE5
        ; sub rsp, 32 (shadow space)
        db 0x48, 0x83, 0xEC, 0x20
        ; call runtime_init
        db 0xE8, 0x00, 0x00, 0x00, 0x00  ; Will be patched with offset
        ; call main function (will be patched)
        db 0xE8, 0x00, 0x00, 0x00, 0x00  ; Will be patched with offset
        ; mov rcx, rax (exit code)
        db 0x48, 0x89, 0xC1
        ; call runtime_cleanup
        db 0xE8, 0x00, 0x00, 0x00, 0x00  ; Will be patched with offset
        ; call ExitProcess
        db 0xE8, 0x00, 0x00, 0x00, 0x00  ; Will be patched with offset
    entry_point_template_size equ $ - entry_point_template
    
    ; Runtime initialization code template
    runtime_init_template:
        ; Initialize heap
        ; call GetProcessHeap
        db 0xE8, 0x00, 0x00, 0x00, 0x00
        ; Store heap handle (simplified)
        ; mov [heap_handle], rax
        db 0x48, 0xA3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        ; Initialize stack checking
        ; mov rax, rsp
        db 0x48, 0x89, 0xE0
        ; sub rax, STACK_SIZE
        db 0x48, 0x2D, 0x00, 0x00, 0x01, 0x00
        ; mov [stack_limit], rax
        db 0x48, 0xA3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        ; ret
        db 0xC3
    runtime_init_template_size equ $ - runtime_init_template
    
    ; Runtime cleanup code template
    runtime_cleanup_template:
        ; Cleanup heap allocations (simplified)
        ; ret
        db 0xC3
    runtime_cleanup_template_size equ $ - runtime_cleanup_template

section .bss
    ; Runtime generation state
    runtime_state:
        .entry_point_code   resq 1      ; Generated entry point code
        .entry_point_size   resd 1      ; Size of entry point code
        .init_code          resq 1      ; Generated init code
        .init_code_size     resd 1      ; Size of init code
        .cleanup_code       resq 1      ; Generated cleanup code
        .cleanup_code_size  resd 1      ; Size of cleanup code
        .main_function_rva  resd 1      ; RVA of main function
        .heap_handle        resq 1      ; Process heap handle
        .stack_limit        resq 1      ; Stack overflow limit

section .text
    global runtime_init_generator
    global runtime_generate_entry_point
    global runtime_generate_init_code
    global runtime_generate_cleanup_code
    global runtime_set_main_function
    global runtime_get_entry_point
    global runtime_get_total_runtime_size
    global runtime_destroy_generator
    
    ; Memory allocation functions
    extern safe_malloc
    extern safe_free

; Initialize runtime code generator
runtime_init_generator:
    push rbp
    mov rbp, rsp
    
    ; Clear runtime state
    mov qword [rel runtime_state.entry_point_code], 0
    mov dword [rel runtime_state.entry_point_size], 0
    mov qword [rel runtime_state.init_code], 0
    mov dword [rel runtime_state.init_code_size], 0
    mov qword [rel runtime_state.cleanup_code], 0
    mov dword [rel runtime_state.cleanup_code_size], 0
    mov dword [rel runtime_state.main_function_rva], 0
    mov qword [rel runtime_state.heap_handle], 0
    mov qword [rel runtime_state.stack_limit], 0
    
    mov eax, 1          ; Success
    
    pop rbp
    ret

; Set main function RVA for entry point generation
; Parameters: rdi = main function RVA
runtime_set_main_function:
    push rbp
    mov rbp, rsp
    
    mov dword [rel runtime_state.main_function_rva], edi
    
    pop rbp
    ret

; Generate entry point code
runtime_generate_entry_point:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    push r13
    
    ; Allocate memory for entry point code
    mov rcx, entry_point_template_size + 32  ; Extra space for patches
    call safe_malloc
    test rax, rax
    jz .error
    
    mov r12, rax        ; Save allocated memory
    mov qword [rel runtime_state.entry_point_code], r12
    
    ; Copy template
    mov rdi, r12
    lea rsi, [rel entry_point_template]
    mov rcx, entry_point_template_size
    rep movsb
    
    ; Patch call offsets (simplified - would need proper relocation)
    ; For now, just store the template size
    mov dword [rel runtime_state.entry_point_size], entry_point_template_size
    
    mov eax, 1          ; Success
    jmp .done
    
.error:
    xor eax, eax        ; Failure
    
.done:
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret

; Generate runtime initialization code
runtime_generate_init_code:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    
    ; Allocate memory for init code
    mov rcx, runtime_init_template_size + 32
    call safe_malloc
    test rax, rax
    jz .error
    
    mov r12, rax        ; Save allocated memory
    mov qword [rel runtime_state.init_code], r12
    
    ; Copy template
    mov rdi, r12
    lea rsi, [rel runtime_init_template]
    mov rcx, runtime_init_template_size
    rep movsb
    
    ; Store size
    mov dword [rel runtime_state.init_code_size], runtime_init_template_size
    
    mov eax, 1          ; Success
    jmp .done
    
.error:
    xor eax, eax        ; Failure
    
.done:
    pop r12
    pop rbx
    pop rbp
    ret

; Generate runtime cleanup code
runtime_generate_cleanup_code:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    
    ; Allocate memory for cleanup code
    mov rcx, runtime_cleanup_template_size + 32
    call safe_malloc
    test rax, rax
    jz .error
    
    mov r12, rax        ; Save allocated memory
    mov qword [rel runtime_state.cleanup_code], r12
    
    ; Copy template
    mov rdi, r12
    lea rsi, [rel runtime_cleanup_template]
    mov rcx, runtime_cleanup_template_size
    rep movsb
    
    ; Store size
    mov dword [rel runtime_state.cleanup_code_size], runtime_cleanup_template_size
    
    mov eax, 1          ; Success
    jmp .done
    
.error:
    xor eax, eax        ; Failure
    
.done:
    pop r12
    pop rbx
    pop rbp
    ret

; Get entry point code and size
; Returns: rax = entry point code pointer, rdx = size
runtime_get_entry_point:
    push rbp
    mov rbp, rsp
    
    mov rax, qword [rel runtime_state.entry_point_code]
    mov edx, dword [rel runtime_state.entry_point_size]
    
    pop rbp
    ret

; Get total size of all runtime code
; Returns: rax = total size
runtime_get_total_runtime_size:
    push rbp
    mov rbp, rsp
    
    mov eax, dword [rel runtime_state.entry_point_size]
    add eax, dword [rel runtime_state.init_code_size]
    add eax, dword [rel runtime_state.cleanup_code_size]
    
    pop rbp
    ret

; Cleanup runtime generator
runtime_destroy_generator:
    push rbp
    mov rbp, rsp
    
    ; Free allocated memory
    mov rcx, qword [rel runtime_state.entry_point_code]
    test rcx, rcx
    jz .skip_entry_point
    call safe_free
    
.skip_entry_point:
    mov rcx, qword [rel runtime_state.init_code]
    test rcx, rcx
    jz .skip_init_code
    call safe_free
    
.skip_init_code:
    mov rcx, qword [rel runtime_state.cleanup_code]
    test rcx, rcx
    jz .skip_cleanup_code
    call safe_free
    
.skip_cleanup_code:
    ; Clear state
    mov qword [rel runtime_state.entry_point_code], 0
    mov dword [rel runtime_state.entry_point_size], 0
    mov qword [rel runtime_state.init_code], 0
    mov dword [rel runtime_state.init_code_size], 0
    mov qword [rel runtime_state.cleanup_code], 0
    mov dword [rel runtime_state.cleanup_code_size], 0
    
    pop rbp
    ret