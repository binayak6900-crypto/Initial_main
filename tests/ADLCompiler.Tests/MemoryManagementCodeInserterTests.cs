using Xunit;
using ADLCompiler.SemanticAnalysis;
using SharedUtilities.Models;
using System.Linq;

namespace ADLCompiler.Tests
{
    public class MemoryManagementCodeInserterTests
    {
        [Fact]
        public void InsertMemoryManagementCode_SimplePointerAllocation_InsertsCleanupCode()
        {
            // Arrange
            var ast = CreateSimplePointerAllocation();
            var analyzer = new MemoryManagementAnalyzer();
            var instructions = analyzer.Analyze(ast);
            var inserter = new MemoryManagementCodeInserter(instructions);

            // Act
            inserter.InsertMemoryManagementCode(ast);

            // Assert
            var method = ast.Classes[0].Methods[0];
            var block = method.Body as BlockStatement;
            Assert.NotNull(block);
            
            // Should have original statement + cleanup statements
            Assert.True(block.Statements.Count > 1, "Should have cleanup statements added");
            
            // Check for ref count decrement call
            var hasRefCountDec = block.Statements.Any(s => 
                s is ExpressionStatement es &&
                es.Expression is MethodCallExpression mce &&
                mce.MethodName == "__ref_count_dec");
            Assert.True(hasRefCountDec, "Should have ref count decrement call");
            
            // Check for conditional delete
            var hasConditionalDelete = block.Statements.Any(s => 
                s is IfStatement ifStmt &&
                ifStmt.Condition is MethodCallExpression condMce &&
                condMce.MethodName == "__ref_count_is_zero");
            Assert.True(hasConditionalDelete, "Should have conditional delete statement");
        }

        [Fact]
        public void InsertMemoryManagementCode_ArrayAllocation_InsertsArrayDeleteCode()
        {
            // Arrange
            var ast = CreateArrayAllocation();
            var analyzer = new MemoryManagementAnalyzer();
            var instructions = analyzer.Analyze(ast);
            var inserter = new MemoryManagementCodeInserter(instructions);

            // Act
            inserter.InsertMemoryManagementCode(ast);

            // Assert
            var method = ast.Classes[0].Methods[0];
            var block = method.Body as BlockStatement;
            Assert.NotNull(block);
            
            // Check for array delete call
            var hasArrayDelete = block.Statements.Any(s =>
                s is IfStatement ifStmt &&
                ifStmt.ThenBranch is BlockStatement thenBlock &&
                thenBlock.Statements.Any(stmt =>
                    stmt is ExpressionStatement es &&
                    es.Expression is MethodCallExpression mce &&
                    mce.MethodName == "__safe_delete_array"));
            Assert.True(hasArrayDelete, "Should have array delete call");
        }

        [Fact]
        public void InsertMemoryManagementCode_MultipleAllocations_InsertsMultipleCleanups()
        {
            // Arrange
            var ast = CreateMultipleAllocations();
            var analyzer = new MemoryManagementAnalyzer();
            var instructions = analyzer.Analyze(ast);
            var inserter = new MemoryManagementCodeInserter(instructions);

            // Act
            inserter.InsertMemoryManagementCode(ast);

            // Assert
            var method = ast.Classes[0].Methods[0];
            var block = method.Body as BlockStatement;
            Assert.NotNull(block);
            
            // Should have cleanup for both allocations
            var refCountDecCalls = block.Statements
                .OfType<ExpressionStatement>()
                .Where(s => s.Expression is MethodCallExpression mce && 
                           mce.MethodName == "__ref_count_dec")
                .ToList();
            Assert.Equal(2, refCountDecCalls.Count);
        }

        [Fact]
        public void CreateRefCountInitStatement_CreatesCorrectStatement()
        {
            // Arrange
            var location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 };

            // Act
            var statement = MemoryManagementCodeInserter.CreateRefCountInitStatement("img", location);

            // Assert
            Assert.IsType<ExpressionStatement>(statement);
            var exprStmt = (ExpressionStatement)statement;
            Assert.IsType<MethodCallExpression>(exprStmt.Expression);
            var callExpr = (MethodCallExpression)exprStmt.Expression;
            Assert.Equal("__ref_count_init", callExpr.MethodName);
            Assert.Single(callExpr.Arguments);
        }

        [Fact]
        public void CreateNullSafetyCheck_CreatesCorrectStatement()
        {
            // Arrange
            var location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 };

            // Act
            var statement = MemoryManagementCodeInserter.CreateNullSafetyCheck("img", location);

            // Assert
            Assert.IsType<IfStatement>(statement);
            var ifStmt = (IfStatement)statement;
            Assert.IsType<BinaryExpression>(ifStmt.Condition);
            Assert.IsType<BlockStatement>(ifStmt.ThenBranch);
            
            var thenBlock = (BlockStatement)ifStmt.ThenBranch;
            Assert.Single(thenBlock.Statements);
            Assert.IsType<ThrowStatement>(thenBlock.Statements[0]);
        }

        [Fact]
        public void CreateRefCountIncrementStatement_CreatesCorrectStatement()
        {
            // Arrange
            var location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 };

            // Act
            var statement = MemoryManagementCodeInserter.CreateRefCountIncrementStatement("img", location);

            // Assert
            Assert.IsType<ExpressionStatement>(statement);
            var exprStmt = (ExpressionStatement)statement;
            Assert.IsType<MethodCallExpression>(exprStmt.Expression);
            var callExpr = (MethodCallExpression)exprStmt.Expression;
            Assert.Equal("__ref_count_inc", callExpr.MethodName);
            Assert.Single(callExpr.Arguments);
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
    }
}
