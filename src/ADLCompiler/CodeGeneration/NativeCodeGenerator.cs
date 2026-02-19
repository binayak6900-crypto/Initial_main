using System;
using System.Collections.Generic;
using System.Linq;
using SharedUtilities.Models;
using ADLCompiler.SemanticAnalysis;

namespace ADLCompiler.CodeGeneration
{
    /// <summary>
    /// Generates native ARM/x86 machine code for C++-style code
    /// </summary>
    public class NativeCodeGenerator
    {
        private readonly NativeCodeSection _nativeSection;
        private readonly Architecture _targetArchitecture;
        private readonly MemoryManagementInstructions? _memoryManagement;
        private NativeFunction? _currentFunction;
        private List<byte> _currentCode;
        private readonly Dictionary<string, int> _localVariables;
        private int _stackOffset;
        private readonly List<Relocation> _relocations;

        public NativeCodeGenerator(NativeCodeSection nativeSection, Architecture targetArchitecture, MemoryManagementInstructions? memoryManagement = null)
        {
            _nativeSection = nativeSection;
            _targetArchitecture = targetArchitecture;
            _memoryManagement = memoryManagement;
            _currentCode = new List<byte>();
            _localVariables = new Dictionary<string, int>();
            _relocations = new List<Relocation>();
        }

        /// <summary>
        /// Generate native code for a compilation unit
        /// </summary>
        public void GenerateCompilationUnit(CompilationUnit unit)
        {
            // Generate code for namespace functions
            if (unit.Namespace != null)
            {
                foreach (var function in unit.Namespace.Functions)
                {
                    GenerateFunction(function);
                }
            }

            // Generate code for C++ style classes
            foreach (var classDecl in unit.Classes)
            {
                if (IsCppStyle(classDecl))
                {
                    GenerateClass(classDecl);
                }
            }
        }

        /// <summary>
        /// Determine if a class uses C++ style (has pointers, namespaces, etc.)
        /// </summary>
        private bool IsCppStyle(ClassDeclaration classDecl)
        {
            // Check if any methods use pointers or C++ specific features
            foreach (var method in classDecl.Methods)
            {
                if (method.ReturnType.IsPointer || method.Parameters.Any(p => p.ParameterType.IsPointer))
                {
                    return true;
                }
            }

            // Check if any fields use pointers
            foreach (var field in classDecl.Fields)
            {
                if (field.FieldType.IsPointer)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Generate native code for a class
        /// </summary>
        private void GenerateClass(ClassDeclaration classDecl)
        {
            foreach (var method in classDecl.Methods)
            {
                if (!method.IsAbstract && !method.IsNative)
                {
                    GenerateFunction(method, classDecl.Name);
                }
            }
        }

        /// <summary>
        /// Generate native code for a function/method
        /// </summary>
        private void GenerateFunction(MethodDeclaration method, string? className = null)
        {
            // Create function name (mangled for C++)
            var functionName = className != null ? $"{className}::{method.Name}" : method.Name;

            _currentFunction = new NativeFunction
            {
                FunctionName = functionName,
                Signature = GetFunctionSignature(method),
                Architecture = _targetArchitecture,
                Convention = GetCallingConvention(_targetArchitecture)
            };

            // Reset state
            _currentCode.Clear();
            _localVariables.Clear();
            _relocations.Clear();
            _stackOffset = 0;

            // Generate function prologue
            GeneratePrologue(method);

            // Allocate space for parameters
            int paramOffset = 0;
            foreach (var param in method.Parameters)
            {
                _localVariables[param.Name] = paramOffset;
                paramOffset += GetTypeSize(param.ParameterType);
            }

            // Generate function body
            if (method.Body != null)
            {
                GenerateStatement(method.Body);
            }

            // Generate function epilogue
            GenerateEpilogue(method);

            // Finalize function
            _currentFunction.MachineCode = _currentCode.ToArray();
            _currentFunction.CodeSize = (uint)_currentCode.Count;
            _currentFunction.Relocations = _relocations.ToList();
            _currentFunction.StackFrameSize = (uint)Math.Abs(_stackOffset);

            _nativeSection.Functions.Add(_currentFunction);
            _currentFunction = null;
        }

        /// <summary>
        /// Generate function prologue (save registers, set up stack frame)
        /// </summary>
        private void GeneratePrologue(MethodDeclaration method)
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    GenerateARMPrologue(method);
                    break;
                case Architecture.ARM64:
                    GenerateARM64Prologue(method);
                    break;
                case Architecture.x86_64:
                    Generatex86_64Prologue(method);
                    break;
                default:
                    throw new NotSupportedException($"Architecture {_targetArchitecture} not supported");
            }
        }

        /// <summary>
        /// Generate ARM function prologue
        /// </summary>
        private void GenerateARMPrologue(MethodDeclaration method)
        {
            // PUSH {R11, LR} - Save frame pointer and link register
            _currentCode.AddRange(ARMCodeGen.Push(ARMRegister.R11, ARMRegister.LR));
            
            // MOV R11, SP - Set up frame pointer
            _currentCode.AddRange(ARMCodeGen.Mov(ARMRegister.R11, ARMRegister.SP));
            
            // SUB SP, SP, #frameSize - Allocate stack space for locals
            // We'll calculate frame size later
        }

        /// <summary>
        /// Generate ARM64 function prologue
        /// </summary>
        private void GenerateARM64Prologue(MethodDeclaration method)
        {
            // ARM64 prologue: STP X29, X30, [SP, #-16]!
            // Save frame pointer (X29) and link register (X30)
            // For simplicity, we'll use a basic implementation
            _currentCode.Add(0xA9); // STP instruction prefix
            _currentCode.Add(0xBF);
            _currentCode.Add(0x7B);
            _currentCode.Add(0xFD);
        }

        /// <summary>
        /// Generate x86-64 function prologue
        /// </summary>
        private void Generatex86_64Prologue(MethodDeclaration method)
        {
            // PUSH RBP - Save base pointer
            _currentCode.AddRange(x86_64CodeGen.Push(x86_64Register.RBP));
            
            // MOV RBP, RSP - Set up frame pointer
            _currentCode.AddRange(x86_64CodeGen.Mov(x86_64Register.RBP, x86_64Register.RSP));
            
            // SUB RSP, frameSize - Allocate stack space for locals
            // We'll calculate frame size later
        }

        /// <summary>
        /// Generate function epilogue (restore registers, return)
        /// </summary>
        private void GenerateEpilogue(MethodDeclaration method)
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    GenerateARMEpilogue(method);
                    break;
                case Architecture.ARM64:
                    GenerateARM64Epilogue(method);
                    break;
                case Architecture.x86_64:
                    Generatex86_64Epilogue(method);
                    break;
            }
        }

        /// <summary>
        /// Generate ARM function epilogue
        /// </summary>
        private void GenerateARMEpilogue(MethodDeclaration method)
        {
            // MOV SP, R11 - Restore stack pointer
            _currentCode.AddRange(ARMCodeGen.Mov(ARMRegister.SP, ARMRegister.R11));
            
            // POP {R11, PC} - Restore frame pointer and return
            _currentCode.AddRange(ARMCodeGen.Pop(ARMRegister.R11, ARMRegister.PC));
        }

        /// <summary>
        /// Generate ARM64 function epilogue
        /// </summary>
        private void GenerateARM64Epilogue(MethodDeclaration method)
        {
            // LDP X29, X30, [SP], #16 - Restore frame pointer and link register
            _currentCode.Add(0xA8);
            _currentCode.Add(0xC1);
            _currentCode.Add(0x7B);
            _currentCode.Add(0xFD);
            
            // RET - Return
            _currentCode.Add(0xD6);
            _currentCode.Add(0x5F);
            _currentCode.Add(0x03);
            _currentCode.Add(0xC0);
        }

        /// <summary>
        /// Generate x86-64 function epilogue
        /// </summary>
        private void Generatex86_64Epilogue(MethodDeclaration method)
        {
            // MOV RSP, RBP - Restore stack pointer
            _currentCode.AddRange(x86_64CodeGen.Mov(x86_64Register.RSP, x86_64Register.RBP));
            
            // POP RBP - Restore base pointer
            _currentCode.AddRange(x86_64CodeGen.Pop(x86_64Register.RBP));
            
            // RET - Return
            _currentCode.AddRange(x86_64CodeGen.Ret());
        }

        /// <summary>
        /// Generate code for a statement
        /// </summary>
        private void GenerateStatement(Statement statement)
        {
            switch (statement)
            {
                case BlockStatement block:
                    GenerateBlockStatement(block);
                    break;

                case ExpressionStatement exprStmt:
                    GenerateExpression(exprStmt.Expression);
                    break;

                case ReturnStatement returnStmt:
                    if (returnStmt.Value != null)
                    {
                        GenerateExpression(returnStmt.Value);
                        // Result is in accumulator register (R0/RAX)
                    }
                    GenerateEpilogue(new MethodDeclaration()); // Return
                    break;

                case IfStatement ifStmt:
                    GenerateIfStatement(ifStmt);
                    break;

                case WhileStatement whileStmt:
                    GenerateWhileStatement(whileStmt);
                    break;

                case ForStatement forStmt:
                    GenerateForStatement(forStmt);
                    break;

                case VariableDeclarationStatement varDecl:
                    GenerateVariableDeclaration(varDecl);
                    break;

                default:
                    // Other statements not yet implemented
                    break;
            }
        }

        /// <summary>
        /// Generate code for an if statement
        /// </summary>
        private void GenerateIfStatement(IfStatement ifStmt)
        {
            // Generate condition
            GenerateExpression(ifStmt.Condition);
            
            // Compare result with 0 and branch
            int elseOffset = _currentCode.Count;
            
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // CMP R0, #0
                    _currentCode.Add(0xE3);
                    _currentCode.Add(0x50);
                    _currentCode.Add(0x00);
                    _currentCode.Add(0x00);
                    // BEQ else_label (branch if equal to zero)
                    _currentCode.AddRange(new byte[] { 0x0A, 0x00, 0x00, 0x00 }); // Placeholder
                    break;
                    
                case Architecture.x86_64:
                    // CMP RAX, 0
                    _currentCode.Add(0x48);
                    _currentCode.Add(0x83);
                    _currentCode.Add(0xF8);
                    _currentCode.Add(0x00);
                    // JE else_label
                    _currentCode.Add(0x0F);
                    _currentCode.Add(0x84);
                    _currentCode.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // Placeholder
                    break;
            }
            
            int branchInstructionOffset = _currentCode.Count;
            
            // Generate then branch
            GenerateStatement(ifStmt.ThenBranch);
            
            // Jump over else branch
            int endJumpOffset = _currentCode.Count;
            if (ifStmt.ElseBranch != null)
            {
                // Add unconditional jump placeholder
                _currentCode.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            }
            
            // Patch else branch offset
            int elseLocation = _currentCode.Count;
            
            if (ifStmt.ElseBranch != null)
            {
                GenerateStatement(ifStmt.ElseBranch);
            }
            
            // Patch end jump offset
            int endLocation = _currentCode.Count;
        }

        /// <summary>
        /// Generate code for a while statement
        /// </summary>
        private void GenerateWhileStatement(WhileStatement whileStmt)
        {
            int startOffset = _currentCode.Count;
            
            // Generate condition
            GenerateExpression(whileStmt.Condition);
            
            // Compare and branch to end if false
            int branchOffset = _currentCode.Count;
            _currentCode.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // Placeholder
            
            // Generate body
            GenerateStatement(whileStmt.Body);
            
            // Jump back to start
            int endOffset = _currentCode.Count;
        }

        /// <summary>
        /// Generate code for a for statement
        /// </summary>
        private void GenerateForStatement(ForStatement forStmt)
        {
            // Generate initializer
            if (forStmt.Initializer != null)
            {
                GenerateStatement(forStmt.Initializer);
            }
            
            int startOffset = _currentCode.Count;
            
            // Generate condition
            if (forStmt.Condition != null)
            {
                GenerateExpression(forStmt.Condition);
                // Branch to end if false
            }
            
            // Generate body
            GenerateStatement(forStmt.Body);
            
            // Generate increment
            if (forStmt.Increment != null)
            {
                GenerateExpression(forStmt.Increment);
            }
            
            // Jump back to start
        }

        /// <summary>
        /// Generate code for a variable declaration
        /// </summary>
        private void GenerateVariableDeclaration(VariableDeclarationStatement varDecl)
        {
            // Allocate stack space
            int size = GetTypeSize(varDecl.VariableType);
            _stackOffset -= size;
            _localVariables[varDecl.Name] = _stackOffset;
            
            // Generate initializer if present
            if (varDecl.Initializer != null)
            {
                GenerateExpression(varDecl.Initializer);
                
                // Store result to local variable
                StoreToLocal(varDecl.Name);
            }

            // Emit reference counting initialization if needed
            if (_memoryManagement != null && varDecl.Initializer is NewExpression)
            {
                EmitRefCountInit(varDecl.Name);
            }
        }

        /// <summary>
        /// Generate code for an expression
        /// </summary>
        private void GenerateExpression(Expression expression)
        {
            switch (expression)
            {
                case LiteralExpression literal:
                    GenerateLiteral(literal);
                    break;

                case IdentifierExpression identifier:
                    GenerateIdentifier(identifier);
                    break;

                case BinaryExpression binary:
                    GenerateBinaryExpression(binary);
                    break;

                case UnaryExpression unary:
                    GenerateUnaryExpression(unary);
                    break;

                case MethodCallExpression methodCall:
                    GenerateMethodCall(methodCall);
                    break;

                case MemberAccessExpression memberAccess:
                    GenerateMemberAccess(memberAccess);
                    break;

                case AssignmentExpression assignment:
                    GenerateAssignment(assignment);
                    break;

                case NewExpression newExpr:
                    GenerateNewExpression(newExpr);
                    break;

                default:
                    // Other expressions not yet implemented
                    break;
            }
        }

        /// <summary>
        /// Generate code for a literal expression
        /// </summary>
        private void GenerateLiteral(LiteralExpression literal)
        {
            switch (literal.LiteralType)
            {
                case LiteralType.Integer:
                    int intValue = Convert.ToInt32(literal.Value);
                    LoadImmediate(intValue);
                    break;

                case LiteralType.Float:
                    // Float literals need special handling
                    break;

                case LiteralType.Boolean:
                    bool boolValue = Convert.ToBoolean(literal.Value);
                    LoadImmediate(boolValue ? 1 : 0);
                    break;

                case LiteralType.Null:
                    LoadImmediate(0);
                    break;
            }
        }

        /// <summary>
        /// Load an immediate value into the accumulator register
        /// </summary>
        private void LoadImmediate(int value)
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // MOV R0, #value (simplified - real ARM has encoding limits)
                    if (value >= 0 && value <= 255)
                    {
                        uint instruction = 0xE3A00000 | (uint)value;
                        _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    }
                    else
                    {
                        // LDR R0, [PC, #offset] for larger values
                        _currentCode.AddRange(new byte[] { 0x00, 0x00, 0x9F, 0xE5 });
                    }
                    break;

                case Architecture.x86_64:
                    // MOV RAX, immediate
                    _currentCode.Add(0x48); // REX.W prefix
                    _currentCode.Add(0xC7);
                    _currentCode.Add(0xC0); // ModR/M for RAX
                    _currentCode.AddRange(BitConverter.GetBytes(value));
                    break;
            }
        }

        /// <summary>
        /// Generate code for an identifier (variable load)
        /// </summary>
        private void GenerateIdentifier(IdentifierExpression identifier)
        {
            if (_localVariables.TryGetValue(identifier.Name, out int offset))
            {
                LoadFromLocal(identifier.Name);
            }
        }

        /// <summary>
        /// Load a local variable into the accumulator register
        /// </summary>
        private void LoadFromLocal(string varName)
        {
            int offset = _localVariables[varName];
            
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // LDR R0, [R11, #offset]
                    uint instruction = 0xE59B0000 | (uint)(offset & 0xFFF);
                    _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    break;

                case Architecture.x86_64:
                    // MOV RAX, [RBP + offset]
                    _currentCode.Add(0x48); // REX.W
                    _currentCode.Add(0x8B);
                    _currentCode.Add(0x85); // ModR/M for [RBP + disp32]
                    _currentCode.AddRange(BitConverter.GetBytes(offset));
                    break;
            }
        }

        /// <summary>
        /// Store the accumulator register to a local variable
        /// </summary>
        private void StoreToLocal(string varName)
        {
            int offset = _localVariables[varName];
            
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // STR R0, [R11, #offset]
                    uint instruction = 0xE58B0000 | (uint)(offset & 0xFFF);
                    _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    break;

                case Architecture.x86_64:
                    // MOV [RBP + offset], RAX
                    _currentCode.Add(0x48); // REX.W
                    _currentCode.Add(0x89);
                    _currentCode.Add(0x85); // ModR/M for [RBP + disp32]
                    _currentCode.AddRange(BitConverter.GetBytes(offset));
                    break;
            }
        }

        /// <summary>
        /// Generate code for a binary expression
        /// </summary>
        private void GenerateBinaryExpression(BinaryExpression binary)
        {
            // Generate left operand (result in accumulator)
            GenerateExpression(binary.Left);
            
            // Push left operand to stack
            PushAccumulator();
            
            // Generate right operand (result in accumulator)
            GenerateExpression(binary.Right);
            
            // Pop left operand to second register
            PopToSecondRegister();
            
            // Perform operation
            switch (binary.Operator)
            {
                case BinaryOperator.Add:
                    GenerateAdd();
                    break;
                case BinaryOperator.Subtract:
                    GenerateSubtract();
                    break;
                case BinaryOperator.Multiply:
                    GenerateMultiply();
                    break;
                case BinaryOperator.Divide:
                    GenerateDivide();
                    break;
                default:
                    // Other operators not yet implemented
                    break;
            }
        }

        /// <summary>
        /// Push accumulator register to stack
        /// </summary>
        private void PushAccumulator()
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // PUSH {R0}
                    _currentCode.AddRange(ARMCodeGen.Push(ARMRegister.R0));
                    break;

                case Architecture.x86_64:
                    // PUSH RAX
                    _currentCode.AddRange(x86_64CodeGen.Push(x86_64Register.RAX));
                    break;
            }
            _stackOffset -= 8;
        }

        /// <summary>
        /// Pop from stack to second register
        /// </summary>
        private void PopToSecondRegister()
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // POP {R1}
                    _currentCode.AddRange(ARMCodeGen.Pop(ARMRegister.R1));
                    break;

                case Architecture.x86_64:
                    // POP RCX
                    _currentCode.AddRange(x86_64CodeGen.Pop(x86_64Register.RCX));
                    break;
            }
            _stackOffset += 8;
        }

        /// <summary>
        /// Generate ADD instruction
        /// </summary>
        private void GenerateAdd()
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // ADD R0, R1, R0
                    _currentCode.AddRange(ARMCodeGen.Add(ARMRegister.R0, ARMRegister.R1, ARMRegister.R0));
                    break;

                case Architecture.x86_64:
                    // ADD RAX, RCX
                    _currentCode.AddRange(x86_64CodeGen.Add(x86_64Register.RAX, x86_64Register.RCX));
                    break;
            }
        }

        /// <summary>
        /// Generate SUB instruction
        /// </summary>
        private void GenerateSubtract()
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // SUB R0, R1, R0
                    uint instruction = 0xE0410000;
                    _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    break;

                case Architecture.x86_64:
                    // SUB RCX, RAX (then MOV RAX, RCX)
                    _currentCode.Add(0x48);
                    _currentCode.Add(0x29);
                    _currentCode.Add(0xC1);
                    _currentCode.AddRange(x86_64CodeGen.Mov(x86_64Register.RAX, x86_64Register.RCX));
                    break;
            }
        }

        /// <summary>
        /// Generate MUL instruction
        /// </summary>
        private void GenerateMultiply()
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // MUL R0, R1, R0
                    uint instruction = 0xE0000190;
                    _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    break;

                case Architecture.x86_64:
                    // IMUL RAX, RCX
                    _currentCode.Add(0x48);
                    _currentCode.Add(0x0F);
                    _currentCode.Add(0xAF);
                    _currentCode.Add(0xC1);
                    break;
            }
        }

        /// <summary>
        /// Generate DIV instruction
        /// </summary>
        private void GenerateDivide()
        {
            // Division is complex and architecture-specific
            // This is a simplified placeholder
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // SDIV R0, R1, R0 (ARMv7 and later)
                    uint instruction = 0xE710F110;
                    _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    break;

                case Architecture.x86_64:
                    // IDIV RCX (RAX = RAX / RCX)
                    _currentCode.Add(0x48);
                    _currentCode.Add(0xF7);
                    _currentCode.Add(0xF9);
                    break;
            }
        }

        /// <summary>
        /// Generate code for a unary expression
        /// </summary>
        private void GenerateUnaryExpression(UnaryExpression unary)
        {
            GenerateExpression(unary.Operand);
            
            switch (unary.Operator)
            {
                case UnaryOperator.Negate:
                    GenerateNegate();
                    break;
                case UnaryOperator.LogicalNot:
                    GenerateLogicalNot();
                    break;
                case UnaryOperator.BitwiseNot:
                    GenerateBitwiseNot();
                    break;
            }
        }

        /// <summary>
        /// Generate negation (two's complement)
        /// </summary>
        private void GenerateNegate()
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // RSB R0, R0, #0 (Reverse subtract: R0 = 0 - R0)
                    uint instruction = 0xE2600000;
                    _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    break;

                case Architecture.x86_64:
                    // NEG RAX
                    _currentCode.Add(0x48);
                    _currentCode.Add(0xF7);
                    _currentCode.Add(0xD8);
                    break;
            }
        }

        /// <summary>
        /// Generate logical NOT
        /// </summary>
        private void GenerateLogicalNot()
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // CMP R0, #0
                    // MOVEQ R0, #1
                    // MOVNE R0, #0
                    _currentCode.AddRange(new byte[] { 0x00, 0x00, 0x50, 0xE3 });
                    _currentCode.AddRange(new byte[] { 0x01, 0x00, 0xA0, 0x03 });
                    _currentCode.AddRange(new byte[] { 0x00, 0x00, 0xA0, 0x13 });
                    break;

                case Architecture.x86_64:
                    // TEST RAX, RAX
                    // SETZ AL
                    // MOVZX RAX, AL
                    _currentCode.Add(0x48);
                    _currentCode.Add(0x85);
                    _currentCode.Add(0xC0);
                    _currentCode.Add(0x0F);
                    _currentCode.Add(0x94);
                    _currentCode.Add(0xC0);
                    break;
            }
        }

        /// <summary>
        /// Generate bitwise NOT
        /// </summary>
        private void GenerateBitwiseNot()
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // MVN R0, R0 (Move NOT)
                    uint instruction = 0xE1E00000;
                    _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    break;

                case Architecture.x86_64:
                    // NOT RAX
                    _currentCode.Add(0x48);
                    _currentCode.Add(0xF7);
                    _currentCode.Add(0xD0);
                    break;
            }
        }

        /// <summary>
        /// Generate code for a method call
        /// </summary>
        private void GenerateMethodCall(MethodCallExpression methodCall)
        {
            // Push arguments in reverse order (right to left)
            for (int i = methodCall.Arguments.Count - 1; i >= 0; i--)
            {
                GenerateExpression(methodCall.Arguments[i]);
                PushAccumulator();
            }
            
            // Generate call instruction
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // BL function_name
                    _currentCode.AddRange(ARMCodeGen.BranchLink(0)); // Offset will be patched
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        methodCall.MethodName
                    ));
                    break;

                case Architecture.x86_64:
                    // CALL function_name
                    _currentCode.AddRange(x86_64CodeGen.Call(0)); // Offset will be patched
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        methodCall.MethodName
                    ));
                    break;
            }
            
            // Clean up arguments from stack
            int argSize = methodCall.Arguments.Count * 8;
            _stackOffset += argSize;
        }

        /// <summary>
        /// Generate code for member access (pointer dereference)
        /// </summary>
        private void GenerateMemberAccess(MemberAccessExpression memberAccess)
        {
            // Generate target expression (should result in a pointer)
            GenerateExpression(memberAccess.Target);
            
            // Load from memory at pointer address
            // This is simplified - real implementation would need offset calculation
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // LDR R0, [R0, #0]
                    uint instruction = 0xE5900000;
                    _currentCode.AddRange(BitConverter.GetBytes(instruction));
                    break;

                case Architecture.x86_64:
                    // MOV RAX, [RAX]
                    _currentCode.Add(0x48);
                    _currentCode.Add(0x8B);
                    _currentCode.Add(0x00);
                    break;
            }
        }

        /// <summary>
        /// Generate code for assignment
        /// </summary>
        private void GenerateAssignment(AssignmentExpression assignment)
        {
            // Generate value to assign
            GenerateExpression(assignment.Value);
            
            // Handle different assignment targets
            if (assignment.Target is IdentifierExpression identifier)
            {
                // Simple variable assignment
                StoreToLocal(identifier.Name);
            }
            else if (assignment.Target is MemberAccessExpression memberAccess)
            {
                // Pointer dereference assignment
                // This is simplified - real implementation would be more complex
                PushAccumulator(); // Save value
                GenerateExpression(memberAccess.Target); // Get pointer
                PopToSecondRegister(); // Get value back
                
                // Store to memory
                switch (_targetArchitecture)
                {
                    case Architecture.ARM:
                        // STR R1, [R0, #0]
                        uint instruction = 0xE5801000;
                        _currentCode.AddRange(BitConverter.GetBytes(instruction));
                        break;

                    case Architecture.x86_64:
                        // MOV [RAX], RCX
                        _currentCode.Add(0x48);
                        _currentCode.Add(0x89);
                        _currentCode.Add(0x08);
                        break;
                }
            }
        }

        /// <summary>
        /// Generate code for new expression (memory allocation)
        /// </summary>
        private void GenerateNewExpression(NewExpression newExpr)
        {
            // Calculate size to allocate
            int size = GetTypeSize(newExpr.TypeToCreate);
            
            // Call memory allocation function (malloc or similar)
            LoadImmediate(size);
            
            // This would call a runtime function to allocate memory
            // For now, just a placeholder
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    _currentCode.AddRange(ARMCodeGen.BranchLink(0));
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        "__allocate_memory"
                    ));
                    break;

                case Architecture.x86_64:
                    _currentCode.AddRange(x86_64CodeGen.Call(0));
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        "__allocate_memory"
                    ));
                    break;
            }
            
            // Result (pointer) is now in accumulator register
        }

        /// <summary>
        /// Get the size of a type in bytes
        /// </summary>
        private int GetTypeSize(TypeReference type)
        {
            if (type.IsPointer)
            {
                return 8; // Pointer size (64-bit)
            }
            
            return type.Name switch
            {
                "int" => 4,
                "long" => 8,
                "short" => 2,
                "byte" => 1,
                "float" => 4,
                "double" => 8,
                "bool" => 1,
                "char" => 1,
                _ => 8 // Default to pointer size for objects
            };
        }

        /// <summary>
        /// Get the calling convention for an architecture
        /// </summary>
        private CallingConvention GetCallingConvention(Architecture arch)
        {
            return arch switch
            {
                Architecture.ARM => CallingConvention.ARM_AAPCS,
                Architecture.ARM64 => CallingConvention.ARM64_AAPCS64,
                Architecture.x86 => CallingConvention.x86_cdecl,
                Architecture.x86_64 => CallingConvention.x86_64_SysV,
                _ => throw new NotSupportedException($"Architecture {arch} not supported")
            };
        }

        /// <summary>
        /// Get the function signature string
        /// </summary>
        private string GetFunctionSignature(MethodDeclaration method)
        {
            var paramTypes = string.Join(",", method.Parameters.Select(p => p.ParameterType.Name));
            return $"{method.ReturnType.Name} {method.Name}({paramTypes})";
        }

        /// <summary>
        /// Generate code for a block statement with scope cleanup
        /// </summary>
        private void GenerateBlockStatement(BlockStatement block)
        {
            foreach (var stmt in block.Statements)
            {
                GenerateStatement(stmt);
            }

            // Emit scope cleanup code if memory management is enabled
            if (_memoryManagement != null && block.Location != null)
            {
                EmitScopeCleanup(block.Location);
            }
        }

        /// <summary>
        /// Emit reference count initialization for a variable
        /// </summary>
        private void EmitRefCountInit(string variableName)
        {
            if (!_localVariables.ContainsKey(variableName))
                return;

            // Load the variable address
            LoadFromLocal(variableName);

            // Call __ref_count_init runtime function
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    _currentCode.AddRange(ARMCodeGen.BranchLink(0));
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        "__ref_count_init"
                    ));
                    break;

                case Architecture.x86_64:
                    _currentCode.AddRange(x86_64CodeGen.Call(0));
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        "__ref_count_init"
                    ));
                    break;
            }
        }

        /// <summary>
        /// Emit scope cleanup code (reference counting and deallocation)
        /// </summary>
        private void EmitScopeCleanup(SourceLocation location)
        {
            if (_memoryManagement == null)
                return;

            // Find all deallocations at this location
            var deallocations = _memoryManagement.Deallocations
                .Where(d => LocationMatches(d.Location, location))
                .ToList();

            foreach (var dealloc in deallocations)
            {
                if (!_localVariables.ContainsKey(dealloc.VariableName))
                    continue;

                // Load variable for ref count decrement
                LoadFromLocal(dealloc.VariableName);

                // Call __ref_count_dec
                EmitRuntimeCall("__ref_count_dec");

                // Load variable for zero check
                LoadFromLocal(dealloc.VariableName);

                // Call __ref_count_is_zero
                EmitRuntimeCall("__ref_count_is_zero");

                // Compare result with zero and branch
                int branchOffset = _currentCode.Count;
                switch (_targetArchitecture)
                {
                    case Architecture.ARM:
                        // CMP R0, #0
                        _currentCode.AddRange(new byte[] { 0x00, 0x00, 0x50, 0xE3 });
                        // BEQ skip_delete (branch if equal to zero)
                        _currentCode.AddRange(new byte[] { 0x03, 0x00, 0x00, 0x0A });
                        break;

                    case Architecture.x86_64:
                        // CMP RAX, 0
                        _currentCode.Add(0x48);
                        _currentCode.Add(0x83);
                        _currentCode.Add(0xF8);
                        _currentCode.Add(0x00);
                        // JE skip_delete
                        _currentCode.Add(0x0F);
                        _currentCode.Add(0x84);
                        _currentCode.AddRange(new byte[] { 0x0A, 0x00, 0x00, 0x00 });
                        break;
                }

                // Load variable for deletion
                LoadFromLocal(dealloc.VariableName);

                // Call appropriate delete function
                var deleteFuncName = dealloc.Kind == DeallocationKind.DeleteArray
                    ? "__safe_delete_array"
                    : "__safe_delete";
                EmitRuntimeCall(deleteFuncName);

                // skip_delete label (code continues here)
            }
        }

        /// <summary>
        /// Emit a call to a runtime function
        /// </summary>
        private void EmitRuntimeCall(string functionName)
        {
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    _currentCode.AddRange(ARMCodeGen.BranchLink(0));
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        functionName
                    ));
                    break;

                case Architecture.ARM64:
                    // BL instruction for ARM64
                    _currentCode.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x94 });
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        functionName
                    ));
                    break;

                case Architecture.x86_64:
                    _currentCode.AddRange(x86_64CodeGen.Call(0));
                    _relocations.Add(new Relocation(
                        (uint)(_currentCode.Count - 4),
                        RelocationType.Relative,
                        functionName
                    ));
                    break;
            }
        }

        /// <summary>
        /// Emit null safety check before pointer dereference
        /// </summary>
        private void EmitNullSafetyCheck(string variableName)
        {
            if (!_localVariables.ContainsKey(variableName))
                return;

            // Load the variable
            LoadFromLocal(variableName);

            // Compare with null (0)
            switch (_targetArchitecture)
            {
                case Architecture.ARM:
                    // CMP R0, #0
                    _currentCode.AddRange(new byte[] { 0x00, 0x00, 0x50, 0xE3 });
                    // BNE not_null (branch if not equal)
                    _currentCode.AddRange(new byte[] { 0x02, 0x00, 0x00, 0x1A });
                    break;

                case Architecture.x86_64:
                    // CMP RAX, 0
                    _currentCode.Add(0x48);
                    _currentCode.Add(0x83);
                    _currentCode.Add(0xF8);
                    _currentCode.Add(0x00);
                    // JNE not_null
                    _currentCode.Add(0x0F);
                    _currentCode.Add(0x85);
                    _currentCode.AddRange(new byte[] { 0x05, 0x00, 0x00, 0x00 });
                    break;
            }

            // Call null pointer exception handler
            EmitRuntimeCall("__throw_null_pointer_exception");

            // not_null label (code continues here)
        }

        /// <summary>
        /// Check if two source locations match
        /// </summary>
        private bool LocationMatches(SourceLocation loc1, SourceLocation loc2)
        {
            if (loc1 == null || loc2 == null)
                return false;

            return loc1.File == loc2.File &&
                   loc1.Line == loc2.Line &&
                   loc1.Column == loc2.Column;
        }
    }
}
