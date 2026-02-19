using ADLCompiler.SemanticAnalysis;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests;

/// <summary>
/// Unit tests for TypeChecker
/// Requirements: 4.2, 4.3, 4.6, 4.7
/// </summary>
public class TypeCheckerTests
{
    private static TypeChecker CreateTypeChecker()
    {
        var symbolTable = new SymbolTable();
        return new TypeChecker(symbolTable);
    }
    
    private static CompilationUnit CreateSimpleCompilationUnit()
    {
        return new CompilationUnit
        {
            Classes = new List<ClassDeclaration>(),
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
        };
    }
    
    [Fact]
    public void Check_EmptyCompilationUnit_ReturnsTrue()
    {
        // Arrange
        var typeChecker = CreateTypeChecker();
        var compilationUnit = CreateSimpleCompilationUnit();
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void Check_ClassWithValidField_ReturnsTrue()
    {
        // Arrange
        var typeChecker = CreateTypeChecker();
        var compilationUnit = CreateSimpleCompilationUnit();
        
        var classDecl = new ClassDeclaration
        {
            Name = "TestClass",
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" },
            Fields = new List<FieldDeclaration>
            {
                new FieldDeclaration
                {
                    Name = "count",
                    FieldType = new TypeReference { Name = "int" },
                    Location = new SourceLocation { Line = 2, Column = 5, File = "test.adl" }
                }
            }
        };
        
        compilationUnit.Classes.Add(classDecl);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void Check_MethodWithValidReturnType_ReturnsTrue()
    {
        // Arrange
        var typeChecker = CreateTypeChecker();
        var compilationUnit = CreateSimpleCompilationUnit();
        
        var classDecl = new ClassDeclaration
        {
            Name = "TestClass",
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" },
            Methods = new List<MethodDeclaration>
            {
                new MethodDeclaration
                {
                    Name = "getValue",
                    ReturnType = new TypeReference { Name = "int" },
                    Parameters = new List<ParameterDeclaration>(),
                    Location = new SourceLocation { Line = 2, Column = 5, File = "test.adl" }
                }
            }
        };
        
        compilationUnit.Classes.Add(classDecl);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void Check_VariableDeclarationWithInferredType_InfersCorrectly()
    {
        // Arrange
        var symbolTable = new SymbolTable();
        var typeChecker = new TypeChecker(symbolTable);
        
        var varDecl = new VariableDeclarationStatement
        {
            Name = "x",
            VariableType = new TypeReference { Name = "var" },
            Initializer = new LiteralExpression
            {
                Value = 42,
                Location = new SourceLocation { Line = 1, Column = 15, File = "test.adl" }
            },
            Location = new SourceLocation { Line = 1, Column = 5, File = "test.adl" }
        };
        
        // Act
        var compilationUnit = new CompilationUnit
        {
            Classes = new List<ClassDeclaration>
            {
                new ClassDeclaration
                {
                    Name = "Test",
                    Methods = new List<MethodDeclaration>
                    {
                        new MethodDeclaration
                        {
                            Name = "test",
                            ReturnType = new TypeReference { Name = "void" },
                            Body = new BlockStatement
                            {
                                Statements = new List<Statement> { varDecl }
                            },
                            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                        }
                    },
                    Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                }
            },
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
        };
        
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Equal("int", varDecl.VariableType.Name);
    }
    
    [Fact]
    public void Check_VariableDeclarationWithInferredTypeNoInitializer_ReturnsError()
    {
        // Arrange
        var symbolTable = new SymbolTable();
        var typeChecker = new TypeChecker(symbolTable);
        
        var varDecl = new VariableDeclarationStatement
        {
            Name = "x",
            VariableType = new TypeReference { Name = "var" },
            Initializer = null,
            Location = new SourceLocation { Line = 1, Column = 5, File = "test.adl" }
        };
        
        var compilationUnit = new CompilationUnit
        {
            Classes = new List<ClassDeclaration>
            {
                new ClassDeclaration
                {
                    Name = "Test",
                    Methods = new List<MethodDeclaration>
                    {
                        new MethodDeclaration
                        {
                            Name = "test",
                            ReturnType = new TypeReference { Name = "void" },
                            Body = new BlockStatement
                            {
                                Statements = new List<Statement> { varDecl }
                            },
                            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                        }
                    },
                    Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                }
            },
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
        };
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.False(result);
        Assert.Single(typeChecker.Errors);
        Assert.Contains("must have an initializer", typeChecker.Errors[0].Message);
    }
    
    [Fact]
    public void Check_AssignmentWithIncompatibleTypes_ReturnsError()
    {
        // Arrange
        var symbolTable = new SymbolTable();
        symbolTable.Define("x", new Symbol(
            "x",
            new TypeReference { Name = "int" },
            SymbolKind.Variable,
            new SourceLocation { Line = 1, Column = 5, File = "test.adl" }
        ));
        
        var typeChecker = new TypeChecker(symbolTable);
        
        var varDecl = new VariableDeclarationStatement
        {
            Name = "y",
            VariableType = new TypeReference { Name = "int" },
            Initializer = new LiteralExpression
            {
                Value = "hello",
                Location = new SourceLocation { Line = 2, Column = 15, File = "test.adl" }
            },
            Location = new SourceLocation { Line = 2, Column = 5, File = "test.adl" }
        };
        
        var compilationUnit = new CompilationUnit
        {
            Classes = new List<ClassDeclaration>
            {
                new ClassDeclaration
                {
                    Name = "Test",
                    Methods = new List<MethodDeclaration>
                    {
                        new MethodDeclaration
                        {
                            Name = "test",
                            ReturnType = new TypeReference { Name = "void" },
                            Body = new BlockStatement
                            {
                                Statements = new List<Statement> { varDecl }
                            },
                            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                        }
                    },
                    Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                }
            },
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
        };
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.False(result);
        Assert.Single(typeChecker.Errors);
        Assert.Contains("Cannot assign", typeChecker.Errors[0].Message);
    }
    
    [Fact]
    public void Check_ReturnStatementWithWrongType_ReturnsError()
    {
        // Arrange
        var symbolTable = new SymbolTable();
        var typeChecker = new TypeChecker(symbolTable);
        
        var returnStmt = new ReturnStatement
        {
            Value = new LiteralExpression
            {
                Value = "hello",
                Location = new SourceLocation { Line = 2, Column = 12, File = "test.adl" }
            },
            Location = new SourceLocation { Line = 2, Column = 5, File = "test.adl" }
        };
        
        var compilationUnit = new CompilationUnit
        {
            Classes = new List<ClassDeclaration>
            {
                new ClassDeclaration
                {
                    Name = "Test",
                    Methods = new List<MethodDeclaration>
                    {
                        new MethodDeclaration
                        {
                            Name = "getValue",
                            ReturnType = new TypeReference { Name = "int" },
                            Body = new BlockStatement
                            {
                                Statements = new List<Statement> { returnStmt }
                            },
                            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                        }
                    },
                    Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                }
            },
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
        };
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.False(result);
        Assert.Single(typeChecker.Errors);
        Assert.Contains("Cannot return", typeChecker.Errors[0].Message);
    }
    
    [Fact]
    public void Check_IfStatementWithNonBooleanCondition_ReturnsError()
    {
        // Arrange
        var symbolTable = new SymbolTable();
        var typeChecker = new TypeChecker(symbolTable);
        
        var ifStmt = new IfStatement
        {
            Condition = new LiteralExpression
            {
                Value = 42,
                Location = new SourceLocation { Line = 1, Column = 8, File = "test.adl" }
            },
            ThenBranch = new BlockStatement
            {
                Statements = new List<Statement>()
            },
            Location = new SourceLocation { Line = 1, Column = 5, File = "test.adl" }
        };
        
        var compilationUnit = new CompilationUnit
        {
            Classes = new List<ClassDeclaration>
            {
                new ClassDeclaration
                {
                    Name = "Test",
                    Methods = new List<MethodDeclaration>
                    {
                        new MethodDeclaration
                        {
                            Name = "test",
                            ReturnType = new TypeReference { Name = "void" },
                            Body = new BlockStatement
                            {
                                Statements = new List<Statement> { ifStmt }
                            },
                            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                        }
                    },
                    Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                }
            },
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
        };
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.False(result);
        Assert.Single(typeChecker.Errors);
        Assert.Contains("must be boolean", typeChecker.Errors[0].Message);
    }
    
    [Fact]
    public void Check_ArrayAccessWithNonIntegerIndex_ReturnsError()
    {
        // Arrange
        var symbolTable = new SymbolTable();
        symbolTable.Define("arr", new Symbol(
            "arr",
            new TypeReference { Name = "int", IsArray = true },
            SymbolKind.Variable,
            new SourceLocation { Line = 1, Column = 5, File = "test.adl" }
        ));
        
        var typeChecker = new TypeChecker(symbolTable);
        
        var arrayAccess = new ArrayAccessExpression
        {
            Array = new IdentifierExpression
            {
                Name = "arr",
                Location = new SourceLocation { Line = 2, Column = 5, File = "test.adl" }
            },
            Index = new LiteralExpression
            {
                Value = "hello",
                Location = new SourceLocation { Line = 2, Column = 9, File = "test.adl" }
            },
            Location = new SourceLocation { Line = 2, Column = 5, File = "test.adl" }
        };
        
        var exprStmt = new ExpressionStatement
        {
            Expression = arrayAccess,
            Location = new SourceLocation { Line = 2, Column = 5, File = "test.adl" }
        };
        
        var compilationUnit = new CompilationUnit
        {
            Classes = new List<ClassDeclaration>
            {
                new ClassDeclaration
                {
                    Name = "Test",
                    Methods = new List<MethodDeclaration>
                    {
                        new MethodDeclaration
                        {
                            Name = "test",
                            ReturnType = new TypeReference { Name = "void" },
                            Body = new BlockStatement
                            {
                                Statements = new List<Statement> { exprStmt }
                            },
                            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                        }
                    },
                    Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
                }
            },
            Location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
        };
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.False(result);
        Assert.Single(typeChecker.Errors);
        Assert.Contains("must be integer", typeChecker.Errors[0].Message);
    }
}
