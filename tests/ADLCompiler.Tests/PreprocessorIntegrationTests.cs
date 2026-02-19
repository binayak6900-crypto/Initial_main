using ADLCompiler.Preprocessing;
using Xunit;

namespace ADLCompiler.Tests;

public class PreprocessorIntegrationTests
{
    [Fact]
    public void Process_ComplexMacroExpansion_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define PI 3.14159
#define SQUARE(x) ((x) * (x))
#define CIRCLE_AREA(r) (PI * SQUARE(r))

double area = CIRCLE_AREA(5.0);";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("double area = (3.14159 * ((5.0) * (5.0)));", result.Source);
    }
    
    [Fact]
    public void Process_ConditionalCompilationWithMultiplePlatforms_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define PLATFORM_ANDROID

class App {
#ifdef PLATFORM_ANDROID
    void initAndroid() {
        setupTouchInput();
    }
#endif

#ifdef PLATFORM_WINDOWS
    void initWindows() {
        setupMouseInput();
    }
#endif

    void initCommon() {
        loadResources();
    }
}";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("void initAndroid()", result.Source);
        Assert.Contains("setupTouchInput();", result.Source);
        Assert.DoesNotContain("void initWindows()", result.Source);
        Assert.DoesNotContain("setupMouseInput();", result.Source);
        Assert.Contains("void initCommon()", result.Source);
        Assert.Contains("loadResources();", result.Source);
    }
    
    [Fact]
    public void Process_DebugAndReleaseBuilds_WorksCorrectly()
    {
        // Arrange - Debug build
        var preprocessor = new Preprocessor();
        string source = @"#define DEBUG

class Logger {
#ifdef DEBUG
    void log(String message) {
        System.out.println(message);
    }
#endif

#ifndef DEBUG
    void log(String message) {
        // No-op in release
    }
#endif
}";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("System.out.println(message);", result.Source);
        Assert.DoesNotContain("// No-op in release", result.Source);
    }
    
    [Fact]
    public void Process_MacroWithStringConcatenation_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define LOG_PREFIX ""[APP]""
#define LOG(msg) System.out.println(LOG_PREFIX + msg)

LOG(""Application started"");";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains(@"System.out.println(""[APP]"" + ""Application started"");", result.Source);
    }
    
    [Fact]
    public void Process_NestedMacrosWithConditionals_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define FEATURE_A
#define FEATURE_B

#ifdef FEATURE_A
#define CONFIG_A 1
#else
#define CONFIG_A 0
#endif

#ifdef FEATURE_B
#define CONFIG_B 1
#else
#define CONFIG_B 0
#endif

int configA = CONFIG_A;
int configB = CONFIG_B;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int configA = 1;", result.Source);
        Assert.Contains("int configB = 1;", result.Source);
    }
    
    [Fact]
    public void Process_GuardPattern_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#ifndef MYHEADER_H
#define MYHEADER_H

class MyClass {
    int value;
}

#endif";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("class MyClass", result.Source);
        Assert.Contains("int value;", result.Source);
    }
    
    [Fact]
    public void Process_ComplexExpressionMacro_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define MIN(a, b) ((a) < (b) ? (a) : (b))
#define MAX(a, b) ((a) > (b) ? (a) : (b))
#define CLAMP(x, min, max) (MIN(MAX(x, min), max))

int clamped = CLAMP(value, 0, 100);";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        // The macro should expand recursively:
        // CLAMP(value, 0, 100) -> (MIN(MAX(value, 0), 100))
        // -> (MIN(((value) > (0) ? (value) : (0)), 100))
        // -> (((((value) > (0) ? (value) : (0))) < (100) ? (((value) > (0) ? (value) : (0))) : (100)))
        Assert.Contains("int clamped = (((((value) > (0) ? (value) : (0))) < (100) ? (((value) > (0) ? (value) : (0))) : (100)));", result.Source);
    }
    
    [Fact]
    public void Process_RealWorldExample_GameConfiguration_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define GAME_VERSION ""1.0.0""
#define MAX_PLAYERS 4
#define SCREEN_WIDTH 1920
#define SCREEN_HEIGHT 1080
#define FPS 60

#define DEBUG_MODE

class GameConfig {
    String version = GAME_VERSION;
    int maxPlayers = MAX_PLAYERS;
    int screenWidth = SCREEN_WIDTH;
    int screenHeight = SCREEN_HEIGHT;
    int targetFPS = FPS;
    
#ifdef DEBUG_MODE
    boolean debugEnabled = true;
    void enableDebugOverlay() {
        showFPS();
        showMemoryUsage();
    }
#endif

#ifndef DEBUG_MODE
    boolean debugEnabled = false;
#endif
}";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains(@"String version = ""1.0.0"";", result.Source);
        Assert.Contains("int maxPlayers = 4;", result.Source);
        Assert.Contains("int screenWidth = 1920;", result.Source);
        Assert.Contains("int screenHeight = 1080;", result.Source);
        Assert.Contains("int targetFPS = 60;", result.Source);
        Assert.Contains("boolean debugEnabled = true;", result.Source);
        Assert.Contains("void enableDebugOverlay()", result.Source);
        Assert.DoesNotContain("boolean debugEnabled = false;", result.Source);
    }
    
    [Fact]
    public void Process_LineMappingWithConditionals_PreservesCorrectLineNumbers()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"int line1 = 1;
#define DEBUG
#ifdef DEBUG
int line4 = 4;
#endif
int line6 = 6;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        // First line of output (line1) maps to line 1 of input
        var (file1, line1) = result.LineMapping.GetOriginalLocation(1);
        Assert.Equal("test.adl", file1);
        Assert.Equal(1, line1);
        
        // Second line of output (line4) maps to line 4 of input
        var (file2, line2) = result.LineMapping.GetOriginalLocation(2);
        Assert.Equal("test.adl", file2);
        Assert.Equal(4, line2);
        
        // Third line of output (line6) maps to line 6 of input
        var (file3, line3) = result.LineMapping.GetOriginalLocation(3);
        Assert.Equal("test.adl", file3);
        Assert.Equal(6, line3);
    }
}
