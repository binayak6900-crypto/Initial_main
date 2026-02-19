using ADLCompiler.SemanticAnalysis;
using Xunit;

namespace ADLCompiler.Tests;

/// <summary>
/// Tests for raylib drawing API bindings
/// Validates Requirement 14.9: Built-in access to all raylib drawing functions
/// Task 16.2: Implement raylib drawing functions
/// </summary>
public class RaylibDrawingAPITests
{
    [Fact]
    public void APIRegistry_LoadsRaylibDrawingLifecycleAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibDrawingAPI = bundle.LookupClass("raylib.Drawing");
        Assert.NotNull(raylibDrawingAPI);
        
        // Drawing lifecycle - beginDrawing
        var beginDrawingMethod = bundle.LookupMethod("raylib.Drawing", "beginDrawing");
        Assert.NotNull(beginDrawingMethod);
        Assert.Equal("void", beginDrawingMethod.ReturnType);
        Assert.Empty(beginDrawingMethod.Parameters);
        Assert.True(beginDrawingMethod.IsStatic);
        
        // Drawing lifecycle - endDrawing
        var endDrawingMethod = bundle.LookupMethod("raylib.Drawing", "endDrawing");
        Assert.NotNull(endDrawingMethod);
        Assert.Equal("void", endDrawingMethod.ReturnType);
        Assert.Empty(endDrawingMethod.Parameters);
        Assert.True(endDrawingMethod.IsStatic);
        
        // Drawing lifecycle - clearBackground
        var clearBackgroundMethod = bundle.LookupMethod("raylib.Drawing", "clearBackground");
        Assert.NotNull(clearBackgroundMethod);
        Assert.Equal("void", clearBackgroundMethod.ReturnType);
        Assert.Single(clearBackgroundMethod.Parameters);
        Assert.Equal("color", clearBackgroundMethod.Parameters[0].Name);
        Assert.Equal("raylib.Color", clearBackgroundMethod.Parameters[0].Type);
        Assert.True(clearBackgroundMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibColorConstants()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibDrawingAPI = bundle.LookupClass("raylib.Drawing");
        Assert.NotNull(raylibDrawingAPI);
        
        // Color constants
        var raywhiteField = bundle.LookupField("raylib.Drawing", "RAYWHITE");
        Assert.NotNull(raywhiteField);
        Assert.Equal("raylib.Color", raywhiteField.Type);
        Assert.True(raywhiteField.IsStatic);
        
        var whiteField = bundle.LookupField("raylib.Drawing", "WHITE");
        Assert.NotNull(whiteField);
        Assert.True(whiteField.IsStatic);
        
        var blackField = bundle.LookupField("raylib.Drawing", "BLACK");
        Assert.NotNull(blackField);
        
        var redField = bundle.LookupField("raylib.Drawing", "RED");
        Assert.NotNull(redField);
        
        var greenField = bundle.LookupField("raylib.Drawing", "GREEN");
        Assert.NotNull(greenField);
        
        var blueField = bundle.LookupField("raylib.Drawing", "BLUE");
        Assert.NotNull(blueField);
        
        var yellowField = bundle.LookupField("raylib.Drawing", "YELLOW");
        Assert.NotNull(yellowField);
        
        var darkgrayField = bundle.LookupField("raylib.Drawing", "DARKGRAY");
        Assert.NotNull(darkgrayField);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibPixelDrawingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibShapesAPI = bundle.LookupClass("raylib.Shapes");
        Assert.NotNull(raylibShapesAPI);
        
        // Pixel drawing - drawPixel
        var drawPixelMethod = bundle.LookupMethod("raylib.Shapes", "drawPixel");
        Assert.NotNull(drawPixelMethod);
        Assert.Equal("void", drawPixelMethod.ReturnType);
        Assert.Equal(3, drawPixelMethod.Parameters.Count);
        Assert.Equal("posX", drawPixelMethod.Parameters[0].Name);
        Assert.Equal("int", drawPixelMethod.Parameters[0].Type);
        Assert.Equal("posY", drawPixelMethod.Parameters[1].Name);
        Assert.Equal("int", drawPixelMethod.Parameters[1].Type);
        Assert.Equal("color", drawPixelMethod.Parameters[2].Name);
        Assert.Equal("raylib.Color", drawPixelMethod.Parameters[2].Type);
        Assert.True(drawPixelMethod.IsStatic);
        
        // Pixel drawing - drawPixelV
        var drawPixelVMethod = bundle.LookupMethod("raylib.Shapes", "drawPixelV");
        Assert.NotNull(drawPixelVMethod);
        Assert.Equal("void", drawPixelVMethod.ReturnType);
        Assert.Equal(2, drawPixelVMethod.Parameters.Count);
        Assert.Equal("position", drawPixelVMethod.Parameters[0].Name);
        Assert.Equal("raylib.Vector2", drawPixelVMethod.Parameters[0].Type);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibLineDrawingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibShapesAPI = bundle.LookupClass("raylib.Shapes");
        Assert.NotNull(raylibShapesAPI);
        
        // Line drawing - drawLine
        var drawLineMethod = bundle.LookupMethod("raylib.Shapes", "drawLine");
        Assert.NotNull(drawLineMethod);
        Assert.Equal("void", drawLineMethod.ReturnType);
        Assert.Equal(5, drawLineMethod.Parameters.Count);
        Assert.Equal("startPosX", drawLineMethod.Parameters[0].Name);
        Assert.Equal("int", drawLineMethod.Parameters[0].Type);
        Assert.Equal("startPosY", drawLineMethod.Parameters[1].Name);
        Assert.Equal("int", drawLineMethod.Parameters[1].Type);
        Assert.Equal("endPosX", drawLineMethod.Parameters[2].Name);
        Assert.Equal("int", drawLineMethod.Parameters[2].Type);
        Assert.Equal("endPosY", drawLineMethod.Parameters[3].Name);
        Assert.Equal("int", drawLineMethod.Parameters[3].Type);
        Assert.Equal("color", drawLineMethod.Parameters[4].Name);
        Assert.Equal("raylib.Color", drawLineMethod.Parameters[4].Type);
        Assert.True(drawLineMethod.IsStatic);
        
        // Line drawing - drawLineV
        var drawLineVMethod = bundle.LookupMethod("raylib.Shapes", "drawLineV");
        Assert.NotNull(drawLineVMethod);
        Assert.Equal("void", drawLineVMethod.ReturnType);
        Assert.Equal(3, drawLineVMethod.Parameters.Count);
        Assert.Equal("startPos", drawLineVMethod.Parameters[0].Name);
        Assert.Equal("raylib.Vector2", drawLineVMethod.Parameters[0].Type);
        Assert.Equal("endPos", drawLineVMethod.Parameters[1].Name);
        Assert.Equal("raylib.Vector2", drawLineVMethod.Parameters[1].Type);
        
        // Line drawing - drawLineEx
        var drawLineExMethod = bundle.LookupMethod("raylib.Shapes", "drawLineEx");
        Assert.NotNull(drawLineExMethod);
        Assert.Equal("void", drawLineExMethod.ReturnType);
        Assert.Equal(4, drawLineExMethod.Parameters.Count);
        Assert.Equal("thick", drawLineExMethod.Parameters[2].Name);
        Assert.Equal("float", drawLineExMethod.Parameters[2].Type);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibCircleDrawingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibShapesAPI = bundle.LookupClass("raylib.Shapes");
        Assert.NotNull(raylibShapesAPI);
        
        // Circle drawing - drawCircle
        var drawCircleMethod = bundle.LookupMethod("raylib.Shapes", "drawCircle");
        Assert.NotNull(drawCircleMethod);
        Assert.Equal("void", drawCircleMethod.ReturnType);
        Assert.Equal(4, drawCircleMethod.Parameters.Count);
        Assert.Equal("centerX", drawCircleMethod.Parameters[0].Name);
        Assert.Equal("int", drawCircleMethod.Parameters[0].Type);
        Assert.Equal("centerY", drawCircleMethod.Parameters[1].Name);
        Assert.Equal("int", drawCircleMethod.Parameters[1].Type);
        Assert.Equal("radius", drawCircleMethod.Parameters[2].Name);
        Assert.Equal("float", drawCircleMethod.Parameters[2].Type);
        Assert.Equal("color", drawCircleMethod.Parameters[3].Name);
        Assert.Equal("raylib.Color", drawCircleMethod.Parameters[3].Type);
        Assert.True(drawCircleMethod.IsStatic);
        
        // Circle drawing - drawCircleV
        var drawCircleVMethod = bundle.LookupMethod("raylib.Shapes", "drawCircleV");
        Assert.NotNull(drawCircleVMethod);
        Assert.Equal("void", drawCircleVMethod.ReturnType);
        Assert.Equal(3, drawCircleVMethod.Parameters.Count);
        Assert.Equal("center", drawCircleVMethod.Parameters[0].Name);
        Assert.Equal("raylib.Vector2", drawCircleVMethod.Parameters[0].Type);
        
        // Circle drawing - drawCircleLines
        var drawCircleLinesMethod = bundle.LookupMethod("raylib.Shapes", "drawCircleLines");
        Assert.NotNull(drawCircleLinesMethod);
        Assert.Equal("void", drawCircleLinesMethod.ReturnType);
        Assert.Equal(4, drawCircleLinesMethod.Parameters.Count);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibRectangleDrawingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibShapesAPI = bundle.LookupClass("raylib.Shapes");
        Assert.NotNull(raylibShapesAPI);
        
        // Rectangle drawing - drawRectangle
        var drawRectangleMethod = bundle.LookupMethod("raylib.Shapes", "drawRectangle");
        Assert.NotNull(drawRectangleMethod);
        Assert.Equal("void", drawRectangleMethod.ReturnType);
        Assert.Equal(5, drawRectangleMethod.Parameters.Count);
        Assert.Equal("posX", drawRectangleMethod.Parameters[0].Name);
        Assert.Equal("int", drawRectangleMethod.Parameters[0].Type);
        Assert.Equal("posY", drawRectangleMethod.Parameters[1].Name);
        Assert.Equal("int", drawRectangleMethod.Parameters[1].Type);
        Assert.Equal("width", drawRectangleMethod.Parameters[2].Name);
        Assert.Equal("int", drawRectangleMethod.Parameters[2].Type);
        Assert.Equal("height", drawRectangleMethod.Parameters[3].Name);
        Assert.Equal("int", drawRectangleMethod.Parameters[3].Type);
        Assert.Equal("color", drawRectangleMethod.Parameters[4].Name);
        Assert.Equal("raylib.Color", drawRectangleMethod.Parameters[4].Type);
        Assert.True(drawRectangleMethod.IsStatic);
        
        // Rectangle drawing - drawRectangleV
        var drawRectangleVMethod = bundle.LookupMethod("raylib.Shapes", "drawRectangleV");
        Assert.NotNull(drawRectangleVMethod);
        Assert.Equal("void", drawRectangleVMethod.ReturnType);
        Assert.Equal(3, drawRectangleVMethod.Parameters.Count);
        Assert.Equal("position", drawRectangleVMethod.Parameters[0].Name);
        Assert.Equal("raylib.Vector2", drawRectangleVMethod.Parameters[0].Type);
        Assert.Equal("size", drawRectangleVMethod.Parameters[1].Name);
        Assert.Equal("raylib.Vector2", drawRectangleVMethod.Parameters[1].Type);
        
        // Rectangle drawing - drawRectangleRec
        var drawRectangleRecMethod = bundle.LookupMethod("raylib.Shapes", "drawRectangleRec");
        Assert.NotNull(drawRectangleRecMethod);
        Assert.Equal("void", drawRectangleRecMethod.ReturnType);
        Assert.Equal(2, drawRectangleRecMethod.Parameters.Count);
        Assert.Equal("rec", drawRectangleRecMethod.Parameters[0].Name);
        Assert.Equal("raylib.Rectangle", drawRectangleRecMethod.Parameters[0].Type);
        
        // Rectangle drawing - drawRectangleLines
        var drawRectangleLinesMethod = bundle.LookupMethod("raylib.Shapes", "drawRectangleLines");
        Assert.NotNull(drawRectangleLinesMethod);
        Assert.Equal("void", drawRectangleLinesMethod.ReturnType);
        Assert.Equal(5, drawRectangleLinesMethod.Parameters.Count);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibTriangleDrawingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibShapesAPI = bundle.LookupClass("raylib.Shapes");
        Assert.NotNull(raylibShapesAPI);
        
        // Triangle drawing - drawTriangle
        var drawTriangleMethod = bundle.LookupMethod("raylib.Shapes", "drawTriangle");
        Assert.NotNull(drawTriangleMethod);
        Assert.Equal("void", drawTriangleMethod.ReturnType);
        Assert.Equal(4, drawTriangleMethod.Parameters.Count);
        Assert.Equal("v1", drawTriangleMethod.Parameters[0].Name);
        Assert.Equal("raylib.Vector2", drawTriangleMethod.Parameters[0].Type);
        Assert.Equal("v2", drawTriangleMethod.Parameters[1].Name);
        Assert.Equal("raylib.Vector2", drawTriangleMethod.Parameters[1].Type);
        Assert.Equal("v3", drawTriangleMethod.Parameters[2].Name);
        Assert.Equal("raylib.Vector2", drawTriangleMethod.Parameters[2].Type);
        Assert.Equal("color", drawTriangleMethod.Parameters[3].Name);
        Assert.Equal("raylib.Color", drawTriangleMethod.Parameters[3].Type);
        Assert.True(drawTriangleMethod.IsStatic);
        
        // Triangle drawing - drawTriangleLines
        var drawTriangleLinesMethod = bundle.LookupMethod("raylib.Shapes", "drawTriangleLines");
        Assert.NotNull(drawTriangleLinesMethod);
        Assert.Equal("void", drawTriangleLinesMethod.ReturnType);
        Assert.Equal(4, drawTriangleLinesMethod.Parameters.Count);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibTextureLoadingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibTexturesAPI = bundle.LookupClass("raylib.Textures");
        Assert.NotNull(raylibTexturesAPI);
        
        // Texture loading - loadTexture
        var loadTextureMethod = bundle.LookupMethod("raylib.Textures", "loadTexture");
        Assert.NotNull(loadTextureMethod);
        Assert.Equal("raylib.Texture2D", loadTextureMethod.ReturnType);
        Assert.Single(loadTextureMethod.Parameters);
        Assert.Equal("fileName", loadTextureMethod.Parameters[0].Name);
        Assert.Equal("String", loadTextureMethod.Parameters[0].Type);
        Assert.True(loadTextureMethod.IsStatic);
        
        // Texture loading - loadTextureFromImage
        var loadTextureFromImageMethod = bundle.LookupMethod("raylib.Textures", "loadTextureFromImage");
        Assert.NotNull(loadTextureFromImageMethod);
        Assert.Equal("raylib.Texture2D", loadTextureFromImageMethod.ReturnType);
        Assert.Single(loadTextureFromImageMethod.Parameters);
        Assert.Equal("image", loadTextureFromImageMethod.Parameters[0].Name);
        Assert.Equal("raylib.Image", loadTextureFromImageMethod.Parameters[0].Type);
        
        // Texture unloading - unloadTexture
        var unloadTextureMethod = bundle.LookupMethod("raylib.Textures", "unloadTexture");
        Assert.NotNull(unloadTextureMethod);
        Assert.Equal("void", unloadTextureMethod.ReturnType);
        Assert.Single(unloadTextureMethod.Parameters);
        Assert.Equal("texture", unloadTextureMethod.Parameters[0].Name);
        Assert.Equal("raylib.Texture2D", unloadTextureMethod.Parameters[0].Type);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibTextureDrawingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibTexturesAPI = bundle.LookupClass("raylib.Textures");
        Assert.NotNull(raylibTexturesAPI);
        
        // Texture drawing - drawTexture
        var drawTextureMethod = bundle.LookupMethod("raylib.Textures", "drawTexture");
        Assert.NotNull(drawTextureMethod);
        Assert.Equal("void", drawTextureMethod.ReturnType);
        Assert.Equal(4, drawTextureMethod.Parameters.Count);
        Assert.Equal("texture", drawTextureMethod.Parameters[0].Name);
        Assert.Equal("raylib.Texture2D", drawTextureMethod.Parameters[0].Type);
        Assert.Equal("posX", drawTextureMethod.Parameters[1].Name);
        Assert.Equal("int", drawTextureMethod.Parameters[1].Type);
        Assert.Equal("posY", drawTextureMethod.Parameters[2].Name);
        Assert.Equal("int", drawTextureMethod.Parameters[2].Type);
        Assert.Equal("tint", drawTextureMethod.Parameters[3].Name);
        Assert.Equal("raylib.Color", drawTextureMethod.Parameters[3].Type);
        Assert.True(drawTextureMethod.IsStatic);
        
        // Texture drawing - drawTextureV
        var drawTextureVMethod = bundle.LookupMethod("raylib.Textures", "drawTextureV");
        Assert.NotNull(drawTextureVMethod);
        Assert.Equal("void", drawTextureVMethod.ReturnType);
        Assert.Equal(3, drawTextureVMethod.Parameters.Count);
        
        // Texture drawing - drawTextureEx
        var drawTextureExMethod = bundle.LookupMethod("raylib.Textures", "drawTextureEx");
        Assert.NotNull(drawTextureExMethod);
        Assert.Equal("void", drawTextureExMethod.ReturnType);
        Assert.Equal(5, drawTextureExMethod.Parameters.Count);
        Assert.Equal("texture", drawTextureExMethod.Parameters[0].Name);
        Assert.Equal("raylib.Texture2D", drawTextureExMethod.Parameters[0].Type);
        Assert.Equal("position", drawTextureExMethod.Parameters[1].Name);
        Assert.Equal("raylib.Vector2", drawTextureExMethod.Parameters[1].Type);
        Assert.Equal("rotation", drawTextureExMethod.Parameters[2].Name);
        Assert.Equal("float", drawTextureExMethod.Parameters[2].Type);
        Assert.Equal("scale", drawTextureExMethod.Parameters[3].Name);
        Assert.Equal("float", drawTextureExMethod.Parameters[3].Type);
        Assert.Equal("tint", drawTextureExMethod.Parameters[4].Name);
        Assert.Equal("raylib.Color", drawTextureExMethod.Parameters[4].Type);
        
        // Texture drawing - drawTextureRec
        var drawTextureRecMethod = bundle.LookupMethod("raylib.Textures", "drawTextureRec");
        Assert.NotNull(drawTextureRecMethod);
        Assert.Equal("void", drawTextureRecMethod.ReturnType);
        Assert.Equal(4, drawTextureRecMethod.Parameters.Count);
        Assert.Equal("source", drawTextureRecMethod.Parameters[1].Name);
        Assert.Equal("raylib.Rectangle", drawTextureRecMethod.Parameters[1].Type);
        
        // Texture drawing - drawTexturePro
        var drawTextureProMethod = bundle.LookupMethod("raylib.Textures", "drawTexturePro");
        Assert.NotNull(drawTextureProMethod);
        Assert.Equal("void", drawTextureProMethod.ReturnType);
        Assert.Equal(6, drawTextureProMethod.Parameters.Count);
        Assert.Equal("texture", drawTextureProMethod.Parameters[0].Name);
        Assert.Equal("raylib.Texture2D", drawTextureProMethod.Parameters[0].Type);
        Assert.Equal("source", drawTextureProMethod.Parameters[1].Name);
        Assert.Equal("raylib.Rectangle", drawTextureProMethod.Parameters[1].Type);
        Assert.Equal("dest", drawTextureProMethod.Parameters[2].Name);
        Assert.Equal("raylib.Rectangle", drawTextureProMethod.Parameters[2].Type);
        Assert.Equal("origin", drawTextureProMethod.Parameters[3].Name);
        Assert.Equal("raylib.Vector2", drawTextureProMethod.Parameters[3].Type);
        Assert.Equal("rotation", drawTextureProMethod.Parameters[4].Name);
        Assert.Equal("float", drawTextureProMethod.Parameters[4].Type);
        Assert.Equal("tint", drawTextureProMethod.Parameters[5].Name);
        Assert.Equal("raylib.Color", drawTextureProMethod.Parameters[5].Type);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibFontLoadingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibTextAPI = bundle.LookupClass("raylib.Text");
        Assert.NotNull(raylibTextAPI);
        
        // Font loading - getFontDefault
        var getFontDefaultMethod = bundle.LookupMethod("raylib.Text", "getFontDefault");
        Assert.NotNull(getFontDefaultMethod);
        Assert.Equal("raylib.Font", getFontDefaultMethod.ReturnType);
        Assert.Empty(getFontDefaultMethod.Parameters);
        Assert.True(getFontDefaultMethod.IsStatic);
        
        // Font loading - loadFont
        var loadFontMethod = bundle.LookupMethod("raylib.Text", "loadFont");
        Assert.NotNull(loadFontMethod);
        Assert.Equal("raylib.Font", loadFontMethod.ReturnType);
        Assert.Single(loadFontMethod.Parameters);
        Assert.Equal("fileName", loadFontMethod.Parameters[0].Name);
        Assert.Equal("String", loadFontMethod.Parameters[0].Type);
        Assert.True(loadFontMethod.IsStatic);
        
        // Font unloading - unloadFont
        var unloadFontMethod = bundle.LookupMethod("raylib.Text", "unloadFont");
        Assert.NotNull(unloadFontMethod);
        Assert.Equal("void", unloadFontMethod.ReturnType);
        Assert.Single(unloadFontMethod.Parameters);
        Assert.Equal("font", unloadFontMethod.Parameters[0].Name);
        Assert.Equal("raylib.Font", unloadFontMethod.Parameters[0].Type);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibTextDrawingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibTextAPI = bundle.LookupClass("raylib.Text");
        Assert.NotNull(raylibTextAPI);
        
        // Text drawing - drawText
        var drawTextMethod = bundle.LookupMethod("raylib.Text", "drawText");
        Assert.NotNull(drawTextMethod);
        Assert.Equal("void", drawTextMethod.ReturnType);
        Assert.Equal(5, drawTextMethod.Parameters.Count);
        Assert.Equal("text", drawTextMethod.Parameters[0].Name);
        Assert.Equal("String", drawTextMethod.Parameters[0].Type);
        Assert.Equal("posX", drawTextMethod.Parameters[1].Name);
        Assert.Equal("int", drawTextMethod.Parameters[1].Type);
        Assert.Equal("posY", drawTextMethod.Parameters[2].Name);
        Assert.Equal("int", drawTextMethod.Parameters[2].Type);
        Assert.Equal("fontSize", drawTextMethod.Parameters[3].Name);
        Assert.Equal("int", drawTextMethod.Parameters[3].Type);
        Assert.Equal("color", drawTextMethod.Parameters[4].Name);
        Assert.Equal("raylib.Color", drawTextMethod.Parameters[4].Type);
        Assert.True(drawTextMethod.IsStatic);
        
        // Text drawing - drawTextEx
        var drawTextExMethod = bundle.LookupMethod("raylib.Text", "drawTextEx");
        Assert.NotNull(drawTextExMethod);
        Assert.Equal("void", drawTextExMethod.ReturnType);
        Assert.Equal(6, drawTextExMethod.Parameters.Count);
        Assert.Equal("font", drawTextExMethod.Parameters[0].Name);
        Assert.Equal("raylib.Font", drawTextExMethod.Parameters[0].Type);
        Assert.Equal("text", drawTextExMethod.Parameters[1].Name);
        Assert.Equal("String", drawTextExMethod.Parameters[1].Type);
        Assert.Equal("position", drawTextExMethod.Parameters[2].Name);
        Assert.Equal("raylib.Vector2", drawTextExMethod.Parameters[2].Type);
        Assert.Equal("fontSize", drawTextExMethod.Parameters[3].Name);
        Assert.Equal("float", drawTextExMethod.Parameters[3].Type);
        Assert.Equal("spacing", drawTextExMethod.Parameters[4].Name);
        Assert.Equal("float", drawTextExMethod.Parameters[4].Type);
        Assert.Equal("tint", drawTextExMethod.Parameters[5].Name);
        Assert.Equal("raylib.Color", drawTextExMethod.Parameters[5].Type);
        
        // Text drawing - drawTextPro
        var drawTextProMethod = bundle.LookupMethod("raylib.Text", "drawTextPro");
        Assert.NotNull(drawTextProMethod);
        Assert.Equal("void", drawTextProMethod.ReturnType);
        Assert.Equal(8, drawTextProMethod.Parameters.Count);
        Assert.Equal("font", drawTextProMethod.Parameters[0].Name);
        Assert.Equal("raylib.Font", drawTextProMethod.Parameters[0].Type);
        Assert.Equal("origin", drawTextProMethod.Parameters[3].Name);
        Assert.Equal("raylib.Vector2", drawTextProMethod.Parameters[3].Type);
        Assert.Equal("rotation", drawTextProMethod.Parameters[4].Name);
        Assert.Equal("float", drawTextProMethod.Parameters[4].Type);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibTextMeasurementAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibTextAPI = bundle.LookupClass("raylib.Text");
        Assert.NotNull(raylibTextAPI);
        
        // Text measurement - measureText
        var measureTextMethod = bundle.LookupMethod("raylib.Text", "measureText");
        Assert.NotNull(measureTextMethod);
        Assert.Equal("int", measureTextMethod.ReturnType);
        Assert.Equal(2, measureTextMethod.Parameters.Count);
        Assert.Equal("text", measureTextMethod.Parameters[0].Name);
        Assert.Equal("String", measureTextMethod.Parameters[0].Type);
        Assert.Equal("fontSize", measureTextMethod.Parameters[1].Name);
        Assert.Equal("int", measureTextMethod.Parameters[1].Type);
        Assert.True(measureTextMethod.IsStatic);
        
        // Text measurement - measureTextEx
        var measureTextExMethod = bundle.LookupMethod("raylib.Text", "measureTextEx");
        Assert.NotNull(measureTextExMethod);
        Assert.Equal("raylib.Vector2", measureTextExMethod.ReturnType);
        Assert.Equal(4, measureTextExMethod.Parameters.Count);
        Assert.Equal("font", measureTextExMethod.Parameters[0].Name);
        Assert.Equal("raylib.Font", measureTextExMethod.Parameters[0].Type);
        Assert.Equal("text", measureTextExMethod.Parameters[1].Name);
        Assert.Equal("String", measureTextExMethod.Parameters[1].Type);
        Assert.Equal("fontSize", measureTextExMethod.Parameters[2].Name);
        Assert.Equal("float", measureTextExMethod.Parameters[2].Type);
        Assert.Equal("spacing", measureTextExMethod.Parameters[3].Name);
        Assert.Equal("float", measureTextExMethod.Parameters[3].Type);
    }

    [Fact]
    public void APIRegistry_RaylibDrawingAPIsAvailableInAllSDKVersions()
    {
        var registry = new APIRegistry();
        
        // Raylib drawing APIs should be available in all SDK versions from 23 onwards
        foreach (var apiLevel in new[] { 23, 24, 26, 28, 30, 33, 35 })
        {
            var bundle = registry.GetSDKBundle(apiLevel);
            Assert.NotNull(bundle);
            
            var raylibDrawingAPI = bundle.LookupClass("raylib.Drawing");
            Assert.NotNull(raylibDrawingAPI);
            
            var raylibShapesAPI = bundle.LookupClass("raylib.Shapes");
            Assert.NotNull(raylibShapesAPI);
            
            var raylibTexturesAPI = bundle.LookupClass("raylib.Textures");
            Assert.NotNull(raylibTexturesAPI);
            
            var raylibTextAPI = bundle.LookupClass("raylib.Text");
            Assert.NotNull(raylibTextAPI);
            
            // Verify key functions exist
            var beginDrawingMethod = bundle.LookupMethod("raylib.Drawing", "beginDrawing");
            Assert.NotNull(beginDrawingMethod);
            
            var drawCircleMethod = bundle.LookupMethod("raylib.Shapes", "drawCircle");
            Assert.NotNull(drawCircleMethod);
            
            var loadTextureMethod = bundle.LookupMethod("raylib.Textures", "loadTexture");
            Assert.NotNull(loadTextureMethod);
            
            var drawTextMethod = bundle.LookupMethod("raylib.Text", "drawText");
            Assert.NotNull(drawTextMethod);
        }
    }
}
