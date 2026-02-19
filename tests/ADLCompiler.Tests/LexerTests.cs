using ADLCompiler;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenize_EmptySource_ReturnsOnlyEOF()
    {
        var lexer = new Lexer("", "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Single(tokens);
        Assert.Equal(TokenType.EndOfFile, tokens[0].Type);
    }
    
    [Fact]
    public void Tokenize_JavaKeywords_RecognizesAllKeywords()
    {
        var source = "public class void int if else for while return";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Public, tokens[0].Type);
        Assert.Equal(TokenType.Class, tokens[1].Type);
        Assert.Equal(TokenType.Void, tokens[2].Type);
        Assert.Equal(TokenType.Int, tokens[3].Type);
        Assert.Equal(TokenType.If, tokens[4].Type);
        Assert.Equal(TokenType.Else, tokens[5].Type);
        Assert.Equal(TokenType.For, tokens[6].Type);
        Assert.Equal(TokenType.While, tokens[7].Type);
        Assert.Equal(TokenType.Return, tokens[8].Type);
    }
    
    [Fact]
    public void Tokenize_CppKeywords_RecognizesAllKeywords()
    {
        var source = "namespace using template typename virtual struct";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Namespace, tokens[0].Type);
        Assert.Equal(TokenType.Using, tokens[1].Type);
        Assert.Equal(TokenType.Template, tokens[2].Type);
        Assert.Equal(TokenType.Typename, tokens[3].Type);
        Assert.Equal(TokenType.Virtual, tokens[4].Type);
        Assert.Equal(TokenType.Struct, tokens[5].Type);
    }
    
    [Fact]
    public void Tokenize_Identifiers_RecognizesIdentifiers()
    {
        var source = "myVariable _privateVar MyClass123";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("myVariable", tokens[0].Value);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("_privateVar", tokens[1].Value);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
        Assert.Equal("MyClass123", tokens[2].Value);
    }
    
    [Fact]
    public void Tokenize_IntegerLiterals_RecognizesIntegers()
    {
        var source = "42 0 12345 100L";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.IntegerLiteral, tokens[0].Type);
        Assert.Equal("42", tokens[0].Value);
        Assert.Equal(TokenType.IntegerLiteral, tokens[1].Type);
        Assert.Equal("0", tokens[1].Value);
        Assert.Equal(TokenType.IntegerLiteral, tokens[2].Type);
        Assert.Equal("12345", tokens[2].Value);
        Assert.Equal(TokenType.IntegerLiteral, tokens[3].Type);
        Assert.Equal("100L", tokens[3].Value);
    }
    
    [Fact]
    public void Tokenize_FloatLiterals_RecognizesFloats()
    {
        var source = "3.14 0.5 2.0f 1.5e10 2.5E-3";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.FloatLiteral, tokens[0].Type);
        Assert.Equal("3.14", tokens[0].Value);
        Assert.Equal(TokenType.FloatLiteral, tokens[1].Type);
        Assert.Equal("0.5", tokens[1].Value);
        Assert.Equal(TokenType.FloatLiteral, tokens[2].Type);
        Assert.Equal("2.0f", tokens[2].Value);
        Assert.Equal(TokenType.FloatLiteral, tokens[3].Type);
        Assert.Equal("1.5e10", tokens[3].Value);
        Assert.Equal(TokenType.FloatLiteral, tokens[4].Type);
        Assert.Equal("2.5E-3", tokens[4].Value);
    }
    
    [Fact]
    public void Tokenize_StringLiterals_RecognizesStrings()
    {
        var source = "\"hello\" \"world with spaces\" \"escaped\\\"quote\"";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.StringLiteral, tokens[0].Type);
        Assert.Equal("\"hello\"", tokens[0].Value);
        Assert.Equal(TokenType.StringLiteral, tokens[1].Type);
        Assert.Equal("\"world with spaces\"", tokens[1].Value);
        Assert.Equal(TokenType.StringLiteral, tokens[2].Type);
        Assert.Equal("\"escaped\\\"quote\"", tokens[2].Value);
    }
    
    [Fact]
    public void Tokenize_CharLiterals_RecognizesChars()
    {
        var source = "'a' 'Z' '\\n' '\\t'";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.CharLiteral, tokens[0].Type);
        Assert.Equal("'a'", tokens[0].Value);
        Assert.Equal(TokenType.CharLiteral, tokens[1].Type);
        Assert.Equal("'Z'", tokens[1].Value);
        Assert.Equal(TokenType.CharLiteral, tokens[2].Type);
        Assert.Equal("'\\n'", tokens[2].Value);
        Assert.Equal(TokenType.CharLiteral, tokens[3].Type);
        Assert.Equal("'\\t'", tokens[3].Value);
    }
    
    [Fact]
    public void Tokenize_LineComment_RecognizesJavaStyleComment()
    {
        var source = "int x; // This is a comment\nint y;";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Int, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal(TokenType.Semicolon, tokens[2].Type);
        Assert.Equal(TokenType.Comment, tokens[3].Type);
        Assert.Equal("// This is a comment", tokens[3].Value);
        Assert.Equal(TokenType.Int, tokens[4].Type);
    }
    
    [Fact]
    public void Tokenize_BlockComment_RecognizesCppStyleComment()
    {
        var source = "int x; /* This is a\nmulti-line comment */ int y;";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Int, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal(TokenType.Semicolon, tokens[2].Type);
        Assert.Equal(TokenType.Comment, tokens[3].Type);
        Assert.Contains("multi-line comment", tokens[3].Value);
        Assert.Equal(TokenType.Int, tokens[4].Type);
    }
    
    [Fact]
    public void Tokenize_Operators_RecognizesAllOperators()
    {
        var source = "+ - * / % == != < > <= >= && || ! & | ^ ~ ++ -- += -= *= /= %= &= |= ^= << >> <<= >>=";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Plus, tokens[0].Type);
        Assert.Equal(TokenType.Minus, tokens[1].Type);
        Assert.Equal(TokenType.Star, tokens[2].Type);
        Assert.Equal(TokenType.Slash, tokens[3].Type);
        Assert.Equal(TokenType.Percent, tokens[4].Type);
        Assert.Equal(TokenType.Equal, tokens[5].Type);
        Assert.Equal(TokenType.NotEqual, tokens[6].Type);
        Assert.Equal(TokenType.Less, tokens[7].Type);
        Assert.Equal(TokenType.Greater, tokens[8].Type);
        Assert.Equal(TokenType.LessEqual, tokens[9].Type);
        Assert.Equal(TokenType.GreaterEqual, tokens[10].Type);
        Assert.Equal(TokenType.LogicalAnd, tokens[11].Type);
        Assert.Equal(TokenType.LogicalOr, tokens[12].Type);
        Assert.Equal(TokenType.LogicalNot, tokens[13].Type);
        Assert.Equal(TokenType.BitwiseAnd, tokens[14].Type);
        Assert.Equal(TokenType.BitwiseOr, tokens[15].Type);
        Assert.Equal(TokenType.BitwiseXor, tokens[16].Type);
        Assert.Equal(TokenType.BitwiseNot, tokens[17].Type);
        Assert.Equal(TokenType.Increment, tokens[18].Type);
        Assert.Equal(TokenType.Decrement, tokens[19].Type);
        Assert.Equal(TokenType.PlusAssign, tokens[20].Type);
        Assert.Equal(TokenType.MinusAssign, tokens[21].Type);
        Assert.Equal(TokenType.StarAssign, tokens[22].Type);
        Assert.Equal(TokenType.SlashAssign, tokens[23].Type);
        Assert.Equal(TokenType.PercentAssign, tokens[24].Type);
        Assert.Equal(TokenType.AndAssign, tokens[25].Type);
        Assert.Equal(TokenType.OrAssign, tokens[26].Type);
        Assert.Equal(TokenType.XorAssign, tokens[27].Type);
        Assert.Equal(TokenType.LeftShift, tokens[28].Type);
        Assert.Equal(TokenType.RightShift, tokens[29].Type);
        Assert.Equal(TokenType.LeftShiftAssign, tokens[30].Type);
        Assert.Equal(TokenType.RightShiftAssign, tokens[31].Type);
    }
    
    [Fact]
    public void Tokenize_CppSpecificOperators_RecognizesArrowAndDoubleColon()
    {
        var source = "-> ::";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Arrow, tokens[0].Type);
        Assert.Equal(TokenType.DoubleColon, tokens[1].Type);
    }
    
    [Fact]
    public void Tokenize_Punctuation_RecognizesAllPunctuation()
    {
        var source = "( ) { } [ ] ; , . :";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.LeftParen, tokens[0].Type);
        Assert.Equal(TokenType.RightParen, tokens[1].Type);
        Assert.Equal(TokenType.LeftBrace, tokens[2].Type);
        Assert.Equal(TokenType.RightBrace, tokens[3].Type);
        Assert.Equal(TokenType.LeftBracket, tokens[4].Type);
        Assert.Equal(TokenType.RightBracket, tokens[5].Type);
        Assert.Equal(TokenType.Semicolon, tokens[6].Type);
        Assert.Equal(TokenType.Comma, tokens[7].Type);
        Assert.Equal(TokenType.Dot, tokens[8].Type);
        Assert.Equal(TokenType.Colon, tokens[9].Type);
    }
    
    [Fact]
    public void Tokenize_PreprocessorDirective_RecognizesHash()
    {
        var source = "#include";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Hash, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("include", tokens[1].Value);
    }
    
    [Fact]
    public void Tokenize_JavaClassDeclaration_TokenizesCorrectly()
    {
        var source = @"public class MyClass {
    private int value;
    
    public void setValue(int v) {
        value = v;
    }
}";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Public, tokens[0].Type);
        Assert.Equal(TokenType.Class, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
        Assert.Equal("MyClass", tokens[2].Value);
        Assert.Equal(TokenType.LeftBrace, tokens[3].Type);
        Assert.Equal(TokenType.Private, tokens[4].Type);
        Assert.Equal(TokenType.Int, tokens[5].Type);
        Assert.Equal(TokenType.Identifier, tokens[6].Type);
        Assert.Equal("value", tokens[6].Value);
        Assert.Equal(TokenType.Semicolon, tokens[7].Type);
    }
    
    [Fact]
    public void Tokenize_CppNamespace_TokenizesCorrectly()
    {
        var source = @"namespace MyNamespace {
    class MyClass {
        int value;
    };
}";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Namespace, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("MyNamespace", tokens[1].Value);
        Assert.Equal(TokenType.LeftBrace, tokens[2].Type);
        Assert.Equal(TokenType.Class, tokens[3].Type);
        Assert.Equal(TokenType.Identifier, tokens[4].Type);
        Assert.Equal("MyClass", tokens[4].Value);
    }
    
    [Fact]
    public void Tokenize_SourceLocation_TracksLineAndColumn()
    {
        var source = "int x;\nint y;";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(1, tokens[0].Location.Line);
        Assert.Equal(1, tokens[0].Location.Column);
        
        Assert.Equal(1, tokens[1].Location.Line);
        Assert.Equal(5, tokens[1].Location.Column);
        
        Assert.Equal(2, tokens[3].Location.Line);
        Assert.Equal(1, tokens[3].Location.Column);
    }
    
    [Fact]
    public void TokenizeLine_SingleLine_TokenizesOnlyThatLine()
    {
        var source = "int x;\nint y;\nint z;";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.TokenizeLine(2);
        
        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Int, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("y", tokens[1].Value);
        Assert.Equal(TokenType.Semicolon, tokens[2].Type);
    }
    
    [Fact]
    public void Tokenize_BooleanLiterals_RecognizesTrueAndFalse()
    {
        var source = "true false";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.True, tokens[0].Type);
        Assert.Equal(TokenType.False, tokens[1].Type);
    }
    
    [Fact]
    public void Tokenize_NullLiterals_RecognizesNullAndNullptr()
    {
        var source = "null nullptr";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Null, tokens[0].Type);
        Assert.Equal(TokenType.Nullptr, tokens[1].Type);
    }
    
    [Fact]
    public void Tokenize_ComplexExpression_TokenizesCorrectly()
    {
        var source = "result = (a + b) * c / d - e % f;";
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(TokenType.Assign, tokens[1].Type);
        Assert.Equal(TokenType.LeftParen, tokens[2].Type);
        Assert.Equal(TokenType.Identifier, tokens[3].Type);
        Assert.Equal(TokenType.Plus, tokens[4].Type);
        Assert.Equal(TokenType.Identifier, tokens[5].Type);
        Assert.Equal(TokenType.RightParen, tokens[6].Type);
        Assert.Equal(TokenType.Star, tokens[7].Type);
        Assert.Equal(TokenType.Identifier, tokens[8].Type);
        Assert.Equal(TokenType.Slash, tokens[9].Type);
        Assert.Equal(TokenType.Identifier, tokens[10].Type);
        Assert.Equal(TokenType.Minus, tokens[11].Type);
        Assert.Equal(TokenType.Identifier, tokens[12].Type);
        Assert.Equal(TokenType.Percent, tokens[13].Type);
        Assert.Equal(TokenType.Identifier, tokens[14].Type);
        Assert.Equal(TokenType.Semicolon, tokens[15].Type);
    }
}
