using System;
using System.Collections.Generic;
using System.Linq;
using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis
{
    /// <summary>
    /// Analyzes code to insert automatic memory management
    /// </summary>
    public class MemoryManagementAnalyzer
    {
        private readonly MemoryManagementStrategy _strategy;
        private readonly Stack<MemoryScope> _scopeStack;
        private int _scopeCounter;

        public MemoryManagementAnalyzer(MemoryManagementStrategy strategy = MemoryManagementStrategy.Auto)
        {
            _strategy = strategy;
            _scopeStack = new Stack<MemoryScope>();
            _scopeCounter = 0;
        }

        /// <summary>
        /// Analyze an AST and generate memory management instructions
        /// </summary>
        public MemoryManagementInstructions Analyze(ASTNode node)
        {
            if (_strategy == MemoryManagementStrategy.Manual)
            {
                return new MemoryManagementInstructions("manual");
            }

            // Create root scope
            var rootScope = new MemoryScope(
                "global",
                null,
                ScopeKind.Global,
                node.Location
            );
            _scopeStack.Push(rootScope);

            // Analyze the AST
            AnalyzeNode(node);

            // Generate instructions from all scopes
            return GenerateInstructions(rootScope);
        }

        /// <summary>
        /// Analyze a single AST node
        /// </summary>
        private void AnalyzeNode(ASTNode node)
        {
            if (node == null) return;

            switch (node.Type)
            {
                case NodeType.ClassDeclaration:
                    AnalyzeClassDeclaration((ClassDeclaration)node);
                    break;

                case NodeType.FieldDeclaration:
                    AnalyzeFieldDeclaration((FieldDeclaration)node);
                    break;

                case NodeType.MethodDeclaration:
                    AnalyzeMethodDeclaration((MethodDeclaration)node);
                    break;

                case NodeType.BlockStatement:
                    AnalyzeBlockStatement((BlockStatement)node);
                    break;

                case NodeType.VariableDeclarationStatement:
                    AnalyzeVariableDeclaration((VariableDeclarationStatement)node);
                    break;

                case NodeType.AssignmentExpression:
                    AnalyzeAssignment((AssignmentExpression)node);
                    break;

                case NodeType.NewExpression:
                    AnalyzeNewExpression((NewExpression)node);
                    break;

                default:
                    // Recursively analyze children based on node type
                    if (node is CompilationUnit cu)
                    {
                        if (cu.Namespace != null)
                            AnalyzeNode(cu.Namespace);
                        foreach (var cls in cu.Classes)
                            AnalyzeNode(cls);
                    }
                    else if (node is NamespaceDeclaration ns)
                    {
                        foreach (var cls in ns.Classes)
                            AnalyzeNode(cls);
                        foreach (var func in ns.Functions)
                            AnalyzeNode(func);
                    }
                    else if (node is Statement stmt)
                    {
                        // Handle other statement types
                        if (stmt is ExpressionStatement exprStmt)
                            AnalyzeNode(exprStmt.Expression);
                    }
                    else if (node is Expression expr)
                    {
                        // Handle expression types
                        if (expr is BinaryExpression binExpr)
                        {
                            AnalyzeNode(binExpr.Left);
                            AnalyzeNode(binExpr.Right);
                        }
                        else if (expr is UnaryExpression unExpr)
                        {
                            AnalyzeNode(unExpr.Operand);
                        }
                        else if (expr is MethodCallExpression callExpr)
                        {
                            if (callExpr.Target != null)
                                AnalyzeNode(callExpr.Target);
                            foreach (var arg in callExpr.Arguments)
                                AnalyzeNode(arg);
                        }
                        else if (expr is MemberAccessExpression memExpr)
                        {
                            AnalyzeNode(memExpr.Target);
                        }
                        else if (expr is ArrayAccessExpression arrExpr)
                        {
                            AnalyzeNode(arrExpr.Array);
                            AnalyzeNode(arrExpr.Index);
                        }
                        else if (expr is CastExpression castExpr)
                        {
                            AnalyzeNode(castExpr.Expression);
                        }
                        else if (expr is TernaryExpression ternExpr)
                        {
                            AnalyzeNode(ternExpr.Condition);
                            AnalyzeNode(ternExpr.TrueExpression);
                            AnalyzeNode(ternExpr.FalseExpression);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Analyze a class declaration
        /// </summary>
        private void AnalyzeClassDeclaration(ClassDeclaration node)
        {
            var scope = EnterScope(ScopeKind.Class, node.Location);
            scope.Id = $"class_{node.Name}";

            // Analyze fields
            foreach (var field in node.Fields)
            {
                AnalyzeNode(field);
            }

            // Analyze methods
            foreach (var method in node.Methods)
            {
                AnalyzeNode(method);
            }

            ExitScope(node.Location);
        }

        /// <summary>
        /// Analyze a field declaration
        /// </summary>
        private void AnalyzeFieldDeclaration(FieldDeclaration node)
        {
            // Check if this is a pointer type that needs memory management
            if (node.FieldType.IsPointer && node.Initializer != null)
            {
                if (node.Initializer is NewExpression newExpr)
                {
                    var allocation = new Allocation(node.Name, node.FieldType.Name, node.Location)
                    {
                        IsPointer = true,
                        Kind = newExpr.IsArray ? AllocationKind.NewArray : AllocationKind.New,
                        AssignmentLocation = newExpr.Location
                    };

                    CurrentScope?.Allocations.Add(allocation);
                }
            }

            // Analyze initializer
            if (node.Initializer != null)
            {
                AnalyzeNode(node.Initializer);
            }
        }

        /// <summary>
        /// Analyze a method declaration
        /// </summary>
        private void AnalyzeMethodDeclaration(MethodDeclaration node)
        {
            var scope = EnterScope(ScopeKind.Method, node.Location);
            scope.Id = $"method_{node.Name}";

            if (node.Body != null)
            {
                AnalyzeNode(node.Body);
            }

            ExitScope(node.Location);
        }

        /// <summary>
        /// Analyze a block statement
        /// </summary>
        private void AnalyzeBlockStatement(BlockStatement node)
        {
            var scope = EnterScope(ScopeKind.Block, node.Location);

            foreach (var statement in node.Statements)
            {
                AnalyzeNode(statement);
            }

            ExitScope(node.Location);
        }

        /// <summary>
        /// Analyze a variable declaration
        /// </summary>
        private void AnalyzeVariableDeclaration(VariableDeclarationStatement node)
        {
            // Check if this is a pointer type that needs memory management
            if (node.VariableType.IsPointer && node.Initializer != null)
            {
                if (node.Initializer is NewExpression newExpr)
                {
                    var allocation = new Allocation(node.Name, node.VariableType.Name, node.Location)
                    {
                        IsPointer = true,
                        Kind = newExpr.IsArray ? AllocationKind.NewArray : AllocationKind.New,
                        AssignmentLocation = newExpr.Location
                    };

                    CurrentScope?.Allocations.Add(allocation);
                }
            }

            // Analyze initializer
            if (node.Initializer != null)
            {
                AnalyzeNode(node.Initializer);
            }
        }

        /// <summary>
        /// Analyze an assignment expression
        /// </summary>
        private void AnalyzeAssignment(AssignmentExpression node)
        {
            // Check if assigning a new expression to a pointer
            if (node.Value is NewExpression newExpr)
            {
                var leftName = GetIdentifierName(node.Target);
                if (!string.IsNullOrEmpty(leftName))
                {
                    // Find if this variable already exists in scope
                    var existingAlloc = FindAllocation(leftName);
                    if (existingAlloc != null)
                    {
                        // Update existing allocation
                        existingAlloc.AssignmentLocation = newExpr.Location;
                    }
                    else
                    {
                        // Create new allocation
                        var allocation = new Allocation(leftName, "unknown", node.Location)
                        {
                            IsPointer = true,
                            Kind = newExpr.IsArray ? AllocationKind.NewArray : AllocationKind.New,
                            AssignmentLocation = newExpr.Location
                        };
                        CurrentScope?.Allocations.Add(allocation);
                    }
                }
            }

            AnalyzeNode(node.Target);
            AnalyzeNode(node.Value);
        }

        /// <summary>
        /// Analyze a new expression
        /// </summary>
        private void AnalyzeNewExpression(NewExpression node)
        {
            // New expressions are handled in variable declarations and assignments
            foreach (var arg in node.Arguments)
            {
                AnalyzeNode(arg);
            }
        }

        /// <summary>
        /// Generate memory management instructions from analyzed scopes
        /// </summary>
        private MemoryManagementInstructions GenerateInstructions(MemoryScope rootScope)
        {
            var instructions = new MemoryManagementInstructions(rootScope.Id);

            // Collect all allocations from all scopes
            CollectAllocations(rootScope, instructions);

            // Set scope end locations for all allocations
            SetScopeEndLocations(rootScope);

            // Generate reference counting operations
            GenerateRefCountOps(instructions);

            // Generate scope-based cleanup
            GenerateScopeCleanup(rootScope, instructions);

            return instructions;
        }

        /// <summary>
        /// Set scope end locations for allocations
        /// </summary>
        private void SetScopeEndLocations(MemoryScope scope)
        {
            if (scope.EndLocation != null)
            {
                foreach (var alloc in scope.Allocations)
                {
                    alloc.ScopeEnd = scope.EndLocation;
                }
            }

            foreach (var child in scope.Children)
            {
                SetScopeEndLocations(child);
            }
        }

        /// <summary>
        /// Collect allocations from scope tree
        /// </summary>
        private void CollectAllocations(MemoryScope scope, MemoryManagementInstructions instructions)
        {
            instructions.Allocations.AddRange(scope.Allocations);

            foreach (var child in scope.Children)
            {
                CollectAllocations(child, instructions);
            }
        }

        /// <summary>
        /// Generate reference counting operations
        /// </summary>
        private void GenerateRefCountOps(MemoryManagementInstructions instructions)
        {
            foreach (var alloc in instructions.Allocations)
            {
                // Initialize ref count after allocation
                instructions.RefCountOps.Add(new RefCountOperation(
                    RefCountOpType.Initialize,
                    alloc.VariableName,
                    alloc.AllocationLocation
                ));

                // Decrement ref count at scope end
                if (alloc.ScopeEnd != null)
                {
                    instructions.RefCountOps.Add(new RefCountOperation(
                        RefCountOpType.Decrement,
                        alloc.VariableName,
                        alloc.ScopeEnd
                    ));

                    // Check and delete if ref count is zero
                    instructions.RefCountOps.Add(new RefCountOperation(
                        RefCountOpType.CheckAndDelete,
                        alloc.VariableName,
                        alloc.ScopeEnd
                    ));
                }
            }
        }

        /// <summary>
        /// Generate scope-based cleanup operations
        /// </summary>
        private void GenerateScopeCleanup(MemoryScope scope, MemoryManagementInstructions instructions)
        {
            // Add deallocations at scope end
            foreach (var alloc in scope.Allocations)
            {
                if (scope.EndLocation != null)
                {
                    alloc.ScopeEnd = scope.EndLocation;

                    var deallocKind = alloc.Kind == AllocationKind.NewArray
                        ? DeallocationKind.DeleteArray
                        : DeallocationKind.Delete;

                    instructions.Deallocations.Add(new Deallocation(
                        alloc.VariableName,
                        scope.EndLocation,
                        deallocKind
                    ));
                }
            }

            // Process child scopes
            foreach (var child in scope.Children)
            {
                GenerateScopeCleanup(child, instructions);
            }
        }

        /// <summary>
        /// Convert raw pointers to smart pointers
        /// </summary>
        public List<SmartPointerConversion> GenerateSmartPointerConversions(MemoryManagementInstructions instructions)
        {
            var conversions = new List<SmartPointerConversion>();

            foreach (var alloc in instructions.Allocations)
            {
                if (alloc.IsPointer)
                {
                    // Determine smart pointer type based on usage
                    var smartPtrType = DetermineSmartPointerType(alloc);

                    conversions.Add(new SmartPointerConversion(
                        alloc.VariableName,
                        smartPtrType,
                        alloc.AllocationLocation
                    ));
                }
            }

            return conversions;
        }

        /// <summary>
        /// Determine the appropriate smart pointer type for an allocation
        /// </summary>
        private SmartPointerType DetermineSmartPointerType(Allocation alloc)
        {
            // For now, use shared pointers for all allocations
            // In a more sophisticated implementation, we would analyze:
            // - If the pointer is passed to other functions (shared)
            // - If the pointer has a single owner (unique)
            // - If the pointer is stored in multiple places (shared)
            return SmartPointerType.Shared;
        }

        /// <summary>
        /// Enter a new scope
        /// </summary>
        private MemoryScope EnterScope(ScopeKind kind, SourceLocation location)
        {
            var parent = _scopeStack.Count > 0 ? _scopeStack.Peek() : null;
            var scope = new MemoryScope($"scope_{_scopeCounter++}", parent, kind, location);

            if (parent != null)
            {
                parent.Children.Add(scope);
            }

            _scopeStack.Push(scope);
            return scope;
        }

        /// <summary>
        /// Exit the current scope
        /// </summary>
        private void ExitScope(SourceLocation location)
        {
            if (_scopeStack.Count > 0)
            {
                var scope = _scopeStack.Pop();
                scope.EndLocation = location;
            }
        }

        /// <summary>
        /// Get the current scope
        /// </summary>
        private MemoryScope? CurrentScope => _scopeStack.Count > 0 ? _scopeStack.Peek() : null;

        /// <summary>
        /// Check if a type is a pointer type
        /// </summary>
        private bool IsPointerType(string typeName)
        {
            return typeName != null && typeName.EndsWith("*");
        }

        /// <summary>
        /// Get identifier name from an expression
        /// </summary>
        private string? GetIdentifierName(Expression expr)
        {
            if (expr is IdentifierExpression identExpr)
            {
                return identExpr.Name;
            }
            return null;
        }

        /// <summary>
        /// Find an allocation by variable name in current scope chain
        /// </summary>
        private Allocation? FindAllocation(string variableName)
        {
            if (CurrentScope == null) return null;

            return CurrentScope.GetAllAllocations()
                .FirstOrDefault(a => a.VariableName == variableName);
        }
    }
}
