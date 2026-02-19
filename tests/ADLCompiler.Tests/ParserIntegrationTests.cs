using ADLCompiler;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests;

public class ParserIntegrationTests
{
    private CompilationUnit ParseSource(string source)
    {
        var lexer = new Lexer(source, "test.adl");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.adl");
        return parser.Parse();
    }
    
    [Fact]
    public void Parse_CompleteJavaProgram_Success()
    {
        var source = @"
package com.example.app;

public class MainActivity {
    private String title;
    private int count = 0;
    
    public MainActivity(String title) {
        this.title = title;
    }
    
    public void onCreate() {
        count = count + 1;
        if (count > 10) {
            reset();
        }
    }
    
    private void reset() {
        count = 0;
    }
    
    public static void main(String[] args) {
        MainActivity app = new MainActivity(""Hello"");
        app.onCreate();
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        Assert.Equal("com.example.app", ast.PackageName);
        Assert.Single(ast.Classes);
        
        var cls = ast.Classes[0];
        Assert.Equal("MainActivity", cls.Name);
        Assert.Equal(AccessModifier.Public, cls.AccessModifier);
        Assert.Equal(2, cls.Fields.Count);
        Assert.Equal(4, cls.Methods.Count);
    }
    
    [Fact]
    public void Parse_CompleteCppProgram_Success()
    {
        var source = @"
namespace graphics {
    class Renderer {
        private:
            int* buffer;
            int width;
            int height;
        
        public:
            Renderer(int w, int h) {
                width = w;
                height = h;
                buffer = new int[w * h];
            }
            
            void clear() {
                for (int i = 0; i < width * height; i = i + 1) {
                    buffer[i] = 0;
                }
            }
            
            void setPixel(int x, int y, int color) {
                if (x >= 0 && x < width && y >= 0 && y < height) {
                    buffer[y * width + x] = color;
                }
            }
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        Assert.NotNull(ast.Namespace);
        Assert.Equal("graphics", ast.Namespace.Name);
        Assert.Single(ast.Namespace.Classes);
        
        var cls = ast.Namespace.Classes[0];
        Assert.Equal("Renderer", cls.Name);
        Assert.Equal(3, cls.Fields.Count);
        Assert.Equal(3, cls.Methods.Count);
    }
    
    [Fact]
    public void Parse_MixedJavaAndCppSyntax_Success()
    {
        var source = @"
class DataProcessor {
    private int* data;
    private int size;
    
    public DataProcessor(int size) {
        this.size = size;
        this.data = new int[size];
    }
    
    public void process() {
        for (int i = 0; i < size; i++) {
            data[i] = data[i] * 2;
        }
    }
    
    public int[] getData() {
        int[] result = new int[size];
        for (int i = 0; i < size; i++) {
            result[i] = data[i];
        }
        return result;
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        Assert.Single(ast.Classes);
        var cls = ast.Classes[0];
        Assert.Equal("DataProcessor", cls.Name);
        Assert.Equal(2, cls.Fields.Count);
        Assert.True(cls.Fields[0].FieldType.IsPointer);
        Assert.Equal(3, cls.Methods.Count);
    }
    
    [Fact]
    public void Parse_ComplexControlFlow_Success()
    {
        var source = @"
class Algorithm {
    public int calculate(int n) {
        int result = 0;
        
        for (int i = 0; i < n; i++) {
            if (i % 2 == 0) {
                result = result + i;
            } else {
                result = result - i;
            }
        }
        
        while (result > 100) {
            result = result / 2;
        }
        
        switch (result) {
            case 0:
                return -1;
            case 1:
                return 1;
            default:
                return result;
        }
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        var method = ast.Classes[0].Methods[0];
        Assert.Equal("calculate", method.Name);
        Assert.NotNull(method.Body);
        Assert.Equal(4, method.Body.Statements.Count);
    }
    
    [Fact]
    public void Parse_NestedExpressions_Success()
    {
        var source = @"
class Math {
    public int compute() {
        return ((a + b) * (c - d)) / ((e + f) * (g - h));
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        var method = ast.Classes[0].Methods[0];
        var returnStmt = Assert.IsType<ReturnStatement>(method.Body!.Statements[0]);
        Assert.NotNull(returnStmt.Value);
        Assert.IsType<BinaryExpression>(returnStmt.Value);
    }
    
    [Fact]
    public void Parse_ChainedMethodCalls_Success()
    {
        var source = @"
class Test {
    void test() {
        obj.method1().method2().method3();
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        var method = ast.Classes[0].Methods[0];
        var exprStmt = Assert.IsType<ExpressionStatement>(method.Body!.Statements[0]);
        Assert.IsType<MethodCallExpression>(exprStmt.Expression);
    }
    
    [Fact]
    public void Parse_MultipleClasses_Success()
    {
        var source = @"
class Person {
    private String name;
    private int age;
}

class Address {
    private String street;
    private String city;
}

class Employee extends Person {
    private Address address;
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        Assert.Equal(3, ast.Classes.Count);
        Assert.Equal("Person", ast.Classes[0].Name);
        Assert.Equal("Address", ast.Classes[1].Name);
        Assert.Equal("Employee", ast.Classes[2].Name);
        Assert.Equal("Person", ast.Classes[2].SuperClass);
    }
    
    [Fact]
    public void Parse_ComplexTypes_Success()
    {
        var source = @"
class Container {
    private int[] array;
    private int** matrix;
    private String[] names;
    
    public int[] getArray() {
        return array;
    }
    
    public void setMatrix(int** m) {
        matrix = m;
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        var cls = ast.Classes[0];
        Assert.True(cls.Fields[0].FieldType.IsArray);
        Assert.True(cls.Fields[1].FieldType.IsPointer);
        Assert.Equal(2, cls.Fields[1].FieldType.PointerLevel);
        Assert.True(cls.Fields[2].FieldType.IsArray);
    }
    
    [Fact]
    public void Parse_AllOperators_Success()
    {
        var source = @"
class Operators {
    void test() {
        int a = 1 + 2 - 3 * 4 / 5 % 6;
        boolean b = x == y && z != w || p < q && r > s;
        int c = m & n | o ^ p << 2 >> 1;
        int d = x > 0 ? 1 : -1;
        int e = -x;
        boolean f = !flag;
        int g = ~bits;
    }
}";
        
        var ast = ParseSource(source);
        
        Assert.NotNull(ast);
        var method = ast.Classes[0].Methods[0];
        Assert.Equal(7, method.Body!.Statements.Count);
    }
}
