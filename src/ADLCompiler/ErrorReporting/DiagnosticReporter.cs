using System.Text;
using SharedUtilities.Models;

namespace ADLCompiler.ErrorReporting;

/// <summary>
/// Collects and reports compiler diagnostics with friendly error messages
/// </summary>
public class DiagnosticReporter
{
    private readonly List<CompilerDiagnostic> _diagnostics = new();
    private readonly Dictionary<string, string[]> _sourceCache = new();

    /// <summary>
    /// Gets all collected diagnostics
    /// </summary>
    public IReadOnlyList<CompilerDiagnostic> Diagnostics => _diagnostics.AsReadOnly();

    /// <summary>
    /// Gets whether any errors have been reported
    /// </summary>
    public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets the count of errors
    /// </summary>
    public int ErrorCount => _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets the count of warnings
    /// </summary>
    public int WarningCount => _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);

    /// <summary>
    /// Adds a diagnostic to the collection
    /// </summary>
    public void Report(CompilerDiagnostic diagnostic)
    {
        // Add source snippet if not already present
        if (string.IsNullOrEmpty(diagnostic.SourceSnippet))
        {
            diagnostic.SourceSnippet = GetSourceLine(diagnostic.Location);
        }

        _diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Reports a syntax error
    /// </summary>
    public void ReportSyntaxError(SourceLocation location, string message, string? suggestion = null)
    {
        var diagnostic = CompilerDiagnostic.Error(location, message, message);
        
        if (suggestion != null)
        {
            diagnostic.Suggestions.Add(suggestion);
        }

        Report(diagnostic);
    }

    /// <summary>
    /// Reports a type error
    /// </summary>
    public void ReportTypeError(SourceLocation location, string expectedType, string actualType)
    {
        var message = $"cannot convert {actualType} to {expectedType}";
        var diagnostic = CompilerDiagnostic.Error(location, message, message);
        
        // Add helpful suggestion for common type conversions
        var suggestion = GetTypeConversionSuggestion(expectedType, actualType);
        if (suggestion != null)
        {
            diagnostic.Suggestions.Add(suggestion);
        }

        Report(diagnostic);
    }

    /// <summary>
    /// Reports an undefined symbol error
    /// </summary>
    public void ReportUndefinedSymbol(SourceLocation location, string symbolName, List<string>? availableSymbols = null)
    {
        var message = $"unknown variable '{symbolName}'";
        var diagnostic = CompilerDiagnostic.Error(location, message, message);
        
        // Try to find similar symbol names (typo detection)
        if (availableSymbols != null)
        {
            var similar = FindSimilarSymbols(symbolName, availableSymbols);
            if (similar.Count > 0)
            {
                diagnostic.Suggestions.Add($"Did you mean '{similar[0]}'?");
            }
            else
            {
                diagnostic.Suggestions.Add($"Did you forget to declare '{symbolName}'?");
            }
        }
        else
        {
            diagnostic.Suggestions.Add($"Did you forget to declare '{symbolName}'?");
        }

        Report(diagnostic);
    }

    /// <summary>
    /// Reports a missing class/file error
    /// </summary>
    public void ReportMissingClass(SourceLocation location, string className)
    {
        var message = $"class '{className}' not found";
        var diagnostic = CompilerDiagnostic.Error(location, message, message);
        diagnostic.Suggestions.Add($"Make sure {className}.adl exists in your project directory");

        Report(diagnostic);
    }

    /// <summary>
    /// Reports a duplicate symbol error
    /// </summary>
    public void ReportDuplicateSymbol(SourceLocation location, string symbolName, SourceLocation originalLocation)
    {
        var message = $"'{symbolName}' is already defined";
        var diagnostic = CompilerDiagnostic.Error(location, message, message);
        diagnostic.Suggestions.Add($"'{symbolName}' was first defined at {originalLocation}");

        Report(diagnostic);
    }

    /// <summary>
    /// Formats all diagnostics for output
    /// </summary>
    public string FormatAll()
    {
        if (_diagnostics.Count == 0)
        {
            return string.Empty;
        }

        // Sort diagnostics by file, line, column
        var sorted = _diagnostics
            .OrderBy(d => d.Location.File)
            .ThenBy(d => d.Location.Line)
            .ThenBy(d => d.Location.Column)
            .ToList();

        var sb = new StringBuilder();
        foreach (var diagnostic in sorted)
        {
            sb.Append(diagnostic.Format());
            sb.AppendLine();
        }

        // Add summary
        sb.AppendLine($"{ErrorCount} error(s), {WarningCount} warning(s)");

        return sb.ToString();
    }

    /// <summary>
    /// Clears all diagnostics
    /// </summary>
    public void Clear()
    {
        _diagnostics.Clear();
    }

    /// <summary>
    /// Gets the source line for a given location
    /// </summary>
    private string GetSourceLine(SourceLocation location)
    {
        try
        {
            if (!_sourceCache.TryGetValue(location.File, out var lines))
            {
                if (File.Exists(location.File))
                {
                    lines = File.ReadAllLines(location.File);
                    _sourceCache[location.File] = lines;
                }
                else
                {
                    return string.Empty;
                }
            }

            if (location.Line > 0 && location.Line <= lines.Length)
            {
                return lines[location.Line - 1].TrimEnd();
            }
        }
        catch
        {
            // Ignore errors reading source file
        }

        return string.Empty;
    }

    /// <summary>
    /// Finds similar symbol names using Levenshtein distance
    /// </summary>
    private List<string> FindSimilarSymbols(string target, List<string> candidates)
    {
        var similar = new List<(string symbol, int distance)>();

        foreach (var candidate in candidates)
        {
            var distance = LevenshteinDistance(target.ToLower(), candidate.ToLower());
            if (distance <= 2) // Allow up to 2 character differences
            {
                similar.Add((candidate, distance));
            }
        }

        return similar
            .OrderBy(s => s.distance)
            .Take(3)
            .Select(s => s.symbol)
            .ToList();
    }

    /// <summary>
    /// Calculates Levenshtein distance between two strings
    /// </summary>
    private int LevenshteinDistance(string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var matrix = new int[len1 + 1, len2 + 1];

        for (var i = 0; i <= len1; i++)
            matrix[i, 0] = i;
        for (var j = 0; j <= len2; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= len1; i++)
        {
            for (var j = 1; j <= len2; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        return matrix[len1, len2];
    }

    /// <summary>
    /// Gets a helpful suggestion for type conversion
    /// </summary>
    private string? GetTypeConversionSuggestion(string expectedType, string actualType)
    {
        // Common type conversion suggestions
        var conversions = new Dictionary<(string, string), string>
        {
            { ("int", "string"), "Use Integer.parseInt(value) to convert String to int" },
            { ("int", "String"), "Use Integer.parseInt(value) to convert String to int" },
            { ("double", "string"), "Use Double.parseDouble(value) to convert String to double" },
            { ("double", "String"), "Use Double.parseDouble(value) to convert String to double" },
            { ("float", "string"), "Use Float.parseFloat(value) to convert String to float" },
            { ("float", "String"), "Use Float.parseFloat(value) to convert String to float" },
            { ("bool", "string"), "Use Boolean.parseBoolean(value) to convert String to bool" },
            { ("bool", "String"), "Use Boolean.parseBoolean(value) to convert String to bool" },
            { ("string", "int"), "Use Integer.toString(value) or String.valueOf(value) to convert int to String" },
            { ("String", "int"), "Use Integer.toString(value) or String.valueOf(value) to convert int to String" },
        };

        var key = (expectedType.ToLower(), actualType.ToLower());
        if (conversions.TryGetValue(key, out var suggestion))
        {
            return suggestion;
        }

        return null;
    }
}
