; Windows PE Executable Format Generation
; Generates native Windows PE executables from machine code

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
    
    ; PE header sizes
    DOS_HEADER_SIZE     equ 64
    DOS_STUB_SIZE       equ 64
    PE_HEADER_SIZE      equ 24
    OPTIONAL_HEADER_SIZE equ 240
    SECTION_HEADER_SIZE equ 40
    
    ; Base addresses
    IMAGE_BASE          equ 0x400000
    SECTION_ALIGNMENT   equ 0x1000
    FILE_ALIGNMENT      equ 0x200

section .bss
    ; PE generation state
    pe_state:
        .output_file    resq 1      ; Output file handle
        .code_buffer    resq 1      ; Generated machine code
        .code_size      resd 1      ; Size of machine code
        .entry_point    resd 1      ; Entry point RVA
        .image_size     resd 1      ; Total image size
        .headers_size   resd 1      ; Size of all headers
        .symbol_table   resq 1      ; Symbol table for relocations
        .reloc_count    resd 1      ; Number of relocations
        
    ; PE file buffer (64KB should be enough for bootstrap)
    pe_file_buffer      resb 65536
    pe_buffer_size      resd 1

section .text
    global pe_init
    global pe_set_code
    global pe_add_symbol
    global pe_generate_executable
    global pe_destroy
    
    ; Windows API imports
    extern CreateFileA
    extern WriteFile
    extern CloseHandle
    extern GetLastError

; Initialize PE generator
; Parameters: rdi = output filename
pe_init:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Store filename (simplified - assume it stays valid)
    mov qword [rel pe_state.output_file], rdi
    
    ; Initialize state
    mov qword [rel pe_state.code_buffer], 0
    mov dword [rel pe_state.code_size], 0
    mov dword [rel pe_state.entry_point], 0x1000  ; Default entry point RVA
    mov dword [rel pe_state.image_size], 0
    mov dword [rel pe_state.headers_size], 0
    mov qword [rel pe_state.symbol_table], 0
    mov dword [rel pe_state.reloc_count], 0
    
    ; Clear PE buffer
    lea rdi, [rel pe_file_buffer]
    mov rcx, 65536
    xor al, al
    rep stosb
    
    mov dword [rel pe_buffer_size], 0
    
    mov eax, 1          ; Success
    
    pop rbx
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

; Add symbol for relocation (simplified)
; Parameters: rdi = symbol name, rsi = address
pe_add_symbol:
    push rbp
    mov rbp, rsp
    
    ; Simplified - just increment relocation count
    inc dword [rel pe_state.reloc_count]
    
    pop rbp
    ret

; Generate complete PE executable file
pe_generate_executable:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    push r13
    push r14
    push r15
    sub rsp, 32         ; Shadow space
    
    ; For now, create a minimal PE file with just headers
    ; This is a simplified version to get basic functionality working
    
    ; Calculate basic layout
    mov dword [rel pe_state.headers_size], 0x400    ; 1KB for headers
    mov dword [rel pe_state.image_size], 0x2000     ; 8KB total image
    
    ; Build minimal PE file
    call build_minimal_pe
    
    ; Write PE file to disk
    call write_pe_file
    
    mov eax, 1          ; Success
    
    add rsp, 32
    pop r15
    pop r14
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret

; Build minimal PE file
build_minimal_pe:
    push rbp
    mov rbp, rsp
    push rdi
    push rsi
    
    lea rdi, [rel pe_file_buffer]
    
    ; DOS header - minimal version
    mov word [rdi], 0x5A4D          ; "MZ"
    mov word [rdi + 60], 0x80       ; PE header offset
    
    ; Skip to PE header
    add rdi, 0x80
    
    ; PE signature
    mov dword [rdi], 0x00004550     ; "PE\0\0"
    add rdi, 4
    
    ; COFF header
    mov word [rdi], 0x8664          ; Machine (AMD64)
    mov word [rdi + 2], 1           ; Number of sections
    mov dword [rdi + 4], 0          ; Timestamp
    mov dword [rdi + 8], 0          ; Symbol table pointer
    mov dword [rdi + 12], 0         ; Number of symbols
    mov word [rdi + 16], 240        ; Optional header size
    mov word [rdi + 18], 0x0022     ; Characteristics
    add rdi, 20
    
    ; Optional header (simplified)
    mov word [rdi], 0x20B           ; Magic (PE32+)
    mov byte [rdi + 2], 14          ; Major linker version
    mov byte [rdi + 3], 0           ; Minor linker version
    
    ; Size of code
    mov eax, dword [rel pe_state.code_size]
    test eax, eax
    jnz .has_code
    mov eax, 8                      ; Minimal code size
.has_code:
    mov dword [rdi + 4], eax
    
    mov dword [rdi + 8], 0          ; Size of initialized data
    mov dword [rdi + 12], 0         ; Size of uninitialized data
    mov dword [rdi + 16], 0x1000    ; Entry point RVA
    mov dword [rdi + 20], 0x1000    ; Base of code
    
    ; Image base
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
    mov dword [rdi + 60], 0x400     ; Size of headers
    mov dword [rdi + 64], 0         ; Checksum
    mov word [rdi + 68], 3          ; Subsystem (console)
    mov word [rdi + 70], 0          ; DLL characteristics
    
    ; Stack and heap sizes
    mov qword [rdi + 72], 0x100000  ; Stack reserve
    mov qword [rdi + 80], 0x1000    ; Stack commit
    mov qword [rdi + 88], 0x100000  ; Heap reserve
    mov qword [rdi + 96], 0x1000    ; Heap commit
    
    mov dword [rdi + 104], 0        ; Loader flags
    mov dword [rdi + 108], 16       ; Number of data directories
    
    ; Skip data directories (all zeros)
    add rdi, 112 + 16*8
    
    ; Section header for .text
    mov qword [rdi], 0x747865742E   ; ".text"
    mov eax, dword [rel pe_state.code_size]
    test eax, eax
    jnz .set_code_size
    mov eax, 8                      ; Minimal code size
.set_code_size:
    mov dword [rdi + 8], eax        ; Virtual size
    mov dword [rdi + 12], 0x1000    ; Virtual address
    mov dword [rdi + 16], 0x200     ; Size of raw data (aligned)
    mov dword [rdi + 20], 0x400     ; Pointer to raw data
    mov dword [rdi + 24], 0         ; Pointer to relocations
    mov dword [rdi + 28], 0         ; Pointer to line numbers
    mov word [rdi + 32], 0          ; Number of relocations
    mov word [rdi + 34], 0          ; Number of line numbers
    mov dword [rdi + 36], 0x60000020 ; Characteristics (code, execute, read)
    
    ; Set buffer size to include headers and minimal code section
    mov dword [rel pe_buffer_size], 0x600  ; Headers + minimal code section
    
    ; Add minimal code at offset 0x400
    lea rdi, [rel pe_file_buffer]
    add rdi, 0x400
    
    ; Check if we have actual code to embed
    mov rsi, qword [rel pe_state.code_buffer]
    test rsi, rsi
    jz .add_default_code
    
    ; Copy actual generated code
    mov rcx, qword [rel pe_state.code_size]
    test rcx, rcx
    jz .add_default_code
    rep movsb
    jmp .done_code
    
.add_default_code:
    ; Add minimal code that just returns 0
    ; mov rax, 0; ret
    mov byte [rdi], 0x48        ; REX.W prefix
    mov byte [rdi + 1], 0x31    ; xor
    mov byte [rdi + 2], 0xC0    ; rax, rax
    mov byte [rdi + 3], 0xC3    ; ret
    
.done_code:
    pop rsi
    pop rdi
    pop rbp
    ret

; Calculate PE layout and sizes
calculate_pe_layout:
    push rbp
    mov rbp, rsp
    
    ; Calculate headers size
    mov eax, DOS_HEADER_SIZE
    add eax, DOS_STUB_SIZE
    add eax, 4              ; PE signature
    add eax, PE_HEADER_SIZE
    add eax, OPTIONAL_HEADER_SIZE
    add eax, SECTION_HEADER_SIZE  ; One section for now
    
    ; Align to file alignment
    add eax, FILE_ALIGNMENT - 1
    and eax, ~(FILE_ALIGNMENT - 1)
    mov dword [rel pe_state.headers_size], eax
    
    ; Calculate image size
    mov ebx, eax            ; Headers size
    mov ecx, dword [rel pe_state.code_size]
    add ecx, SECTION_ALIGNMENT - 1
    and ecx, ~(SECTION_ALIGNMENT - 1)
    add ebx, ecx
    mov dword [rel pe_state.image_size], ebx
    
    pop rbp
    ret

; Build DOS header
build_dos_header:
    push rbp
    mov rbp, rsp
    push rdi
    push rsi
    
    lea rdi, [rel pe_file_buffer]
    
    ; DOS signature "MZ"
    mov word [rdi], 0x5A4D
    
    ; Bytes on last page
    mov word [rdi + 2], 0x90
    
    ; Pages in file
    mov word [rdi + 4], 0x03
    
    ; Relocations
    mov word [rdi + 6], 0x00
    
    ; Size of header in paragraphs
    mov word [rdi + 8], 0x04
    
    ; Minimum extra paragraphs
    mov word [rdi + 10], 0x00
    
    ; Maximum extra paragraphs
    mov word [rdi + 12], 0xFFFF
    
    ; Initial SS
    mov word [rdi + 14], 0x00
    
    ; Initial SP
    mov word [rdi + 16], 0xB8
    
    ; Checksum
    mov word [rdi + 18], 0x00
    
    ; Initial IP
    mov word [rdi + 20], 0x00
    
    ; Initial CS
    mov word [rdi + 22], 0x00
    
    ; Relocation table offset
    mov word [rdi + 24], 0x40
    
    ; Overlay number
    mov word [rdi + 26], 0x00
    
    ; PE header offset (at offset 60)
    mov dword [rdi + 60], DOS_HEADER_SIZE + DOS_STUB_SIZE
    
    ; Add DOS stub (simple message)
    add rdi, DOS_HEADER_SIZE
    mov rsi, dos_stub_msg
    mov rcx, dos_stub_msg_len
    rep movsb
    
    ; Update buffer size
    add dword [rel pe_buffer_size], DOS_HEADER_SIZE + DOS_STUB_SIZE
    
    pop rsi
    pop rdi
    pop rbp
    ret

; Build PE header
build_pe_header:
    push rbp
    mov rbp, rsp
    push rdi
    
    ; Get position after DOS header and stub
    lea rdi, [rel pe_file_buffer]
    add rdi, DOS_HEADER_SIZE + DOS_STUB_SIZE
    
    ; PE signature
    mov dword [rdi], PE_SIGNATURE
    add rdi, 4
    
    ; COFF header
    ; Machine type
    mov word [rdi], IMAGE_FILE_MACHINE_AMD64
    
    ; Number of sections
    mov word [rdi + 2], 1
    
    ; Time/date stamp (simplified - use 0)
    mov dword [rdi + 4], 0
    
    ; Pointer to symbol table
    mov dword [rdi + 8], 0
    
    ; Number of symbols
    mov dword [rdi + 12], 0
    
    ; Size of optional header
    mov word [rdi + 16], OPTIONAL_HEADER_SIZE
    
    ; Characteristics
    mov word [rdi + 18], IMAGE_FILE_EXECUTABLE_IMAGE | IMAGE_FILE_LARGE_ADDRESS_AWARE
    
    ; Update buffer size
    add dword [rel pe_buffer_size], 4 + PE_HEADER_SIZE
    
    pop rdi
    pop rbp
    ret

; Build optional header
build_optional_header:
    push rbp
    mov rbp, rsp
    push rdi
    
    ; Get position after PE header
    lea rdi, [rel pe_file_buffer]
    add rdi, DOS_HEADER_SIZE + DOS_STUB_SIZE + 4 + PE_HEADER_SIZE
    
    ; Magic (PE32+)
    mov word [rdi], 0x20B
    
    ; Linker version
    mov byte [rdi + 2], 14
    mov byte [rdi + 3], 0
    
    ; Size of code
    mov eax, dword [rel pe_state.code_size]
    add eax, SECTION_ALIGNMENT - 1
    and eax, ~(SECTION_ALIGNMENT - 1)
    mov dword [rdi + 4], eax
    
    ; Size of initialized data
    mov dword [rdi + 8], 0
    
    ; Size of uninitialized data
    mov dword [rdi + 12], 0
    
    ; Entry point RVA
    mov eax, dword [rel pe_state.entry_point]
    mov dword [rdi + 16], eax
    
    ; Base of code
    mov dword [rdi + 20], 0x1000
    
    ; Image base
    mov qword [rdi + 24], IMAGE_BASE
    
    ; Section alignment
    mov dword [rdi + 32], SECTION_ALIGNMENT
    
    ; File alignment
    mov dword [rdi + 36], FILE_ALIGNMENT
    
    ; OS version
    mov word [rdi + 40], 6
    mov word [rdi + 42], 0
    
    ; Image version
    mov word [rdi + 44], 0
    mov word [rdi + 46], 0
    
    ; Subsystem version
    mov word [rdi + 48], 6
    mov word [rdi + 50], 0
    
    ; Win32 version
    mov dword [rdi + 52], 0
    
    ; Size of image
    mov eax, dword [rel pe_state.image_size]
    mov dword [rdi + 56], eax
    
    ; Size of headers
    mov eax, dword [rel pe_state.headers_size]
    mov dword [rdi + 60], eax
    
    ; Checksum
    mov dword [rdi + 64], 0
    
    ; Subsystem (console)
    mov word [rdi + 68], IMAGE_SUBSYSTEM_CONSOLE
    
    ; DLL characteristics
    mov word [rdi + 70], 0
    
    ; Stack reserve/commit
    mov qword [rdi + 72], 0x100000
    mov qword [rdi + 80], 0x1000
    
    ; Heap reserve/commit
    mov qword [rdi + 88], 0x100000
    mov qword [rdi + 96], 0x1000
    
    ; Loader flags
    mov dword [rdi + 104], 0
    
    ; Number of data directories
    mov dword [rdi + 108], 16
    
    ; Data directories (all zeros for bootstrap)
    add rdi, 112
    mov rcx, 16 * 8         ; 16 directories * 8 bytes each
    xor al, al
    rep stosb
    
    ; Update buffer size
    add dword [rel pe_buffer_size], OPTIONAL_HEADER_SIZE
    
    pop rdi
    pop rbp
    ret

; Build section headers
build_section_headers:
    push rbp
    mov rbp, rsp
    push rdi
    push rsi
    
    ; Get position after optional header
    lea rdi, [rel pe_file_buffer]
    add rdi, DOS_HEADER_SIZE + DOS_STUB_SIZE + 4 + PE_HEADER_SIZE + OPTIONAL_HEADER_SIZE
    
    ; .text section
    ; Name
    mov qword [rdi], 0x747865742E        ; ".text" (little-endian)
    
    ; Virtual size
    mov eax, dword [rel pe_state.code_size]
    mov dword [rdi + 8], eax
    
    ; Virtual address
    mov dword [rdi + 12], 0x1000
    
    ; Size of raw data (aligned)
    add eax, FILE_ALIGNMENT - 1
    and eax, ~(FILE_ALIGNMENT - 1)
    mov dword [rdi + 16], eax
    
    ; Pointer to raw data
    mov eax, dword [rel pe_state.headers_size]
    mov dword [rdi + 20], eax
    
    ; Pointer to relocations
    mov dword [rdi + 24], 0
    
    ; Pointer to line numbers
    mov dword [rdi + 28], 0
    
    ; Number of relocations
    mov word [rdi + 32], 0
    
    ; Number of line numbers
    mov word [rdi + 34], 0
    
    ; Characteristics
    mov dword [rdi + 36], IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ
    
    ; Update buffer size
    add dword [rel pe_buffer_size], SECTION_HEADER_SIZE
    
    pop rsi
    pop rdi
    pop rbp
    ret

; Build code section
build_code_section:
    push rbp
    mov rbp, rsp
    push rdi
    push rsi
    
    ; Get position for code section
    lea rdi, [rel pe_file_buffer]
    mov eax, dword [rel pe_state.headers_size]
    add rdi, rax
    
    ; Copy machine code
    mov rsi, qword [rel pe_state.code_buffer]
    mov rcx, qword [rel pe_state.code_size]
    test rsi, rsi
    jz .no_code
    test rcx, rcx
    jz .no_code
    
    rep movsb
    
    ; Update buffer size
    mov eax, dword [rel pe_state.code_size]
    add eax, FILE_ALIGNMENT - 1
    and eax, ~(FILE_ALIGNMENT - 1)
    add dword [rel pe_buffer_size], eax
    
.no_code:
    pop rsi
    pop rdi
    pop rbp
    ret

; Write PE file to disk
write_pe_file:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    push r13
    sub rsp, 48         ; Shadow space + alignment
    
    ; Create output file
    mov rcx, qword [rel pe_state.output_file]  ; filename
    mov rdx, 0x40000000                        ; GENERIC_WRITE
    mov r8, 0                                  ; share mode
    mov r9, 0                                  ; security attributes
    mov qword [rsp + 32], 2                    ; CREATE_ALWAYS
    mov qword [rsp + 40], 0x80                 ; FILE_ATTRIBUTE_NORMAL
    mov qword [rsp + 48], 0                    ; template file
    call CreateFileA
    
    cmp rax, -1         ; INVALID_HANDLE_VALUE
    je .write_failed
    
    mov r12, rax        ; Save file handle
    
    ; Write PE file data
    mov rcx, r12                    ; file handle
    lea rdx, [rel pe_file_buffer]   ; buffer
    mov r8d, dword [rel pe_buffer_size]  ; bytes to write
    lea r9, [rsp + 32]              ; bytes written (reuse stack space)
    mov qword [rsp + 32], 0         ; overlapped
    call WriteFile
    
    test eax, eax
    jz .write_failed
    
    ; Close file
    mov rcx, r12
    call CloseHandle
    
    mov eax, 1          ; Success
    jmp .done
    
.write_failed:
    ; Close file if it was opened
    cmp r12, -1
    je .done
    mov rcx, r12
    call CloseHandle
    
    xor eax, eax        ; Failure
    
.done:
    add rsp, 48
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret

; Cleanup PE generator
pe_destroy:
    push rbp
    mov rbp, rsp
    
    ; Clear state
    mov qword [rel pe_state.output_file], 0
    mov qword [rel pe_state.code_buffer], 0
    mov dword [rel pe_state.code_size], 0
    
    pop rbp
    ret

section .data
    ; DOS stub message
    dos_stub_msg db "This program cannot be run in DOS mode.", 13, 10, "$"
    dos_stub_msg_len equ $ - dos_stub_msg