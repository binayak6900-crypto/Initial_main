using ADLCompiler.ErrorReporting;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests.ErrorReporting;

public class DiagnosticReporterTests
{
    [Fact]
    public void Report_AddsDiagnostic()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        var diagnostic = CompilerDiagnostic.Error(location, "error", "test error");

        // Act
        reporter.Report(diagnostic);

        // Assert
        Assert.Single(reporter.Diagnostics);
        Assert.Equal(diagnostic, reporter.Diagnostics[0]);
    }

    [Fact]
    public void HasErrors_ReturnsTrueWhenErrorsExist()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };

        // Act
        reporter.ReportSyntaxError(location, "syntax error");

        // Assert
        Assert.True(reporter.HasErrors);
        Assert.Equal(1, reporter.ErrorCount);
    }

    [Fact]
    public void HasErrors_ReturnsFalseForWarnings()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        var diagnostic = CompilerDiagnostic.Warning(location, "warning", "test warning");

        // Act
        reporter.Report(diagnostic);

        // Assert
        Assert.False(reporter.HasErrors);
        Assert.Equal(0, reporter.ErrorCount);
        Assert.Equal(1, reporter.WarningCount);
    }

    [Fact]
    public void ReportTypeError_CreatesTypeErrorWithSuggestion()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 20, Column = 15 };

        // Act
        reporter.ReportTypeError(location, "int", "String");

        // Assert
        Assert.Single(reporter.Diagnostics);
        var diagnostic = reporter.Diagnostics[0];
        Assert.Contains("cannot convert String to int", diagnostic.FriendlyMessage);
        Assert.Contains("Integer.parseInt", diagnostic.Suggestions[0]);
    }

    [Fact]
    public void ReportUndefinedSymbol_WithSimilarSymbols_SuggestsSimilar()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 15, Column = 23 };
        var availableSymbols = new List<string> { "results", "result2", "count" };

        // Act
        reporter.ReportUndefinedSymbol(location, "result", availableSymbols);

        // Assert
        Assert.Single(reporter.Diagnostics);
        var diagnostic = reporter.Diagnostics[0];
        Assert.Contains("unknown variable 'result'", diagnostic.FriendlyMessage);
        Assert.Contains("Did you mean 'results'?", diagnostic.Suggestions[0]);
    }

    [Fact]
    public void ReportUndefinedSymbol_WithoutSimilarSymbols_SuggestsDeclaration()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 15, Column = 23 };
        var availableSymbols = new List<string> { "foo", "bar", "baz" };

        // Act
        reporter.ReportUndefinedSymbol(location, "result", availableSymbols);

        // Assert
        Assert.Single(reporter.Diagnostics);
        var diagnostic = reporter.Diagnostics[0];
        Assert.Contains("Did you forget to declare 'result'?", diagnostic.Suggestions[0]);
    }

    [Fact]
    public void ReportMissingClass_SuggestsFileCreation()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "Main.adl", Line = 10, Column = 5 };

        // Act
        reporter.ReportMissingClass(location, "Utils");

        // Assert
        Assert.Single(reporter.Diagnostics);
        var diagnostic = reporter.Diagnostics[0];
        Assert.Contains("class 'Utils' not found", diagnostic.FriendlyMessage);
        Assert.Contains("Make sure Utils.adl exists", diagnostic.Suggestions[0]);
    }

    [Fact]
    public void ReportDuplicateSymbol_ShowsOriginalLocation()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 20, Column = 10 };
        var originalLocation = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };

        // Act
        reporter.ReportDuplicateSymbol(location, "myVar", originalLocation);

        // Assert
        Assert.Single(reporter.Diagnostics);
        var diagnostic = reporter.Diagnostics[0];
        Assert.Contains("'myVar' is already defined", diagnostic.FriendlyMessage);
        Assert.Contains("test.adl:10:5", diagnostic.Suggestions[0]);
    }

    [Fact]
    public void FormatAll_SortsDiagnosticsByLocation()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var loc1 = new SourceLocation { File = "b.adl", Line = 10, Column = 5 };
        var loc2 = new SourceLocation { File = "a.adl", Line = 20, Column = 10 };
        var loc3 = new SourceLocation { File = "a.adl", Line = 15, Column = 5 };

        reporter.ReportSyntaxError(loc1, "error 1");
        reporter.ReportSyntaxError(loc2, "error 2");
        reporter.ReportSyntaxError(loc3, "error 3");

        // Act
        var formatted = reporter.FormatAll();

        // Assert
        var lines = formatted.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains("a.adl:15:5", lines[0]); // First by file, then by line
        Assert.Contains("a.adl:20:10", lines[2]);
        Assert.Contains("b.adl:10:5", lines[4]);
    }

    [Fact]
    public void FormatAll_IncludesSummary()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        
        reporter.ReportSyntaxError(location, "error 1");
        reporter.ReportSyntaxError(location, "error 2");
        reporter.Report(CompilerDiagnostic.Warning(location, "warning", "warning 1"));

        // Act
        var formatted = reporter.FormatAll();

        // Assert
        Assert.Contains("2 error(s), 1 warning(s)", formatted);
    }

    [Fact]
    public void Clear_RemovesAllDiagnostics()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        reporter.ReportSyntaxError(location, "error");

        // Act
        reporter.Clear();

        // Assert
        Assert.Empty(reporter.Diagnostics);
        Assert.False(reporter.HasErrors);
    }

    [Fact]
    public void ReportMultipleErrors_CollectsAll()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var loc1 = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        var loc2 = new SourceLocation { File = "test.adl", Line = 20, Column = 10 };
        var loc3 = new SourceLocation { File = "test.adl", Line = 30, Column = 15 };

        // Act
        reporter.ReportSyntaxError(loc1, "error 1");
        reporter.ReportTypeError(loc2, "int", "String");
        reporter.ReportUndefinedSymbol(loc3, "foo");

        // Assert
        Assert.Equal(3, reporter.Diagnostics.Count);
        Assert.Equal(3, reporter.ErrorCount);
    }
}
