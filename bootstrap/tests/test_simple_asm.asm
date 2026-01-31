; Simple test assembly function
section .text
    global test_simple_function

test_simple_function:
    push rbp
    mov rbp, rsp
    
    ; Just return success
    mov rax, 42
    
    pop rbp
    ret