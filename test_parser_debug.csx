#r "src/SharedUtilities/bin/Debug/net8.0/SharedUtilities.dll"
#r "src/ADLCompiler/bin/Debug/net8.0/ADLCompiler.dll"

using ADLCompiler;
using System;

var source = @"
class Test {
    void test() {
        int i = 0;
        ++i;
    }
}";

Console.WriteLine("Source:");
Console.WriteLine(source);
Console.WriteLine();

var lexer = new Lexer(source, "test.adl");
var tokens = lexer.Tokenize();

Console.WriteLine("Tokens:");
foreach (var token in tokens)
{
    Console.WriteLine($"{token.Line}:{token.Column} {token.Type} = '{token.Value}'");
}
Console.WriteLine();

try
{
    var parser = new Parser(tokens, "test.adl");
    var ast = parser.Parse();
    Console.WriteLine("Parse successful!");
    Console.WriteLine($"Classes: {ast.Classes.Count}");
    if (ast.Classes.Count > 0)
    {
        Console.WriteLine($"Class name: {ast.Classes[0].Name}");
        Console.WriteLine($"Methods: {ast.Classes[0].Methods.Count}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Parse error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
