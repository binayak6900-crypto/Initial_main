using ADLCompiler;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests;

public class ParserTests
{
    private CompilationUnit ParseSource(string source)
    {
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        return parser.Parse();
    }
    
    [Fact]
    public void Parse_SimpleJavaClass_Success()
    {
        var source = @"
public class HelloWorld {
    public static void main(String[] args) {
        int x = 42;
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        Assert.Single(ast.Classes);
        Assert.Equal("HelloWorld", ast.Classes[0].Name);
        Assert.Equal(AccessModifier.Public, ast.Classes[0].AccessModifier);
        Assert.Single(ast.Classes[0].Methods);
        Assert.Equal("main", ast.Classes[0].Methods[0].Name);
        Assert.True(ast.Classes[0].Methods[0].IsStatic);
    }
    
    [Fact]
    public void Parse_CppNamespace_Success()
    {
        var source = @"
namespace com::example {
    class MyClass {
        void doSomething() {
        }
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        Assert.NotNull(ast.Namespace);
        Assert.Equal("com.example", ast.Namespace.Name);
        Assert.Single(ast.Namespace.Classes);
        Assert.Equal("MyClass", ast.Namespace.Classes[0].Name);
    }
    
    [Fact]
    public void Parse_ClassWithFields_Success()
    {
        var source = @"
class Person {
    private String name;
    private int age;
    public boolean active = true;
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        Assert.Single(ast.Classes);
        var cls = ast.Classes[0];
        Assert.Equal(3, cls.Fields.Count);
        Assert.Equal("name", cls.Fields[0].Name);
        Assert.Equal("String", cls.Fields[0].FieldType.Name);
        Assert.Equal(AccessModifier.Private, cls.Fields[0].AccessModifier);
        Assert.Equal("age", cls.Fields[1].Name);
        Assert.Equal("int", cls.Fields[1].FieldType.Name);
        Assert.NotNull(cls.Fields[2].Initializer);
    }
    
    [Fact]
    public void Parse_MethodWithParameters_Success()
    {
        var source = @"
class Calculator {
    public int add(int a, int b) {
        return a + b;
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        var method = ast.Classes[0].Methods[0];
        Assert.Equal("add", method.Name);
        Assert.Equal("int", method.ReturnType.Name);
        Assert.Equal(2, method.Parameters.Count);
        Assert.Equal("a", method.Parameters[0].Name);
        Assert.Equal("b", method.Parameters[1].Name);
        Assert.NotNull(method.Body);
    }
    
    [Fact]
    public void Parse_IfStatement_Success()
    {
        var source = @"
class Test {
    void test() {
        if (x > 0) {
            y = 1;
        } else {
            y = 0;
        }
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        Assert.NotNull(method.Body);
        Assert.Single(method.Body.Statements);
        var ifStmt = Assert.IsType<IfStatement>(method.Body.Statements[0]);
        Assert.NotNull(ifStmt.Condition);
        Assert.NotNull(ifStmt.ThenBranch);
        Assert.NotNull(ifStmt.ElseBranch);
    }
    
    [Fact]
    public void Parse_WhileLoop_Success()
    {
        var source = @"
class Test {
    void test() {
        while (i < 10) {
            i = i + 1;
        }
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var whileStmt = Assert.IsType<WhileStatement>(method.Body!.Statements[0]);
        Assert.NotNull(whileStmt.Condition);
        Assert.NotNull(whileStmt.Body);
    }
    
    [Fact]
    public void Parse_ForLoop_Success()
    {
        var source = @"
class Test {
    void test() {
        for (int i = 0; i < 10; i = i + 1) {
            sum = sum + i;
        }
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var forStmt = Assert.IsType<ForStatement>(method.Body!.Statements[0]);
        Assert.NotNull(forStmt.Initializer);
        Assert.NotNull(forStmt.Condition);
        Assert.NotNull(forStmt.Increment);
        Assert.NotNull(forStmt.Body);
    }
    
    [Fact]
    public void Parse_SwitchStatement_Success()
    {
        var source = @"
class Test {
    void test() {
        switch (x) {
            case 1:
                y = 10;
                break;
            case 2:
                y = 20;
                break;
            default:
                y = 0;
                break;
        }
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var switchStmt = Assert.IsType<SwitchStatement>(method.Body!.Statements[0]);
        Assert.NotNull(switchStmt.Expression);
        Assert.Equal(3, switchStmt.Cases.Count);
        Assert.NotNull(switchStmt.Cases[0].Value);
        Assert.NotNull(switchStmt.Cases[1].Value);
        Assert.Null(switchStmt.Cases[2].Value); // default case
    }
    
    [Fact]
    public void Parse_BinaryExpressions_Success()
    {
        var source = @"
class Test {
    void test() {
        int result = a + b * c - d / e;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        Assert.NotNull(varDecl.Initializer);
        var expr = Assert.IsType<BinaryExpression>(varDecl.Initializer);
        Assert.Equal(BinaryOperator.Subtract, expr.Operator);
    }
    
    [Fact]
    public void Parse_MethodCall_Success()
    {
        var source = @"
class Test {
    void test() {
        obj.doSomething(1, 2, 3);
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var exprStmt = Assert.IsType<ExpressionStatement>(method.Body!.Statements[0]);
        var callExpr = Assert.IsType<MethodCallExpression>(exprStmt.Expression);
        Assert.Equal("doSomething", callExpr.MethodName);
        Assert.Equal(3, callExpr.Arguments.Count);
    }
    
    [Fact]
    public void Parse_ArrayAccess_Success()
    {
        var source = @"
class Test {
    void test() {
        int value = arr[5];
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        var arrayAccess = Assert.IsType<ArrayAccessExpression>(varDecl.Initializer);
        Assert.NotNull(arrayAccess.Array);
        Assert.NotNull(arrayAccess.Index);
    }
    
    [Fact]
    public void Parse_NewExpression_Success()
    {
        var source = @"
class Test {
    void test() {
        Person p = new Person(""John"", 30);
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        var newExpr = Assert.IsType<NewExpression>(varDecl.Initializer);
        Assert.Equal("Person", newExpr.TypeToCreate.Name);
        Assert.Equal(2, newExpr.Arguments.Count);
    }
    
    [Fact]
    public void Parse_PointerType_Success()
    {
        var source = @"
class Test {
    void test() {
        int* ptr;
        int** ptrptr;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl1 = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        Assert.True(varDecl1.VariableType.IsPointer);
        Assert.Equal(1, varDecl1.VariableType.PointerLevel);
        
        var varDecl2 = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[1]);
        Assert.True(varDecl2.VariableType.IsPointer);
        Assert.Equal(2, varDecl2.VariableType.PointerLevel);
    }
    
    [Fact]
    public void Parse_ArrayType_Success()
    {
        var source = @"
class Test {
    void test() {
        int[] arr;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        Assert.True(varDecl.VariableType.IsArray);
    }
    
    [Fact]
    public void Parse_TernaryExpression_Success()
    {
        var source = @"
class Test {
    void test() {
        int result = x > 0 ? 1 : -1;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        var ternary = Assert.IsType<TernaryExpression>(varDecl.Initializer);
        Assert.NotNull(ternary.Condition);
        Assert.NotNull(ternary.TrueExpression);
        Assert.NotNull(ternary.FalseExpression);
    }
    
    [Fact]
    public void Parse_UnaryExpressions_Success()
    {
        var source = @"
class Test {
    void test() {
        int a = -x;
        boolean b = !flag;
        int c = ~bits;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl1 = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        var unary1 = Assert.IsType<UnaryExpression>(varDecl1.Initializer);
        Assert.Equal(UnaryOperator.Negate, unary1.Operator);
        
        var varDecl2 = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[1]);
        var unary2 = Assert.IsType<UnaryExpression>(varDecl2.Initializer);
        Assert.Equal(UnaryOperator.LogicalNot, unary2.Operator);
        
        var varDecl3 = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[2]);
        var unary3 = Assert.IsType<UnaryExpression>(varDecl3.Initializer);
        Assert.Equal(UnaryOperator.BitwiseNot, unary3.Operator);
    }
    
    [Fact]
    public void Parse_IncrementDecrement_Success()
    {
        var source = @"
class Test {
    void test() {
        ++i;
        i++;
        --j;
        j--;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var expr1 = Assert.IsType<ExpressionStatement>(method.Body!.Statements[0]);
        var unary1 = Assert.IsType<UnaryExpression>(expr1.Expression);
        Assert.Equal(UnaryOperator.PreIncrement, unary1.Operator);
        
        var expr2 = Assert.IsType<ExpressionStatement>(method.Body!.Statements[1]);
        var unary2 = Assert.IsType<UnaryExpression>(expr2.Expression);
        Assert.Equal(UnaryOperator.PostIncrement, unary2.Operator);
    }
    
    [Fact]
    public void Parse_CastExpression_Success()
    {
        var source = @"
class Test {
    void test() {
        int x = (int) y;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        var cast = Assert.IsType<CastExpression>(varDecl.Initializer);
        Assert.Equal("int", cast.TargetType.Name);
    }
    
    [Fact]
    public void Parse_PackageDeclaration_Success()
    {
        var source = @"
package com.example.app;

class MyClass {
}";
        
        var ast = ParseSource(source);
        
        Assert.Equal("com.example.app", ast.PackageName);
        Assert.Single(ast.Classes);
    }
    
    [Fact]
    public void Parse_ClassInheritance_Success()
    {
        var source = @"
class Child extends Parent {
}";
        
        var ast = ParseSource(source);
        
        Assert.Equal("Child", ast.Classes[0].Name);
        Assert.Equal("Parent", ast.Classes[0].SuperClass);
    }
    
    [Fact]
    public void Parse_InterfaceImplementation_Success()
    {
        var source = @"
class MyClass implements Interface1, Interface2 {
}";
        
        var ast = ParseSource(source);
        
        Assert.Equal(2, ast.Classes[0].Interfaces.Count);
        Assert.Contains("Interface1", ast.Classes[0].Interfaces);
        Assert.Contains("Interface2", ast.Classes[0].Interfaces);
    }
    
    [Fact]
    public void Parse_AbstractClass_Success()
    {
        var source = @"
public abstract class AbstractClass {
    public abstract void doSomething();
}";
        
        var ast = ParseSource(source);
        
        Assert.True(ast.Classes[0].IsAbstract);
        Assert.True(ast.Classes[0].Methods[0].IsAbstract);
        Assert.Null(ast.Classes[0].Methods[0].Body);
    }
    
    [Fact]
    public void Parse_StaticMembers_Success()
    {
        var source = @"
class Test {
    public static int count = 0;
    public static void increment() {
        count = count + 1;
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.True(ast.Classes[0].Fields[0].IsStatic);
        Assert.True(ast.Classes[0].Methods[0].IsStatic);
    }
    
    [Fact]
    public void Parse_DoWhileLoop_Success()
    {
        var source = @"
class Test {
    void test() {
        do {
            i = i + 1;
        } while (i < 10);
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var doWhile = Assert.IsType<DoWhileStatement>(method.Body!.Statements[0]);
        Assert.NotNull(doWhile.Body);
        Assert.NotNull(doWhile.Condition);
    }
    
    [Fact]
    public void Parse_ComplexExpression_Success()
    {
        var source = @"
class Test {
    void test() {
        int result = (a + b) * (c - d) / e;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        Assert.NotNull(varDecl.Initializer);
        Assert.IsType<BinaryExpression>(varDecl.Initializer);
    }
    
    [Fact]
    public void Parse_MemberAccess_Success()
    {
        var source = @"
class Test {
    void test() {
        int value = obj.field;
    }
}";
        
        var ast = ParseSource(source);
        
        var method = ast.Classes[0].Methods[0];
        var varDecl = Assert.IsType<VariableDeclarationStatement>(method.Body!.Statements[0]);
        var memberAccess = Assert.IsType<MemberAccessExpression>(varDecl.Initializer);
        Assert.Equal("field", memberAccess.MemberName);
    }
}
