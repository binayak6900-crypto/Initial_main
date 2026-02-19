using SharedUtilities.Models;
using Xunit;

namespace SharedUtilities.Tests;

public class DataModelsTests
{
    [Fact]
    public void Project_CanBeCreatedWithDefaults()
    {
        // Act
        var project = new Project
        {
            Name = "TestProject",
            Path = "~/projects/test"
        };
        
        // Assert
        Assert.Equal("TestProject", project.Name);
        Assert.Equal("~/projects/test", project.Path);
        Assert.NotNull(project.Files);
        Assert.Empty(project.Files);
        Assert.NotNull(project.BuildConfig);
        Assert.Equal("15.0", project.TargetSDK);
    }
    
    [Fact]
    public void ADLFile_CanBeCreated()
    {
        // Act
        var file = new ADLFile
        {
            Name = "Main.adl",
            Path = "~/projects/test/Main.adl",
            Content = "public class Main { }"
        };
        
        // Assert
        Assert.Equal("Main.adl", file.Name);
        Assert.Equal("~/projects/test/Main.adl", file.Path);
        Assert.Equal("public class Main { }", file.Content);
        Assert.Equal(FileStatus.Unmodified, file.Status);
    }
    
    [Fact]
    public void BuildConfiguration_HasCorrectDefaults()
    {
        // Act
        var config = new BuildConfiguration();
        
        // Assert
        Assert.Equal("bin/output.apk", config.OutputPath);
        Assert.NotNull(config.CompilerFlags);
        Assert.Empty(config.CompilerFlags);
        Assert.Equal(OptimizationLevel.Debug, config.Optimization);
        Assert.Equal(TargetPlatform.Android, config.Platform);
        Assert.Equal(PackageMode.Standalone, config.PackageMode);
    }
    
    [Fact]
    public void Token_CanBeCreated()
    {
        // Act
        var token = new Token
        {
            Type = TokenType.Keyword,
            Value = "public",
            Location = new SourceLocation
            {
                File = "Main.adl",
                Line = 1,
                Column = 1
            }
        };
        
        // Assert
        Assert.Equal(TokenType.Keyword, token.Type);
        Assert.Equal("public", token.Value);
        Assert.Equal("Main.adl:1:1", token.Location.ToString());
    }
    
    [Fact]
    public void ClassDeclarationNode_CanBeCreated()
    {
        // Act
        var classNode = new ClassDeclarationNode
        {
            Name = "MainActivity",
            Namespace = "com.example",
            AccessModifier = AccessModifier.Public
        };
        
        // Assert
        Assert.Equal("MainActivity", classNode.Name);
        Assert.Equal("com.example", classNode.Namespace);
        Assert.Equal(AccessModifier.Public, classNode.AccessModifier);
        Assert.NotNull(classNode.Methods);
        Assert.Empty(classNode.Methods);
        Assert.NotNull(classNode.Fields);
        Assert.Empty(classNode.Fields);
    }
    
    [Fact]
    public void MethodDeclarationNode_CanBeCreated()
    {
        // Act
        var method = new MethodDeclarationNode
        {
            Name = "onCreate",
            IsStatic = false,
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeReferenceNode { TypeName = "void" }
        };
        
        // Assert
        Assert.Equal("onCreate", method.Name);
        Assert.False(method.IsStatic);
        Assert.Equal(AccessModifier.Public, method.AccessModifier);
        Assert.Equal("void", method.ReturnType.TypeName);
        Assert.NotNull(method.Parameters);
        Assert.Empty(method.Parameters);
    }
}
