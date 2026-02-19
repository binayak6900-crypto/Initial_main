namespace SharedUtilities.Models;

/// <summary>
/// Represents a lexical token from ADL source code
/// </summary>
public class Token
{
    public TokenType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public SourceLocation Location { get; set; } = new();
}

public enum TokenType
{
    // Java Keywords
    Abstract, Assert, Boolean, Break, Byte, Case, Catch, Char, Class, Const,
    Continue, Default, Do, Double, Else, Enum, Extends, Final, Finally, Float,
    For, Goto, If, Implements, Import, Instanceof, Int, Interface, Long,
    Native, New, Package, Private, Protected, Public, Return, Short, Static,
    Strictfp, Super, Switch, Synchronized, This, Throw, Throws, Transient,
    Try, Void, Volatile, While,
    
    // C++ Keywords (additional to Java)
    Asm, Auto, Explicit, Export, Extern, Friend, Inline, Mutable, Namespace,
    Operator, Register, Reinterpret_Cast, Signed, Sizeof, Static_Cast, Struct,
    Template, Typedef, Typename, Union, Unsigned, Using, Virtual, Wchar_T,
    Dynamic_Cast, Const_Cast, Typeid, And, Or, Not, Xor, Bitand, Bitor, Compl,
    And_Eq, Or_Eq, Xor_Eq, Not_Eq,
    
    // Common literals
    True, False, Null, Nullptr,
    
    // Identifiers and literals
    Identifier,
    IntegerLiteral,
    FloatLiteral,
    StringLiteral,
    CharLiteral,
    
    // Operators
    Plus, Minus, Star, Slash, Percent,
    Equal, NotEqual, Less, Greater, LessEqual, GreaterEqual,
    LogicalAnd, LogicalOr, LogicalNot,
    BitwiseAnd, BitwiseOr, BitwiseXor, BitwiseNot, LeftShift, RightShift,
    Assign, PlusAssign, MinusAssign, StarAssign, SlashAssign, PercentAssign,
    AndAssign, OrAssign, XorAssign, LeftShiftAssign, RightShiftAssign,
    Increment, Decrement,
    Question, Arrow, DoubleColon,
    
    // Punctuation
    LeftParen, RightParen,
    LeftBrace, RightBrace,
    LeftBracket, RightBracket,
    Semicolon, Comma, Dot, Colon,
    
    // Preprocessor
    Hash,
    
    // Special
    Comment,
    Whitespace,
    EndOfFile,
    Unknown
}

public class SourceLocation
{
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    
    public override string ToString() => $"{File}:{Line}:{Column}";
}
