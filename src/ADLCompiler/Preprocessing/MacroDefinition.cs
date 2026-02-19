namespace ADLCompiler.Preprocessing;

/// <summary>
/// Represents a preprocessor macro definition
/// </summary>
public class MacroDefinition
{
    /// <summary>
    /// Name of the macro
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Parameters for function-like macros (null for object-like macros)
    /// </summary>
    public List<string>? Parameters { get; set; }
    
    /// <summary>
    /// Replacement text for the macro
    /// </summary>
    public string Replacement { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is a function-like macro (has parameters)
    /// </summary>
    public bool IsFunctionLike => Parameters != null;
    
    /// <summary>
    /// Expand the macro with given arguments
    /// </summary>
    public string Expand(List<string>? arguments = null)
    {
        if (!IsFunctionLike)
        {
            return Replacement;
        }
        
        if (arguments == null || Parameters == null)
        {
            throw new InvalidOperationException($"Function-like macro '{Name}' requires arguments");
        }
        
        if (arguments.Count != Parameters.Count)
        {
            throw new InvalidOperationException(
                $"Macro '{Name}' expects {Parameters.Count} arguments but got {arguments.Count}");
        }
        
        // Perform parameter substitution
        string result = Replacement;
        for (int i = 0; i < Parameters.Count; i++)
        {
            // Replace parameter with argument
            // Use word boundaries to avoid partial replacements
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                $@"\b{Parameters[i]}\b",
                arguments[i]);
        }
        
        return result;
    }
}
