using ADLCompiler;
using System;

var source = @"
class Calculator {
    public int add(int a, int b) {
        return a + b;
    }
}
";

var lexer = new Lexer(source, "test.adl");
var tokens = lexer.Tokenize();
var parser = new Parser(tokens, "test.adl");
var compilationUnit = parser.Parse();

var symbolTableBuilder = new SymbolTableBuilder();
var symbolTable = symbolTableBuilder.Build(compilationUnit);
var typeChecker = new TypeChecker(symbolTable);

var result = typeChecker.Check(compilationUnit);

Console.WriteLine($"Type checking result: {result}");
Console.WriteLine($"Number of errors: {typeChecker.Errors.Count}");
foreach (var error in typeChecker.Errors)
{
    Console.WriteLine($"  - {error}");
}
