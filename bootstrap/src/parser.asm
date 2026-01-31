; Bootstrap Parser Implementation
; Direct binary generation parser for xit bootstrap compiler

section .data
    ; Parser constants
    MAX_CODE_SIZE       equ 65536   ; Maximum generated code size
    MAX_SYMBOLS         equ 1024    ; Maximum symbol table entries
    MAX_SCOPES          equ 64      ; Maximum nested scope depth
    SYMBOL_NAME_SIZE    equ 64      ; Maximum symbol name length

    ; Symbol types
    SYMBOL_VARIABLE     equ 0
    SYMBOL_FUNCTION     equ 1
    SYMBOL_PARAMETER    equ 2

    ; Variable types
    TYPE_INT            equ 0
    TYPE_STRING         equ 1
    TYPE_BOOL           equ 2

section .bss
    ; Parser state structure for direct code generation
    parser_state:
        .tokens         resq 1      ; Pointer to token array
        .current_token  resq 1      ; Current token index
        .code_buffer    resq 1      ; Generated code buffer
        .code_size      resd 1      ; Current code size
        .code_capacity  resd 1      ; Code buffer capacity
        .symbol_table   resq 1      ; Symbol table pointer
        .scope_depth    resd 1      ; Current scope depth
        .current_func   resq 1      ; Current function being parsed
        .stack_offset   resd 1      ; Current stack offset for locals
        .label_counter  resd 1      ; Counter for generating unique labels
        .error_flag     resd 1      ; Error flag for parsing errors

    ; Symbol table entry structure (80 bytes per entry)
    symbol_entry:
        .name           resb SYMBOL_NAME_SIZE  ; Symbol name (64 bytes)
        .type           resd 1      ; Symbol type (variable/function/parameter)
        .data_type      resd 1      ; Data type (int/string/bool)
        .scope_level    resd 1      ; Scope level where defined
        .stack_offset   resd 1      ; Stack offset (for variables)
        .is_used        resd 1      ; Usage flag for unused variable detection

    ; Symbol table storage
    symbol_table        resb MAX_SYMBOLS * 80  ; 80 bytes per symbol

    ; Scope management stack
    scope_stack:
        .levels         resd MAX_SCOPES    ; Scope depth levels
        .symbol_counts  resd MAX_SCOPES    ; Number of symbols per scope
        .stack_offsets  resd MAX_SCOPES    ; Stack offset per scope

    ; Code generation buffer
    code_buffer         resb MAX_CODE_SIZE

    ; Current symbol count
    symbol_count        resd 1

section .text
    global parser_init
    global parser_parse_program
    global parser_emit_executable
    global parser_destroy
    global symbol_table_init
    global symbol_table_add
    global symbol_table_lookup
    global scope_enter
    global scope_exit

; Initialize parser with token array
; Parameters: rdi = tokens pointer
parser_init:
    push rbp
    mov rbp, rsp
    
    ; Initialize parser state
    mov qword [rel parser_state.tokens], rdi
    mov qword [rel parser_state.current_token], 0
    lea rax, [rel code_buffer]
    mov qword [rel parser_state.code_buffer], rax
    mov dword [rel parser_state.code_size], 0
    mov dword [rel parser_state.code_capacity], MAX_CODE_SIZE
    mov dword [rel parser_state.scope_depth], 0
    mov qword [rel parser_state.current_func], 0
    mov dword [rel parser_state.stack_offset], 0
    mov dword [rel parser_state.label_counter], 0
    mov dword [rel parser_state.error_flag], 0
    
    ; Initialize symbol table
    call symbol_table_init
    
    ; Initialize scope management
    mov dword [rel scope_stack.levels], 0
    mov dword [rel scope_stack.symbol_counts], 0
    mov dword [rel scope_stack.stack_offsets], 0
    
    pop rbp
    ret

; Initialize symbol table
symbol_table_init:
    push rbp
    mov rbp, rsp
    
    ; Set symbol table pointer
    lea rax, [rel symbol_table]
    mov qword [rel parser_state.symbol_table], rax
    
    ; Clear symbol count
    mov dword [rel symbol_count], 0
    
    ; Clear all symbol entries
    lea rdi, [rel symbol_table]
    mov rcx, MAX_SYMBOLS * 80
    xor al, al
    rep stosb
    
    pop rbp
    ret

; Add symbol to symbol table
; Parameters: rdi = symbol name, rsi = symbol type, rdx = data type
; Returns: rax = symbol index, or -1 if table full
symbol_table_add:
    push rbp
    mov rbp, rsp
    push rbx
    push rcx
    push rdx
    push rsi
    push rdi
    
    ; Check if symbol table is full
    mov eax, dword [rel symbol_count]
    cmp eax, MAX_SYMBOLS
    jae .table_full
    
    ; Calculate symbol entry address
    mov rbx, rax                    ; rbx = symbol index
    imul rax, 80                    ; 80 bytes per entry
    lea rcx, [rel symbol_table]
    add rcx, rax                    ; rcx = symbol entry address
    
    ; Copy symbol name (max 63 chars + null terminator)
    mov rsi, rdi                    ; source = symbol name
    mov rdi, rcx                    ; dest = symbol entry name field
    mov rdx, SYMBOL_NAME_SIZE - 1   ; max chars to copy
    
.copy_name:
    test rdx, rdx
    jz .name_copied
    mov al, byte [rsi]
    test al, al
    jz .name_copied
    mov byte [rdi], al
    inc rsi
    inc rdi
    dec rdx
    jmp .copy_name
    
.name_copied:
    mov byte [rdi], 0               ; null terminator
    
    ; Restore parameters
    pop rdi
    pop rsi
    pop rdx
    
    ; Set symbol type and data type
    mov dword [rcx + SYMBOL_NAME_SIZE], esi      ; symbol type
    mov dword [rcx + SYMBOL_NAME_SIZE + 4], edx  ; data type
    
    ; Set scope level
    mov eax, dword [rel parser_state.scope_depth]
    mov dword [rcx + SYMBOL_NAME_SIZE + 8], eax
    
    ; Set stack offset for variables
    cmp esi, SYMBOL_VARIABLE
    jne .not_variable
    mov eax, dword [rel parser_state.stack_offset]
    mov dword [rcx + SYMBOL_NAME_SIZE + 12], eax
    ; Update stack offset for next variable
    sub dword [rel parser_state.stack_offset], 8  ; 8 bytes per variable
    jmp .set_usage
    
.not_variable:
    mov dword [rcx + SYMBOL_NAME_SIZE + 12], 0   ; no stack offset
    
.set_usage:
    mov dword [rcx + SYMBOL_NAME_SIZE + 16], 0   ; initially unused
    
    ; Increment symbol count
    inc dword [rel symbol_count]
    
    ; Return symbol index
    mov rax, rbx
    jmp .done
    
.table_full:
    mov rax, -1
    
.done:
    pop rcx
    pop rbx
    pop rbp
    ret

; Lookup symbol in symbol table
; Parameters: rdi = symbol name
; Returns: rax = symbol index, or -1 if not found
symbol_table_lookup:
    push rbp
    mov rbp, rsp
    push rbx
    push rcx
    push rdx
    push rsi
    
    ; Get symbol count
    mov ecx, dword [rel symbol_count]
    test ecx, ecx
    jz .not_found
    
    ; Start from most recent symbols (reverse order for scope resolution)
    dec ecx
    mov rbx, rcx                    ; rbx = current symbol index
    
.search_loop:
    ; Calculate symbol entry address
    mov rax, rbx
    imul rax, 80
    lea rsi, [rel symbol_table]
    add rsi, rax                    ; rsi = symbol entry address
    
    ; Compare symbol names
    push rdi
    push rsi
    call strcmp_symbols
    pop rsi
    pop rdi
    
    test eax, eax
    jz .found                       ; strings match
    
    ; Try next symbol
    test rbx, rbx
    jz .not_found
    dec rbx
    jmp .search_loop
    
.found:
    ; Mark symbol as used
    mov dword [rsi + SYMBOL_NAME_SIZE + 16], 1
    mov rax, rbx
    jmp .done
    
.not_found:
    mov rax, -1
    
.done:
    pop rsi
    pop rdx
    pop rcx
    pop rbx
    pop rbp
    ret

; Compare two null-terminated strings
; Parameters: rdi = string1, rsi = string2
; Returns: rax = 0 if equal, non-zero if different
strcmp_symbols:
    push rbp
    mov rbp, rsp
    
.compare_loop:
    mov al, byte [rdi]
    mov bl, byte [rsi]
    cmp al, bl
    jne .different
    
    test al, al
    jz .equal                       ; both strings ended
    
    inc rdi
    inc rsi
    jmp .compare_loop
    
.equal:
    xor rax, rax
    jmp .done
    
.different:
    mov rax, 1
    
.done:
    pop rbp
    ret

; Enter new scope
scope_enter:
    push rbp
    mov rbp, rsp
    
    ; Increment scope depth
    inc dword [rel parser_state.scope_depth]
    
    ; Get current scope depth
    mov eax, dword [rel parser_state.scope_depth]
    cmp eax, MAX_SCOPES
    jae .scope_overflow
    
    ; Save current state in scope stack
    dec eax                         ; convert to 0-based index
    lea rbx, [rel scope_stack.levels]
    mov dword [rbx + rax*4], eax
    mov ebx, dword [rel symbol_count]
    lea rcx, [rel scope_stack.symbol_counts]
    mov dword [rcx + rax*4], ebx
    mov ebx, dword [rel parser_state.stack_offset]
    lea rcx, [rel scope_stack.stack_offsets]
    mov dword [rcx + rax*4], ebx
    
    jmp .done
    
.scope_overflow:
    ; Set error flag
    mov dword [rel parser_state.error_flag], 1
    
.done:
    pop rbp
    ret

; Exit current scope
scope_exit:
    push rbp
    mov rbp, rsp
    
    ; Check if we can exit scope
    mov eax, dword [rel parser_state.scope_depth]
    test eax, eax
    jz .scope_underflow
    
    ; Remove symbols from current scope
    dec eax                         ; convert to 0-based index
    lea rbx, [rel scope_stack.symbol_counts]
    mov ebx, dword [rbx + rax*4]
    mov dword [rel symbol_count], ebx
    
    ; Restore stack offset
    lea rbx, [rel scope_stack.stack_offsets]
    mov ebx, dword [rbx + rax*4]
    mov dword [rel parser_state.stack_offset], ebx
    
    ; Decrement scope depth
    dec dword [rel parser_state.scope_depth]
    
    jmp .done
    
.scope_underflow:
    ; Set error flag
    mov dword [rel parser_state.error_flag], 1
    
.done:
    pop rbp
    ret

; Parse program and generate machine code directly
parser_parse_program:
    push rbp
    mov rbp, rsp
    
    ; Enter global scope
    call scope_enter
    
    ; Parse functions until EOF
.parse_loop:
    call get_current_token
    test rax, rax
    jz .done
    
    ; Check token type
    mov rbx, rax
    mov eax, dword [rbx]            ; token type
    cmp eax, 6                      ; TOKEN_EOF
    je .done
    
    ; Parse function
    call parse_function
    test eax, eax
    jz .parse_error
    
    jmp .parse_loop
    
.parse_error:
    mov dword [rel parser_state.error_flag], 1
    
.done:
    ; Exit global scope
    call scope_exit
    
    ; Return success/failure
    mov eax, dword [rel parser_state.error_flag]
    xor eax, 1                      ; invert error flag (0=error, 1=success)
    
    pop rbp
    ret

; Get current token
; Returns: rax = pointer to current token, or 0 if at end
get_current_token:
    push rbp
    mov rbp, rsp
    
    ; Get token array and current index
    mov rdi, qword [rel parser_state.tokens]
    mov rsi, qword [rel parser_state.current_token]
    
    ; Calculate token address (assuming 32-byte token structure)
    imul rsi, 32
    add rdi, rsi
    
    ; Always return the token pointer, even for EOF
    mov rax, rdi
    
    pop rbp
    ret

; Advance to next token
advance_token:
    inc qword [rel parser_state.current_token]
    ret

; Parse function definition and emit code directly
; Grammar: func IDENTIFIER '(' params? ')' type? '{' statement* '}'
parse_function:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Expect 'func' keyword
    call get_current_token
    test rax, rax
    jz .error
    
    mov rbx, rax
    mov eax, dword [rbx]            ; token type
    cmp eax, 3                      ; TOKEN_KEYWORD
    jne .error
    
    ; Check if it's actually 'func' keyword
    mov rdi, qword [rbx + 8]        ; token value pointer (offset 8)
    mov ecx, dword [rbx + 16]       ; token value length (offset 16)
    cmp ecx, 4                      ; 'func' is 4 characters
    jne .error
    
    ; Simple check - just verify first character is 'f'
    mov al, byte [rdi]
    cmp al, 'f'
    jne .error
    
    call advance_token
    
    ; Expect function name (identifier)
    call get_current_token
    test rax, rax
    jz .error
    
    mov rbx, rax
    mov eax, dword [rbx]            ; token type
    cmp eax, 0                      ; TOKEN_IDENTIFIER
    jne .error
    
    ; Add function to symbol table
    mov rdi, qword [rbx + 8]        ; token value pointer
    mov rsi, SYMBOL_FUNCTION
    mov rdx, TYPE_INT               ; assume int return type for now
    call symbol_table_add
    
    ; Store current function
    mov qword [rel parser_state.current_func], rax
    
    call advance_token
    
    ; Emit function prologue
    call emit_function_prologue
    
    ; Expect '('
    mov al, '('
    call expect_specific_delimiter
    test eax, eax
    jz .error
    
    ; Parse parameters (simplified - skip for now)
    ; TODO: implement parameter parsing
    
    ; Expect ')'
    mov al, ')'
    call expect_specific_delimiter
    test eax, eax
    jz .error
    
    ; Optional return type (skip for now)
    
    ; Expect '{'
    mov al, '{'
    call expect_specific_delimiter
    test eax, eax
    jz .error
    
    ; Enter function scope
    call scope_enter
    
    ; Parse statements
.parse_statements:
    call get_current_token
    test rax, rax
    jz .error
    
    mov rbx, rax
    mov eax, dword [rbx]            ; token type
    cmp eax, 5                      ; TOKEN_DELIMITER
    jne .parse_statement
    
    ; Check if it's '}'
    mov rdi, qword [rbx + 8]        ; token value pointer
    mov al, byte [rdi]
    cmp al, '}'
    je .end_function
    
.parse_statement:
    call parse_statement
    test eax, eax
    jz .error
    jmp .parse_statements
    
.end_function:
    call advance_token              ; consume '}'
    
    ; Exit function scope
    call scope_exit
    
    ; Emit function epilogue
    call emit_function_epilogue
    
    mov eax, 1                      ; success
    jmp .done
    
.error:
    xor eax, eax                    ; failure
    
.done:
    pop rbx
    pop rbp
    ret

; Parse statement
; Grammar: assignment | if_stmt | while_stmt | return_stmt | var_decl
parse_statement:
    push rbp
    mov rbp, rsp
    
    call get_current_token
    test rax, rax
    jz .error
    
    mov rbx, rax
    mov eax, dword [rbx]            ; token type
    
    ; Check for keywords
    cmp eax, 3                      ; TOKEN_KEYWORD
    je .parse_keyword_statement
    
    ; Check for identifier (assignment)
    cmp eax, 0                      ; TOKEN_IDENTIFIER
    je .parse_assignment
    
    ; Unknown statement
    jmp .error
    
.parse_keyword_statement:
    ; Check which keyword
    mov rdi, qword [rbx + 8]        ; token value pointer
    
    ; Check for 'var'
    mov al, byte [rdi]
    cmp al, 'v'
    je .check_var
    
    ; Check for 'return'
    cmp al, 'r'
    je .check_return
    
    ; Check for 'if'
    cmp al, 'i'
    je .check_if
    
    jmp .error
    
.check_var:
    ; Simple check for 'var' (just check first char for now)
    call parse_var_declaration
    jmp .done
    
.check_return:
    ; Simple check for 'return'
    call parse_return_statement
    jmp .done
    
.check_if:
    ; Simple check for 'if'
    call parse_if_statement
    jmp .done
    
.parse_assignment:
    call parse_assignment_statement
    jmp .done
    
.error:
    xor eax, eax
    jmp .exit
    
.done:
    mov eax, 1
    
.exit:
    pop rbp
    ret

; Parse variable declaration: var IDENTIFIER = expression
parse_var_declaration:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Skip 'var' keyword
    call advance_token
    
    ; Expect identifier
    call get_current_token
    test rax, rax
    jz .error
    
    mov rbx, rax
    mov eax, dword [rbx]
    cmp eax, 0                      ; TOKEN_IDENTIFIER
    jne .error
    
    ; Add variable to symbol table
    mov rdi, qword [rbx + 8]        ; variable name
    mov rsi, SYMBOL_VARIABLE
    mov rdx, TYPE_INT               ; assume int for now
    call symbol_table_add
    
    ; Store variable index
    push rax
    
    call advance_token
    
    ; Expect '='
    call get_current_token
    test rax, rax
    jz .error_pop
    
    mov rbx, rax
    mov eax, dword [rbx]
    cmp eax, 4                      ; TOKEN_OPERATOR
    jne .error_pop
    
    call advance_token
    
    ; Parse expression
    call parse_expression
    test eax, eax
    jz .error_pop
    
    ; Emit code to store result in variable
    pop rax                         ; variable index
    call emit_variable_store
    
    mov eax, 1
    jmp .done
    
.error_pop:
    pop rax
.error:
    xor eax, eax
    
.done:
    pop rbx
    pop rbp
    ret

; Parse return statement: return expression?
parse_return_statement:
    push rbp
    mov rbp, rsp
    
    ; Skip 'return' keyword
    call advance_token
    
    ; Check if there's an expression
    call get_current_token
    test rax, rax
    jz .return_void
    
    ; Parse expression
    call parse_expression
    test eax, eax
    jz .error
    
    ; Emit return with value
    call emit_return_value
    jmp .done
    
.return_void:
    ; Emit void return
    call emit_return_void
    
.done:
    mov eax, 1
    jmp .exit
    
.error:
    xor eax, eax
    
.exit:
    pop rbp
    ret

; Parse if statement: if '(' expression ')' '{' statements '}'
parse_if_statement:
    push rbp
    mov rbp, rsp
    
    ; Skip 'if' keyword
    call advance_token
    
    ; Expect '('
    mov al, '('
    call expect_specific_delimiter
    test eax, eax
    jz .error
    
    ; Parse condition expression
    call parse_expression
    test eax, eax
    jz .error
    
    ; Expect ')'
    mov al, ')'
    call expect_specific_delimiter
    test eax, eax
    jz .error
    
    ; Emit conditional jump
    call emit_conditional_jump
    
    ; Expect '{'
    mov al, '{'
    call expect_specific_delimiter
    test eax, eax
    jz .error
    
    ; Parse statements
    call scope_enter
    
.parse_if_statements:
    call get_current_token
    test rax, rax
    jz .error_scope
    
    mov rbx, rax
    mov eax, dword [rbx]
    cmp eax, 5                      ; TOKEN_DELIMITER
    jne .parse_if_statement_body
    
    ; Check for '}'
    mov rdi, qword [rbx + 8]
    mov al, byte [rdi]
    cmp al, '}'
    je .end_if
    
.parse_if_statement_body:
    call parse_statement
    test eax, eax
    jz .error_scope
    jmp .parse_if_statements
    
.end_if:
    call advance_token              ; consume '}'
    call scope_exit
    
    ; Emit end of conditional
    call emit_conditional_end
    
    mov eax, 1
    jmp .done
    
.error_scope:
    call scope_exit
.error:
    xor eax, eax
    
.done:
    pop rbp
    ret

; Parse assignment: IDENTIFIER = expression
parse_assignment_statement:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Get identifier
    call get_current_token
    mov rbx, rax
    
    ; Look up variable in symbol table
    mov rdi, qword [rbx + 8]        ; variable name
    call symbol_table_lookup
    cmp rax, -1
    je .error                       ; undefined variable
    
    push rax                        ; save variable index
    
    call advance_token
    
    ; Expect '='
    call get_current_token
    test rax, rax
    jz .error_pop
    
    call advance_token
    
    ; Parse expression
    call parse_expression
    test eax, eax
    jz .error_pop
    
    ; Emit assignment code
    pop rax                         ; variable index
    call emit_variable_store
    
    mov eax, 1
    jmp .done
    
.error_pop:
    pop rax
.error:
    xor eax, eax
    
.done:
    pop rbx
    pop rbp
    ret

; Parse expression (simplified - just numbers and identifiers for now)
parse_expression:
    push rbp
    mov rbp, rsp
    
    call get_current_token
    test rax, rax
    jz .error
    
    mov rbx, rax
    mov eax, dword [rbx]            ; token type
    
    ; Check for number
    cmp eax, 1                      ; TOKEN_NUMBER
    je .parse_number
    
    ; Check for identifier
    cmp eax, 0                      ; TOKEN_IDENTIFIER
    je .parse_identifier
    
    jmp .error
    
.parse_number:
    ; Emit code to load immediate value
    mov rdi, qword [rbx + 8]        ; number string
    call emit_load_immediate
    call advance_token
    jmp .done
    
.parse_identifier:
    ; Look up variable
    mov rdi, qword [rbx + 8]        ; variable name
    call symbol_table_lookup
    cmp rax, -1
    je .error                       ; undefined variable
    
    ; Emit code to load variable
    call emit_variable_load
    call advance_token
    jmp .done
    
.error:
    xor eax, eax
    jmp .exit
    
.done:
    mov eax, 1
    
.exit:
    pop rbp
    ret

; Expect delimiter token
; Returns: eax = 1 if found, 0 if not
expect_delimiter:
    push rbp
    mov rbp, rsp
    
    call get_current_token
    test rax, rax
    jz .error
    
    mov rbx, rax
    mov eax, dword [rbx]
    cmp eax, 5                      ; TOKEN_DELIMITER
    jne .error
    
    call advance_token
    mov eax, 1
    jmp .done
    
.error:
    xor eax, eax
    
.done:
    pop rbp
    ret

; Expect specific delimiter character
; Parameters: al = expected character
; Returns: eax = 1 if found, 0 if not
expect_specific_delimiter:
    push rbp
    mov rbp, rsp
    push rbx
    push rcx
    
    mov cl, al                      ; save expected character
    
    call get_current_token
    test rax, rax
    jz .error
    
    mov rbx, rax
    mov eax, dword [rbx]
    cmp eax, 5                      ; TOKEN_DELIMITER
    jne .error
    
    ; Check if it's the right character
    mov rdi, qword [rbx + 8]        ; token value pointer
    mov al, byte [rdi]
    cmp al, cl
    jne .error
    
    call advance_token
    mov eax, 1
    jmp .done
    
.error:
    xor eax, eax
    
.done:
    pop rcx
    pop rbx
    pop rbp
    ret

; Direct x86-64 code emission functions

; Emit function prologue
emit_function_prologue:
    push rbp
    mov rbp, rsp
    
    ; Standard x86-64 function prologue:
    ; push rbp
    ; mov rbp, rsp
    ; sub rsp, <local_space>
    
    ; Emit: push rbp (0x55)
    mov al, 0x55
    call emit_byte
    
    ; Emit: mov rbp, rsp (0x48 0x89 0xe5)
    mov al, 0x48
    call emit_byte
    mov al, 0x89
    call emit_byte
    mov al, 0xe5
    call emit_byte
    
    ; Reserve space for locals (simplified - reserve 64 bytes)
    ; Emit: sub rsp, 64 (0x48 0x83 0xec 0x40)
    mov al, 0x48
    call emit_byte
    mov al, 0x83
    call emit_byte
    mov al, 0xec
    call emit_byte
    mov al, 0x40
    call emit_byte
    
    pop rbp
    ret

; Emit function epilogue
emit_function_epilogue:
    push rbp
    mov rbp, rsp
    
    ; Standard x86-64 function epilogue:
    ; mov rsp, rbp
    ; pop rbp
    ; ret
    
    ; Emit: mov rsp, rbp (0x48 0x89 0xec)
    mov al, 0x48
    call emit_byte
    mov al, 0x89
    call emit_byte
    mov al, 0xec
    call emit_byte
    
    ; Emit: pop rbp (0x5d)
    mov al, 0x5d
    call emit_byte
    
    ; Emit: ret (0xc3)
    mov al, 0xc3
    call emit_byte
    
    pop rbp
    ret

; Emit immediate value load into rax
; Parameters: rdi = number string
emit_load_immediate:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Convert string to integer (simplified)
    call string_to_int
    mov rbx, rax                    ; rbx = integer value
    
    ; Emit: mov rax, immediate (0x48 0xb8 + 8-byte immediate)
    mov al, 0x48
    call emit_byte
    mov al, 0xb8
    call emit_byte
    
    ; Emit 8-byte immediate value (little-endian)
    mov rax, rbx
    call emit_qword
    
    pop rbx
    pop rbp
    ret

; Emit variable load into rax
; Parameters: rax = variable index
emit_variable_load:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Get variable stack offset
    mov rbx, rax
    imul rbx, 80                    ; 80 bytes per symbol entry
    lea rcx, [rel symbol_table]
    add rcx, rbx
    mov ebx, dword [rcx + SYMBOL_NAME_SIZE + 12]  ; stack offset
    
    ; Emit: mov rax, [rbp + offset] (0x48 0x8b 0x45 + offset)
    mov al, 0x48
    call emit_byte
    mov al, 0x8b
    call emit_byte
    mov al, 0x45
    call emit_byte
    mov al, bl                      ; offset (simplified to 1 byte)
    call emit_byte
    
    pop rbx
    pop rbp
    ret

; Emit variable store from rax
; Parameters: rax = variable index
emit_variable_store:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Get variable stack offset
    mov rbx, rax
    imul rbx, 80                    ; 80 bytes per symbol entry
    lea rcx, [rel symbol_table]
    add rcx, rbx
    mov ebx, dword [rcx + SYMBOL_NAME_SIZE + 12]  ; stack offset
    
    ; Emit: mov [rbp + offset], rax (0x48 0x89 0x45 + offset)
    mov al, 0x48
    call emit_byte
    mov al, 0x89
    call emit_byte
    mov al, 0x45
    call emit_byte
    mov al, bl                      ; offset (simplified to 1 byte)
    call emit_byte
    
    pop rbx
    pop rbp
    ret

; Emit return with value in rax
emit_return_value:
    push rbp
    mov rbp, rsp
    
    ; Value is already in rax, just emit epilogue
    call emit_function_epilogue
    
    pop rbp
    ret

; Emit void return
emit_return_void:
    push rbp
    mov rbp, rsp
    
    ; Set rax to 0
    ; Emit: xor rax, rax (0x48 0x31 0xc0)
    mov al, 0x48
    call emit_byte
    mov al, 0x31
    call emit_byte
    mov al, 0xc0
    call emit_byte
    
    call emit_function_epilogue
    
    pop rbp
    ret

; Emit conditional jump (simplified)
emit_conditional_jump:
    push rbp
    mov rbp, rsp
    
    ; Test rax and jump if zero
    ; Emit: test rax, rax (0x48 0x85 0xc0)
    mov al, 0x48
    call emit_byte
    mov al, 0x85
    call emit_byte
    mov al, 0xc0
    call emit_byte
    
    ; Emit: jz <offset> (0x74 + 1-byte offset, simplified)
    mov al, 0x74
    call emit_byte
    mov al, 0x10                    ; placeholder offset
    call emit_byte
    
    pop rbp
    ret

; Emit end of conditional
emit_conditional_end:
    ; Placeholder - would need proper label management
    ret

; Emit single byte to code buffer
; Parameters: al = byte to emit
emit_byte:
    push rbp
    mov rbp, rsp
    push rbx
    
    ; Check if buffer has space
    mov ebx, dword [rel parser_state.code_size]
    cmp ebx, dword [rel parser_state.code_capacity]
    jae .buffer_full
    
    ; Get code buffer address
    mov rcx, qword [rel parser_state.code_buffer]
    add rcx, rbx
    
    ; Store byte
    mov byte [rcx], al
    
    ; Increment code size
    inc dword [rel parser_state.code_size]
    
    jmp .done
    
.buffer_full:
    ; Set error flag
    mov dword [rel parser_state.error_flag], 1
    
.done:
    pop rbx
    pop rbp
    ret

; Emit 8-byte value to code buffer (little-endian)
; Parameters: rax = qword to emit
emit_qword:
    push rbp
    mov rbp, rsp
    push rbx
    
    mov rbx, rax
    
    ; Emit bytes in little-endian order
    mov al, bl
    call emit_byte
    shr rbx, 8
    mov al, bl
    call emit_byte
    shr rbx, 8
    mov al, bl
    call emit_byte
    shr rbx, 8
    mov al, bl
    call emit_byte
    shr rbx, 8
    mov al, bl
    call emit_byte
    shr rbx, 8
    mov al, bl
    call emit_byte
    shr rbx, 8
    mov al, bl
    call emit_byte
    shr rbx, 8
    mov al, bl
    call emit_byte
    
    pop rbx
    pop rbp
    ret

; Convert string to integer (simplified)
; Parameters: rdi = string pointer
; Returns: rax = integer value
string_to_int:
    push rbp
    mov rbp, rsp
    push rbx
    push rcx
    
    xor rax, rax                    ; result
    xor rbx, rbx                    ; current digit
    mov rcx, 10                     ; base
    
.convert_loop:
    mov bl, byte [rdi]
    test bl, bl
    jz .done
    
    ; Check if digit
    cmp bl, '0'
    jb .done
    cmp bl, '9'
    ja .done
    
    ; Convert digit
    sub bl, '0'
    
    ; result = result * 10 + digit
    mul rcx
    add rax, rbx
    
    inc rdi
    jmp .convert_loop
    
.done:
    pop rcx
    pop rbx
    pop rbp
    ret
; Emit executable file
; Parameters: rdi = filename pointer
parser_emit_executable:
    push rbp
    mov rbp, rsp
    
    ; For now, just write raw machine code to file
    ; In a complete implementation, this would generate PE/ELF/Mach-O format
    
    ; TODO: Implement proper executable format generation
    ; This is a placeholder that would:
    ; 1. Create appropriate executable header (PE/ELF/Mach-O)
    ; 2. Add code section with generated machine code
    ; 3. Add data section if needed
    ; 4. Set up entry point
    ; 5. Write to file
    
    mov eax, 1                      ; success for now
    
    pop rbp
    ret

; Cleanup parser resources
parser_destroy:
    push rbp
    mov rbp, rsp
    
    ; Clear parser state
    mov qword [rel parser_state.tokens], 0
    mov qword [rel parser_state.current_token], 0
    mov dword [rel parser_state.code_size], 0
    mov dword [rel parser_state.scope_depth], 0
    mov dword [rel parser_state.error_flag], 0
    mov dword [rel symbol_count], 0
    
    pop rbp
    ret