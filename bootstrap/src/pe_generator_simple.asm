; Simplified PE Generator using C file I/O
; This version uses C library functions instead of Windows API for better compatibility

section .data
    ; PE constants
    PE_SIGNATURE        equ 0x00004550      ; "PE\0\0"
    IMAGE_FILE_MACHINE_AMD64 equ 0x8664     ; x86-64 architecture
    IMAGE_FILE_EXECUTABLE_IMAGE equ 0x0002
    IMAGE_FILE_LARGE_ADDRESS_AWARE equ 0x0020
    IMAGE_SUBSYSTEM_CONSOLE equ 3
    
    ; Section characteristics
    IMAGE_SCN_CNT_CODE  equ 0x00000020
    IMAGE_SCN_CNT_INITIALIZED_DATA equ 0x00000040
    IMAGE_SCN_MEM_EXECUTE equ 0x20000000
    IMAGE_SCN_MEM_READ  equ 0x40000000
    IMAGE_SCN_MEM_WRITE equ 0x80000000
    
    ; File mode strings for fopen
    write_mode          db "wb", 0

section .bss
    ; PE generation state
    pe_state:
        .output_filename resq 1     ; Output filename
        .code_buffer    resq 1      ; Generated machine code
        .code_size      resd 1      ; Size of machine code
        .entry_point    resd 1      ; Entry point RVA
        
    ; PE file buffer (smaller for simplicity)
    pe_file_buffer      resb 4096
    pe_buffer_size      resd 1

section .text
    global pe_init
    global pe_set_code
    global pe_generate_executable
    global pe_destroy
    
    ; C library functions
    extern fopen
    extern fwrite
    extern fclose

; Initialize PE generator
; Parameters: rdi = output filename
pe_init:
    push rbp
    mov rbp, rsp
    
    ; Store filename
    mov qword [rel pe_state.output_filename], rdi
    
    ; Initialize state
    mov qword [rel pe_state.code_buffer], 0
    mov dword [rel pe_state.code_size], 0
    mov dword [rel pe_state.entry_point], 0x1000
    
    ; Clear PE buffer
    lea rdi, [rel pe_file_buffer]
    mov rcx, 4096
    xor al, al
    rep stosb
    
    mov dword [rel pe_buffer_size], 0
    
    mov eax, 1          ; Success
    
    pop rbp
    ret

; Set machine code to be embedded in PE
; Parameters: rdi = code buffer, rsi = code size
pe_set_code:
    push rbp
    mov rbp, rsp
    
    mov qword [rel pe_state.code_buffer], rdi
    mov dword [rel pe_state.code_size], esi
    
    pop rbp
    ret

; Generate complete PE executable file
pe_generate_executable:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    sub rsp, 32         ; Shadow space
    
    ; Build minimal PE file
    call build_minimal_pe_simple
    
    ; Write PE file using C library
    call write_pe_file_simple
    
    add rsp, 32
    pop r12
    pop rbx
    pop rbp
    ret

; Build minimal PE file (simplified version)
build_minimal_pe_simple:
    push rbp
    mov rbp, rsp
    push rdi
    
    lea rdi, [rel pe_file_buffer]
    
    ; DOS header - minimal
    mov word [rdi], 0x5A4D          ; "MZ"
    mov word [rdi + 60], 0x80       ; PE header offset
    
    ; Skip to PE header at offset 0x80
    add rdi, 0x80
    
    ; PE signature
    mov dword [rdi], PE_SIGNATURE
    add rdi, 4
    
    ; COFF header (20 bytes)
    mov word [rdi], IMAGE_FILE_MACHINE_AMD64    ; Machine
    mov word [rdi + 2], 1                       ; Number of sections
    mov dword [rdi + 4], 0                      ; Timestamp
    mov dword [rdi + 8], 0                      ; Symbol table pointer
    mov dword [rdi + 12], 0                     ; Number of symbols
    mov word [rdi + 16], 240                    ; Optional header size
    mov word [rdi + 18], IMAGE_FILE_EXECUTABLE_IMAGE | IMAGE_FILE_LARGE_ADDRESS_AWARE
    add rdi, 20
    
    ; Optional header (240 bytes)
    mov word [rdi], 0x20B           ; Magic (PE32+)
    mov byte [rdi + 2], 14          ; Major linker version
    mov byte [rdi + 3], 0           ; Minor linker version
    
    ; Code size
    mov eax, dword [rel pe_state.code_size]
    test eax, eax
    jnz .has_code_size
    mov eax, 16                     ; Default minimal code size
.has_code_size:
    mov dword [rdi + 4], eax        ; Size of code
    
    mov dword [rdi + 8], 0          ; Size of initialized data
    mov dword [rdi + 12], 0         ; Size of uninitialized data
    mov dword [rdi + 16], 0x1000    ; Entry point RVA
    mov dword [rdi + 20], 0x1000    ; Base of code
    
    ; Image base (8 bytes)
    mov qword [rdi + 24], 0x400000
    
    ; Alignments
    mov dword [rdi + 32], 0x1000    ; Section alignment
    mov dword [rdi + 36], 0x200     ; File alignment
    
    ; Versions
    mov word [rdi + 40], 6          ; Major OS version
    mov word [rdi + 42], 0          ; Minor OS version
    mov word [rdi + 44], 0          ; Major image version
    mov word [rdi + 46], 0          ; Minor image version
    mov word [rdi + 48], 6          ; Major subsystem version
    mov word [rdi + 50], 0          ; Minor subsystem version
    
    mov dword [rdi + 52], 0         ; Win32 version
    mov dword [rdi + 56], 0x2000    ; Size of image
    mov dword [rdi + 60], 0x200     ; Size of headers
    mov dword [rdi + 64], 0         ; Checksum
    mov word [rdi + 68], IMAGE_SUBSYSTEM_CONSOLE  ; Subsystem
    mov word [rdi + 70], 0          ; DLL characteristics
    
    ; Stack and heap sizes (8 bytes each)
    mov qword [rdi + 72], 0x100000  ; Stack reserve
    mov qword [rdi + 80], 0x1000    ; Stack commit
    mov qword [rdi + 88], 0x100000  ; Heap reserve
    mov qword [rdi + 96], 0x1000    ; Heap commit
    
    mov dword [rdi + 104], 0        ; Loader flags
    mov dword [rdi + 108], 16       ; Number of data directories
    
    ; Skip data directories (16 * 8 = 128 bytes, all zeros)
    add rdi, 112 + 128
    
    ; Section header for .text (40 bytes)
    mov qword [rdi], 0x747865742E   ; ".text" (little-endian)
    
    mov eax, dword [rel pe_state.code_size]
    test eax, eax
    jnz .set_virtual_size
    mov eax, 16                     ; Default size
.set_virtual_size:
    mov dword [rdi + 8], eax        ; Virtual size
    mov dword [rdi + 12], 0x1000    ; Virtual address
    
    ; Size of raw data (aligned to file alignment)
    add eax, 0x200 - 1
    and eax, ~(0x200 - 1)
    mov dword [rdi + 16], eax       ; Size of raw data
    
    mov dword [rdi + 20], 0x200     ; Pointer to raw data
    mov dword [rdi + 24], 0         ; Pointer to relocations
    mov dword [rdi + 28], 0         ; Pointer to line numbers
    mov word [rdi + 32], 0          ; Number of relocations
    mov word [rdi + 34], 0          ; Number of line numbers
    mov dword [rdi + 36], IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ
    
    ; Add code section at offset 0x200
    lea rdi, [rel pe_file_buffer]
    add rdi, 0x200
    
    ; Add actual code or default code
    mov rsi, qword [rel pe_state.code_buffer]
    test rsi, rsi
    jz .add_default_code
    
    mov rcx, qword [rel pe_state.code_size]
    test rcx, rcx
    jz .add_default_code
    
    ; Copy actual code
    rep movsb
    jmp .set_buffer_size
    
.add_default_code:
    ; Add minimal code: mov rax, 0; ret
    mov dword [rdi], 0xC031C048     ; mov rax, 0 (48 31 C0) + ret (C3)
    
.set_buffer_size:
    ; Set total buffer size
    mov dword [rel pe_buffer_size], 0x400   ; Headers + minimal code section
    
    pop rdi
    pop rbp
    ret

; Write PE file using C library functions
write_pe_file_simple:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    sub rsp, 32         ; Shadow space
    
    ; Open file for writing
    mov rcx, qword [rel pe_state.output_filename]  ; filename
    lea rdx, [rel write_mode]                      ; "wb"
    call fopen
    
    test rax, rax
    jz .write_failed
    
    mov r12, rax        ; Save file pointer
    
    ; Write PE data
    lea rcx, [rel pe_file_buffer]           ; buffer
    mov rdx, 1                              ; size of each element
    mov r8d, dword [rel pe_buffer_size]     ; number of elements
    mov r9, r12                             ; file pointer
    call fwrite
    
    ; Check if all bytes were written
    mov ebx, dword [rel pe_buffer_size]
    cmp eax, ebx
    jne .write_failed
    
    ; Close file
    mov rcx, r12
    call fclose
    
    mov eax, 1          ; Success
    jmp .done
    
.write_failed:
    ; Close file if it was opened
    test r12, r12
    jz .done
    mov rcx, r12
    call fclose
    
    xor eax, eax        ; Failure
    
.done:
    add rsp, 32
    pop r12
    pop rbx
    pop rbp
    ret

; Cleanup PE generator
pe_destroy:
    push rbp
    mov rbp, rsp
    
    ; Clear state
    mov qword [rel pe_state.output_filename], 0
    mov qword [rel pe_state.code_buffer], 0
    mov dword [rel pe_state.code_size], 0
    
    pop rbp
    ret