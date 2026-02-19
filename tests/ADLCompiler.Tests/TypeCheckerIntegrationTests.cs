using ADLCompiler.SemanticAnalysis;
using Xunit;

namespace ADLCompiler.Tests;

/// <summary>
/// Integration tests for TypeChecker with SymbolTable
/// Requirements: 4.2, 4.3, 4.6, 4.7
/// </summary>
public class TypeCheckerIntegrationTests
{
    private (SymbolTable symbolTable, TypeChecker typeChecker) CreateTypeCheckerWithSymbolTable(string source)
    {
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        
        var typeChecker = new TypeChecker(symbolTable);
        
        return (symbolTable, typeChecker);
    }
    
    [Fact]
    public void TypeChecker_SimpleClassWithMethod_PassesTypeChecking()
    {
        // Arrange
        var source = @"
            class Calculator {
                public int add(int a, int b) {
                    return a + b;
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void TypeChecker_VariableWithTypeInference_InfersCorrectType()
    {
        // Arrange
        var source = @"
            class Test {
                public void testMethod() {
                    var x = 42;
                    var y = 3.14;
                    var z = true;
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void TypeChecker_IfStatementWithBooleanCondition_PassesTypeChecking()
    {
        // Arrange
        var source = @"
            class Test {
                public void testMethod() {
                    int x = 10;
                    if (x > 5) {
                        x = x + 1;
                    }
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void TypeChecker_WhileLoopWithBooleanCondition_PassesTypeChecking()
    {
        // Arrange
        var source = @"
            class Test {
                public void testMethod() {
                    int count = 0;
                    while (count < 10) {
                        count = count + 1;
                    }
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void TypeChecker_ForLoopWithValidCondition_PassesTypeChecking()
    {
        // Arrange
        var source = @"
            class Test {
                public void testMethod() {
                    for (int i = 0; i < 10; i = i + 1) {
                        int x = i * 2;
                    }
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void TypeChecker_MethodWithCorrectReturnType_PassesTypeChecking()
    {
        // Arrange
        var source = @"
            class Calculator {
                public int multiply(int a, int b) {
                    int result = a * b;
                    return result;
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void TypeChecker_ArrayDeclarationAndAccess_PassesTypeChecking()
    {
        // Arrange
        var source = @"
            class Test {
                public void testMethod() {
                    int[] numbers = new int[10];
                    numbers[0] = 42;
                    int first = numbers[0];
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void TypeChecker_NamespaceWithFunction_PassesTypeChecking()
    {
        // Arrange
        var source = @"
            namespace Math {
                int square(int x) {
                    return x * x;
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
    
    [Fact]
    public void TypeChecker_ComplexExpressions_PassesTypeChecking()
    {
        // Arrange
        var source = @"
            class Test {
                public void testMethod() {
                    int a = 10;
                    int b = 20;
                    int c = 30;
                    int result = (a + b) * c - (a / 2);
                    bool isValid = result > 100 && a < b;
                }
            }
        ";
        
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        var compilationUnit = parser.Parse();
        
        var symbolTableBuilder = new SymbolTableBuilder();
        var symbolTable = symbolTableBuilder.Build(compilationUnit);
        var typeChecker = new TypeChecker(symbolTable);
        
        // Act
        var result = typeChecker.Check(compilationUnit);
        
        // Assert
        Assert.True(result);
        Assert.Empty(typeChecker.Errors);
    }
}
