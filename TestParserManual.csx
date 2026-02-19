#r "src/SharedUtilities/bin/Debug/net8.0/SharedUtilities.dll"
#r "src/ADLCompiler/bin/Debug/net8.0/ADLCompiler.dll"

using ADLCompiler;
using SharedUtilities.Models;

var source = @"
public class HelloWorld {
    public static void main(String[] args) {
        int x = 42;
    }
}";

Console.WriteLine("=== Testing Parser ===");

var lexer = new Lexer(source, "test.adl");
var tokens = lexer.Tokenize();
Console.WriteLine($"✓ Lexer generated {tokens.Count} tokens");

var parser = new Parser(tokens, "test.adl");
var ast = parser.Parse();

Console.WriteLine($"✓ Parser succeeded!");
Console.WriteLine($"  Classes: {ast.Classes.Count}");
Console.WriteLine($"  Class name: {ast.Classes[0].Name}");
Console.WriteLine($"  Methods: {ast.Classes[0].Methods.Count}");
Console.WriteLine($"  Method name: {ast.Classes[0].Methods[0].Name}");

Console.WriteLine("\n✓ All parser tests passed!");
