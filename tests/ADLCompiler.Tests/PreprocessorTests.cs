using ADLCompiler.Preprocessing;
using Xunit;

namespace ADLCompiler.Tests;

public class PreprocessorTests
{
    [Fact]
    public void Process_SimpleDefine_ExpandsMacro()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define MAX 100
int value = MAX;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int value = 100;", result.Source);
        Assert.DoesNotContain("#define", result.Source);
    }
    
    [Fact]
    public void Process_FunctionLikeMacro_ExpandsWithParameters()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define ADD(a, b) ((a) + (b))
int result = ADD(5, 10);";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int result = ((5) + (10));", result.Source);
    }
    
    [Fact]
    public void Process_IfdefDefined_IncludesCode()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define DEBUG
#ifdef DEBUG
int debugMode = 1;
#endif
int normalCode = 2;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int debugMode = 1;", result.Source);
        Assert.Contains("int normalCode = 2;", result.Source);
    }
    
    [Fact]
    public void Process_IfdefNotDefined_ExcludesCode()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#ifdef DEBUG
int debugMode = 1;
#endif
int normalCode = 2;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.DoesNotContain("int debugMode = 1;", result.Source);
        Assert.Contains("int normalCode = 2;", result.Source);
    }
    
    [Fact]
    public void Process_IfndefNotDefined_IncludesCode()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#ifndef RELEASE
int debugMode = 1;
#endif
int normalCode = 2;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int debugMode = 1;", result.Source);
        Assert.Contains("int normalCode = 2;", result.Source);
    }
    
    [Fact]
    public void Process_IfndefDefined_ExcludesCode()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define RELEASE
#ifndef RELEASE
int debugMode = 1;
#endif
int normalCode = 2;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.DoesNotContain("int debugMode = 1;", result.Source);
        Assert.Contains("int normalCode = 2;", result.Source);
    }
    
    [Fact]
    public void Process_NestedConditionals_WorksCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define OUTER
#ifdef OUTER
int outer = 1;
#ifdef INNER
int inner = 2;
#endif
int afterInner = 3;
#endif
int afterOuter = 4;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int outer = 1;", result.Source);
        Assert.DoesNotContain("int inner = 2;", result.Source);
        Assert.Contains("int afterInner = 3;", result.Source);
        Assert.Contains("int afterOuter = 4;", result.Source);
    }
    
    [Fact]
    public void Process_MacroWithMultipleParameters_ExpandsCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define MAX(a, b) ((a) > (b) ? (a) : (b))
int max = MAX(x, y);";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int max = ((x) > (y) ? (x) : (y));", result.Source);
    }
    
    [Fact]
    public void Process_RecursiveMacroExpansion_ExpandsMultipleLevels()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define A B
#define B C
#define C 42
int value = A;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int value = 42;", result.Source);
    }
    
    [Fact]
    public void Process_UnclosedIfdef_ThrowsException()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#ifdef DEBUG
int debugMode = 1;";
        
        // Act & Assert
        var ex = Assert.Throws<PreprocessorException>(() => 
            preprocessor.Process(source, "test.adl"));
        Assert.Contains("Unclosed #ifdef", ex.Message);
    }
    
    [Fact]
    public void Process_EndifWithoutIfdef_ThrowsException()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"int value = 1;
#endif";
        
        // Act & Assert
        var ex = Assert.Throws<PreprocessorException>(() => 
            preprocessor.Process(source, "test.adl"));
        Assert.Contains("#endif without matching", ex.Message);
    }
    
    [Fact]
    public void Process_MacroWithNestedParentheses_ParsesArgumentsCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define CALL(f, x) f(x)
int result = CALL(func, (a + b));";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int result = func((a + b));", result.Source);
    }
    
    [Fact]
    public void LineMapping_PreservesOriginalLineNumbers()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"int line1 = 1;
#define MAX 100
int line3 = MAX;
int line4 = 4;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        var (file, line) = result.LineMapping.GetOriginalLocation(1);
        Assert.Equal("test.adl", file);
        Assert.Equal(1, line);
        
        // Line 2 in output corresponds to line 3 in original (line 2 was #define)
        (file, line) = result.LineMapping.GetOriginalLocation(2);
        Assert.Equal("test.adl", file);
        Assert.Equal(3, line);
    }
    
    [Fact]
    public void Process_EmptySource_ReturnsEmptyResult()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = "";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Empty(result.Source.Trim());
    }
    
    [Fact]
    public void Process_OnlyDirectives_ReturnsEmptyResult()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define MAX 100
#define MIN 0";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Empty(result.Source.Trim());
    }
    
    [Fact]
    public void Process_MacroNotUsed_DoesNotAppearInOutput()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define UNUSED 999
int value = 42;";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.DoesNotContain("UNUSED", result.Source);
        Assert.DoesNotContain("999", result.Source);
        Assert.Contains("int value = 42;", result.Source);
    }
    
    [Fact]
    public void Process_PlatformDefines_WorkCorrectly()
    {
        // Arrange
        var preprocessor = new Preprocessor();
        string source = @"#define PLATFORM_ANDROID
#ifdef PLATFORM_ANDROID
int platform = 1;
#endif
#ifdef PLATFORM_WINDOWS
int platform = 2;
#endif";
        
        // Act
        var result = preprocessor.Process(source, "test.adl");
        
        // Assert
        Assert.Contains("int platform = 1;", result.Source);
        Assert.DoesNotContain("int platform = 2;", result.Source);
    }
}
