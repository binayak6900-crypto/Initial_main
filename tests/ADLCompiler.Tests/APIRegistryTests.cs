using Xunit;
using ADLCompiler.SemanticAnalysis;

namespace ADLCompiler.Tests;

public class APIRegistryTests
{
    [Fact]
    public void APIRegistry_Constructor_LoadsAllSDKBundles()
    {
        // Arrange & Act
        var registry = new APIRegistry();
        
        // Assert
        var supportedLevels = registry.GetSupportedAPILevels().ToList();
        Assert.Contains(23, supportedLevels); // Android 6.0
        Assert.Contains(24, supportedLevels); // Android 7.0
        Assert.Contains(25, supportedLevels); // Android 7.1
        Assert.Contains(26, supportedLevels); // Android 8.0
        Assert.Contains(27, supportedLevels); // Android 8.1
        Assert.Contains(28, supportedLevels); // Android 9.0
        Assert.Contains(29, supportedLevels); // Android 10.0
        Assert.Contains(30, supportedLevels); // Android 11.0
        Assert.Contains(31, supportedLevels); // Android 12.0
        Assert.Contains(33, supportedLevels); // Android 13.0
        Assert.Contains(34, supportedLevels); // Android 14.0
        Assert.Contains(35, supportedLevels); // Android 15.0
    }
    
    [Fact]
    public void GetSDKBundle_WithValidAPILevel_ReturnsBundle()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var bundle = registry.GetSDKBundle(23);
        
        // Assert
        Assert.NotNull(bundle);
        Assert.Equal("6.0", bundle.Version);
        Assert.Equal(23, bundle.APILevel);
    }
    
    [Fact]
    public void GetSDKBundle_WithValidVersion_ReturnsBundle()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var bundle = registry.GetSDKBundle("6.0");
        
        // Assert
        Assert.NotNull(bundle);
        Assert.Equal("6.0", bundle.Version);
        Assert.Equal(23, bundle.APILevel);
    }
    
    [Fact]
    public void GetSDKBundle_WithInvalidVersion_ReturnsNull()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var bundle = registry.GetSDKBundle("99.0");
        
        // Assert
        Assert.Null(bundle);
    }
    
    [Fact]
    public void LookupClass_WithCoreAndroidAPI_ReturnsAPIDefinition()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var activityAPI = registry.LookupClass(23, "android.app.Activity");
        
        // Assert
        Assert.NotNull(activityAPI);
        Assert.Equal("android.app.Activity", activityAPI.ClassName);
        Assert.True(activityAPI.Methods.Count > 0);
    }
    
    [Fact]
    public void LookupMethod_WithCoreAndroidMethod_ReturnsMethodSignature()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var onCreateMethod = registry.LookupMethod(23, "android.app.Activity", "onCreate");
        
        // Assert
        Assert.NotNull(onCreateMethod);
        Assert.Equal("onCreate", onCreateMethod.Name);
        Assert.Equal("void", onCreateMethod.ReturnType);
        Assert.Single(onCreateMethod.Parameters);
        Assert.Equal("savedInstanceState", onCreateMethod.Parameters[0].Name);
    }
    
    [Fact]
    public void LookupField_WithCoreAndroidField_ReturnsFieldSignature()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var visibleField = registry.LookupField(23, "android.view.View", "VISIBLE");
        
        // Assert
        Assert.NotNull(visibleField);
        Assert.Equal("VISIBLE", visibleField.Name);
        Assert.Equal("int", visibleField.Type);
        Assert.True(visibleField.IsStatic);
    }
    
    [Fact]
    public void LookupClass_WithNewerAPI_OnlyAvailableInHigherSDK()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var biometricAPI23 = registry.LookupClass(23, "android.hardware.biometrics.BiometricPrompt");
        var biometricAPI28 = registry.LookupClass(28, "android.hardware.biometrics.BiometricPrompt");
        
        // Assert
        Assert.Null(biometricAPI23); // Not available in API 23
        Assert.NotNull(biometricAPI28); // Available in API 28
    }
    
    [Fact]
    public void GetSupportedVersions_ReturnsAllVersionsInOrder()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var versions = registry.GetSupportedVersions().ToList();
        
        // Assert
        Assert.Equal(12, versions.Count);
        Assert.Equal("6.0", versions[0]);
        Assert.Equal("15.0", versions[11]);
    }
    
    [Theory]
    [InlineData(23, "6.0")]
    [InlineData(24, "7.0")]
    [InlineData(25, "7.1")]
    [InlineData(26, "8.0")]
    [InlineData(27, "8.1")]
    [InlineData(28, "9.0")]
    [InlineData(29, "10.0")]
    [InlineData(30, "11.0")]
    [InlineData(31, "12.0")]
    [InlineData(33, "13.0")]
    [InlineData(34, "14.0")]
    [InlineData(35, "15.0")]
    public void GetSDKBundle_AllSupportedVersions_ReturnsCorrectBundle(int apiLevel, string expectedVersion)
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var bundle = registry.GetSDKBundle(apiLevel);
        
        // Assert
        Assert.NotNull(bundle);
        Assert.Equal(expectedVersion, bundle.Version);
        Assert.Equal(apiLevel, bundle.APILevel);
    }
    
    [Fact]
    public void LookupMethod_MultipleMethodsInClass_ReturnsCorrectMethod()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var onCreate = registry.LookupMethod(23, "android.app.Activity", "onCreate");
        var onStart = registry.LookupMethod(23, "android.app.Activity", "onStart");
        var onResume = registry.LookupMethod(23, "android.app.Activity", "onResume");
        
        // Assert
        Assert.NotNull(onCreate);
        Assert.NotNull(onStart);
        Assert.NotNull(onResume);
        Assert.Equal("onCreate", onCreate.Name);
        Assert.Equal("onStart", onStart.Name);
        Assert.Equal("onResume", onResume.Name);
    }
}
