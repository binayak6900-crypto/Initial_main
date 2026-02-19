namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Loads raylib API bindings for graphics, audio, input, textures, models, and utilities
/// Requirement 14.9: Built-in access to all raylib functions
/// </summary>
public static class RaylibAPILoader
{
    /// <summary>
    /// Load all raylib API bindings into the SDK bundle
    /// </summary>
    public static void LoadRaylibAPIs(SDKBundle bundle, int apiLevel)
    {
        LoadCoreWindowFunctions(bundle, apiLevel);
        LoadTimingFunctions(bundle, apiLevel);
        LoadInputFunctions(bundle, apiLevel);
        LoadDrawingFunctions(bundle, apiLevel);
        LoadShapesFunctions(bundle, apiLevel);
        LoadTexturesFunctions(bundle, apiLevel);
        LoadTextFunctions(bundle, apiLevel);
        LoadAudioFunctions(bundle, apiLevel);
    }
    
    /// <summary>
    /// Load raylib core window management functions
    /// </summary>
    private static void LoadCoreWindowFunctions(SDKBundle bundle, int apiLevel)
    {
        var raylibCoreAPI = new APIDefinition("raylib.Core", 23);
        
        // Window management
        AddRaylibMethod(raylibCoreAPI, "initWindow", "void", 23,
            new ParameterInfo("width", "int"),
            new ParameterInfo("height", "int"),
            new ParameterInfo("title", "String"));
        
        AddRaylibMethod(raylibCoreAPI, "closeWindow", "void", 23);
        
        AddRaylibMethod(raylibCoreAPI, "windowShouldClose", "boolean", 23);
        
        AddRaylibMethod(raylibCoreAPI, "isWindowReady", "boolean", 23);
        
        AddRaylibMethod(raylibCoreAPI, "isWindowFullscreen", "boolean", 23);
        
        AddRaylibMethod(raylibCoreAPI, "isWindowHidden", "boolean", 23);
        
        AddRaylibMethod(raylibCoreAPI, "isWindowMinimized", "boolean", 23);
        
        AddRaylibMethod(raylibCoreAPI, "isWindowMaximized", "boolean", 23);
        
        AddRaylibMethod(raylibCoreAPI, "isWindowFocused", "boolean", 23);
        
        AddRaylibMethod(raylibCoreAPI, "isWindowResized", "boolean", 23);
        
        AddRaylibMethod(raylibCoreAPI, "toggleFullscreen", "void", 23);
        
        AddRaylibMethod(raylibCoreAPI, "maximizeWindow", "void", 23);
        
        AddRaylibMethod(raylibCoreAPI, "minimizeWindow", "void", 23);
        
        AddRaylibMethod(raylibCoreAPI, "restoreWindow", "void", 23);
        
        AddRaylibMethod(raylibCoreAPI, "setWindowTitle", "void", 23,
            new ParameterInfo("title", "String"));
        
        AddRaylibMethod(raylibCoreAPI, "setWindowPosition", "void", 23,
            new ParameterInfo("x", "int"),
            new ParameterInfo("y", "int"));
        
        AddRaylibMethod(raylibCoreAPI, "setWindowSize", "void", 23,
            new ParameterInfo("width", "int"),
            new ParameterInfo("height", "int"));
        
        AddRaylibMethod(raylibCoreAPI, "getScreenWidth", "int", 23);
        
        AddRaylibMethod(raylibCoreAPI, "getScreenHeight", "int", 23);
        
        AddRaylibMethod(raylibCoreAPI, "setTargetFPS", "void", 23,
            new ParameterInfo("fps", "int"));
        
        bundle.APIs.Add(raylibCoreAPI);
    }
    
    /// <summary>
    /// Load raylib timing functions
    /// </summary>
    private static void LoadTimingFunctions(SDKBundle bundle, int apiLevel)
    {
        var raylibTimingAPI = new APIDefinition("raylib.Timing", 23);
        
        AddRaylibMethod(raylibTimingAPI, "getFrameTime", "float", 23);
        
        AddRaylibMethod(raylibTimingAPI, "getTime", "double", 23);
        
        AddRaylibMethod(raylibTimingAPI, "getFPS", "int", 23);
        
        bundle.APIs.Add(raylibTimingAPI);
    }
    
    /// <summary>
    /// Load raylib input functions (keyboard, mouse)
    /// </summary>
    private static void LoadInputFunctions(SDKBundle bundle, int apiLevel)
    {
        var raylibInputAPI = new APIDefinition("raylib.Input", 23);
        
        // Keyboard input
        AddRaylibMethod(raylibInputAPI, "isKeyPressed", "boolean", 23,
            new ParameterInfo("key", "int"));
        
        AddRaylibMethod(raylibInputAPI, "isKeyDown", "boolean", 23,
            new ParameterInfo("key", "int"));
        
        AddRaylibMethod(raylibInputAPI, "isKeyReleased", "boolean", 23,
            new ParameterInfo("key", "int"));
        
        AddRaylibMethod(raylibInputAPI, "isKeyUp", "boolean", 23,
            new ParameterInfo("key", "int"));
        
        AddRaylibMethod(raylibInputAPI, "getKeyPressed", "int", 23);
        
        AddRaylibMethod(raylibInputAPI, "getCharPressed", "int", 23);
        
        // Mouse input
        AddRaylibMethod(raylibInputAPI, "isMouseButtonPressed", "boolean", 23,
            new ParameterInfo("button", "int"));
        
        AddRaylibMethod(raylibInputAPI, "isMouseButtonDown", "boolean", 23,
            new ParameterInfo("button", "int"));
        
        AddRaylibMethod(raylibInputAPI, "isMouseButtonReleased", "boolean", 23,
            new ParameterInfo("button", "int"));
        
        AddRaylibMethod(raylibInputAPI, "isMouseButtonUp", "boolean", 23,
            new ParameterInfo("button", "int"));
        
        AddRaylibMethod(raylibInputAPI, "getMouseX", "int", 23);
        
        AddRaylibMethod(raylibInputAPI, "getMouseY", "int", 23);
        
        AddRaylibMethod(raylibInputAPI, "getMousePosition", "raylib.Vector2", 23);
        
        AddRaylibMethod(raylibInputAPI, "getMouseDelta", "raylib.Vector2", 23);
        
        AddRaylibMethod(raylibInputAPI, "setMousePosition", "void", 23,
            new ParameterInfo("x", "int"),
            new ParameterInfo("y", "int"));
        
        AddRaylibMethod(raylibInputAPI, "setMouseOffset", "void", 23,
            new ParameterInfo("offsetX", "int"),
            new ParameterInfo("offsetY", "int"));
        
        AddRaylibMethod(raylibInputAPI, "setMouseScale", "void", 23,
            new ParameterInfo("scaleX", "float"),
            new ParameterInfo("scaleY", "float"));
        
        AddRaylibMethod(raylibInputAPI, "getMouseWheelMove", "float", 23);
        
        AddRaylibMethod(raylibInputAPI, "getMouseWheelMoveV", "raylib.Vector2", 23);
        
        // Mouse cursor
        AddRaylibMethod(raylibInputAPI, "showCursor", "void", 23);
        
        AddRaylibMethod(raylibInputAPI, "hideCursor", "void", 23);
        
        AddRaylibMethod(raylibInputAPI, "isCursorHidden", "boolean", 23);
        
        AddRaylibMethod(raylibInputAPI, "enableCursor", "void", 23);
        
        AddRaylibMethod(raylibInputAPI, "disableCursor", "void", 23);
        
        AddRaylibMethod(raylibInputAPI, "isCursorOnScreen", "boolean", 23);
        
        // Key constants
        AddRaylibField(raylibInputAPI, "KEY_NULL", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_SPACE", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_ENTER", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_TAB", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_BACKSPACE", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_ESCAPE", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_RIGHT", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_LEFT", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_DOWN", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_UP", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_A", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_B", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_C", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_D", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_E", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_F", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_W", "int", 23);
        AddRaylibField(raylibInputAPI, "KEY_S", "int", 23);
        
        // Mouse button constants
        AddRaylibField(raylibInputAPI, "MOUSE_BUTTON_LEFT", "int", 23);
        AddRaylibField(raylibInputAPI, "MOUSE_BUTTON_RIGHT", "int", 23);
        AddRaylibField(raylibInputAPI, "MOUSE_BUTTON_MIDDLE", "int", 23);
        AddRaylibField(raylibInputAPI, "MOUSE_BUTTON_SIDE", "int", 23);
        AddRaylibField(raylibInputAPI, "MOUSE_BUTTON_EXTRA", "int", 23);
        
        bundle.APIs.Add(raylibInputAPI);
    }
    
    /// <summary>
    /// Helper method to add a raylib method to an API definition
    /// </summary>
    private static void AddRaylibMethod(APIDefinition api, string name, string returnType, int minSDK, params ParameterInfo[] parameters)
    {
        var method = new MethodSignature(name, returnType, minSDK);
        method.Parameters.AddRange(parameters);
        method.IsStatic = true; // All raylib functions are static/global
        api.Methods.Add(method);
    }
    
    /// <summary>
    /// Helper method to add a raylib field/constant to an API definition
    /// </summary>
    private static void AddRaylibField(APIDefinition api, string name, string type, int minSDK)
    {
        var field = new FieldSignature(name, type, minSDK);
        field.IsStatic = true;
        api.Fields.Add(field);
    }
    
    /// <summary>
    /// Load raylib drawing lifecycle functions
    /// </summary>
    private static void LoadDrawingFunctions(SDKBundle bundle, int apiLevel)
    {
        var raylibDrawingAPI = new APIDefinition("raylib.Drawing", 23);
        
        // Drawing lifecycle
        AddRaylibMethod(raylibDrawingAPI, "beginDrawing", "void", 23);
        
        AddRaylibMethod(raylibDrawingAPI, "endDrawing", "void", 23);
        
        AddRaylibMethod(raylibDrawingAPI, "clearBackground", "void", 23,
            new ParameterInfo("color", "raylib.Color"));
        
        // Color constants
        AddRaylibField(raylibDrawingAPI, "RAYWHITE", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "WHITE", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "BLACK", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "RED", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "GREEN", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "BLUE", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "YELLOW", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "ORANGE", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "PURPLE", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "DARKGRAY", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "GRAY", "raylib.Color", 23);
        AddRaylibField(raylibDrawingAPI, "LIGHTGRAY", "raylib.Color", 23);
        
        bundle.APIs.Add(raylibDrawingAPI);
    }
    
    /// <summary>
    /// Load raylib shape drawing functions
    /// </summary>
    private static void LoadShapesFunctions(SDKBundle bundle, int apiLevel)
    {
        var raylibShapesAPI = new APIDefinition("raylib.Shapes", 23);
        
        // Pixel drawing
        AddRaylibMethod(raylibShapesAPI, "drawPixel", "void", 23,
            new ParameterInfo("posX", "int"),
            new ParameterInfo("posY", "int"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawPixelV", "void", 23,
            new ParameterInfo("position", "raylib.Vector2"),
            new ParameterInfo("color", "raylib.Color"));
        
        // Line drawing
        AddRaylibMethod(raylibShapesAPI, "drawLine", "void", 23,
            new ParameterInfo("startPosX", "int"),
            new ParameterInfo("startPosY", "int"),
            new ParameterInfo("endPosX", "int"),
            new ParameterInfo("endPosY", "int"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawLineV", "void", 23,
            new ParameterInfo("startPos", "raylib.Vector2"),
            new ParameterInfo("endPos", "raylib.Vector2"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawLineEx", "void", 23,
            new ParameterInfo("startPos", "raylib.Vector2"),
            new ParameterInfo("endPos", "raylib.Vector2"),
            new ParameterInfo("thick", "float"),
            new ParameterInfo("color", "raylib.Color"));
        
        // Circle drawing
        AddRaylibMethod(raylibShapesAPI, "drawCircle", "void", 23,
            new ParameterInfo("centerX", "int"),
            new ParameterInfo("centerY", "int"),
            new ParameterInfo("radius", "float"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawCircleV", "void", 23,
            new ParameterInfo("center", "raylib.Vector2"),
            new ParameterInfo("radius", "float"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawCircleLines", "void", 23,
            new ParameterInfo("centerX", "int"),
            new ParameterInfo("centerY", "int"),
            new ParameterInfo("radius", "float"),
            new ParameterInfo("color", "raylib.Color"));
        
        // Rectangle drawing
        AddRaylibMethod(raylibShapesAPI, "drawRectangle", "void", 23,
            new ParameterInfo("posX", "int"),
            new ParameterInfo("posY", "int"),
            new ParameterInfo("width", "int"),
            new ParameterInfo("height", "int"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawRectangleV", "void", 23,
            new ParameterInfo("position", "raylib.Vector2"),
            new ParameterInfo("size", "raylib.Vector2"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawRectangleRec", "void", 23,
            new ParameterInfo("rec", "raylib.Rectangle"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawRectangleLines", "void", 23,
            new ParameterInfo("posX", "int"),
            new ParameterInfo("posY", "int"),
            new ParameterInfo("width", "int"),
            new ParameterInfo("height", "int"),
            new ParameterInfo("color", "raylib.Color"));
        
        // Triangle drawing
        AddRaylibMethod(raylibShapesAPI, "drawTriangle", "void", 23,
            new ParameterInfo("v1", "raylib.Vector2"),
            new ParameterInfo("v2", "raylib.Vector2"),
            new ParameterInfo("v3", "raylib.Vector2"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibShapesAPI, "drawTriangleLines", "void", 23,
            new ParameterInfo("v1", "raylib.Vector2"),
            new ParameterInfo("v2", "raylib.Vector2"),
            new ParameterInfo("v3", "raylib.Vector2"),
            new ParameterInfo("color", "raylib.Color"));
        
        bundle.APIs.Add(raylibShapesAPI);
    }
    
    /// <summary>
    /// Load raylib texture functions
    /// </summary>
    private static void LoadTexturesFunctions(SDKBundle bundle, int apiLevel)
    {
        var raylibTexturesAPI = new APIDefinition("raylib.Textures", 23);
        
        // Texture loading
        AddRaylibMethod(raylibTexturesAPI, "loadTexture", "raylib.Texture2D", 23,
            new ParameterInfo("fileName", "String"));
        
        AddRaylibMethod(raylibTexturesAPI, "loadTextureFromImage", "raylib.Texture2D", 23,
            new ParameterInfo("image", "raylib.Image"));
        
        AddRaylibMethod(raylibTexturesAPI, "unloadTexture", "void", 23,
            new ParameterInfo("texture", "raylib.Texture2D"));
        
        // Texture drawing
        AddRaylibMethod(raylibTexturesAPI, "drawTexture", "void", 23,
            new ParameterInfo("texture", "raylib.Texture2D"),
            new ParameterInfo("posX", "int"),
            new ParameterInfo("posY", "int"),
            new ParameterInfo("tint", "raylib.Color"));
        
        AddRaylibMethod(raylibTexturesAPI, "drawTextureV", "void", 23,
            new ParameterInfo("texture", "raylib.Texture2D"),
            new ParameterInfo("position", "raylib.Vector2"),
            new ParameterInfo("tint", "raylib.Color"));
        
        AddRaylibMethod(raylibTexturesAPI, "drawTextureEx", "void", 23,
            new ParameterInfo("texture", "raylib.Texture2D"),
            new ParameterInfo("position", "raylib.Vector2"),
            new ParameterInfo("rotation", "float"),
            new ParameterInfo("scale", "float"),
            new ParameterInfo("tint", "raylib.Color"));
        
        AddRaylibMethod(raylibTexturesAPI, "drawTextureRec", "void", 23,
            new ParameterInfo("texture", "raylib.Texture2D"),
            new ParameterInfo("source", "raylib.Rectangle"),
            new ParameterInfo("position", "raylib.Vector2"),
            new ParameterInfo("tint", "raylib.Color"));
        
        AddRaylibMethod(raylibTexturesAPI, "drawTexturePro", "void", 23,
            new ParameterInfo("texture", "raylib.Texture2D"),
            new ParameterInfo("source", "raylib.Rectangle"),
            new ParameterInfo("dest", "raylib.Rectangle"),
            new ParameterInfo("origin", "raylib.Vector2"),
            new ParameterInfo("rotation", "float"),
            new ParameterInfo("tint", "raylib.Color"));
        
        bundle.APIs.Add(raylibTexturesAPI);
    }
    
    /// <summary>
    /// Load raylib text drawing functions
    /// </summary>
    private static void LoadTextFunctions(SDKBundle bundle, int apiLevel)
    {
        var raylibTextAPI = new APIDefinition("raylib.Text", 23);
        
        // Font loading
        AddRaylibMethod(raylibTextAPI, "getFontDefault", "raylib.Font", 23);
        
        AddRaylibMethod(raylibTextAPI, "loadFont", "raylib.Font", 23,
            new ParameterInfo("fileName", "String"));
        
        AddRaylibMethod(raylibTextAPI, "unloadFont", "void", 23,
            new ParameterInfo("font", "raylib.Font"));
        
        // Text drawing
        AddRaylibMethod(raylibTextAPI, "drawText", "void", 23,
            new ParameterInfo("text", "String"),
            new ParameterInfo("posX", "int"),
            new ParameterInfo("posY", "int"),
            new ParameterInfo("fontSize", "int"),
            new ParameterInfo("color", "raylib.Color"));
        
        AddRaylibMethod(raylibTextAPI, "drawTextEx", "void", 23,
            new ParameterInfo("font", "raylib.Font"),
            new ParameterInfo("text", "String"),
            new ParameterInfo("position", "raylib.Vector2"),
            new ParameterInfo("fontSize", "float"),
            new ParameterInfo("spacing", "float"),
            new ParameterInfo("tint", "raylib.Color"));
        
        AddRaylibMethod(raylibTextAPI, "drawTextPro", "void", 23,
            new ParameterInfo("font", "raylib.Font"),
            new ParameterInfo("text", "String"),
            new ParameterInfo("position", "raylib.Vector2"),
            new ParameterInfo("origin", "raylib.Vector2"),
            new ParameterInfo("rotation", "float"),
            new ParameterInfo("fontSize", "float"),
            new ParameterInfo("spacing", "float"),
            new ParameterInfo("tint", "raylib.Color"));
        
        // Text measurement
        AddRaylibMethod(raylibTextAPI, "measureText", "int", 23,
            new ParameterInfo("text", "String"),
            new ParameterInfo("fontSize", "int"));
        
        AddRaylibMethod(raylibTextAPI, "measureTextEx", "raylib.Vector2", 23,
            new ParameterInfo("font", "raylib.Font"),
            new ParameterInfo("text", "String"),
            new ParameterInfo("fontSize", "float"),
            new ParameterInfo("spacing", "float"));
        
        bundle.APIs.Add(raylibTextAPI);
    }
    
    /// <summary>
    /// Load raylib audio functions
    /// </summary>
    private static void LoadAudioFunctions(SDKBundle bundle, int apiLevel)
    {
        var raylibAudioAPI = new APIDefinition("raylib.Audio", 23);
        
        // Audio device management
        AddRaylibMethod(raylibAudioAPI, "initAudioDevice", "void", 23);
        
        AddRaylibMethod(raylibAudioAPI, "closeAudioDevice", "void", 23);
        
        AddRaylibMethod(raylibAudioAPI, "isAudioDeviceReady", "boolean", 23);
        
        // Sound loading and management
        AddRaylibMethod(raylibAudioAPI, "loadSound", "raylib.Sound", 23,
            new ParameterInfo("fileName", "String"));
        
        AddRaylibMethod(raylibAudioAPI, "unloadSound", "void", 23,
            new ParameterInfo("sound", "raylib.Sound"));
        
        // Sound playback
        AddRaylibMethod(raylibAudioAPI, "playSound", "void", 23,
            new ParameterInfo("sound", "raylib.Sound"));
        
        AddRaylibMethod(raylibAudioAPI, "stopSound", "void", 23,
            new ParameterInfo("sound", "raylib.Sound"));
        
        AddRaylibMethod(raylibAudioAPI, "pauseSound", "void", 23,
            new ParameterInfo("sound", "raylib.Sound"));
        
        AddRaylibMethod(raylibAudioAPI, "resumeSound", "void", 23,
            new ParameterInfo("sound", "raylib.Sound"));
        
        AddRaylibMethod(raylibAudioAPI, "isSoundPlaying", "boolean", 23,
            new ParameterInfo("sound", "raylib.Sound"));
        
        // Sound properties
        AddRaylibMethod(raylibAudioAPI, "setSoundVolume", "void", 23,
            new ParameterInfo("sound", "raylib.Sound"),
            new ParameterInfo("volume", "float"));
        
        AddRaylibMethod(raylibAudioAPI, "setSoundPitch", "void", 23,
            new ParameterInfo("sound", "raylib.Sound"),
            new ParameterInfo("pitch", "float"));
        
        AddRaylibMethod(raylibAudioAPI, "setSoundPan", "void", 23,
            new ParameterInfo("sound", "raylib.Sound"),
            new ParameterInfo("pan", "float"));
        
        // Music streaming
        AddRaylibMethod(raylibAudioAPI, "loadMusicStream", "raylib.Music", 23,
            new ParameterInfo("fileName", "String"));
        
        AddRaylibMethod(raylibAudioAPI, "unloadMusicStream", "void", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        // Music playback
        AddRaylibMethod(raylibAudioAPI, "playMusicStream", "void", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        AddRaylibMethod(raylibAudioAPI, "stopMusicStream", "void", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        AddRaylibMethod(raylibAudioAPI, "pauseMusicStream", "void", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        AddRaylibMethod(raylibAudioAPI, "resumeMusicStream", "void", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        AddRaylibMethod(raylibAudioAPI, "updateMusicStream", "void", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        AddRaylibMethod(raylibAudioAPI, "isMusicStreamPlaying", "boolean", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        // Music properties
        AddRaylibMethod(raylibAudioAPI, "setMusicVolume", "void", 23,
            new ParameterInfo("music", "raylib.Music"),
            new ParameterInfo("volume", "float"));
        
        AddRaylibMethod(raylibAudioAPI, "setMusicPitch", "void", 23,
            new ParameterInfo("music", "raylib.Music"),
            new ParameterInfo("pitch", "float"));
        
        AddRaylibMethod(raylibAudioAPI, "setMusicPan", "void", 23,
            new ParameterInfo("music", "raylib.Music"),
            new ParameterInfo("pan", "float"));
        
        AddRaylibMethod(raylibAudioAPI, "getMusicTimeLength", "float", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        AddRaylibMethod(raylibAudioAPI, "getMusicTimePlayed", "float", 23,
            new ParameterInfo("music", "raylib.Music"));
        
        AddRaylibMethod(raylibAudioAPI, "seekMusicStream", "void", 23,
            new ParameterInfo("music", "raylib.Music"),
            new ParameterInfo("position", "float"));
        
        bundle.APIs.Add(raylibAudioAPI);
    }
}
