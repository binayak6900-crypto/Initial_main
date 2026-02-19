using Xunit;
using ADLCompiler.CodeGeneration;
using SharedUtilities.Models;
using System.Linq;

namespace ADLCompiler.Tests
{
    /// <summary>
    /// Integration tests for bytecode generation from complete AST structures
    /// </summary>
    public class BytecodeGeneratorIntegrationTests
    {
        [Fact]
        public void GenerateCompilationUnit_SimpleClass_GeneratesCompleteBytecode()
        {
            // Arrange
            var bytecodeSection = new BytecodeSection();
            var resources = new ResourceSection();
            var generator = new BytecodeGenerator(bytecodeSection, resources);

            var unit = new CompilationUnit
            {
                FileName = "Test.adl",
                Classes = new()
                {
                    new ClassDeclaration
                    {
                        Name = "Calculator",
                        AccessModifier = AccessModifier.Public,
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
                                Body = new BlockStatement
                                {
                                    Statements = new()
                                    {
                                        new ReturnStatement
                                        {
                                            Value = new BinaryExpression
                                            {
                                                Left = new IdentifierExpression
                                                {
                                                    Name = "a",
                                                    EvaluatedType = new TypeReference { Name = "int" }
                                                },
                                                Operator = BinaryOperator.Add,
                                                Right = new IdentifierExpression
                                                {
                                                    Name = "b",
                                                    EvaluatedType = new TypeReference { Name = "int" }
                                                },
                                                EvaluatedType = new TypeReference { Name = "int" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            generator.GenerateCompilationUnit(unit);

            // Assert
            Assert.Single(bytecodeSection.Classes);
            var calculatorClass = bytecodeSection.Classes[0];
            Assert.Equal("Calculator", calculatorClass.ClassName);
            Assert.Single(calculatorClass.Methods);
            
            var addMethod = calculatorClass.Methods[0];
            Assert.Equal("add", addMethod.Name);
            Assert.True(addMethod.Instructions.Count > 0);
            Assert.Contains(addMethod.Instructions, i => i.Opcode == Opcode.ADD_INT);
            Assert.Contains(addMethod.Instructions, i => i.Opcode == Opcode.RETURN);
        }

        [Fact]
        public void GenerateCompilationUnit_ClassWithFields_GeneratesFieldsAndMethods()
        {
            // Arrange
            var bytecodeSection = new BytecodeSection();
            var resources = new ResourceSection();
            var generator = new BytecodeGenerator(bytecodeSection, resources);

            var unit = new CompilationUnit
            {
                FileName = "Counter.adl",
                Classes = new()
                {
                    new ClassDeclaration
                    {
                        Name = "Counter",
                        AccessModifier = AccessModifier.Public,
                        Fields = new()
                        {
                            new FieldDeclaration
                            {
                                Name = "count",
                                FieldType = new TypeReference { Name = "int" },
                                AccessModifier = AccessModifier.Private,
                                Initializer = new LiteralExpression
                                {
                                    Value = 0,
                                    LiteralType = LiteralType.Integer
                                }
                            }
                        },
                        Methods = new()
                        {
                            new MethodDeclaration
                            {
                                Name = "increment",
                                ReturnType = new TypeReference { Name = "void" },
                                Body = new BlockStatement()
                            }
                        }
                    }
                }
            };

            // Act
            generator.GenerateCompilationUnit(unit);

            // Assert
            var counterClass = bytecodeSection.Classes[0];
            Assert.Equal("Counter", counterClass.ClassName);
            Assert.Single(counterClass.Fields);
            Assert.Single(counterClass.Methods);
            
            var countField = counterClass.Fields[0];
            Assert.Equal("count", countField.Name);
            Assert.Equal("I", countField.TypeDescriptor);
            Assert.Equal(0, countField.InitialValue);
        }

        [Fact]
        public void GenerateCompilationUnit_ControlFlow_GeneratesCorrectBranches()
        {
            // Arrange
            var bytecodeSection = new BytecodeSection();
            var resources = new ResourceSection();
            var generator = new BytecodeGenerator(bytecodeSection, resources);

            var unit = new CompilationUnit
            {
                FileName = "Logic.adl",
                Classes = new()
                {
                    new ClassDeclaration
                    {
                        Name = "Logic",
                        Methods = new()
                        {
                            new MethodDeclaration
                            {
                                Name = "max",
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
                                Body = new BlockStatement
                                {
                                    Statements = new()
                                    {
                                        new IfStatement
                                        {
                                            Condition = new BinaryExpression
                                            {
                                                Left = new IdentifierExpression
                                                {
                                                    Name = "a",
                                                    EvaluatedType = new TypeReference { Name = "int" }
                                                },
                                                Operator = BinaryOperator.Greater,
                                                Right = new IdentifierExpression
                                                {
                                                    Name = "b",
                                                    EvaluatedType = new TypeReference { Name = "int" }
                                                }
                                            },
                                            ThenBranch = new ReturnStatement
                                            {
                                                Value = new IdentifierExpression
                                                {
                                                    Name = "a",
                                                    EvaluatedType = new TypeReference { Name = "int" }
                                                }
                                            },
                                            ElseBranch = new ReturnStatement
                                            {
                                                Value = new IdentifierExpression
                                                {
                                                    Name = "b",
                                                    EvaluatedType = new TypeReference { Name = "int" }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            generator.GenerateCompilationUnit(unit);

            // Assert
            var logicClass = bytecodeSection.Classes[0];
            var maxMethod = logicClass.Methods[0];
            
            // Should have conditional branch for if statement
            Assert.Contains(maxMethod.Instructions, i => 
                i.Opcode == Opcode.IF_GT || i.Opcode == Opcode.IF_LE);
            
            // Should have multiple return statements
            var returnCount = maxMethod.Instructions.Count(i => i.Opcode == Opcode.RETURN);
            Assert.True(returnCount >= 2);
        }

        [Fact]
        public void GenerateCompilationUnit_Loop_GeneratesLoopBytecode()
        {
            // Arrange
            var bytecodeSection = new BytecodeSection();
            var resources = new ResourceSection();
            var generator = new BytecodeGenerator(bytecodeSection, resources);

            var unit = new CompilationUnit
            {
                FileName = "Loop.adl",
                Classes = new()
                {
                    new ClassDeclaration
                    {
                        Name = "Loop",
                        Methods = new()
                        {
                            new MethodDeclaration
                            {
                                Name = "sum",
                                ReturnType = new TypeReference { Name = "int" },
                                Parameters = new()
                                {
                                    new ParameterDeclaration
                                    {
                                        Name = "n",
                                        ParameterType = new TypeReference { Name = "int" }
                                    }
                                },
                                Body = new BlockStatement
                                {
                                    Statements = new()
                                    {
                                        new VariableDeclarationStatement
                                        {
                                            Name = "sum",
                                            VariableType = new TypeReference { Name = "int" },
                                            Initializer = new LiteralExpression
                                            {
                                                Value = 0,
                                                LiteralType = LiteralType.Integer
                                            }
                                        },
                                        new VariableDeclarationStatement
                                        {
                                            Name = "i",
                                            VariableType = new TypeReference { Name = "int" },
                                            Initializer = new LiteralExpression
                                            {
                                                Value = 0,
                                                LiteralType = LiteralType.Integer
                                            }
                                        },
                                        new WhileStatement
                                        {
                                            Condition = new BinaryExpression
                                            {
                                                Left = new IdentifierExpression
                                                {
                                                    Name = "i",
                                                    EvaluatedType = new TypeReference { Name = "int" }
                                                },
                                                Operator = BinaryOperator.Less,
                                                Right = new IdentifierExpression
                                                {
                                                    Name = "n",
                                                    EvaluatedType = new TypeReference { Name = "int" }
                                                }
                                            },
                                            Body = new BlockStatement()
                                        },
                                        new ReturnStatement
                                        {
                                            Value = new IdentifierExpression
                                            {
                                                Name = "sum",
                                                EvaluatedType = new TypeReference { Name = "int" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            generator.GenerateCompilationUnit(unit);

            // Assert
            var loopClass = bytecodeSection.Classes[0];
            var sumMethod = loopClass.Methods[0];
            
            // Should have conditional branch for while loop
            Assert.Contains(sumMethod.Instructions, i => i.Opcode == Opcode.IF_EQ);
            
            // Should allocate local variables
            Assert.True(sumMethod.MaxLocals >= 4); // this + n + sum + i
        }

        [Fact]
        public void GenerateCompilationUnit_StringLiterals_AddsToResourcePool()
        {
            // Arrange
            var bytecodeSection = new BytecodeSection();
            var resources = new ResourceSection();
            var generator = new BytecodeGenerator(bytecodeSection, resources);

            var unit = new CompilationUnit
            {
                FileName = "Strings.adl",
                Classes = new()
                {
                    new ClassDeclaration
                    {
                        Name = "Strings",
                        Methods = new()
                        {
                            new MethodDeclaration
                            {
                                Name = "greet",
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
                    }
                }
            };

            // Act
            generator.GenerateCompilationUnit(unit);

            // Assert
            var stringsClass = bytecodeSection.Classes[0];
            var greetMethod = stringsClass.Methods[0];
            
            // Should have const_string instruction
            Assert.Contains(greetMethod.Instructions, i => i.Opcode == Opcode.CONST_STRING);
            
            // Should add string to resource pool
            Assert.True(resources.Strings.Count > 0);
        }
    }
}
