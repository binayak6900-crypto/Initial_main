; Bootstrap Lexer Implementation
; Tokenizes xit source code for bootstrap compiler

section .data
    ; Token type constants
    TOKEN_IDENTIFIER    equ 0
    TOKEN_NUMBER        equ 1
    TOKEN_STRING        equ 2
    TOKEN_KEYWORD       equ 3
    TOKEN_OPERATOR      equ 4
    TOKEN_DELIMITER     equ 5
    TOKEN_EOF          equ 6

    ; Keywords for recognition
    keywords:
        db "func", 0
        db "var", 0
        db "if", 0
        db "else", 0
        db "while", 0
        db "for", 0
        db "return", 0
        db "true", 0
        db "false", 0
        db 0                        ; End marker

section .data
    ; Lexer state structure (initialized to zero)
    lexer_state:
        .source_ptr     dq 0        ; Pointer to current position in source
        .source_end     dq 0        ; Pointer to end of source
        .source_start   dq 0        ; Pointer to start of source (for value extraction)
        .line_number    dd 0        ; Current line number
        .column_number  dd 0        ; Current column number
        .current_char   db 0        ; Current character being processed
        .padding        db 0, 0, 0  ; Padding for alignment

    ; Token structure (initialized to zero)
    current_token:
        .type           dd 0        ; Token type (4 bytes)
        .padding1       dd 0        ; Padding for pointer alignment
        .value_ptr      dq 0        ; Pointer to token value string (8 bytes)
        .value_len      dd 0        ; Length of token value (4 bytes)
        .line           dd 0        ; Line number (4 bytes)
        .column         dd 0        ; Column number (4 bytes)
        .padding2       dd 0        ; Padding to make 32 bytes total

section .text
    global lexer_init
    global lexer_next_token
    global lexer_destroy
    global lexer_get_current_token
    global test_is_identifier_start

; Initialize lexer with source code
; Parameters: rcx = source code pointer, rdx = source length (Windows x64 calling convention)
lexer_init:
    push rbp
    mov rbp, rsp
    
    ; Store source pointers
    mov qword [rel lexer_state.source_ptr], rcx
    mov qword [rel lexer_state.source_start], rcx
    add rdx, rcx
    mov qword [rel lexer_state.source_end], rdx
    
    ; Initialize position tracking
    mov dword [rel lexer_state.line_number], 1
    mov dword [rel lexer_state.column_number], 1
    
    ; Load first character
    cmp rcx, rdx
    jae .end_of_source
    mov al, byte [rcx]
    mov byte [rel lexer_state.current_char], al
    jmp .done
    
.end_of_source:
    mov byte [rel lexer_state.current_char], 0
    
.done:
    pop rbp
    ret

; Get current token structure
; Returns: rax = pointer to current token structure
lexer_get_current_token:
    lea rax, [rel current_token]
    ret

; Advance to next character in source
; Modifies: lexer_state.source_ptr, current_char, line_number, column_number
advance_char:
    push rbp
    mov rbp, rsp
    
    ; Get current source pointer
    mov rdi, qword [rel lexer_state.source_ptr]
    mov rsi, qword [rel lexer_state.source_end]
    
    ; Check if at end
    cmp rdi, rsi
    jae .end_of_source
    
    ; Check for newline to update line/column
    mov al, byte [rel lexer_state.current_char]
    cmp al, 10                      ; '\n'
    jne .not_newline
    
    ; Update line number and reset column
    inc dword [rel lexer_state.line_number]
    mov dword [rel lexer_state.column_number], 1
    jmp .advance
    
.not_newline:
    ; Update column number
    inc dword [rel lexer_state.column_number]
    
.advance:
    ; Move to next character
    inc rdi
    mov qword [rel lexer_state.source_ptr], rdi
    
    ; Load next character
    cmp rdi, rsi
    jae .end_of_source
    mov al, byte [rdi]
    mov byte [rel lexer_state.current_char], al
    jmp .done
    
.end_of_source:
    mov byte [rel lexer_state.current_char], 0
    
.done:
    pop rbp
    ret

; Skip whitespace characters
skip_whitespace:
    push rbp
    mov rbp, rsp
    
.loop:
    mov al, byte [rel lexer_state.current_char]
    
    ; Check for space, tab, newline, carriage return
    cmp al, 32                      ; space
    je .skip
    cmp al, 9                       ; tab
    je .skip
    cmp al, 10                      ; newline
    je .skip
    cmp al, 13                      ; carriage return
    je .skip
    cmp al, 0                       ; end of source
    je .done
    
    ; Not whitespace, done
    jmp .done
    
.skip:
    call advance_char
    jmp .loop
    
.done:
    pop rbp
    ret

; Check if character is alphabetic or underscore (valid identifier start)
; Parameters: al = character
; Returns: al = 1 if valid identifier start, 0 if not
is_identifier_start_alt:
    ; Check for underscore
    cmp al, '_'
    je .valid
    
    ; Check for uppercase letters (A-Z)
    cmp al, 'A'
    jb .invalid
    cmp al, 'Z'
    jbe .valid
    
    ; Check for lowercase letters (a-z)
    cmp al, 'a'
    jb .invalid
    cmp al, 'z'
    jbe .valid
    
.invalid:
    mov al, 0
    ret
    
.valid:
    mov al, 1
    ret

; Check if character is alphanumeric or underscore (valid identifier continuation)
; Parameters: al = character
; Returns: ZF set if valid identifier character
is_identifier_char:
    ; Check identifier start characters first
    push rax
    call is_identifier_start_alt
    cmp al, 1
    pop rax
    je .valid
    
    ; Check for digits (0-9)
    cmp al, '0'
    jb .invalid
    cmp al, '9'
    jbe .valid
    
.invalid:
    ; Clear zero flag
    or al, 1
    ret
    
.valid:
    ; Set zero flag
    cmp al, al
    ret

; Check if character is a digit
; Parameters: al = character
; Returns: ZF set if digit
is_digit:
    cmp al, '0'
    jb .invalid
    cmp al, '9'
    jbe .valid
    
.invalid:
    or al, 1
    ret
    
.valid:
    cmp al, al
    ret

; Check if character is a delimiter
; Parameters: al = character
; Returns: ZF set if delimiter
is_delimiter_char:
    cmp al, '('
    je .valid
    cmp al, ')'
    je .valid
    cmp al, '{'
    je .valid
    cmp al, '}'
    je .valid
    cmp al, '['
    je .valid
    cmp al, ']'
    je .valid
    cmp al, ';'
    je .valid
    cmp al, ','
    je .valid
    cmp al, '.'
    je .valid
    cmp al, ':'
    je .valid
    
    ; Not a delimiter
    or al, 1
    ret
    
.valid:
    cmp al, al
    ret

; Get next token from source
; Returns: rax = pointer to current token structure
lexer_next_token:
    push rbp
    mov rbp, rsp
    
    ; Skip whitespace
    call skip_whitespace
    
    ; Get current character
    mov al, byte [rel lexer_state.current_char]
    
    ; Check for end of file
    test al, al
    jz .eof_token
    
    ; Store current position for token value (preserve al in bl)
    mov bl, al                      ; Save character
    mov rdi, qword [rel lexer_state.source_ptr]
    mov qword [rel current_token.value_ptr], rdi
    mov eax, dword [rel lexer_state.line_number]
    mov dword [rel current_token.line], eax
    mov eax, dword [rel lexer_state.column_number]
    mov dword [rel current_token.column], eax
    
    ; Debug: ensure column is never 0
    cmp eax, 0
    jne .column_ok
    mov dword [rel current_token.column], 1
.column_ok:
    
    ; Restore character
    mov al, bl
    
    ; Check token type - proper character classification
    ; Check for letters (a-z, A-Z) and underscore
    cmp al, 'a'
    jb .check_uppercase
    cmp al, 'z'
    jbe .identifier_token
    
.check_uppercase:
    cmp al, 'A'
    jb .check_underscore
    cmp al, 'Z'
    jbe .identifier_token
    
.check_underscore:
    cmp al, '_'
    je .identifier_token
    
    ; Check for digits (0-9)
    cmp al, '0'
    jb .check_string
    cmp al, '9'
    jbe .number_token
    
.check_string:
    
    cmp al, '"'
    je .string_token
    
    ; Check for operators and delimiters
    jmp .operator_or_delimiter
    
.eof_token:
    mov dword [rel current_token.type], TOKEN_EOF
    mov qword [rel current_token.value_ptr], 0
    mov dword [rel current_token.value_len], 0
    jmp .done
    
.identifier_token:
    call read_identifier
    jmp .done
    
.number_token:
    call read_number
    jmp .done
    
.string_token:
    call read_string
    jmp .done
    
.operator_or_delimiter:
    call read_operator_or_delimiter
    jmp .done
    
.done:
    lea rax, [rel current_token]
    pop rbp
    ret

; Read identifier token
read_identifier:
    push rbp
    mov rbp, rsp
    push rbx
    push rcx
    
    ; Store start position
    mov rbx, qword [rel lexer_state.source_ptr]
    mov qword [rel current_token.value_ptr], rbx
    
    ; Count characters
    xor rcx, rcx
    
.loop:
    mov al, byte [rel lexer_state.current_char]
    call is_identifier_char
    jnz .end_identifier             ; jnz means NOT zero flag, so NOT valid identifier char
    
    inc rcx
    call advance_char
    jmp .loop
    
.end_identifier:
    ; Store length
    mov dword [rel current_token.value_len], ecx
    
    ; Check if it's a keyword
    call check_keyword
    test eax, eax
    jnz .keyword
    
    ; It's an identifier
    mov dword [rel current_token.type], TOKEN_IDENTIFIER
    jmp .done
    
.keyword:
    mov dword [rel current_token.type], TOKEN_KEYWORD
    
.done:
    pop rcx
    pop rbx
    pop rbp
    ret

; Read number token
read_number:
    push rbp
    mov rbp, rsp
    push rbx
    push rcx
    
    ; Store start position
    mov rbx, qword [rel lexer_state.source_ptr]
    mov qword [rel current_token.value_ptr], rbx
    
    ; Count characters
    xor rcx, rcx
    
.loop:
    mov al, byte [rel lexer_state.current_char]
    call is_digit
    jnz .end_number
    
    inc rcx
    call advance_char
    jmp .loop
    
.end_number:
    ; Store length and type
    mov dword [rel current_token.value_len], ecx
    mov dword [rel current_token.type], TOKEN_NUMBER
    
    pop rcx
    pop rbx
    pop rbp
    ret

; Read string token
read_string:
    push rbp
    mov rbp, rsp
    push rbx
    push rcx
    
    ; Skip opening quote
    call advance_char
    
    ; Store start position (after quote)
    mov rbx, qword [rel lexer_state.source_ptr]
    mov qword [rel current_token.value_ptr], rbx
    
    ; Count characters
    xor rcx, rcx
    
.loop:
    mov al, byte [rel lexer_state.current_char]
    test al, al
    jz .unterminated_string
    
    cmp al, '"'
    je .end_string
    
    inc rcx
    call advance_char
    jmp .loop
    
.end_string:
    ; Skip closing quote
    call advance_char
    
    ; Store length and type
    mov dword [rel current_token.value_len], ecx
    mov dword [rel current_token.type], TOKEN_STRING
    jmp .done
    
.unterminated_string:
    ; Error: unterminated string - for now, treat as string
    mov dword [rel current_token.value_len], ecx
    mov dword [rel current_token.type], TOKEN_STRING
    
.done:
    pop rcx
    pop rbx
    pop rbp
    ret

; Read operator or delimiter token
read_operator_or_delimiter:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Store start position
    mov rbx, qword [rel lexer_state.source_ptr]
    mov qword [rel current_token.value_ptr], rbx
    
    ; Get current character
    mov al, byte [rel lexer_state.current_char]
    
    ; Check for two-character operators first
    cmp al, '='
    je .check_equals
    cmp al, '!'
    je .check_not_equals
    cmp al, '<'
    je .check_less_equal
    cmp al, '>'
    je .check_greater_equal
    cmp al, '&'
    je .check_and
    cmp al, '|'
    je .check_or
    
    ; Single character operator/delimiter
    call advance_char
    mov dword [rel current_token.value_len], 1
    
    ; Determine if operator or delimiter
    mov al, byte [rbx]
    call is_delimiter_char
    jz .delimiter
    
    mov dword [rel current_token.type], TOKEN_OPERATOR
    jmp .done
    
.delimiter:
    mov dword [rel current_token.type], TOKEN_DELIMITER
    jmp .done
    
.check_equals:
    call advance_char
    mov al, byte [rel lexer_state.current_char]
    cmp al, '='
    jne .single_char
    call advance_char
    mov dword [rel current_token.value_len], 2
    mov dword [rel current_token.type], TOKEN_OPERATOR
    jmp .done
    
.check_not_equals:
    call advance_char
    mov al, byte [rel lexer_state.current_char]
    cmp al, '='
    jne .single_char
    call advance_char
    mov dword [rel current_token.value_len], 2
    mov dword [rel current_token.type], TOKEN_OPERATOR
    jmp .done
    
.check_less_equal:
    call advance_char
    mov al, byte [rel lexer_state.current_char]
    cmp al, '='
    jne .single_char
    call advance_char
    mov dword [rel current_token.value_len], 2
    mov dword [rel current_token.type], TOKEN_OPERATOR
    jmp .done
    
.check_greater_equal:
    call advance_char
    mov al, byte [rel lexer_state.current_char]
    cmp al, '='
    jne .single_char
    call advance_char
    mov dword [rel current_token.value_len], 2
    mov dword [rel current_token.type], TOKEN_OPERATOR
    jmp .done
    
.check_and:
    call advance_char
    mov al, byte [rel lexer_state.current_char]
    cmp al, '&'
    jne .single_char
    call advance_char
    mov dword [rel current_token.value_len], 2
    mov dword [rel current_token.type], TOKEN_OPERATOR
    jmp .done
    
.check_or:
    call advance_char
    mov al, byte [rel lexer_state.current_char]
    cmp al, '|'
    jne .single_char
    call advance_char
    mov dword [rel current_token.value_len], 2
    mov dword [rel current_token.type], TOKEN_OPERATOR
    jmp .done
    
.single_char:
    mov dword [rel current_token.value_len], 1
    mov dword [rel current_token.type], TOKEN_OPERATOR
    jmp .done
    
.done:
    pop rbx
    pop rbp
    ret

; Check if current token is a keyword
; Returns: eax = 1 if keyword, 0 if not
check_keyword:
    push rbp
    mov rbp, rsp
    push rbx
    push rcx
    push rdx
    push rsi
    push rdi
    
    ; Get token value
    mov rsi, qword [rel current_token.value_ptr]
    mov ecx, dword [rel current_token.value_len]
    
    ; Point to keywords list
    lea rdi, [rel keywords]
    
.check_loop:
    ; Check if end of keywords list
    mov al, byte [rdi]
    test al, al
    jz .not_keyword
    
    ; Compare keyword
    push rsi
    push rdi
    push rcx
    
    ; Compare strings
    xor rdx, rdx                    ; Character counter
    
.compare_loop:
    cmp edx, ecx
    jae .check_end
    
    mov al, byte [rsi + rdx]
    mov bl, byte [rdi + rdx]
    cmp al, bl
    jne .next_keyword
    
    inc rdx
    jmp .compare_loop
    
.check_end:
    ; Check if keyword ends here
    mov bl, byte [rdi + rdx]
    test bl, bl
    jz .found_keyword
    
.next_keyword:
    pop rcx
    pop rdi
    pop rsi
    
    ; Skip to next keyword
.skip_loop:
    mov al, byte [rdi]
    inc rdi
    test al, al
    jnz .skip_loop
    
    jmp .check_loop
    
.found_keyword:
    pop rcx
    pop rdi
    pop rsi
    mov eax, 1
    jmp .done
    
.not_keyword:
    xor eax, eax
    
.done:
    pop rdi
    pop rsi
    pop rdx
    pop rcx
    pop rbx
    pop rbp
    ret

; Test function for is_identifier_start
; Parameters: rcx = character (Windows x64 calling convention)
; Returns: rax = 1 if valid identifier start, 0 if not
test_is_identifier_start:
    push rbp
    mov rbp, rsp
    
    mov al, cl                      ; Get character from parameter
    call is_identifier_start_alt
    movzx rax, al                   ; Zero-extend al to rax
    
    pop rbp
    ret

; Cleanup lexer resources
lexer_destroy:
    ; Nothing to cleanup for bootstrap version
    ret