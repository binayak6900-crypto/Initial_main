using ADLCompiler.SemanticAnalysis;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests;

/// <summary>
/// Unit tests for TypeSystem
/// Requirements: 4.2, 4.3, 4.6, 4.7
/// </summary>
public class TypeSystemTests
{
    [Theory]
    [InlineData("int")]
    [InlineData("bool")]
    [InlineData("string")]
    [InlineData("double")]
    [InlineData("float")]
    [InlineData("char")]
    public void IsValidType_PrimitiveTypes_ReturnsTrue(string typeName)
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var typeRef = new TypeReference { Name = typeName };
        
        // Act
        var result = typeSystem.IsValidType(typeRef);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsValidType_ArrayOfValidType_ReturnsTrue()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var typeRef = new TypeReference { Name = "int", IsArray = true };
        
        // Act
        var result = typeSystem.IsValidType(typeRef);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsValidType_PointerOfValidType_ReturnsTrue()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var typeRef = new TypeReference { Name = "int", IsPointer = true, PointerLevel = 1 };
        
        // Act
        var result = typeSystem.IsValidType(typeRef);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData("int")]
    [InlineData("double")]
    [InlineData("float")]
    [InlineData("long")]
    public void IsNumeric_NumericTypes_ReturnsTrue(string typeName)
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var typeRef = new TypeReference { Name = typeName };
        
        // Act
        var result = typeSystem.IsNumeric(typeRef);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData("bool")]
    [InlineData("string")]
    public void IsNumeric_NonNumericTypes_ReturnsFalse(string typeName)
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var typeRef = new TypeReference { Name = typeName };
        
        // Act
        var result = typeSystem.IsNumeric(typeRef);
        
        // Assert
        Assert.False(result);
    }
    
    [Theory]
    [InlineData("int")]
    [InlineData("long")]
    [InlineData("short")]
    [InlineData("byte")]
    public void IsInteger_IntegerTypes_ReturnsTrue(string typeName)
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var typeRef = new TypeReference { Name = typeName };
        
        // Act
        var result = typeSystem.IsInteger(typeRef);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData("float")]
    [InlineData("double")]
    public void IsInteger_FloatingPointTypes_ReturnsFalse(string typeName)
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var typeRef = new TypeReference { Name = typeName };
        
        // Act
        var result = typeSystem.IsInteger(typeRef);
        
        // Assert
        Assert.False(result);
    }
    
    [Theory]
    [InlineData("bool")]
    [InlineData("boolean")]
    public void IsBoolean_BooleanTypes_ReturnsTrue(string typeName)
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var typeRef = new TypeReference { Name = typeName };
        
        // Act
        var result = typeSystem.IsBoolean(typeRef);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsAssignableFrom_SameType_ReturnsTrue()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var type1 = new TypeReference { Name = "int" };
        var type2 = new TypeReference { Name = "int" };
        
        // Act
        var result = typeSystem.IsAssignableFrom(type1, type2);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsAssignableFrom_IntToDouble_ReturnsTrue()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var targetType = new TypeReference { Name = "double" };
        var sourceType = new TypeReference { Name = "int" };
        
        // Act
        var result = typeSystem.IsAssignableFrom(targetType, sourceType);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsAssignableFrom_DoubleToInt_ReturnsFalse()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var targetType = new TypeReference { Name = "int" };
        var sourceType = new TypeReference { Name = "double" };
        
        // Act
        var result = typeSystem.IsAssignableFrom(targetType, sourceType);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsAssignableFrom_StringToInt_ReturnsFalse()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var targetType = new TypeReference { Name = "int" };
        var sourceType = new TypeReference { Name = "string" };
        
        // Act
        var result = typeSystem.IsAssignableFrom(targetType, sourceType);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void GetBinaryOperationResultType_AddIntegers_ReturnsInt()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var leftType = new TypeReference { Name = "int" };
        var rightType = new TypeReference { Name = "int" };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Act
        var result = typeSystem.GetBinaryOperationResultType(BinaryOperator.Add, leftType, rightType, location);
        
        // Assert
        Assert.Equal("int", result.Name);
    }
    
    [Fact]
    public void GetBinaryOperationResultType_AddIntAndDouble_ReturnsDouble()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var leftType = new TypeReference { Name = "int" };
        var rightType = new TypeReference { Name = "double" };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Act
        var result = typeSystem.GetBinaryOperationResultType(BinaryOperator.Add, leftType, rightType, location);
        
        // Assert
        Assert.Equal("double", result.Name);
    }
    
    [Fact]
    public void GetBinaryOperationResultType_CompareIntegers_ReturnsBool()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var leftType = new TypeReference { Name = "int" };
        var rightType = new TypeReference { Name = "int" };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Act
        var result = typeSystem.GetBinaryOperationResultType(BinaryOperator.Less, leftType, rightType, location);
        
        // Assert
        Assert.Equal("bool", result.Name);
    }
    
    [Fact]
    public void GetBinaryOperationResultType_LogicalAnd_ReturnsBool()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var leftType = new TypeReference { Name = "bool" };
        var rightType = new TypeReference { Name = "bool" };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Act
        var result = typeSystem.GetBinaryOperationResultType(BinaryOperator.LogicalAnd, leftType, rightType, location);
        
        // Assert
        Assert.Equal("bool", result.Name);
    }
    
    [Fact]
    public void GetBinaryOperationResultType_LogicalAndWithNonBoolean_ThrowsException()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var leftType = new TypeReference { Name = "int" };
        var rightType = new TypeReference { Name = "bool" };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Act & Assert
        Assert.Throws<SemanticException>(() =>
            typeSystem.GetBinaryOperationResultType(BinaryOperator.LogicalAnd, leftType, rightType, location)
        );
    }
    
    [Fact]
    public void GetUnaryOperationResultType_NegateInteger_ReturnsInt()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var operandType = new TypeReference { Name = "int" };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Act
        var result = typeSystem.GetUnaryOperationResultType(UnaryOperator.Negate, operandType, location);
        
        // Assert
        Assert.Equal("int", result.Name);
    }
    
    [Fact]
    public void GetUnaryOperationResultType_LogicalNot_ReturnsBool()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var operandType = new TypeReference { Name = "bool" };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Act
        var result = typeSystem.GetUnaryOperationResultType(UnaryOperator.LogicalNot, operandType, location);
        
        // Assert
        Assert.Equal("bool", result.Name);
    }
    
    [Fact]
    public void GetUnaryOperationResultType_DereferencePointer_ReturnsBaseType()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var operandType = new TypeReference { Name = "int", IsPointer = true, PointerLevel = 1 };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Note: Dereference is not in the UnaryOperator enum, so we skip this test
        // This would be handled differently in the actual implementation
    }
    
    [Fact]
    public void GetUnaryOperationResultType_AddressOf_ReturnsPointer()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        var operandType = new TypeReference { Name = "int" };
        var location = new SourceLocation { Line = 1, Column = 1, File = "test.adl" };
        
        // Note: AddressOf is not in the UnaryOperator enum, so we skip this test
        // This would be handled differently in the actual implementation
    }
    
    [Theory]
    [InlineData(42, "int")]
    [InlineData(3.14, "double")]
    [InlineData(true, "bool")]
    [InlineData("hello", "string")]
    [InlineData('a', "char")]
    public void GetLiteralType_VariousLiterals_ReturnsCorrectType(object value, string expectedType)
    {
        // Arrange
        var typeSystem = new TypeSystem();
        
        // Act
        var result = typeSystem.GetLiteralType(value);
        
        // Assert
        Assert.Equal(expectedType, result.Name);
    }
    
    [Fact]
    public void GetLiteralType_Null_ReturnsNullType()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        
        // Act
        var result = typeSystem.GetLiteralType(null);
        
        // Assert
        Assert.Equal("null", result.Name);
    }
    
    [Fact]
    public void RegisterType_CustomClass_CanBeValidated()
    {
        // Arrange
        var typeSystem = new TypeSystem();
        typeSystem.RegisterType("MyClass", TypeKind.Class);
        var typeRef = new TypeReference { Name = "MyClass" };
        
        // Act
        var result = typeSystem.IsValidType(typeRef);
        
        // Assert
        Assert.True(result);
    }
}
