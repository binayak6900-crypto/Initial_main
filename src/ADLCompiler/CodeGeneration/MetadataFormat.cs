using System;
using System.Collections.Generic;

namespace ADLCompiler.CodeGeneration
{
    /// <summary>
    /// Metadata section containing debug info, type information, and API bindings
    /// </summary>
    public class MetadataSection
    {
        public DebugInfo DebugInfo { get; set; }
        public TypeInformation TypeInfo { get; set; }
        public APIBindings APIBindings { get; set; }

        public MetadataSection()
        {
            DebugInfo = new DebugInfo();
            TypeInfo = new TypeInformation();
            APIBindings = new APIBindings();
        }
    }

    /// <summary>
    /// Debug information for source mapping and debugging
    /// </summary>
    public class DebugInfo
    {
        public List<SourceFileMapping> SourceFiles { get; set; }
        public List<LineNumberTable> LineNumbers { get; set; }
        public List<LocalVariableTable> LocalVariables { get; set; }

        public DebugInfo()
        {
            SourceFiles = new List<SourceFileMapping>();
            LineNumbers = new List<LineNumberTable>();
            LocalVariables = new List<LocalVariableTable>();
        }
    }

    /// <summary>
    /// Maps file IDs to source file paths
    /// </summary>
    public struct SourceFileMapping
    {
        public uint FileID;
        public string FilePath;
        public string SourceHash;

        public SourceFileMapping(uint fileID, string filePath, string sourceHash)
        {
            FileID = fileID;
            FilePath = filePath;
            SourceHash = sourceHash;
        }
    }

    /// <summary>
    /// Line number table for a method
    /// </summary>
    public class LineNumberTable
    {
        public uint MethodID { get; set; }
        public List<LineNumberEntry> Entries { get; set; }

        public LineNumberTable(uint methodID)
        {
            MethodID = methodID;
            Entries = new List<LineNumberEntry>();
        }
    }

    /// <summary>
    /// Maps instruction offset to source line
    /// </summary>
    public struct LineNumberEntry
    {
        public uint InstructionOffset;
        public uint SourceLine;
        public uint SourceColumn;
        public uint FileID;

        public LineNumberEntry(uint instructionOffset, uint sourceLine, uint sourceColumn, uint fileID)
        {
            InstructionOffset = instructionOffset;
            SourceLine = sourceLine;
            SourceColumn = sourceColumn;
            FileID = fileID;
        }
    }

    /// <summary>
    /// Local variable table for a method
    /// </summary>
    public class LocalVariableTable
    {
        public uint MethodID { get; set; }
        public List<LocalVariableEntry> Variables { get; set; }

        public LocalVariableTable(uint methodID)
        {
            MethodID = methodID;
            Variables = new List<LocalVariableEntry>();
        }
    }

    /// <summary>
    /// Information about a local variable
    /// </summary>
    public struct LocalVariableEntry
    {
        public string Name;
        public string TypeDescriptor;
        public uint StartOffset;
        public uint EndOffset;
        public uint RegisterOrSlot;

        public LocalVariableEntry(string name, string typeDescriptor, uint startOffset, uint endOffset, uint registerOrSlot)
        {
            Name = name;
            TypeDescriptor = typeDescriptor;
            StartOffset = startOffset;
            EndOffset = endOffset;
            RegisterOrSlot = registerOrSlot;
        }
    }

    /// <summary>
    /// Type information for runtime type checking
    /// </summary>
    public class TypeInformation
    {
        public List<ClassDescriptor> Classes { get; set; }
        public List<MethodDescriptor> Methods { get; set; }
        public List<FieldDescriptor> Fields { get; set; }

        public TypeInformation()
        {
            Classes = new List<ClassDescriptor>();
            Methods = new List<MethodDescriptor>();
            Fields = new List<FieldDescriptor>();
        }
    }

    /// <summary>
    /// Descriptor for a class
    /// </summary>
    public class ClassDescriptor
    {
        public uint ClassID { get; set; }
        public string ClassName { get; set; }
        public uint SuperClassID { get; set; }
        public List<uint> InterfaceIDs { get; set; }
        public List<uint> FieldIDs { get; set; }
        public List<uint> MethodIDs { get; set; }
        public AccessFlags Flags { get; set; }

        public ClassDescriptor()
        {
            InterfaceIDs = new List<uint>();
            FieldIDs = new List<uint>();
            MethodIDs = new List<uint>();
        }
    }

    /// <summary>
    /// Descriptor for a method
    /// </summary>
    public struct MethodDescriptor
    {
        public uint MethodID;
        public string MethodName;
        public string Signature;
        public uint ClassID;
        public AccessFlags Flags;
        public bool IsNative;
        public uint CodeOffset;

        public MethodDescriptor(uint methodID, string methodName, string signature, uint classID, AccessFlags flags, bool isNative, uint codeOffset)
        {
            MethodID = methodID;
            MethodName = methodName;
            Signature = signature;
            ClassID = classID;
            Flags = flags;
            IsNative = isNative;
            CodeOffset = codeOffset;
        }
    }

    /// <summary>
    /// Descriptor for a field
    /// </summary>
    public struct FieldDescriptor
    {
        public uint FieldID;
        public string FieldName;
        public string TypeDescriptor;
        public uint ClassID;
        public AccessFlags Flags;

        public FieldDescriptor(uint fieldID, string fieldName, string typeDescriptor, uint classID, AccessFlags flags)
        {
            FieldID = fieldID;
            FieldName = fieldName;
            TypeDescriptor = typeDescriptor;
            ClassID = classID;
            Flags = flags;
        }
    }

    /// <summary>
    /// API bindings for built-in APIs, NDK, and raylib
    /// </summary>
    public class APIBindings
    {
        public List<BuiltinAPIReference> BuiltinAPIs { get; set; }
        public List<NDKFunctionPointer> NDKFunctions { get; set; }
        public List<RaylibFunctionPointer> RaylibFunctions { get; set; }

        public APIBindings()
        {
            BuiltinAPIs = new List<BuiltinAPIReference>();
            NDKFunctions = new List<NDKFunctionPointer>();
            RaylibFunctions = new List<RaylibFunctionPointer>();
        }
    }

    /// <summary>
    /// Reference to a built-in Android API
    /// </summary>
    public struct BuiltinAPIReference
    {
        public uint APIID;
        public string ClassName;
        public string MethodName;
        public string Signature;
        public uint MinSDK;
        public bool IsDeprecated;

        public BuiltinAPIReference(uint apiID, string className, string methodName, string signature, uint minSDK, bool isDeprecated = false)
        {
            APIID = apiID;
            ClassName = className;
            MethodName = methodName;
            Signature = signature;
            MinSDK = minSDK;
            IsDeprecated = isDeprecated;
        }
    }

    /// <summary>
    /// Pointer to an NDK function
    /// </summary>
    public struct NDKFunctionPointer
    {
        public uint FunctionID;
        public string FunctionName;
        public string Library;
        public string Symbol;

        public NDKFunctionPointer(uint functionID, string functionName, string library, string symbol)
        {
            FunctionID = functionID;
            FunctionName = functionName;
            Library = library;
            Symbol = symbol;
        }
    }

    /// <summary>
    /// Pointer to a raylib function
    /// </summary>
    public struct RaylibFunctionPointer
    {
        public uint FunctionID;
        public string FunctionName;
        public string Signature;
        public bool IsAutoManaged;

        public RaylibFunctionPointer(uint functionID, string functionName, string signature, bool isAutoManaged)
        {
            FunctionID = functionID;
            FunctionName = functionName;
            Signature = signature;
            IsAutoManaged = isAutoManaged;
        }
    }
}
