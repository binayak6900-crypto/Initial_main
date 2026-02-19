using SharedUtilities.Models;

namespace ADLCompiler.ErrorReporting;

/// <summary>
/// Severity level for compiler diagnostics
/// </summary>
public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Represents a compiler diagnostic (error, warning, or info) with location and friendly message
/// </summary>
public class CompilerDiagnostic
{
    public DiagnosticSeverity Severity { get; set; }
    public SourceLocation Location { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public string FriendlyMessage { get; set; } = string.Empty;
    public string SourceSnippet { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public List<QuickFix> QuickFixes { get; set; } = new();

    /// <summary>
    /// Formats the diagnostic as: file:line:column: severity: message
    /// </summary>
    public string Format()
    {
        var severityStr = Severity.ToString().ToLower();
        var result = $"{Location}: {severityStr}: {FriendlyMessage}\n";
        
        if (!string.IsNullOrEmpty(SourceSnippet))
        {
            result += $"  {SourceSnippet}\n";
            result += GenerateCaretIndicator();
        }
        
        if (Suggestions.Count > 0)
        {
            result += "\n";
            foreach (var suggestion in Suggestions)
            {
                result += $"Suggestion: {suggestion}\n";
            }
        }
        
        return result;
    }

    /// <summary>
    /// Generates a caret indicator (^~~~~) pointing to the error location
    /// </summary>
    private string GenerateCaretIndicator()
    {
        // Calculate the position of the caret based on column
        var indent = new string(' ', Location.Column + 2); // +2 for "  " prefix
        var caret = "^";
        
        // Add tildes to show the extent of the error (estimate 5 characters)
        var tildes = new string('~', Math.Min(5, SourceSnippet.Length - Location.Column));
        
        return $"{indent}{caret}{tildes}\n";
    }

    /// <summary>
    /// Creates an error diagnostic
    /// </summary>
    public static CompilerDiagnostic Error(SourceLocation location, string message, string friendlyMessage)
    {
        return new CompilerDiagnostic
        {
            Severity = DiagnosticSeverity.Error,
            Location = location,
            Message = message,
            FriendlyMessage = friendlyMessage
        };
    }

    /// <summary>
    /// Creates a warning diagnostic
    /// </summary>
    public static CompilerDiagnostic Warning(SourceLocation location, string message, string friendlyMessage)
    {
        return new CompilerDiagnostic
        {
            Severity = DiagnosticSeverity.Warning,
            Location = location,
            Message = message,
            FriendlyMessage = friendlyMessage
        };
    }

    /// <summary>
    /// Creates an info diagnostic
    /// </summary>
    public static CompilerDiagnostic Info(SourceLocation location, string message, string friendlyMessage)
    {
        return new CompilerDiagnostic
        {
            Severity = DiagnosticSeverity.Info,
            Location = location,
            Message = message,
            FriendlyMessage = friendlyMessage
        };
    }
}

/// <summary>
/// Represents an automated fix that can be applied to resolve a diagnostic
/// </summary>
public class QuickFix
{
    public string Description { get; set; } = string.Empty;
    public CodeReplacement Replacement { get; set; } = null!;
}

/// <summary>
/// Represents a code replacement for a quick fix
/// </summary>
public class CodeReplacement
{
    public SourceLocation Start { get; set; } = null!;
    public SourceLocation End { get; set; } = null!;
    public string NewText { get; set; } = string.Empty;
}
