using Xunit;
using ADLCompiler.SemanticAnalysis;

namespace ADLCompiler.Tests;

public class SDKBundleTests
{
    [Fact]
    public void SDKBundle_Constructor_SetsProperties()
    {
        // Arrange & Act
        var bundle = new SDKBundle("6.0", 23);
        
        // Assert
        Assert.Equal("6.0", bundle.Version);
        Assert.Equal(23, bundle.APILevel);
        Assert.Empty(bundle.APIs);
    }
    
    [Fact]
    public void LookupClass_WithExistingClass_ReturnsAPIDefinition()
    {
        // Arrange
        var bundle = new SDKBundle("6.0", 23);
        var activityAPI = new APIDefinition("android.app.Activity", 23);
        bundle.APIs.Add(activityAPI);
        
        // Act
        var result = bundle.LookupClass("android.app.Activity");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("android.app.Activity", result.ClassName);
    }
    
    [Fact]
    public void LookupClass_WithNonExistingClass_ReturnsNull()
    {
        // Arrange
        var bundle = new SDKBundle("6.0", 23);
        
        // Act
        var result = bundle.LookupClass("android.app.NonExistent");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void LookupMethod_WithExistingMethod_ReturnsMethodSignature()
    {
        // Arrange
        var bundle = new SDKBundle("6.0", 23);
        var activityAPI = new APIDefinition("android.app.Activity", 23);
        var onCreateMethod = new MethodSignature("onCreate", "void", 23);
        activityAPI.Methods.Add(onCreateMethod);
        bundle.APIs.Add(activityAPI);
        
        // Act
        var result = bundle.LookupMethod("android.app.Activity", "onCreate");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("onCreate", result.Name);
        Assert.Equal("void", result.ReturnType);
    }
    
    [Fact]
    public void LookupMethod_WithNonExistingClass_ReturnsNull()
    {
        // Arrange
        var bundle = new SDKBundle("6.0", 23);
        
        // Act
        var result = bundle.LookupMethod("android.app.NonExistent", "onCreate");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void LookupMethod_WithNonExistingMethod_ReturnsNull()
    {
        // Arrange
        var bundle = new SDKBundle("6.0", 23);
        var activityAPI = new APIDefinition("android.app.Activity", 23);
        bundle.APIs.Add(activityAPI);
        
        // Act
        var result = bundle.LookupMethod("android.app.Activity", "nonExistentMethod");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void LookupField_WithExistingField_ReturnsFieldSignature()
    {
        // Arrange
        var bundle = new SDKBundle("6.0", 23);
        var viewAPI = new APIDefinition("android.view.View", 23);
        var visibleField = new FieldSignature("VISIBLE", "int", 23) { IsStatic = true };
        viewAPI.Fields.Add(visibleField);
        bundle.APIs.Add(viewAPI);
        
        // Act
        var result = bundle.LookupField("android.view.View", "VISIBLE");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("VISIBLE", result.Name);
        Assert.Equal("int", result.Type);
        Assert.True(result.IsStatic);
    }
    
    [Fact]
    public void LookupField_WithNonExistingField_ReturnsNull()
    {
        // Arrange
        var bundle = new SDKBundle("6.0", 23);
        var viewAPI = new APIDefinition("android.view.View", 23);
        bundle.APIs.Add(viewAPI);
        
        // Act
        var result = bundle.LookupField("android.view.View", "NON_EXISTENT");
        
        // Assert
        Assert.Null(result);
    }
}
