using System;
using System.Collections.Generic;

namespace ADLCompiler.CodeGeneration
{
    /// <summary>
    /// Memory management section containing automatic memory management instructions
    /// </summary>
    public class MemoryManagementSection
    {
        public List<MemoryManagementScope> Scopes { get; set; }

        public MemoryManagementSection()
        {
            Scopes = new List<MemoryManagementScope>();
        }
    }

    /// <summary>
    /// Represents a scope with memory management operations
    /// </summary>
    public class MemoryManagementScope
    {
        public string ScopeID { get; set; }
        public List<Allocation> Allocations { get; set; }
        public List<RefCountOperation> RefCountOps { get; set; }
        public List<Deallocation> Deallocations { get; set; }

        public MemoryManagementScope(string scopeID)
        {
            ScopeID = scopeID;
            Allocations = new List<Allocation>();
            RefCountOps = new List<RefCountOperation>();
            Deallocations = new List<Deallocation>();
        }
    }

    /// <summary>
    /// Represents a memory allocation
    /// </summary>
    public struct Allocation
    {
        public uint PointerID;
        public string TypeName;
        public uint AllocationLocation;
        public uint ScopeEnd;

        public Allocation(uint pointerID, string typeName, uint allocationLocation, uint scopeEnd)
        {
            PointerID = pointerID;
            TypeName = typeName;
            AllocationLocation = allocationLocation;
            ScopeEnd = scopeEnd;
        }
    }

    /// <summary>
    /// Represents a reference counting operation
    /// </summary>
    public struct RefCountOperation
    {
        public RefCountOpType Type;
        public uint PointerID;
        public uint Location;

        public RefCountOperation(RefCountOpType type, uint pointerID, uint location)
        {
            Type = type;
            PointerID = pointerID;
            Location = location;
        }
    }

    /// <summary>
    /// Types of reference counting operations
    /// </summary>
    public enum RefCountOpType
    {
        INCREMENT,
        DECREMENT
    }

    /// <summary>
    /// Represents a deallocation operation
    /// </summary>
    public struct Deallocation
    {
        public uint PointerID;
        public uint Location;
        public bool Conditional;

        public Deallocation(uint pointerID, uint location, bool conditional = true)
        {
            PointerID = pointerID;
            Location = location;
            Conditional = conditional;
        }
    }

    /// <summary>
    /// Helper class for generating memory management instructions
    /// </summary>
    public static class MemoryManagementHelper
    {
        private static uint _nextPointerID = 1;

        /// <summary>
        /// Allocate a new pointer ID
        /// </summary>
        public static uint AllocatePointerID()
        {
            return _nextPointerID++;
        }

        /// <summary>
        /// Reset pointer ID counter (for testing)
        /// </summary>
        public static void ResetPointerIDCounter()
        {
            _nextPointerID = 1;
        }

        /// <summary>
        /// Create a scope for a method
        /// </summary>
        public static MemoryManagementScope CreateMethodScope(string className, string methodName)
        {
            return new MemoryManagementScope($"{className}::{methodName}");
        }

        /// <summary>
        /// Create a scope for a block
        /// </summary>
        public static MemoryManagementScope CreateBlockScope(string parentScope, int blockIndex)
        {
            return new MemoryManagementScope($"{parentScope}::block{blockIndex}");
        }

        /// <summary>
        /// Add allocation with automatic cleanup
        /// </summary>
        public static void AddAllocation(
            MemoryManagementScope scope,
            string typeName,
            uint allocationLocation,
            uint scopeEnd)
        {
            uint pointerID = AllocatePointerID();

            // Add allocation
            scope.Allocations.Add(new Allocation(
                pointerID,
                typeName,
                allocationLocation,
                scopeEnd
            ));

            // Add initial reference count increment
            scope.RefCountOps.Add(new RefCountOperation(
                RefCountOpType.INCREMENT,
                pointerID,
                allocationLocation
            ));

            // Add cleanup at scope end
            scope.RefCountOps.Add(new RefCountOperation(
                RefCountOpType.DECREMENT,
                pointerID,
                scopeEnd
            ));

            scope.Deallocations.Add(new Deallocation(
                pointerID,
                scopeEnd,
                conditional: true  // Only deallocate if ref count is zero
            ));
        }

        /// <summary>
        /// Add reference count increment for assignment
        /// </summary>
        public static void AddAssignment(
            MemoryManagementScope scope,
            uint pointerID,
            uint location)
        {
            scope.RefCountOps.Add(new RefCountOperation(
                RefCountOpType.INCREMENT,
                pointerID,
                location
            ));
        }

        /// <summary>
        /// Add reference count decrement for reassignment
        /// </summary>
        public static void AddReassignment(
            MemoryManagementScope scope,
            uint oldPointerID,
            uint newPointerID,
            uint location)
        {
            // Decrement old pointer
            scope.RefCountOps.Add(new RefCountOperation(
                RefCountOpType.DECREMENT,
                oldPointerID,
                location
            ));

            // Increment new pointer
            scope.RefCountOps.Add(new RefCountOperation(
                RefCountOpType.INCREMENT,
                newPointerID,
                location
            ));

            // Conditionally deallocate old pointer if ref count is zero
            scope.Deallocations.Add(new Deallocation(
                oldPointerID,
                location,
                conditional: true
            ));
        }
    }
}
