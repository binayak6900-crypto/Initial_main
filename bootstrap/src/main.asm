; Bootstrap Compiler Main Entry Point
; Main function for the xit bootstrap compiler (Windows version)

section .data
    usage_msg       db "Usage: xit-bootstrap <source_file> <output_file>", 13, 10, 0
    success_msg     db "Compilation successful", 13, 10, 0
    error_msg       db "Compilation failed", 13, 10, 0
    file_error_msg  db "Error: Could not read source file", 13, 10, 0
    
    ; Test source code for bootstrap
    test_source     db "func main() { var x = 42; return x; }", 0
    test_source_len equ $ - test_source - 1

section .bss
    ; Token array for parser (max 1000 tokens)
    token_array     resb 32000      ; 32 bytes per token * 1000 tokens
    token_count     resd 1

section .text
    global main
    extern printf
    extern memory_init
    extern lexer_init
    extern lexer_next_token
    extern lexer_destroy
    extern parser_init
    extern parser_parse_program
    extern parser_emit_executable
    extern parser_destroy

main:
    push rbp
    mov rbp, rsp
    sub rsp, 32         ; Shadow space for Windows x64 calling convention
    
    ; Initialize memory management
    call memory_init
    
    ; For bootstrap, use test source code
    ; In full implementation, would read from command line arguments
    
    ; Initialize lexer with test source
    lea rcx, [rel test_source]
    mov rdx, test_source_len
    call lexer_init
    
    ; Tokenize source code
    call tokenize_source
    test eax, eax
    jz .compilation_failed
    
    ; Initialize parser with tokens
    lea rdi, [rel token_array]
    call parser_init
    
    ; Parse program and generate machine code
    call parser_parse_program
    test eax, eax
    jz .compilation_failed
    
    ; Generate executable file
    mov rdi, output_filename
    call parser_emit_executable
    test eax, eax
    jz .compilation_failed
    
    ; Cleanup
    call parser_destroy
    call lexer_destroy
    
    ; Print success message
    lea rcx, [rel success_msg]
    call printf
    
    ; Return successfully
    mov rax, 0          ; return code
    jmp .done
    
.compilation_failed:
    ; Cleanup
    call parser_destroy
    call lexer_destroy
    
    ; Print error message
    lea rcx, [rel error_msg]
    call printf
    
    ; Return with error
    mov rax, 1          ; error return code
    
.done:
    add rsp, 32
    pop rbp
    ret

; Tokenize source code into token array
tokenize_source:
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    push r13
    
    ; Initialize token count
    mov dword [rel token_count], 0
    
    ; Get token array pointer
    lea r12, [rel token_array]
    mov r13d, 0         ; token index
    
.tokenize_loop:
    ; Check if we have space for more tokens
    cmp r13d, 1000
    jae .tokenize_done
    
    ; Get next token
    call lexer_next_token
    test rax, rax
    jz .tokenize_done
    
    mov rbx, rax        ; token pointer
    
    ; Check for EOF token
    mov eax, dword [rbx]    ; token type
    cmp eax, 6              ; TOKEN_EOF
    je .tokenize_done
    
    ; Copy token to array (32 bytes per token)
    mov rdi, r12
    mov rsi, rbx
    mov rcx, 32
    rep movsb
    
    ; Move to next token slot
    add r12, 32
    inc r13d
    
    jmp .tokenize_loop
    
.tokenize_done:
    ; Store final token count
    mov dword [rel token_count], r13d
    
    ; Return success if we got at least one token
    test r13d, r13d
    setnz al
    movzx eax, al
    
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret

section .data
    output_filename db "test_output.exe", 0