using System;
using System.Collections.Generic;
using System.Linq;
using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis
{
    /// <summary>
    /// Inserts memory management code into the AST based on analysis results
    /// </summary>
    public class MemoryManagementCodeInserter
    {
        private readonly MemoryManagementInstructions _instructions;
        private readonly Dictionary<string, List<RefCountOperation>> _refCountOpsByLocation;
        private readonly Dictionary<string, List<Deallocation>> _deallocationsByLocation;

        public MemoryManagementCodeInserter(MemoryManagementInstructions instructions)
        {
            _instructions = instructions;
            _refCountOpsByLocation = new Dictionary<string, List<RefCountOperation>>();
            _deallocationsByLocation = new Dictionary<string, List<Deallocation>>();
            
            IndexOperationsByLocation();
        }

        /// <summary>
        /// Index operations by their location for efficient lookup
        /// </summary>
        private void IndexOperationsByLocation()
        {
            foreach (var op in _instructions.RefCountOps)
            {
                var key = LocationKey(op.Location);
                if (!_refCountOpsByLocation.ContainsKey(key))
                {
                    _refCountOpsByLocation[key] = new List<RefCountOperation>();
                }
                _refCountOpsByLocation[key].Add(op);
            }

            foreach (var dealloc in _instructions.Deallocations)
            {
                var key = LocationKey(dealloc.Location);
                if (!_deallocationsByLocation.ContainsKey(key))
                {
                    _deallocationsByLocation[key] = new List<Deallocation>();
                }
                _deallocationsByLocation[key].Add(dealloc);
            }
        }

        /// <summary>
        /// Insert memory management code into the AST
        /// </summary>
        public void InsertMemoryManagementCode(ASTNode root)
        {
            ProcessNode(root);
        }

        /// <summary>
        /// Process a node and insert memory management code where needed
        /// </summary>
        private void ProcessNode(ASTNode node)
        {
            if (node == null) return;

            switch (node.Type)
            {
                case NodeType.MethodDeclaration:
                    ProcessMethodDeclaration((MethodDeclaration)node);
                    break;

                case NodeType.BlockStatement:
                    ProcessBlockStatement((BlockStatement)node);
                    break;

                case NodeType.VariableDeclarationStatement:
                    ProcessVariableDeclaration((VariableDeclarationStatement)node);
                    break;

                default:
                    // Recursively process children
                    ProcessChildren(node);
                    break;
            }
        }

        /// <summary>
        /// Process a method declaration
        /// </summary>
        private void ProcessMethodDeclaration(MethodDeclaration node)
        {
            if (node.Body != null)
            {
                ProcessNode(node.Body);
            }
        }

        /// <summary>
        /// Process a block statement and insert cleanup code at the end
        /// </summary>
        private void ProcessBlockStatement(BlockStatement node)
        {
            // Process existing statements first
            foreach (var statement in node.Statements.ToList())
            {
                ProcessNode(statement);
            }

            // Insert cleanup code at the end of the block
            if (node.Location != null)
            {
                var cleanupStatements = GenerateCleanupStatements(node.Location);
                node.Statements.AddRange(cleanupStatements);
            }
        }

        /// <summary>
        /// Process a variable declaration and insert ref count initialization
        /// </summary>
        private void ProcessVariableDeclaration(VariableDeclarationStatement node)
        {
            // Check if this variable has an allocation
            var allocation = _instructions.Allocations
                .FirstOrDefault(a => a.VariableName == node.Name && 
                                   LocationsMatch(a.AllocationLocation, node.Location));

            if (allocation != null)
            {
                // Find the parent block to insert initialization code
                // For now, we'll mark the node as needing initialization
                node.NeedsMemoryManagement = true;
            }
        }

        /// <summary>
        /// Generate cleanup statements for a given location
        /// </summary>
        private List<Statement> GenerateCleanupStatements(SourceLocation location)
        {
            var statements = new List<Statement>();
            var key = LocationKey(location);

            // Get all operations at this location
            var refCountOps = _refCountOpsByLocation.ContainsKey(key) 
                ? _refCountOpsByLocation[key] 
                : new List<RefCountOperation>();

            var deallocations = _deallocationsByLocation.ContainsKey(key)
                ? _deallocationsByLocation[key]
                : new List<Deallocation>();

            // Group operations by variable
            var variableOps = refCountOps
                .Where(op => op.Type == RefCountOpType.Decrement || op.Type == RefCountOpType.CheckAndDelete)
                .GroupBy(op => op.VariableName);

            foreach (var varGroup in variableOps)
            {
                var variableName = varGroup.Key;
                
                // Generate: __ref_count_dec(variableName);
                statements.Add(CreateRefCountDecStatement(variableName, location));

                // Generate: if (__ref_count_is_zero(variableName)) { __safe_delete(variableName); }
                var dealloc = deallocations.FirstOrDefault(d => d.VariableName == variableName);
                if (dealloc != null)
                {
                    statements.Add(CreateConditionalDeleteStatement(variableName, dealloc.Kind, location));
                }
            }

            return statements;
        }

        /// <summary>
        /// Create a reference count decrement statement
        /// </summary>
        private Statement CreateRefCountDecStatement(string variableName, SourceLocation location)
        {
            return new ExpressionStatement
            {
                Expression = new MethodCallExpression
                {
                    MethodName = "__ref_count_dec",
                    Arguments = new List<Expression>
                    {
                        new IdentifierExpression { Name = variableName, Location = location }
                    },
                    Location = location
                },
                Location = location
            };
        }

        /// <summary>
        /// Create a conditional delete statement
        /// </summary>
        private Statement CreateConditionalDeleteStatement(string variableName, DeallocationKind kind, SourceLocation location)
        {
            var deleteMethodName = kind == DeallocationKind.DeleteArray 
                ? "__safe_delete_array" 
                : "__safe_delete";

            return new IfStatement
            {
                Condition = new MethodCallExpression
                {
                    MethodName = "__ref_count_is_zero",
                    Arguments = new List<Expression>
                    {
                        new IdentifierExpression { Name = variableName, Location = location }
                    },
                    Location = location
                },
                ThenBranch = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ExpressionStatement
                        {
                            Expression = new MethodCallExpression
                            {
                                MethodName = deleteMethodName,
                                Arguments = new List<Expression>
                                {
                                    new IdentifierExpression { Name = variableName, Location = location }
                                },
                                Location = location
                            },
                            Location = location
                        }
                    },
                    Location = location
                },
                Location = location
            };
        }

        /// <summary>
        /// Create a reference count initialization statement
        /// </summary>
        public static Statement CreateRefCountInitStatement(string variableName, SourceLocation location)
        {
            return new ExpressionStatement
            {
                Expression = new MethodCallExpression
                {
                    MethodName = "__ref_count_init",
                    Arguments = new List<Expression>
                    {
                        new IdentifierExpression { Name = variableName, Location = location }
                    },
                    Location = location
                },
                Location = location
            };
        }

        /// <summary>
        /// Create a null safety check statement
        /// </summary>
        public static Statement CreateNullSafetyCheck(string variableName, SourceLocation location)
        {
            return new IfStatement
            {
                Condition = new BinaryExpression
                {
                    Left = new IdentifierExpression { Name = variableName, Location = location },
                    Operator = BinaryOperator.Equal,
                    Right = new LiteralExpression 
                    { 
                        Value = null, 
                        LiteralType = LiteralType.Null,
                        Location = location 
                    },
                    Location = location
                },
                ThenBranch = new BlockStatement
                {
                    Statements = new List<Statement>
                    {
                        new ThrowStatement
                        {
                            Exception = new NewExpression
                            {
                                TypeToCreate = new TypeReference { Name = "NullPointerException" },
                                Arguments = new List<Expression>
                                {
                                    new LiteralExpression 
                                    { 
                                        Value = $"Null pointer dereference: {variableName}",
                                        LiteralType = LiteralType.String,
                                        Location = location
                                    }
                                },
                                Location = location
                            },
                            Location = location
                        }
                    },
                    Location = location
                },
                Location = location
            };
        }

        /// <summary>
        /// Insert reference count increment on assignment
        /// </summary>
        public static Statement CreateRefCountIncrementStatement(string variableName, SourceLocation location)
        {
            return new ExpressionStatement
            {
                Expression = new MethodCallExpression
                {
                    MethodName = "__ref_count_inc",
                    Arguments = new List<Expression>
                    {
                        new IdentifierExpression { Name = variableName, Location = location }
                    },
                    Location = location
                },
                Location = location
            };
        }

        /// <summary>
        /// Process children nodes recursively
        /// </summary>
        private void ProcessChildren(ASTNode node)
        {
            if (node is CompilationUnit cu)
            {
                if (cu.Namespace != null)
                    ProcessNode(cu.Namespace);
                foreach (var cls in cu.Classes)
                    ProcessNode(cls);
            }
            else if (node is ClassDeclaration classDecl)
            {
                foreach (var method in classDecl.Methods)
                    ProcessNode(method);
            }
            else if (node is NamespaceDeclaration ns)
            {
                foreach (var cls in ns.Classes)
                    ProcessNode(cls);
                foreach (var func in ns.Functions)
                    ProcessNode(func);
            }
        }

        /// <summary>
        /// Create a location key for indexing
        /// </summary>
        private string LocationKey(SourceLocation location)
        {
            if (location == null) return "unknown";
            return $"{location.File}:{location.Line}:{location.Column}";
        }

        /// <summary>
        /// Check if two locations match
        /// </summary>
        private bool LocationsMatch(SourceLocation loc1, SourceLocation loc2)
        {
            if (loc1 == null || loc2 == null) return false;
            return loc1.File == loc2.File && 
                   loc1.Line == loc2.Line && 
                   loc1.Column == loc2.Column;
        }
    }
}
