namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Represents a native function binding from the Android NDK
/// </summary>
public class NDKBinding
{
    public string FunctionName { get; set; } = string.Empty;
    public string NativeSymbol { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = string.Empty;
    public string Library { get; set; } = string.Empty; // e.g., libGLESv2.so, libOpenSLES.so
    public int MinSDK { get; set; }
    public string Category { get; set; } = string.Empty; // Graphics, Audio, Sensors, Input, Camera
    
    public NDKBinding() { }
    
    public NDKBinding(string functionName, string nativeSymbol, string returnType, string library, int minSDK = 23)
    {
        FunctionName = functionName;
        NativeSymbol = nativeSymbol;
        ReturnType = returnType;
        Library = library;
        MinSDK = minSDK;
    }
}

/// <summary>
/// Represents a native library with its symbols
/// </summary>
public class NativeLibrary
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Dictionary<string, NDKBinding> Symbols { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    
    public NativeLibrary() { }
    
    public NativeLibrary(string name, string path)
    {
        Name = name;
        Path = path;
    }
}
