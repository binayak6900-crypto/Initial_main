using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Builds symbol tables from AST nodes by traversing the tree and creating appropriate scopes.
/// Requirements: 4.2, 4.4, 4.5
/// </summary>
public class SymbolTableBuilder
{
    private SymbolTable _currentScope;
    
    public SymbolTableBuilder()
    {
        _currentScope = new SymbolTable(null, ScopeType.Global, "global");
    }
    
    /// <summary>
    /// Gets the current symbol table (root scope)
    /// </summary>
    public SymbolTable GlobalScope => GetRootScope();
    
    /// <summary>
    /// Builds symbol tables for a compilation unit
    /// </summary>
    public SymbolTable Build(CompilationUnit compilationUnit)
    {
        _currentScope = new SymbolTable(null, ScopeType.Global, "global");
        
        // Process namespace if present
        if (compilationUnit.Namespace != null)
        {
            ProcessNamespace(compilationUnit.Namespace);
        }
        
        // Process top-level classes
        foreach (var classDecl in compilationUnit.Classes)
        {
            ProcessClass(classDecl);
        }
        
        return _currentScope;
    }
    
    private void ProcessNamespace(NamespaceDeclaration namespaceDecl)
    {
        // Create namespace scope
        var namespaceScope = _currentScope.CreateChildScope(ScopeType.Namespace, namespaceDecl.Name);
        var previousScope = _currentScope;
        _currentScope = namespaceScope;
        
        // Define namespace symbol in parent scope
        previousScope.Define(namespaceDecl.Name, new Symbol(
            namespaceDecl.Name,
            new TypeReference { Name = namespaceDecl.Name },
            SymbolKind.Namespace,
            namespaceDecl.Location
        ));
        
        // Process classes in namespace
        foreach (var classDecl in namespaceDecl.Classes)
        {
            ProcessClass(classDecl);
        }
        
        // Process functions in namespace
        foreach (var function in namespaceDecl.Functions)
        {
            ProcessMethod(function);
        }
        
        _currentScope = previousScope;
    }
    
    private void ProcessClass(ClassDeclaration classDecl)
    {
        // Define class symbol in current scope
        _currentScope.Define(classDecl.Name, new Symbol(
            classDecl.Name,
            new TypeReference { Name = classDecl.Name },
            SymbolKind.Class,
            classDecl.Location
        ));
        
        // Create class scope
        var classScope = _currentScope.CreateChildScope(ScopeType.Class, classDecl.Name);
        var previousScope = _currentScope;
        _currentScope = classScope;
        
        // Process fields
        foreach (var field in classDecl.Fields)
        {
            _currentScope.Define(field.Name, new Symbol(
                field.Name,
                field.FieldType,
                SymbolKind.Field,
                field.Location
            ));
        }
        
        // Process methods
        foreach (var method in classDecl.Methods)
        {
            ProcessMethod(method);
        }
        
        _currentScope = previousScope;
    }
    
    private void ProcessMethod(MethodDeclaration methodDecl)
    {
        // Define method symbol in current scope (class or namespace)
        _currentScope.Define(methodDecl.Name, new Symbol(
            methodDecl.Name,
            methodDecl.ReturnType,
            SymbolKind.Method,
            methodDecl.Location
        ));
        
        // Create method scope
        var methodScope = _currentScope.CreateChildScope(ScopeType.Method, methodDecl.Name);
        var previousScope = _currentScope;
        _currentScope = methodScope;
        
        // Add parameters to method scope
        foreach (var param in methodDecl.Parameters)
        {
            _currentScope.Define(param.Name, new Symbol(
                param.Name,
                param.ParameterType,
                SymbolKind.Parameter,
                param.Location
            ));
        }
        
        // Process method body
        if (methodDecl.Body != null)
        {
            ProcessStatement(methodDecl.Body);
        }
        
        _currentScope = previousScope;
    }
    
    private void ProcessStatement(Statement statement)
    {
        switch (statement)
        {
            case BlockStatement blockStmt:
                ProcessBlock(blockStmt);
                break;
                
            case VariableDeclarationStatement varDecl:
                _currentScope.Define(varDecl.Name, new Symbol(
                    varDecl.Name,
                    varDecl.VariableType,
                    SymbolKind.Variable,
                    varDecl.Location
                ));
                break;
                
            case IfStatement ifStmt:
                ProcessStatement(ifStmt.ThenBranch);
                if (ifStmt.ElseBranch != null)
                {
                    ProcessStatement(ifStmt.ElseBranch);
                }
                break;
                
            case WhileStatement whileStmt:
                ProcessStatement(whileStmt.Body);
                break;
                
            case DoWhileStatement doWhileStmt:
                ProcessStatement(doWhileStmt.Body);
                break;
                
            case ForStatement forStmt:
                // Create new scope for for loop
                var forScope = _currentScope.CreateChildScope(ScopeType.Block, "for");
                var previousScope = _currentScope;
                _currentScope = forScope;
                
                if (forStmt.Initializer != null)
                {
                    ProcessStatement(forStmt.Initializer);
                }
                ProcessStatement(forStmt.Body);
                
                _currentScope = previousScope;
                break;
                
            case SwitchStatement switchStmt:
                foreach (var caseStmt in switchStmt.Cases)
                {
                    foreach (var stmt in caseStmt.Statements)
                    {
                        ProcessStatement(stmt);
                    }
                }
                break;
                
            // Other statement types don't introduce new scopes or symbols
            case ExpressionStatement:
            case ReturnStatement:
            case BreakStatement:
            case ContinueStatement:
                break;
        }
    }
    
    private void ProcessBlock(BlockStatement block)
    {
        // Create new scope for block
        var blockScope = _currentScope.CreateChildScope(ScopeType.Block, "block");
        var previousScope = _currentScope;
        _currentScope = blockScope;
        
        foreach (var statement in block.Statements)
        {
            ProcessStatement(statement);
        }
        
        _currentScope = previousScope;
    }
    
    private SymbolTable GetRootScope()
    {
        var root = _currentScope;
        while (root.Parent != null)
        {
            root = root.Parent;
        }
        return root;
    }
}
