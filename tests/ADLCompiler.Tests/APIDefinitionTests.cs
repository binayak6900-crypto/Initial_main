using Xunit;
using ADLCompiler.SemanticAnalysis;

namespace ADLCompiler.Tests;

public class APIDefinitionTests
{
    [Fact]
    public void APIDefinition_Constructor_SetsProperties()
    {
        // Arrange & Act
        var apiDef = new APIDefinition("android.app.Activity", 23);
        
        // Assert
        Assert.Equal("android.app.Activity", apiDef.ClassName);
        Assert.Equal(23, apiDef.MinSDK);
        Assert.Empty(apiDef.Methods);
        Assert.Empty(apiDef.Fields);
        Assert.False(apiDef.Deprecated);
    }
    
    [Fact]
    public void MethodSignature_Constructor_SetsProperties()
    {
        // Arrange & Act
        var method = new MethodSignature("onCreate", "void", 23);
        
        // Assert
        Assert.Equal("onCreate", method.Name);
        Assert.Equal("void", method.ReturnType);
        Assert.Equal(23, method.MinSDK);
        Assert.Empty(method.Parameters);
        Assert.False(method.IsStatic);
        Assert.False(method.Deprecated);
    }
    
    [Fact]
    public void FieldSignature_Constructor_SetsProperties()
    {
        // Arrange & Act
        var field = new FieldSignature("VISIBLE", "int", 23);
        
        // Assert
        Assert.Equal("VISIBLE", field.Name);
        Assert.Equal("int", field.Type);
        Assert.Equal(23, field.MinSDK);
        Assert.False(field.IsStatic);
        Assert.False(field.Deprecated);
    }
    
    [Fact]
    public void ParameterInfo_Constructor_SetsProperties()
    {
        // Arrange & Act
        var param = new ParameterInfo("savedInstanceState", "android.os.Bundle");
        
        // Assert
        Assert.Equal("savedInstanceState", param.Name);
        Assert.Equal("android.os.Bundle", param.Type);
    }
}
