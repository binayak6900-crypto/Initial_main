using Xunit;
using ADLCompiler.CodeGeneration;
using ADLCompiler.SemanticAnalysis;
using SharedUtilities.Models;
using System.Linq;
using SemanticAllocation = ADLCompiler.SemanticAnalysis.Allocation;
using SemanticDeallocation = ADLCompiler.SemanticAnalysis.Deallocation;

namespace ADLCompiler.Tests
{
    /// <summary>
    /// Tests for memory management integration into code generators
    /// </summary>
    public class MemoryManagementIntegrationCodeGenTests
    {
        [Fact]
        public void BytecodeGenerator_WithMemoryManagement_EmitsRefCountInit()
        {
            // Arrange
            var bytecodeSection = new BytecodeSection();
            var resources = new ResourceSection();
            
            // Create memory management instructions
            var memMgmt = new MemoryManagementInstructions("test_scope");
            memMgmt.Allocations.Add(new SemanticAllocation("img", "Image", new SourceLocation { File = "test.adl", Line = 1, Column = 1 })
            {
                IsPointer = true,
                Kind = AllocationKind.New,
                AllocationLocation = new SourceLocation { File = "test.adl", Line = 1, Column = 15 }
            });

            var generator = new BytecodeGenerator(bytecodeSection, resources, memMgmt);

            // Create a class with a method that allocates memory
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "process",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new VariableDeclarationStatement
                                {
                                    Name = "img",
                                    VariableType = new TypeReference { Name = "Image", IsPointer = true },
                                    Initializer = new NewExpression
                                    {
                                        TypeToCreate = new TypeReference { Name = "Image" },
                                        Location = new SourceLocation { File = "test.adl", Line = 1, Column = 15 }
                                    },
                                    Location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 }
                                }
                            },
                            Location = new SourceLocation { File = "test.adl", Line = 1, Column = 0 }
                        },
                        Location = new SourceLocation { File = "test.adl", Line = 1, Column = 0 }
                    }
                }
            };

            // Act
            generator.GenerateClass(classDecl);

            // Assert
            var bytecodeClass = bytecodeSection.Classes[0];
            var method = bytecodeClass.Methods[0];
            
            // Should have instructions for:
            // 1. NEW_INSTANCE (object creation)
            // 2. INVOKE_STATIC (__ref_count_init)
            // 3. RETURN_VOID
            Assert.True(method.Instructions.Count >= 3, 
                $"Expected at least 3 instructions, got {method.Instructions.Count}");
            
            // Check that __ref_count_init is called
            var refCountInitCalls = method.Instructions
                .Where(i => i.Opcode == Opcode.INVOKE_STATIC)
                .ToList();
            Assert.NotEmpty(refCountInitCalls);
        }

        [Fact]
        public void BytecodeGenerator_WithMemoryManagement_EmitsScopeCleanup()
        {
            // Arrange
            var bytecodeSection = new BytecodeSection();
            var resources = new ResourceSection();
            
            var location = new SourceLocation { File = "test.adl", Line = 5, Column = 1 };
            
            // Create memory management instructions with deallocation
            var memMgmt = new MemoryManagementInstructions("test_scope");
            memMgmt.Allocations.Add(new SemanticAllocation("img", "Image", new SourceLocation { File = "test.adl", Line = 1, Column = 1 })
            {
                IsPointer = true,
                Kind = AllocationKind.New,
                AllocationLocation = new SourceLocation { File = "test.adl", Line = 1, Column = 15 },
                ScopeEnd = location
            });
            memMgmt.Deallocations.Add(new SemanticDeallocation("img", location, DeallocationKind.Delete));

            var generator = new BytecodeGenerator(bytecodeSection, resources, memMgmt);

            // Create a class with a method that has a block scope
            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "process",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new VariableDeclarationStatement
                                {
                                    Name = "img",
                                    VariableType = new TypeReference { Name = "Image", IsPointer = true },
                                    Initializer = new NewExpression
                                    {
                                        TypeToCreate = new TypeReference { Name = "Image" },
                                        Location = new SourceLocation { File = "test.adl", Line = 1, Column = 15 }
                                    },
                                    Location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 }
                                }
                            },
                            Location = location
                        },
                        Location = new SourceLocation { File = "test.adl", Line = 1, Column = 0 }
                    }
                }
            };

            // Act
            generator.GenerateClass(classDecl);

            // Assert
            var bytecodeClass = bytecodeSection.Classes[0];
            var method = bytecodeClass.Methods[0];
            
            // Should have cleanup instructions:
            // - __ref_count_dec
            // - __ref_count_is_zero
            // - __safe_delete (conditional)
            var staticCalls = method.Instructions
                .Where(i => i.Opcode == Opcode.INVOKE_STATIC)
                .ToList();
            
            Assert.True(staticCalls.Count >= 3, 
                $"Expected at least 3 static calls (init, dec, check), got {staticCalls.Count}");
        }

        [Fact]
        public void NativeCodeGenerator_WithMemoryManagement_EmitsRefCountInit()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            
            // Create memory management instructions
            var memMgmt = new MemoryManagementInstructions("test_scope");
            memMgmt.Allocations.Add(new SemanticAllocation("img", "Image", new SourceLocation { File = "test.adl", Line = 1, Column = 1 })
            {
                IsPointer = true,
                Kind = AllocationKind.New,
                AllocationLocation = new SourceLocation { File = "test.adl", Line = 1, Column = 15 }
            });

            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64, memMgmt);

            // Create a method that allocates memory
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
                            Name = "img",
                            VariableType = new TypeReference { Name = "Image", IsPointer = true },
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Location = new SourceLocation { File = "test.adl", Line = 1, Column = 15 }
                            },
                            Location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 }
                        }
                    },
                    Location = new SourceLocation { File = "test.adl", Line = 1, Column = 0 }
                },
                Location = new SourceLocation { File = "test.adl", Line = 1, Column = 0 }
            };

            // Act
            var compilationUnit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Name = "test",
                    Functions = new() { method }
                }
            };
            generator.GenerateCompilationUnit(compilationUnit);

            // Assert
            var function = nativeSection.Functions[0];
            
            // Should have relocations for:
            // - __allocate_memory (from new expression)
            // - __ref_count_init
            var refCountInitReloc = function.Relocations
                .FirstOrDefault(r => r.TargetSymbol == "__ref_count_init");
            
            Assert.NotNull(refCountInitReloc);
        }

        [Fact]
        public void NativeCodeGenerator_WithMemoryManagement_EmitsScopeCleanup()
        {
            // Arrange
            var nativeSection = new NativeCodeSection();
            var location = new SourceLocation { File = "test.adl", Line = 5, Column = 1 };
            
            // Create memory management instructions with deallocation
            var memMgmt = new MemoryManagementInstructions("test_scope");
            memMgmt.Allocations.Add(new SemanticAllocation("img", "Image", new SourceLocation { File = "test.adl", Line = 1, Column = 1 })
            {
                IsPointer = true,
                Kind = AllocationKind.New,
                AllocationLocation = new SourceLocation { File = "test.adl", Line = 1, Column = 15 },
                ScopeEnd = location
            });
            memMgmt.Deallocations.Add(new SemanticDeallocation("img", location, DeallocationKind.Delete));

            var generator = new NativeCodeGenerator(nativeSection, Architecture.x86_64, memMgmt);

            // Create a method with a block scope
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
                            Name = "img",
                            VariableType = new TypeReference { Name = "Image", IsPointer = true },
                            Initializer = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "Image" },
                                Location = new SourceLocation { File = "test.adl", Line = 1, Column = 15 }
                            },
                            Location = new SourceLocation { File = "test.adl", Line = 1, Column = 1 }
                        }
                    },
                    Location = location
                },
                Location = new SourceLocation { File = "test.adl", Line = 1, Column = 0 }
            };

            // Act
            var compilationUnit = new CompilationUnit
            {
                Namespace = new NamespaceDeclaration
                {
                    Name = "test",
                    Functions = new() { method }
                }
            };
            generator.GenerateCompilationUnit(compilationUnit);

            // Assert
            var function = nativeSection.Functions[0];
            
            // Should have relocations for cleanup functions:
            // - __ref_count_dec
            // - __ref_count_is_zero
            // - __safe_delete
            var cleanupRelocs = function.Relocations
                .Where(r => r.TargetSymbol.Contains("ref_count") || r.TargetSymbol.Contains("delete"))
                .ToList();
            
            Assert.True(cleanupRelocs.Count >= 3, 
                $"Expected at least 3 cleanup relocations, got {cleanupRelocs.Count}");
        }

        [Fact]
        public void BytecodeGenerator_WithoutMemoryManagement_NoExtraInstructions()
        {
            // Arrange
            var bytecodeSection = new BytecodeSection();
            var resources = new ResourceSection();
            
            // No memory management instructions
            var generator = new BytecodeGenerator(bytecodeSection, resources, null);

            var classDecl = new ClassDeclaration
            {
                Name = "TestClass",
                Methods = new()
                {
                    new MethodDeclaration
                    {
                        Name = "process",
                        ReturnType = new TypeReference { Name = "void" },
                        Body = new BlockStatement
                        {
                            Statements = new()
                            {
                                new VariableDeclarationStatement
                                {
                                    Name = "img",
                                    VariableType = new TypeReference { Name = "Image", IsPointer = true },
                                    Initializer = new NewExpression
                                    {
                                        TypeToCreate = new TypeReference { Name = "Image" }
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
            var bytecodeClass = bytecodeSection.Classes[0];
            var method = bytecodeClass.Methods[0];
            
            // Should only have basic instructions, no ref counting
            var refCountCalls = method.Instructions
                .Where(i => i.Opcode == Opcode.INVOKE_STATIC)
                .ToList();
            
            // Should be 0 or minimal (no ref count calls)
            Assert.True(refCountCalls.Count == 0, 
                $"Expected no ref count calls without memory management, got {refCountCalls.Count}");
        }
    }
}
