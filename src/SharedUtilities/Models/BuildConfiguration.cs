namespace SharedUtilities.Models;

/// <summary>
/// Build configuration for compiling ADL projects
/// </summary>
public class BuildConfiguration
{
    public string OutputPath { get; set; } = "bin/output.apk";
    public List<string> CompilerFlags { get; set; } = new();
    public List<string> IncludeDirectories { get; set; } = new();
    public Dictionary<string, string> Defines { get; set; } = new();
    public OptimizationLevel Optimization { get; set; } = OptimizationLevel.Debug;
    public TargetPlatform Platform { get; set; } = TargetPlatform.Android;
    public PackageMode PackageMode { get; set; } = PackageMode.Standalone;
}

public enum OptimizationLevel
{
    Debug,
    Release,
    Optimized
}

public enum TargetPlatform
{
    Android,
    Windows,
    Linux,
    MacOS,
    All
}

public enum PackageMode
{
    Installer,
    Standalone
}
