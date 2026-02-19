namespace ADLCompiler.SemanticAnalysis;

/// <summary>
/// Loads NDK API bindings for graphics, audio, sensors, input, and camera APIs
/// </summary>
public static class NDKAPILoader
{
    /// <summary>
    /// Load all NDK API bindings into the SDK bundle
    /// </summary>
    public static void LoadNDKAPIs(SDKBundle bundle, int apiLevel)
    {
        LoadOpenGLESBindings(bundle, apiLevel);
        LoadVulkanBindings(bundle, apiLevel);
        LoadAudioBindings(bundle, apiLevel);
        LoadSensorBindings(bundle, apiLevel);
        LoadInputBindings(bundle, apiLevel);
        LoadCameraBindings(bundle, apiLevel);
    }
    
    /// <summary>
    /// Load OpenGL ES 2.0/3.0/3.1/3.2 bindings
    /// </summary>
    private static void LoadOpenGLESBindings(SDKBundle bundle, int apiLevel)
    {
        // OpenGL ES 2.0 (API 23+)
        var gles2API = new APIDefinition("android.opengl.GLES20", 23);
        
        // Core functions
        AddNDKMethod(gles2API, "glClear", "void", 23, 
            new ParameterInfo("mask", "int"));
        AddNDKMethod(gles2API, "glClearColor", "void", 23,
            new ParameterInfo("red", "float"),
            new ParameterInfo("green", "float"),
            new ParameterInfo("blue", "float"),
            new ParameterInfo("alpha", "float"));
        AddNDKMethod(gles2API, "glViewport", "void", 23,
            new ParameterInfo("x", "int"),
            new ParameterInfo("y", "int"),
            new ParameterInfo("width", "int"),
            new ParameterInfo("height", "int"));
        
        // Shader functions
        AddNDKMethod(gles2API, "glCreateShader", "int", 23,
            new ParameterInfo("type", "int"));
        AddNDKMethod(gles2API, "glShaderSource", "void", 23,
            new ParameterInfo("shader", "int"),
            new ParameterInfo("count", "int"),
            new ParameterInfo("string", "String[]"),
            new ParameterInfo("length", "int[]"));
        AddNDKMethod(gles2API, "glCompileShader", "void", 23,
            new ParameterInfo("shader", "int"));
        AddNDKMethod(gles2API, "glCreateProgram", "int", 23);
        AddNDKMethod(gles2API, "glAttachShader", "void", 23,
            new ParameterInfo("program", "int"),
            new ParameterInfo("shader", "int"));
        AddNDKMethod(gles2API, "glLinkProgram", "void", 23,
            new ParameterInfo("program", "int"));
        AddNDKMethod(gles2API, "glUseProgram", "void", 23,
            new ParameterInfo("program", "int"));
        
        // Buffer functions
        AddNDKMethod(gles2API, "glGenBuffers", "void", 23,
            new ParameterInfo("n", "int"),
            new ParameterInfo("buffers", "int[]"),
            new ParameterInfo("offset", "int"));
        AddNDKMethod(gles2API, "glBindBuffer", "void", 23,
            new ParameterInfo("target", "int"),
            new ParameterInfo("buffer", "int"));
        AddNDKMethod(gles2API, "glBufferData", "void", 23,
            new ParameterInfo("target", "int"),
            new ParameterInfo("size", "int"),
            new ParameterInfo("data", "java.nio.Buffer"),
            new ParameterInfo("usage", "int"));
        
        // Drawing functions
        AddNDKMethod(gles2API, "glDrawArrays", "void", 23,
            new ParameterInfo("mode", "int"),
            new ParameterInfo("first", "int"),
            new ParameterInfo("count", "int"));
        AddNDKMethod(gles2API, "glDrawElements", "void", 23,
            new ParameterInfo("mode", "int"),
            new ParameterInfo("count", "int"),
            new ParameterInfo("type", "int"),
            new ParameterInfo("indices", "java.nio.Buffer"));
        
        // Texture functions
        AddNDKMethod(gles2API, "glGenTextures", "void", 23,
            new ParameterInfo("n", "int"),
            new ParameterInfo("textures", "int[]"),
            new ParameterInfo("offset", "int"));
        AddNDKMethod(gles2API, "glBindTexture", "void", 23,
            new ParameterInfo("target", "int"),
            new ParameterInfo("texture", "int"));
        AddNDKMethod(gles2API, "glTexImage2D", "void", 23,
            new ParameterInfo("target", "int"),
            new ParameterInfo("level", "int"),
            new ParameterInfo("internalformat", "int"),
            new ParameterInfo("width", "int"),
            new ParameterInfo("height", "int"),
            new ParameterInfo("border", "int"),
            new ParameterInfo("format", "int"),
            new ParameterInfo("type", "int"),
            new ParameterInfo("pixels", "java.nio.Buffer"));
        AddNDKMethod(gles2API, "glTexParameteri", "void", 23,
            new ParameterInfo("target", "int"),
            new ParameterInfo("pname", "int"),
            new ParameterInfo("param", "int"));
        
        // Constants
        AddNDKField(gles2API, "GL_COLOR_BUFFER_BIT", "int", 23);
        AddNDKField(gles2API, "GL_DEPTH_BUFFER_BIT", "int", 23);
        AddNDKField(gles2API, "GL_TRIANGLES", "int", 23);
        AddNDKField(gles2API, "GL_TRIANGLE_STRIP", "int", 23);
        AddNDKField(gles2API, "GL_VERTEX_SHADER", "int", 23);
        AddNDKField(gles2API, "GL_FRAGMENT_SHADER", "int", 23);
        AddNDKField(gles2API, "GL_ARRAY_BUFFER", "int", 23);
        AddNDKField(gles2API, "GL_ELEMENT_ARRAY_BUFFER", "int", 23);
        AddNDKField(gles2API, "GL_STATIC_DRAW", "int", 23);
        AddNDKField(gles2API, "GL_TEXTURE_2D", "int", 23);
        
        bundle.APIs.Add(gles2API);
        
        // OpenGL ES 3.0 (API 18+, but we start at 23)
        if (apiLevel >= 23)
        {
            var gles3API = new APIDefinition("android.opengl.GLES30", 23);
            
            AddNDKMethod(gles3API, "glGenVertexArrays", "void", 23,
                new ParameterInfo("n", "int"),
                new ParameterInfo("arrays", "int[]"),
                new ParameterInfo("offset", "int"));
            AddNDKMethod(gles3API, "glBindVertexArray", "void", 23,
                new ParameterInfo("array", "int"));
            AddNDKMethod(gles3API, "glDrawArraysInstanced", "void", 23,
                new ParameterInfo("mode", "int"),
                new ParameterInfo("first", "int"),
                new ParameterInfo("count", "int"),
                new ParameterInfo("instancecount", "int"));
            
            bundle.APIs.Add(gles3API);
        }
        
        // OpenGL ES 3.1 (API 21+, but we start at 23)
        if (apiLevel >= 23)
        {
            var gles31API = new APIDefinition("android.opengl.GLES31", 23);
            
            AddNDKMethod(gles31API, "glDispatchCompute", "void", 23,
                new ParameterInfo("num_groups_x", "int"),
                new ParameterInfo("num_groups_y", "int"),
                new ParameterInfo("num_groups_z", "int"));
            AddNDKMethod(gles31API, "glMemoryBarrier", "void", 23,
                new ParameterInfo("barriers", "int"));
            
            bundle.APIs.Add(gles31API);
        }
        
        // OpenGL ES 3.2 (API 24+)
        if (apiLevel >= 24)
        {
            var gles32API = new APIDefinition("android.opengl.GLES32", 24);
            
            AddNDKMethod(gles32API, "glBlendBarrier", "void", 24);
            AddNDKMethod(gles32API, "glPrimitiveBoundingBox", "void", 24,
                new ParameterInfo("minX", "float"),
                new ParameterInfo("minY", "float"),
                new ParameterInfo("minZ", "float"),
                new ParameterInfo("minW", "float"),
                new ParameterInfo("maxX", "float"),
                new ParameterInfo("maxY", "float"),
                new ParameterInfo("maxZ", "float"),
                new ParameterInfo("maxW", "float"));
            
            bundle.APIs.Add(gles32API);
        }
        
        // EGL (OpenGL ES context management)
        var eglAPI = new APIDefinition("android.opengl.EGL14", 23);
        
        AddNDKMethod(eglAPI, "eglGetDisplay", "android.opengl.EGLDisplay", 23,
            new ParameterInfo("display_id", "int"));
        AddNDKMethod(eglAPI, "eglInitialize", "boolean", 23,
            new ParameterInfo("dpy", "android.opengl.EGLDisplay"),
            new ParameterInfo("major", "int[]"),
            new ParameterInfo("majorOffset", "int"),
            new ParameterInfo("minor", "int[]"),
            new ParameterInfo("minorOffset", "int"));
        AddNDKMethod(eglAPI, "eglCreateContext", "android.opengl.EGLContext", 23,
            new ParameterInfo("dpy", "android.opengl.EGLDisplay"),
            new ParameterInfo("config", "android.opengl.EGLConfig"),
            new ParameterInfo("share_context", "android.opengl.EGLContext"),
            new ParameterInfo("attrib_list", "int[]"),
            new ParameterInfo("offset", "int"));
        AddNDKMethod(eglAPI, "eglMakeCurrent", "boolean", 23,
            new ParameterInfo("dpy", "android.opengl.EGLDisplay"),
            new ParameterInfo("draw", "android.opengl.EGLSurface"),
            new ParameterInfo("read", "android.opengl.EGLSurface"),
            new ParameterInfo("ctx", "android.opengl.EGLContext"));
        AddNDKMethod(eglAPI, "eglSwapBuffers", "boolean", 23,
            new ParameterInfo("dpy", "android.opengl.EGLDisplay"),
            new ParameterInfo("surface", "android.opengl.EGLSurface"));
        
        AddNDKField(eglAPI, "EGL_DEFAULT_DISPLAY", "int", 23);
        AddNDKField(eglAPI, "EGL_NO_CONTEXT", "android.opengl.EGLContext", 23);
        AddNDKField(eglAPI, "EGL_NO_SURFACE", "android.opengl.EGLSurface", 23);
        
        bundle.APIs.Add(eglAPI);
    }
    
    /// <summary>
    /// Load Vulkan bindings (API 24+)
    /// </summary>
    private static void LoadVulkanBindings(SDKBundle bundle, int apiLevel)
    {
        if (apiLevel < 24) return;
        
        // Vulkan support starts at API 24 (Android 7.0)
        var vulkanAPI = new APIDefinition("android.view.SurfaceHolder", 24);
        
        // Note: Vulkan is primarily used through native code, but we provide surface support
        AddNDKMethod(vulkanAPI, "getSurface", "android.view.Surface", 24);
        
        bundle.APIs.Add(vulkanAPI);
        
        // Add Vulkan constants
        var vulkanConstantsAPI = new APIDefinition("android.hardware.HardwareBuffer", 26);
        if (apiLevel >= 26)
        {
            AddNDKField(vulkanConstantsAPI, "USAGE_GPU_SAMPLED_IMAGE", "long", 26);
            AddNDKField(vulkanConstantsAPI, "USAGE_GPU_COLOR_OUTPUT", "long", 26);
            bundle.APIs.Add(vulkanConstantsAPI);
        }
    }
    
    /// <summary>
    /// Load OpenSL ES and AAudio bindings
    /// </summary>
    private static void LoadAudioBindings(SDKBundle bundle, int apiLevel)
    {
        // OpenSL ES (available from API 9, but we start at 23)
        var openSLAPI = new APIDefinition("android.media.AudioTrack", 23);
        
        AddNDKMethod(openSLAPI, "play", "void", 23);
        AddNDKMethod(openSLAPI, "pause", "void", 23);
        AddNDKMethod(openSLAPI, "stop", "void", 23);
        AddNDKMethod(openSLAPI, "write", "int", 23,
            new ParameterInfo("audioData", "byte[]"),
            new ParameterInfo("offsetInBytes", "int"),
            new ParameterInfo("sizeInBytes", "int"));
        AddNDKMethod(openSLAPI, "setStereoVolume", "int", 23,
            new ParameterInfo("leftVolume", "float"),
            new ParameterInfo("rightVolume", "float"));
        AddNDKMethod(openSLAPI, "getPlaybackRate", "int", 23);
        AddNDKMethod(openSLAPI, "setPlaybackRate", "int", 23,
            new ParameterInfo("sampleRateInHz", "int"));
        
        AddNDKField(openSLAPI, "MODE_STREAM", "int", 23);
        AddNDKField(openSLAPI, "MODE_STATIC", "int", 23);
        AddNDKField(openSLAPI, "STATE_INITIALIZED", "int", 23);
        AddNDKField(openSLAPI, "STATE_UNINITIALIZED", "int", 23);
        
        bundle.APIs.Add(openSLAPI);
        
        var audioRecordAPI = new APIDefinition("android.media.AudioRecord", 23);
        
        AddNDKMethod(audioRecordAPI, "startRecording", "void", 23);
        AddNDKMethod(audioRecordAPI, "stop", "void", 23);
        AddNDKMethod(audioRecordAPI, "read", "int", 23,
            new ParameterInfo("audioData", "byte[]"),
            new ParameterInfo("offsetInBytes", "int"),
            new ParameterInfo("sizeInBytes", "int"));
        AddNDKMethod(audioRecordAPI, "getRecordingState", "int", 23);
        
        AddNDKField(audioRecordAPI, "RECORDSTATE_RECORDING", "int", 23);
        AddNDKField(audioRecordAPI, "RECORDSTATE_STOPPED", "int", 23);
        
        bundle.APIs.Add(audioRecordAPI);
        
        // AAudio (API 26+)
        if (apiLevel >= 26)
        {
            // Note: AAudio methods are added to AudioTrack in API 26+
            // We need to check if AudioTrack API already exists and add to it
            var existingAudioTrackAPI = bundle.APIs.FirstOrDefault(api => api.ClassName == "android.media.AudioTrack");
            if (existingAudioTrackAPI != null)
            {
                AddNDKMethod(existingAudioTrackAPI, "setPerformanceMode", "void", 26,
                    new ParameterInfo("performanceMode", "int"));
                AddNDKField(existingAudioTrackAPI, "PERFORMANCE_MODE_LOW_LATENCY", "int", 26);
                AddNDKField(existingAudioTrackAPI, "PERFORMANCE_MODE_POWER_SAVING", "int", 26);
            }
        }
    }
    
    /// <summary>
    /// Load sensor APIs (accelerometer, gyroscope, magnetometer)
    /// </summary>
    private static void LoadSensorBindings(SDKBundle bundle, int apiLevel)
    {
        var sensorManagerAPI = new APIDefinition("android.hardware.SensorManager", 23);
        
        AddNDKMethod(sensorManagerAPI, "getDefaultSensor", "android.hardware.Sensor", 23,
            new ParameterInfo("type", "int"));
        AddNDKMethod(sensorManagerAPI, "registerListener", "boolean", 23,
            new ParameterInfo("listener", "android.hardware.SensorEventListener"),
            new ParameterInfo("sensor", "android.hardware.Sensor"),
            new ParameterInfo("samplingPeriodUs", "int"));
        AddNDKMethod(sensorManagerAPI, "unregisterListener", "void", 23,
            new ParameterInfo("listener", "android.hardware.SensorEventListener"));
        AddNDKMethod(sensorManagerAPI, "getSensorList", "java.util.List", 23,
            new ParameterInfo("type", "int"));
        
        AddNDKField(sensorManagerAPI, "SENSOR_DELAY_FASTEST", "int", 23);
        AddNDKField(sensorManagerAPI, "SENSOR_DELAY_GAME", "int", 23);
        AddNDKField(sensorManagerAPI, "SENSOR_DELAY_UI", "int", 23);
        AddNDKField(sensorManagerAPI, "SENSOR_DELAY_NORMAL", "int", 23);
        
        bundle.APIs.Add(sensorManagerAPI);
        
        var sensorAPI = new APIDefinition("android.hardware.Sensor", 23);
        
        AddNDKMethod(sensorAPI, "getName", "String", 23);
        AddNDKMethod(sensorAPI, "getType", "int", 23);
        AddNDKMethod(sensorAPI, "getVendor", "String", 23);
        AddNDKMethod(sensorAPI, "getMaximumRange", "float", 23);
        AddNDKMethod(sensorAPI, "getResolution", "float", 23);
        AddNDKMethod(sensorAPI, "getPower", "float", 23);
        
        AddNDKField(sensorAPI, "TYPE_ACCELEROMETER", "int", 23);
        AddNDKField(sensorAPI, "TYPE_GYROSCOPE", "int", 23);
        AddNDKField(sensorAPI, "TYPE_MAGNETIC_FIELD", "int", 23);
        AddNDKField(sensorAPI, "TYPE_GRAVITY", "int", 23);
        AddNDKField(sensorAPI, "TYPE_LINEAR_ACCELERATION", "int", 23);
        AddNDKField(sensorAPI, "TYPE_ROTATION_VECTOR", "int", 23);
        AddNDKField(sensorAPI, "TYPE_PROXIMITY", "int", 23);
        AddNDKField(sensorAPI, "TYPE_LIGHT", "int", 23);
        AddNDKField(sensorAPI, "TYPE_PRESSURE", "int", 23);
        
        bundle.APIs.Add(sensorAPI);
        
        var sensorEventAPI = new APIDefinition("android.hardware.SensorEvent", 23);
        
        AddNDKField(sensorEventAPI, "values", "float[]", 23);
        AddNDKField(sensorEventAPI, "timestamp", "long", 23);
        AddNDKField(sensorEventAPI, "accuracy", "int", 23);
        AddNDKField(sensorEventAPI, "sensor", "android.hardware.Sensor", 23);
        
        bundle.APIs.Add(sensorEventAPI);
    }
    
    /// <summary>
    /// Load input APIs (touch, keyboard, gamepad)
    /// </summary>
    private static void LoadInputBindings(SDKBundle bundle, int apiLevel)
    {
        // Touch input
        var motionEventAPI = new APIDefinition("android.view.MotionEvent", 23);
        
        AddNDKMethod(motionEventAPI, "getAction", "int", 23);
        AddNDKMethod(motionEventAPI, "getX", "float", 23);
        AddNDKMethod(motionEventAPI, "getY", "float", 23);
        AddNDKMethod(motionEventAPI, "getX", "float", 23,
            new ParameterInfo("pointerIndex", "int"));
        AddNDKMethod(motionEventAPI, "getY", "float", 23,
            new ParameterInfo("pointerIndex", "int"));
        AddNDKMethod(motionEventAPI, "getPointerCount", "int", 23);
        AddNDKMethod(motionEventAPI, "getPointerId", "int", 23,
            new ParameterInfo("pointerIndex", "int"));
        AddNDKMethod(motionEventAPI, "getPressure", "float", 23);
        AddNDKMethod(motionEventAPI, "getEventTime", "long", 23);
        
        AddNDKField(motionEventAPI, "ACTION_DOWN", "int", 23);
        AddNDKField(motionEventAPI, "ACTION_UP", "int", 23);
        AddNDKField(motionEventAPI, "ACTION_MOVE", "int", 23);
        AddNDKField(motionEventAPI, "ACTION_CANCEL", "int", 23);
        AddNDKField(motionEventAPI, "ACTION_POINTER_DOWN", "int", 23);
        AddNDKField(motionEventAPI, "ACTION_POINTER_UP", "int", 23);
        
        bundle.APIs.Add(motionEventAPI);
        
        // Keyboard input
        var keyEventAPI = new APIDefinition("android.view.KeyEvent", 23);
        
        AddNDKMethod(keyEventAPI, "getAction", "int", 23);
        AddNDKMethod(keyEventAPI, "getKeyCode", "int", 23);
        AddNDKMethod(keyEventAPI, "getUnicodeChar", "int", 23);
        AddNDKMethod(keyEventAPI, "getRepeatCount", "int", 23);
        AddNDKMethod(keyEventAPI, "isShiftPressed", "boolean", 23);
        AddNDKMethod(keyEventAPI, "isCtrlPressed", "boolean", 23);
        AddNDKMethod(keyEventAPI, "isAltPressed", "boolean", 23);
        
        AddNDKField(keyEventAPI, "ACTION_DOWN", "int", 23);
        AddNDKField(keyEventAPI, "ACTION_UP", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_A", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_B", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_SPACE", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_ENTER", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_BACK", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_DPAD_UP", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_DPAD_DOWN", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_DPAD_LEFT", "int", 23);
        AddNDKField(keyEventAPI, "KEYCODE_DPAD_RIGHT", "int", 23);
        
        bundle.APIs.Add(keyEventAPI);
        
        // Gamepad input
        var inputDeviceAPI = new APIDefinition("android.view.InputDevice", 23);
        
        AddNDKMethod(inputDeviceAPI, "getDevice", "android.view.InputDevice", 23,
            new ParameterInfo("id", "int"));
        AddNDKMethod(inputDeviceAPI, "getDeviceIds", "int[]", 23);
        AddNDKMethod(inputDeviceAPI, "getName", "String", 23);
        AddNDKMethod(inputDeviceAPI, "getSources", "int", 23);
        AddNDKMethod(inputDeviceAPI, "getMotionRange", "android.view.InputDevice.MotionRange", 23,
            new ParameterInfo("axis", "int"));
        
        AddNDKField(inputDeviceAPI, "SOURCE_GAMEPAD", "int", 23);
        AddNDKField(inputDeviceAPI, "SOURCE_JOYSTICK", "int", 23);
        AddNDKField(inputDeviceAPI, "SOURCE_KEYBOARD", "int", 23);
        AddNDKField(inputDeviceAPI, "SOURCE_MOUSE", "int", 23);
        AddNDKField(inputDeviceAPI, "SOURCE_TOUCHSCREEN", "int", 23);
        
        bundle.APIs.Add(inputDeviceAPI);
    }
    
    /// <summary>
    /// Load Camera2 API bindings
    /// </summary>
    private static void LoadCameraBindings(SDKBundle bundle, int apiLevel)
    {
        // Camera2 API (API 21+, but we start at 23)
        var cameraManagerAPI = new APIDefinition("android.hardware.camera2.CameraManager", 23);
        
        AddNDKMethod(cameraManagerAPI, "getCameraIdList", "String[]", 23);
        AddNDKMethod(cameraManagerAPI, "getCameraCharacteristics", "android.hardware.camera2.CameraCharacteristics", 23,
            new ParameterInfo("cameraId", "String"));
        AddNDKMethod(cameraManagerAPI, "openCamera", "void", 23,
            new ParameterInfo("cameraId", "String"),
            new ParameterInfo("callback", "android.hardware.camera2.CameraDevice.StateCallback"),
            new ParameterInfo("handler", "android.os.Handler"));
        
        bundle.APIs.Add(cameraManagerAPI);
        
        var cameraDeviceAPI = new APIDefinition("android.hardware.camera2.CameraDevice", 23);
        
        AddNDKMethod(cameraDeviceAPI, "createCaptureSession", "void", 23,
            new ParameterInfo("outputs", "java.util.List"),
            new ParameterInfo("callback", "android.hardware.camera2.CameraCaptureSession.StateCallback"),
            new ParameterInfo("handler", "android.os.Handler"));
        AddNDKMethod(cameraDeviceAPI, "createCaptureRequest", "android.hardware.camera2.CaptureRequest.Builder", 23,
            new ParameterInfo("templateType", "int"));
        AddNDKMethod(cameraDeviceAPI, "close", "void", 23);
        
        AddNDKField(cameraDeviceAPI, "TEMPLATE_PREVIEW", "int", 23);
        AddNDKField(cameraDeviceAPI, "TEMPLATE_STILL_CAPTURE", "int", 23);
        AddNDKField(cameraDeviceAPI, "TEMPLATE_RECORD", "int", 23);
        
        bundle.APIs.Add(cameraDeviceAPI);
        
        var captureRequestAPI = new APIDefinition("android.hardware.camera2.CaptureRequest.Builder", 23);
        
        AddNDKMethod(captureRequestAPI, "addTarget", "void", 23,
            new ParameterInfo("outputTarget", "android.view.Surface"));
        AddNDKMethod(captureRequestAPI, "set", "void", 23,
            new ParameterInfo("key", "android.hardware.camera2.CaptureRequest.Key"),
            new ParameterInfo("value", "Object"));
        AddNDKMethod(captureRequestAPI, "build", "android.hardware.camera2.CaptureRequest", 23);
        
        bundle.APIs.Add(captureRequestAPI);
        
        var captureSessionAPI = new APIDefinition("android.hardware.camera2.CameraCaptureSession", 23);
        
        AddNDKMethod(captureSessionAPI, "setRepeatingRequest", "int", 23,
            new ParameterInfo("request", "android.hardware.camera2.CaptureRequest"),
            new ParameterInfo("listener", "android.hardware.camera2.CameraCaptureSession.CaptureCallback"),
            new ParameterInfo("handler", "android.os.Handler"));
        AddNDKMethod(captureSessionAPI, "capture", "int", 23,
            new ParameterInfo("request", "android.hardware.camera2.CaptureRequest"),
            new ParameterInfo("listener", "android.hardware.camera2.CameraCaptureSession.CaptureCallback"),
            new ParameterInfo("handler", "android.os.Handler"));
        AddNDKMethod(captureSessionAPI, "close", "void", 23);
        
        bundle.APIs.Add(captureSessionAPI);
    }
    
    /// <summary>
    /// Helper method to add an NDK method to an API definition
    /// </summary>
    private static void AddNDKMethod(APIDefinition api, string name, string returnType, int minSDK, params ParameterInfo[] parameters)
    {
        var method = new MethodSignature(name, returnType, minSDK);
        method.Parameters.AddRange(parameters);
        api.Methods.Add(method);
    }
    
    /// <summary>
    /// Helper method to add an NDK field to an API definition
    /// </summary>
    private static void AddNDKField(APIDefinition api, string name, string type, int minSDK)
    {
        var field = new FieldSignature(name, type, minSDK);
        field.IsStatic = true;
        api.Fields.Add(field);
    }
}
