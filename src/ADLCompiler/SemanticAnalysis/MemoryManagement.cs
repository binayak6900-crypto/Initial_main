using System;
using System.Collections.Generic;
using System.Linq;
using SharedUtilities.Models;

namespace ADLCompiler.SemanticAnalysis
{
    /// <summary>
    /// Represents a memory allocation in the code
    /// </summary>
    public class Allocation
    {
        public string VariableName { get; set; }
        public string TypeName { get; set; }
        public SourceLocation AllocationLocation { get; set; }
        public SourceLocation AssignmentLocation { get; set; }
        public SourceLocation? ScopeEnd { get; set; }
        public bool IsPointer { get; set; }
        public bool IsArray { get; set; }
        public AllocationKind Kind { get; set; }

        public Allocation(string variableName, string typeName, SourceLocation allocationLocation)
        {
            VariableName = variableName;
            TypeName = typeName;
            AllocationLocation = allocationLocation;
            AssignmentLocation = allocationLocation;
            IsPointer = false;
            IsArray = false;
            Kind = AllocationKind.New;
        }
    }

    /// <summary>
    /// Kind of memory allocation
    /// </summary>
    public enum AllocationKind
    {
        New,        // new operator
        NewArray,   // new[] operator
        Malloc,     // malloc/calloc
        Other       // other allocation methods
    }

    /// <summary>
    /// Represents a deallocation operation
    /// </summary>
    public class Deallocation
    {
        public string VariableName { get; set; }
        public SourceLocation Location { get; set; }
        public DeallocationKind Kind { get; set; }

        public Deallocation(string variableName, SourceLocation location, DeallocationKind kind)
        {
            VariableName = variableName;
            Location = location;
            Kind = kind;
        }
    }

    /// <summary>
    /// Kind of deallocation operation
    /// </summary>
    public enum DeallocationKind
    {
        Delete,         // delete operator
        DeleteArray,    // delete[] operator
        Free,           // free() function
        AutoScope       // automatic scope-based cleanup
    }

    /// <summary>
    /// Represents a reference counting operation
    /// </summary>
    public class RefCountOperation
    {
        public RefCountOpType Type { get; set; }
        public string VariableName { get; set; }
        public SourceLocation Location { get; set; }

        public RefCountOperation(RefCountOpType type, string variableName, SourceLocation location)
        {
            Type = type;
            VariableName = variableName;
            Location = location;
        }
    }

    /// <summary>
    /// Type of reference counting operation
    /// </summary>
    public enum RefCountOpType
    {
        Initialize,     // Initialize ref count to 1
        Increment,      // Increment ref count
        Decrement,      // Decrement ref count
        CheckAndDelete  // Check if zero and delete
    }

    /// <summary>
    /// Memory management instructions for a scope
    /// </summary>
    public class MemoryManagementInstructions
    {
        public string ScopeId { get; set; }
        public List<Allocation> Allocations { get; set; }
        public List<Deallocation> Deallocations { get; set; }
        public List<RefCountOperation> RefCountOps { get; set; }

        public MemoryManagementInstructions(string scopeId)
        {
            ScopeId = scopeId;
            Allocations = new List<Allocation>();
            Deallocations = new List<Deallocation>();
            RefCountOps = new List<RefCountOperation>();
        }
    }

    /// <summary>
    /// Strategy for memory management
    /// </summary>
    public enum MemoryManagementStrategy
    {
        Auto,       // Automatic memory management
        Manual      // Manual memory management (no insertion)
    }

    /// <summary>
    /// Represents a scope in the code for memory management
    /// </summary>
    public class MemoryScope
    {
        public string Id { get; set; }
        public MemoryScope? Parent { get; set; }
        public List<MemoryScope> Children { get; set; }
        public List<Allocation> Allocations { get; set; }
        public SourceLocation StartLocation { get; set; }
        public SourceLocation? EndLocation { get; set; }
        public ScopeKind Kind { get; set; }

        public MemoryScope(string id, MemoryScope? parent, ScopeKind kind, SourceLocation startLocation)
        {
            Id = id;
            Parent = parent;
            Kind = kind;
            StartLocation = startLocation;
            Children = new List<MemoryScope>();
            Allocations = new List<Allocation>();
        }

        /// <summary>
        /// Get all allocations in this scope and parent scopes
        /// </summary>
        public IEnumerable<Allocation> GetAllAllocations()
        {
            var allocs = new List<Allocation>(Allocations);
            var current = Parent;
            while (current != null)
            {
                allocs.AddRange(current.Allocations);
                current = current.Parent;
            }
            return allocs;
        }
    }

    /// <summary>
    /// Kind of scope
    /// </summary>
    public enum ScopeKind
    {
        Global,
        Class,
        Method,
        Block,
        Loop,
        Conditional
    }

    /// <summary>
    /// Represents a smart pointer conversion
    /// </summary>
    public class SmartPointerConversion
    {
        public string OriginalPointerName { get; set; }
        public string SmartPointerName { get; set; }
        public SmartPointerType Type { get; set; }
        public SourceLocation Location { get; set; }

        public SmartPointerConversion(string originalName, SmartPointerType type, SourceLocation location)
        {
            OriginalPointerName = originalName;
            SmartPointerName = $"__smart_{originalName}";
            Type = type;
            Location = location;
        }
    }

    /// <summary>
    /// Type of smart pointer
    /// </summary>
    public enum SmartPointerType
    {
        Unique,     // Unique ownership (like unique_ptr)
        Shared,     // Shared ownership with ref counting (like shared_ptr)
        Weak        // Weak reference (like weak_ptr)
    }
}
