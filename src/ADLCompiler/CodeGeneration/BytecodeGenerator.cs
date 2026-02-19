using System;
using System.Collections.Generic;
using System.Linq;
using SharedUtilities.Models;
using ADLCompiler.SemanticAnalysis;

namespace ADLCompiler.CodeGeneration
{
    /// <summary>
    /// Generates Dalvik-compatible bytecode for Java-style code
    /// </summary>
    public class BytecodeGenerator
    {
        private readonly BytecodeSection _bytecodeSection;
        private readonly ResourceSection _resources;
        private readonly MemoryManagementInstructions? _memoryManagement;
        private BytecodeMethod? _currentMethod;
        private ushort _currentStackSize;
        private ushort _maxStackSize;
        private ushort _nextLocalIndex;
        private readonly Dictionary<string, ushort> _localVariables;
        private readonly List<(int instructionIndex, string label)> _unresolvedLabels;
        private readonly Dictionary<string, int> _labels;
        private int _nextLabelId;

        public BytecodeGenerator(BytecodeSection bytecodeSection, ResourceSection resources, MemoryManagementInstructions? memoryManagement = null)
        {
            _bytecodeSection = bytecodeSection;
            _resources = resources;
            _memoryManagement = memoryManagement;
            _localVariables = new Dictionary<string, ushort>();
            _unresolvedLabels = new List<(int, string)>();
            _labels = new Dictionary<string, int>();
            _nextLabelId = 0;
        }

        /// <summary>
        /// Generate bytecode for a compilation unit
        /// </summary>
        public void GenerateCompilationUnit(CompilationUnit unit)
        {
            foreach (var classDecl in unit.Classes)
            {
                GenerateClass(classDecl);
            }
        }

        /// <summary>
        /// Generate bytecode for a class declaration
        /// </summary>
        public void GenerateClass(ClassDeclaration classDecl)
        {
            var bytecodeClass = new BytecodeClass
            {
                ClassName = GetFullClassName(classDecl),
                SuperClassName = classDecl.SuperClass ?? "java.lang.Object",
                AccessFlags = ConvertAccessModifier(classDecl.AccessModifier)
            };

            if (classDecl.IsAbstract)
                bytecodeClass.AccessFlags |= AccessFlags.Abstract;
            if (classDecl.IsStatic)
                bytecodeClass.AccessFlags |= AccessFlags.Static;

            // Add interfaces
            bytecodeClass.Interfaces.AddRange(classDecl.Interfaces);

            // Generate fields
            foreach (var field in classDecl.Fields)
            {
                GenerateField(bytecodeClass, field);
            }

            // Generate methods
            foreach (var method in classDecl.Methods)
            {
                GenerateMethod(bytecodeClass, method);
            }

            _bytecodeSection.Classes.Add(bytecodeClass);
        }

        /// <summary>
        /// Generate bytecode for a field declaration
        /// </summary>
        private void GenerateField(BytecodeClass bytecodeClass, FieldDeclaration field)
        {
            var bytecodeField = new BytecodeField
            {
                Name = field.Name,
                TypeDescriptor = GetTypeDescriptor(field.FieldType),
                AccessFlags = ConvertAccessModifier(field.AccessModifier)
            };

            if (field.IsStatic)
                bytecodeField.AccessFlags |= AccessFlags.Static;
            if (field.IsFinal)
                bytecodeField.AccessFlags |= AccessFlags.Final;

            // Handle constant initializers
            if (field.Initializer is LiteralExpression literal && literal.Value != null)
            {
                bytecodeField.InitialValue = literal.Value;
            }

            bytecodeClass.Fields.Add(bytecodeField);
        }

        /// <summary>
        /// Generate bytecode for a method declaration
        /// </summary>
        private void GenerateMethod(BytecodeClass bytecodeClass, MethodDeclaration method)
        {
            _currentMethod = new BytecodeMethod
            {
                Name = method.Name,
                Signature = GetMethodSignature(method),
                AccessFlags = ConvertAccessModifier(method.AccessModifier)
            };

            if (method.IsStatic)
                _currentMethod.AccessFlags |= AccessFlags.Static;
            if (method.IsNative)
                _currentMethod.AccessFlags |= AccessFlags.Native;
            if (method.IsAbstract)
                _currentMethod.AccessFlags |= AccessFlags.Abstract;

            // Reset state for new method
            _localVariables.Clear();
            _unresolvedLabels.Clear();
            _labels.Clear();
            _currentStackSize = 0;
            _maxStackSize = 0;
            _nextLocalIndex = method.IsStatic ? (ushort)0 : (ushort)1; // Reserve 0 for 'this' if non-static

            // Allocate local variables for parameters
            foreach (var param in method.Parameters)
            {
                _localVariables[param.Name] = _nextLocalIndex++;
            }

            // Generate method body
            if (method.Body != null && !method.IsAbstract && !method.IsNative)
            {
                GenerateStatement(method.Body);

                // Add implicit return for void methods
                if (method.ReturnType.Name == "void" && 
                    (_currentMethod.Instructions.Count == 0 || 
                     _currentMethod.Instructions.Last().Opcode != Opcode.RETURN_VOID))
                {
                    EmitInstruction(Opcode.RETURN_VOID, Array.Empty<byte>(), method.Location.Line);
                }
            }

            _currentMethod.MaxStack = _maxStackSize;
            _currentMethod.MaxLocals = _nextLocalIndex;

            bytecodeClass.Methods.Add(_currentMethod);
            _currentMethod = null;
        }

        /// <summary>
        /// Generate bytecode for a statement
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
                    // Pop result if expression produces a value
                    if (exprStmt.Expression.EvaluatedType?.Name != "void")
                    {
                        PopStack();
                    }
                    break;

                case ReturnStatement returnStmt:
                    if (returnStmt.Value != null)
                    {
                        GenerateExpression(returnStmt.Value);
                        var returnType = returnStmt.Value.EvaluatedType?.Name ?? "void";
                        EmitReturn(returnType, returnStmt.Location.Line);
                    }
                    else
                    {
                        EmitInstruction(Opcode.RETURN_VOID, Array.Empty<byte>(), returnStmt.Location.Line);
                    }
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

                case DoWhileStatement doWhileStmt:
                    GenerateDoWhileStatement(doWhileStmt);
                    break;

                case SwitchStatement switchStmt:
                    GenerateSwitchStatement(switchStmt);
                    break;

                case BreakStatement:
                    // Break will be handled by loop/switch context
                    EmitInstruction(Opcode.NOP, Array.Empty<byte>(), statement.Location.Line);
                    break;

                case ContinueStatement:
                    // Continue will be handled by loop context
                    EmitInstruction(Opcode.NOP, Array.Empty<byte>(), statement.Location.Line);
                    break;

                case VariableDeclarationStatement varDecl:
                    GenerateVariableDeclaration(varDecl);
                    break;

                case ThrowStatement throwStmt:
                    GenerateExpression(throwStmt.Exception);
                    // Throw instruction would go here (not in standard Dalvik, would need extension)
                    EmitInstruction(Opcode.NOP, Array.Empty<byte>(), throwStmt.Location.Line);
                    break;

                default:
                    throw new NotImplementedException($"Statement type {statement.GetType().Name} not implemented");
            }
        }

        /// <summary>
        /// Generate bytecode for a block statement and insert cleanup code at the end
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
        /// Generate bytecode for an if statement
        /// </summary>
        private void GenerateIfStatement(IfStatement ifStmt)
        {
            var elseLabel = GenerateLabel();
            var endLabel = GenerateLabel();

            // Generate condition
            GenerateExpression(ifStmt.Condition);

            // Branch to else if condition is false
            EmitConditionalBranch(Opcode.IF_EQ, elseLabel, ifStmt.Location.Line); // IF_EQ branches if zero (false)
            PopStack();

            // Generate then branch
            GenerateStatement(ifStmt.ThenBranch);

            if (ifStmt.ElseBranch != null)
            {
                // Jump to end after then branch
                EmitBranch(endLabel, ifStmt.Location.Line);

                // Else branch
                MarkLabel(elseLabel);
                GenerateStatement(ifStmt.ElseBranch);

                MarkLabel(endLabel);
            }
            else
            {
                MarkLabel(elseLabel);
            }
        }

        /// <summary>
        /// Generate bytecode for a while statement
        /// </summary>
        private void GenerateWhileStatement(WhileStatement whileStmt)
        {
            var startLabel = GenerateLabel();
            var endLabel = GenerateLabel();

            MarkLabel(startLabel);

            // Generate condition
            GenerateExpression(whileStmt.Condition);

            // Branch to end if condition is false
            EmitConditionalBranch(Opcode.IF_EQ, endLabel, whileStmt.Location.Line);
            PopStack();

            // Generate body
            GenerateStatement(whileStmt.Body);

            // Jump back to start
            EmitBranch(startLabel, whileStmt.Location.Line);

            MarkLabel(endLabel);
        }

        /// <summary>
        /// Generate bytecode for a for statement
        /// </summary>
        private void GenerateForStatement(ForStatement forStmt)
        {
            var startLabel = GenerateLabel();
            var continueLabel = GenerateLabel();
            var endLabel = GenerateLabel();

            // Generate initializer
            if (forStmt.Initializer != null)
            {
                GenerateStatement(forStmt.Initializer);
            }

            MarkLabel(startLabel);

            // Generate condition
            if (forStmt.Condition != null)
            {
                GenerateExpression(forStmt.Condition);
                EmitConditionalBranch(Opcode.IF_EQ, endLabel, forStmt.Location.Line);
                PopStack();
            }

            // Generate body
            GenerateStatement(forStmt.Body);

            // Continue point
            MarkLabel(continueLabel);

            // Generate increment
            if (forStmt.Increment != null)
            {
                GenerateExpression(forStmt.Increment);
                PopStack(); // Discard increment result
            }

            // Jump back to start
            EmitBranch(startLabel, forStmt.Location.Line);

            MarkLabel(endLabel);
        }

        /// <summary>
        /// Generate bytecode for a do-while statement
        /// </summary>
        private void GenerateDoWhileStatement(DoWhileStatement doWhileStmt)
        {
            var startLabel = GenerateLabel();

            MarkLabel(startLabel);

            // Generate body
            GenerateStatement(doWhileStmt.Body);

            // Generate condition
            GenerateExpression(doWhileStmt.Condition);

            // Branch back to start if condition is true
            EmitConditionalBranch(Opcode.IF_NE, startLabel, doWhileStmt.Location.Line); // IF_NE branches if non-zero (true)
            PopStack();
        }

        /// <summary>
        /// Generate bytecode for a switch statement
        /// </summary>
        private void GenerateSwitchStatement(SwitchStatement switchStmt)
        {
            var endLabel = GenerateLabel();
            var caseLabels = new List<string>();
            var defaultLabel = GenerateLabel();

            // Generate switch expression
            GenerateExpression(switchStmt.Expression);

            // For simplicity, generate as if-else chain
            // A real implementation would use packed-switch or sparse-switch instructions
            foreach (var caseStmt in switchStmt.Cases)
            {
                var caseLabel = GenerateLabel();
                caseLabels.Add(caseLabel);

                if (caseStmt.Value != null)
                {
                    // Duplicate switch value for comparison
                    EmitInstruction(Opcode.MOVE, new byte[] { 0, 0 }, caseStmt.Location.Line);
                    PushStack();

                    // Generate case value
                    GenerateExpression(caseStmt.Value);

                    // Compare
                    EmitConditionalBranch(Opcode.IF_EQ, caseLabel, caseStmt.Location.Line);
                    PopStack();
                    PopStack();
                }
            }

            // No match, jump to default or end
            EmitBranch(defaultLabel, switchStmt.Location.Line);
            PopStack(); // Pop switch value

            // Generate case bodies
            for (int i = 0; i < switchStmt.Cases.Count; i++)
            {
                var caseStmt = switchStmt.Cases[i];
                
                if (caseStmt.Value == null)
                {
                    MarkLabel(defaultLabel);
                }
                else
                {
                    MarkLabel(caseLabels[i]);
                }

                foreach (var stmt in caseStmt.Statements)
                {
                    GenerateStatement(stmt);
                }
            }

            MarkLabel(endLabel);
        }

        /// <summary>
        /// Generate bytecode for a variable declaration
        /// </summary>
        private void GenerateVariableDeclaration(VariableDeclarationStatement varDecl)
        {
            // Allocate local variable
            var localIndex = _nextLocalIndex++;
            _localVariables[varDecl.Name] = localIndex;

            // Generate initializer if present
            if (varDecl.Initializer != null)
            {
                GenerateExpression(varDecl.Initializer);
                
                // Store to local variable
                EmitStoreLocal(localIndex, varDecl.VariableType.Name, varDecl.Location.Line);
                PopStack();
            }

            // Emit reference counting initialization if needed
            if (_memoryManagement != null && varDecl.Initializer is NewExpression)
            {
                EmitRefCountInit(varDecl.Name, varDecl.Location.Line);
            }
        }

        /// <summary>
        /// Generate bytecode for an expression
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

                case ArrayAccessExpression arrayAccess:
                    GenerateArrayAccess(arrayAccess);
                    break;

                case NewExpression newExpr:
                    GenerateNewExpression(newExpr);
                    break;

                case CastExpression cast:
                    GenerateCastExpression(cast);
                    break;

                case TernaryExpression ternary:
                    GenerateTernaryExpression(ternary);
                    break;

                default:
                    throw new NotImplementedException($"Expression type {expression.GetType().Name} not implemented");
            }
        }

        /// <summary>
        /// Generate bytecode for a literal expression
        /// </summary>
        private void GenerateLiteral(LiteralExpression literal)
        {
            switch (literal.LiteralType)
            {
                case LiteralType.Integer:
                    var intValue = Convert.ToInt32(literal.Value);
                    if (intValue >= -8 && intValue <= 7)
                    {
                        // Use const/4 for small integers
                        EmitInstruction(Opcode.CONST_4, new byte[] { (byte)intValue }, literal.Location.Line);
                    }
                    else if (intValue >= short.MinValue && intValue <= short.MaxValue)
                    {
                        // Use const/16 for 16-bit integers
                        var bytes = BitConverter.GetBytes((short)intValue);
                        EmitInstruction(Opcode.CONST_16, bytes, literal.Location.Line);
                    }
                    else
                    {
                        // Use const for 32-bit integers
                        var bytes = BitConverter.GetBytes(intValue);
                        EmitInstruction(Opcode.CONST, bytes, literal.Location.Line);
                    }
                    PushStack();
                    break;

                case LiteralType.Float:
                    var floatValue = Convert.ToSingle(literal.Value);
                    var floatBytes = BitConverter.GetBytes(floatValue);
                    EmitInstruction(Opcode.CONST, floatBytes, literal.Location.Line);
                    PushStack();
                    break;

                case LiteralType.String:
                    var stringValue = literal.Value?.ToString() ?? "";
                    var stringId = _resources.Strings.AddString(stringValue);
                    var stringIdBytes = BitConverter.GetBytes(stringId);
                    EmitInstruction(Opcode.CONST_STRING, stringIdBytes, literal.Location.Line);
                    PushStack();
                    break;

                case LiteralType.Boolean:
                    var boolValue = Convert.ToBoolean(literal.Value);
                    EmitInstruction(Opcode.CONST_4, new byte[] { (byte)(boolValue ? 1 : 0) }, literal.Location.Line);
                    PushStack();
                    break;

                case LiteralType.Null:
                    EmitInstruction(Opcode.CONST_4, new byte[] { 0 }, literal.Location.Line);
                    PushStack();
                    break;

                case LiteralType.Char:
                    var charValue = Convert.ToChar(literal.Value);
                    EmitInstruction(Opcode.CONST_16, BitConverter.GetBytes((short)charValue), literal.Location.Line);
                    PushStack();
                    break;

                default:
                    throw new NotImplementedException($"Literal type {literal.LiteralType} not implemented");
            }
        }

        /// <summary>
        /// Generate bytecode for an identifier expression
        /// </summary>
        private void GenerateIdentifier(IdentifierExpression identifier)
        {
            if (_localVariables.TryGetValue(identifier.Name, out var localIndex))
            {
                // Load from local variable
                EmitLoadLocal(localIndex, identifier.EvaluatedType?.Name ?? "int", identifier.Location.Line);
                PushStack();
            }
            else
            {
                // Assume it's a field access on 'this'
                // In a real implementation, we'd check the symbol table
                throw new InvalidOperationException($"Undefined variable: {identifier.Name}");
            }
        }

        /// <summary>
        /// Generate bytecode for a binary expression
        /// </summary>
        private void GenerateBinaryExpression(BinaryExpression binary)
        {
            // Generate left operand
            GenerateExpression(binary.Left);

            // Generate right operand
            GenerateExpression(binary.Right);

            // Generate operation
            byte opcode;
            switch (binary.Operator)
            {
                case BinaryOperator.Add:
                    opcode = Opcode.ADD_INT;
                    break;
                case BinaryOperator.Subtract:
                    opcode = Opcode.SUB_INT;
                    break;
                case BinaryOperator.Multiply:
                    opcode = Opcode.MUL_INT;
                    break;
                case BinaryOperator.Divide:
                    opcode = Opcode.DIV_INT;
                    break;
                case BinaryOperator.Modulo:
                    opcode = Opcode.REM_INT;
                    break;
                case BinaryOperator.BitwiseAnd:
                    opcode = Opcode.AND_INT;
                    break;
                case BinaryOperator.BitwiseOr:
                    opcode = Opcode.OR_INT;
                    break;
                case BinaryOperator.BitwiseXor:
                    opcode = Opcode.XOR_INT;
                    break;
                case BinaryOperator.Equal:
                case BinaryOperator.NotEqual:
                case BinaryOperator.Less:
                case BinaryOperator.Greater:
                case BinaryOperator.LessEqual:
                case BinaryOperator.GreaterEqual:
                    // Comparison operators need special handling
                    GenerateComparison(binary.Operator, binary.Location.Line);
                    return;
                default:
                    throw new NotImplementedException($"Binary operator {binary.Operator} not implemented");
            }

            EmitInstruction(opcode, new byte[] { 0, 0, 0 }, binary.Location.Line);
            PopStack(); // Two operands consumed, one result produced
        }

        /// <summary>
        /// Generate comparison bytecode
        /// </summary>
        private void GenerateComparison(BinaryOperator op, int line)
        {
            var trueLabel = GenerateLabel();
            var endLabel = GenerateLabel();

            byte branchOpcode = op switch
            {
                BinaryOperator.Equal => Opcode.IF_EQ,
                BinaryOperator.NotEqual => Opcode.IF_NE,
                BinaryOperator.Less => Opcode.IF_LT,
                BinaryOperator.Greater => Opcode.IF_GT,
                BinaryOperator.LessEqual => Opcode.IF_LE,
                BinaryOperator.GreaterEqual => Opcode.IF_GE,
                _ => throw new InvalidOperationException($"Invalid comparison operator: {op}")
            };

            // Branch if comparison is true
            EmitConditionalBranch(branchOpcode, trueLabel, line);
            PopStack();
            PopStack();

            // False case: push 0
            EmitInstruction(Opcode.CONST_4, new byte[] { 0 }, line);
            PushStack();
            EmitBranch(endLabel, line);

            // True case: push 1
            MarkLabel(trueLabel);
            EmitInstruction(Opcode.CONST_4, new byte[] { 1 }, line);
            PushStack();

            MarkLabel(endLabel);
        }

        /// <summary>
        /// Generate bytecode for a unary expression
        /// </summary>
        private void GenerateUnaryExpression(UnaryExpression unary)
        {
            GenerateExpression(unary.Operand);

            switch (unary.Operator)
            {
                case UnaryOperator.Negate:
                    // Negate: 0 - operand
                    EmitInstruction(Opcode.CONST_4, new byte[] { 0 }, unary.Location.Line);
                    PushStack();
                    EmitInstruction(Opcode.SUB_INT, new byte[] { 0, 0, 0 }, unary.Location.Line);
                    PopStack();
                    break;

                case UnaryOperator.LogicalNot:
                    // Logical not: operand == 0
                    EmitInstruction(Opcode.CONST_4, new byte[] { 0 }, unary.Location.Line);
                    PushStack();
                    GenerateComparison(BinaryOperator.Equal, unary.Location.Line);
                    break;

                case UnaryOperator.BitwiseNot:
                    // Bitwise not: operand XOR -1
                    EmitInstruction(Opcode.CONST_4, new byte[] { 0xFF }, unary.Location.Line);
                    PushStack();
                    EmitInstruction(Opcode.XOR_INT, new byte[] { 0, 0, 0 }, unary.Location.Line);
                    PopStack();
                    break;

                default:
                    throw new NotImplementedException($"Unary operator {unary.Operator} not implemented");
            }
        }

        /// <summary>
        /// Generate bytecode for a method call
        /// </summary>
        private void GenerateMethodCall(MethodCallExpression methodCall)
        {
            // Generate target (receiver) if present
            if (methodCall.Target != null)
            {
                GenerateExpression(methodCall.Target);
            }

            // Generate arguments
            foreach (var arg in methodCall.Arguments)
            {
                GenerateExpression(arg);
            }

            // Determine invocation type
            byte invokeOpcode;
            if (methodCall.Target == null)
            {
                // Static or local method call
                invokeOpcode = Opcode.INVOKE_STATIC;
            }
            else
            {
                // Instance method call
                invokeOpcode = Opcode.INVOKE_VIRTUAL;
            }

            // Create method reference
            var methodRef = $"{methodCall.MethodName}";
            var methodRefId = _resources.Strings.AddString(methodRef);
            var methodRefBytes = BitConverter.GetBytes(methodRefId);

            EmitInstruction(invokeOpcode, methodRefBytes, methodCall.Location.Line);

            // Pop arguments and receiver from stack
            if (methodCall.Target != null)
                PopStack();
            foreach (var _ in methodCall.Arguments)
                PopStack();

            // Push return value if method returns something
            if (methodCall.EvaluatedType?.Name != "void")
            {
                PushStack();
            }
        }

        /// <summary>
        /// Generate bytecode for member access (field access)
        /// </summary>
        private void GenerateMemberAccess(MemberAccessExpression memberAccess)
        {
            // Generate target object
            GenerateExpression(memberAccess.Target);

            // Get field reference
            var fieldRef = memberAccess.MemberName;
            var fieldRefId = _resources.Strings.AddString(fieldRef);
            var fieldRefBytes = BitConverter.GetBytes(fieldRefId);

            // Instance field get
            EmitInstruction(Opcode.IGET, fieldRefBytes, memberAccess.Location.Line);
            // Stack: object -> value
        }

        /// <summary>
        /// Generate bytecode for assignment
        /// </summary>
        private void GenerateAssignment(AssignmentExpression assignment)
        {
            // Generate value to assign
            GenerateExpression(assignment.Value);

            // Handle different assignment targets
            if (assignment.Target is IdentifierExpression identifier)
            {
                // Local variable assignment
                if (_localVariables.TryGetValue(identifier.Name, out var localIndex))
                {
                    EmitStoreLocal(localIndex, assignment.Value.EvaluatedType?.Name ?? "int", assignment.Location.Line);
                    PopStack();
                }
                else
                {
                    throw new InvalidOperationException($"Undefined variable: {identifier.Name}");
                }
            }
            else if (assignment.Target is MemberAccessExpression memberAccess)
            {
                // Field assignment
                GenerateExpression(memberAccess.Target);
                
                var fieldRef = memberAccess.MemberName;
                var fieldRefId = _resources.Strings.AddString(fieldRef);
                var fieldRefBytes = BitConverter.GetBytes(fieldRefId);

                EmitInstruction(Opcode.IPUT, fieldRefBytes, assignment.Location.Line);
                PopStack(); // Pop value
                PopStack(); // Pop object
            }
            else if (assignment.Target is ArrayAccessExpression arrayAccess)
            {
                // Array element assignment
                GenerateExpression(arrayAccess.Array);
                GenerateExpression(arrayAccess.Index);

                EmitInstruction(Opcode.APUT, new byte[] { 0, 0, 0 }, assignment.Location.Line);
                PopStack(); // Pop value
                PopStack(); // Pop index
                PopStack(); // Pop array
            }
            else
            {
                throw new NotImplementedException($"Assignment target type {assignment.Target.GetType().Name} not implemented");
            }

            // Assignment expression produces the assigned value
            GenerateExpression(assignment.Value);
        }

        /// <summary>
        /// Generate bytecode for array access
        /// </summary>
        private void GenerateArrayAccess(ArrayAccessExpression arrayAccess)
        {
            // Generate array reference
            GenerateExpression(arrayAccess.Array);

            // Generate index
            GenerateExpression(arrayAccess.Index);

            // Array get
            EmitInstruction(Opcode.AGET, new byte[] { 0, 0, 0 }, arrayAccess.Location.Line);
            PopStack(); // Pop index
            // Stack: array -> value
        }

        /// <summary>
        /// Generate bytecode for new expression (object/array creation)
        /// </summary>
        private void GenerateNewExpression(NewExpression newExpr)
        {
            if (newExpr.IsArray)
            {
                // Array creation
                // Generate array size
                if (newExpr.Arguments.Count > 0)
                {
                    GenerateExpression(newExpr.Arguments[0]);
                }
                else
                {
                    EmitInstruction(Opcode.CONST_4, new byte[] { 0 }, newExpr.Location.Line);
                    PushStack();
                }

                // Get type reference
                var typeDescriptor = GetTypeDescriptor(newExpr.TypeToCreate);
                var typeId = _resources.Strings.AddString(typeDescriptor);
                var typeBytes = BitConverter.GetBytes(typeId);

                EmitInstruction(Opcode.NEW_ARRAY, typeBytes, newExpr.Location.Line);
                // Stack: size -> array
            }
            else
            {
                // Object creation
                var className = newExpr.TypeToCreate.Name;
                var classId = _resources.Strings.AddString(className);
                var classBytes = BitConverter.GetBytes(classId);

                // Create new instance
                EmitInstruction(Opcode.NEW_INSTANCE, classBytes, newExpr.Location.Line);
                PushStack();

                // Call constructor if arguments present
                if (newExpr.Arguments.Count > 0)
                {
                    // Duplicate instance reference for constructor call
                    EmitInstruction(Opcode.MOVE, new byte[] { 0, 0 }, newExpr.Location.Line);
                    PushStack();

                    // Generate constructor arguments
                    foreach (var arg in newExpr.Arguments)
                    {
                        GenerateExpression(arg);
                    }

                    // Call constructor
                    var ctorRef = $"{className}.<init>";
                    var ctorId = _resources.Strings.AddString(ctorRef);
                    var ctorBytes = BitConverter.GetBytes(ctorId);

                    EmitInstruction(Opcode.INVOKE_DIRECT, ctorBytes, newExpr.Location.Line);

                    // Pop constructor arguments and duplicate instance
                    foreach (var _ in newExpr.Arguments)
                        PopStack();
                    PopStack();
                }
            }
        }

        /// <summary>
        /// Generate bytecode for cast expression
        /// </summary>
        private void GenerateCastExpression(CastExpression cast)
        {
            // Generate expression to cast
            GenerateExpression(cast.Expression);

            // Get target type
            var targetType = GetTypeDescriptor(cast.TargetType);
            var typeId = _resources.Strings.AddString(targetType);
            var typeBytes = BitConverter.GetBytes(typeId);

            // Check cast
            EmitInstruction(Opcode.CHECK_CAST, typeBytes, cast.Location.Line);
            // Stack unchanged: value -> value (but type-checked)
        }

        /// <summary>
        /// Generate bytecode for ternary expression (condition ? true : false)
        /// </summary>
        private void GenerateTernaryExpression(TernaryExpression ternary)
        {
            var falseLabel = GenerateLabel();
            var endLabel = GenerateLabel();

            // Generate condition
            GenerateExpression(ternary.Condition);

            // Branch to false if condition is false
            EmitConditionalBranch(Opcode.IF_EQ, falseLabel, ternary.Location.Line);
            PopStack();

            // True branch
            GenerateExpression(ternary.TrueExpression);
            EmitBranch(endLabel, ternary.Location.Line);

            // False branch
            MarkLabel(falseLabel);
            GenerateExpression(ternary.FalseExpression);

            MarkLabel(endLabel);
        }

        // Helper methods

        private void EmitInstruction(byte opcode, byte[] operands, int sourceLine)
        {
            if (_currentMethod == null)
                throw new InvalidOperationException("No current method");

            _currentMethod.Instructions.Add(new BytecodeInstruction(opcode, operands, (uint)sourceLine));
        }

        private void EmitReturn(string returnType, int line)
        {
            byte opcode = returnType switch
            {
                "void" => Opcode.RETURN_VOID,
                "long" or "double" => Opcode.RETURN_WIDE,
                _ when IsReferenceType(returnType) => Opcode.RETURN_OBJECT,
                _ => Opcode.RETURN
            };

            EmitInstruction(opcode, new byte[] { 0 }, line);
            if (opcode != Opcode.RETURN_VOID)
                PopStack();
        }

        private void EmitLoadLocal(ushort localIndex, string type, int line)
        {
            byte opcode = type switch
            {
                "long" or "double" => Opcode.MOVE_WIDE,
                _ when IsReferenceType(type) => Opcode.MOVE_OBJECT,
                _ => Opcode.MOVE
            };

            EmitInstruction(opcode, BitConverter.GetBytes(localIndex), line);
        }

        private void EmitStoreLocal(ushort localIndex, string type, int line)
        {
            byte opcode = type switch
            {
                "long" or "double" => Opcode.MOVE_WIDE,
                _ when IsReferenceType(type) => Opcode.MOVE_OBJECT,
                _ => Opcode.MOVE
            };

            EmitInstruction(opcode, BitConverter.GetBytes(localIndex), line);
        }

        private void EmitBranch(string label, int line)
        {
            var instructionIndex = _currentMethod!.Instructions.Count;
            _unresolvedLabels.Add((instructionIndex, label));
            EmitInstruction(Opcode.NOP, new byte[] { 0, 0 }, line); // Placeholder, will be patched
        }

        private void EmitConditionalBranch(byte branchOpcode, string label, int line)
        {
            var instructionIndex = _currentMethod!.Instructions.Count;
            _unresolvedLabels.Add((instructionIndex, label));
            EmitInstruction(branchOpcode, new byte[] { 0, 0 }, line); // Placeholder, will be patched
        }

        private string GenerateLabel()
        {
            return $"L{_nextLabelId++}";
        }

        private void MarkLabel(string label)
        {
            if (_currentMethod == null)
                throw new InvalidOperationException("No current method");

            _labels[label] = _currentMethod.Instructions.Count;
        }

        private void PushStack()
        {
            _currentStackSize++;
            if (_currentStackSize > _maxStackSize)
                _maxStackSize = _currentStackSize;
        }

        private void PopStack()
        {
            if (_currentStackSize > 0)
                _currentStackSize--;
        }

        private bool IsReferenceType(string typeName)
        {
            return typeName != "int" && typeName != "long" && typeName != "float" && 
                   typeName != "double" && typeName != "boolean" && typeName != "byte" &&
                   typeName != "short" && typeName != "char" && typeName != "void";
        }

        private string GetFullClassName(ClassDeclaration classDecl)
        {
            // In a real implementation, this would include package/namespace
            return classDecl.Name;
        }

        private AccessFlags ConvertAccessModifier(AccessModifier modifier)
        {
            return modifier switch
            {
                AccessModifier.Public => AccessFlags.Public,
                AccessModifier.Private => AccessFlags.Private,
                AccessModifier.Protected => AccessFlags.Protected,
                _ => AccessFlags.Public
            };
        }

        private string GetTypeDescriptor(TypeReference typeRef)
        {
            if (typeRef.IsArray)
            {
                return TypeDescriptor.Array(GetBaseTypeDescriptor(typeRef.Name));
            }
            return GetBaseTypeDescriptor(typeRef.Name);
        }

        private string GetBaseTypeDescriptor(string typeName)
        {
            return typeName switch
            {
                "void" => TypeDescriptor.Void,
                "boolean" => TypeDescriptor.Boolean,
                "byte" => TypeDescriptor.Byte,
                "short" => TypeDescriptor.Short,
                "char" => TypeDescriptor.Char,
                "int" => TypeDescriptor.Int,
                "long" => TypeDescriptor.Long,
                "float" => TypeDescriptor.Float,
                "double" => TypeDescriptor.Double,
                _ => TypeDescriptor.Class(typeName)
            };
        }

        private string GetMethodSignature(MethodDeclaration method)
        {
            var paramTypes = method.Parameters
                .Select(p => GetTypeDescriptor(p.ParameterType))
                .ToArray();
            var returnType = GetTypeDescriptor(method.ReturnType);
            return TypeDescriptor.MethodSignature(paramTypes, returnType);
        }

        /// <summary>
        /// Emit reference count initialization for a variable
        /// </summary>
        private void EmitRefCountInit(string variableName, int line)
        {
            if (!_localVariables.TryGetValue(variableName, out var localIndex))
                return;

            // Load the variable
            EmitLoadLocal(localIndex, "object", line);

            // Call __ref_count_init runtime function
            var funcId = _resources.Strings.AddString("__ref_count_init");
            var funcBytes = BitConverter.GetBytes(funcId);
            EmitInstruction(Opcode.INVOKE_STATIC, funcBytes, line);
            PopStack();
        }

        /// <summary>
        /// Emit scope cleanup code (reference counting and deallocation)
        /// </summary>
        private void EmitScopeCleanup(SourceLocation location)
        {
            if (_memoryManagement == null)
                return;

            var locationKey = $"{location.File}:{location.Line}:{location.Column}";

            // Find all deallocations at this location
            var deallocations = _memoryManagement.Deallocations
                .Where(d => LocationMatches(d.Location, location))
                .ToList();

            foreach (var dealloc in deallocations)
            {
                if (!_localVariables.TryGetValue(dealloc.VariableName, out var localIndex))
                    continue;

                // Emit reference count decrement
                EmitLoadLocal(localIndex, "object", location.Line);
                var decFuncId = _resources.Strings.AddString("__ref_count_dec");
                var decFuncBytes = BitConverter.GetBytes(decFuncId);
                EmitInstruction(Opcode.INVOKE_STATIC, decFuncBytes, location.Line);
                PopStack();

                // Emit conditional delete
                EmitLoadLocal(localIndex, "object", location.Line);
                var checkFuncId = _resources.Strings.AddString("__ref_count_is_zero");
                var checkFuncBytes = BitConverter.GetBytes(checkFuncId);
                EmitInstruction(Opcode.INVOKE_STATIC, checkFuncBytes, location.Line);

                // Branch if not zero
                var skipLabel = GenerateLabel();
                EmitConditionalBranch(Opcode.IF_EQ, skipLabel, location.Line);
                PopStack();

                // Call appropriate delete function
                EmitLoadLocal(localIndex, "object", location.Line);
                var deleteFuncName = dealloc.Kind == DeallocationKind.DeleteArray
                    ? "__safe_delete_array"
                    : "__safe_delete";
                var deleteFuncId = _resources.Strings.AddString(deleteFuncName);
                var deleteFuncBytes = BitConverter.GetBytes(deleteFuncId);
                EmitInstruction(Opcode.INVOKE_STATIC, deleteFuncBytes, location.Line);
                PopStack();

                MarkLabel(skipLabel);
            }
        }

        /// <summary>
        /// Emit null safety check before pointer dereference
        /// </summary>
        private void EmitNullSafetyCheck(string variableName, int line)
        {
            if (!_localVariables.TryGetValue(variableName, out var localIndex))
                return;

            // Load the variable
            EmitLoadLocal(localIndex, "object", line);

            // Compare with null
            EmitInstruction(Opcode.CONST_4, new byte[] { 0 }, line);
            PushStack();

            // Branch if not equal (not null)
            var notNullLabel = GenerateLabel();
            EmitConditionalBranch(Opcode.IF_NE, notNullLabel, line);
            PopStack();
            PopStack();

            // Throw NullPointerException
            var exceptionClass = _resources.Strings.AddString("java.lang.NullPointerException");
            var exceptionBytes = BitConverter.GetBytes(exceptionClass);
            EmitInstruction(Opcode.NEW_INSTANCE, exceptionBytes, line);
            PushStack();

            // Create exception message
            var message = $"Null pointer dereference: {variableName}";
            var messageId = _resources.Strings.AddString(message);
            var messageBytes = BitConverter.GetBytes(messageId);
            EmitInstruction(Opcode.CONST_STRING, messageBytes, line);
            PushStack();

            // Call exception constructor
            var ctorId = _resources.Strings.AddString("java.lang.NullPointerException.<init>");
            var ctorBytes = BitConverter.GetBytes(ctorId);
            EmitInstruction(Opcode.INVOKE_DIRECT, ctorBytes, line);
            PopStack();
            PopStack();

            // Throw (would need proper throw instruction)
            EmitInstruction(Opcode.NOP, Array.Empty<byte>(), line);

            MarkLabel(notNullLabel);
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
