using Xunit;
using ADLCompiler.SemanticAnalysis;
using SharedUtilities.Models;
using System.Linq;

namespace ADLCompiler.Tests
{
    public class MemoryManagementTests
    {
        [Fact]
        public void Analyze_SimplePointerAllocation_CreatesAllocation()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateSimplePointerAllocation();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.Single(instructions.Allocations);
            var alloc = instructions.Allocations[0];
            Assert.Equal("img", alloc.VariableName);
            Assert.True(alloc.IsPointer);
            Assert.Equal(AllocationKind.New, alloc.Kind);
        }

        [Fact]
        public void Analyze_ArrayAllocation_CreatesArrayAllocation()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateArrayAllocation();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.Single(instructions.Allocations);
            var alloc = instructions.Allocations[0];
            Assert.Equal("arr", alloc.VariableName);
            Assert.True(alloc.IsPointer);
            Assert.Equal(AllocationKind.NewArray, alloc.Kind);
        }

        [Fact]
        public void Analyze_SimplePointerAllocation_GeneratesRefCountOps()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateSimplePointerAllocation();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.NotEmpty(instructions.RefCountOps);
            
            // Should have Initialize, Decrement, and CheckAndDelete operations
            var initOps = instructions.RefCountOps.Where(op => op.Type == RefCountOpType.Initialize).ToList();
            var decOps = instructions.RefCountOps.Where(op => op.Type == RefCountOpType.Decrement).ToList();
            var checkOps = instructions.RefCountOps.Where(op => op.Type == RefCountOpType.CheckAndDelete).ToList();
            
            Assert.Single(initOps);
            Assert.Single(decOps);
            Assert.Single(checkOps);
        }

        [Fact]
        public void Analyze_SimplePointerAllocation_GeneratesDeallocations()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateSimplePointerAllocation();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.Single(instructions.Deallocations);
            var dealloc = instructions.Deallocations[0];
            Assert.Equal("img", dealloc.VariableName);
            Assert.Equal(DeallocationKind.Delete, dealloc.Kind);
        }

        [Fact]
        public void Analyze_ArrayAllocation_GeneratesArrayDeallocation()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateArrayAllocation();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.Single(instructions.Deallocations);
            var dealloc = instructions.Deallocations[0];
            Assert.Equal("arr", dealloc.VariableName);
            Assert.Equal(DeallocationKind.DeleteArray, dealloc.Kind);
        }

        [Fact]
        public void Analyze_MultipleAllocations_TracksAllAllocations()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateMultipleAllocations();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.Equal(2, instructions.Allocations.Count);
            Assert.Contains(instructions.Allocations, a => a.VariableName == "img1");
            Assert.Contains(instructions.Allocations, a => a.VariableName == "img2");
        }

        [Fact]
        public void Analyze_NestedScopes_TracksAllocationsInCorrectScope()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateNestedScopes();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.Equal(2, instructions.Allocations.Count);
            
            // Both allocations should have deallocations
            Assert.Equal(2, instructions.Deallocations.Count);
        }

        [Fact]
        public void GenerateSmartPointerConversions_CreatesConversionsForPointers()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateSimplePointerAllocation();
            var instructions = analyzer.Analyze(ast);

            // Act
            var conversions = analyzer.GenerateSmartPointerConversions(instructions);

            // Assert
            Assert.Single(conversions);
            var conversion = conversions[0];
            Assert.Equal("img", conversion.OriginalPointerName);
            Assert.Equal("__smart_img", conversion.SmartPointerName);
            Assert.Equal(SmartPointerType.Shared, conversion.Type);
        }

        [Fact]
        public void Analyze_ManualStrategy_ReturnsEmptyInstructions()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer(MemoryManagementStrategy.Manual);
            var ast = CreateSimplePointerAllocation();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.Empty(instructions.Allocations);
            Assert.Empty(instructions.RefCountOps);
            Assert.Empty(instructions.Deallocations);
        }

        [Fact]
        public void Analyze_FieldAllocation_TracksFieldAllocations()
        {
            // Arrange
            var analyzer = new MemoryManagementAnalyzer();
            var ast = CreateClassWithPointerField();

            // Act
            var instructions = analyzer.Analyze(ast);

            // Assert
            Assert.Single(instructions.Allocations);
            var alloc = instructions.Allocations[0];
            Assert.Equal("data", alloc.VariableName);
            Assert.True(alloc.IsPointer);
        }

        // Helper methods to create test ASTs

        private CompilationUnit CreateSimplePointerAllocation()
        {
            // void process() {
            //     Image* img = new Image(1920, 1080);
            // }
            
            var method = new MethodDeclaration
            {
                Name = "process",
                ReturnType = new TypeReference { Name = "void" },
                Body = new BlockStatement
                {
                    Statements = new()
                    {
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                            Name = "img",
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 1920, LiteralType = LiteralType.Integer },
                                    new LiteralExpression { Value = 1080, LiteralType = LiteralType.Integer }
                                },
                                IsArray = false
                            }
                        }
                    }
                }
            };

            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new() { method }
            };

            return new CompilationUnit
            {
                Classes = new() { classDecl }
            };
        }

        private CompilationUnit CreateArrayAllocation()
        {
            // void process() {
            //     int* arr = new int[100];
            // }
            
            var method = new MethodDeclaration
            {
                Name = "process",
                ReturnType = new TypeReference { Name = "void" },
                Body = new BlockStatement
                {
                    Statements = new()
                    {
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "int", IsPointer = true, PointerLevel = 1 },
                            Name = "arr",
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "int", IsArray = true },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 100, LiteralType = LiteralType.Integer }
                                },
                                IsArray = true
                            }
                        }
                    }
                }
            };

            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new() { method }
            };

            return new CompilationUnit
            {
                Classes = new() { classDecl }
            };
        }

        private CompilationUnit CreateMultipleAllocations()
        {
            // void process() {
            //     Image* img1 = new Image(1920, 1080);
            //     Image* img2 = new Image(800, 600);
            // }
            
            var method = new MethodDeclaration
            {
                Name = "process",
                ReturnType = new TypeReference { Name = "void" },
                Body = new BlockStatement
                {
                    Statements = new()
                    {
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                            Name = "img1",
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 1920, LiteralType = LiteralType.Integer },
                                    new LiteralExpression { Value = 1080, LiteralType = LiteralType.Integer }
                                },
                                IsArray = false
                            }
                        },
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                            Name = "img2",
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 800, LiteralType = LiteralType.Integer },
                                    new LiteralExpression { Value = 600, LiteralType = LiteralType.Integer }
                                },
                                IsArray = false
                            }
                        }
                    }
                }
            };

            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new() { method }
            };

            return new CompilationUnit
            {
                Classes = new() { classDecl }
            };
        }

        private CompilationUnit CreateNestedScopes()
        {
            // void process() {
            //     Image* img1 = new Image(1920, 1080);
            //     {
            //         Image* img2 = new Image(800, 600);
            //     }
            // }
            
            var method = new MethodDeclaration
            {
                Name = "process",
                ReturnType = new TypeReference { Name = "void" },
                Body = new BlockStatement
                {
                    Statements = new()
                    {
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                            Name = "img1",
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 1920, LiteralType = LiteralType.Integer },
                                    new LiteralExpression { Value = 1080, LiteralType = LiteralType.Integer }
                                },
                                IsArray = false
                            }
                        },
                        new BlockStatement
                        {
                            Statements = new()
                            {
                                new VariableDeclarationStatement
                                {
                                    VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                                    Name = "img2",
                                    Initializer = new NewExpression
                                    {
                                        TypeToCreate = new TypeReference { Name = "Image" },
                                        Arguments = new()
                                        {
                                            new LiteralExpression { Value = 800, LiteralType = LiteralType.Integer },
                                            new LiteralExpression { Value = 600, LiteralType = LiteralType.Integer }
                                        },
                                        IsArray = false
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new() { method }
            };

            return new CompilationUnit
            {
                Classes = new() { classDecl }
            };
        }

        private CompilationUnit CreateClassWithPointerField()
        {
            // class TestClass {
            //     private Image* data = new Image(100, 100);
            // }
            
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Fields = new()
                {
                    new FieldDeclaration
                    {
                        Name = "data",
                        FieldType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                        Initializer = new NewExpression
                        {
                            TypeToCreate = new TypeReference { Name = "Image" },
                            Arguments = new()
                            {
                                new LiteralExpression { Value = 100, LiteralType = LiteralType.Integer },
                                new LiteralExpression { Value = 100, LiteralType = LiteralType.Integer }
                            },
                            IsArray = false
                        }
                    }
                }
            };

            return new CompilationUnit
            {
                Classes = new() { classDecl }
            };
        }
    }
}
