using ADLCompiler.ErrorReporting;
using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Checks API usage against target SDK version and reports compatibility errors
/// Validates: Requirements 17.1, 17.2, 17.3, 17.4
/// </summary>
public class APICompatibilityChecker
{
    private readonly APIRegistry _apiRegistry;
    private readonly DiagnosticReporter _diagnosticReporter;
    private readonly int _targetSDK;

    public APICompatibilityChecker(APIRegistry apiRegistry, DiagnosticReporter diagnosticReporter, int targetSDK)
    {
        _apiRegistry = apiRegistry;
        _diagnosticReporter = diagnosticReporter;
        _targetSDK = targetSDK;
    }

    /// <summary>
    /// Check if a method call is compatible with the target SDK version
    /// Requirement 17.1: Check method calls against target SDK version
    /// Requirement 17.2: Report errors when using APIs above target SDK
    /// Requirement 17.3: Warn about deprecated APIs
    /// </summary>
    public void CheckMethodCall(MethodCallExpression methodCall, string? className = null)
    {
        if (methodCall.Target != null)
        {
            // Instance method call - need to determine the class from the target expression
            // For now, we'll extract the class name if the target is an identifier or member access
            className = ExtractClassName(methodCall.Target);
        }

        if (string.IsNullOrEmpty(className))
        {
            // Cannot determine class name - skip compatibility check
            return;
        }

        // Lookup the method in the API registry
        var methodSignature = _apiRegistry.LookupMethod(_targetSDK, className, methodCall.MethodName);

        if (methodSignature == null)
        {
            // Method not found in API registry - might be user-defined or from a different source
            return;
        }

        // Requirement 17.2: Report errors when using APIs above target SDK
        if (methodSignature.MinSDK > _targetSDK)
        {
            var diagnostic = CompilerDiagnostic.Error(
                methodCall.Location,
                $"API level {methodSignature.MinSDK} required",
                $"Method '{className}.{methodCall.MethodName}' requires API level {methodSignature.MinSDK}, but target SDK is {_targetSDK}"
            );
            
            diagnostic.Suggestions.Add($"Increase target SDK to {methodSignature.MinSDK} or higher");
            diagnostic.Suggestions.Add($"Add runtime API level check: if (Build.VERSION.SDK_INT >= {methodSignature.MinSDK})");
            
            _diagnosticReporter.Report(diagnostic);
        }

        // Requirement 17.3: Warn about deprecated APIs
        if (methodSignature.Deprecated)
        {
            var diagnostic = CompilerDiagnostic.Warning(
                methodCall.Location,
                $"Deprecated API usage",
                $"Method '{className}.{methodCall.MethodName}' is deprecated and may be removed in future Android versions"
            );
            
            diagnostic.Suggestions.Add("Consider using a newer alternative API");
            diagnostic.Suggestions.Add("Check Android documentation for recommended replacements");
            
            _diagnosticReporter.Report(diagnostic);
        }
    }

    /// <summary>
    /// Check if a field access is compatible with the target SDK version
    /// Requirement 17.1: Check field access against target SDK version
    /// Requirement 17.2: Report errors when using APIs above target SDK
    /// Requirement 17.3: Warn about deprecated APIs
    /// </summary>
    public void CheckFieldAccess(MemberAccessExpression memberAccess, string? className = null)
    {
        if (className == null)
        {
            className = ExtractClassName(memberAccess.Target);
        }

        if (string.IsNullOrEmpty(className))
        {
            // Cannot determine class name - skip compatibility check
            return;
        }

        // Lookup the field in the API registry
        var fieldSignature = _apiRegistry.LookupField(_targetSDK, className, memberAccess.MemberName);

        if (fieldSignature == null)
        {
            // Field not found in API registry - might be user-defined or from a different source
            return;
        }

        // Requirement 17.2: Report errors when using APIs above target SDK
        if (fieldSignature.MinSDK > _targetSDK)
        {
            var diagnostic = CompilerDiagnostic.Error(
                memberAccess.Location,
                $"API level {fieldSignature.MinSDK} required",
                $"Field '{className}.{memberAccess.MemberName}' requires API level {fieldSignature.MinSDK}, but target SDK is {_targetSDK}"
            );
            
            diagnostic.Suggestions.Add($"Increase target SDK to {fieldSignature.MinSDK} or higher");
            diagnostic.Suggestions.Add($"Add runtime API level check: if (Build.VERSION.SDK_INT >= {fieldSignature.MinSDK})");
            
            _diagnosticReporter.Report(diagnostic);
        }

        // Requirement 17.3: Warn about deprecated APIs
        if (fieldSignature.Deprecated)
        {
            var diagnostic = CompilerDiagnostic.Warning(
                memberAccess.Location,
                $"Deprecated API usage",
                $"Field '{className}.{memberAccess.MemberName}' is deprecated and may be removed in future Android versions"
            );
            
            diagnostic.Suggestions.Add("Consider using a newer alternative API");
            diagnostic.Suggestions.Add("Check Android documentation for recommended replacements");
            
            _diagnosticReporter.Report(diagnostic);
        }
    }

    /// <summary>
    /// Check if a class usage is compatible with the target SDK version
    /// Requirement 17.1: Check class usage against target SDK version
    /// Requirement 17.2: Report errors when using APIs above target SDK
    /// Requirement 17.3: Warn about deprecated APIs
    /// Requirement 17.4: Validate API availability
    /// </summary>
    public void CheckClassUsage(string className, SourceLocation location)
    {
        // Lookup the class in the API registry
        var apiDefinition = _apiRegistry.LookupClass(_targetSDK, className);

        if (apiDefinition == null)
        {
            // Class not found in API registry - might be user-defined or from a different source
            return;
        }

        // Requirement 17.2: Report errors when using APIs above target SDK
        if (apiDefinition.MinSDK > _targetSDK)
        {
            var diagnostic = CompilerDiagnostic.Error(
                location,
                $"API level {apiDefinition.MinSDK} required",
                $"Class '{className}' requires API level {apiDefinition.MinSDK}, but target SDK is {_targetSDK}"
            );
            
            diagnostic.Suggestions.Add($"Increase target SDK to {apiDefinition.MinSDK} or higher");
            diagnostic.Suggestions.Add($"Add runtime API level check before using this class");
            
            _diagnosticReporter.Report(diagnostic);
        }

        // Requirement 17.3: Warn about deprecated APIs
        if (apiDefinition.Deprecated)
        {
            var diagnostic = CompilerDiagnostic.Warning(
                location,
                $"Deprecated API usage",
                $"Class '{className}' is deprecated and may be removed in future Android versions"
            );
            
            diagnostic.Suggestions.Add("Consider using a newer alternative class");
            diagnostic.Suggestions.Add("Check Android documentation for recommended replacements");
            
            _diagnosticReporter.Report(diagnostic);
        }
    }

    /// <summary>
    /// Extract class name from an expression (best effort)
    /// </summary>
    private string? ExtractClassName(Expression expression)
    {
        return expression switch
        {
            IdentifierExpression identifier => identifier.Name,
            MemberAccessExpression memberAccess => ExtractClassName(memberAccess.Target),
            MethodCallExpression methodCall => ExtractClassName(methodCall.Target!),
            NewExpression newExpr => newExpr.TypeToCreate.Name,
            _ => null
        };
    }
}
