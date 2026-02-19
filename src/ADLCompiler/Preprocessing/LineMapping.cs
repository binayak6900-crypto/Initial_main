namespace ADLCompiler.Preprocessing;

/// <summary>
/// Maps preprocessed source lines back to original source locations for debugging
/// </summary>
public class LineMapping
{
    private readonly List<LineMapEntry> _entries = new();
    
    /// <summary>
    /// Add a mapping entry
    /// </summary>
    public void AddMapping(int preprocessedLine, string originalFile, int originalLine)
    {
        _entries.Add(new LineMapEntry
        {
            PreprocessedLine = preprocessedLine,
            OriginalFile = originalFile,
            OriginalLine = originalLine
        });
    }
    
    /// <summary>
    /// Get the original source location for a preprocessed line
    /// </summary>
    public (string file, int line) GetOriginalLocation(int preprocessedLine)
    {
        // Find the closest mapping entry
        var entry = _entries
            .Where(e => e.PreprocessedLine <= preprocessedLine)
            .OrderByDescending(e => e.PreprocessedLine)
            .FirstOrDefault();
        
        if (entry == null)
        {
            return ("unknown", preprocessedLine);
        }
        
        // Calculate offset from the mapping entry
        int offset = preprocessedLine - entry.PreprocessedLine;
        return (entry.OriginalFile, entry.OriginalLine + offset);
    }
    
    /// <summary>
    /// Get all mappings
    /// </summary>
    public IReadOnlyList<LineMapEntry> GetMappings() => _entries.AsReadOnly();
}

/// <summary>
/// Represents a single line mapping entry
/// </summary>
public class LineMapEntry
{
    public int PreprocessedLine { get; set; }
    public string OriginalFile { get; set; } = string.Empty;
    public int OriginalLine { get; set; }
}
