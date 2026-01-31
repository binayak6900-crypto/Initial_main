; Debug version of lexer_init
section .text
    global test_lexer_init_debug

test_lexer_init_debug:
    push rbp
    mov rbp, rsp
    
    ; Windows x64 calling convention: rcx = source pointer, rdx = length
    
    ; Test if we can access the parameters
    test rcx, rcx
    jz .error
    
    test rdx, rdx
    jz .error
    
    ; Try to read first character
    mov al, byte [rcx]
    
    ; Return success
    mov rax, 1
    jmp .done
    
.error:
    mov rax, 0
    
.done:
    pop rbp
    ret