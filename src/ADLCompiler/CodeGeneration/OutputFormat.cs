using System;
using System.Collections.Generic;

namespace ADLCompiler.CodeGeneration
{
    /// <summary>
    /// Represents the complete ADL Binary Bundle (.adlb) output format
    /// </summary>
    public class ADLBinaryBundle
    {
        public ADLBHeader Header { get; set; }
        public BytecodeSection Bytecode { get; set; }
        public NativeCodeSection NativeCode { get; set; }
        public MemoryManagementSection MemoryManagement { get; set; }
        public MetadataSection Metadata { get; set; }
        public ResourceSection Resources { get; set; }

        public ADLBinaryBundle()
        {
            Header = new ADLBHeader();
            Bytecode = new BytecodeSection();
            NativeCode = new NativeCodeSection();
            MemoryManagement = new MemoryManagementSection();
            Metadata = new MetadataSection();
            Resources = new ResourceSection();
        }
    }

    /// <summary>
    /// Header structure for ADL Binary Bundle
    /// </summary>
    public struct ADLBHeader
    {
        public const uint MAGIC_NUMBER = 0x41444C42; // "ADLB"
        public const uint CURRENT_VERSION = 1;

        public uint MagicNumber;
        public uint Version;
        public uint TargetSDK;
        public uint ArchitectureFlags;
        public uint BytecodeSectionOffset;
        public uint NativeCodeSectionOffset;
        public uint MemoryManagementSectionOffset;
        public uint MetadataSectionOffset;
        public uint ResourceSectionOffset;
        public uint TotalSize;

        public static ADLBHeader Create(uint targetSDK, uint architectureFlags)
        {
            return new ADLBHeader
            {
                MagicNumber = MAGIC_NUMBER,
                Version = CURRENT_VERSION,
                TargetSDK = targetSDK,
                ArchitectureFlags = architectureFlags
            };
        }
    }

    /// <summary>
    /// Architecture flags for multi-platform support
    /// </summary>
    [Flags]
    public enum Architecture : uint
    {
        ARM = 1,
        ARM64 = 2,
        x86 = 4,
        x86_64 = 8
    }

    /// <summary>
    /// Access flags for classes, methods, and fields
    /// </summary>
    [Flags]
    public enum AccessFlags : ushort
    {
        Public = 0x0001,
        Private = 0x0002,
        Protected = 0x0004,
        Static = 0x0008,
        Final = 0x0010,
        Synchronized = 0x0020,
        Volatile = 0x0040,
        Transient = 0x0080,
        Native = 0x0100,
        Interface = 0x0200,
        Abstract = 0x0400,
        Strict = 0x0800,
        Synthetic = 0x1000,
        Annotation = 0x2000,
        Enum = 0x4000
    }
}
