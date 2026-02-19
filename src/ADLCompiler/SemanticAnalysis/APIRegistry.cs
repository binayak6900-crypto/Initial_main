namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Central registry for Android SDK API definitions across multiple SDK versions
/// </summary>
public class APIRegistry
{
    private readonly Dictionary<int, SDKBundle> _sdkBundles = new();
    
    /// <summary>
    /// Mapping of Android version strings to API levels
    /// </summary>
    private static readonly Dictionary<string, int> VersionToAPILevel = new()
    {
        { "6.0", 23 },
        { "7.0", 24 },
        { "7.1", 25 },
        { "8.0", 26 },
        { "8.1", 27 },
        { "9.0", 28 },
        { "10.0", 29 },
        { "11.0", 30 },
        { "12.0", 31 },
        { "13.0", 33 },
        { "14.0", 34 },
        { "15.0", 35 }
    };
    
    public APIRegistry()
    {
        LoadAllSDKBundles();
    }
    
    /// <summary>
    /// Load API definitions for all supported SDK versions (6.0 through 15.0)
    /// </summary>
    private void LoadAllSDKBundles()
    {
        foreach (var (version, apiLevel) in VersionToAPILevel)
        {
            var bundle = LoadSDKBundle(version, apiLevel);
            _sdkBundles[apiLevel] = bundle;
        }
    }
    
    /// <summary>
    /// Load API definitions for a specific SDK version
    /// </summary>
    private SDKBundle LoadSDKBundle(string version, int apiLevel)
    {
        var bundle = new SDKBundle(version, apiLevel);
        
        // Load core Android APIs for this SDK version
        LoadCoreAPIs(bundle, apiLevel);
        
        return bundle;
    }
    
    /// <summary>
    /// Load core Android API definitions
    /// </summary>
    private void LoadCoreAPIs(SDKBundle bundle, int apiLevel)
    {
        CoreAPILoader.LoadCoreAPIs(bundle, apiLevel);
        NDKAPILoader.LoadNDKAPIs(bundle, apiLevel);
        RaylibAPILoader.LoadRaylibAPIs(bundle, apiLevel);
    }

    /// <summary>
    /// Legacy method - kept for backward compatibility
    /// </summary>
    private void LoadCoreAPIsLegacy(SDKBundle bundle, int apiLevel)
    {
        // Activity API
        var activityAPI = new APIDefinition("android.app.Activity", 23);
        activityAPI.Methods.Add(new MethodSignature("onCreate", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("savedInstanceState", "android.os.Bundle") }
        });
        activityAPI.Methods.Add(new MethodSignature("onStart", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onResume", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onPause", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onStop", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("onDestroy", "void", 23));
        activityAPI.Methods.Add(new MethodSignature("setContentView", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("layoutResID", "int") }
        });
        activityAPI.Methods.Add(new MethodSignature("findViewById", "android.view.View", 23)
        {
            Parameters = new List<ParameterInfo> { new("id", "int") }
        });
        bundle.APIs.Add(activityAPI);
        
        // View API
        var viewAPI = new APIDefinition("android.view.View", 23);
        viewAPI.Methods.Add(new MethodSignature("setOnClickListener", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("listener", "android.view.View.OnClickListener") }
        });
        viewAPI.Methods.Add(new MethodSignature("setVisibility", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("visibility", "int") }
        });
        viewAPI.Fields.Add(new FieldSignature("VISIBLE", "int", 23) { IsStatic = true });
        viewAPI.Fields.Add(new FieldSignature("INVISIBLE", "int", 23) { IsStatic = true });
        viewAPI.Fields.Add(new FieldSignature("GONE", "int", 23) { IsStatic = true });
        bundle.APIs.Add(viewAPI);
        
        // Context API
        var contextAPI = new APIDefinition("android.content.Context", 23);
        contextAPI.Methods.Add(new MethodSignature("getSharedPreferences", "android.content.SharedPreferences", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("name", "String"),
                new("mode", "int")
            }
        });
        contextAPI.Methods.Add(new MethodSignature("startActivity", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("intent", "android.content.Intent") }
        });
        bundle.APIs.Add(contextAPI);
        
        // Intent API
        var intentAPI = new APIDefinition("android.content.Intent", 23);
        intentAPI.Methods.Add(new MethodSignature("putExtra", "android.content.Intent", 23)
        {
            Parameters = new List<ParameterInfo> 
            { 
                new("name", "String"),
                new("value", "String")
            }
        });
        intentAPI.Methods.Add(new MethodSignature("getStringExtra", "String", 23)
        {
            Parameters = new List<ParameterInfo> { new("name", "String") }
        });
        bundle.APIs.Add(intentAPI);
        
        // Button API
        var buttonAPI = new APIDefinition("android.widget.Button", 23);
        buttonAPI.Methods.Add(new MethodSignature("setText", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("text", "CharSequence") }
        });
        buttonAPI.Methods.Add(new MethodSignature("getText", "CharSequence", 23));
        bundle.APIs.Add(buttonAPI);
        
        // TextView API
        var textViewAPI = new APIDefinition("android.widget.TextView", 23);
        textViewAPI.Methods.Add(new MethodSignature("setText", "void", 23)
        {
            Parameters = new List<ParameterInfo> { new("text", "CharSequence") }
        });
        textViewAPI.Methods.Add(new MethodSignature("getText", "CharSequence", 23));
        bundle.APIs.Add(textViewAPI);
        
        // Add newer APIs for higher SDK levels
        if (apiLevel >= 28) // Android 9.0
        {
            var biometricAPI = new APIDefinition("android.hardware.biometrics.BiometricPrompt", 28);
            biometricAPI.Methods.Add(new MethodSignature("authenticate", "void", 28)
            {
                Parameters = new List<ParameterInfo> { new("crypto", "android.hardware.biometrics.BiometricPrompt.CryptoObject") }
            });
            bundle.APIs.Add(biometricAPI);
        }
        
        if (apiLevel >= 30) // Android 11.0
        {
            var windowInsetsAPI = new APIDefinition("android.view.WindowInsets", 30);
            windowInsetsAPI.Methods.Add(new MethodSignature("getInsets", "android.graphics.Insets", 30)
            {
                Parameters = new List<ParameterInfo> { new("typeMask", "int") }
            });
            bundle.APIs.Add(windowInsetsAPI);
        }
    }
    
    /// <summary>
    /// Get SDK bundle for a specific API level
    /// </summary>
    public SDKBundle? GetSDKBundle(int apiLevel)
    {
        return _sdkBundles.GetValueOrDefault(apiLevel);
    }
    
    /// <summary>
    /// Get SDK bundle for a specific Android version string
    /// </summary>
    public SDKBundle? GetSDKBundle(string version)
    {
        if (VersionToAPILevel.TryGetValue(version, out int apiLevel))
        {
            return GetSDKBundle(apiLevel);
        }
        return null;
    }
    
    /// <summary>
    /// Lookup an API class in a specific SDK version
    /// </summary>
    public APIDefinition? LookupClass(int apiLevel, string className)
    {
        var bundle = GetSDKBundle(apiLevel);
        return bundle?.LookupClass(className);
    }
    
    /// <summary>
    /// Lookup a method in a specific SDK version
    /// </summary>
    public MethodSignature? LookupMethod(int apiLevel, string className, string methodName)
    {
        var bundle = GetSDKBundle(apiLevel);
        return bundle?.LookupMethod(className, methodName);
    }
    
    /// <summary>
    /// Lookup a field in a specific SDK version
    /// </summary>
    public FieldSignature? LookupField(int apiLevel, string className, string fieldName)
    {
        var bundle = GetSDKBundle(apiLevel);
        return bundle?.LookupField(className, fieldName);
    }
    
    /// <summary>
    /// Get all supported API levels
    /// </summary>
    public IEnumerable<int> GetSupportedAPILevels()
    {
        return _sdkBundles.Keys.OrderBy(k => k);
    }
    
    /// <summary>
    /// Get all supported Android versions
    /// </summary>
    public IEnumerable<string> GetSupportedVersions()
    {
        return VersionToAPILevel.Keys.OrderBy(v => VersionToAPILevel[v]);
    }
}
