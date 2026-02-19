using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCompiler.CodeGeneration
{
    /// <summary>
    /// Resource section containing strings, constants, and static data
    /// </summary>
    public class ResourceSection
    {
        public StringPool Strings { get; set; }
        public ConstantPool Constants { get; set; }
        public StaticData StaticData { get; set; }

        public ResourceSection()
        {
            Strings = new StringPool();
            Constants = new ConstantPool();
            StaticData = new StaticData();
        }
    }

    /// <summary>
    /// String pool for efficient string storage
    /// </summary>
    public class StringPool
    {
        private Dictionary<string, uint> _stringToID;
        private List<StringEntry> _strings;

        public StringPool()
        {
            _stringToID = new Dictionary<string, uint>();
            _strings = new List<StringEntry>();
        }

        /// <summary>
        /// Add a string to the pool and return its ID
        /// </summary>
        public uint AddString(string str)
        {
            if (_stringToID.TryGetValue(str, out uint existingID))
            {
                return existingID;
            }

            uint newID = (uint)_strings.Count;
            byte[] utf8Data = Encoding.UTF8.GetBytes(str);
            
            _strings.Add(new StringEntry
            {
                StringID = newID,
                Length = (uint)utf8Data.Length,
                UTF8Data = utf8Data
            });

            _stringToID[str] = newID;
            return newID;
        }

        /// <summary>
        /// Get a string by ID
        /// </summary>
        public string GetString(uint stringID)
        {
            if (stringID >= _strings.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(stringID));
            }

            return Encoding.UTF8.GetString(_strings[(int)stringID].UTF8Data);
        }

        /// <summary>
        /// Get all strings
        /// </summary>
        public IReadOnlyList<StringEntry> GetAllStrings()
        {
            return _strings.AsReadOnly();
        }

        /// <summary>
        /// Get string count
        /// </summary>
        public uint Count => (uint)_strings.Count;
    }

    /// <summary>
    /// Entry in the string pool
    /// </summary>
    public struct StringEntry
    {
        public uint StringID;
        public uint Length;
        public byte[] UTF8Data;
    }

    /// <summary>
    /// Constant pool for storing compile-time constants
    /// </summary>
    public class ConstantPool
    {
        private List<Constant> _constants;
        private Dictionary<string, uint> _constantIndex;

        public ConstantPool()
        {
            _constants = new List<Constant>();
            _constantIndex = new Dictionary<string, uint>();
        }

        /// <summary>
        /// Add an integer constant
        /// </summary>
        public uint AddInteger(int value)
        {
            string key = $"I:{value}";
            if (_constantIndex.TryGetValue(key, out uint existingID))
            {
                return existingID;
            }

            uint newID = (uint)_constants.Count;
            _constants.Add(new Constant
            {
                Type = ConstantType.Integer,
                Data = BitConverter.GetBytes(value)
            });

            _constantIndex[key] = newID;
            return newID;
        }

        /// <summary>
        /// Add a long constant
        /// </summary>
        public uint AddLong(long value)
        {
            string key = $"J:{value}";
            if (_constantIndex.TryGetValue(key, out uint existingID))
            {
                return existingID;
            }

            uint newID = (uint)_constants.Count;
            _constants.Add(new Constant
            {
                Type = ConstantType.Long,
                Data = BitConverter.GetBytes(value)
            });

            _constantIndex[key] = newID;
            return newID;
        }

        /// <summary>
        /// Add a float constant
        /// </summary>
        public uint AddFloat(float value)
        {
            string key = $"F:{value}";
            if (_constantIndex.TryGetValue(key, out uint existingID))
            {
                return existingID;
            }

            uint newID = (uint)_constants.Count;
            _constants.Add(new Constant
            {
                Type = ConstantType.Float,
                Data = BitConverter.GetBytes(value)
            });

            _constantIndex[key] = newID;
            return newID;
        }

        /// <summary>
        /// Add a double constant
        /// </summary>
        public uint AddDouble(double value)
        {
            string key = $"D:{value}";
            if (_constantIndex.TryGetValue(key, out uint existingID))
            {
                return existingID;
            }

            uint newID = (uint)_constants.Count;
            _constants.Add(new Constant
            {
                Type = ConstantType.Double,
                Data = BitConverter.GetBytes(value)
            });

            _constantIndex[key] = newID;
            return newID;
        }

        /// <summary>
        /// Add a string constant (references string pool)
        /// </summary>
        public uint AddString(uint stringPoolID)
        {
            string key = $"S:{stringPoolID}";
            if (_constantIndex.TryGetValue(key, out uint existingID))
            {
                return existingID;
            }

            uint newID = (uint)_constants.Count;
            _constants.Add(new Constant
            {
                Type = ConstantType.String,
                Data = BitConverter.GetBytes(stringPoolID)
            });

            _constantIndex[key] = newID;
            return newID;
        }

        /// <summary>
        /// Get all constants
        /// </summary>
        public IReadOnlyList<Constant> GetAllConstants()
        {
            return _constants.AsReadOnly();
        }

        /// <summary>
        /// Get constant count
        /// </summary>
        public uint Count => (uint)_constants.Count;
    }

    /// <summary>
    /// A constant value
    /// </summary>
    public struct Constant
    {
        public ConstantType Type;
        public byte[] Data;
    }

    /// <summary>
    /// Types of constants
    /// </summary>
    public enum ConstantType
    {
        Integer,
        Long,
        Float,
        Double,
        String,
        Class,
        MethodHandle,
        MethodType
    }

    /// <summary>
    /// Static data segment for global variables
    /// </summary>
    public class StaticData
    {
        private List<byte> _data;
        private Dictionary<string, uint> _symbolOffsets;

        public StaticData()
        {
            _data = new List<byte>();
            _symbolOffsets = new Dictionary<string, uint>();
        }

        /// <summary>
        /// Add data and return its offset
        /// </summary>
        public uint AddData(string symbolName, byte[] data)
        {
            uint offset = (uint)_data.Count;
            _data.AddRange(data);
            _symbolOffsets[symbolName] = offset;
            return offset;
        }

        /// <summary>
        /// Get offset of a symbol
        /// </summary>
        public uint GetSymbolOffset(string symbolName)
        {
            if (!_symbolOffsets.TryGetValue(symbolName, out uint offset))
            {
                throw new ArgumentException($"Symbol not found: {symbolName}");
            }
            return offset;
        }

        /// <summary>
        /// Get all data
        /// </summary>
        public byte[] GetData()
        {
            return _data.ToArray();
        }

        /// <summary>
        /// Get data size
        /// </summary>
        public uint Size => (uint)_data.Count;
    }
}
