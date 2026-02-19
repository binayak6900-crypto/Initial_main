namespace ADLCompiler;

/// <summary>
/// Represents the parsed command-line options for the ADL compiler.
/// </summary>
public class CommandLineOptions
{
    /// <summary>
    /// Compile mode - full compilation to executable format.
    /// </summary>
    public bool Compile { get; set; }
    
    /// <summary>
    /// Preprocess mode - output preprocessed source code only.
    /// </summary>
    public bool PreprocessOnly { get; set; }
    
    /// <summary>
    /// Output file path.
    /// </summary>
    public string? OutputPath { get; set; }
    
    /// <summary>
    /// Target platform for compilation.
    /// </summary>
    public TargetPlatform Platform { get; set; } = TargetPlatform.Android;
    
    /// <summary>
    /// Package mode for APK generation.
    /// </summary>
    public PackageMode PackageMode { get; set; } = PackageMode.Standalone;
    
    /// <summary>
    /// Entry source file to compile.
    /// </summary>
    public string? SourceFile { get; set; }
    
    /// <summary>
    /// Target SDK version (defaults to latest).
    /// </summary>
    public int TargetSdk { get; set; } = 35; // Android 15.0
    
    /// <summary>
    /// Minimum SDK version (defaults to API 23 - Android 6.0).
    /// </summary>
    public int MinSdk { get; set; } = 23;
    
    /// <summary>
    /// Optimization level (0-3, defaults to 2).
    /// </summary>
    public int OptimizationLevel { get; set; } = 2;
    
    /// <summary>
    /// Show help message.
    /// </summary>
    public bool ShowHelp { get; set; }
    
    /// <summary>
    /// Show version information.
    /// </summary>
    public bool ShowVersion { get; set; }
}

/// <summary>
/// Target platform for compilation.
/// </summary>
public enum TargetPlatform
{
    Android,
    Windows,
    Linux,
    MacOS,
    All
}

/// <summary>
/// APK packaging mode.
/// </summary>
public enum PackageMode
{
    /// <summary>
    /// Installer APK that installs AVM runtime, compiler, and IDE.
    /// </summary>
    Installer,
    
    /// <summary>
    /// Standalone APK with no external dependencies.
    /// </summary>
    Standalone
}
