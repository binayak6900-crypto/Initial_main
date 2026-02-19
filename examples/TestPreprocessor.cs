using ADLCompiler.Preprocessing;
using System;
using System.IO;

namespace PreprocessorDemo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ADL Preprocessor Demo");
        Console.WriteLine("====================\n");
        
        // Read the example file
        string examplePath = "PreprocessorExample.adl";
        if (!File.Exists(examplePath))
        {
            Console.WriteLine($"Error: {examplePath} not found");
            return;
        }
        
        string source = File.ReadAllText(examplePath);
        
        Console.WriteLine("Original Source:");
        Console.WriteLine("----------------");
        Console.WriteLine(source);
        Console.WriteLine("\n");
        
        // Process with preprocessor
        var preprocessor = new Preprocessor();
        var result = preprocessor.Process(source, examplePath);
        
        Console.WriteLine("Preprocessed Output:");
        Console.WriteLine("-------------------");
        Console.WriteLine(result.Source);
        Console.WriteLine("\n");
        
        // Show line mappings
        Console.WriteLine("Line Mappings (first 10):");
        Console.WriteLine("-------------------------");
        var mappings = result.LineMapping.GetMappings().Take(10);
        foreach (var mapping in mappings)
        {
            Console.WriteLine($"Preprocessed line {mapping.PreprocessedLine} -> {mapping.OriginalFile}:{mapping.OriginalLine}");
        }
    }
}
