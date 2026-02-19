using ADLCompiler.SemanticAnalysis;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests;

/// <summary>
/// Unit tests for the DependencyAnalyzer.
/// Tests automatic file discovery, dependency graph building, and topological sorting.
/// </summary>
public class DependencyAnalyzerTests
{
    [Fact]
    public void AnalyzeDependencies_EmptyDirectory_ReturnsEmptyList()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file => new CompilationUnit());
        
        // Assert
        Assert.Empty(result);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_SingleFile_ReturnsSingleFile()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var file1 = Path.Combine(tempDir, "Main.adl");
        File.WriteAllText(file1, "class Main {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            ast.Classes.Add(new ClassDeclaration { Name = "Main" });
            return ast;
        });
        
        // Assert
        Assert.Single(result);
        Assert.Equal(file1, result[0]);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_TwoFilesNoDependencies_ReturnsAnyOrder()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var file1 = Path.Combine(tempDir, "ClassA.adl");
        var file2 = Path.Combine(tempDir, "ClassB.adl");
        File.WriteAllText(file1, "class ClassA {}");
        File.WriteAllText(file2, "class ClassB {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("ClassA"))
            {
                ast.Classes.Add(new ClassDeclaration { Name = "ClassA" });
            }
            else
            {
                ast.Classes.Add(new ClassDeclaration { Name = "ClassB" });
            }
            return ast;
        });
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(file1, result);
        Assert.Contains(file2, result);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_SimpleDependency_ReturnsCorrectOrder()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var file1 = Path.Combine(tempDir, "Main.adl");
        var file2 = Path.Combine(tempDir, "Utils.adl");
        File.WriteAllText(file1, "class Main extends Utils {}");
        File.WriteAllText(file2, "class Utils {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("Main"))
            {
                ast.Classes.Add(new ClassDeclaration 
                { 
                    Name = "Main",
                    SuperClass = "Utils"
                });
            }
            else
            {
                ast.Classes.Add(new ClassDeclaration { Name = "Utils" });
            }
            return ast;
        });
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(file2, result[0]); // Utils should come first
        Assert.Equal(file1, result[1]); // Main depends on Utils
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_ChainedDependencies_ReturnsCorrectOrder()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var fileA = Path.Combine(tempDir, "A.adl");
        var fileB = Path.Combine(tempDir, "B.adl");
        var fileC = Path.Combine(tempDir, "C.adl");
        File.WriteAllText(fileA, "class A extends B {}");
        File.WriteAllText(fileB, "class B extends C {}");
        File.WriteAllText(fileC, "class C {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("A.adl"))
            {
                ast.Classes.Add(new ClassDeclaration 
                { 
                    Name = "A",
                    SuperClass = "B"
                });
            }
            else if (file.Contains("B.adl"))
            {
                ast.Classes.Add(new ClassDeclaration 
                { 
                    Name = "B",
                    SuperClass = "C"
                });
            }
            else
            {
                ast.Classes.Add(new ClassDeclaration { Name = "C" });
            }
            return ast;
        });
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(fileC, result[0]); // C has no dependencies
        Assert.Equal(fileB, result[1]); // B depends on C
        Assert.Equal(fileA, result[2]); // A depends on B
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_CircularDependency_ThrowsException()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var fileA = Path.Combine(tempDir, "A.adl");
        var fileB = Path.Combine(tempDir, "B.adl");
        File.WriteAllText(fileA, "class A extends B {}");
        File.WriteAllText(fileB, "class B extends A {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act & Assert
        var exception = Assert.Throws<CircularDependencyException>(() =>
        {
            analyzer.AnalyzeDependencies(file =>
            {
                var ast = new CompilationUnit { FileName = file };
                if (file.Contains("A.adl"))
                {
                    ast.Classes.Add(new ClassDeclaration 
                    { 
                        Name = "A",
                        SuperClass = "B"
                    });
                }
                else
                {
                    ast.Classes.Add(new ClassDeclaration 
                    { 
                        Name = "B",
                        SuperClass = "A"
                    });
                }
                return ast;
            });
        });
        
        Assert.Contains("Circular dependency detected", exception.Message);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_NamespaceReferences_ReturnsCorrectOrder()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var fileMain = Path.Combine(tempDir, "Main.adl");
        var fileUtils = Path.Combine(tempDir, "Utils.adl");
        File.WriteAllText(fileMain, "class Main { Utils.Helper h; }");
        File.WriteAllText(fileUtils, "namespace Utils { class Helper {} }");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("Main"))
            {
                var cls = new ClassDeclaration { Name = "Main" };
                cls.Fields.Add(new FieldDeclaration 
                { 
                    Name = "h",
                    FieldType = new TypeReference { Name = "Utils.Helper" }
                });
                ast.Classes.Add(cls);
            }
            else
            {
                var ns = new NamespaceDeclaration { Name = "Utils" };
                ns.Classes.Add(new ClassDeclaration { Name = "Helper" });
                ast.Namespace = ns;
            }
            return ast;
        });
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(fileUtils, result[0]); // Utils should come first
        Assert.Equal(fileMain, result[1]); // Main depends on Utils
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_FieldTypeReferences_ReturnsCorrectOrder()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var fileMain = Path.Combine(tempDir, "Main.adl");
        var fileData = Path.Combine(tempDir, "Data.adl");
        File.WriteAllText(fileMain, "class Main { Data data; }");
        File.WriteAllText(fileData, "class Data {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("Main"))
            {
                var cls = new ClassDeclaration { Name = "Main" };
                cls.Fields.Add(new FieldDeclaration 
                { 
                    Name = "data",
                    FieldType = new TypeReference { Name = "Data" }
                });
                ast.Classes.Add(cls);
            }
            else
            {
                ast.Classes.Add(new ClassDeclaration { Name = "Data" });
            }
            return ast;
        });
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(fileData, result[0]); // Data should come first
        Assert.Equal(fileMain, result[1]); // Main depends on Data
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_MethodParameterReferences_ReturnsCorrectOrder()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var fileMain = Path.Combine(tempDir, "Main.adl");
        var fileProcessor = Path.Combine(tempDir, "Processor.adl");
        File.WriteAllText(fileMain, "class Main { void process(Processor p) {} }");
        File.WriteAllText(fileProcessor, "class Processor {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("Main"))
            {
                var cls = new ClassDeclaration { Name = "Main" };
                var method = new MethodDeclaration 
                { 
                    Name = "process",
                    ReturnType = new TypeReference { Name = "void" }
                };
                method.Parameters.Add(new ParameterDeclaration 
                { 
                    Name = "p",
                    ParameterType = new TypeReference { Name = "Processor" }
                });
                cls.Methods.Add(method);
                ast.Classes.Add(cls);
            }
            else
            {
                ast.Classes.Add(new ClassDeclaration { Name = "Processor" });
            }
            return ast;
        });
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(fileProcessor, result[0]); // Processor should come first
        Assert.Equal(fileMain, result[1]); // Main depends on Processor
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_ComplexDependencyGraph_ReturnsValidOrder()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var fileA = Path.Combine(tempDir, "A.adl");
        var fileB = Path.Combine(tempDir, "B.adl");
        var fileC = Path.Combine(tempDir, "C.adl");
        var fileD = Path.Combine(tempDir, "D.adl");
        
        // Dependency graph: A -> B, A -> C, B -> D, C -> D
        File.WriteAllText(fileA, "class A extends B { C c; }");
        File.WriteAllText(fileB, "class B extends D {}");
        File.WriteAllText(fileC, "class C extends D {}");
        File.WriteAllText(fileD, "class D {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("A.adl"))
            {
                var cls = new ClassDeclaration 
                { 
                    Name = "A",
                    SuperClass = "B"
                };
                cls.Fields.Add(new FieldDeclaration 
                { 
                    Name = "c",
                    FieldType = new TypeReference { Name = "C" }
                });
                ast.Classes.Add(cls);
            }
            else if (file.Contains("B.adl"))
            {
                ast.Classes.Add(new ClassDeclaration 
                { 
                    Name = "B",
                    SuperClass = "D"
                });
            }
            else if (file.Contains("C.adl"))
            {
                ast.Classes.Add(new ClassDeclaration 
                { 
                    Name = "C",
                    SuperClass = "D"
                });
            }
            else
            {
                ast.Classes.Add(new ClassDeclaration { Name = "D" });
            }
            return ast;
        });
        
        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal(fileD, result[0]); // D has no dependencies
        
        // B and C both depend only on D, so they can be in any order
        var indexB = result.IndexOf(fileB);
        var indexC = result.IndexOf(fileC);
        var indexA = result.IndexOf(fileA);
        var indexD = result.IndexOf(fileD);
        
        Assert.True(indexD < indexB); // D before B
        Assert.True(indexD < indexC); // D before C
        Assert.True(indexB < indexA); // B before A
        Assert.True(indexC < indexA); // C before A
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void AnalyzeDependencies_SubdirectoryFiles_DiscoversAllFiles()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var subDir = Path.Combine(tempDir, "models");
        Directory.CreateDirectory(subDir);
        
        var fileMain = Path.Combine(tempDir, "Main.adl");
        var fileModel = Path.Combine(subDir, "Model.adl");
        File.WriteAllText(fileMain, "class Main { Model m; }");
        File.WriteAllText(fileModel, "class Model {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        var result = analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("Main"))
            {
                var cls = new ClassDeclaration { Name = "Main" };
                cls.Fields.Add(new FieldDeclaration 
                { 
                    Name = "m",
                    FieldType = new TypeReference { Name = "Model" }
                });
                ast.Classes.Add(cls);
            }
            else
            {
                ast.Classes.Add(new ClassDeclaration { Name = "Model" });
            }
            return ast;
        });
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(fileModel, result[0]); // Model should come first
        Assert.Equal(fileMain, result[1]); // Main depends on Model
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void GetDependencyGraph_ReturnsCorrectGraph()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var fileA = Path.Combine(tempDir, "A.adl");
        var fileB = Path.Combine(tempDir, "B.adl");
        File.WriteAllText(fileA, "class A extends B {}");
        File.WriteAllText(fileB, "class B {}");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            if (file.Contains("A.adl"))
            {
                ast.Classes.Add(new ClassDeclaration 
                { 
                    Name = "A",
                    SuperClass = "B"
                });
            }
            else
            {
                ast.Classes.Add(new ClassDeclaration { Name = "B" });
            }
            return ast;
        });
        
        var graph = analyzer.GetDependencyGraph();
        
        // Assert
        Assert.Equal(2, graph.Count);
        Assert.Contains(fileB, graph[fileA]); // A depends on B
        Assert.Empty(graph[fileB]); // B has no dependencies
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public void GetDefinedSymbols_ReturnsCorrectSymbols()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var fileUtils = Path.Combine(tempDir, "Utils.adl");
        File.WriteAllText(fileUtils, "namespace Utils { class Helper {} }");
        
        var analyzer = new DependencyAnalyzer(tempDir);
        
        // Act
        analyzer.AnalyzeDependencies(file =>
        {
            var ast = new CompilationUnit { FileName = file };
            var ns = new NamespaceDeclaration { Name = "Utils" };
            ns.Classes.Add(new ClassDeclaration { Name = "Helper" });
            ast.Namespace = ns;
            return ast;
        });
        
        var symbols = analyzer.GetDefinedSymbols();
        
        // Assert
        Assert.Single(symbols);
        Assert.Contains("Utils", symbols[fileUtils]);
        Assert.Contains("Utils.Helper", symbols[fileUtils]);
        Assert.Contains("Helper", symbols[fileUtils]);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
    
    private string CreateTempDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"adl_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }
}
