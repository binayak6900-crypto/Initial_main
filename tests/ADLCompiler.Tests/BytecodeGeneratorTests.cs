using Xunit;
using ADLCompiler.CodeGeneration;
using SharedUtilities.Models;
using System.Linq;

namespace ADLCompiler.Tests
{
    public class BytecodeGeneratorTests
    {
        private BytecodeSection _bytecodeSection;
        private ResourceSection _resources;
        private BytecodeGenerator _generator;

        public BytecodeGeneratorTests()
        {
            _bytecodeSection = new BytecodeSection();
            _resources = new ResourceSection();
            _generator = new BytecodeGenerator(_bytecodeSection, _resources);
        }

        [Fact]
        public void GenerateClass_CreatesBasicClass()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                AccessModifier = AccessModifier.Public,
                SuperClass = "java.lang.Object"
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            Assert.Single(_bytecodeSection.Classes);
            var bytecodeClass = _bytecodeSection.Classes[0];
            Assert.Equal("TestClass", bytecodeClass.ClassName);
            Assert.Equal("java.lang.Object", bytecodeClass.SuperClassName);
            Assert.True(bytecodeClass.AccessFlags.HasFlag(AccessFlags.Public));
        }

        [Fact]
        public void GenerateClass_WithFields_CreatesFields()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Fields = new()
                {
                    new FieldDeclaration
                    {
                        Name = "count",
                        FieldType = new TypeReference { Name = "int" },
                        AccessModifier = AccessModifier.Private
                    },
                    new FieldDeclaration
                    {
                        Name = "name",
                        FieldType = new TypeReference { Name = "String" },
                        AccessModifier = AccessModifier.Public,
                        IsStatic = true
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var bytecodeClass = _bytecodeSection.Classes[0];
            Assert.Equal(2, bytecodeClass.Fields.Count);
            
            var countField = bytecodeClass.Fields[0];
            Assert.Equal("count", countField.Name);
            Assert.Equal("I", countField.TypeDescriptor);
            Assert.True(countField.AccessFlags.HasFlag(AccessFlags.Private));

            var nameField = bytecodeClass.Fields[1];
            Assert.Equal("name", nameField.Name);
            Assert.True(nameField.AccessFlags.HasFlag(AccessFlags.Public));
            Assert.True(nameField.AccessFlags.HasFlag(AccessFlags.Static));
        }

        [Fact]
        public void GenerateMethod_EmptyVoidMethod_GeneratesReturnVoid()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "doNothing",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement()
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.Equal("doNothing", method.Name);
            Assert.Single(method.Instructions);
            Assert.Equal(Opcode.RETURN_VOID, method.Instructions[0].Opcode);
        }

        [Fact]
        public void GenerateMethod_WithParameters_AllocatesLocals()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "add",
                        ReturnType = new TypeReference { Name = "int" },
                        Parameters = new()
                        {
                            new ParameterDeclaration
                            {
                                Name = "a",
                                ParameterType = new TypeReference { Name = "int" }
                            },
                            new ParameterDeclaration
                            {
                                Name = "b",
                                ParameterType = new TypeReference { Name = "int" }
                            }
                        },
                        Body = new BlockStatement()
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.True(method.MaxLocals >= 3); // this + a + b
        }

        [Fact]
        public void GenerateLiteral_Integer_GeneratesConstInstruction()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "getNumber",
                        ReturnType = new TypeReference { Name = "int" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new ReturnStatement
                                {
                                    Value = new LiteralExpression
                                    {
                                        Value = 42,
                                        LiteralType = LiteralType.Integer,
                                        EvaluatedType = new TypeReference { Name = "int" }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.True(method.Instructions.Count >= 2); // const + return
            Assert.Contains(method.Instructions, i => 
                i.Opcode == Opcode.CONST_4 || i.Opcode == Opcode.CONST_16 || i.Opcode == Opcode.CONST);
            Assert.Contains(method.Instructions, i => i.Opcode == Opcode.RETURN);
        }

        [Fact]
        public void GenerateLiteral_String_AddsToResourcePool()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "getMessage",
                        ReturnType = new TypeReference { Name = "String" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new ReturnStatement
                                {
                                    Value = new LiteralExpression
                                    {
                                        Value = "Hello, World!",
                                        LiteralType = LiteralType.String
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.Contains(method.Instructions, i => i.Opcode == Opcode.CONST_STRING);
            Assert.True(_resources.Strings.Count > 0);
        }

        [Fact]
        public void GenerateBinaryExpression_Addition_GeneratesAddInstruction()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "add",
                        ReturnType = new TypeReference { Name = "int" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new ReturnStatement
                                {
                                    Value = new BinaryExpression
                                    {
                                        Left = new LiteralExpression { Value = 5, LiteralType = LiteralType.Integer },
                                        Operator = BinaryOperator.Add,
                                        Right = new LiteralExpression { Value = 3, LiteralType = LiteralType.Integer }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.Contains(method.Instructions, i => i.Opcode == Opcode.ADD_INT);
        }

        [Fact]
        public void GenerateBinaryExpression_Arithmetic_GeneratesCorrectOpcodes()
        {
            // Test multiple arithmetic operations
            var operations = new[]
            {
                (BinaryOperator.Subtract, Opcode.SUB_INT),
                (BinaryOperator.Multiply, Opcode.MUL_INT),
                (BinaryOperator.Divide, Opcode.DIV_INT),
                (BinaryOperator.Modulo, Opcode.REM_INT)
            };

            foreach (var (op, expectedOpcode) in operations)
            {
                // Arrange
                var bytecodeSection = new BytecodeSection();
                var resources = new ResourceSection();
                var generator = new BytecodeGenerator(bytecodeSection, resources);

                var classDecl = new ClassDeclaration
                {
                    Name = "TestClass",
                    Methods = new()
                    {
                        new MethodDeclaration
                        {
                            Name = "calculate",
                            ReturnType = new TypeReference { Name = "int" },
                            Body = new BlockStatement
                            {
                                Statements = new()
                                {
                                    new ReturnStatement
                                    {
                                        Value = new BinaryExpression
                                        {
                                            Left = new LiteralExpression { Value = 10, LiteralType = LiteralType.Integer },
                                            Operator = op,
                                            Right = new LiteralExpression { Value = 2, LiteralType = LiteralType.Integer }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                // Act
                generator.GenerateClass(classDecl);

                // Assert
                var method = bytecodeSection.Classes[0].Methods[0];
                Assert.Contains(method.Instructions, i => i.Opcode == expectedOpcode);
            }
        }

        [Fact]
        public void GenerateIfStatement_GeneratesConditionalBranch()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "test",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new IfStatement
                                {
                                    Condition = new LiteralExpression { Value = true, LiteralType = LiteralType.Boolean },
                                    ThenBranch = new BlockStatement()
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.Contains(method.Instructions, i => i.Opcode == Opcode.IF_EQ);
        }

        [Fact]
        public void GenerateWhileStatement_GeneratesLoop()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "loop",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new WhileStatement
                                {
                                    Condition = new LiteralExpression { Value = true, LiteralType = LiteralType.Boolean },
                                    Body = new BlockStatement()
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            // While loop should have conditional branch
            Assert.Contains(method.Instructions, i => i.Opcode == Opcode.IF_EQ);
        }

        [Fact]
        public void GenerateForStatement_GeneratesLoopWithInitAndIncrement()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "forLoop",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new ForStatement
                                {
                                    Initializer = new VariableDeclarationStatement
                                    {
                                        Name = "i",
                                        VariableType = new TypeReference { Name = "int" },
                                        Initializer = new LiteralExpression { Value = 0, LiteralType = LiteralType.Integer }
                                    },
                                    Condition = new BinaryExpression
                                    {
                                        Left = new IdentifierExpression { Name = "i" },
                                        Operator = BinaryOperator.Less,
                                        Right = new LiteralExpression { Value = 10, LiteralType = LiteralType.Integer }
                                    },
                                    Increment = new AssignmentExpression
                                    {
                                        Target = new IdentifierExpression { Name = "i" },
                                        Value = new BinaryExpression
                                        {
                                            Left = new IdentifierExpression { Name = "i" },
                                            Operator = BinaryOperator.Add,
                                            Right = new LiteralExpression { Value = 1, LiteralType = LiteralType.Integer }
                                        }
                                    },
                                    Body = new BlockStatement()
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.True(method.Instructions.Count > 0);
            Assert.True(method.MaxLocals >= 2); // this + i
        }

        [Fact]
        public void GenerateMethodCall_Static_GeneratesInvokeStatic()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "callStatic",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new ExpressionStatement
                                {
                                    Expression = new MethodCallExpression
                                    {
                                        MethodName = "staticMethod",
                                        Arguments = new()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.Contains(method.Instructions, i => i.Opcode == Opcode.INVOKE_STATIC);
        }

        [Fact]
        public void GenerateNewExpression_Object_GeneratesNewInstance()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "createObject",
                        ReturnType = new TypeReference { Name = "Object" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new ReturnStatement
                                {
                                    Value = new NewExpression
                                    {
                                        TypeToCreate = new TypeReference { Name = "Object" },
                                        IsArray = false
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.Contains(method.Instructions, i => i.Opcode == Opcode.NEW_INSTANCE);
        }

        [Fact]
        public void GenerateNewExpression_Array_GeneratesNewArray()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "createArray",
                        ReturnType = new TypeReference { Name = "int", IsArray = true },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new ReturnStatement
                                {
                                    Value = new NewExpression
                                    {
                                        TypeToCreate = new TypeReference { Name = "int" },
                                        IsArray = true,
                                        Arguments = new()
                                        {
                                            new LiteralExpression { Value = 10, LiteralType = LiteralType.Integer }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.Contains(method.Instructions, i => i.Opcode == Opcode.NEW_ARRAY);
        }

        [Fact]
        public void GenerateVariableDeclaration_AllocatesLocal()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "declareVar",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new VariableDeclarationStatement
                                {
                                    Name = "x",
                                    VariableType = new TypeReference { Name = "int" },
                                    Initializer = new LiteralExpression { Value = 42, LiteralType = LiteralType.Integer }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.True(method.MaxLocals >= 2); // this + x
        }

        [Fact]
        public void GenerateMethod_TracksMaxStack()
        {
            // Arrange
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "complexExpression",
                        ReturnType = new TypeReference { Name = "int" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new ReturnStatement
                                {
                                    Value = new BinaryExpression
                                    {
                                        Left = new BinaryExpression
                                        {
                                            Left = new LiteralExpression { Value = 1, LiteralType = LiteralType.Integer },
                                            Operator = BinaryOperator.Add,
                                            Right = new LiteralExpression { Value = 2, LiteralType = LiteralType.Integer }
                                        },
                                        Operator = BinaryOperator.Multiply,
                                        Right = new LiteralExpression { Value = 3, LiteralType = LiteralType.Integer }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _generator.GenerateClass(classDecl);

            // Assert
            var method = _bytecodeSection.Classes[0].Methods[0];
            Assert.True(method.MaxStack > 0);
        }
    }
}
