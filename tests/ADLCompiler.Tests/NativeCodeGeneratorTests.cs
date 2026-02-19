using ADLCompiler.CodeGeneration;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests
{
    /// <summary>
    /// Unit tests for NativeCodeGenerator
    /// </summary>
    public class NativeCodeGeneratorTests
    {
        [Fact]
        public void GenerateFunction_SimpleFunction_GeneratesCode()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "add",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>
                {
                    new ParameterDeclaration { Name = "a", ParameterType = new TypeReference { Name = "int" } },
                    new ParameterDeclaration { Name = "b", ParameterType = new TypeReference { Name = "int" } }
                },
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new BinaryExpression
                            {
                                Left = new IdentifierExpression { Name = "a" },
                                Operator = BinaryOperator.Add,
                                Right = new IdentifierExpression { Name = "b" }
                            }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.Equal("add", function.FunctionName);
            Assert.Equal(Architecture.x86_64, function.Architecture);
            Assert.True(function.CodeSize > 0);
            Assert.NotEmpty(function.MachineCode);
        }

        [Fact]
        public void GenerateFunction_ARMArchitecture_GeneratesARMCode()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.ARM);
            
            var method = new MethodDeclaration
            {
                Name = "square",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>
                {
                    new ParameterDeclaration { Name = "x", ParameterType = new TypeReference { Name = "int" } }
                },
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new BinaryExpression
                            {
                                Left = new IdentifierExpression { Name = "x" },
                                Operator = BinaryOperator.Multiply,
                                Right = new IdentifierExpression { Name = "x" }
                            }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.Equal("square", function.FunctionName);
            Assert.Equal(Architecture.ARM, function.Architecture);
            Assert.Equal(CallingConvention.ARM_AAPCS, function.Convention);
            Assert.True(function.CodeSize > 0);
        }

        [Fact]
        public void GenerateFunction_WithLocalVariables_AllocatesStackSpace()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "calculate",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>(),
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new VariableDeclarationStatement
                        {
                            Name = "result",
                            VariableType = new TypeReference { Name = "int" },
                            Initializer = new LiteralExpression { Value = 42, LiteralType = LiteralType.Integer }
                        },
                        new ReturnStatement
                        {
                            Value = new IdentifierExpression { Name = "result" }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.True(function.StackFrameSize > 0);
        }

        [Fact]
        public void GenerateFunction_WithPointerParameter_HandlesCppStyle()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "processPointer",
                ReturnType = new TypeReference { Name = "void" },
                Parameters = new List<ParameterDeclaration>
                {
                    new ParameterDeclaration 
                    { 
                        Name = "ptr", 
                        ParameterType = new TypeReference { Name = "int", IsPointer = true, PointerLevel = 1 }
                    }
                },
                Body = new BlockStatement
                {
                    Statements = new List<Statement>()
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.Equal("processPointer", function.FunctionName);
        }

        [Fact]
        public void GenerateFunction_WithMethodCall_CreatesRelocation()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "caller",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>(),
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new MethodCallExpression
                            {
                                MethodName = "helper",
                                Arguments = new List<Expression>()
                            }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.NotEmpty(function.Relocations);
            Assert.Contains(function.Relocations, r => r.TargetSymbol == "helper");
        }

        [Fact]
        public void GenerateClass_CppStyleClass_GeneratesMethodsAsNativeFunctions()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var classDecl = new ClassDeclaration
            {
                Name = "Calculator",
                Methods = new List<MethodDeclaration>
                {
                    new MethodDeclaration
                    {
                        Name = "add",
                        ReturnType = new TypeReference { Name = "int" },
                        Parameters = new List<ParameterDeclaration>
                        {
                            new ParameterDeclaration { Name = "a", ParameterType = new TypeReference { Name = "int" } },
                            new ParameterDeclaration { Name = "b", ParameterType = new TypeReference { Name = "int" } }
                        },
                        Body = new BlockStatement { Statements = new List<Statement>() }
                    }
                },
                Fields = new List<FieldDeclaration>
                {
                    new FieldDeclaration 
                    { 
                        Name = "data", 
                        FieldType = new TypeReference { Name = "int", IsPointer = true }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Classes = new List<ClassDeclaration> { classDecl }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.Equal("Calculator::add", function.FunctionName);
        }

        [Fact]
        public void GenerateExpression_LiteralInteger_LoadsImmediate()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "returnConstant",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>(),
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new LiteralExpression { Value = 123, LiteralType = LiteralType.Integer }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.True(function.CodeSize > 0);
        }

        [Fact]
        public void GenerateExpression_BinaryOperations_GeneratesCorrectInstructions()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "compute",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>(),
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new BinaryExpression
                            {
                                Left = new LiteralExpression { Value = 10, LiteralType = LiteralType.Integer },
                                Operator = BinaryOperator.Subtract,
                                Right = new LiteralExpression { Value = 5, LiteralType = LiteralType.Integer }
                            }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.True(function.CodeSize > 0);
        }

        [Fact]
        public void GenerateExpression_UnaryNegate_GeneratesNegateInstruction()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "negate",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>
                {
                    new ParameterDeclaration { Name = "x", ParameterType = new TypeReference { Name = "int" } }
                },
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new UnaryExpression
                            {
                                Operator = UnaryOperator.Negate,
                                Operand = new IdentifierExpression { Name = "x" }
                            }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.True(function.CodeSize > 0);
        }

        [Fact]
        public void GenerateExpression_NewExpression_GeneratesAllocationCall()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "allocate",
                ReturnType = new TypeReference { Name = "int", IsPointer = true },
                Parameters = new List<ParameterDeclaration>(),
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "int" },
                                Arguments = new List<Expression>()
                            }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.Contains(function.Relocations, r => r.TargetSymbol == "__allocate_memory");
        }

        [Fact]
        public void GenerateStatement_IfStatement_GeneratesConditionalBranch()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var method = new MethodDeclaration
            {
                Name = "conditional",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>
                {
                    new ParameterDeclaration { Name = "x", ParameterType = new TypeReference { Name = "int" } }
                },
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new IfStatement
                        {
                            Condition = new IdentifierExpression { Name = "x" },
                            ThenBranch = new ReturnStatement
                            {
                                Value = new LiteralExpression { Value = 1, LiteralType = LiteralType.Integer }
                            },
                            ElseBranch = new ReturnStatement
                            {
                                Value = new LiteralExpression { Value = 0, LiteralType = LiteralType.Integer }
                            }
                        }
                    }
                }
            };
            
            var unit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Functions = new List<MethodDeclaration> { method }
                }
            };
            
            // Act
            generator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(nativeSection.Functions);
            var function = nativeSection.Functions[0];
            Assert.True(function.CodeSize > 0);
        }
    }
}
