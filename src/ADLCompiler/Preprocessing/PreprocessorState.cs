namespace ADLCompiler.Preprocessing;

/// <summary>
/// Represents the state of the preprocessor during processing
/// </summary>
public class PreprocessorState
{
    /// <summary>
    /// Map of defined macros (name -> definition)
    /// </summary>
    public Dictionary<string, MacroDefinition> Defines { get; set; } = new();
    
    /// <summary>
    /// Files that have been discovered and processed
    /// </summary>
    public List<string> DiscoveredFiles { get; set; } = new();
    
    /// <summary>
    /// Current file being processed
    /// </summary>
    public string CurrentFile { get; set; } = string.Empty;
    
    /// <summary>
    /// Current line number in the current file
    /// </summary>
    public int CurrentLine { get; set; } = 1;
    
    /// <summary>
    /// Stack of conditional compilation states (#ifdef, #ifndef)
    /// </summary>
    public Stack<ConditionalState> ConditionalStack { get; set; } = new();
    
    /// <summary>
    /// Whether we're currently in an active code block (not skipped by #ifdef)
    /// </summary>
    public bool IsActive => ConditionalStack.Count == 0 || ConditionalStack.All(s => s.IsActive);
}

/// <summary>
/// Represents a conditional compilation state
/// </summary>
public class ConditionalState
{
    /// <summary>
    /// Whether this conditional block is active (code should be included)
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// The directive that started this block (#ifdef, #ifndef)
    /// </summary>
    public string Directive { get; set; } = string.Empty;
    
    /// <summary>
    /// The symbol being tested
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
}
