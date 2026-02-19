using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Analyzes dependencies between ADL files and determines compilation order.
/// Automatically discovers all files in a project and builds a dependency graph.
/// Requirements: 5.1, 5.3, 5.6, 5.7
/// </summary>
public class DependencyAnalyzer
{
    private readonly string _projectDirectory;
    private readonly Dictionary<string, CompilationUnit> _parsedFiles;
    private readonly Dictionary<string, HashSet<string>> _dependencyGraph;
    private readonly Dictionary<string, HashSet<string>> _definedSymbols;
    
    public DependencyAnalyzer(string projectDirectory)
    {
        _projectDirectory = projectDirectory;
        _parsedFiles = new Dictionary<string, CompilationUnit>();
        _dependencyGraph = new Dictionary<string, HashSet<string>>();
        _definedSymbols = new Dictionary<string, HashSet<string>>();
    }
    
    /// <summary>
    /// Discovers all .adl files in the project directory and analyzes their dependencies.
    /// Returns the files in compilation order (dependencies first).
    /// </summary>
    /// <param name="parseFile">Function to parse a file and return its AST</param>
    /// <returns>List of file paths in compilation order</returns>
    /// <exception cref="CircularDependencyException">Thrown when circular dependencies are detected</exception>
    public List<string> AnalyzeDependencies(Func<string, CompilationUnit> parseFile)
    {
        // Phase 1: Discover all .adl files
        var allFiles = DiscoverAdlFiles();
        
        if (allFiles.Count == 0)
        {
            return new List<string>();
        }
        
        // Phase 2: Parse all files and build symbol index
        foreach (var file in allFiles)
        {
            var ast = parseFile(file);
            _parsedFiles[file] = ast;
            IndexDefinedSymbols(file, ast);
        }
        
        // Phase 3: Analyze references and build dependency graph
        foreach (var file in allFiles)
        {
            var ast = _parsedFiles[file];
            var references = ExtractReferences(ast);
            _dependencyGraph[file] = new HashSet<string>();
            
            foreach (var reference in references)
            {
                var definingFile = FindDefiningFile(reference, file);
                if (definingFile != null && definingFile != file)
                {
                    _dependencyGraph[file].Add(definingFile);
                }
            }
        }
        
        // Phase 4: Topological sort to determine compilation order
        return TopologicalSort(allFiles);
    }
    
    /// <summary>
    /// Discovers all .adl files in the project directory recursively
    /// </summary>
    private List<string> DiscoverAdlFiles()
    {
        var files = new List<string>();
        
        if (!Directory.Exists(_projectDirectory))
        {
            return files;
        }
        
        try
        {
            var adlFiles = Directory.GetFiles(_projectDirectory, "*.adl", SearchOption.AllDirectories);
            files.AddRange(adlFiles);
        }
        catch (Exception ex)
        {
            throw new DependencyAnalysisException($"Failed to discover .adl files: {ex.Message}");
        }
        
        return files;
    }
    
    /// <summary>
    /// Indexes all symbols (classes, namespaces) defined in a file
    /// </summary>
    private void IndexDefinedSymbols(string file, CompilationUnit ast)
    {
        var symbols = new HashSet<string>();
        
        // Index namespace
        if (ast.Namespace != null)
        {
            symbols.Add(ast.Namespace.Name);
            
            // Index classes in namespace
            foreach (var cls in ast.Namespace.Classes)
            {
                symbols.Add($"{ast.Namespace.Name}.{cls.Name}");
                symbols.Add(cls.Name); // Also add unqualified name
            }
            
            // Index namespace functions
            foreach (var func in ast.Namespace.Functions)
            {
                symbols.Add($"{ast.Namespace.Name}.{func.Name}");
            }
        }
        
        // Index package
        if (ast.PackageName != null)
        {
            symbols.Add(ast.PackageName);
        }
        
        // Index top-level classes
        foreach (var cls in ast.Classes)
        {
            if (ast.PackageName != null)
            {
                symbols.Add($"{ast.PackageName}.{cls.Name}");
            }
            symbols.Add(cls.Name);
        }
        
        _definedSymbols[file] = symbols;
    }
    
    /// <summary>
    /// Extracts all class and namespace references from an AST
    /// </summary>
    private HashSet<string> ExtractReferences(CompilationUnit ast)
    {
        var references = new HashSet<string>();
        var visitor = new ReferenceExtractor(references);
        
        // Visit namespace
        if (ast.Namespace != null)
        {
            foreach (var cls in ast.Namespace.Classes)
            {
                visitor.VisitClass(cls);
            }
            
            foreach (var func in ast.Namespace.Functions)
            {
                visitor.VisitMethod(func);
            }
        }
        
        // Visit top-level classes
        foreach (var cls in ast.Classes)
        {
            visitor.VisitClass(cls);
        }
        
        return references;
    }
    
    /// <summary>
    /// Finds the file that defines a given symbol
    /// </summary>
    private string? FindDefiningFile(string symbol, string currentFile)
    {
        foreach (var kvp in _definedSymbols)
        {
            if (kvp.Key == currentFile)
                continue;
                
            if (kvp.Value.Contains(symbol))
            {
                return kvp.Key;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Performs topological sort on the dependency graph to determine compilation order.
    /// Detects circular dependencies.
    /// </summary>
    private List<string> TopologicalSort(List<string> files)
    {
        var result = new List<string>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();
        
        foreach (var file in files)
        {
            if (!visited.Contains(file))
            {
                Visit(file, visited, visiting, result);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// DFS visit for topological sort
    /// </summary>
    private void Visit(string file, HashSet<string> visited, HashSet<string> visiting, List<string> result)
    {
        if (visiting.Contains(file))
        {
            // Circular dependency detected
            var cycle = BuildCyclePath(file, visiting);
            throw new CircularDependencyException($"Circular dependency detected: {cycle}");
        }
        
        if (visited.Contains(file))
        {
            return;
        }
        
        visiting.Add(file);
        
        // Visit dependencies first
        if (_dependencyGraph.TryGetValue(file, out var dependencies))
        {
            foreach (var dependency in dependencies)
            {
                Visit(dependency, visited, visiting, result);
            }
        }
        
        visiting.Remove(file);
        visited.Add(file);
        result.Add(file);
    }
    
    /// <summary>
    /// Builds a readable path showing the circular dependency
    /// </summary>
    private string BuildCyclePath(string file, HashSet<string> visiting)
    {
        var cycle = new List<string> { file };
        
        // Try to build the cycle path
        var current = file;
        if (_dependencyGraph.TryGetValue(current, out var deps))
        {
            foreach (var dep in deps)
            {
                if (visiting.Contains(dep))
                {
                    cycle.Add(dep);
                    break;
                }
            }
        }
        
        return string.Join(" -> ", cycle.Select(Path.GetFileName));
    }
    
    /// <summary>
    /// Gets the dependency graph for debugging/visualization
    /// </summary>
    public IReadOnlyDictionary<string, HashSet<string>> GetDependencyGraph()
    {
        return _dependencyGraph;
    }
    
    /// <summary>
    /// Gets all symbols defined in each file
    /// </summary>
    public IReadOnlyDictionary<string, HashSet<string>> GetDefinedSymbols()
    {
        return _definedSymbols;
    }
}

/// <summary>
/// Visitor that extracts all type references from an AST
/// </summary>
internal class ReferenceExtractor
{
    private readonly HashSet<string> _references;
    
    public ReferenceExtractor(HashSet<string> references)
    {
        _references = references;
    }
    
    public void VisitClass(ClassDeclaration cls)
    {
        // Add superclass reference
        if (cls.SuperClass != null)
        {
            _references.Add(cls.SuperClass);
        }
        
        // Add interface references
        foreach (var iface in cls.Interfaces)
        {
            _references.Add(iface);
        }
        
        // Visit fields
        foreach (var field in cls.Fields)
        {
            VisitTypeReference(field.FieldType);
            if (field.Initializer != null)
            {
                VisitExpression(field.Initializer);
            }
        }
        
        // Visit methods
        foreach (var method in cls.Methods)
        {
            VisitMethod(method);
        }
    }
    
    public void VisitMethod(MethodDeclaration method)
    {
        // Visit return type
        VisitTypeReference(method.ReturnType);
        
        // Visit parameters
        foreach (var param in method.Parameters)
        {
            VisitTypeReference(param.ParameterType);
        }
        
        // Visit body
        if (method.Body != null)
        {
            VisitStatement(method.Body);
        }
    }
    
    private void VisitTypeReference(TypeReference typeRef)
    {
        // Add the type name as a reference
        _references.Add(typeRef.Name);
    }
    
    private void VisitStatement(Statement stmt)
    {
        switch (stmt)
        {
            case BlockStatement block:
                foreach (var s in block.Statements)
                {
                    VisitStatement(s);
                }
                break;
                
            case ExpressionStatement exprStmt:
                VisitExpression(exprStmt.Expression);
                break;
                
            case ReturnStatement ret:
                if (ret.Value != null)
                {
                    VisitExpression(ret.Value);
                }
                break;
                
            case IfStatement ifStmt:
                VisitExpression(ifStmt.Condition);
                VisitStatement(ifStmt.ThenBranch);
                if (ifStmt.ElseBranch != null)
                {
                    VisitStatement(ifStmt.ElseBranch);
                }
                break;
                
            case WhileStatement whileStmt:
                VisitExpression(whileStmt.Condition);
                VisitStatement(whileStmt.Body);
                break;
                
            case ForStatement forStmt:
                if (forStmt.Initializer != null)
                {
                    VisitStatement(forStmt.Initializer);
                }
                if (forStmt.Condition != null)
                {
                    VisitExpression(forStmt.Condition);
                }
                if (forStmt.Increment != null)
                {
                    VisitExpression(forStmt.Increment);
                }
                VisitStatement(forStmt.Body);
                break;
                
            case DoWhileStatement doWhile:
                VisitStatement(doWhile.Body);
                VisitExpression(doWhile.Condition);
                break;
                
            case SwitchStatement switchStmt:
                VisitExpression(switchStmt.Expression);
                foreach (var caseStmt in switchStmt.Cases)
                {
                    if (caseStmt.Value != null)
                    {
                        VisitExpression(caseStmt.Value);
                    }
                    foreach (var s in caseStmt.Statements)
                    {
                        VisitStatement(s);
                    }
                }
                break;
                
            case VariableDeclarationStatement varDecl:
                VisitTypeReference(varDecl.VariableType);
                if (varDecl.Initializer != null)
                {
                    VisitExpression(varDecl.Initializer);
                }
                break;
        }
    }
    
    private void VisitExpression(Expression expr)
    {
        switch (expr)
        {
            case BinaryExpression binary:
                VisitExpression(binary.Left);
                VisitExpression(binary.Right);
                break;
                
            case UnaryExpression unary:
                VisitExpression(unary.Operand);
                break;
                
            case MethodCallExpression methodCall:
                if (methodCall.Target != null)
                {
                    VisitExpression(methodCall.Target);
                }
                foreach (var arg in methodCall.Arguments)
                {
                    VisitExpression(arg);
                }
                break;
                
            case MemberAccessExpression memberAccess:
                VisitExpression(memberAccess.Target);
                break;
                
            case AssignmentExpression assignment:
                VisitExpression(assignment.Target);
                VisitExpression(assignment.Value);
                break;
                
            case ArrayAccessExpression arrayAccess:
                VisitExpression(arrayAccess.Array);
                VisitExpression(arrayAccess.Index);
                break;
                
            case NewExpression newExpr:
                VisitTypeReference(newExpr.TypeToCreate);
                foreach (var arg in newExpr.Arguments)
                {
                    VisitExpression(arg);
                }
                break;
                
            case CastExpression cast:
                VisitTypeReference(cast.TargetType);
                VisitExpression(cast.Expression);
                break;
                
            case TernaryExpression ternary:
                VisitExpression(ternary.Condition);
                VisitExpression(ternary.TrueExpression);
                VisitExpression(ternary.FalseExpression);
                break;
        }
    }
}

/// <summary>
/// Exception thrown when circular dependencies are detected
/// </summary>
public class CircularDependencyException : Exception
{
    public CircularDependencyException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown during dependency analysis
/// </summary>
public class DependencyAnalysisException : Exception
{
    public DependencyAnalysisException(string message) : base(message)
    {
    }
}
