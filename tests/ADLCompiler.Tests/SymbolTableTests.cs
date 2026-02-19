using ADLCompiler.SemanticAnalysis;
using SharedUtilities.Models;
using Xunit;

namespace ADLCompiler.Tests;

/// <summary>
/// Tests for SymbolTable class - scope management and symbol resolution
/// Requirements: 4.2, 4.4, 4.5
/// </summary>
public class SymbolTableTests
{
    private static Symbol CreateTestSymbol(string name, string typeName, SymbolKind kind)
    {
        return new Symbol(
            name,
            new TypeReference { Name = typeName },
            kind,
            new SourceLocation { Line = 1, Column = 1, File = "test.adl" }
        );
    }
    
    [Fact]
    public void Constructor_CreatesEmptyGlobalScope()
    {
        var table = new SymbolTable();
        
        Assert.Null(table.Parent);
        Assert.Equal(ScopeType.Global, table.ScopeType);
        Assert.Equal("global", table.ScopeName);
        Assert.Empty(table.Symbols);
    }
    
    [Fact]
    public void Constructor_WithParent_CreatesChildScope()
    {
        var parent = new SymbolTable();
        var child = new SymbolTable(parent, ScopeType.Method, "testMethod");
        
        Assert.Same(parent, child.Parent);
        Assert.Equal(ScopeType.Method, child.ScopeType);
        Assert.Equal("testMethod", child.ScopeName);
    }
    
    [Fact]
    public void Define_AddsSymbolToScope()
    {
        var table = new SymbolTable();
        var symbol = CreateTestSymbol("x", "int", SymbolKind.Variable);
        
        table.Define("x", symbol);
        
        Assert.Single(table.Symbols);
        Assert.True(table.IsDefined("x"));
    }
    
    [Fact]
    public void Define_DuplicateSymbol_ThrowsSemanticException()
    {
        var table = new SymbolTable();
        var symbol1 = CreateTestSymbol("x", "int", SymbolKind.Variable);
        var symbol2 = CreateTestSymbol("x", "string", SymbolKind.Variable);
        
        table.Define("x", symbol1);
        
        var exception = Assert.Throws<SemanticException>(() => table.Define("x", symbol2));
        Assert.Contains("already defined", exception.Message);
    }
    
    [Fact]
    public void Lookup_FindsSymbolInCurrentScope()
    {
        var table = new SymbolTable();
        var symbol = CreateTestSymbol("x", "int", SymbolKind.Variable);
        table.Define("x", symbol);
        
        var found = table.Lookup("x");
        
        Assert.NotNull(found);
        Assert.Equal("x", found.Name);
        Assert.Equal("int", found.Type.Name);
    }
    
    [Fact]
    public void Lookup_SymbolNotFound_ReturnsNull()
    {
        var table = new SymbolTable();
        
        var found = table.Lookup("nonexistent");
        
        Assert.Null(found);
    }
    
    [Fact]
    public void Lookup_TraversesParentScope()
    {
        var parent = new SymbolTable();
        var child = new SymbolTable(parent, ScopeType.Block, "block");
        
        var parentSymbol = CreateTestSymbol("x", "int", SymbolKind.Variable);
        parent.Define("x", parentSymbol);
        
        var found = child.Lookup("x");
        
        Assert.NotNull(found);
        Assert.Equal("x", found.Name);
    }
    
    [Fact]
    public void Lookup_ChildShadowsParent()
    {
        var parent = new SymbolTable();
        var child = new SymbolTable(parent, ScopeType.Block, "block");
        
        var parentSymbol = CreateTestSymbol("x", "int", SymbolKind.Variable);
        var childSymbol = CreateTestSymbol("x", "string", SymbolKind.Variable);
        
        parent.Define("x", parentSymbol);
        child.Define("x", childSymbol);
        
        var found = child.Lookup("x");
        
        Assert.NotNull(found);
        Assert.Equal("string", found.Type.Name); // Should find child's version
    }
    
    [Fact]
    public void LookupLocal_OnlySearchesCurrentScope()
    {
        var parent = new SymbolTable();
        var child = new SymbolTable(parent, ScopeType.Block, "block");
        
        var parentSymbol = CreateTestSymbol("x", "int", SymbolKind.Variable);
        parent.Define("x", parentSymbol);
        
        var found = child.LookupLocal("x");
        
        Assert.Null(found); // Should not find parent's symbol
    }
    
    [Fact]
    public void IsDefined_ChecksOnlyCurrentScope()
    {
        var parent = new SymbolTable();
        var child = new SymbolTable(parent, ScopeType.Block, "block");
        
        var parentSymbol = CreateTestSymbol("x", "int", SymbolKind.Variable);
        parent.Define("x", parentSymbol);
        
        Assert.True(parent.IsDefined("x"));
        Assert.False(child.IsDefined("x")); // Not defined in child scope
    }
    
    [Fact]
    public void Exists_ChecksScopeChain()
    {
        var parent = new SymbolTable();
        var child = new SymbolTable(parent, ScopeType.Block, "block");
        
        var parentSymbol = CreateTestSymbol("x", "int", SymbolKind.Variable);
        parent.Define("x", parentSymbol);
        
        Assert.True(child.Exists("x")); // Should find in parent
        Assert.False(child.Exists("y")); // Should not find nonexistent
    }
    
    [Fact]
    public void CreateChildScope_CreatesNewScopeWithParent()
    {
        var parent = new SymbolTable();
        var child = parent.CreateChildScope(ScopeType.Method, "testMethod");
        
        Assert.Same(parent, child.Parent);
        Assert.Equal(ScopeType.Method, child.ScopeType);
        Assert.Equal("testMethod", child.ScopeName);
    }
    
    [Fact]
    public void GetDepth_ReturnsCorrectDepth()
    {
        var global = new SymbolTable();
        var classScope = global.CreateChildScope(ScopeType.Class, "MyClass");
        var methodScope = classScope.CreateChildScope(ScopeType.Method, "myMethod");
        var blockScope = methodScope.CreateChildScope(ScopeType.Block, "if");
        
        Assert.Equal(0, global.GetDepth());
        Assert.Equal(1, classScope.GetDepth());
        Assert.Equal(2, methodScope.GetDepth());
        Assert.Equal(3, blockScope.GetDepth());
    }
    
    [Fact]
    public void NestedScopes_SupportsMultipleLevels()
    {
        // Global scope
        var global = new SymbolTable();
        global.Define("globalVar", CreateTestSymbol("globalVar", "int", SymbolKind.Variable));
        
        // Namespace scope
        var namespaceScope = global.CreateChildScope(ScopeType.Namespace, "MyNamespace");
        namespaceScope.Define("namespaceVar", CreateTestSymbol("namespaceVar", "string", SymbolKind.Variable));
        
        // Class scope
        var classScope = namespaceScope.CreateChildScope(ScopeType.Class, "MyClass");
        classScope.Define("field", CreateTestSymbol("field", "double", SymbolKind.Field));
        
        // Method scope
        var methodScope = classScope.CreateChildScope(ScopeType.Method, "myMethod");
        methodScope.Define("param", CreateTestSymbol("param", "bool", SymbolKind.Parameter));
        
        // Block scope
        var blockScope = methodScope.CreateChildScope(ScopeType.Block, "if");
        blockScope.Define("localVar", CreateTestSymbol("localVar", "char", SymbolKind.Variable));
        
        // Test lookup from deepest scope
        Assert.NotNull(blockScope.Lookup("localVar"));
        Assert.NotNull(blockScope.Lookup("param"));
        Assert.NotNull(blockScope.Lookup("field"));
        Assert.NotNull(blockScope.Lookup("namespaceVar"));
        Assert.NotNull(blockScope.Lookup("globalVar"));
        
        // Test that parent scopes don't see child symbols
        Assert.Null(methodScope.Lookup("localVar"));
        Assert.Null(classScope.Lookup("param"));
        Assert.Null(namespaceScope.Lookup("field"));
        Assert.Null(global.Lookup("namespaceVar"));
    }
    
    [Fact]
    public void GetAllAccessibleSymbols_ReturnsAllSymbolsInChain()
    {
        var parent = new SymbolTable();
        var child = new SymbolTable(parent, ScopeType.Block, "block");
        
        parent.Define("x", CreateTestSymbol("x", "int", SymbolKind.Variable));
        parent.Define("y", CreateTestSymbol("y", "string", SymbolKind.Variable));
        child.Define("z", CreateTestSymbol("z", "bool", SymbolKind.Variable));
        
        var allSymbols = child.GetAllAccessibleSymbols();
        
        Assert.Equal(3, allSymbols.Count);
        Assert.True(allSymbols.ContainsKey("x"));
        Assert.True(allSymbols.ContainsKey("y"));
        Assert.True(allSymbols.ContainsKey("z"));
    }
    
    [Fact]
    public void GetAllAccessibleSymbols_InnerScopeOverridesOuter()
    {
        var parent = new SymbolTable();
        var child = new SymbolTable(parent, ScopeType.Block, "block");
        
        parent.Define("x", CreateTestSymbol("x", "int", SymbolKind.Variable));
        child.Define("x", CreateTestSymbol("x", "string", SymbolKind.Variable));
        
        var allSymbols = child.GetAllAccessibleSymbols();
        
        Assert.Single(allSymbols);
        Assert.Equal("string", allSymbols["x"].Type.Name); // Should have child's version
    }
    
    [Fact]
    public void SymbolKinds_AllTypesSupported()
    {
        var table = new SymbolTable();
        
        table.Define("var", CreateTestSymbol("var", "int", SymbolKind.Variable));
        table.Define("param", CreateTestSymbol("param", "string", SymbolKind.Parameter));
        table.Define("method", CreateTestSymbol("method", "void", SymbolKind.Method));
        table.Define("field", CreateTestSymbol("field", "double", SymbolKind.Field));
        table.Define("class", CreateTestSymbol("class", "MyClass", SymbolKind.Class));
        table.Define("namespace", CreateTestSymbol("namespace", "MyNamespace", SymbolKind.Namespace));
        
        Assert.Equal(6, table.Symbols.Count);
        Assert.Equal(SymbolKind.Variable, table.Lookup("var")!.Kind);
        Assert.Equal(SymbolKind.Parameter, table.Lookup("param")!.Kind);
        Assert.Equal(SymbolKind.Method, table.Lookup("method")!.Kind);
        Assert.Equal(SymbolKind.Field, table.Lookup("field")!.Kind);
        Assert.Equal(SymbolKind.Class, table.Lookup("class")!.Kind);
        Assert.Equal(SymbolKind.Namespace, table.Lookup("namespace")!.Kind);
    }
    
    [Fact]
    public void ScopeTypes_AllTypesSupported()
    {
        var global = new SymbolTable(null, ScopeType.Global, "global");
        var namespaceScope = new SymbolTable(global, ScopeType.Namespace, "ns");
        var classScope = new SymbolTable(namespaceScope, ScopeType.Class, "class");
        var methodScope = new SymbolTable(classScope, ScopeType.Method, "method");
        var blockScope = new SymbolTable(methodScope, ScopeType.Block, "block");
        
        Assert.Equal(ScopeType.Global, global.ScopeType);
        Assert.Equal(ScopeType.Namespace, namespaceScope.ScopeType);
        Assert.Equal(ScopeType.Class, classScope.ScopeType);
        Assert.Equal(ScopeType.Method, methodScope.ScopeType);
        Assert.Equal(ScopeType.Block, blockScope.ScopeType);
    }
    
    [Fact]
    public void ComplexScenario_MethodWithNestedBlocks()
    {
        // Simulate: class MyClass { void myMethod(int param) { int x = 1; if (x > 0) { int y = 2; } } }
        
        var global = new SymbolTable();
        var classScope = global.CreateChildScope(ScopeType.Class, "MyClass");
        var methodScope = classScope.CreateChildScope(ScopeType.Method, "myMethod");
        
        // Add parameter
        methodScope.Define("param", CreateTestSymbol("param", "int", SymbolKind.Parameter));
        
        // Add local variable
        methodScope.Define("x", CreateTestSymbol("x", "int", SymbolKind.Variable));
        
        // Create if block
        var ifBlock = methodScope.CreateChildScope(ScopeType.Block, "if");
        ifBlock.Define("y", CreateTestSymbol("y", "int", SymbolKind.Variable));
        
        // Test lookups
        Assert.NotNull(ifBlock.Lookup("y")); // Local to if block
        Assert.NotNull(ifBlock.Lookup("x")); // From method scope
        Assert.NotNull(ifBlock.Lookup("param")); // From method scope
        
        Assert.Null(methodScope.Lookup("y")); // Not visible outside if block
        Assert.NotNull(methodScope.Lookup("x")); // Visible in method
        Assert.NotNull(methodScope.Lookup("param")); // Visible in method
    }
    
    [Fact]
    public void ComplexScenario_NamespaceWithMultipleClasses()
    {
        // Simulate: namespace MyNamespace { class A { int fieldA; } class B { int fieldB; } }
        
        var global = new SymbolTable();
        var namespaceScope = global.CreateChildScope(ScopeType.Namespace, "MyNamespace");
        
        // Class A
        var classAScope = namespaceScope.CreateChildScope(ScopeType.Class, "A");
        classAScope.Define("fieldA", CreateTestSymbol("fieldA", "int", SymbolKind.Field));
        
        // Class B
        var classBScope = namespaceScope.CreateChildScope(ScopeType.Class, "B");
        classBScope.Define("fieldB", CreateTestSymbol("fieldB", "int", SymbolKind.Field));
        
        // Test that classes don't see each other's fields
        Assert.NotNull(classAScope.Lookup("fieldA"));
        Assert.Null(classAScope.Lookup("fieldB"));
        
        Assert.NotNull(classBScope.Lookup("fieldB"));
        Assert.Null(classBScope.Lookup("fieldA"));
    }
}
