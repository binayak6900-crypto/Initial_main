using Xunit;
using ADLCompiler;

namespace ADLCompiler.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_WithCompileFlag_SetsCompileMode()
    {
        // Arrange
        string[] args = { "-C", "Main.adl" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.True(options.Compile);
        Assert.Equal("Main.adl", options.SourceFile);
    }
    
    [Fact]
    public void Parse_WithPreprocessFlag_SetsPreprocessMode()
    {
        // Arrange
        string[] args = { "-E", "Main.adl" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.True(options.PreprocessOnly);
        Assert.Equal("Main.adl", options.SourceFile);
    }
    
    [Fact]
    public void Parse_WithOutputFlag_SetsOutputPath()
    {
        // Arrange
        string[] args = { "-C", "-o", "output.apk", "Main.adl" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.Equal("output.apk", options.OutputPath);
    }
    
    [Fact]
    public void Parse_WithPlatformFlag_SetsPlatform()
    {
        // Arrange
        string[] args = { "-C", "--platform", "windows", "Main.adl" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.Equal(TargetPlatform.Windows, options.Platform);
    }
    
    [Fact]
    public void Parse_WithAllPlatforms_SetsAllPlatform()
    {
        // Arrange
        string[] args = { "-C", "--platform", "all", "Main.adl" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.Equal(TargetPlatform.All, options.Platform);
    }
    
    [Fact]
    public void Parse_WithPackageModeFlag_SetsPackageMode()
    {
        // Arrange
        string[] args = { "-C", "--package-mode", "installer", "Main.adl" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.Equal(PackageMode.Installer, options.PackageMode);
    }
    
    [Fact]
    public void Parse_WithTargetSdkFlag_SetsTargetSdk()
    {
        // Arrange
        string[] args = { "-C", "--target-sdk", "33", "Main.adl" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.Equal(33, options.TargetSdk);
    }
    
    [Fact]
    public void Parse_WithMinSdkFlag_SetsMinSdk()
    {
        // Arrange
        string[] args = { "-C", "--min-sdk", "28", "Main.adl" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.Equal(28, options.MinSdk);
    }
    
    [Fact]
    public void Parse_WithOptimizationFlags_SetsOptimizationLevel()
    {
        // Arrange & Act
        var options0 = CommandLineParser.Parse(new[] { "-O0", "Main.adl" });
        var options1 = CommandLineParser.Parse(new[] { "-O1", "Main.adl" });
        var options2 = CommandLineParser.Parse(new[] { "-O2", "Main.adl" });
        var options3 = CommandLineParser.Parse(new[] { "-O3", "Main.adl" });
        
        // Assert
        Assert.Equal(0, options0.OptimizationLevel);
        Assert.Equal(1, options1.OptimizationLevel);
        Assert.Equal(2, options2.OptimizationLevel);
        Assert.Equal(3, options3.OptimizationLevel);
    }
    
    [Fact]
    public void Parse_WithHelpFlag_SetsShowHelp()
    {
        // Arrange
        string[] args = { "--help" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.True(options.ShowHelp);
    }
    
    [Fact]
    public void Parse_WithVersionFlag_SetsShowVersion()
    {
        // Arrange
        string[] args = { "--version" };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.True(options.ShowVersion);
    }
    
    [Fact]
    public void Parse_WithUnknownFlag_ThrowsException()
    {
        // Arrange
        string[] args = { "--unknown-flag", "Main.adl" };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse(args));
    }
    
    [Fact]
    public void Parse_WithMultipleSourceFiles_ThrowsException()
    {
        // Arrange
        string[] args = { "Main.adl", "Other.adl" };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse(args));
    }
    
    [Fact]
    public void Parse_WithMissingOutputPath_ThrowsException()
    {
        // Arrange
        string[] args = { "-o" };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse(args));
    }
    
    [Fact]
    public void ValidateAndApplyDefaults_WithNoMode_DefaultsToCompile()
    {
        // Arrange
        var options = new CommandLineOptions { SourceFile = "Main.adl" };
        
        // Act
        CommandLineParser.ValidateAndApplyDefaults(options);
        
        // Assert
        Assert.True(options.Compile);
    }
    
    [Fact]
    public void ValidateAndApplyDefaults_WithNoOutputPath_GeneratesDefault()
    {
        // Arrange
        var options = new CommandLineOptions 
        { 
            Compile = true,
            SourceFile = "Main.adl",
            Platform = TargetPlatform.Android
        };
        
        // Act
        CommandLineParser.ValidateAndApplyDefaults(options);
        
        // Assert
        Assert.Equal("Main.apk", options.OutputPath);
    }
    
    [Fact]
    public void ValidateAndApplyDefaults_WithPreprocessMode_GeneratesPreprocessedOutput()
    {
        // Arrange
        var options = new CommandLineOptions 
        { 
            PreprocessOnly = true,
            SourceFile = "Main.adl"
        };
        
        // Act
        CommandLineParser.ValidateAndApplyDefaults(options);
        
        // Assert
        Assert.Equal("Main.i", options.OutputPath);
    }
    
    [Fact]
    public void ValidateAndApplyDefaults_WithWindowsPlatform_GeneratesExeOutput()
    {
        // Arrange
        var options = new CommandLineOptions 
        { 
            Compile = true,
            SourceFile = "Main.adl",
            Platform = TargetPlatform.Windows
        };
        
        // Act
        CommandLineParser.ValidateAndApplyDefaults(options);
        
        // Assert
        Assert.Equal("Main.exe", options.OutputPath);
    }
    
    [Fact]
    public void ValidateAndApplyDefaults_WithLinuxPlatform_GeneratesNativeOutput()
    {
        // Arrange
        var options = new CommandLineOptions 
        { 
            Compile = true,
            SourceFile = "Main.adl",
            Platform = TargetPlatform.Linux
        };
        
        // Act
        CommandLineParser.ValidateAndApplyDefaults(options);
        
        // Assert
        Assert.Equal("Main", options.OutputPath);
    }
    
    [Fact]
    public void ValidateAndApplyDefaults_WithNoSourceFile_ThrowsException()
    {
        // Arrange
        var options = new CommandLineOptions { Compile = true };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommandLineParser.ValidateAndApplyDefaults(options));
    }
    
    [Fact]
    public void ValidateAndApplyDefaults_WithInvalidMinSdk_ThrowsException()
    {
        // Arrange
        var options = new CommandLineOptions 
        { 
            Compile = true,
            SourceFile = "Main.adl",
            MinSdk = 22
        };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommandLineParser.ValidateAndApplyDefaults(options));
    }
    
    [Fact]
    public void ValidateAndApplyDefaults_WithTargetSdkLowerThanMinSdk_ThrowsException()
    {
        // Arrange
        var options = new CommandLineOptions 
        { 
            Compile = true,
            SourceFile = "Main.adl",
            MinSdk = 30,
            TargetSdk = 28
        };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommandLineParser.ValidateAndApplyDefaults(options));
    }
    
    [Fact]
    public void Parse_ComplexCommandLine_ParsesAllOptions()
    {
        // Arrange
        string[] args = 
        { 
            "-C", 
            "-o", "MyApp.apk",
            "--platform", "android",
            "--package-mode", "standalone",
            "--target-sdk", "33",
            "--min-sdk", "28",
            "-O3",
            "Main.adl"
        };
        
        // Act
        var options = CommandLineParser.Parse(args);
        
        // Assert
        Assert.True(options.Compile);
        Assert.Equal("MyApp.apk", options.OutputPath);
        Assert.Equal(TargetPlatform.Android, options.Platform);
        Assert.Equal(PackageMode.Standalone, options.PackageMode);
        Assert.Equal(33, options.TargetSdk);
        Assert.Equal(28, options.MinSdk);
        Assert.Equal(3, options.OptimizationLevel);
        Assert.Equal("Main.adl", options.SourceFile);
    }
}
