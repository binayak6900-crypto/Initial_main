using Xunit;
using ADLCompiler.SemanticAnalysis;
using SharedUtilities.Models;
using System.Linq;

namespace ADLCompiler.Tests
{
    /// <summary>
    /// Integration tests for the complete memory management flow:
    /// Analysis -> Code Insertion -> Verification
    /// </summary>
    public class MemoryManagementIntegrationTests
    {
        [Fact]
        public void CompleteFlow_SimplePointerAllocation_InsertsAllMemoryManagementCode()
        {
            // Arrange
            var ast = CreateSimplePointerAllocation();
            
            // Act - Analyze
            var analyzer = new MemoryManagementAnalyzer();
            var instructions = analyzer.Analyze(ast);
            
            // Act - Insert
            var inserter = new MemoryManagementCodeInserter(instructions);
            inserter.InsertMemoryManagementCode(ast);
            
            // Assert - Verify analysis results
            Assert.Single(instructions.Allocations);
            Assert.Equal("img", instructions.Allocations[0].VariableName);
            Assert.Equal(3, instructions.RefCountOps.Count); // Init, Dec, CheckAndDelete
            Assert.Single(instructions.Deallocations);
            
            // Assert - Verify code insertion
            var method = ast.Classes[0].Methods[0];
            var block = method.Body as BlockStatement;
            Assert.NotNull(block);
            
            // Should have: variable declaration + ref count dec + conditional delete
            Assert.True(block.Statements.Count >= 2, "Should have cleanup statements");
            
            // Verify ref count decrement
            var refCountDec = block.Statements
                .OfType<ExpressionStatement>()
                .FirstOrDefault(s => 
                    s.Expression is MethodCallExpression mce && 
                    mce.MethodName == "__ref_count_dec");
            Assert.NotNull(refCountDec);
            
            // Verify conditional delete
            var conditionalDelete = block.Statements
                .OfType<IfStatement>()
                .FirstOrDefault(s => 
                    s.Condition is MethodCallExpression mce && 
                    mce.MethodName == "__ref_count_is_zero");
            Assert.NotNull(conditionalDelete);
            
            // Verify delete call inside conditional
            var thenBlock = conditionalDelete.ThenBranch as BlockStatement;
            Assert.NotNull(thenBlock);
            var deleteCall = thenBlock.Statements
                .OfType<ExpressionStatement>()
                .FirstOrDefault(s => 
                    s.Expression is MethodCallExpression mce && 
                    mce.MethodName == "__safe_delete");
            Assert.NotNull(deleteCall);
        }

        [Fact]
        public void CompleteFlow_ArrayAllocation_InsertsArrayDeleteCode()
        {
            // Arrange
            var ast = CreateArrayAllocation();
            
            // Act
            var analyzer = new MemoryManagementAnalyzer();
            var instructions = analyzer.Analyze(ast);
            var inserter = new MemoryManagementCodeInserter(instructions);
            inserter.InsertMemoryManagementCode(ast);
            
            // Assert
            var method = ast.Classes[0].Methods[0];
            var block = method.Body as BlockStatement;
            Assert.NotNull(block);
            
            // Verify array delete is used
            var conditionalDelete = block.Statements
                .OfType<IfStatement>()
                .FirstOrDefault();
            Assert.NotNull(conditionalDelete);
            
            var thenBlock = conditionalDelete.ThenBranch as BlockStatement;
            Assert.NotNull(thenBlock);
            var deleteCall = thenBlock.Statements
                .OfType<ExpressionStatement>()
                .FirstOrDefault(s => 
                    s.Expression is MethodCallExpression mce && 
                    mce.MethodName == "__safe_delete_array");
            Assert.NotNull(deleteCall);
        }

        [Fact]
        public void CompleteFlow_MultipleAllocations_InsertsCleanupForEach()
        {
            // Arrange
            var ast = CreateMultipleAllocations();
            
            // Act
            var analyzer = new MemoryManagementAnalyzer();
            var instructions = analyzer.Analyze(ast);
            var inserter = new MemoryManagementCodeInserter(instructions);
            inserter.InsertMemoryManagementCode(ast);
            
            // Assert
            Assert.Equal(2, instructions.Allocations.Count);
            
            var method = ast.Classes[0].Methods[0];
            var block = method.Body as BlockStatement;
            Assert.NotNull(block);
            
            // Should have cleanup for both allocations
            var refCountDecCalls = block.Statements
                .OfType<ExpressionStatement>()
                .Where(s => 
                    s.Expression is MethodCallExpression mce && 
                    mce.MethodName == "__ref_count_dec")
                .ToList();
            Assert.Equal(2, refCountDecCalls.Count);
            
            var conditionalDeletes = block.Statements
                .OfType<IfStatement>()
                .Where(s => 
                    s.Condition is MethodCallExpression mce && 
                    mce.MethodName == "__ref_count_is_zero")
                .ToList();
            Assert.Equal(2, conditionalDeletes.Count);
        }

        [Fact]
        public void CompleteFlow_NestedScopes_InsertsCleanupAtCorrectScopes()
        {
            // Arrange
            var ast = CreateNestedScopes();
            
            // Act
            var analyzer = new MemoryManagementAnalyzer();
            var instructions = analyzer.Analyze(ast);
            var inserter = new MemoryManagementCodeInserter(instructions);
            inserter.InsertMemoryManagementCode(ast);
            
            // Assert
            Assert.Equal(2, instructions.Allocations.Count);
            
            var method = ast.Classes[0].Methods[0];
            var outerBlock = method.Body as BlockStatement;
            Assert.NotNull(outerBlock);
            
            // Inner block should have cleanup for img2
            var innerBlock = outerBlock.Statements
                .OfType<BlockStatement>()
                .FirstOrDefault();
            Assert.NotNull(innerBlock);
            
            // Inner block should have cleanup statements
            var innerCleanup = innerBlock.Statements
                .OfType<ExpressionStatement>()
                .Any(s => 
                    s.Expression is MethodCallExpression mce && 
                    mce.MethodName == "__ref_count_dec");
            Assert.True(innerCleanup, "Inner block should have cleanup for img2");
            
            // Outer block should have cleanup for img1
            var outerCleanup = outerBlock.Statements
                .OfType<ExpressionStatement>()
                .Any(s => 
                    s.Expression is MethodCallExpression mce && 
                    mce.MethodName == "__ref_count_dec");
            Assert.True(outerCleanup, "Outer block should have cleanup for img1");
        }

        [Fact]
        public void CompleteFlow_ManualStrategy_NoCodeInsertion()
        {
            // Arrange
            var ast = CreateSimplePointerAllocation();
            
            // Act
            var analyzer = new MemoryManagementAnalyzer(MemoryManagementStrategy.Manual);
            var instructions = analyzer.Analyze(ast);
            var inserter = new MemoryManagementCodeInserter(instructions);
            inserter.InsertMemoryManagementCode(ast);
            
            // Assert
            Assert.Empty(instructions.Allocations);
            Assert.Empty(instructions.RefCountOps);
            Assert.Empty(instructions.Deallocations);
            
            var method = ast.Classes[0].Methods[0];
            var block = method.Body as BlockStatement;
            Assert.NotNull(block);
            
            // Should only have the original variable declaration
            Assert.Single(block.Statements);
        }

        [Fact]
        public void StaticMethods_CreateCorrectStatements()
        {
            // Arrange
            var location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 };
            
            // Act & Assert - Ref count init
            var initStmt = MemoryManagementCodeInserter.CreateRefCountInitStatement("ptr", location);
            Assert.IsType<ExpressionStatement>(initStmt);
            var initCall = ((ExpressionStatement)initStmt).Expression as MethodCallExpression;
            Assert.NotNull(initCall);
            Assert.Equal("__ref_count_init", initCall.MethodName);
            
            // Act & Assert - Ref count increment
            var incStmt = MemoryManagementCodeInserter.CreateRefCountIncrementStatement("ptr", location);
            Assert.IsType<ExpressionStatement>(incStmt);
            var incCall = ((ExpressionStatement)incStmt).Expression as MethodCallExpression;
            Assert.NotNull(incCall);
            Assert.Equal("__ref_count_inc", incCall.MethodName);
            
            // Act & Assert - Null safety check
            var nullCheck = MemoryManagementCodeInserter.CreateNullSafetyCheck("ptr", location);
            Assert.IsType<IfStatement>(nullCheck);
            var ifStmt = (IfStatement)nullCheck;
            Assert.IsType<BinaryExpression>(ifStmt.Condition);
            var thenBlock = ifStmt.ThenBranch as BlockStatement;
            Assert.NotNull(thenBlock);
            Assert.Single(thenBlock.Statements);
            Assert.IsType<ThrowStatement>(thenBlock.Statements[0]);
        }

        // Helper methods to create test ASTs

        private CompilationUnit CreateSimplePointerAllocation()
        {
            var location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 };
            
            var method = new MethodDeclaration
            {
                Name = "process",
                ReturnType = new TypeReference { Name = "void" },
                Location = location,
                Body = new BlockStatement
                {
                    Location = location,
                    Statements = new()
                    {
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                            Name = "img",
                            Location = location,
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 1920, LiteralType = LiteralType.Integer, Location = location },
                                    new LiteralExpression { Value = 1080, LiteralType = LiteralType.Integer, Location = location }
                                },
                                IsArray = false,
                                Location = location
                            }
                        }
                    }
                }
            };

            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Location = location,
                Methods = new() { method }
            };

            return new CompilationUnit
            {
                Location = location,
                Classes = new() { classDecl }
            };
        }

        private CompilationUnit CreateArrayAllocation()
        {
            var location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 };
            
            var method = new MethodDeclaration
            {
                Name = "process",
                ReturnType = new TypeReference { Name = "void" },
                Location = location,
                Body = new BlockStatement
                {
                    Location = location,
                    Statements = new()
                    {
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "int", IsPointer = true, PointerLevel = 1 },
                            Name = "arr",
                            Location = location,
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "int", IsArray = true },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 100, LiteralType = LiteralType.Integer, Location = location }
                                },
                                IsArray = true,
                                Location = location
                            }
                        }
                    }
                }
            };

            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Location = location,
                Methods = new() { method }
            };

            return new CompilationUnit
            {
                Location = location,
                Classes = new() { classDecl }
            };
        }

        private CompilationUnit CreateMultipleAllocations()
        {
            var location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 };
            
            var method = new MethodDeclaration
            {
                Name = "process",
                ReturnType = new TypeReference { Name = "void" },
                Location = location,
                Body = new BlockStatement
                {
                    Location = location,
                    Statements = new()
                    {
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                            Name = "img1",
                            Location = location,
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 1920, LiteralType = LiteralType.Integer, Location = location },
                                    new LiteralExpression { Value = 1080, LiteralType = LiteralType.Integer, Location = location }
                                },
                                IsArray = false,
                                Location = location
                            }
                        },
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                            Name = "img2",
                            Location = location,
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 800, LiteralType = LiteralType.Integer, Location = location },
                                    new LiteralExpression { Value = 600, LiteralType = LiteralType.Integer, Location = location }
                                },
                                IsArray = false,
                                Location = location
                            }
                        }
                    }
                }
            };

            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Location = location,
                Methods = new() { method }
            };

            return new CompilationUnit
            {
                Location = location,
                Classes = new() { classDecl }
            };
        }

        private CompilationUnit CreateNestedScopes()
        {
            var location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 };
            
            var method = new MethodDeclaration
            {
                Name = "process",
                ReturnType = new TypeReference { Name = "void" },
                Location = location,
                Body = new BlockStatement
                {
                    Location = location,
                    Statements = new()
                    {
                        new VariableDeclarationStatement
                        {
                            VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                            Name = "img1",
                            Location = location,
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Arguments = new()
                                {
                                    new LiteralExpression { Value = 1920, LiteralType = LiteralType.Integer, Location = location },
                                    new LiteralExpression { Value = 1080, LiteralType = LiteralType.Integer, Location = location }
                                },
                                IsArray = false,
                                Location = location
                            }
                        },
                        new BlockStatement
                        {
                            Location = location,
                            Statements = new()
                            {
                                new VariableDeclarationStatement
                                {
                                    VariableType = new TypeReference { Name = "Image", IsPointer = true, PointerLevel = 1 },
                                    Name = "img2",
                                    Location = location,
                                    Initializer = new NewExpression
                                    {
                                        TypeToCreate = new TypeReference { Name = "Image" },
                                        Arguments = new()
                                        {
                                            new LiteralExpression { Value = 800, LiteralType = LiteralType.Integer, Location = location },
                                            new LiteralExpression { Value = 600, LiteralType = LiteralType.Integer, Location = location }
                                        },
                                        IsArray = false,
                                        Location = location
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
                Location = location,
                Methods = new() { method }
            };

            return new CompilationUnit
            {
                Location = location,
                Classes = new() { classDecl }
            };
        }
    }
}
