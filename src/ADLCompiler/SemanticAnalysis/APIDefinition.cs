namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Represents a method signature in an Android API
/// </summary>
public class MethodSignature
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public bool IsStatic { get; set; }
    public string AccessModifier { get; set; } = "public"; // public, private, protected
    public int MinSDK { get; set; }
    public bool Deprecated { get; set; }
    
    public MethodSignature() { }
    
    public MethodSignature(string name, string returnType, int minSDK = 23)
    {
        Name = name;
        ReturnType = returnType;
        MinSDK = minSDK;
    }
}

/// <summary>
/// Represents a parameter in a method signature
/// </summary>
public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    
    public ParameterInfo() { }
    
    public ParameterInfo(string name, string type)
    {
        Name = name;
        Type = type;
    }
}

/// <summary>
/// Represents a field signature in an Android API
/// </summary>
public class FieldSignature
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public string AccessModifier { get; set; } = "public";
    public int MinSDK { get; set; }
    public bool Deprecated { get; set; }
    
    public FieldSignature() { }
    
    public FieldSignature(string name, string type, int minSDK = 23)
    {
        Name = name;
        Type = type;
        MinSDK = minSDK;
    }
}

/// <summary>
/// Represents an Android API class definition with its methods and fields
/// </summary>
public class APIDefinition
{
    public string ClassName { get; set; } = string.Empty;
    public List<MethodSignature> Methods { get; set; } = new();
    public List<FieldSignature> Fields { get; set; } = new();
    public int MinSDK { get; set; }
    public bool Deprecated { get; set; }
    
    public APIDefinition() { }
    
    public APIDefinition(string className, int minSDK = 23)
    {
        ClassName = className;
        MinSDK = minSDK;
    }
}
