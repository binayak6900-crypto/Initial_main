using ADLCompiler.ErrorReporting;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests.ErrorReporting;

public class CompilerDiagnosticTests
{
    [Fact]
    public void Error_CreatesErrorDiagnostic()
    {
        // Arrange
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        var message = "Test error";
        var friendlyMessage = "This is a test error";

        // Act
        var diagnostic = CompilerDiagnostic.Error(location, message, friendlyMessage);

        // Assert
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(location, diagnostic.Location);
        Assert.Equal(message, diagnostic.Message);
        Assert.Equal(friendlyMessage, diagnostic.FriendlyMessage);
    }

    [Fact]
    public void Warning_CreatesWarningDiagnostic()
    {
        // Arrange
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        var message = "Test warning";
        var friendlyMessage = "This is a test warning";

        // Act
        var diagnostic = CompilerDiagnostic.Warning(location, message, friendlyMessage);

        // Assert
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [Fact]
    public void Format_ProducesCorrectFormat()
    {
        // Arrange
        var location = new SourceLocation { File = "Main.adl", Line = 15, Column = 23 };
        var diagnostic = CompilerDiagnostic.Error(location, "unknown variable", "unknown variable 'result'");
        diagnostic.SourceSnippet = "    int total = result + 5;";
        diagnostic.Suggestions.Add("Did you mean 'results'?");

        // Act
        var formatted = diagnostic.Format();

        // Assert
        Assert.Contains("Main.adl:15:23: error: unknown variable 'result'", formatted);
        Assert.Contains("int total = result + 5;", formatted);
        Assert.Contains("^", formatted);
        Assert.Contains("Suggestion: Did you mean 'results'?", formatted);
    }

    [Fact]
    public void Format_WithMultipleSuggestions_ShowsAll()
    {
        // Arrange
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        var diagnostic = CompilerDiagnostic.Error(location, "error", "test error");
        diagnostic.Suggestions.Add("First suggestion");
        diagnostic.Suggestions.Add("Second suggestion");

        // Act
        var formatted = diagnostic.Format();

        // Assert
        Assert.Contains("Suggestion: First suggestion", formatted);
        Assert.Contains("Suggestion: Second suggestion", formatted);
    }

    [Fact]
    public void Format_WithoutSourceSnippet_DoesNotShowCaret()
    {
        // Arrange
        var location = new SourceLocation { File = "test.adl", Line = 10, Column = 5 };
        var diagnostic = CompilerDiagnostic.Error(location, "error", "test error");

        // Act
        var formatted = diagnostic.Format();

        // Assert
        Assert.DoesNotContain("^", formatted);
    }
}
