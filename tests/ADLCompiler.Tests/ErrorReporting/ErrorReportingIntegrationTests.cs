using ADLCompiler.ErrorReporting;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests.ErrorReporting;

public class ErrorReportingIntegrationTests
{
    [Fact]
    public void FormatError_WithRealSourceFile_ShowsSourceSnippet()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var source = @"public class Test {
    public void method() {
        int total = result + 5;
    }
}";
        File.WriteAllText(tempFile, source);

        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = tempFile, Line = 3, Column = 20 };

        // Act
        reporter.ReportUndefinedSymbol(location, "result", new List<string> { "results", "count" });
        var formatted = reporter.FormatAll();

        // Assert
        Assert.Contains("unknown variable 'result'", formatted);
        Assert.Contains("int total = result + 5;", formatted);
        Assert.Contains("^", formatted);
        Assert.Contains("Did you mean 'results'?", formatted);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void MultipleErrors_FormattedCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var source = @"public class Test {
    public void method() {
        int x = ""hello"";
        int y = result;
        String z = 123;
    }
}";
        File.WriteAllText(tempFile, source);

        var reporter = new DiagnosticReporter();
        
        // Act - Report multiple errors
        reporter.ReportTypeError(
            new SourceLocation { File = tempFile, Line = 3, Column = 16 },
            "int", "String");
        
        reporter.ReportUndefinedSymbol(
            new SourceLocation { File = tempFile, Line = 4, Column = 16 },
            "result");
        
        reporter.ReportTypeError(
            new SourceLocation { File = tempFile, Line = 5, Column = 19 },
            "String", "int");

        var formatted = reporter.FormatAll();

        // Assert
        Assert.Contains("3 error(s), 0 warning(s)", formatted);
        Assert.Contains("cannot convert String to int", formatted);
        Assert.Contains("unknown variable 'result'", formatted);
        Assert.Contains("cannot convert int to String", formatted);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void TypeErrorSuggestion_ForStringToInt_ShowsParseInt()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 15 };

        // Act
        reporter.ReportTypeError(location, "int", "String");
        var formatted = reporter.FormatAll();

        // Assert
        Assert.Contains("Integer.parseInt", formatted);
    }

    [Fact]
    public void TypeErrorSuggestion_ForIntToString_ShowsToString()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 15 };

        // Act
        reporter.ReportTypeError(location, "String", "int");
        var formatted = reporter.FormatAll();

        // Assert
        Assert.Contains("Integer.toString", formatted);
    }

    [Fact]
    public void TypoDetection_FindsSimilarSymbol()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        var availableSymbols = new List<string> 
        { 
            "userName", "userEmail", "userId", "count", "total" 
        };

        // Act
        reporter.ReportUndefinedSymbol(location, "userNam", availableSymbols);
        var formatted = reporter.FormatAll();

        // Assert
        Assert.Contains("Did you mean 'userName'?", formatted);
    }

    [Fact]
    public void DuplicateSymbol_ShowsOriginalDefinition()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var originalLocation = new SourceLocation { File = "test.adl", Line = 5, Column = 10 };
        var duplicateLocation = new SourceLocation { File = "test.adl", Line = 15, Column = 10 };

        // Act
        reporter.ReportDuplicateSymbol(duplicateLocation, "myVariable", originalLocation);
        var formatted = reporter.FormatAll();

        // Assert
        Assert.Contains("'myVariable' is already defined", formatted);
        Assert.Contains("test.adl:5:10", formatted);
    }

    [Fact]
    public void MissingClass_SuggestsFileCreation()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "Main.adl", Line = 10, Column = 5 };

        // Act
        reporter.ReportMissingClass(location, "DatabaseHelper");
        var formatted = reporter.FormatAll();

        // Assert
        Assert.Contains("class 'DatabaseHelper' not found", formatted);
        Assert.Contains("Make sure DatabaseHelper.adl exists", formatted);
    }

    [Fact]
    public void ErrorsAndWarnings_CountedSeparately()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };

        // Act
        reporter.ReportSyntaxError(location, "syntax error 1");
        reporter.ReportSyntaxError(location, "syntax error 2");
        reporter.Report(CompilerDiagnostic.Warning(location, "warning", "unused variable"));
        reporter.Report(CompilerDiagnostic.Warning(location, "warning", "deprecated method"));

        var formatted = reporter.FormatAll();

        // Assert
        Assert.Equal(2, reporter.ErrorCount);
        Assert.Equal(2, reporter.WarningCount);
        Assert.Contains("2 error(s), 2 warning(s)", formatted);
    }

    [Fact]
    public void ErrorFormat_MatchesIDEParseableFormat()
    {
        // Arrange
        var reporter = new DiagnosticReporter();
        var location = new SourceLocation { File = "Main.adl", Line = 15, Column = 23 };

        // Act
        reporter.ReportSyntaxError(location, "unexpected token");
        var formatted = reporter.FormatAll();

        // Assert - Should match file:line:column: severity: message format
        Assert.Contains("Main.adl:15:23: error:", formatted);
    }
}
