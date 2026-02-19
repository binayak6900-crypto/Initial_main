namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Represents an Android SDK version bundle with API definitions
/// </summary>
public class SDKBundle
{
    public string Version { get; set; } = string.Empty; // "6.0", "7.0", etc.
    public int APILevel { get; set; } // 23, 24, etc.
    public List<APIDefinition> APIs { get; set; } = new();
    
    public SDKBundle() { }
    
    public SDKBundle(string version, int apiLevel)
    {
        Version = version;
        APILevel = apiLevel;
    }
    
    /// <summary>
    /// Lookup an API definition by class name
    /// </summary>
    public APIDefinition? LookupClass(string className)
    {
        return APIs.FirstOrDefault(api => api.ClassName == className);
    }
    
    /// <summary>
    /// Lookup a method in a specific class
    /// </summary>
    public MethodSignature? LookupMethod(string className, string methodName)
    {
        var apiDef = LookupClass(className);
        return apiDef?.Methods.FirstOrDefault(m => m.Name == methodName);
    }
    
    /// <summary>
    /// Lookup a field in a specific class
    /// </summary>
    public FieldSignature? LookupField(string className, string fieldName)
    {
        var apiDef = LookupClass(className);
        return apiDef?.Fields.FirstOrDefault(f => f.Name == fieldName);
    }
}
