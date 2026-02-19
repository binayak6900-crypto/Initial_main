using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Represents a symbol in the symbol table (variable, method, class, namespace, etc.)
/// </summary>
public class Symbol
{
    public string Name { get; set; }
    public TypeReference Type { get; set; }
    public SymbolKind Kind { get; set; }
    public SourceLocation Location { get; set; }
    
    public Symbol(string name, TypeReference type, SymbolKind kind, SourceLocation location)
    {
        Name = name;
        Type = type;
        Kind = kind;
        Location = location;
    }
    
    public override string ToString()
    {
        return $"{Kind} {Name}: {Type.Name}";
    }
}

/// <summary>
/// Types of symbols that can be stored in the symbol table
/// </summary>
public enum SymbolKind
{
    Variable,
    Parameter,
    Method,
    Field,
    Class,
    Namespace
}
