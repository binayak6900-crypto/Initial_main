namespace SharedUtilities.Models;

/// <summary>
/// Base class for all AST nodes
/// </summary>
public abstract class ASTNode
{
    public SourceLocation Location { get; set; } = new();
    public abstract NodeType Type { get; }
    
    /// <summary>
    /// Get all child nodes for traversal
    /// </summary>
    public virtual IEnumerable<ASTNode> Children
    {
        get
        {
            // Default implementation returns empty
            // Subclasses can override to return their children
            return Enumerable.Empty<ASTNode>();
        }
    }
}

public enum NodeType
{
    // Declarations
    CompilationUnit,
    ClassDeclaration,
    MethodDeclaration,
    FieldDeclaration,
    ParameterDeclaration,
    NamespaceDeclaration,
    
    // Statements
    BlockStatement,
    ExpressionStatement,
    ReturnStatement,
    IfStatement,
    WhileStatement,
    ForStatement,
    DoWhileStatement,
    SwitchStatement,
    CaseStatement,
    BreakStatement,
    ContinueStatement,
    VariableDeclarationStatement,
    ThrowStatement,
    
    // Expressions
    BinaryExpression,
    UnaryExpression,
    LiteralExpression,
    IdentifierExpression,
    MethodCallExpression,
    MemberAccessExpression,
    AssignmentExpression,
    ArrayAccessExpression,
    NewExpression,
    CastExpression,
    TernaryExpression,
    
    // Types
    TypeReference,
    ArrayTypeReference,
    PointerTypeReference
}

/// <summary>
/// Root node representing a complete source file
/// </summary>
public class CompilationUnit : ASTNode
{
    public override NodeType Type => NodeType.CompilationUnit;
    public string FileName { get; set; } = string.Empty;
    public NamespaceDeclaration? Namespace { get; set; }
    public string? PackageName { get; set; }
    public List<ClassDeclaration> Classes { get; set; } = new();
}

/// <summary>
/// Namespace declaration (C++ style)
/// </summary>
public class NamespaceDeclaration : ASTNode
{
    public override NodeType Type => NodeType.NamespaceDeclaration;
    public string Name { get; set; } = string.Empty;
    public List<ClassDeclaration> Classes { get; set; } = new();
    public List<MethodDeclaration> Functions { get; set; } = new();
}

/// <summary>
/// Class declaration
/// </summary>
public class ClassDeclaration : ASTNode
{
    public override NodeType Type => NodeType.ClassDeclaration;
    public string Name { get; set; } = string.Empty;
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public string? SuperClass { get; set; }
    public List<string> Interfaces { get; set; } = new();
    public List<FieldDeclaration> Fields { get; set; } = new();
    public List<MethodDeclaration> Methods { get; set; } = new();
    public bool IsAbstract { get; set; }
    public bool IsStatic { get; set; }
}

/// <summary>
/// Method declaration
/// </summary>
public class MethodDeclaration : ASTNode
{
    public override NodeType Type => NodeType.MethodDeclaration;
    public string Name { get; set; } = string.Empty;
    public TypeReference ReturnType { get; set; } = new();
    public List<ParameterDeclaration> Parameters { get; set; } = new();
    public BlockStatement? Body { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsStatic { get; set; }
    public bool IsNative { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
}

/// <summary>
/// Field declaration
/// </summary>
public class FieldDeclaration : ASTNode
{
    public override NodeType Type => NodeType.FieldDeclaration;
    public string Name { get; set; } = string.Empty;
    public TypeReference FieldType { get; set; } = new();
    public Expression? Initializer { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Private;
    public bool IsStatic { get; set; }
    public bool IsFinal { get; set; }
}

/// <summary>
/// Parameter declaration
/// </summary>
public class ParameterDeclaration : ASTNode
{
    public override NodeType Type => NodeType.ParameterDeclaration;
    public string Name { get; set; } = string.Empty;
    public TypeReference ParameterType { get; set; } = new();
}

/// <summary>
/// Type reference
/// </summary>
public class TypeReference : ASTNode
{
    public override NodeType Type => NodeType.TypeReference;
    public string Name { get; set; } = string.Empty;
    public bool IsArray { get; set; }
    public bool IsPointer { get; set; }
    public int PointerLevel { get; set; }
}

// Statements

/// <summary>
/// Block statement containing multiple statements
/// </summary>
public class BlockStatement : Statement
{
    public override NodeType Type => NodeType.BlockStatement;
    public List<Statement> Statements { get; set; } = new();
}

/// <summary>
/// Base class for all statements
/// </summary>
public abstract class Statement : ASTNode
{
}

/// <summary>
/// Expression statement
/// </summary>
public class ExpressionStatement : Statement
{
    public override NodeType Type => NodeType.ExpressionStatement;
    public Expression Expression { get; set; } = null!;
}

/// <summary>
/// Return statement
/// </summary>
public class ReturnStatement : Statement
{
    public override NodeType Type => NodeType.ReturnStatement;
    public Expression? Value { get; set; }
}

/// <summary>
/// If statement
/// </summary>
public class IfStatement : Statement
{
    public override NodeType Type => NodeType.IfStatement;
    public Expression Condition { get; set; } = null!;
    public Statement ThenBranch { get; set; } = null!;
    public Statement? ElseBranch { get; set; }
}

/// <summary>
/// While statement
/// </summary>
public class WhileStatement : Statement
{
    public override NodeType Type => NodeType.WhileStatement;
    public Expression Condition { get; set; } = null!;
    public Statement Body { get; set; } = null!;
}

/// <summary>
/// For statement
/// </summary>
public class ForStatement : Statement
{
    public override NodeType Type => NodeType.ForStatement;
    public Statement? Initializer { get; set; }
    public Expression? Condition { get; set; }
    public Expression? Increment { get; set; }
    public Statement Body { get; set; } = null!;
}

/// <summary>
/// Do-while statement
/// </summary>
public class DoWhileStatement : Statement
{
    public override NodeType Type => NodeType.DoWhileStatement;
    public Statement Body { get; set; } = null!;
    public Expression Condition { get; set; } = null!;
}

/// <summary>
/// Switch statement
/// </summary>
public class SwitchStatement : Statement
{
    public override NodeType Type => NodeType.SwitchStatement;
    public Expression Expression { get; set; } = null!;
    public List<CaseStatement> Cases { get; set; } = new();
}

/// <summary>
/// Case statement
/// </summary>
public class CaseStatement : Statement
{
    public override NodeType Type => NodeType.CaseStatement;
    public Expression? Value { get; set; } // null for default case
    public List<Statement> Statements { get; set; } = new();
}

/// <summary>
/// Break statement
/// </summary>
public class BreakStatement : Statement
{
    public override NodeType Type => NodeType.BreakStatement;
}

/// <summary>
/// Continue statement
/// </summary>
public class ContinueStatement : Statement
{
    public override NodeType Type => NodeType.ContinueStatement;
}

/// <summary>
/// Variable declaration statement
/// </summary>
public class VariableDeclarationStatement : Statement
{
    public override NodeType Type => NodeType.VariableDeclarationStatement;
    public TypeReference VariableType { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public Expression? Initializer { get; set; }
    public bool NeedsMemoryManagement { get; set; } = false;
}

/// <summary>
/// Throw statement
/// </summary>
public class ThrowStatement : Statement
{
    public override NodeType Type => NodeType.ThrowStatement;
    public Expression Exception { get; set; } = null!;
}

// Expressions

/// <summary>
/// Base class for all expressions
/// </summary>
public abstract class Expression : ASTNode
{
    public TypeReference? EvaluatedType { get; set; }
}

/// <summary>
/// Binary expression (e.g., a + b)
/// </summary>
public class BinaryExpression : Expression
{
    public override NodeType Type => NodeType.BinaryExpression;
    public Expression Left { get; set; } = null!;
    public BinaryOperator Operator { get; set; }
    public Expression Right { get; set; } = null!;
}

/// <summary>
/// Unary expression (e.g., -a, !b)
/// </summary>
public class UnaryExpression : Expression
{
    public override NodeType Type => NodeType.UnaryExpression;
    public UnaryOperator Operator { get; set; }
    public Expression Operand { get; set; } = null!;
}

/// <summary>
/// Literal expression (e.g., 42, "hello", true)
/// </summary>
public class LiteralExpression : Expression
{
    public override NodeType Type => NodeType.LiteralExpression;
    public object? Value { get; set; }
    public LiteralType LiteralType { get; set; }
}

/// <summary>
/// Identifier expression (e.g., variable name)
/// </summary>
public class IdentifierExpression : Expression
{
    public override NodeType Type => NodeType.IdentifierExpression;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Method call expression
/// </summary>
public class MethodCallExpression : Expression
{
    public override NodeType Type => NodeType.MethodCallExpression;
    public Expression? Target { get; set; } // null for static or local calls
    public string MethodName { get; set; } = string.Empty;
    public List<Expression> Arguments { get; set; } = new();
}

/// <summary>
/// Member access expression (e.g., obj.field)
/// </summary>
public class MemberAccessExpression : Expression
{
    public override NodeType Type => NodeType.MemberAccessExpression;
    public Expression Target { get; set; } = null!;
    public string MemberName { get; set; } = string.Empty;
}

/// <summary>
/// Assignment expression
/// </summary>
public class AssignmentExpression : Expression
{
    public override NodeType Type => NodeType.AssignmentExpression;
    public Expression Target { get; set; } = null!;
    public Expression Value { get; set; } = null!;
}

/// <summary>
/// Array access expression (e.g., arr[0])
/// </summary>
public class ArrayAccessExpression : Expression
{
    public override NodeType Type => NodeType.ArrayAccessExpression;
    public Expression Array { get; set; } = null!;
    public Expression Index { get; set; } = null!;
}

/// <summary>
/// New expression (object creation)
/// </summary>
public class NewExpression : Expression
{
    public override NodeType Type => NodeType.NewExpression;
    public TypeReference TypeToCreate { get; set; } = new();
    public List<Expression> Arguments { get; set; } = new();
    public bool IsArray { get; set; }
}

/// <summary>
/// Cast expression
/// </summary>
public class CastExpression : Expression
{
    public override NodeType Type => NodeType.CastExpression;
    public TypeReference TargetType { get; set; } = new();
    public Expression Expression { get; set; } = null!;
}

/// <summary>
/// Ternary expression (condition ? trueExpr : falseExpr)
/// </summary>
public class TernaryExpression : Expression
{
    public override NodeType Type => NodeType.TernaryExpression;
    public Expression Condition { get; set; } = null!;
    public Expression TrueExpression { get; set; } = null!;
    public Expression FalseExpression { get; set; } = null!;
}

// Enums

public enum AccessModifier
{
    Public,
    Private,
    Protected
}

public enum BinaryOperator
{
    Add, Subtract, Multiply, Divide, Modulo,
    Equal, NotEqual, Less, Greater, LessEqual, GreaterEqual,
    LogicalAnd, LogicalOr,
    BitwiseAnd, BitwiseOr, BitwiseXor, LeftShift, RightShift
}

public enum UnaryOperator
{
    Negate, LogicalNot, BitwiseNot, PreIncrement, PostIncrement, PreDecrement, PostDecrement
}

public enum LiteralType
{
    Integer, Float, String, Char, Boolean, Null
}
