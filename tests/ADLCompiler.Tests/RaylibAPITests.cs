using ADLCompiler.SemanticAnalysis;
using Xunit;

namespace ADLCompiler.Tests;

/// <summary>
/// Tests for raylib API bindings
/// Validates Requirement 14.9: Built-in access to all raylib functions
/// </summary>
public class RaylibAPITests
{
    [Fact]
    public void APIRegistry_LoadsRaylibCoreWindowAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibCoreAPI = bundle.LookupClass("raylib.Core");
        Assert.NotNull(raylibCoreAPI);
        
        // Window management - initWindow
        var initWindowMethod = bundle.LookupMethod("raylib.Core", "initWindow");
        Assert.NotNull(initWindowMethod);
        Assert.Equal("void", initWindowMethod.ReturnType);
        Assert.Equal(3, initWindowMethod.Parameters.Count);
        Assert.Equal("width", initWindowMethod.Parameters[0].Name);
        Assert.Equal("int", initWindowMethod.Parameters[0].Type);
        Assert.Equal("height", initWindowMethod.Parameters[1].Name);
        Assert.Equal("int", initWindowMethod.Parameters[1].Type);
        Assert.Equal("title", initWindowMethod.Parameters[2].Name);
        Assert.Equal("String", initWindowMethod.Parameters[2].Type);
        Assert.True(initWindowMethod.IsStatic);
        
        // Window management - closeWindow
        var closeWindowMethod = bundle.LookupMethod("raylib.Core", "closeWindow");
        Assert.NotNull(closeWindowMethod);
        Assert.Equal("void", closeWindowMethod.ReturnType);
        Assert.Empty(closeWindowMethod.Parameters);
        Assert.True(closeWindowMethod.IsStatic);
        
        // Window management - setTargetFPS
        var setTargetFPSMethod = bundle.LookupMethod("raylib.Core", "setTargetFPS");
        Assert.NotNull(setTargetFPSMethod);
        Assert.Equal("void", setTargetFPSMethod.ReturnType);
        Assert.Single(setTargetFPSMethod.Parameters);
        Assert.Equal("fps", setTargetFPSMethod.Parameters[0].Name);
        Assert.Equal("int", setTargetFPSMethod.Parameters[0].Type);
        Assert.True(setTargetFPSMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibWindowStateAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibCoreAPI = bundle.LookupClass("raylib.Core");
        Assert.NotNull(raylibCoreAPI);
        
        // Window state queries
        var windowShouldCloseMethod = bundle.LookupMethod("raylib.Core", "windowShouldClose");
        Assert.NotNull(windowShouldCloseMethod);
        Assert.Equal("boolean", windowShouldCloseMethod.ReturnType);
        Assert.Empty(windowShouldCloseMethod.Parameters);
        
        var isWindowReadyMethod = bundle.LookupMethod("raylib.Core", "isWindowReady");
        Assert.NotNull(isWindowReadyMethod);
        Assert.Equal("boolean", isWindowReadyMethod.ReturnType);
        
        var isWindowFullscreenMethod = bundle.LookupMethod("raylib.Core", "isWindowFullscreen");
        Assert.NotNull(isWindowFullscreenMethod);
        Assert.Equal("boolean", isWindowFullscreenMethod.ReturnType);
        
        var isWindowMinimizedMethod = bundle.LookupMethod("raylib.Core", "isWindowMinimized");
        Assert.NotNull(isWindowMinimizedMethod);
        Assert.Equal("boolean", isWindowMinimizedMethod.ReturnType);
        
        var isWindowMaximizedMethod = bundle.LookupMethod("raylib.Core", "isWindowMaximized");
        Assert.NotNull(isWindowMaximizedMethod);
        Assert.Equal("boolean", isWindowMaximizedMethod.ReturnType);
        
        var isWindowFocusedMethod = bundle.LookupMethod("raylib.Core", "isWindowFocused");
        Assert.NotNull(isWindowFocusedMethod);
        Assert.Equal("boolean", isWindowFocusedMethod.ReturnType);
        
        var isWindowResizedMethod = bundle.LookupMethod("raylib.Core", "isWindowResized");
        Assert.NotNull(isWindowResizedMethod);
        Assert.Equal("boolean", isWindowResizedMethod.ReturnType);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibWindowControlAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibCoreAPI = bundle.LookupClass("raylib.Core");
        Assert.NotNull(raylibCoreAPI);
        
        // Window control
        var toggleFullscreenMethod = bundle.LookupMethod("raylib.Core", "toggleFullscreen");
        Assert.NotNull(toggleFullscreenMethod);
        Assert.Equal("void", toggleFullscreenMethod.ReturnType);
        
        var maximizeWindowMethod = bundle.LookupMethod("raylib.Core", "maximizeWindow");
        Assert.NotNull(maximizeWindowMethod);
        Assert.Equal("void", maximizeWindowMethod.ReturnType);
        
        var minimizeWindowMethod = bundle.LookupMethod("raylib.Core", "minimizeWindow");
        Assert.NotNull(minimizeWindowMethod);
        Assert.Equal("void", minimizeWindowMethod.ReturnType);
        
        var restoreWindowMethod = bundle.LookupMethod("raylib.Core", "restoreWindow");
        Assert.NotNull(restoreWindowMethod);
        Assert.Equal("void", restoreWindowMethod.ReturnType);
        
        // Window properties
        var setWindowTitleMethod = bundle.LookupMethod("raylib.Core", "setWindowTitle");
        Assert.NotNull(setWindowTitleMethod);
        Assert.Equal("void", setWindowTitleMethod.ReturnType);
        Assert.Single(setWindowTitleMethod.Parameters);
        Assert.Equal("title", setWindowTitleMethod.Parameters[0].Name);
        Assert.Equal("String", setWindowTitleMethod.Parameters[0].Type);
        
        var setWindowPositionMethod = bundle.LookupMethod("raylib.Core", "setWindowPosition");
        Assert.NotNull(setWindowPositionMethod);
        Assert.Equal("void", setWindowPositionMethod.ReturnType);
        Assert.Equal(2, setWindowPositionMethod.Parameters.Count);
        Assert.Equal("x", setWindowPositionMethod.Parameters[0].Name);
        Assert.Equal("int", setWindowPositionMethod.Parameters[0].Type);
        Assert.Equal("y", setWindowPositionMethod.Parameters[1].Name);
        Assert.Equal("int", setWindowPositionMethod.Parameters[1].Type);
        
        var setWindowSizeMethod = bundle.LookupMethod("raylib.Core", "setWindowSize");
        Assert.NotNull(setWindowSizeMethod);
        Assert.Equal("void", setWindowSizeMethod.ReturnType);
        Assert.Equal(2, setWindowSizeMethod.Parameters.Count);
        Assert.Equal("width", setWindowSizeMethod.Parameters[0].Name);
        Assert.Equal("height", setWindowSizeMethod.Parameters[1].Name);
        
        // Screen dimensions
        var getScreenWidthMethod = bundle.LookupMethod("raylib.Core", "getScreenWidth");
        Assert.NotNull(getScreenWidthMethod);
        Assert.Equal("int", getScreenWidthMethod.ReturnType);
        Assert.Empty(getScreenWidthMethod.Parameters);
        
        var getScreenHeightMethod = bundle.LookupMethod("raylib.Core", "getScreenHeight");
        Assert.NotNull(getScreenHeightMethod);
        Assert.Equal("int", getScreenHeightMethod.ReturnType);
        Assert.Empty(getScreenHeightMethod.Parameters);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibTimingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibTimingAPI = bundle.LookupClass("raylib.Timing");
        Assert.NotNull(raylibTimingAPI);
        
        // Timing functions - getFrameTime
        var getFrameTimeMethod = bundle.LookupMethod("raylib.Timing", "getFrameTime");
        Assert.NotNull(getFrameTimeMethod);
        Assert.Equal("float", getFrameTimeMethod.ReturnType);
        Assert.Empty(getFrameTimeMethod.Parameters);
        Assert.True(getFrameTimeMethod.IsStatic);
        
        // Timing functions - getTime
        var getTimeMethod = bundle.LookupMethod("raylib.Timing", "getTime");
        Assert.NotNull(getTimeMethod);
        Assert.Equal("double", getTimeMethod.ReturnType);
        Assert.Empty(getTimeMethod.Parameters);
        Assert.True(getTimeMethod.IsStatic);
        
        // Timing functions - getFPS
        var getFPSMethod = bundle.LookupMethod("raylib.Timing", "getFPS");
        Assert.NotNull(getFPSMethod);
        Assert.Equal("int", getFPSMethod.ReturnType);
        Assert.Empty(getFPSMethod.Parameters);
        Assert.True(getFPSMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibKeyboardInputAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibInputAPI = bundle.LookupClass("raylib.Input");
        Assert.NotNull(raylibInputAPI);
        
        // Keyboard input - isKeyPressed
        var isKeyPressedMethod = bundle.LookupMethod("raylib.Input", "isKeyPressed");
        Assert.NotNull(isKeyPressedMethod);
        Assert.Equal("boolean", isKeyPressedMethod.ReturnType);
        Assert.Single(isKeyPressedMethod.Parameters);
        Assert.Equal("key", isKeyPressedMethod.Parameters[0].Name);
        Assert.Equal("int", isKeyPressedMethod.Parameters[0].Type);
        Assert.True(isKeyPressedMethod.IsStatic);
        
        // Keyboard input - isKeyDown
        var isKeyDownMethod = bundle.LookupMethod("raylib.Input", "isKeyDown");
        Assert.NotNull(isKeyDownMethod);
        Assert.Equal("boolean", isKeyDownMethod.ReturnType);
        Assert.Single(isKeyDownMethod.Parameters);
        Assert.Equal("key", isKeyDownMethod.Parameters[0].Name);
        Assert.Equal("int", isKeyDownMethod.Parameters[0].Type);
        Assert.True(isKeyDownMethod.IsStatic);
        
        // Keyboard input - isKeyReleased
        var isKeyReleasedMethod = bundle.LookupMethod("raylib.Input", "isKeyReleased");
        Assert.NotNull(isKeyReleasedMethod);
        Assert.Equal("boolean", isKeyReleasedMethod.ReturnType);
        Assert.Single(isKeyReleasedMethod.Parameters);
        
        // Keyboard input - isKeyUp
        var isKeyUpMethod = bundle.LookupMethod("raylib.Input", "isKeyUp");
        Assert.NotNull(isKeyUpMethod);
        Assert.Equal("boolean", isKeyUpMethod.ReturnType);
        Assert.Single(isKeyUpMethod.Parameters);
        
        // Keyboard input - getKeyPressed
        var getKeyPressedMethod = bundle.LookupMethod("raylib.Input", "getKeyPressed");
        Assert.NotNull(getKeyPressedMethod);
        Assert.Equal("int", getKeyPressedMethod.ReturnType);
        Assert.Empty(getKeyPressedMethod.Parameters);
        
        // Keyboard input - getCharPressed
        var getCharPressedMethod = bundle.LookupMethod("raylib.Input", "getCharPressed");
        Assert.NotNull(getCharPressedMethod);
        Assert.Equal("int", getCharPressedMethod.ReturnType);
        Assert.Empty(getCharPressedMethod.Parameters);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMouseInputAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibInputAPI = bundle.LookupClass("raylib.Input");
        Assert.NotNull(raylibInputAPI);
        
        // Mouse button input - isMouseButtonPressed
        var isMouseButtonPressedMethod = bundle.LookupMethod("raylib.Input", "isMouseButtonPressed");
        Assert.NotNull(isMouseButtonPressedMethod);
        Assert.Equal("boolean", isMouseButtonPressedMethod.ReturnType);
        Assert.Single(isMouseButtonPressedMethod.Parameters);
        Assert.Equal("button", isMouseButtonPressedMethod.Parameters[0].Name);
        Assert.Equal("int", isMouseButtonPressedMethod.Parameters[0].Type);
        Assert.True(isMouseButtonPressedMethod.IsStatic);
        
        // Mouse button input - isMouseButtonDown
        var isMouseButtonDownMethod = bundle.LookupMethod("raylib.Input", "isMouseButtonDown");
        Assert.NotNull(isMouseButtonDownMethod);
        Assert.Equal("boolean", isMouseButtonDownMethod.ReturnType);
        Assert.Single(isMouseButtonDownMethod.Parameters);
        
        // Mouse button input - isMouseButtonReleased
        var isMouseButtonReleasedMethod = bundle.LookupMethod("raylib.Input", "isMouseButtonReleased");
        Assert.NotNull(isMouseButtonReleasedMethod);
        Assert.Equal("boolean", isMouseButtonReleasedMethod.ReturnType);
        
        // Mouse button input - isMouseButtonUp
        var isMouseButtonUpMethod = bundle.LookupMethod("raylib.Input", "isMouseButtonUp");
        Assert.NotNull(isMouseButtonUpMethod);
        Assert.Equal("boolean", isMouseButtonUpMethod.ReturnType);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMousePositionAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibInputAPI = bundle.LookupClass("raylib.Input");
        Assert.NotNull(raylibInputAPI);
        
        // Mouse position - getMouseX
        var getMouseXMethod = bundle.LookupMethod("raylib.Input", "getMouseX");
        Assert.NotNull(getMouseXMethod);
        Assert.Equal("int", getMouseXMethod.ReturnType);
        Assert.Empty(getMouseXMethod.Parameters);
        Assert.True(getMouseXMethod.IsStatic);
        
        // Mouse position - getMouseY
        var getMouseYMethod = bundle.LookupMethod("raylib.Input", "getMouseY");
        Assert.NotNull(getMouseYMethod);
        Assert.Equal("int", getMouseYMethod.ReturnType);
        Assert.Empty(getMouseYMethod.Parameters);
        Assert.True(getMouseYMethod.IsStatic);
        
        // Mouse position - getMousePosition
        var getMousePositionMethod = bundle.LookupMethod("raylib.Input", "getMousePosition");
        Assert.NotNull(getMousePositionMethod);
        Assert.Equal("raylib.Vector2", getMousePositionMethod.ReturnType);
        Assert.Empty(getMousePositionMethod.Parameters);
        Assert.True(getMousePositionMethod.IsStatic);
        
        // Mouse delta - getMouseDelta
        var getMouseDeltaMethod = bundle.LookupMethod("raylib.Input", "getMouseDelta");
        Assert.NotNull(getMouseDeltaMethod);
        Assert.Equal("raylib.Vector2", getMouseDeltaMethod.ReturnType);
        Assert.Empty(getMouseDeltaMethod.Parameters);
        
        // Mouse position control
        var setMousePositionMethod = bundle.LookupMethod("raylib.Input", "setMousePosition");
        Assert.NotNull(setMousePositionMethod);
        Assert.Equal("void", setMousePositionMethod.ReturnType);
        Assert.Equal(2, setMousePositionMethod.Parameters.Count);
        Assert.Equal("x", setMousePositionMethod.Parameters[0].Name);
        Assert.Equal("int", setMousePositionMethod.Parameters[0].Type);
        Assert.Equal("y", setMousePositionMethod.Parameters[1].Name);
        Assert.Equal("int", setMousePositionMethod.Parameters[1].Type);
        
        var setMouseOffsetMethod = bundle.LookupMethod("raylib.Input", "setMouseOffset");
        Assert.NotNull(setMouseOffsetMethod);
        Assert.Equal(2, setMouseOffsetMethod.Parameters.Count);
        
        var setMouseScaleMethod = bundle.LookupMethod("raylib.Input", "setMouseScale");
        Assert.NotNull(setMouseScaleMethod);
        Assert.Equal(2, setMouseScaleMethod.Parameters.Count);
        Assert.Equal("scaleX", setMouseScaleMethod.Parameters[0].Name);
        Assert.Equal("float", setMouseScaleMethod.Parameters[0].Type);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMouseWheelAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibInputAPI = bundle.LookupClass("raylib.Input");
        Assert.NotNull(raylibInputAPI);
        
        // Mouse wheel
        var getMouseWheelMoveMethod = bundle.LookupMethod("raylib.Input", "getMouseWheelMove");
        Assert.NotNull(getMouseWheelMoveMethod);
        Assert.Equal("float", getMouseWheelMoveMethod.ReturnType);
        Assert.Empty(getMouseWheelMoveMethod.Parameters);
        
        var getMouseWheelMoveVMethod = bundle.LookupMethod("raylib.Input", "getMouseWheelMoveV");
        Assert.NotNull(getMouseWheelMoveVMethod);
        Assert.Equal("raylib.Vector2", getMouseWheelMoveVMethod.ReturnType);
        Assert.Empty(getMouseWheelMoveVMethod.Parameters);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMouseCursorAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibInputAPI = bundle.LookupClass("raylib.Input");
        Assert.NotNull(raylibInputAPI);
        
        // Mouse cursor control
        var showCursorMethod = bundle.LookupMethod("raylib.Input", "showCursor");
        Assert.NotNull(showCursorMethod);
        Assert.Equal("void", showCursorMethod.ReturnType);
        Assert.Empty(showCursorMethod.Parameters);
        
        var hideCursorMethod = bundle.LookupMethod("raylib.Input", "hideCursor");
        Assert.NotNull(hideCursorMethod);
        Assert.Equal("void", hideCursorMethod.ReturnType);
        
        var isCursorHiddenMethod = bundle.LookupMethod("raylib.Input", "isCursorHidden");
        Assert.NotNull(isCursorHiddenMethod);
        Assert.Equal("boolean", isCursorHiddenMethod.ReturnType);
        
        var enableCursorMethod = bundle.LookupMethod("raylib.Input", "enableCursor");
        Assert.NotNull(enableCursorMethod);
        Assert.Equal("void", enableCursorMethod.ReturnType);
        
        var disableCursorMethod = bundle.LookupMethod("raylib.Input", "disableCursor");
        Assert.NotNull(disableCursorMethod);
        Assert.Equal("void", disableCursorMethod.ReturnType);
        
        var isCursorOnScreenMethod = bundle.LookupMethod("raylib.Input", "isCursorOnScreen");
        Assert.NotNull(isCursorOnScreenMethod);
        Assert.Equal("boolean", isCursorOnScreenMethod.ReturnType);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibKeyConstants()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibInputAPI = bundle.LookupClass("raylib.Input");
        Assert.NotNull(raylibInputAPI);
        
        // Key constants
        var keyNullField = bundle.LookupField("raylib.Input", "KEY_NULL");
        Assert.NotNull(keyNullField);
        Assert.Equal("int", keyNullField.Type);
        Assert.True(keyNullField.IsStatic);
        
        var keySpaceField = bundle.LookupField("raylib.Input", "KEY_SPACE");
        Assert.NotNull(keySpaceField);
        Assert.True(keySpaceField.IsStatic);
        
        var keyEnterField = bundle.LookupField("raylib.Input", "KEY_ENTER");
        Assert.NotNull(keyEnterField);
        
        var keyEscapeField = bundle.LookupField("raylib.Input", "KEY_ESCAPE");
        Assert.NotNull(keyEscapeField);
        
        var keyRightField = bundle.LookupField("raylib.Input", "KEY_RIGHT");
        Assert.NotNull(keyRightField);
        
        var keyLeftField = bundle.LookupField("raylib.Input", "KEY_LEFT");
        Assert.NotNull(keyLeftField);
        
        var keyUpField = bundle.LookupField("raylib.Input", "KEY_UP");
        Assert.NotNull(keyUpField);
        
        var keyDownField = bundle.LookupField("raylib.Input", "KEY_DOWN");
        Assert.NotNull(keyDownField);
        
        // Letter keys
        var keyAField = bundle.LookupField("raylib.Input", "KEY_A");
        Assert.NotNull(keyAField);
        
        var keyWField = bundle.LookupField("raylib.Input", "KEY_W");
        Assert.NotNull(keyWField);
        
        var keySField = bundle.LookupField("raylib.Input", "KEY_S");
        Assert.NotNull(keySField);
        
        var keyDField = bundle.LookupField("raylib.Input", "KEY_D");
        Assert.NotNull(keyDField);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMouseButtonConstants()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibInputAPI = bundle.LookupClass("raylib.Input");
        Assert.NotNull(raylibInputAPI);
        
        // Mouse button constants
        var mouseLeftField = bundle.LookupField("raylib.Input", "MOUSE_BUTTON_LEFT");
        Assert.NotNull(mouseLeftField);
        Assert.Equal("int", mouseLeftField.Type);
        Assert.True(mouseLeftField.IsStatic);
        
        var mouseRightField = bundle.LookupField("raylib.Input", "MOUSE_BUTTON_RIGHT");
        Assert.NotNull(mouseRightField);
        Assert.True(mouseRightField.IsStatic);
        
        var mouseMiddleField = bundle.LookupField("raylib.Input", "MOUSE_BUTTON_MIDDLE");
        Assert.NotNull(mouseMiddleField);
        
        var mouseSideField = bundle.LookupField("raylib.Input", "MOUSE_BUTTON_SIDE");
        Assert.NotNull(mouseSideField);
        
        var mouseExtraField = bundle.LookupField("raylib.Input", "MOUSE_BUTTON_EXTRA");
        Assert.NotNull(mouseExtraField);
    }

    [Fact]
    public void APIRegistry_RaylibAPIsAvailableInAllSDKVersions()
    {
        var registry = new APIRegistry();
        
        // Raylib should be available in all SDK versions from 23 onwards
        foreach (var apiLevel in new[] { 23, 24, 26, 28, 30, 33, 35 })
        {
            var bundle = registry.GetSDKBundle(apiLevel);
            Assert.NotNull(bundle);
            
            var raylibCoreAPI = bundle.LookupClass("raylib.Core");
            Assert.NotNull(raylibCoreAPI);
            
            var raylibTimingAPI = bundle.LookupClass("raylib.Timing");
            Assert.NotNull(raylibTimingAPI);
            
            var raylibInputAPI = bundle.LookupClass("raylib.Input");
            Assert.NotNull(raylibInputAPI);
            
            // Verify key functions exist
            var initWindowMethod = bundle.LookupMethod("raylib.Core", "initWindow");
            Assert.NotNull(initWindowMethod);
            
            var getFrameTimeMethod = bundle.LookupMethod("raylib.Timing", "getFrameTime");
            Assert.NotNull(getFrameTimeMethod);
            
            var isKeyPressedMethod = bundle.LookupMethod("raylib.Input", "isKeyPressed");
            Assert.NotNull(isKeyPressedMethod);
        }
    }

    [Fact]
    public void APIRegistry_RaylibMethodsAreStatic()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        // All raylib functions should be static (global functions)
        var initWindowMethod = bundle.LookupMethod("raylib.Core", "initWindow");
        Assert.NotNull(initWindowMethod);
        Assert.True(initWindowMethod.IsStatic);
        
        var getFrameTimeMethod = bundle.LookupMethod("raylib.Timing", "getFrameTime");
        Assert.NotNull(getFrameTimeMethod);
        Assert.True(getFrameTimeMethod.IsStatic);
        
        var isKeyPressedMethod = bundle.LookupMethod("raylib.Input", "isKeyPressed");
        Assert.NotNull(isKeyPressedMethod);
        Assert.True(isKeyPressedMethod.IsStatic);
        
        var getMousePositionMethod = bundle.LookupMethod("raylib.Input", "getMousePosition");
        Assert.NotNull(getMousePositionMethod);
        Assert.True(getMousePositionMethod.IsStatic);
    }
}
