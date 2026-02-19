using ADLCompiler;
using System;
using System.IO;

class TestParser
{
    static void Main()
    {
        var source = File.ReadAllText("test_parser_simple.adl");
        
        Console.WriteLine("=== Lexing ===");
        var lexer = new Lexer(source, "test_parser_simple.adl");
        var tokens = lexer.Tokenize();
        Console.WriteLine($"Generated {tokens.Count} tokens");
        
        Console.WriteLine("\n=== Parsing ===");
        var parser = new Parser(tokens, "test_parser_simple.adl");
        var ast = parser.Parse();
        
        Console.WriteLine($"Parsed successfully!");
        Console.WriteLine($"File: {ast.FileName}");
        Console.WriteLine($"Classes: {ast.Classes.Count}");
        
        if (ast.Classes.Count > 0)
        {
            var cls = ast.Classes[0];
            Console.WriteLine($"\nClass: {cls.Name}");
            Console.WriteLine($"  Access: {cls.AccessModifier}");
            Console.WriteLine($"  Methods: {cls.Methods.Count}");
            Console.WriteLine($"  Fields: {cls.Fields.Count}");
            
            if (cls.Methods.Count > 0)
            {
                var method = cls.Methods[0];
                Console.WriteLine($"\n  Method: {method.Name}");
                Console.WriteLine($"    Return type: {method.ReturnType.Name}");
                Console.WriteLine($"    Parameters: {method.Parameters.Count}");
                Console.WriteLine($"    Is static: {method.IsStatic}");
                Console.WriteLine($"    Body statements: {method.Body?.Statements.Count ?? 0}");
            }
        }
        
        Console.WriteLine("\n✓ Parser test passed!");
    }
}
