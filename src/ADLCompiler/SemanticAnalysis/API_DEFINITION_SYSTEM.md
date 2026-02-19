# API Definition System

## Overview

The API Definition System provides a comprehensive registry of Android SDK APIs across multiple SDK versions (6.0 through 15.0). This system enables the ADL compiler to:

1. Validate API usage against target SDK versions
2. Provide intelligent code suggestions in the IDE
3. Check API compatibility and report errors when using APIs above the target SDK
4. Support all Android SDK versions from API 23 (Android 6.0) through API 35 (Android 15.0)

## Architecture

### Core Components

#### 1. APIDefinition
Represents a single Android API class with its methods and fields.

```csharp
public class APIDefinition
{
    public string ClassName { get; set; }
    public List<MethodSignature> Methods { get; set; }
    public List<FieldSignature> Fields { get; set; }
    public int MinSDK { get; set; }
    public bool Deprecated { get; set; }
}
```

#### 2. MethodSignature
Represents a method in an Android API class.

```csharp
public class MethodSignature
{
    public string Name { get; set; }
    public string ReturnType { get; set; }
    public List<ParameterInfo> Parameters { get; set; }
    public bool IsStatic { get; set; }
    public string AccessModifier { get; set; }
    public int MinSDK { get; set; }
    public bool Deprecated { get; set; }
}
```

#### 3. FieldSignature
Represents a field in an Android API class.

```csharp
public class FieldSignature
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsStatic { get; set; }
    public string AccessModifier { get; set; }
    public int MinSDK { get; set; }
    public bool Deprecated { get; set; }
}
```

#### 4. SDKBundle
Represents a complete SDK version with all its API definitions.

```csharp
public class SDKBundle
{
    public string Version { get; set; }  // "6.0", "7.0", etc.
    public int APILevel { get; set; }    // 23, 24, etc.
    public List<APIDefinition> APIs { get; set; }
    
    // Lookup methods
    public APIDefinition? LookupClass(string className);
    public MethodSignature? LookupMethod(string className, string methodName);
    public FieldSignature? LookupField(string className, string fieldName);
}
```

#### 5. APIRegistry
Central registry that manages all SDK bundles and provides lookup functionality.

```csharp
public class APIRegistry
{
    // Get SDK bundle by API level or version string
    public SDKBundle? GetSDKBundle(int apiLevel);
    public SDKBundle? GetSDKBundle(string version);
    
    // Lookup APIs in specific SDK versions
    public APIDefinition? LookupClass(int apiLevel, string className);
    public MethodSignature? LookupMethod(int apiLevel, string className, string methodName);
    public FieldSignature? LookupField(int apiLevel, string className, string fieldName);
    
    // Get supported versions
    public IEnumerable<int> GetSupportedAPILevels();
    public IEnumerable<string> GetSupportedVersions();
}
```

## Supported SDK Versions

The system supports the following Android SDK versions:

| Version | API Level | Release Name |
|---------|-----------|--------------|
| 6.0     | 23        | Marshmallow  |
| 7.0     | 24        | Nougat       |
| 7.1     | 25        | Nougat       |
| 8.0     | 26        | Oreo         |
| 8.1     | 27        | Oreo         |
| 9.0     | 28        | Pie          |
| 10.0    | 29        | Q            |
| 11.0    | 30        | R            |
| 12.0    | 31        | S            |
| 13.0    | 33        | Tiramisu     |
| 14.0    | 34        | UpsideDownCake |
| 15.0    | 35        | VanillaIceCream |

## Core Android APIs

The system includes definitions for core Android APIs:

### Activity APIs
- `android.app.Activity`
  - `onCreate(Bundle savedInstanceState)`
  - `onStart()`
  - `onResume()`
  - `onPause()`
  - `onStop()`
  - `onDestroy()`
  - `setContentView(int layoutResID)`
  - `findViewById(int id)`

### View APIs
- `android.view.View`
  - `setOnClickListener(OnClickListener listener)`
  - `setVisibility(int visibility)`
  - Fields: `VISIBLE`, `INVISIBLE`, `GONE`

### Context APIs
- `android.content.Context`
  - `getSharedPreferences(String name, int mode)`
  - `startActivity(Intent intent)`

### Intent APIs
- `android.content.Intent`
  - `putExtra(String name, String value)`
  - `getStringExtra(String name)`

### Widget APIs
- `android.widget.Button`
  - `setText(CharSequence text)`
  - `getText()`

- `android.widget.TextView`
  - `setText(CharSequence text)`
  - `getText()`

### Version-Specific APIs

#### API 28+ (Android 9.0+)
- `android.hardware.biometrics.BiometricPrompt`
  - `authenticate(CryptoObject crypto)`

#### API 30+ (Android 11.0+)
- `android.view.WindowInsets`
  - `getInsets(int typeMask)`

## Usage Examples

### Basic Lookup

```csharp
// Create registry
var registry = new APIRegistry();

// Lookup a class
var activityAPI = registry.LookupClass(23, "android.app.Activity");

// Lookup a method
var onCreateMethod = registry.LookupMethod(23, "android.app.Activity", "onCreate");

// Lookup a field
var visibleField = registry.LookupField(23, "android.view.View", "VISIBLE");
```

### Version Compatibility Checking

```csharp
var registry = new APIRegistry();

// Check if API is available in target SDK
var biometricAPI = registry.LookupClass(23, "android.hardware.biometrics.BiometricPrompt");
if (biometricAPI == null)
{
    // API not available in API 23
    Console.WriteLine("BiometricPrompt requires API 28+");
}

// Check in API 28
var biometricAPI28 = registry.LookupClass(28, "android.hardware.biometrics.BiometricPrompt");
if (biometricAPI28 != null)
{
    // API is available
    Console.WriteLine("BiometricPrompt is available in API 28");
}
```

### Iterating Through Supported Versions

```csharp
var registry = new APIRegistry();

// Get all supported versions
foreach (var version in registry.GetSupportedVersions())
{
    var bundle = registry.GetSDKBundle(version);
    Console.WriteLine($"SDK {version} (API {bundle.APILevel}) has {bundle.APIs.Count} APIs");
}
```

## Integration with Compiler

The API Definition System integrates with the compiler in several ways:

### 1. Type Checking
The type checker uses the API registry to validate that referenced Android classes and methods exist and are available in the target SDK version.

### 2. API Compatibility Checking
During semantic analysis, the compiler checks that all API calls are compatible with the target SDK version. If a method requires a higher API level than the target, a compilation error is reported.

### 3. Code Suggestions
The IDE uses the API registry to provide intelligent code completion. When the user types a class name or method call, the IDE queries the registry to show available methods and their signatures.

### 4. Deprecation Warnings
The system tracks deprecated APIs and can warn developers when they use deprecated methods or classes.

## Future Enhancements

The API Definition System is designed to be extensible. Future enhancements may include:

1. **Complete API Coverage**: Add definitions for all Android SDK APIs, not just core APIs
2. **NDK API Definitions**: Add definitions for NDK APIs (OpenGL ES, Vulkan, audio, sensors)
3. **Third-Party Library Support**: Support for popular Android libraries (AndroidX, Material Components)
4. **API Documentation**: Include documentation strings for each API to show in IDE tooltips
5. **API Usage Examples**: Include code examples for common API patterns
6. **Automatic API Discovery**: Generate API definitions automatically from Android SDK sources
7. **API Change Tracking**: Track API changes between SDK versions for migration guidance

## Testing

The API Definition System includes comprehensive tests:

- **Unit Tests**: Test individual components (APIDefinition, SDKBundle, APIRegistry)
- **Integration Tests**: Test the complete system with realistic scenarios
- **Coverage**: All core Android APIs are tested across all supported SDK versions

Run tests with:
```bash
dotnet test --filter "FullyQualifiedName~APIDefinition"
```

## Requirements Satisfied

This implementation satisfies the following requirements from the spec:

- **Requirement 14.1-14.5**: Built-in Android API access
- **Requirement 17.1-17.6**: Multi-SDK version support (6.0 through 15.0)

## Related Files

- `APIDefinition.cs` - Core data structures
- `SDKBundle.cs` - SDK version bundle
- `APIRegistry.cs` - Central API registry
- `APIDefinitionTests.cs` - Unit tests
- `SDKBundleTests.cs` - SDK bundle tests
- `APIRegistryTests.cs` - Registry tests
- `APIDefinitionIntegrationTests.cs` - Integration tests
