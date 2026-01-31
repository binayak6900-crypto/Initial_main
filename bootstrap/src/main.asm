; Bootstrap Compiler Main Entry Point
; Main function for the xit bootstrap compiler (Windows version)

section .data
    usage_msg       db "Usage: xit-bootstrap <source_file> <output_file>", 13, 10, 0
    success_msg     db "Compilation successful", 13, 10, 0

section .text
    global main
    extern printf
    extern memory_init

main:
    push rbp
    mov rbp, rsp
    sub rsp, 32         ; Shadow space for Windows x64 calling convention
    
    ; Initialize memory management
    call memory_init
    
    ; Print success message
    lea rcx, [rel success_msg]
    call printf
    
    ; Return successfully
    mov rax, 0          ; return code
    
    add rsp, 32
    pop rbp
    ret