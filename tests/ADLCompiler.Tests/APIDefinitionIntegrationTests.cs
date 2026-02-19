using Xunit;
using ADLCompiler.SemanticAnalysis;

namespace ADLCompiler.Tests;

public class APIDefinitionIntegrationTests
{
    [Fact]
    public void APIRegistry_LoadsAllSDKVersions_WithCoreAPIs()
    {
        // Arrange & Act
        var registry = new APIRegistry();
        
        // Assert - Verify all SDK versions are loaded
        var versions = registry.GetSupportedVersions().ToList();
        Assert.Equal(12, versions.Count);
        
        // Verify each SDK version has core APIs
        foreach (var version in versions)
        {
            var bundle = registry.GetSDKBundle(version);
            Assert.NotNull(bundle);
            Assert.True(bundle.APIs.Count > 0, $"SDK {version} should have APIs loaded");
            
            // Verify Activity API is present in all versions
            var activityAPI = bundle.LookupClass("android.app.Activity");
            Assert.NotNull(activityAPI);
        }
    }
    
    [Fact]
    public void APIRegistry_LookupActivityLifecycleMethods_AllPresent()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act - Lookup all lifecycle methods
        var onCreate = registry.LookupMethod(23, "android.app.Activity", "onCreate");
        var onStart = registry.LookupMethod(23, "android.app.Activity", "onStart");
        var onResume = registry.LookupMethod(23, "android.app.Activity", "onResume");
        var onPause = registry.LookupMethod(23, "android.app.Activity", "onPause");
        var onStop = registry.LookupMethod(23, "android.app.Activity", "onStop");
        var onDestroy = registry.LookupMethod(23, "android.app.Activity", "onDestroy");
        
        // Assert
        Assert.NotNull(onCreate);
        Assert.NotNull(onStart);
        Assert.NotNull(onResume);
        Assert.NotNull(onPause);
        Assert.NotNull(onStop);
        Assert.NotNull(onDestroy);
        
        // Verify onCreate has correct signature
        Assert.Equal("void", onCreate.ReturnType);
        Assert.Single(onCreate.Parameters);
        Assert.Equal("savedInstanceState", onCreate.Parameters[0].Name);
        Assert.Equal("android.os.Bundle", onCreate.Parameters[0].Type);
    }
    
    [Fact]
    public void APIRegistry_LookupViewAPIs_ReturnsCorrectDefinitions()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var setOnClickListener = registry.LookupMethod(23, "android.view.View", "setOnClickListener");
        var setVisibility = registry.LookupMethod(23, "android.view.View", "setVisibility");
        var visibleField = registry.LookupField(23, "android.view.View", "VISIBLE");
        var invisibleField = registry.LookupField(23, "android.view.View", "INVISIBLE");
        var goneField = registry.LookupField(23, "android.view.View", "GONE");
        
        // Assert
        Assert.NotNull(setOnClickListener);
        Assert.NotNull(setVisibility);
        Assert.NotNull(visibleField);
        Assert.NotNull(invisibleField);
        Assert.NotNull(goneField);
        
        // Verify field properties
        Assert.True(visibleField.IsStatic);
        Assert.Equal("int", visibleField.Type);
    }
    
    [Fact]
    public void APIRegistry_LookupWidgetAPIs_ButtonAndTextView()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var buttonSetText = registry.LookupMethod(23, "android.widget.Button", "setText");
        var buttonGetText = registry.LookupMethod(23, "android.widget.Button", "getText");
        var textViewSetText = registry.LookupMethod(23, "android.widget.TextView", "setText");
        var textViewGetText = registry.LookupMethod(23, "android.widget.TextView", "getText");
        
        // Assert
        Assert.NotNull(buttonSetText);
        Assert.NotNull(buttonGetText);
        Assert.NotNull(textViewSetText);
        Assert.NotNull(textViewGetText);
        
        // Verify setText signature
        Assert.Equal("void", buttonSetText.ReturnType);
        Assert.Single(buttonSetText.Parameters);
        Assert.Equal("text", buttonSetText.Parameters[0].Name);
        Assert.Equal("CharSequence", buttonSetText.Parameters[0].Type);
    }
    
    [Fact]
    public void APIRegistry_LookupContextAPIs_SharedPreferencesAndIntent()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var getSharedPreferences = registry.LookupMethod(23, "android.content.Context", "getSharedPreferences");
        var startActivity = registry.LookupMethod(23, "android.content.Context", "startActivity");
        
        // Assert
        Assert.NotNull(getSharedPreferences);
        Assert.NotNull(startActivity);
        
        // Verify getSharedPreferences signature
        Assert.Equal("android.content.SharedPreferences", getSharedPreferences.ReturnType);
        Assert.Equal(2, getSharedPreferences.Parameters.Count);
        Assert.Equal("name", getSharedPreferences.Parameters[0].Name);
        Assert.Equal("String", getSharedPreferences.Parameters[0].Type);
        Assert.Equal("mode", getSharedPreferences.Parameters[1].Name);
        Assert.Equal("int", getSharedPreferences.Parameters[1].Type);
    }
    
    [Fact]
    public void APIRegistry_LookupIntentAPIs_PutAndGetExtra()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var putExtra = registry.LookupMethod(23, "android.content.Intent", "putExtra");
        var getStringExtra = registry.LookupMethod(23, "android.content.Intent", "getStringExtra");
        
        // Assert
        Assert.NotNull(putExtra);
        Assert.NotNull(getStringExtra);
        
        // Verify putExtra returns Intent for chaining
        Assert.Equal("android.content.Intent", putExtra.ReturnType);
        Assert.Equal(2, putExtra.Parameters.Count);
    }
    
    [Fact]
    public void APIRegistry_NewerAPIs_OnlyAvailableInCorrectSDKVersion()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act - BiometricPrompt is only available in API 28+
        var biometric23 = registry.LookupClass(23, "android.hardware.biometrics.BiometricPrompt");
        var biometric27 = registry.LookupClass(27, "android.hardware.biometrics.BiometricPrompt");
        var biometric28 = registry.LookupClass(28, "android.hardware.biometrics.BiometricPrompt");
        var biometric30 = registry.LookupClass(30, "android.hardware.biometrics.BiometricPrompt");
        
        // Assert
        Assert.Null(biometric23); // Not available in API 23
        Assert.Null(biometric27); // Not available in API 27
        Assert.NotNull(biometric28); // Available starting API 28
        Assert.NotNull(biometric30); // Still available in API 30
    }
    
    [Fact]
    public void APIRegistry_WindowInsetsAPI_OnlyAvailableInAPI30Plus()
    {
        // Arrange
        var registry = new APIRegistry();
        
        // Act
        var windowInsets28 = registry.LookupClass(28, "android.view.WindowInsets");
        var windowInsets29 = registry.LookupClass(29, "android.view.WindowInsets");
        var windowInsets30 = registry.LookupClass(30, "android.view.WindowInsets");
        
        // Assert
        Assert.Null(windowInsets28); // Not available in API 28
        Assert.Null(windowInsets29); // Not available in API 29
        Assert.NotNull(windowInsets30); // Available starting API 30
        
        // Verify method is present
        var getInsets = registry.LookupMethod(30, "android.view.WindowInsets", "getInsets");
        Assert.NotNull(getInsets);
        Assert.Equal("android.graphics.Insets", getInsets.ReturnType);
    }
    
    [Fact]
    public void APIRegistry_CrossVersionCompatibility_CoreAPIsAvailableInAllVersions()
    {
        // Arrange
        var registry = new APIRegistry();
        var apiLevels = new[] { 23, 24, 25, 26, 27, 28, 29, 30, 31, 33, 34, 35 };
        
        // Act & Assert - Core APIs should be available in all versions
        foreach (var apiLevel in apiLevels)
        {
            var activity = registry.LookupClass(apiLevel, "android.app.Activity");
            var view = registry.LookupClass(apiLevel, "android.view.View");
            var context = registry.LookupClass(apiLevel, "android.content.Context");
            var intent = registry.LookupClass(apiLevel, "android.content.Intent");
            var button = registry.LookupClass(apiLevel, "android.widget.Button");
            
            Assert.NotNull(activity);
            Assert.NotNull(view);
            Assert.NotNull(context);
            Assert.NotNull(intent);
            Assert.NotNull(button);
        }
    }
}
