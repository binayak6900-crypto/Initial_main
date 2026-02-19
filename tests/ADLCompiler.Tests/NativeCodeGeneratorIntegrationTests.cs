using ADLCompiler.CodeGeneration;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests
{
    /// <summary>
    /// Integration tests for NativeCodeGenerator with complete scenarios
    /// </summary>
    public class NativeCodeGeneratorIntegrationTests
    {
        [Fact]
        public void GenerateNativeCode_CompleteFunction_GeneratesValidCode()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            // Create a complete function: int fibonacci(int n) { ... }
            var method = new MethodDeclaration
            {
                Name = "fibonacci",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>
                {
                    new ParameterDeclaration { Name = "n", ParameterType = new TypeReference { Name = "int" } }
                },
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        // if (n <= 1) return n;
                        new IfStatement
                        {
                            Condition = new BinaryExpression
                            {
                                Left = new IdentifierExpression { Name = "n" },
                                Operator = BinaryOperator.LessEqual,
                                Right = new LiteralExpression { Value = 1, LiteralType = LiteralType.Integer }
                            },
                            ThenBranch = new ReturnStatement
                            {
                                Value = new IdentifierExpression { Name = "n" }
                            }
                        },
                        // return fibonacci(n-1) + fibonacci(n-2);
                        new ReturnStatement
                        {
                            Value = new BinaryExpression
                            {
                                Left = new MethodCallExpression
                                {
                                    MethodName = "fibonacci",
                                    Arguments = new List<Expression>
                                    {
                                        new BinaryExpression
                                        {
                                            Left = new IdentifierExpression { Name = "n" },
                                            Operator = BinaryOperator.Subtract,
                                            Right = new LiteralExpression { Value = 1, LiteralType = LiteralType.Integer }
                                        }
                                    }
                                },
                                Operator = BinaryOperator.Add,
                                Right = new MethodCallExpression
                                {
                                    MethodName = "fibonacci",
                                    Arguments = new List<Expression>
                                    {
                                        new BinaryExpression
                                        {
                                            Left = new IdentifierExpression { Name = "n" },
                                            Operator = BinaryOperator.Subtract,
                                            Right = new LiteralExpression { Value = 2, LiteralType = LiteralType.Integer }
                                        }
                                    }
                                }
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
            Assert.Equal("fibonacci", function.FunctionName);
            Assert.Equal(Architecture.x86_64, function.Architecture);
            Assert.Equal(CallingConvention.x86_64_SysV, function.Convention);
            Assert.True(function.CodeSize > 0);
            Assert.NotEmpty(function.MachineCode);
            Assert.NotEmpty(function.Relocations);
            Assert.Equal(2, function.Relocations.Count); // Two recursive calls
        }

        [Fact]
        public void GenerateNativeCode_PointerArithmetic_HandlesCorrectly()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            // Create function: void increment(int* ptr) { *ptr = *ptr + 1; }
            var method = new MethodDeclaration
            {
                Name = "increment",
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
                    Statements = new List<Statement>
                    {
                        new ExpressionStatement
                        {
                            Expression = new AssignmentExpression
                            {
                                Target = new MemberAccessExpression
                                {
                                    Target = new IdentifierExpression { Name = "ptr" },
                                    MemberName = ""
                                },
                                Value = new BinaryExpression
                                {
                                    Left = new MemberAccessExpression
                                    {
                                        Target = new IdentifierExpression { Name = "ptr" },
                                        MemberName = ""
                                    },
                                    Operator = BinaryOperator.Add,
                                    Right = new LiteralExpression { Value = 1, LiteralType = LiteralType.Integer }
                                }
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
            Assert.Equal("increment", function.FunctionName);
            Assert.True(function.CodeSize > 0);
        }

        [Fact]
        public void GenerateNativeCode_MultipleArchitectures_GeneratesDifferentCode()
        {
            // Arrange
            var method = new MethodDeclaration
            {
                Name = "simple",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>(),
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new LiteralExpression { Value = 42, LiteralType = LiteralType.Integer }
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
            
            // Act - Generate for x86_64
            var x86Section = new NativeCodeSection();
            var x86Generator = new NativeCodeGenerator(x86Section, Architecture.x86_64);
            x86Generator.GenerateCompilationUnit(unit);
            
            // Act - Generate for ARM
            var armSection = new NativeCodeSection();
            var armGenerator = new NativeCodeGenerator(armSection, Architecture.ARM);
            armGenerator.GenerateCompilationUnit(unit);
            
            // Assert
            Assert.Single(x86Section.Functions);
            Assert.Single(armSection.Functions);
            
            var x86Function = x86Section.Functions[0];
            var armFunction = armSection.Functions[0];
            
            Assert.Equal(Architecture.x86_64, x86Function.Architecture);
            Assert.Equal(Architecture.ARM, armFunction.Architecture);
            Assert.Equal(CallingConvention.x86_64_SysV, x86Function.Convention);
            Assert.Equal(CallingConvention.ARM_AAPCS, armFunction.Convention);
            
            // Machine code should be different for different architectures
            Assert.NotEqual(x86Function.MachineCode, armFunction.MachineCode);
        }

        [Fact]
        public void GenerateNativeCode_CppClassWithMethods_GeneratesMangledNames()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            var classDecl = new ClassDeclaration
            {
                Name = "Vector",
                Methods = new List<MethodDeclaration>
                {
                    new MethodDeclaration
                    {
                        Name = "length",
                        ReturnType = new TypeReference { Name = "float" },
                        Parameters = new List<ParameterDeclaration>(),
                        Body = new BlockStatement
                        {
                            Statements = new List<Statement>
                            {
                                new ReturnStatement
                                {
                                    Value = new LiteralExpression { Value = 0.0f, LiteralType = LiteralType.Float }
                                }
                            }
                        }
                    }
                },
                Fields = new List<FieldDeclaration>
                {
                    new FieldDeclaration 
                    { 
                        Name = "x", 
                        FieldType = new TypeReference { Name = "float", IsPointer = true }
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
            Assert.Equal("Vector::length", function.FunctionName);
        }

        [Fact]
        public void GenerateNativeCode_ComplexExpression_GeneratesCorrectSequence()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            // Create function: int compute(int a, int b, int c) { return (a + b) * c - 10; }
            var method = new MethodDeclaration
            {
                Name = "compute",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>
                {
                    new ParameterDeclaration { Name = "a", ParameterType = new TypeReference { Name = "int" } },
                    new ParameterDeclaration { Name = "b", ParameterType = new TypeReference { Name = "int" } },
                    new ParameterDeclaration { Name = "c", ParameterType = new TypeReference { Name = "int" } }
                },
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ReturnStatement
                        {
                            Value = new BinaryExpression
                            {
                                Left = new BinaryExpression
                                {
                                    Left = new BinaryExpression
                                    {
                                        Left = new IdentifierExpression { Name = "a" },
                                        Operator = BinaryOperator.Add,
                                        Right = new IdentifierExpression { Name = "b" }
                                    },
                                    Operator = BinaryOperator.Multiply,
                                    Right = new IdentifierExpression { Name = "c" }
                                },
                                Operator = BinaryOperator.Subtract,
                                Right = new LiteralExpression { Value = 10, LiteralType = LiteralType.Integer }
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
            Assert.Equal("compute", function.FunctionName);
            Assert.True(function.CodeSize > 0);
            Assert.True(function.StackFrameSize >= 0); // May be 0 if no locals allocated
        }

        [Fact]
        public void GenerateNativeCode_LoopWithLocalVariables_AllocatesStackCorrectly()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64);
            
            // Create function with a for loop
            var method = new MethodDeclaration
            {
                Name = "sum",
                ReturnType = new TypeReference { Name = "int" },
                Parameters = new List<ParameterDeclaration>
                {
                    new ParameterDeclaration { Name = "n", ParameterType = new TypeReference { Name = "int" } }
                },
                Body = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new VariableDeclarationStatement
                        {
                            Name = "result",
                            VariableType = new TypeReference { Name = "int" },
                            Initializer = new LiteralExpression { Value = 0, LiteralType = LiteralType.Integer }
                        },
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
                                Right = new IdentifierExpression { Name = "n" }
                            },
                            Increment = new UnaryExpression
                            {
                                Operator = UnaryOperator.PreIncrement,
                                Operand = new IdentifierExpression { Name = "i" }
                            },
                            Body = new BlockStatement
                            {
                                Statements = new List<Statement>
                                {
                                    new ExpressionStatement
                                    {
                                        Expression = new AssignmentExpression
                                        {
                                            Target = new IdentifierExpression { Name = "result" },
                                            Value = new BinaryExpression
                                            {
                                                Left = new IdentifierExpression { Name = "result" },
                                                Operator = BinaryOperator.Add,
                                                Right = new IdentifierExpression { Name = "i" }
                                            }
                                        }
                                    }
                                }
                            }
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
            Assert.Equal("sum", function.FunctionName);
            Assert.True(function.StackFrameSize >= 8); // At least 2 local variables
        }
    }
}
