using SharedUtilities.Models;

namespace ADLCompiler;

/// <summary>
/// Recursive descent parser for ADL language supporting Java and C++ syntax
/// </summary>
public class Parser
{
    private readonly List<Token> _tokens;
    private readonly string _fileName;
    private int _current;
    
    public Parser(List<Token> tokens, string fileName = "")
    {
        _tokens = tokens;
        _fileName = fileName;
        _current = 0;
    }
    
    /// <summary>
    /// Parse tokens into a compilation unit (AST root)
    /// </summary>
    public CompilationUnit Parse()
    {
        var compilationUnit = new CompilationUnit
        {
            FileName = _fileName,
            Location = CurrentLocation()
        };
        
        try
        {
            // Parse package declaration (Java style)
            if (Match(TokenType.Package))
            {
                compilationUnit.PackageName = ParseQualifiedName();
                Consume(TokenType.Semicolon, "Expected ';' after package declaration");
            }
            
            // Parse namespace declaration (C++ style)
            if (Match(TokenType.Namespace))
            {
                compilationUnit.Namespace = ParseNamespaceDeclaration();
            }
            
            // Parse class declarations
            while (!IsAtEnd())
            {
                compilationUnit.Classes.Add(ParseClassDeclaration());
            }
        }
        catch (ParseException ex)
        {
            throw new Exception($"Parse error at {ex.Location}: {ex.Message}", ex);
        }
        
        return compilationUnit;
    }
    
    // Namespace parsing
    
    private NamespaceDeclaration ParseNamespaceDeclaration()
    {
        var ns = new NamespaceDeclaration { Location = PreviousLocation() };
        ns.Name = ParseQualifiedName();
        
        Consume(TokenType.LeftBrace, "Expected '{' after namespace name");
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            if (Check(TokenType.Class) || Check(TokenType.Struct))
            {
                ns.Classes.Add(ParseClassDeclaration());
            }
            else
            {
                // Parse standalone function
                ns.Functions.Add(ParseMethodDeclaration(AccessModifier.Public));
            }
        }
        
        Consume(TokenType.RightBrace, "Expected '}' after namespace body");
        return ns;
    }
    
    // Class parsing
    
    private ClassDeclaration ParseClassDeclaration()
    {
        var classDecl = new ClassDeclaration { Location = CurrentLocation() };
        
        // Parse modifiers
        while (Check(TokenType.Public) || Check(TokenType.Private) || Check(TokenType.Protected) ||
               Check(TokenType.Abstract) || Check(TokenType.Static))
        {
            if (Match(TokenType.Public)) classDecl.AccessModifier = AccessModifier.Public;
            else if (Match(TokenType.Private)) classDecl.AccessModifier = AccessModifier.Private;
            else if (Match(TokenType.Protected)) classDecl.AccessModifier = AccessModifier.Protected;
            else if (Match(TokenType.Abstract)) classDecl.IsAbstract = true;
            else if (Match(TokenType.Static)) classDecl.IsStatic = true;
        }
        
        // Parse class or struct keyword
        if (!Match(TokenType.Class) && !Match(TokenType.Struct))
        {
            throw new ParseException("Expected 'class' or 'struct'", CurrentLocation());
        }
        
        // Parse class name
        classDecl.Name = Consume(TokenType.Identifier, "Expected class name").Value;
        
        // Parse extends clause (Java style)
        if (Match(TokenType.Extends))
        {
            classDecl.SuperClass = Consume(TokenType.Identifier, "Expected superclass name").Value;
        }
        
        // Parse implements clause (Java style)
        if (Match(TokenType.Implements))
        {
            do
            {
                classDecl.Interfaces.Add(Consume(TokenType.Identifier, "Expected interface name").Value);
            } while (Match(TokenType.Comma));
        }
        
        Consume(TokenType.LeftBrace, "Expected '{' after class declaration");
        
        // Parse class body
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            ParseClassMember(classDecl);
        }
        
        Consume(TokenType.RightBrace, "Expected '}' after class body");
        
        // Optional semicolon (C++ style)
        Match(TokenType.Semicolon);
        
        return classDecl;
    }
    
    private void ParseClassMember(ClassDeclaration classDecl)
    {
        var location = CurrentLocation();
        var accessModifier = AccessModifier.Private; // Default for class members
        bool isStatic = false;
        bool isNative = false;
        bool isAbstract = false;
        bool isVirtual = false;
        bool isFinal = false;
        
        // Parse modifiers
        while (true)
        {
            if (Match(TokenType.Public)) accessModifier = AccessModifier.Public;
            else if (Match(TokenType.Private)) accessModifier = AccessModifier.Private;
            else if (Match(TokenType.Protected)) accessModifier = AccessModifier.Protected;
            else if (Match(TokenType.Static)) isStatic = true;
            else if (Match(TokenType.Native)) isNative = true;
            else if (Match(TokenType.Abstract)) isAbstract = true;
            else if (Match(TokenType.Virtual)) isVirtual = true;
            else if (Match(TokenType.Final)) isFinal = true;
            else break;
        }
        
        // Parse type
        var type = ParseType();
        
        // Parse name
        var name = Consume(TokenType.Identifier, "Expected member name").Value;
        
        // Determine if this is a method or field
        if (Check(TokenType.LeftParen))
        {
            // Method declaration
            var method = ParseMethodDeclarationContinuation(type, name, accessModifier);
            method.IsStatic = isStatic;
            method.IsNative = isNative;
            method.IsAbstract = isAbstract;
            method.IsVirtual = isVirtual;
            method.Location = location;
            classDecl.Methods.Add(method);
        }
        else
        {
            // Field declaration
            var field = new FieldDeclaration
            {
                Location = location,
                FieldType = type,
                Name = name,
                AccessModifier = accessModifier,
                IsStatic = isStatic,
                IsFinal = isFinal
            };
            
            // Parse initializer
            if (Match(TokenType.Assign))
            {
                field.Initializer = ParseExpression();
            }
            
            Consume(TokenType.Semicolon, "Expected ';' after field declaration");
            classDecl.Fields.Add(field);
        }
    }
    
    // Method parsing
    
    private MethodDeclaration ParseMethodDeclaration(AccessModifier accessModifier)
    {
        var location = CurrentLocation();
        bool isStatic = false;
        bool isNative = false;
        bool isVirtual = false;
        
        // Parse modifiers
        while (Match(TokenType.Static) || Match(TokenType.Native) || Match(TokenType.Virtual))
        {
            if (Previous().Type == TokenType.Static) isStatic = true;
            else if (Previous().Type == TokenType.Native) isNative = true;
            else if (Previous().Type == TokenType.Virtual) isVirtual = true;
        }
        
        var returnType = ParseType();
        var name = Consume(TokenType.Identifier, "Expected method name").Value;
        
        var method = ParseMethodDeclarationContinuation(returnType, name, accessModifier);
        method.IsStatic = isStatic;
        method.IsNative = isNative;
        method.IsVirtual = isVirtual;
        method.Location = location;
        
        return method;
    }
    
    private MethodDeclaration ParseMethodDeclarationContinuation(TypeReference returnType, string name, AccessModifier accessModifier)
    {
        var method = new MethodDeclaration
        {
            ReturnType = returnType,
            Name = name,
            AccessModifier = accessModifier
        };
        
        // Parse parameters
        Consume(TokenType.LeftParen, "Expected '(' after method name");
        
        if (!Check(TokenType.RightParen))
        {
            do
            {
                var paramType = ParseType();
                var paramName = Consume(TokenType.Identifier, "Expected parameter name").Value;
                method.Parameters.Add(new ParameterDeclaration
                {
                    ParameterType = paramType,
                    Name = paramName,
                    Location = PreviousLocation()
                });
            } while (Match(TokenType.Comma));
        }
        
        Consume(TokenType.RightParen, "Expected ')' after parameters");
        
        // Parse method body or semicolon (for abstract/native methods)
        if (Match(TokenType.Semicolon))
        {
            method.Body = null;
        }
        else
        {
            method.Body = ParseBlockStatement();
        }
        
        return method;
    }
    
    // Type parsing
    
    private TypeReference ParseType()
    {
        var type = new TypeReference { Location = CurrentLocation() };
        
        // Parse base type name - accept both primitive types and identifiers
        if (!IsTypeToken(Peek()))
        {
            throw new ParseException($"Expected type name", CurrentLocation());
        }
        
        type.Name = Advance().Value;
        
        // Parse pointer level (C++ style)
        while (Match(TokenType.Star))
        {
            type.IsPointer = true;
            type.PointerLevel++;
        }
        
        // Parse array brackets (Java style)
        while (Match(TokenType.LeftBracket))
        {
            Consume(TokenType.RightBracket, "Expected ']' after '['");
            type.IsArray = true;
        }
        
        return type;
    }
    
    // Statement parsing
    
    private BlockStatement ParseBlockStatement()
    {
        var block = new BlockStatement { Location = CurrentLocation() };
        
        Consume(TokenType.LeftBrace, "Expected '{'");
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            block.Statements.Add(ParseStatement());
        }
        
        Consume(TokenType.RightBrace, "Expected '}'");
        
        return block;
    }
    
    private Statement ParseStatement()
    {
        // Return statement
        if (Match(TokenType.Return))
        {
            return ParseReturnStatement();
        }
        
        // If statement
        if (Match(TokenType.If))
        {
            return ParseIfStatement();
        }
        
        // While statement
        if (Match(TokenType.While))
        {
            return ParseWhileStatement();
        }
        
        // Do-while statement
        if (Match(TokenType.Do))
        {
            return ParseDoWhileStatement();
        }
        
        // For statement
        if (Match(TokenType.For))
        {
            return ParseForStatement();
        }
        
        // Switch statement
        if (Match(TokenType.Switch))
        {
            return ParseSwitchStatement();
        }
        
        // Break statement
        if (Match(TokenType.Break))
        {
            var stmt = new BreakStatement { Location = PreviousLocation() };
            Consume(TokenType.Semicolon, "Expected ';' after 'break'");
            return stmt;
        }
        
        // Continue statement
        if (Match(TokenType.Continue))
        {
            var stmt = new ContinueStatement { Location = PreviousLocation() };
            Consume(TokenType.Semicolon, "Expected ';' after 'continue'");
            return stmt;
        }
        
        // Block statement
        if (Check(TokenType.LeftBrace))
        {
            return ParseBlockStatement();
        }
        
        // Variable declaration or expression statement
        return ParseVariableDeclarationOrExpressionStatement();
    }
    
    private ReturnStatement ParseReturnStatement()
    {
        var stmt = new ReturnStatement { Location = PreviousLocation() };
        
        if (!Check(TokenType.Semicolon))
        {
            stmt.Value = ParseExpression();
        }
        
        Consume(TokenType.Semicolon, "Expected ';' after return statement");
        return stmt;
    }
    
    private IfStatement ParseIfStatement()
    {
        var stmt = new IfStatement { Location = PreviousLocation() };
        
        Consume(TokenType.LeftParen, "Expected '(' after 'if'");
        stmt.Condition = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after if condition");
        
        stmt.ThenBranch = ParseStatement();
        
        if (Match(TokenType.Else))
        {
            stmt.ElseBranch = ParseStatement();
        }
        
        return stmt;
    }
    
    private WhileStatement ParseWhileStatement()
    {
        var stmt = new WhileStatement { Location = PreviousLocation() };
        
        Consume(TokenType.LeftParen, "Expected '(' after 'while'");
        stmt.Condition = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after while condition");
        
        stmt.Body = ParseStatement();
        
        return stmt;
    }
    
    private DoWhileStatement ParseDoWhileStatement()
    {
        var stmt = new DoWhileStatement { Location = PreviousLocation() };
        
        stmt.Body = ParseStatement();
        
        Consume(TokenType.While, "Expected 'while' after do body");
        Consume(TokenType.LeftParen, "Expected '(' after 'while'");
        stmt.Condition = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after while condition");
        Consume(TokenType.Semicolon, "Expected ';' after do-while statement");
        
        return stmt;
    }
    
    private ForStatement ParseForStatement()
    {
        var stmt = new ForStatement { Location = PreviousLocation() };
        
        Consume(TokenType.LeftParen, "Expected '(' after 'for'");
        
        // Parse initializer
        if (Match(TokenType.Semicolon))
        {
            stmt.Initializer = null;
        }
        else
        {
            stmt.Initializer = ParseVariableDeclarationOrExpressionStatement();
        }
        
        // Parse condition
        if (!Check(TokenType.Semicolon))
        {
            stmt.Condition = ParseExpression();
        }
        Consume(TokenType.Semicolon, "Expected ';' after for condition");
        
        // Parse increment
        if (!Check(TokenType.RightParen))
        {
            stmt.Increment = ParseExpression();
        }
        
        Consume(TokenType.RightParen, "Expected ')' after for clauses");
        
        stmt.Body = ParseStatement();
        
        return stmt;
    }
    
    private SwitchStatement ParseSwitchStatement()
    {
        var stmt = new SwitchStatement { Location = PreviousLocation() };
        
        Consume(TokenType.LeftParen, "Expected '(' after 'switch'");
        stmt.Expression = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after switch expression");
        
        Consume(TokenType.LeftBrace, "Expected '{' after switch expression");
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            if (Match(TokenType.Case))
            {
                var caseStmt = new CaseStatement { Location = PreviousLocation() };
                caseStmt.Value = ParseExpression();
                Consume(TokenType.Colon, "Expected ':' after case value");
                
                while (!Check(TokenType.Case) && !Check(TokenType.Default) && !Check(TokenType.RightBrace))
                {
                    caseStmt.Statements.Add(ParseStatement());
                }
                
                stmt.Cases.Add(caseStmt);
            }
            else if (Match(TokenType.Default))
            {
                var defaultCase = new CaseStatement { Location = PreviousLocation(), Value = null };
                Consume(TokenType.Colon, "Expected ':' after 'default'");
                
                while (!Check(TokenType.Case) && !Check(TokenType.Default) && !Check(TokenType.RightBrace))
                {
                    defaultCase.Statements.Add(ParseStatement());
                }
                
                stmt.Cases.Add(defaultCase);
            }
            else
            {
                throw new ParseException("Expected 'case' or 'default' in switch statement", CurrentLocation());
            }
        }
        
        Consume(TokenType.RightBrace, "Expected '}' after switch body");
        
        return stmt;
    }
    
    private Statement ParseVariableDeclarationOrExpressionStatement()
    {
        // Try to determine if this is a variable declaration or expression
        // Look ahead to see if we have: Type Identifier = ...
        
        int savedPosition = _current;
        
        // Try to parse as variable declaration
        if (IsTypeToken(Peek()))
        {
            try
            {
                var type = ParseType();
                if (Check(TokenType.Identifier))
                {
                    var name = Advance().Value;
                    
                    var varDecl = new VariableDeclarationStatement
                    {
                        Location = type.Location,
                        VariableType = type,
                        Name = name
                    };
                    
                    if (Match(TokenType.Assign))
                    {
                        varDecl.Initializer = ParseExpression();
                    }
                    
                    Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
                    return varDecl;
                }
                else
                {
                    // Not followed by an identifier, reset and parse as expression
                    _current = savedPosition;
                }
            }
            catch
            {
                // Not a variable declaration, reset and parse as expression
                _current = savedPosition;
            }
        }
        
        // Parse as expression statement
        var expr = ParseExpression();
        Consume(TokenType.Semicolon, "Expected ';' after expression");
        return new ExpressionStatement { Expression = expr, Location = expr.Location };
    }
    
    private bool IsTypeToken(Token token)
    {
        return token.Type == TokenType.Int || token.Type == TokenType.Long ||
               token.Type == TokenType.Float || token.Type == TokenType.Double ||
               token.Type == TokenType.Boolean || token.Type == TokenType.Char ||
               token.Type == TokenType.Byte || token.Type == TokenType.Short ||
               token.Type == TokenType.Void || token.Type == TokenType.Identifier;
    }
    
    // Expression parsing with operator precedence
    
    private Expression ParseExpression()
    {
        return ParseAssignment();
    }
    
    private Expression ParseAssignment()
    {
        var expr = ParseTernary();
        
        if (Match(TokenType.Assign))
        {
            var value = ParseAssignment(); // Right-associative
            return new AssignmentExpression
            {
                Target = expr,
                Value = value,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseTernary()
    {
        var expr = ParseLogicalOr();
        
        if (Match(TokenType.Question))
        {
            var trueExpr = ParseExpression();
            Consume(TokenType.Colon, "Expected ':' in ternary expression");
            var falseExpr = ParseExpression();
            
            return new TernaryExpression
            {
                Condition = expr,
                TrueExpression = trueExpr,
                FalseExpression = falseExpr,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseLogicalOr()
    {
        var expr = ParseLogicalAnd();
        
        while (Match(TokenType.LogicalOr) || Match(TokenType.Or))
        {
            var op = BinaryOperator.LogicalOr;
            var right = ParseLogicalAnd();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = op,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseLogicalAnd()
    {
        var expr = ParseBitwiseOr();
        
        while (Match(TokenType.LogicalAnd) || Match(TokenType.And))
        {
            var op = BinaryOperator.LogicalAnd;
            var right = ParseBitwiseOr();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = op,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseBitwiseOr()
    {
        var expr = ParseBitwiseXor();
        
        while (Match(TokenType.BitwiseOr))
        {
            var right = ParseBitwiseXor();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = BinaryOperator.BitwiseOr,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseBitwiseXor()
    {
        var expr = ParseBitwiseAnd();
        
        while (Match(TokenType.BitwiseXor))
        {
            var right = ParseBitwiseAnd();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = BinaryOperator.BitwiseXor,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseBitwiseAnd()
    {
        var expr = ParseEquality();
        
        while (Match(TokenType.BitwiseAnd))
        {
            var right = ParseEquality();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = BinaryOperator.BitwiseAnd,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseEquality()
    {
        var expr = ParseComparison();
        
        while (Match(TokenType.Equal) || Match(TokenType.NotEqual))
        {
            var op = Previous().Type == TokenType.Equal ? BinaryOperator.Equal : BinaryOperator.NotEqual;
            var right = ParseComparison();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = op,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseComparison()
    {
        var expr = ParseShift();
        
        while (Match(TokenType.Less) || Match(TokenType.Greater) || 
               Match(TokenType.LessEqual) || Match(TokenType.GreaterEqual))
        {
            var op = Previous().Type switch
            {
                TokenType.Less => BinaryOperator.Less,
                TokenType.Greater => BinaryOperator.Greater,
                TokenType.LessEqual => BinaryOperator.LessEqual,
                TokenType.GreaterEqual => BinaryOperator.GreaterEqual,
                _ => throw new ParseException("Invalid comparison operator", PreviousLocation())
            };
            
            var right = ParseShift();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = op,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseShift()
    {
        var expr = ParseAdditive();
        
        while (Match(TokenType.LeftShift) || Match(TokenType.RightShift))
        {
            var op = Previous().Type == TokenType.LeftShift ? BinaryOperator.LeftShift : BinaryOperator.RightShift;
            var right = ParseAdditive();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = op,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseAdditive()
    {
        var expr = ParseMultiplicative();
        
        while (Match(TokenType.Plus) || Match(TokenType.Minus))
        {
            var op = Previous().Type == TokenType.Plus ? BinaryOperator.Add : BinaryOperator.Subtract;
            var right = ParseMultiplicative();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = op,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseMultiplicative()
    {
        var expr = ParseUnary();
        
        while (Match(TokenType.Star) || Match(TokenType.Slash) || Match(TokenType.Percent))
        {
            var op = Previous().Type switch
            {
                TokenType.Star => BinaryOperator.Multiply,
                TokenType.Slash => BinaryOperator.Divide,
                TokenType.Percent => BinaryOperator.Modulo,
                _ => throw new ParseException("Invalid multiplicative operator", PreviousLocation())
            };
            
            var right = ParseUnary();
            expr = new BinaryExpression
            {
                Left = expr,
                Operator = op,
                Right = right,
                Location = expr.Location
            };
        }
        
        return expr;
    }
    
    private Expression ParseUnary()
    {
        // Prefix operators
        if (Match(TokenType.Minus) || Match(TokenType.LogicalNot) || Match(TokenType.BitwiseNot) ||
            Match(TokenType.Increment) || Match(TokenType.Decrement))
        {
            var op = Previous().Type switch
            {
                TokenType.Minus => UnaryOperator.Negate,
                TokenType.LogicalNot => UnaryOperator.LogicalNot,
                TokenType.BitwiseNot => UnaryOperator.BitwiseNot,
                TokenType.Increment => UnaryOperator.PreIncrement,
                TokenType.Decrement => UnaryOperator.PreDecrement,
                _ => throw new ParseException("Invalid unary operator", PreviousLocation())
            };
            
            var operand = ParseUnary();
            return new UnaryExpression
            {
                Operator = op,
                Operand = operand,
                Location = PreviousLocation()
            };
        }
        
        return ParsePostfix();
    }
    
    private Expression ParsePostfix()
    {
        var expr = ParsePrimary();
        
        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                // Method call
                expr = ParseMethodCall(expr);
            }
            else if (Match(TokenType.LeftBracket))
            {
                // Array access
                var index = ParseExpression();
                Consume(TokenType.RightBracket, "Expected ']' after array index");
                expr = new ArrayAccessExpression
                {
                    Array = expr,
                    Index = index,
                    Location = expr.Location
                };
            }
            else if (Match(TokenType.Dot))
            {
                // Member access
                var memberName = Consume(TokenType.Identifier, "Expected member name after '.'").Value;
                expr = new MemberAccessExpression
                {
                    Target = expr,
                    MemberName = memberName,
                    Location = expr.Location
                };
            }
            else if (Match(TokenType.Increment))
            {
                // Postfix increment
                expr = new UnaryExpression
                {
                    Operator = UnaryOperator.PostIncrement,
                    Operand = expr,
                    Location = expr.Location
                };
            }
            else if (Match(TokenType.Decrement))
            {
                // Postfix decrement
                expr = new UnaryExpression
                {
                    Operator = UnaryOperator.PostDecrement,
                    Operand = expr,
                    Location = expr.Location
                };
            }
            else
            {
                break;
            }
        }
        
        return expr;
    }
    
    private Expression ParseMethodCall(Expression target)
    {
        var call = new MethodCallExpression { Location = target.Location };
        
        // Determine target and method name
        if (target is IdentifierExpression identExpr)
        {
            call.Target = null;
            call.MethodName = identExpr.Name;
        }
        else if (target is MemberAccessExpression memberExpr)
        {
            call.Target = memberExpr.Target;
            call.MethodName = memberExpr.MemberName;
        }
        else
        {
            throw new ParseException("Invalid method call target", target.Location);
        }
        
        // Parse arguments
        if (!Check(TokenType.RightParen))
        {
            do
            {
                call.Arguments.Add(ParseExpression());
            } while (Match(TokenType.Comma));
        }
        
        Consume(TokenType.RightParen, "Expected ')' after method arguments");
        
        return call;
    }
    
    private Expression ParsePrimary()
    {
        var location = CurrentLocation();
        
        // Literals
        if (Match(TokenType.True))
        {
            return new LiteralExpression
            {
                Value = true,
                LiteralType = LiteralType.Boolean,
                Location = location
            };
        }
        
        if (Match(TokenType.False))
        {
            return new LiteralExpression
            {
                Value = false,
                LiteralType = LiteralType.Boolean,
                Location = location
            };
        }
        
        if (Match(TokenType.Null) || Match(TokenType.Nullptr))
        {
            return new LiteralExpression
            {
                Value = null,
                LiteralType = LiteralType.Null,
                Location = location
            };
        }
        
        if (Match(TokenType.IntegerLiteral))
        {
            return new LiteralExpression
            {
                Value = int.Parse(Previous().Value),
                LiteralType = LiteralType.Integer,
                Location = location
            };
        }
        
        if (Match(TokenType.FloatLiteral))
        {
            return new LiteralExpression
            {
                Value = double.Parse(Previous().Value),
                LiteralType = LiteralType.Float,
                Location = location
            };
        }
        
        if (Match(TokenType.StringLiteral))
        {
            return new LiteralExpression
            {
                Value = Previous().Value,
                LiteralType = LiteralType.String,
                Location = location
            };
        }
        
        if (Match(TokenType.CharLiteral))
        {
            return new LiteralExpression
            {
                Value = Previous().Value.Length > 0 ? Previous().Value[0] : '\0',
                LiteralType = LiteralType.Char,
                Location = location
            };
        }
        
        // Identifier
        if (Match(TokenType.Identifier))
        {
            return new IdentifierExpression
            {
                Name = Previous().Value,
                Location = location
            };
        }
        
        // Parenthesized expression or cast
        if (Match(TokenType.LeftParen))
        {
            // Try to determine if this is a cast or parenthesized expression
            if (IsTypeToken(Peek()) && PeekAhead(1).Type == TokenType.RightParen)
            {
                // Cast expression
                var targetType = ParseType();
                Consume(TokenType.RightParen, "Expected ')' after cast type");
                var expr = ParseUnary();
                return new CastExpression
                {
                    TargetType = targetType,
                    Expression = expr,
                    Location = location
                };
            }
            else
            {
                // Parenthesized expression
                var expr = ParseExpression();
                Consume(TokenType.RightParen, "Expected ')' after expression");
                return expr;
            }
        }
        
        // New expression
        if (Match(TokenType.New))
        {
            var typeToCreate = ParseType();
            
            Consume(TokenType.LeftParen, "Expected '(' after type in new expression");
            
            var newExpr = new NewExpression
            {
                TypeToCreate = typeToCreate,
                Location = location
            };
            
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    newExpr.Arguments.Add(ParseExpression());
                } while (Match(TokenType.Comma));
            }
            
            Consume(TokenType.RightParen, "Expected ')' after constructor arguments");
            
            return newExpr;
        }
        
        throw new ParseException($"Unexpected token: {Peek().Type}", location);
    }
    
    // Helper methods
    
    private string ParseQualifiedName()
    {
        var parts = new List<string>();
        parts.Add(Consume(TokenType.Identifier, "Expected identifier").Value);
        
        while (Match(TokenType.Dot) || Match(TokenType.DoubleColon))
        {
            parts.Add(Consume(TokenType.Identifier, "Expected identifier after '.' or '::'").Value);
        }
        
        return string.Join(".", parts);
    }
    
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }
    
    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }
    
    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }
    
    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EndOfFile;
    }
    
    private Token Peek()
    {
        return _tokens[_current];
    }
    
    private Token PeekAhead(int offset)
    {
        int index = _current + offset;
        if (index >= _tokens.Count) return _tokens[^1]; // Return EOF
        return _tokens[index];
    }
    
    private Token Previous()
    {
        return _tokens[_current - 1];
    }
    
    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new ParseException(message, CurrentLocation());
    }
    
    private SourceLocation CurrentLocation()
    {
        return Peek().Location;
    }
    
    private SourceLocation PreviousLocation()
    {
        return Previous().Location;
    }
}

/// <summary>
/// Exception thrown during parsing
/// </summary>
public class ParseException : Exception
{
    public SourceLocation Location { get; }
    
    public ParseException(string message, SourceLocation location) : base(message)
    {
        Location = location;
    }
}
