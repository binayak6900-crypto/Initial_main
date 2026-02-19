using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Type system for ADL language supporting primitives, classes, interfaces, pointers, and arrays.
/// Handles type compatibility checking and type inference.
/// Requirements: 4.2, 4.3, 4.6, 4.7
/// </summary>
public class TypeSystem
{
    private readonly HashSet<string> _primitiveTypes;
    private readonly HashSet<string> _numericTypes;
    private readonly HashSet<string> _integerTypes;
    private readonly Dictionary<string, TypeInfo> _registeredTypes;
    
    public TypeSystem()
    {
        _primitiveTypes = new HashSet<string>
        {
            "void", "bool", "boolean",
            "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong",
            "float", "double", "decimal",
            "char", "string"
        };
        
        _numericTypes = new HashSet<string>
        {
            "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong",
            "float", "double", "decimal"
        };
        
        _integerTypes = new HashSet<string>
        {
            "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong"
        };
        
        _registeredTypes = new Dictionary<string, TypeInfo>();
        
        // Register built-in types
        RegisterBuiltInTypes();
    }
    
    private void RegisterBuiltInTypes()
    {
        // Register primitive types
        foreach (var primType in _primitiveTypes)
        {
            _registeredTypes[primType] = new TypeInfo
            {
                Name = primType,
                Kind = TypeKind.Primitive,
                IsBuiltIn = true
            };
        }
        
        // Register common Java types
        RegisterType("String", TypeKind.Class, true);
        RegisterType("Object", TypeKind.Class, true);
        RegisterType("Integer", TypeKind.Class, true);
        RegisterType("Double", TypeKind.Class, true);
        RegisterType("Boolean", TypeKind.Class, true);
        
        // Register common C++ types
        RegisterType("std::string", TypeKind.Class, true);
        RegisterType("std::vector", TypeKind.Class, true);
        RegisterType("std::map", TypeKind.Class, true);
    }
    
    /// <summary>
    /// Registers a new type in the type system
    /// </summary>
    public void RegisterType(string name, TypeKind kind, bool isBuiltIn = false)
    {
        if (!_registeredTypes.ContainsKey(name))
        {
            _registeredTypes[name] = new TypeInfo
            {
                Name = name,
                Kind = kind,
                IsBuiltIn = isBuiltIn
            };
        }
    }
    
    /// <summary>
    /// Checks if a type is valid (exists in the type system)
    /// </summary>
    public bool IsValidType(TypeReference typeRef)
    {
        // Arrays and pointers of valid types are valid
        if (typeRef.IsArray || typeRef.IsPointer)
        {
            var baseType = new TypeReference { Name = typeRef.Name };
            return IsValidType(baseType);
        }
        
        return _registeredTypes.ContainsKey(typeRef.Name);
    }
    
    /// <summary>
    /// Checks if a type is a primitive type
    /// </summary>
    public bool IsPrimitive(TypeReference typeRef)
    {
        return _primitiveTypes.Contains(typeRef.Name);
    }
    
    /// <summary>
    /// Checks if a type is a numeric type
    /// </summary>
    public bool IsNumeric(TypeReference typeRef)
    {
        return _numericTypes.Contains(typeRef.Name);
    }
    
    /// <summary>
    /// Checks if a type is an integer type
    /// </summary>
    public bool IsInteger(TypeReference typeRef)
    {
        return _integerTypes.Contains(typeRef.Name);
    }
    
    /// <summary>
    /// Checks if a type is a boolean type
    /// </summary>
    public bool IsBoolean(TypeReference typeRef)
    {
        return typeRef.Name == "bool" || typeRef.Name == "boolean";
    }
    
    /// <summary>
    /// Checks if a type is a string type
    /// </summary>
    public bool IsString(TypeReference typeRef)
    {
        return typeRef.Name == "string" || typeRef.Name == "String" || typeRef.Name == "std::string";
    }
    
    /// <summary>
    /// Checks if sourceType can be assigned to targetType
    /// </summary>
    public bool IsAssignableFrom(TypeReference targetType, TypeReference sourceType)
    {
        // Exact match
        if (AreTypesEqual(targetType, sourceType))
        {
            return true;
        }
        
        // Null can be assigned to any reference type
        if (sourceType.Name == "null" && !IsPrimitive(targetType))
        {
            return true;
        }
        
        // Numeric conversions
        if (IsNumeric(targetType) && IsNumeric(sourceType))
        {
            return IsNumericConversionValid(targetType, sourceType);
        }
        
        // String conversions
        if (IsString(targetType) && IsString(sourceType))
        {
            return true;
        }
        
        // Boolean conversions
        if (IsBoolean(targetType) && IsBoolean(sourceType))
        {
            return true;
        }
        
        // Array covariance (simplified)
        if (targetType.IsArray && sourceType.IsArray)
        {
            return targetType.Name == sourceType.Name;
        }
        
        // Pointer compatibility
        if (targetType.IsPointer && sourceType.IsPointer)
        {
            return targetType.Name == sourceType.Name && 
                   targetType.PointerLevel == sourceType.PointerLevel;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if two types are equal
    /// </summary>
    public bool AreTypesEqual(TypeReference type1, TypeReference type2)
    {
        return type1.Name == type2.Name &&
               type1.IsArray == type2.IsArray &&
               type1.IsPointer == type2.IsPointer &&
               type1.PointerLevel == type2.PointerLevel;
    }
    
    /// <summary>
    /// Checks if a numeric conversion is valid
    /// </summary>
    private bool IsNumericConversionValid(TypeReference targetType, TypeReference sourceType)
    {
        // Define numeric type hierarchy (smaller to larger)
        var typeRank = new Dictionary<string, int>
        {
            { "byte", 1 }, { "sbyte", 1 },
            { "short", 2 }, { "ushort", 2 },
            { "int", 3 }, { "uint", 3 },
            { "long", 4 }, { "ulong", 4 },
            { "float", 5 },
            { "double", 6 },
            { "decimal", 7 }
        };
        
        if (!typeRank.ContainsKey(targetType.Name) || !typeRank.ContainsKey(sourceType.Name))
        {
            return false;
        }
        
        // Allow implicit conversion from smaller to larger types
        return typeRank[sourceType.Name] <= typeRank[targetType.Name];
    }
    
    /// <summary>
    /// Gets the result type of a binary operation
    /// </summary>
    public TypeReference GetBinaryOperationResultType(BinaryOperator op, TypeReference leftType, TypeReference rightType, SourceLocation location)
    {
        switch (op)
        {
            // Arithmetic operators
            case BinaryOperator.Add:
            case BinaryOperator.Subtract:
            case BinaryOperator.Multiply:
            case BinaryOperator.Divide:
            case BinaryOperator.Modulo:
                return GetArithmeticResultType(leftType, rightType, location);
            
            // Comparison operators
            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual:
            case BinaryOperator.Less:
            case BinaryOperator.Greater:
            case BinaryOperator.LessEqual:
            case BinaryOperator.GreaterEqual:
                return GetComparisonResultType(leftType, rightType, location);
            
            // Logical operators
            case BinaryOperator.LogicalAnd:
            case BinaryOperator.LogicalOr:
                return GetLogicalResultType(leftType, rightType, location);
            
            // Bitwise operators
            case BinaryOperator.BitwiseAnd:
            case BinaryOperator.BitwiseOr:
            case BinaryOperator.BitwiseXor:
            case BinaryOperator.LeftShift:
            case BinaryOperator.RightShift:
                return GetBitwiseResultType(leftType, rightType, location);
            
            default:
                throw new SemanticException($"Unknown binary operator: {op}", location);
        }
    }
    
    /// <summary>
    /// Gets the result type of a unary operation
    /// </summary>
    public TypeReference GetUnaryOperationResultType(UnaryOperator op, TypeReference operandType, SourceLocation location)
    {
        switch (op)
        {
            case UnaryOperator.Negate:
                if (!IsNumeric(operandType))
                {
                    throw new SemanticException($"Unary negation requires numeric type, got '{operandType.Name}'", location);
                }
                return operandType;
            
            case UnaryOperator.LogicalNot:
                if (!IsBoolean(operandType))
                {
                    throw new SemanticException($"Logical NOT requires boolean type, got '{operandType.Name}'", location);
                }
                return operandType;
            
            case UnaryOperator.BitwiseNot:
                if (!IsInteger(operandType))
                {
                    throw new SemanticException($"Bitwise NOT requires integer type, got '{operandType.Name}'", location);
                }
                return operandType;
            
            case UnaryOperator.PreIncrement:
            case UnaryOperator.PostIncrement:
            case UnaryOperator.PreDecrement:
            case UnaryOperator.PostDecrement:
                if (!IsNumeric(operandType))
                {
                    throw new SemanticException($"Increment/decrement requires numeric type, got '{operandType.Name}'", location);
                }
                return operandType;
            
            default:
                throw new SemanticException($"Unknown unary operator: {op}", location);
        }
    }
    
    /// <summary>
    /// Gets the type of a literal value
    /// </summary>
    public TypeReference GetLiteralType(object? value)
    {
        if (value == null)
        {
            return new TypeReference { Name = "null" };
        }
        
        return value switch
        {
            bool => new TypeReference { Name = "bool" },
            byte => new TypeReference { Name = "byte" },
            sbyte => new TypeReference { Name = "sbyte" },
            short => new TypeReference { Name = "short" },
            ushort => new TypeReference { Name = "ushort" },
            int => new TypeReference { Name = "int" },
            uint => new TypeReference { Name = "uint" },
            long => new TypeReference { Name = "long" },
            ulong => new TypeReference { Name = "ulong" },
            float => new TypeReference { Name = "float" },
            double => new TypeReference { Name = "double" },
            decimal => new TypeReference { Name = "decimal" },
            char => new TypeReference { Name = "char" },
            string => new TypeReference { Name = "string" },
            _ => new TypeReference { Name = "unknown" }
        };
    }
    
    private TypeReference GetArithmeticResultType(TypeReference leftType, TypeReference rightType, SourceLocation location)
    {
        // String concatenation
        if (IsString(leftType) || IsString(rightType))
        {
            return new TypeReference { Name = "string" };
        }
        
        if (!IsNumeric(leftType) || !IsNumeric(rightType))
        {
            throw new SemanticException(
                $"Arithmetic operation requires numeric types, got '{leftType.Name}' and '{rightType.Name}'",
                location
            );
        }
        
        // Return the wider type
        var typeRank = new Dictionary<string, int>
        {
            { "byte", 1 }, { "sbyte", 1 },
            { "short", 2 }, { "ushort", 2 },
            { "int", 3 }, { "uint", 3 },
            { "long", 4 }, { "ulong", 4 },
            { "float", 5 },
            { "double", 6 },
            { "decimal", 7 }
        };
        
        var leftRank = typeRank.GetValueOrDefault(leftType.Name, 0);
        var rightRank = typeRank.GetValueOrDefault(rightType.Name, 0);
        
        return leftRank >= rightRank ? leftType : rightType;
    }
    
    private TypeReference GetComparisonResultType(TypeReference leftType, TypeReference rightType, SourceLocation location)
    {
        // Comparison always returns boolean
        if (!IsNumeric(leftType) || !IsNumeric(rightType))
        {
            if (!AreTypesEqual(leftType, rightType))
            {
                throw new SemanticException(
                    $"Cannot compare incompatible types '{leftType.Name}' and '{rightType.Name}'",
                    location
                );
            }
        }
        
        return new TypeReference { Name = "bool" };
    }
    
    private TypeReference GetLogicalResultType(TypeReference leftType, TypeReference rightType, SourceLocation location)
    {
        if (!IsBoolean(leftType) || !IsBoolean(rightType))
        {
            throw new SemanticException(
                $"Logical operation requires boolean types, got '{leftType.Name}' and '{rightType.Name}'",
                location
            );
        }
        
        return new TypeReference { Name = "bool" };
    }
    
    private TypeReference GetBitwiseResultType(TypeReference leftType, TypeReference rightType, SourceLocation location)
    {
        if (!IsInteger(leftType) || !IsInteger(rightType))
        {
            throw new SemanticException(
                $"Bitwise operation requires integer types, got '{leftType.Name}' and '{rightType.Name}'",
                location
            );
        }
        
        // Return the wider type
        var typeRank = new Dictionary<string, int>
        {
            { "byte", 1 }, { "sbyte", 1 },
            { "short", 2 }, { "ushort", 2 },
            { "int", 3 }, { "uint", 3 },
            { "long", 4 }, { "ulong", 4 }
        };
        
        var leftRank = typeRank.GetValueOrDefault(leftType.Name, 0);
        var rightRank = typeRank.GetValueOrDefault(rightType.Name, 0);
        
        return leftRank >= rightRank ? leftType : rightType;
    }
}

/// <summary>
/// Information about a type in the type system
/// </summary>
public class TypeInfo
{
    public string Name { get; set; } = string.Empty;
    public TypeKind Kind { get; set; }
    public bool IsBuiltIn { get; set; }
    public List<string> Interfaces { get; set; } = new();
    public string? BaseType { get; set; }
}

/// <summary>
/// Kind of type
/// </summary>
public enum TypeKind
{
    Primitive,
    Class,
    Interface,
    Struct,
    Enum
}
