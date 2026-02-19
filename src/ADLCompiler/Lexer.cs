using SharedUtilities.Models;
using System.Text;

namespace ADLCompiler;

/// <summary>
/// Lexical analyzer for ADL language supporting Java and C++ syntax
/// </summary>
public class Lexer
{
    private readonly string _source;
    private readonly string _fileName;
    private int _position;
    private int _line;
    private int _column;
    private readonly List<Token> _tokens;
    
    // Keyword mappings for Java and C++
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        // Java keywords
        { "abstract", TokenType.Abstract },
        { "assert", TokenType.Assert },
        { "boolean", TokenType.Boolean },
        { "break", TokenType.Break },
        { "byte", TokenType.Byte },
        { "case", TokenType.Case },
        { "catch", TokenType.Catch },
        { "char", TokenType.Char },
        { "class", TokenType.Class },
        { "const", TokenType.Const },
        { "continue", TokenType.Continue },
        { "default", TokenType.Default },
        { "do", TokenType.Do },
        { "double", TokenType.Double },
        { "else", TokenType.Else },
        { "enum", TokenType.Enum },
        { "extends", TokenType.Extends },
        { "final", TokenType.Final },
        { "finally", TokenType.Finally },
        { "float", TokenType.Float },
        { "for", TokenType.For },
        { "goto", TokenType.Goto },
        { "if", TokenType.If },
        { "implements", TokenType.Implements },
        { "import", TokenType.Import },
        { "instanceof", TokenType.Instanceof },
        { "int", TokenType.Int },
        { "interface", TokenType.Interface },
        { "long", TokenType.Long },
        { "native", TokenType.Native },
        { "new", TokenType.New },
        { "package", TokenType.Package },
        { "private", TokenType.Private },
        { "protected", TokenType.Protected },
        { "public", TokenType.Public },
        { "return", TokenType.Return },
        { "short", TokenType.Short },
        { "static", TokenType.Static },
        { "strictfp", TokenType.Strictfp },
        { "super", TokenType.Super },
        { "switch", TokenType.Switch },
        { "synchronized", TokenType.Synchronized },
        { "this", TokenType.This },
        { "throw", TokenType.Throw },
        { "throws", TokenType.Throws },
        { "transient", TokenType.Transient },
        { "try", TokenType.Try },
        { "void", TokenType.Void },
        { "volatile", TokenType.Volatile },
        { "while", TokenType.While },
        
        // C++ keywords
        { "asm", TokenType.Asm },
        { "auto", TokenType.Auto },
        { "explicit", TokenType.Explicit },
        { "export", TokenType.Export },
        { "extern", TokenType.Extern },
        { "friend", TokenType.Friend },
        { "inline", TokenType.Inline },
        { "mutable", TokenType.Mutable },
        { "namespace", TokenType.Namespace },
        { "operator", TokenType.Operator },
        { "register", TokenType.Register },
        { "reinterpret_cast", TokenType.Reinterpret_Cast },
        { "signed", TokenType.Signed },
        { "sizeof", TokenType.Sizeof },
        { "static_cast", TokenType.Static_Cast },
        { "struct", TokenType.Struct },
        { "template", TokenType.Template },
        { "typedef", TokenType.Typedef },
        { "typename", TokenType.Typename },
        { "union", TokenType.Union },
        { "unsigned", TokenType.Unsigned },
        { "using", TokenType.Using },
        { "virtual", TokenType.Virtual },
        { "wchar_t", TokenType.Wchar_T },
        { "dynamic_cast", TokenType.Dynamic_Cast },
        { "const_cast", TokenType.Const_Cast },
        { "typeid", TokenType.Typeid },
        { "and", TokenType.And },
        { "or", TokenType.Or },
        { "not", TokenType.Not },
        { "xor", TokenType.Xor },
        { "bitand", TokenType.Bitand },
        { "bitor", TokenType.Bitor },
        { "compl", TokenType.Compl },
        { "and_eq", TokenType.And_Eq },
        { "or_eq", TokenType.Or_Eq },
        { "xor_eq", TokenType.Xor_Eq },
        { "not_eq", TokenType.Not_Eq },
        
        // Literals
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "null", TokenType.Null },
        { "nullptr", TokenType.Nullptr }
    };
    
    public Lexer(string source, string fileName = "")
    {
        _source = source;
        _fileName = fileName;
        _position = 0;
        _line = 1;
        _column = 1;
        _tokens = new List<Token>();
    }
    
    /// <summary>
    /// Tokenize the entire source code
    /// </summary>
    public List<Token> Tokenize()
    {
        _tokens.Clear();
        _position = 0;
        _line = 1;
        _column = 1;
        
        while (!IsAtEnd())
        {
            Token? token = ScanToken();
            if (token != null)
            {
                _tokens.Add(token);
            }
        }
        
        // Add EOF token
        _tokens.Add(CreateToken(TokenType.EndOfFile, ""));
        
        return _tokens;
    }
    
    /// <summary>
    /// Incremental tokenization for real-time syntax highlighting
    /// Tokenizes only the specified line range
    /// </summary>
    public List<Token> TokenizeLine(int lineNumber)
    {
        var lineTokens = new List<Token>();
        
        // Find the start position of the line
        int lineStart = 0;
        int currentLine = 1;
        
        while (currentLine < lineNumber && lineStart < _source.Length)
        {
            if (_source[lineStart] == '\n')
            {
                currentLine++;
            }
            lineStart++;
        }
        
        if (lineStart >= _source.Length)
        {
            return lineTokens;
        }
        
        // Find the end of the line
        int lineEnd = lineStart;
        while (lineEnd < _source.Length && _source[lineEnd] != '\n')
        {
            lineEnd++;
        }
        
        // Tokenize just this line
        _position = lineStart;
        _line = lineNumber;
        _column = 1;
        
        while (_position < lineEnd && !IsAtEnd())
        {
            Token? token = ScanToken();
            if (token != null)
            {
                lineTokens.Add(token);
            }
        }
        
        return lineTokens;
    }
    
    private Token? ScanToken()
    {
        SkipWhitespace();
        
        if (IsAtEnd())
        {
            return null;
        }
        
        char c = Peek();
        
        // Comments
        if (c == '/' && PeekNext() == '/')
        {
            return ScanLineComment();
        }
        
        if (c == '/' && PeekNext() == '*')
        {
            return ScanBlockComment();
        }
        
        // String literals
        if (c == '"')
        {
            return ScanStringLiteral();
        }
        
        // Character literals
        if (c == '\'')
        {
            return ScanCharLiteral();
        }
        
        // Numbers
        if (char.IsDigit(c))
        {
            return ScanNumber();
        }
        
        // Identifiers and keywords
        if (char.IsLetter(c) || c == '_')
        {
            return ScanIdentifierOrKeyword();
        }
        
        // Preprocessor directives
        if (c == '#')
        {
            return ScanPreprocessor();
        }
        
        // Operators and punctuation
        return ScanOperatorOrPunctuation();
    }
    
    private Token ScanLineComment()
    {
        int startLine = _line;
        int startColumn = _column;
        var sb = new StringBuilder();
        
        // Consume //
        sb.Append(Advance());
        sb.Append(Advance());
        
        // Read until end of line
        while (!IsAtEnd() && Peek() != '\n')
        {
            sb.Append(Advance());
        }
        
        return CreateToken(TokenType.Comment, sb.ToString(), startLine, startColumn);
    }
    
    private Token ScanBlockComment()
    {
        int startLine = _line;
        int startColumn = _column;
        var sb = new StringBuilder();
        
        // Consume /*
        sb.Append(Advance());
        sb.Append(Advance());
        
        // Read until */
        while (!IsAtEnd())
        {
            if (Peek() == '*' && PeekNext() == '/')
            {
                sb.Append(Advance());
                sb.Append(Advance());
                break;
            }
            sb.Append(Advance());
        }
        
        return CreateToken(TokenType.Comment, sb.ToString(), startLine, startColumn);
    }
    
    private Token ScanStringLiteral()
    {
        int startLine = _line;
        int startColumn = _column;
        var sb = new StringBuilder();
        
        // Consume opening "
        sb.Append(Advance());
        
        while (!IsAtEnd() && Peek() != '"')
        {
            if (Peek() == '\\')
            {
                sb.Append(Advance()); // Backslash
                if (!IsAtEnd())
                {
                    sb.Append(Advance()); // Escaped character
                }
            }
            else
            {
                sb.Append(Advance());
            }
        }
        
        // Consume closing "
        if (!IsAtEnd())
        {
            sb.Append(Advance());
        }
        
        return CreateToken(TokenType.StringLiteral, sb.ToString(), startLine, startColumn);
    }
    
    private Token ScanCharLiteral()
    {
        int startLine = _line;
        int startColumn = _column;
        var sb = new StringBuilder();
        
        // Consume opening '
        sb.Append(Advance());
        
        while (!IsAtEnd() && Peek() != '\'')
        {
            if (Peek() == '\\')
            {
                sb.Append(Advance()); // Backslash
                if (!IsAtEnd())
                {
                    sb.Append(Advance()); // Escaped character
                }
            }
            else
            {
                sb.Append(Advance());
            }
        }
        
        // Consume closing '
        if (!IsAtEnd())
        {
            sb.Append(Advance());
        }
        
        return CreateToken(TokenType.CharLiteral, sb.ToString(), startLine, startColumn);
    }
    
    private Token ScanNumber()
    {
        int startLine = _line;
        int startColumn = _column;
        var sb = new StringBuilder();
        bool isFloat = false;
        
        // Read digits
        while (!IsAtEnd() && char.IsDigit(Peek()))
        {
            sb.Append(Advance());
        }
        
        // Check for decimal point
        if (!IsAtEnd() && Peek() == '.' && char.IsDigit(PeekNext()))
        {
            isFloat = true;
            sb.Append(Advance()); // Consume .
            
            while (!IsAtEnd() && char.IsDigit(Peek()))
            {
                sb.Append(Advance());
            }
        }
        
        // Check for exponent (e or E)
        if (!IsAtEnd() && (Peek() == 'e' || Peek() == 'E'))
        {
            isFloat = true;
            sb.Append(Advance());
            
            if (!IsAtEnd() && (Peek() == '+' || Peek() == '-'))
            {
                sb.Append(Advance());
            }
            
            while (!IsAtEnd() && char.IsDigit(Peek()))
            {
                sb.Append(Advance());
            }
        }
        
        // Check for float suffix (f or F)
        if (!IsAtEnd() && (Peek() == 'f' || Peek() == 'F'))
        {
            isFloat = true;
            sb.Append(Advance());
        }
        
        // Check for long suffix (l or L)
        if (!IsAtEnd() && (Peek() == 'l' || Peek() == 'L'))
        {
            sb.Append(Advance());
        }
        
        TokenType type = isFloat ? TokenType.FloatLiteral : TokenType.IntegerLiteral;
        return CreateToken(type, sb.ToString(), startLine, startColumn);
    }
    
    private Token ScanIdentifierOrKeyword()
    {
        int startLine = _line;
        int startColumn = _column;
        var sb = new StringBuilder();
        
        // Read identifier
        while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            sb.Append(Advance());
        }
        
        string text = sb.ToString();
        
        // Check if it's a keyword
        if (Keywords.TryGetValue(text, out TokenType keywordType))
        {
            return CreateToken(keywordType, text, startLine, startColumn);
        }
        
        return CreateToken(TokenType.Identifier, text, startLine, startColumn);
    }
    
    private Token ScanPreprocessor()
    {
        int startLine = _line;
        int startColumn = _column;
        
        char c = Advance();
        return CreateToken(TokenType.Hash, c.ToString(), startLine, startColumn);
    }
    
    private Token ScanOperatorOrPunctuation()
    {
        int startLine = _line;
        int startColumn = _column;
        char c = Advance();
        
        // Two or three character operators
        if (!IsAtEnd())
        {
            char next = Peek();
            string twoChar = $"{c}{next}";
            
            // Three character operators
            if (_position + 1 < _source.Length)
            {
                char nextNext = _source[_position + 1];
                string threeChar = $"{c}{next}{nextNext}";
                
                switch (threeChar)
                {
                    case ">>=":
                        Advance();
                        Advance();
                        return CreateToken(TokenType.RightShiftAssign, threeChar, startLine, startColumn);
                    case "<<=":
                        Advance();
                        Advance();
                        return CreateToken(TokenType.LeftShiftAssign, threeChar, startLine, startColumn);
                }
            }
            
            // Two character operators
            switch (twoChar)
            {
                case "==":
                    Advance();
                    return CreateToken(TokenType.Equal, twoChar, startLine, startColumn);
                case "!=":
                    Advance();
                    return CreateToken(TokenType.NotEqual, twoChar, startLine, startColumn);
                case "<=":
                    Advance();
                    return CreateToken(TokenType.LessEqual, twoChar, startLine, startColumn);
                case ">=":
                    Advance();
                    return CreateToken(TokenType.GreaterEqual, twoChar, startLine, startColumn);
                case "&&":
                    Advance();
                    return CreateToken(TokenType.LogicalAnd, twoChar, startLine, startColumn);
                case "||":
                    Advance();
                    return CreateToken(TokenType.LogicalOr, twoChar, startLine, startColumn);
                case "<<":
                    Advance();
                    return CreateToken(TokenType.LeftShift, twoChar, startLine, startColumn);
                case ">>":
                    Advance();
                    return CreateToken(TokenType.RightShift, twoChar, startLine, startColumn);
                case "++":
                    Advance();
                    return CreateToken(TokenType.Increment, twoChar, startLine, startColumn);
                case "--":
                    Advance();
                    return CreateToken(TokenType.Decrement, twoChar, startLine, startColumn);
                case "->":
                    Advance();
                    return CreateToken(TokenType.Arrow, twoChar, startLine, startColumn);
                case "::":
                    Advance();
                    return CreateToken(TokenType.DoubleColon, twoChar, startLine, startColumn);
                case "+=":
                    Advance();
                    return CreateToken(TokenType.PlusAssign, twoChar, startLine, startColumn);
                case "-=":
                    Advance();
                    return CreateToken(TokenType.MinusAssign, twoChar, startLine, startColumn);
                case "*=":
                    Advance();
                    return CreateToken(TokenType.StarAssign, twoChar, startLine, startColumn);
                case "/=":
                    Advance();
                    return CreateToken(TokenType.SlashAssign, twoChar, startLine, startColumn);
                case "%=":
                    Advance();
                    return CreateToken(TokenType.PercentAssign, twoChar, startLine, startColumn);
                case "&=":
                    Advance();
                    return CreateToken(TokenType.AndAssign, twoChar, startLine, startColumn);
                case "|=":
                    Advance();
                    return CreateToken(TokenType.OrAssign, twoChar, startLine, startColumn);
                case "^=":
                    Advance();
                    return CreateToken(TokenType.XorAssign, twoChar, startLine, startColumn);
            }
        }
        
        // Single character operators and punctuation
        return c switch
        {
            '+' => CreateToken(TokenType.Plus, c.ToString(), startLine, startColumn),
            '-' => CreateToken(TokenType.Minus, c.ToString(), startLine, startColumn),
            '*' => CreateToken(TokenType.Star, c.ToString(), startLine, startColumn),
            '/' => CreateToken(TokenType.Slash, c.ToString(), startLine, startColumn),
            '%' => CreateToken(TokenType.Percent, c.ToString(), startLine, startColumn),
            '=' => CreateToken(TokenType.Assign, c.ToString(), startLine, startColumn),
            '<' => CreateToken(TokenType.Less, c.ToString(), startLine, startColumn),
            '>' => CreateToken(TokenType.Greater, c.ToString(), startLine, startColumn),
            '!' => CreateToken(TokenType.LogicalNot, c.ToString(), startLine, startColumn),
            '&' => CreateToken(TokenType.BitwiseAnd, c.ToString(), startLine, startColumn),
            '|' => CreateToken(TokenType.BitwiseOr, c.ToString(), startLine, startColumn),
            '^' => CreateToken(TokenType.BitwiseXor, c.ToString(), startLine, startColumn),
            '~' => CreateToken(TokenType.BitwiseNot, c.ToString(), startLine, startColumn),
            '?' => CreateToken(TokenType.Question, c.ToString(), startLine, startColumn),
            '(' => CreateToken(TokenType.LeftParen, c.ToString(), startLine, startColumn),
            ')' => CreateToken(TokenType.RightParen, c.ToString(), startLine, startColumn),
            '{' => CreateToken(TokenType.LeftBrace, c.ToString(), startLine, startColumn),
            '}' => CreateToken(TokenType.RightBrace, c.ToString(), startLine, startColumn),
            '[' => CreateToken(TokenType.LeftBracket, c.ToString(), startLine, startColumn),
            ']' => CreateToken(TokenType.RightBracket, c.ToString(), startLine, startColumn),
            ';' => CreateToken(TokenType.Semicolon, c.ToString(), startLine, startColumn),
            ',' => CreateToken(TokenType.Comma, c.ToString(), startLine, startColumn),
            '.' => CreateToken(TokenType.Dot, c.ToString(), startLine, startColumn),
            ':' => CreateToken(TokenType.Colon, c.ToString(), startLine, startColumn),
            _ => CreateToken(TokenType.Unknown, c.ToString(), startLine, startColumn)
        };
    }
    
    private void SkipWhitespace()
    {
        while (!IsAtEnd())
        {
            char c = Peek();
            if (c == ' ' || c == '\t' || c == '\r')
            {
                Advance();
            }
            else if (c == '\n')
            {
                Advance();
            }
            else
            {
                break;
            }
        }
    }
    
    private char Peek()
    {
        if (IsAtEnd())
        {
            return '\0';
        }
        return _source[_position];
    }
    
    private char PeekNext()
    {
        if (_position + 1 >= _source.Length)
        {
            return '\0';
        }
        return _source[_position + 1];
    }
    
    private char Advance()
    {
        char c = _source[_position];
        _position++;
        
        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
        
        return c;
    }
    
    private bool IsAtEnd()
    {
        return _position >= _source.Length;
    }
    
    private Token CreateToken(TokenType type, string value)
    {
        return CreateToken(type, value, _line, _column);
    }
    
    private Token CreateToken(TokenType type, string value, int line, int column)
    {
        return new Token
        {
            Type = type,
            Value = value,
            Location = new SourceLocation
            {
                File = _fileName,
                Line = line,
                Column = column
            }
        };
    }
}
