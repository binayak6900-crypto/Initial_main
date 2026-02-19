using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Type checker for validating type usage in ADL code.
/// Validates type compatibility for assignments, method calls, and expressions.
/// Requirements: 4.2, 4.3, 4.6, 4.7
/// </summary>
public class TypeChecker
{
    private readonly SymbolTable _rootSymbolTable;
    private SymbolTable _currentScope;
    private readonly TypeSystem _typeSystem;
    private readonly List<SemanticException> _errors;
    private readonly APICompatibilityChecker? _apiCompatibilityChecker;
    
    public TypeChecker(SymbolTable symbolTable, APICompatibilityChecker? apiCompatibilityChecker = null)
    {
        _rootSymbolTable = symbolTable;
        _currentScope = symbolTable;
        _typeSystem = new TypeSystem();
        _errors = new List<SemanticException>();
        _apiCompatibilityChecker = apiCompatibilityChecker;
    }
    
    /// <summary>
    /// Gets all type checking errors found during analysis
    /// </summary>
    public IReadOnlyList<SemanticException> Errors => _errors;
    
    /// <summary>
    /// Checks types for a compilation unit
    /// </summary>
    public bool Check(CompilationUnit compilationUnit)
    {
        _errors.Clear();
        
        try
        {
            // Process namespace if present
            if (compilationUnit.Namespace != null)
            {
                CheckNamespace(compilationUnit.Namespace);
            }
            
            // Process top-level classes
            foreach (var classDecl in compilationUnit.Classes)
            {
                CheckClass(classDecl);
            }
        }
        catch (SemanticException ex)
        {
            _errors.Add(ex);
        }
        
        return _errors.Count == 0;
    }
    
    private void CheckNamespace(NamespaceDeclaration namespaceDecl)
    {
        // Create namespace scope
        var namespaceScope = _currentScope.CreateChildScope(ScopeType.Namespace, namespaceDecl.Name);
        
        var previousScope = _currentScope;
        _currentScope = namespaceScope;
        
        // Check classes in namespace
        foreach (var classDecl in namespaceDecl.Classes)
        {
            CheckClass(classDecl);
        }
        
        // Check functions in namespace
        foreach (var function in namespaceDecl.Functions)
        {
            CheckMethod(function);
        }
        
        _currentScope = previousScope;
    }
    
    private void CheckClass(ClassDeclaration classDecl)
    {
        // Create class scope
        var classScope = _currentScope.CreateChildScope(ScopeType.Class, classDecl.Name);
        
        var previousScope = _currentScope;
        _currentScope = classScope;
        
        // Check fields
        foreach (var field in classDecl.Fields)
        {
            ValidateType(field.FieldType, field.Location);
        }
        
        // Check methods
        foreach (var method in classDecl.Methods)
        {
            CheckMethod(method);
        }
        
        _currentScope = previousScope;
    }
    
    private void CheckMethod(MethodDeclaration methodDecl)
    {
        // Validate return type
        ValidateType(methodDecl.ReturnType, methodDecl.Location);
        
        // Validate parameter types
        foreach (var param in methodDecl.Parameters)
        {
            ValidateType(param.ParameterType, param.Location);
        }
        
        // Check method body
        if (methodDecl.Body != null)
        {
            CheckStatement(methodDecl.Body, methodDecl.ReturnType);
        }
    }
    
    private void CheckStatement(Statement statement, TypeReference? expectedReturnType = null)
    {
        try
        {
            switch (statement)
            {
                case BlockStatement blockStmt:
                    foreach (var stmt in blockStmt.Statements)
                    {
                        CheckStatement(stmt, expectedReturnType);
                    }
                    break;
                    
                case VariableDeclarationStatement varDecl:
                    CheckVariableDeclaration(varDecl);
                    break;
                    
                case ExpressionStatement exprStmt:
                    CheckExpression(exprStmt.Expression);
                    break;
                    
                case ReturnStatement returnStmt:
                    CheckReturnStatement(returnStmt, expectedReturnType);
                    break;
                    
                case IfStatement ifStmt:
                    CheckIfStatement(ifStmt, expectedReturnType);
                    break;
                    
                case WhileStatement whileStmt:
                    CheckWhileStatement(whileStmt, expectedReturnType);
                    break;
                    
                case DoWhileStatement doWhileStmt:
                    CheckDoWhileStatement(doWhileStmt, expectedReturnType);
                    break;
                    
                case ForStatement forStmt:
                    CheckForStatement(forStmt, expectedReturnType);
                    break;
                    
                case SwitchStatement switchStmt:
                    CheckSwitchStatement(switchStmt, expectedReturnType);
                    break;
                    
                case BreakStatement:
                case ContinueStatement:
                    // No type checking needed
                    break;
            }
        }
        catch (SemanticException ex)
        {
            _errors.Add(ex);
        }
    }
    
    private void CheckVariableDeclaration(VariableDeclarationStatement varDecl)
    {
        // Check if type inference is needed (var/auto)
        if (varDecl.VariableType.Name == "var" || varDecl.VariableType.Name == "auto")
        {
            if (varDecl.Initializer == null)
            {
                throw new SemanticException(
                    $"Variable '{varDecl.Name}' with inferred type must have an initializer",
                    varDecl.Location
                );
            }
            
            // Infer type from initializer
            var inferredType = CheckExpression(varDecl.Initializer);
            varDecl.VariableType = inferredType;
        }
        else
        {
            // Validate declared type
            ValidateType(varDecl.VariableType, varDecl.Location);
        }
        
        // Check initializer if present
        if (varDecl.Initializer != null)
        {
            var initType = CheckExpression(varDecl.Initializer);
            if (!_typeSystem.IsAssignableFrom(varDecl.VariableType, initType))
            {
                throw new SemanticException(
                    $"Cannot assign '{initType.Name}' to variable of type '{varDecl.VariableType.Name}'",
                    varDecl.Location
                );
            }
        }
    }
    
    private void CheckReturnStatement(ReturnStatement returnStmt, TypeReference? expectedReturnType)
    {
        if (expectedReturnType == null)
        {
            return;
        }
        
        if (returnStmt.Value == null)
        {
            if (expectedReturnType.Name != "void")
            {
                throw new SemanticException(
                    $"Method must return a value of type '{expectedReturnType.Name}'",
                    returnStmt.Location
                );
            }
        }
        else
        {
            var returnType = CheckExpression(returnStmt.Value);
            if (!_typeSystem.IsAssignableFrom(expectedReturnType, returnType))
            {
                throw new SemanticException(
                    $"Cannot return '{returnType.Name}' from method expecting '{expectedReturnType.Name}'",
                    returnStmt.Location
                );
            }
        }
    }
    
    private void CheckIfStatement(IfStatement ifStmt, TypeReference? expectedReturnType)
    {
        var conditionType = CheckExpression(ifStmt.Condition);
        if (!_typeSystem.IsBoolean(conditionType))
        {
            throw new SemanticException(
                $"If condition must be boolean, got '{conditionType.Name}'",
                ifStmt.Location
            );
        }
        
        CheckStatement(ifStmt.ThenBranch, expectedReturnType);
        if (ifStmt.ElseBranch != null)
        {
            CheckStatement(ifStmt.ElseBranch, expectedReturnType);
        }
    }
    
    private void CheckWhileStatement(WhileStatement whileStmt, TypeReference? expectedReturnType)
    {
        var conditionType = CheckExpression(whileStmt.Condition);
        if (!_typeSystem.IsBoolean(conditionType))
        {
            throw new SemanticException(
                $"While condition must be boolean, got '{conditionType.Name}'",
                whileStmt.Location
            );
        }
        
        CheckStatement(whileStmt.Body, expectedReturnType);
    }
    
    private void CheckDoWhileStatement(DoWhileStatement doWhileStmt, TypeReference? expectedReturnType)
    {
        CheckStatement(doWhileStmt.Body, expectedReturnType);
        
        var conditionType = CheckExpression(doWhileStmt.Condition);
        if (!_typeSystem.IsBoolean(conditionType))
        {
            throw new SemanticException(
                $"Do-while condition must be boolean, got '{conditionType.Name}'",
                doWhileStmt.Location
            );
        }
    }
    
    private void CheckForStatement(ForStatement forStmt, TypeReference? expectedReturnType)
    {
        if (forStmt.Initializer != null)
        {
            CheckStatement(forStmt.Initializer, null);
        }
        
        if (forStmt.Condition != null)
        {
            var conditionType = CheckExpression(forStmt.Condition);
            if (!_typeSystem.IsBoolean(conditionType))
            {
                throw new SemanticException(
                    $"For condition must be boolean, got '{conditionType.Name}'",
                    forStmt.Location
                );
            }
        }
        
        if (forStmt.Increment != null)
        {
            CheckExpression(forStmt.Increment);
        }
        
        CheckStatement(forStmt.Body, expectedReturnType);
    }
    
    private void CheckSwitchStatement(SwitchStatement switchStmt, TypeReference? expectedReturnType)
    {
        var switchType = CheckExpression(switchStmt.Expression);
        
        foreach (var caseStmt in switchStmt.Cases)
        {
            if (caseStmt.Value != null)
            {
                var caseType = CheckExpression(caseStmt.Value);
                if (!_typeSystem.IsAssignableFrom(switchType, caseType))
                {
                    throw new SemanticException(
                        $"Case value type '{caseType.Name}' does not match switch expression type '{switchType.Name}'",
                        caseStmt.Location
                    );
                }
            }
            
            foreach (var stmt in caseStmt.Statements)
            {
                CheckStatement(stmt, expectedReturnType);
            }
        }
    }
    
    private TypeReference CheckExpression(Expression expression)
    {
        return expression switch
        {
            BinaryExpression binaryExpr => CheckBinaryExpression(binaryExpr),
            UnaryExpression unaryExpr => CheckUnaryExpression(unaryExpr),
            LiteralExpression literalExpr => CheckLiteralExpression(literalExpr),
            IdentifierExpression identifierExpr => CheckIdentifierExpression(identifierExpr),
            MethodCallExpression methodCallExpr => CheckMethodCallExpression(methodCallExpr),
            MemberAccessExpression memberAccessExpr => CheckMemberAccessExpression(memberAccessExpr),
            AssignmentExpression assignmentExpr => CheckAssignmentExpression(assignmentExpr),
            ArrayAccessExpression arrayAccessExpr => CheckArrayAccessExpression(arrayAccessExpr),
            NewExpression newExpr => CheckNewExpression(newExpr),
            CastExpression castExpr => CheckCastExpression(castExpr),
            _ => throw new SemanticException($"Unknown expression type: {expression.GetType().Name}", expression.Location)
        };
    }
    
    private TypeReference CheckBinaryExpression(BinaryExpression binaryExpr)
    {
        var leftType = CheckExpression(binaryExpr.Left);
        var rightType = CheckExpression(binaryExpr.Right);
        
        return _typeSystem.GetBinaryOperationResultType(binaryExpr.Operator, leftType, rightType, binaryExpr.Location);
    }
    
    private TypeReference CheckUnaryExpression(UnaryExpression unaryExpr)
    {
        var operandType = CheckExpression(unaryExpr.Operand);
        return _typeSystem.GetUnaryOperationResultType(unaryExpr.Operator, operandType, unaryExpr.Location);
    }
    
    private TypeReference CheckLiteralExpression(LiteralExpression literalExpr)
    {
        return _typeSystem.GetLiteralType(literalExpr.Value);
    }
    
    private TypeReference CheckIdentifierExpression(IdentifierExpression identifierExpr)
    {
        var symbol = _currentScope.Lookup(identifierExpr.Name);
        if (symbol == null)
        {
            throw new SemanticException(
                $"Undefined symbol: '{identifierExpr.Name}'",
                identifierExpr.Location
            );
        }
        
        return symbol.Type;
    }
    
    private TypeReference CheckMethodCallExpression(MethodCallExpression methodCallExpr)
    {
        // Check API compatibility if checker is available
        _apiCompatibilityChecker?.CheckMethodCall(methodCallExpr);
        
        // Look up method symbol
        var methodSymbol = _currentScope.Lookup(methodCallExpr.MethodName);
        if (methodSymbol == null || methodSymbol.Kind != SymbolKind.Method)
        {
            throw new SemanticException(
                $"Undefined method: '{methodCallExpr.MethodName}'",
                methodCallExpr.Location
            );
        }
        
        // Check argument types
        // Note: Full parameter validation would require method signature information
        foreach (var arg in methodCallExpr.Arguments)
        {
            CheckExpression(arg);
        }
        
        return methodSymbol.Type;
    }
    
    private TypeReference CheckMemberAccessExpression(MemberAccessExpression memberAccessExpr)
    {
        var objectType = CheckExpression(memberAccessExpr.Target);
        
        // Check API compatibility if checker is available
        _apiCompatibilityChecker?.CheckFieldAccess(memberAccessExpr);
        
        // Look up member in the object's type
        // For now, we'll return a placeholder type
        // Full implementation would require class member lookup
        return new TypeReference { Name = "unknown" };
    }
    
    private TypeReference CheckAssignmentExpression(AssignmentExpression assignmentExpr)
    {
        var leftType = CheckExpression(assignmentExpr.Target);
        var rightType = CheckExpression(assignmentExpr.Value);
        
        if (!_typeSystem.IsAssignableFrom(leftType, rightType))
        {
            throw new SemanticException(
                $"Cannot assign '{rightType.Name}' to '{leftType.Name}'",
                assignmentExpr.Location
            );
        }
        
        return leftType;
    }
    
    private TypeReference CheckArrayAccessExpression(ArrayAccessExpression arrayAccessExpr)
    {
        var arrayType = CheckExpression(arrayAccessExpr.Array);
        var indexType = CheckExpression(arrayAccessExpr.Index);
        
        if (!arrayType.IsArray)
        {
            throw new SemanticException(
                $"Cannot index non-array type '{arrayType.Name}'",
                arrayAccessExpr.Location
            );
        }
        
        if (!_typeSystem.IsInteger(indexType))
        {
            throw new SemanticException(
                $"Array index must be integer, got '{indexType.Name}'",
                arrayAccessExpr.Location
            );
        }
        
        // Return element type (array type without array flag)
        return new TypeReference
        {
            Name = arrayType.Name,
            IsArray = false,
            IsPointer = arrayType.IsPointer,
            PointerLevel = arrayType.PointerLevel
        };
    }
    
    private TypeReference CheckNewExpression(NewExpression newExpr)
    {
        ValidateType(newExpr.TypeToCreate, newExpr.Location);
        
        // Check API compatibility for class usage
        _apiCompatibilityChecker?.CheckClassUsage(newExpr.TypeToCreate.Name, newExpr.Location);
        
        // Check constructor arguments
        foreach (var arg in newExpr.Arguments)
        {
            CheckExpression(arg);
        }
        
        return newExpr.TypeToCreate;
    }
    
    private TypeReference CheckCastExpression(CastExpression castExpr)
    {
        ValidateType(castExpr.TargetType, castExpr.Location);
        CheckExpression(castExpr.Expression);
        
        return castExpr.TargetType;
    }
    
    private void ValidateType(TypeReference typeRef, SourceLocation location)
    {
        if (!_typeSystem.IsValidType(typeRef))
        {
            throw new SemanticException(
                $"Unknown type: '{typeRef.Name}'",
                location
            );
        }
    }
}
