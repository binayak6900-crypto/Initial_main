using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Symbol table for managing scopes and symbol resolution.
/// Supports nested scopes with parent-child relationships for blocks, methods, classes, and namespaces.
/// Requirements: 4.2, 4.4, 4.5
/// </summary>
public class SymbolTable
{
    private readonly Dictionary<string, Symbol> _symbols;
    private readonly SymbolTable? _parent;
    private readonly ScopeType _scopeType;
    private readonly string _scopeName;
    
    /// <summary>
    /// Gets the parent symbol table (null for global scope)
    /// </summary>
    public SymbolTable? Parent => _parent;
    
    /// <summary>
    /// Gets the type of this scope
    /// </summary>
    public ScopeType ScopeType => _scopeType;
    
    /// <summary>
    /// Gets the name of this scope (e.g., class name, method name)
    /// </summary>
    public string ScopeName => _scopeName;
    
    /// <summary>
    /// Gets all symbols defined in this scope (not including parent scopes)
    /// </summary>
    public IReadOnlyDictionary<string, Symbol> Symbols => _symbols;
    
    /// <summary>
    /// Creates a new symbol table with an optional parent scope
    /// </summary>
    /// <param name="parent">Parent symbol table for scope chain traversal</param>
    /// <param name="scopeType">Type of scope this table represents</param>
    /// <param name="scopeName">Name of the scope (for debugging and error messages)</param>
    public SymbolTable(SymbolTable? parent = null, ScopeType scopeType = ScopeType.Global, string scopeName = "global")
    {
        _symbols = new Dictionary<string, Symbol>();
        _parent = parent;
        _scopeType = scopeType;
        _scopeName = scopeName;
    }
    
    /// <summary>
    /// Defines a new symbol in this scope.
    /// Throws an exception if a symbol with the same name already exists in this scope.
    /// </summary>
    /// <param name="name">Symbol name</param>
    /// <param name="symbol">Symbol to define</param>
    /// <exception cref="SemanticException">Thrown when symbol is already defined in this scope</exception>
    public void Define(string name, Symbol symbol)
    {
        if (_symbols.ContainsKey(name))
        {
            var existing = _symbols[name];
            throw new SemanticException(
                $"Symbol '{name}' is already defined in this scope at {existing.Location.Line}:{existing.Location.Column}",
                symbol.Location
            );
        }
        
        _symbols[name] = symbol;
    }
    
    /// <summary>
    /// Looks up a symbol by name, traversing the scope chain from this scope to parent scopes.
    /// Returns null if the symbol is not found in any scope.
    /// </summary>
    /// <param name="name">Symbol name to look up</param>
    /// <returns>Symbol if found, null otherwise</returns>
    public Symbol? Lookup(string name)
    {
        // First check this scope
        if (_symbols.TryGetValue(name, out var symbol))
        {
            return symbol;
        }
        
        // If not found, check parent scope recursively
        return _parent?.Lookup(name);
    }
    
    /// <summary>
    /// Looks up a symbol only in this scope (does not traverse parent scopes).
    /// Returns null if the symbol is not found in this scope.
    /// </summary>
    /// <param name="name">Symbol name to look up</param>
    /// <returns>Symbol if found in this scope, null otherwise</returns>
    public Symbol? LookupLocal(string name)
    {
        return _symbols.TryGetValue(name, out var symbol) ? symbol : null;
    }
    
    /// <summary>
    /// Checks if a symbol is defined in this scope (does not check parent scopes)
    /// </summary>
    /// <param name="name">Symbol name to check</param>
    /// <returns>True if symbol exists in this scope, false otherwise</returns>
    public bool IsDefined(string name)
    {
        return _symbols.ContainsKey(name);
    }
    
    /// <summary>
    /// Checks if a symbol exists in this scope or any parent scope
    /// </summary>
    /// <param name="name">Symbol name to check</param>
    /// <returns>True if symbol exists in scope chain, false otherwise</returns>
    public bool Exists(string name)
    {
        return Lookup(name) != null;
    }
    
    /// <summary>
    /// Creates a new child scope with this scope as the parent
    /// </summary>
    /// <param name="scopeType">Type of the child scope</param>
    /// <param name="scopeName">Name of the child scope</param>
    /// <returns>New child symbol table</returns>
    public SymbolTable CreateChildScope(ScopeType scopeType, string scopeName)
    {
        return new SymbolTable(this, scopeType, scopeName);
    }
    
    /// <summary>
    /// Gets the depth of this scope in the scope chain (0 for global scope)
    /// </summary>
    public int GetDepth()
    {
        int depth = 0;
        var current = _parent;
        while (current != null)
        {
            depth++;
            current = current._parent;
        }
        return depth;
    }
    
    /// <summary>
    /// Gets all symbols in the scope chain (this scope and all parent scopes)
    /// </summary>
    /// <returns>Dictionary of all accessible symbols</returns>
    public Dictionary<string, Symbol> GetAllAccessibleSymbols()
    {
        var result = new Dictionary<string, Symbol>();
        
        // Start from the root and work down to avoid overwriting with parent symbols
        var scopes = new List<SymbolTable>();
        var current = this;
        while (current != null)
        {
            scopes.Add(current);
            current = current._parent;
        }
        
        // Reverse to go from root to current scope
        scopes.Reverse();
        
        // Add symbols, with inner scopes overriding outer scopes
        foreach (var scope in scopes)
        {
            foreach (var kvp in scope._symbols)
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        
        return result;
    }
    
    public override string ToString()
    {
        return $"{_scopeType} Scope '{_scopeName}' (depth: {GetDepth()}, symbols: {_symbols.Count})";
    }
}

/// <summary>
/// Types of scopes in the symbol table hierarchy
/// </summary>
public enum ScopeType
{
    /// <summary>Global scope (top-level)</summary>
    Global,
    
    /// <summary>Namespace scope (C++ style)</summary>
    Namespace,
    
    /// <summary>Class scope</summary>
    Class,
    
    /// <summary>Method/function scope</summary>
    Method,
    
    /// <summary>Block scope (if, while, for, etc.)</summary>
    Block
}

/// <summary>
/// Exception thrown during semantic analysis
/// </summary>
public class SemanticException : Exception
{
    public SourceLocation Location { get; }
    
    public SemanticException(string message, SourceLocation location) : base(message)
    {
        Location = location;
    }
}
