using System;
using System.Collections.Generic;

namespace ADLCompiler.CodeGeneration
{
    /// <summary>
    /// Bytecode section containing Dalvik-compatible bytecode for Java-style code
    /// </summary>
    public class BytecodeSection
    {
        public List<BytecodeClass> Classes { get; set; }

        public BytecodeSection()
        {
            Classes = new List<BytecodeClass>();
        }
    }

    /// <summary>
    /// Represents a class in bytecode format
    /// </summary>
    public class BytecodeClass
    {
        public string ClassName { get; set; }
        public string SuperClassName { get; set; }
        public List<string> Interfaces { get; set; }
        public AccessFlags AccessFlags { get; set; }
        public List<BytecodeField> Fields { get; set; }
        public List<BytecodeMethod> Methods { get; set; }

        public BytecodeClass()
        {
            Interfaces = new List<string>();
            Fields = new List<BytecodeField>();
            Methods = new List<BytecodeMethod>();
        }
    }

    /// <summary>
    /// Represents a field in bytecode format
    /// </summary>
    public class BytecodeField
    {
        public string Name { get; set; }
        public string TypeDescriptor { get; set; }
        public AccessFlags AccessFlags { get; set; }
        public object InitialValue { get; set; }
    }

    /// <summary>
    /// Represents a method in bytecode format
    /// </summary>
    public class BytecodeMethod
    {
        public string Name { get; set; }
        public string Signature { get; set; }
        public AccessFlags AccessFlags { get; set; }
        public ushort MaxStack { get; set; }
        public ushort MaxLocals { get; set; }
        public List<BytecodeInstruction> Instructions { get; set; }
        public List<ExceptionHandler> ExceptionHandlers { get; set; }

        public BytecodeMethod()
        {
            Instructions = new List<BytecodeInstruction>();
            ExceptionHandlers = new List<ExceptionHandler>();
        }
    }

    /// <summary>
    /// Represents a single bytecode instruction
    /// </summary>
    public struct BytecodeInstruction
    {
        public byte Opcode;
        public byte[] Operands;
        public uint SourceLine;

        public BytecodeInstruction(byte opcode, byte[] operands, uint sourceLine)
        {
            Opcode = opcode;
            Operands = operands;
            SourceLine = sourceLine;
        }
    }

    /// <summary>
    /// Exception handler for try-catch blocks
    /// </summary>
    public struct ExceptionHandler
    {
        public uint StartOffset;
        public uint EndOffset;
        public uint HandlerOffset;
        public string ExceptionType;
    }

    /// <summary>
    /// Bytecode opcodes - Dalvik-compatible with ADL extensions
    /// </summary>
    public static class Opcode
    {
        // Standard Dalvik opcodes (0x00-0xEF)
        public const byte NOP = 0x00;
        public const byte MOVE = 0x01;
        public const byte MOVE_WIDE = 0x04;
        public const byte MOVE_OBJECT = 0x07;
        public const byte RETURN_VOID = 0x0E;
        public const byte RETURN = 0x0F;
        public const byte RETURN_WIDE = 0x10;
        public const byte RETURN_OBJECT = 0x11;
        public const byte CONST_4 = 0x12;
        public const byte CONST_16 = 0x13;
        public const byte CONST = 0x14;
        public const byte CONST_WIDE = 0x18;
        public const byte CONST_STRING = 0x1A;
        public const byte CONST_CLASS = 0x1C;
        
        // Arithmetic
        public const byte ADD_INT = 0x90;
        public const byte SUB_INT = 0x91;
        public const byte MUL_INT = 0x92;
        public const byte DIV_INT = 0x93;
        public const byte REM_INT = 0x94;
        public const byte AND_INT = 0x95;
        public const byte OR_INT = 0x96;
        public const byte XOR_INT = 0x97;
        
        // Comparison
        public const byte CMP_LONG = 0x31;
        public const byte IF_EQ = 0x32;
        public const byte IF_NE = 0x33;
        public const byte IF_LT = 0x34;
        public const byte IF_GE = 0x35;
        public const byte IF_GT = 0x36;
        public const byte IF_LE = 0x37;
        
        // Array operations
        public const byte AGET = 0x44;
        public const byte APUT = 0x4B;
        public const byte ARRAY_LENGTH = 0x21;
        public const byte NEW_ARRAY = 0x23;
        
        // Instance operations
        public const byte IGET = 0x52;
        public const byte IPUT = 0x59;
        public const byte NEW_INSTANCE = 0x22;
        public const byte INSTANCE_OF = 0x20;
        public const byte CHECK_CAST = 0x1F;
        
        // Static operations
        public const byte SGET = 0x60;
        public const byte SPUT = 0x67;
        
        // Method invocation
        public const byte INVOKE_VIRTUAL = 0x6E;
        public const byte INVOKE_SUPER = 0x6F;
        public const byte INVOKE_DIRECT = 0x70;
        public const byte INVOKE_STATIC = 0x71;
        public const byte INVOKE_INTERFACE = 0x72;
        
        // ADL Extension opcodes (0xF0-0xFF)
        public const byte INVOKE_BUILTIN = 0xF0;    // Call built-in Android/NDK API
        public const byte INVOKE_NATIVE = 0xF1;     // Call native C++ function
        public const byte REF_COUNT_INC = 0xF2;     // Increment reference count
        public const byte REF_COUNT_DEC = 0xF3;     // Decrement reference count
        public const byte AUTO_CLEANUP = 0xF4;      // Automatic cleanup at scope boundary
        public const byte NULL_CHECK = 0xF5;        // Automatic null safety check
        public const byte RAYLIB_CALL = 0xF6;       // Call raylib function
        // 0xF7-0xFF reserved for future extensions
    }

    /// <summary>
    /// Type descriptor utilities for Dalvik format
    /// </summary>
    public static class TypeDescriptor
    {
        // Primitive types
        public const string Void = "V";
        public const string Boolean = "Z";
        public const string Byte = "B";
        public const string Short = "S";
        public const string Char = "C";
        public const string Int = "I";
        public const string Long = "J";
        public const string Float = "F";
        public const string Double = "D";

        /// <summary>
        /// Creates a class type descriptor
        /// </summary>
        public static string Class(string className)
        {
            return $"L{className.Replace('.', '/')};";
        }

        /// <summary>
        /// Creates an array type descriptor
        /// </summary>
        public static string Array(string elementType)
        {
            return $"[{elementType}";
        }

        /// <summary>
        /// Creates a method signature
        /// </summary>
        public static string MethodSignature(string[] parameterTypes, string returnType)
        {
            return $"({string.Join("", parameterTypes)}){returnType}";
        }
    }
}
