using System;
using System.Collections.Generic;

namespace ADLCompiler.CodeGeneration
{
    /// <summary>
    /// Native code section containing machine code for C++-style code
    /// </summary>
    public class NativeCodeSection
    {
        public List<NativeFunction> Functions { get; set; }

        public NativeCodeSection()
        {
            Functions = new List<NativeFunction>();
        }
    }

    /// <summary>
    /// Represents a native function with machine code
    /// </summary>
    public class NativeFunction
    {
        public string FunctionName { get; set; }
        public string Signature { get; set; }
        public Architecture Architecture { get; set; }
        public uint CodeSize { get; set; }
        public byte[] MachineCode { get; set; }
        public List<Relocation> Relocations { get; set; }
        public uint StackFrameSize { get; set; }
        public CallingConvention Convention { get; set; }

        public NativeFunction()
        {
            Relocations = new List<Relocation>();
        }
    }

    /// <summary>
    /// Calling conventions for different architectures
    /// </summary>
    public enum CallingConvention
    {
        ARM_AAPCS,      // ARM Architecture Procedure Call Standard
        ARM64_AAPCS64,  // ARM64 calling convention
        x86_cdecl,      // x86 C declaration
        x86_64_SysV     // x86-64 System V ABI
    }

    /// <summary>
    /// Relocation entry for linking native code
    /// </summary>
    public struct Relocation
    {
        public uint Offset;
        public RelocationType Type;
        public string TargetSymbol;
        public int Addend;

        public Relocation(uint offset, RelocationType type, string targetSymbol, int addend = 0)
        {
            Offset = offset;
            Type = type;
            TargetSymbol = targetSymbol;
            Addend = addend;
        }
    }

    /// <summary>
    /// Types of relocations for native code
    /// </summary>
    public enum RelocationType
    {
        Absolute,       // Absolute address
        Relative,       // PC-relative address
        GOT,            // Global Offset Table
        PLT             // Procedure Linkage Table
    }

    /// <summary>
    /// ARM register definitions
    /// </summary>
    public static class ARMRegister
    {
        public const byte R0 = 0;
        public const byte R1 = 1;
        public const byte R2 = 2;
        public const byte R3 = 3;
        public const byte R4 = 4;
        public const byte R5 = 5;
        public const byte R6 = 6;
        public const byte R7 = 7;
        public const byte R8 = 8;
        public const byte R9 = 9;
        public const byte R10 = 10;
        public const byte R11 = 11;  // Frame pointer
        public const byte R12 = 12;  // Intra-procedure call scratch
        public const byte SP = 13;   // Stack pointer
        public const byte LR = 14;   // Link register
        public const byte PC = 15;   // Program counter
    }

    /// <summary>
    /// x86-64 register definitions
    /// </summary>
    public static class x86_64Register
    {
        public const byte RAX = 0;
        public const byte RCX = 1;
        public const byte RDX = 2;
        public const byte RBX = 3;
        public const byte RSP = 4;   // Stack pointer
        public const byte RBP = 5;   // Base pointer
        public const byte RSI = 6;
        public const byte RDI = 7;
        public const byte R8 = 8;
        public const byte R9 = 9;
        public const byte R10 = 10;
        public const byte R11 = 11;
        public const byte R12 = 12;
        public const byte R13 = 13;
        public const byte R14 = 14;
        public const byte R15 = 15;
    }

    /// <summary>
    /// Helper class for generating ARM machine code
    /// </summary>
    public static class ARMCodeGen
    {
        /// <summary>
        /// Generate PUSH instruction
        /// </summary>
        public static byte[] Push(params byte[] registers)
        {
            ushort regList = 0;
            foreach (var reg in registers)
            {
                regList |= (ushort)(1 << reg);
            }
            
            // PUSH {reglist} = STMDB SP!, {reglist}
            uint instruction = 0xE92D0000 | regList;
            return BitConverter.GetBytes(instruction);
        }

        /// <summary>
        /// Generate POP instruction
        /// </summary>
        public static byte[] Pop(params byte[] registers)
        {
            ushort regList = 0;
            foreach (var reg in registers)
            {
                regList |= (ushort)(1 << reg);
            }
            
            // POP {reglist} = LDMIA SP!, {reglist}
            uint instruction = 0xE8BD0000 | regList;
            return BitConverter.GetBytes(instruction);
        }

        /// <summary>
        /// Generate MOV instruction
        /// </summary>
        public static byte[] Mov(byte destReg, byte srcReg)
        {
            // MOV Rd, Rm
            uint instruction = 0xE1A00000 | ((uint)destReg << 12) | srcReg;
            return BitConverter.GetBytes(instruction);
        }

        /// <summary>
        /// Generate ADD instruction
        /// </summary>
        public static byte[] Add(byte destReg, byte srcReg, byte operandReg)
        {
            // ADD Rd, Rn, Rm
            uint instruction = 0xE0800000 | ((uint)destReg << 12) | ((uint)srcReg << 16) | operandReg;
            return BitConverter.GetBytes(instruction);
        }

        /// <summary>
        /// Generate BL (Branch with Link) instruction
        /// </summary>
        public static byte[] BranchLink(int offset)
        {
            // BL offset
            uint instruction = 0xEB000000 | ((uint)(offset >> 2) & 0x00FFFFFF);
            return BitConverter.GetBytes(instruction);
        }
    }

    /// <summary>
    /// Helper class for generating x86-64 machine code
    /// </summary>
    public static class x86_64CodeGen
    {
        /// <summary>
        /// Generate PUSH instruction
        /// </summary>
        public static byte[] Push(byte register)
        {
            // PUSH r64
            if (register < 8)
            {
                return new byte[] { (byte)(0x50 + register) };
            }
            else
            {
                return new byte[] { 0x41, (byte)(0x50 + (register - 8)) };
            }
        }

        /// <summary>
        /// Generate POP instruction
        /// </summary>
        public static byte[] Pop(byte register)
        {
            // POP r64
            if (register < 8)
            {
                return new byte[] { (byte)(0x58 + register) };
            }
            else
            {
                return new byte[] { 0x41, (byte)(0x58 + (register - 8)) };
            }
        }

        /// <summary>
        /// Generate MOV instruction (register to register)
        /// </summary>
        public static byte[] Mov(byte destReg, byte srcReg)
        {
            // MOV r64, r64
            byte rex = 0x48;
            if (destReg >= 8) rex |= 0x04;
            if (srcReg >= 8) rex |= 0x01;
            
            byte modRM = (byte)(0xC0 | ((destReg & 7) << 3) | (srcReg & 7));
            return new byte[] { rex, 0x89, modRM };
        }

        /// <summary>
        /// Generate ADD instruction
        /// </summary>
        public static byte[] Add(byte destReg, byte srcReg)
        {
            // ADD r64, r64
            byte rex = 0x48;
            if (destReg >= 8) rex |= 0x04;
            if (srcReg >= 8) rex |= 0x01;
            
            byte modRM = (byte)(0xC0 | ((srcReg & 7) << 3) | (destReg & 7));
            return new byte[] { rex, 0x01, modRM };
        }

        /// <summary>
        /// Generate CALL instruction (relative)
        /// </summary>
        public static byte[] Call(int offset)
        {
            // CALL rel32
            byte[] instruction = new byte[5];
            instruction[0] = 0xE8;
            BitConverter.GetBytes(offset).CopyTo(instruction, 1);
            return instruction;
        }

        /// <summary>
        /// Generate RET instruction
        /// </summary>
        public static byte[] Ret()
        {
            return new byte[] { 0xC3 };
        }
    }
}
